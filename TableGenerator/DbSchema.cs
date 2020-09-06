﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace TableGenerator
{
    public class DbSchema
    {
        public string ClassNamespace { get; }

        public IList<TableInfo> Tables { get; }

        public DbSchema(string classNamespace)
        {
            ClassNamespace = classNamespace;
            Tables = new List<TableInfo>();
        }

        public void ParseTables(SimpleTokenizer tokenizer)
        {
            do
            {
                if (ParseToSequence(tokenizer, new []{"CREATE", "TABLE"}))
                {
                    var table = new TableInfo();

                    table.Parse(tokenizer);

                    Tables.Add(table);
                }

                ParseToEndStmt(tokenizer);

            } while (tokenizer.Token.Type != TokenType.Unknown);
        }

        public void WriteTablesScript(string tablesScriptFilePath)
        {
            using (var writer = new StreamWriter(tablesScriptFilePath))
            {
                writer.WriteLine("// Generated by TableGenerator");
                writer.WriteLine();
                writer.WriteLine("using System;");
                writer.WriteLine("using System.Collections.Generic;");
                writer.WriteLine("using SqlBuilderFramework;");
                writer.WriteLine();
                writer.WriteLine($"namespace {ClassNamespace}");
                writer.WriteLine("{");

                var orderedTables = Tables.OrderBy(table => table.CodeName).ToArray();

                foreach (var table in orderedTables)
                    WriteClass(writer, table);

                writer.WriteLine("    public static class Tables");
                writer.WriteLine("    {");
                foreach (var table in orderedTables)
                {
                    writer.Write("        public static Tbl");
                    writer.Write(table.CodeName);
                    writer.Write(" ");
                    writer.Write(table.CodeName);
                    writer.Write(" = new Tbl");
                    writer.Write(table.CodeName);
                    writer.Write("();");
                    writer.WriteLine();
                }
                writer.WriteLine("    }");
                writer.WriteLine("}");
            }
        }

        private void WriteClass(StreamWriter writer, TableInfo table)
        {
            writer.Write("    public class Tbl");
            writer.Write(table.CodeName);
            writer.WriteLine(" : DbTable");
            writer.WriteLine("    {");
            writer.Write("        public override string Name => \"");
            writer.Write(table.TableName);
            writer.WriteLine("\";");
            writer.WriteLine();
            writer.WriteLine("        public override string Sql(List<ParameterEntity> parameterList, ISqlBuilder query, IDatabase database)");
            writer.WriteLine("        {");
            writer.WriteLine("            return Name;");
            writer.WriteLine("        }");
            writer.WriteLine();
            writer.WriteLine("        public override DbTable Find(ISqlBuilder query)");
            writer.WriteLine("        {");
            writer.WriteLine("            return null;");
            writer.WriteLine("        }");
            writer.WriteLine();

            var ordredColumns = table.Columns.OrderBy(column => column.Name).ToArray();

            foreach (var column in ordredColumns)
            {
                Debug.Assert(!string.IsNullOrEmpty(column.CodeType));

                writer.Write("        public DbTypedColumn<");
                writer.Write(column.CodeType);
                if (column.IsNullable && !(column.CodeType == "string" || column.CodeType == "byte[]"))
                    writer.Write("?");
                writer.Write("> Col");
                writer.Write(column.CodeName);
                writer.Write(" => new DbTypedColumn<");
                writer.Write(column.CodeType);
                if (column.IsNullable && !(column.CodeType == "string" || column.CodeType == "byte[]"))
                    writer.Write("?");
                writer.Write(">(new DbTableColumn(this, \"");
                writer.Write(column.Name);
                writer.Write("\", ");
                writer.Write(column.Length);
                writer.Write(", ");
                writer.Write(column.Scale);
                writer.WriteLine("));");
            }

            writer.WriteLine("    }");
            writer.WriteLine();
        }

        private static void ParseToEndStmt(SimpleTokenizer tokenizer)
        {
            var level = 0;
            while (level >= 0 && (level > 0 || !tokenizer.Token.IsSeparator(")")) && tokenizer.Token.Type != TokenType.Unknown)
            {
                tokenizer.Next();
                if (tokenizer.Token.IsSeparator("("))
                    level++;
                else if (level > 0 && tokenizer.Token.IsSeparator(")"))
                {
                    level--;
                    tokenizer.Next();
                }
            }
        }

        private static bool ParseToSequence(SimpleTokenizer tokenizer, IEnumerable<string> sequence)
        {
            IEnumerable<string> enumerable = sequence as string[] ?? sequence.ToArray();
            var enumerator = enumerable.GetEnumerator();

            enumerator.MoveNext();
            while (tokenizer.Next())
            {
                if (!tokenizer.Token.IsIdentifier(enumerator.Current, StringComparison.OrdinalIgnoreCase))
                    enumerator = enumerable.GetEnumerator();
                if (!enumerator.MoveNext())
                    return true;
            }
            return false;
        }
    }

    public class TableInfo
    {
        public string TableName { get; private set; }
        public string CodeName { get; private set; } // Name in the C# code
        public IList<TableColumn> Columns { get; private set; }

        public TableInfo()
        {
            Columns = new List<TableColumn>();
        }

        public void Parse(SimpleTokenizer tokenizer)
        {
            // Get the table name without schema

            while (true)
            {
                if (!tokenizer.Next() || tokenizer.Token.Type != TokenType.Identifier)
                    return;

                TableName = tokenizer.Token.Value as string;

                if (!tokenizer.Next())
                    return;

                if (!tokenizer.Token.IsSeparator("."))
                    break;

                // This was the schema, now read the name
            }

            // The table name should always be followed by an open bracket
            if (!tokenizer.Token.IsSeparator("("))
                return;

            if (TableName == null)
                return;

            CodeName = TableName;
            TableName = TableName.ToUpper();

            // Read the columns

            do
            {
                var column = new TableColumn();

                if (!column.Parse(tokenizer))
                    break;

                Columns.Add(column);
            } while (tokenizer.Token.IsSeparator(","));
        }

        public void ParseFromSelect(SimpleTokenizer tokenizer, string tableName)
        {
            CodeName = TableName;
            TableName = tableName.ToUpper();

            do
            {
                var column = new TableColumn();

                if (!column.ParseFromSelect(tokenizer))
                    break;

                Columns.Add(column);
            } while (tokenizer.Token.IsSeparator(","));
        }

        public void SetNames(IList<string> aliasList)
        {
            for (int i = 0; i < aliasList.Count; ++i)
            {
                var alias = aliasList[i];
                if (alias[0] == '~')
                {
                    Columns[i].IsNullable = true;
                    alias = alias.Substring(1);
                }
                Columns[i].Name = alias;
            }
        }
    }

    public class TableColumn
    {
        public string Name { get; set; }
        public string CodeName { get; private set; }
        public string InternalName { get; private set; }
        public string Type { get; private set; }
        public string CodeType { get; private set; }
        public int Length { get; private set; }
        public int Scale { get; private set; }
        public bool IsNullable { get; set; }

        public bool Parse(SimpleTokenizer tokenizer)
        {
            try
            {
                // Column name

                var token = Next(tokenizer, TokenType.Identifier);
                Name = token.Value as string;
                if (Name == null)
                    throw new ArgumentException();
                if (Name.ToUpper() == "CONSTRAINT" || Name.ToUpper() == "FOREIGN")
                    return false;

                // Column type

                token = Next(tokenizer, TokenType.Identifier);
                Type = token.Value as string;
                if (Type == null)
                    throw new ArgumentException();
                Type = Type.ToUpper();

                // Type attributes

                token = Next(tokenizer);
                if (token.IsSeparator("("))
                {
                    token = Next(tokenizer);
                    if (token.Type == TokenType.Integer)
                        Length = Convert.ToInt32(token.Value);
                    else
                        Length = -1;
                    token = Next(tokenizer);
                    if (token.IsSeparator(","))
                    {
                        token = Next(tokenizer, TokenType.Integer);
                        Scale = Convert.ToInt32(token.Value);
                        token = Next(tokenizer);
                    }
                    else if (token.IsIdentifier("CHAR", StringComparison.OrdinalIgnoreCase))
                        token = Next(tokenizer);
                    if (!token.IsSeparator(")"))
                        throw new ArgumentException();
                    token = Next(tokenizer);
                }

                // NULL allowed?

                IsNullable = true;
                if (token.IsIdentifier("NOT", StringComparison.OrdinalIgnoreCase))
                {
                    token = Next(tokenizer);
                    if (!token.IsIdentifier("NULL", StringComparison.OrdinalIgnoreCase))
                        throw new ArgumentException();
                    IsNullable = false;
                    token = Next(tokenizer);
                }

                // Set type by sql type

                SetCodeProperties();

                // Move to the end of the column information

                var level = 0;
                while (token.Type != TokenType.Unknown && (level > 0 || !token.IsSeparator(",") && !token.IsSeparator(")")))
                {
                    if (token.IsSeparator("("))
                        level++;
                    else if (token.IsSeparator(")"))
                        level--;
                    token = Next(tokenizer);
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public bool ParseFromSelect(SimpleTokenizer tokenizer)
        {
            try
            {
                var token = Next(tokenizer);
                if (token.IsIdentifier("DISTINCT"))
                    token = Next(tokenizer, TokenType.Identifier);
                InternalName = token.Value as string;
                token = Next(tokenizer);
                if (token.IsSeparator("."))
                {
                    token = Next(tokenizer);
                    InternalName = token.Value as string;
                    token = Next(tokenizer);
                }
                if (InternalName == null)
                    throw new ArgumentException();
                InternalName = InternalName.ToUpper();
                Name = InternalName;
                if (!token.IsIdentifier("FROM", StringComparison.OrdinalIgnoreCase) && (token.Type == TokenType.Identifier || token.Type == TokenType.String))
                {
                    Name = token.Value as string;
                    if (Name == null)
                        throw new ArgumentException();
                    token = Next(tokenizer);
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public void GetAttributes(TableColumn column)
        {
            if (column == null)
                return;
            Type = column.Type;
            Length = column.Length;
            Scale = column.Scale;
            if (column.IsNullable)
                IsNullable = true;
            SetCodeProperties();
        }

        private void SetCodeProperties()
        {
            CodeName = Name;

            // Spaltentyp
            if (Type == "VARCHAR" || Type == "NVARCHAR" || Type == "VARCHAR2" || Type == "CLOB")
                CodeType = "string";
            else if (Type == "CHAR" || Type == "NCHAR")
                CodeType = "string";
            else if (Type == "INT" || Type == "SMALLINT" || Type == "TINYINT")
                CodeType = "int";
            else if (Type == "BIGINT")
                CodeType = "long";
            else if (Type == "BIT")
                CodeType = "bool";
            else if (Type == "DATE" || Type == "DATETIME")
                CodeType = "DateTime";
            else if (Type == "TIMESTAMP")
                CodeType = "DateTime";
            else if (Type == "VARBINARY" || Type == "RAW")
                CodeType = "byte[]";
            else if (Type == "NUMBER" || Type == "DECIMAL")
                CodeType = "decimal";

        }

        private static Token Next(SimpleTokenizer tokenizer, TokenType type = TokenType.Unknown)
        {
            while (tokenizer.Next())
            {
                if (tokenizer.Token.Type != TokenType.Comment)
                    break;
            }

            if (tokenizer.Token.Type == TokenType.Unknown)
                throw new EndOfStreamException();

            if (type != TokenType.Unknown && tokenizer.Token.Type != type)
                throw new ArgumentException();

            return tokenizer.Token;
        }
    }
}