using System.Diagnostics;
using System.IO;
using SqlBuilderFramework;
using Microsoft.Extensions.Options;
using Microsoft.Data.SqlClient;

namespace SqlBuilderSamplesAndTests
{
    public class DatabaseManager
    {
        private readonly IOptions<DatabaseSettings> _dbSettings;
        private readonly object _lock = new object();

        public DatabaseManager(IOptions<DatabaseSettings> databaseSettings)
        {
            _dbSettings = databaseSettings;
        }

        public IDatabase Connect()
        {
            lock (_lock)
            {
                IDatabase database = null;

                switch (_dbSettings.Value.Provider)
                {
                    case "Oracle":
                        database = ConnectToOracle();
                        break;
                    case "SqlServer":
                        database = ConnectToSqlServer();
                        break;
                    case "SQLite":
                        database = ConnectToSQLite();
                        break;
                }

                return database;
            }
        }

        private IDatabase ConnectToSqlServer()
        {
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = _dbSettings.Value.Server,
                InitialCatalog = _dbSettings.Value.Database,
                MultipleActiveResultSets = true,
                PersistSecurityInfo = false,
                UserID = _dbSettings.Value.UserName,
                Password = _dbSettings.Value.Password,
                TrustServerCertificate = _dbSettings.Value.TrustedConnection,
                ConnectRetryCount = 0
            };

            var connectionString = builder.ConnectionString;

            if (!string.IsNullOrEmpty(_dbSettings.Value.AdditionalSettings))
            {
                connectionString += ";" + _dbSettings.Value.AdditionalSettings;
            }

            return new SqlServerDatabase(connectionString);
        }

        private IDatabase ConnectToSQLite()
        {
            var dbName = _dbSettings.Value.Database;
            var isNew = dbName == ":memory:" || !File.Exists(dbName);
            var sqLite = new SqLiteDatabase(dbName);

            // Enable foreign keys

            sqLite.ExecuteNonQuery("PRAGMA foreign_keys = ON");
#if DEBUG
            using (var reader = sqLite.ExecuteReader("PRAGMA foreign_keys"))
            {
                Debug.Assert(reader.Next());
                Debug.Assert(reader.GetLong(0) == 1);
            }
#endif

            // Initialize a new database

            if (isNew)
            {
                var sqlPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase.Substring(8)), "sql");
                if (sqlPath[1] != ':')
                    sqlPath = "/" + sqlPath; // Linux
                sqLite.ExecuteNonQuery(File.ReadAllText(Path.Combine(sqlPath, "SampleSQLiteDatabaseCreateObjects.sql")));
                foreach (var stmt in File.ReadAllText(Path.Combine(sqlPath, "SampleSQLiteData.sql")).Split(";"))
                {
                    sqLite.ExecuteNonQuery(stmt);
                }
            }

            return sqLite;
        }

        private IDatabase ConnectToOracle()
        {
            var connectionString = $"Data Source={_dbSettings.Value.Database};User Id={_dbSettings.Value.UserName};Password={_dbSettings.Value.Password};Max Pool Size=2;";

            if (!string.IsNullOrEmpty(_dbSettings.Value.AdditionalSettings))
            {
                connectionString += _dbSettings.Value.AdditionalSettings;
            }

            return new OracleDatabase(connectionString);
        }
    }
}
