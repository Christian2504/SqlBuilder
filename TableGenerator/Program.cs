using System;
using System.IO;

namespace TableGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2 || args.Length > 3)
            {
                Console.WriteLine("Falsche Anzahl Argumente!\nEs muss die SqlServer Datei mit Tabelleninformationen und die Zieldatei angegeben werden.");
                return;
            }

            var createTableSqlFile = args[0];
            var destFileName = args[1];
            string classNamespace;

            if (args.Length > 2)
            {
                classNamespace = args[2];
            }
            else
            {
                classNamespace = Path.GetFileName(Path.GetDirectoryName(destFileName));
            }

            try
            {
                string sqlTableDefs;

                using (var tableDefStream = new StreamReader(createTableSqlFile))
                    sqlTableDefs = tableDefStream.ReadToEnd();

                // Schema erstellen
                var dbSchema = new DbSchema(classNamespace);

                // Tokenizer zum Parsen des SQL nutzen
                dbSchema.ParseTables(new SimpleTokenizer(sqlTableDefs));

                dbSchema.WriteTablesScript(destFileName);

                Console.WriteLine($"{Path.GetFileName(destFileName)} erfolgreich aktualisiert.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
