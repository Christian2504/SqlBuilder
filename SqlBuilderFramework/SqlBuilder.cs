using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace SqlBuilderFramework
{
    public class SqlBuilder : ISqlBuilder
    {
        public static SqlBuilder Select => new SqlBuilder(SqlType.Select);

        public static SqlBuilder Insert => new SqlBuilder(SqlType.Insert);

        public static SqlBuilder Update => new SqlBuilder(SqlType.Update);

        public static SqlBuilder Delete => new SqlBuilder(SqlType.Delete);

        public static DbMapper<C> Mapper<C>(Func<C> addEntity) where C : new()
        {
            return new DbMapper<C>(addEntity);
        }

        private DbTable _in;
        private bool _distinct;
        private readonly List<DbColumn> _selectedColumns;
        private readonly List<string> _assignmentColumns;
        private readonly List<List<DbColumn>> _assignmentValues;
        private int _assignmentRow;
        private readonly List<KeyValuePair<DbColumn, bool>> _orderList;
        private int _offset;
        private int _limit;
        private int _autoColumnAliasCount;

        public enum SqlType
        {
            Select,
            Insert,
            Update,
            Delete
        };

        public SqlType StatementType { get; set; }

        public IDatabase Database { get; private set; }

        public DbTable Source { get; protected set; }

        public IList<DbColumn> SelectList => _selectedColumns;

        public DbConstraint Constraint { get; private set; }

        public bool IsOrdered => _orderList.Count > 0;

        public bool HasAssignments => _assignmentColumns.Count > 0;

        public SqlBuilder(SqlType sqlType, IDatabase database = null)
        {
            StatementType = sqlType;
            Database = database;
            _selectedColumns = new List<DbColumn>();
            _assignmentColumns = new List<string>();
            _assignmentValues = new List<List<DbColumn>>();
            _orderList = new List<KeyValuePair<DbColumn, bool>>();
        }

        public SqlBuilder From(DbTable table)
        {
            Source = table;
            return this;
        }

        public SqlBuilder In(DbTable table)
        {
            _in = table;
            return this;
        }

        public SqlBuilder Set(params DbAssignment[] assignments)
        {
            foreach (var assignment in assignments)
            {
                if (_assignmentRow == _assignmentValues.Count)
                    _assignmentValues.Add(new List<DbColumn>());

                var columnIndex = _assignmentColumns.IndexOf(assignment.Left.TableColumnName);

                if (columnIndex == -1)
                {
                    columnIndex = _assignmentColumns.Count;
                    _assignmentColumns.Add(assignment.Left.TableColumnName);
                }

                while (columnIndex >= _assignmentValues[_assignmentRow].Count)
                    _assignmentValues[_assignmentRow].Add(null);
                _assignmentValues[_assignmentRow][columnIndex] = assignment.Right;
            }

            return this;
        }

        public SqlBuilder Where(DbConstraint constraint)
        {
            Constraint = constraint;
            return this;
        }

        public SqlBuilder Offset(int offset)
        {
            _offset = offset;
            return this;
        }

        public SqlBuilder Limit(int limit)
        {
            _limit = limit;
            return this;
        }

        public SqlBuilder And(DbConstraint constraint)
        {
            if (Constraint == null)
                return Where(constraint);

            Constraint &= constraint;
            return this;
        }

        public SqlBuilder Or(DbConstraint constraint)
        {
            if (Constraint == null)
                return Where(constraint);

            Constraint |= constraint;
            return this;
        }

        public SqlBuilder Distinct(bool distinct = true)
        {
            _distinct = distinct;
            return this;
        }

        public SqlBuilder OrderBy(params DbColumn[] columns) // Ascending
        {
            return OrderAscending(columns);
        }

        public SqlBuilder OrderAscending(params DbColumn[] columns)
        {
            foreach (var column in columns)
            {
                if ((object)column != null)
                    _orderList.Add(new KeyValuePair<DbColumn, bool>(column, true));
            }
            return this;
        }

        public SqlBuilder OrderDescending(params DbColumn[] columns)
        {
            foreach (var column in columns)
            {
                if ((object)column != null)
                    _orderList.Add(new KeyValuePair<DbColumn, bool>(column, false));
            }
            return this;
        }

        public DbTypedBoundValue<T> Bind<T>(DbTypedColumn<T> column)
        {
            var boundValue = new DbTypedBoundValue<T>();
            column.BoundValue = boundValue;
            _selectedColumns.Add(column);
            return boundValue;
        }

        public SqlBuilder AddSelectColumn(params DbColumn[] columns)
        {
            _selectedColumns.AddRange(columns);
            return this;
        }

        public void StashRow()
        {
            if (_assignmentRow < _assignmentValues.Count)
                _assignmentRow++;
        }

        public void StashClear()
        {
            _assignmentRow = 0;
        }

        public string NextColumnAlias()
        {
            return $"COL{++_autoColumnAliasCount}";
        }

        public SqlBuilder Map<T>(DbTypedColumn<T> column, Action<T> action)
        {
            Bind(column).Setter = action;
            return this;
        }

        public T? ReadValue<T>(IDatabase database, DbTypedColumn<T> column) where T : struct
        {
            Database = database;
            return ReadValue(column);
        }

        public T? ReadValue<T>(DbTypedColumn<T> column) where T : struct
        {
            _selectedColumns.Clear();

            var col = Bind(column);

            using (var reader = ExecuteReader())
            {
                if (reader.Next())
                {
                    return col.Value;
                }
            }

            return null;
        }

        public T ReadNullableValue<T>(IDatabase database, DbTypedColumn<T> column) where T : class
        {
            Database = database;
            return ReadNullableValue(column);
        }

        public T ReadNullableValue<T>(DbTypedColumn<T> column) where T : class
        {
            _selectedColumns.Clear();

            var col = Bind(column);

            using (var reader = ExecuteReader())
            {
                if (reader.Next())
                {
                    return col.Value;
                }
            }

            return null;
        }

        public List<T> ReadValues<T>(IDatabase database, DbTypedColumn<T> column)
        {
            Database = database;
            return ReadValues(column);
        }

        public List<T> ReadValues<T>(DbTypedColumn<T> column)
        {
            _selectedColumns.Clear();

            var result = new List<T>();
            var col = Bind(column);

            using (var reader = ExecuteReader())
            {
                while (reader.Next())
                {
                    result.Add(col.Value);
                }
            }

            return result;
        }

        public void ReadSingle(IDatabase database)
        {
            Database = database;
            ReadSingle();
        }

        public void ReadSingle()
        {
            using (var reader = ExecuteReader())
            {
                reader.Next();
            }
        }

        public void ReadSingle(IDatabase database, Action action)
        {
            Database = database;
            ReadSingle(action);
        }

        public void ReadSingle(Action action)
        {
            using (var reader = ExecuteReader())
            {
                if (reader.Next())
                {
                    action();
                }
            }
        }

        public void ReadSingle<T>(IDatabase database, DbTypedColumn<T> column, Action<T> action)
        {
            Database = database;
            ReadSingle(column, action);
        }

        public void ReadSingle<T>(DbTypedColumn<T> column, Action<T> action)
        {
            var selCount = _selectedColumns.Count;

            var b1 = Bind(column);

            using (var reader = ExecuteReader())
            {
                if (reader.Next())
                {
                    action(b1.Value);
                }
            }

            _selectedColumns.RemoveRange(selCount, 1);
        }

        public void ReadSingle<T1, T2>(IDatabase database, DbTypedColumn<T1> col1, DbTypedColumn<T2> col2, Action<T1, T2> action)
        {
            Database = database;
            ReadSingle(col1, col2, action);
        }

        public void ReadSingle<T1, T2>(DbTypedColumn<T1> col1, DbTypedColumn<T2> col2, Action<T1, T2> action)
        {
            var selCount = _selectedColumns.Count;

            var b1 = Bind(col1);
            var b2 = Bind(col2);

            using (var reader = ExecuteReader())
            {
                if (reader.Next())
                {
                    action(b1.Value, b2.Value);
                }
            }

            _selectedColumns.RemoveRange(selCount, 2);
        }

        public void ReadSingle<T1, T2, T3>(IDatabase database, DbTypedColumn<T1> col1, DbTypedColumn<T2> col2, DbTypedColumn<T3> col3, Action<T1, T2, T3> action)
        {
            Database = database;
            ReadSingle(col1, col2, col3, action);
        }

        public void ReadSingle<T1, T2, T3>(DbTypedColumn<T1> col1, DbTypedColumn<T2> col2, DbTypedColumn<T3> col3, Action<T1, T2, T3> action)
        {
            var selCount = _selectedColumns.Count;

            var b1 = Bind(col1);
            var b2 = Bind(col2);
            var b3 = Bind(col3);

            using (var reader = ExecuteReader())
            {
                if (reader.Next())
                {
                    action(b1.Value, b2.Value, b3.Value);
                }
            }

            _selectedColumns.RemoveRange(selCount, 3);
        }

        public List<C> ReadAll<C>(IDatabase database, DbMapper<C> mapper) where C : new()
        {
            Database = database;
            return ReadAll(mapper);
        }

        public List<C> ReadAll<C>(DbMapper<C> mapper) where C : new()
        {
            var selCount = _selectedColumns.Count;

            mapper.BindColumns(this);

            using (var reader = ExecuteReader())
            {
                mapper.NewList();

                while (reader.Next())
                {
                    mapper.AddEntity();
                }

                mapper.End();
            }

            _selectedColumns.RemoveRange(selCount, _selectedColumns.Count - selCount);

            return mapper.EntityList;
        }

        public void ReadAll(IDatabase database, Action action)
        {
            Database = database;
            ReadAll(action);
        }

        public void ReadAll(Action action)
        {
            using (var reader = ExecuteReader())
            {
                while (reader.Next())
                {
                    action();
                }
            }
        }

        public void ReadAll<T>(IDatabase database, DbTypedColumn<T> column, Action<T> action)
        {
            Database = database;
            ReadAll(column, action);
        }

        public void ReadAll<T>(DbTypedColumn<T> column, Action<T> action)
        {
            var selCount = _selectedColumns.Count;

            var b1 = Bind(column);

            using (var reader = ExecuteReader())
            {
                while (reader.Next())
                {
                    action(b1.Value);
                }
            }

            _selectedColumns.RemoveRange(selCount, 1);
        }

        public void ReadAll<T1, T2>(IDatabase database, DbTypedColumn<T1> col1, DbTypedColumn<T2> col2, Action<T1, T2> action)
        {
            Database = database;
            ReadAll(col1, col2, action);
        }

        public void ReadAll<T1, T2>(DbTypedColumn<T1> col1, DbTypedColumn<T2> col2, Action<T1, T2> action)
        {
            var selCount = _selectedColumns.Count;

            var b1 = Bind(col1);
            var b2 = Bind(col2);

            using (var reader = ExecuteReader())
            {
                while (reader.Next())
                {
                    action(b1.Value, b2.Value);
                }
            }

            _selectedColumns.RemoveRange(selCount, 2);
        }

        public void ReadAll<T1, T2, T3>(IDatabase database, DbTypedColumn<T1> col1, DbTypedColumn<T2> col2, DbTypedColumn<T3> col3, Action<T1, T2, T3> action)
        {
            Database = database;
            ReadAll(col1, col2, col3, action);
        }

        public void ReadAll<T1, T2, T3>(DbTypedColumn<T1> col1, DbTypedColumn<T2> col2, DbTypedColumn<T3> col3, Action<T1, T2, T3> action)
        {
            var selCount = _selectedColumns.Count;

            var b1 = Bind(col1);
            var b2 = Bind(col2);
            var b3 = Bind(col3);

            using (var reader = ExecuteReader())
            {
                while (reader.Next())
                {
                    action(b1.Value, b2.Value, b3.Value);
                }

                _selectedColumns.RemoveRange(selCount, 3);
            }
        }

        public SqlBuilderReader ExecuteReader(IDatabase database)
        {
            Database = database;
            return ExecuteReader();
        }

        public SqlBuilderReader ExecuteReader()
        {
            using (var command = Command())
            {
                return new SqlBuilderReader(command?.ExecuteReader(), this);
            }
        }

        public int Execute(IDatabase database)
        {
            return ExecuteNonQuery(database);
        }

        public int Execute()
        {
            return ExecuteNonQuery();
        }

        public int ExecuteNonQuery(IDatabase database)
        {
            Database = database;
            return ExecuteNonQuery();
        }

        public int ExecuteNonQuery()
        {
            var command = Command();

            if (command == null)
                return 0;

            _assignmentRow = 0; // Clear stash

            if (_selectedColumns.Count > 0)
            {
                if (Database.Provider == DatabaseProvider.Sqlite) // SQLite kann nur die zuletzt eingefügte RowId zurückgeben (siehe auch die Methode sql())
                {
                    // INSERT-Befehl ausführen und den zuletzt verwendeten Auto-Increment-Wert abfragen.
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Next())
                        {
                            var rowId = reader.GetLong(0);

                            if (rowId == 0L)
                                return 0;

                            _selectedColumns[0].BoundValue?.SetValue(rowId);

                            if (_selectedColumns.Count > 1)
                            {
                                // TODO: Select using rowId
                            }

                            return 1;
                        }
                    }

                    return 0;
                }

                if (Database.Provider == DatabaseProvider.MsSql)
                {
                    // INSERT-Befehl als Abfrage ausführen.
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Next())
                        {
                            for (var i = 0; i < _selectedColumns.Count; ++i)
                            {
                                _selectedColumns[i].BoundValue?.SetValue(reader.GetValue(i));
                            }

                            return 1;
                        }
                    }

                    return 0;
                }
            }

            // DML ausfühern
            var affectedRows = command.ExecuteNonQuery();

            if (affectedRows == 1 && _selectedColumns.Count > 0) // Implizite Rückgabewerte abfragen
            {
                if (Database.Provider == DatabaseProvider.Oracle)
                {
                    int i = 0;
                    foreach (var parameter in command.Parameters)
                    {
                        if (i == _selectedColumns.Count)
                            break;

                        _selectedColumns[i].BoundValue?.SetValue(parameter.Value);
                        i++;
                    }
                }
                //else if (database.Provider == DatabaseProvider.MsSql)
                //{
                //    int i = 0;
                //    foreach (var parameter in command.Parameters)
                //    {
                //        if (i == _selectedColumns.Count)
                //            break;

                //        _selectedColumns[i].BoundValue?.SetValue(parameter.Value);
                //        i++;
                //    }
                //}
            }

            return affectedRows;
        }

        public void SetValues(DbResultSet resultSet)
        {
            for (int i = 0; i < _selectedColumns.Count; i++)
                _selectedColumns[i].BoundValue?.SetValue(resultSet, i);
        }

        public string Sql(List<ParameterEntity> parameterList, IDatabase database)
        {
            Database = database;
            return Sql(parameterList);
        }

        public string Sql(List<ParameterEntity> parameterList)
        {
            var sql = new StringBuilder();

            switch (StatementType)
            {
                case SqlType.Select:
                    sql.Append("SELECT ");
                    if (_distinct)
                        sql.Append("DISTINCT ");
                    sql.Append(string.Join(", ", _selectedColumns.Select(column => column.Definition(parameterList, this, Database))));
                    sql.Append(" FROM ");
                    sql.Append(Source.Sql(parameterList, this, Database));
                    if (Constraint != null)
                    {
                        sql.Append(" WHERE ");
                        sql.Append(Constraint.Sql(parameterList, this, Database));
                    }
                    if (_selectedColumns.Any(column => column.IsAggregate))
                    {
                        var groupClause = string.Join(", ", _selectedColumns.Where(column => column.IsGroupable).Select(binding => binding.Sql(parameterList, this, Database)));

                        if (!string.IsNullOrEmpty(groupClause))
                        {
                            sql.Append(" GROUP BY ");
                            sql.Append(groupClause);
                        }
                    }
                    if (_orderList.Any())
                    {
                        sql.Append(" ORDER BY ");
                        sql.Append(string.Join(", ", _orderList.Select(order => (order.Key.Alias ?? order.Key.Sql(parameterList, this, Database)) + (order.Value ? "" : " DESC"))));
                        if (Database.Provider == DatabaseProvider.Sqlite)
                        {
                            if (_limit > 0)
                            {
                                sql.Append(" LIMIT ");
                                sql.Append(_limit);
                                if (_offset > 0)
                                {
                                    sql.Append(" OFFSET ");
                                    sql.Append(_offset);
                                }
                            }
                        }
                        else
                        {
                            if (_offset > 0 || _limit > 0)
                            {
                                sql.Append(" OFFSET ");
                                sql.Append(_offset);
                                sql.Append(" ROWS");
                            }
                            if (_limit > 0)
                            {
                                sql.Append(" FETCH NEXT ");
                                sql.Append(_limit);
                                sql.Append(" ROWS ONLY");
                            }
                        }
                    }
                    break;
                case SqlType.Insert:
                    sql.Append("INSERT INTO ");
                    sql.Append(_in.Sql(parameterList, this, Database));
                    sql.Append(" (");
                    sql.Append(string.Join(", ", _assignmentColumns));
                    sql.Append(")");
                    if (_selectedColumns.Count > 0) // Nach dem Insert sollen Werte zurückgegeben werden
                    {
                        if (Database.Provider == DatabaseProvider.MsSql)
                        {
                            sql.Append(" OUTPUT ");
                            sql.Append(string.Join(", ", _selectedColumns.Select(binding => "INSERTED.[" + binding.TableColumnName + "]")));
                            //sql.Append(" INTO ");
                            //for (int i = 0; i < _selectedColumns.Count; i++)
                            //{
                            //    if (i > 0)
                            //        sql.Append(", ");
                            //    sql.Append("@par" + i.ToString("D2"));
                            //}
                        }
                    }
                    sql.Append(" VALUES ");
                    sql.Append(string.Join(", ", _assignmentValues.Select(valueList => "(" + string.Join(", ", valueList.Select(value => value.Sql(parameterList, this, Database))) + ")")));
                    if (_selectedColumns.Count > 0) // Nach dem Insert sollen Werte zurückgegeben werden
                    {
                        if (Database.Provider == DatabaseProvider.Oracle)
                        {
                            // Bei Oracle geschieht dies mit der RETURNING ... INTO clause.
                            sql.Append(" RETURNING ");
                            sql.Append(string.Join(", ", _selectedColumns.Select(binding => binding.TableColumnName)));
                            sql.Append(" INTO ");
                            for (int i = 0; i < _selectedColumns.Count; i++)
                            {
                                if (i > 0)
                                    sql.Append(", ");
                                sql.Append(":par" + i.ToString("D2"));
                            }
                        }
                        else if (Database.Provider == DatabaseProvider.Sqlite)
                        {
                            // Bei SQLite bekommt man nur den letzten Auto-Increment-Wert zurück
                            // Achtung: last_insert_rowid() funktioniert nur auf der gleichen Connection, d.h. die Connection darf zwischenzeitlich nicht geschlossen worden sein.
                            //          Deshalb werden die beiden Kommandos hier in einem Befehl zusammengefaßt.
                            sql.Append("; SELECT last_insert_rowid();");
                        }
                    }
                    break;
                case SqlType.Update:
                    sql.Append("UPDATE ");
                    sql.Append(_in.Sql(parameterList, this, Database));
                    sql.Append(" SET ");
                    sql.Append(string.Join(", ", _assignmentColumns.Zip(_assignmentValues[0], (left, right) => left + " = " + right.Sql(parameterList, this, Database))));
                    if (Constraint != null)
                    {
                        sql.Append(" WHERE ");
                        sql.Append(Constraint.Sql(parameterList, this, Database));
                    }
                    break;
                case SqlType.Delete:
                    sql.Append("DELETE ");
                    if (Constraint == null)
                    {
                        sql.Append(Source.Sql(parameterList, this, Database));
                    }
                    else
                    {
                        sql.Append("FROM ");
                        sql.Append(Source.Sql(parameterList, this, Database));
                        sql.Append(" WHERE ");
                        sql.Append(Constraint.Sql(parameterList, this, Database));
                    }
                    break;
            }

            return sql.ToString();
        }

        private ISqlStatement Command()
        {
            if (Database == null)
                return null;

            var parameterList = new List<ParameterEntity>();
            var command = Database.CreateStatement(Sql(parameterList, Database));

            if (string.IsNullOrEmpty(command.Sql))
                return null;

            if (StatementType != SqlType.Select)
            {
                if (Database.Provider == DatabaseProvider.Oracle)
                {
                    // Supply the parameters for the RETURNING...INTO clause
                    var counter = 0;

                    foreach (var column in _selectedColumns)
                    {
                        command.AddParameter("par" + counter.ToString("D2"), null, Database.ToDbType(column.Type), 0, ParameterDirection.Output);
                        counter++;
                    }
                }
                //else if (database.Provider == DatabaseProvider.MsSql)
                //{
                //    // Supply the parameters for the OUTPUT...INTO clause
                //    var counter = 0;

                //    foreach (var column in _selectedColumns)
                //    {
                //        command.AddParameter("par" + counter.ToString("D2"), column.DbValue, database.ToDbType(column.ValueType), 0, ParameterDirection.Output);
                //        counter++;
                //    }
                //}
            }

            foreach (var parameter in parameterList)
            {
                command.AddParameter(parameter.Name, parameter.Value, Database.ToDbType(parameter.Type));
            }

            return command;
        }
    }
}
