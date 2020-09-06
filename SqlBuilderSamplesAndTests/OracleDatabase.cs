using System;
using System.Data;
using System.Globalization;
using System.Text;
using Oracle.ManagedDataAccess.Client;
using SqlBuilderFramework;

// Avoid devart if possible

namespace SqlBuilderSamplesAndTests
{
    /// <summary>
    /// Access to an Oracle database.
    /// </summary>
    public class OracleDatabase : AbstractDatabase
    {
        private OracleConnection _connection;

        /// <summary>
        /// NEVER make the connection public!!!
        /// Only this class controls the connection!!!
        /// </summary>
        protected override IDbConnection Connection => _connection;

        public override string DatabaseName => _connection.Database;

        public override DatabaseProvider Provider => DatabaseProvider.Oracle;

        public int FetchSize { get; set; }

        /// <inheritdoc />
        public override string SchemaName => string.Empty;

	public OracleDatabase(string host, string port, string dbName, string user, string password)
        {
            ConnectionString = $"Data Source={host}:{port}/{dbName};User Id={user};Password={password};";

            ConnectionString += "Max Pool Size=2;";

            _connection = new OracleConnection(ConnectionString);

            _connection.Open();
        }

        public OracleDatabase(string dbName, string user, string password)
        {
            ConnectionString = $"Data Source={dbName};User Id={user};Password={password};";

            ConnectionString += "Max Pool Size=2;";

            _connection = new OracleConnection(ConnectionString);

            _connection.Open();
        }

        public OracleDatabase(string connectionString)
        {
            ConnectionString = connectionString;

            _connection = new OracleConnection(ConnectionString);

            _connection.Open();
        }

        ~OracleDatabase()
        {
            Dispose(false);
        }

        /// <summary>
        /// Creates a new command as statement object
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public override ISqlStatement CreateStatement(string sql = null, CommandType type = CommandType.Text)
        {
            var command = _connection.CreateCommand();

            if (FetchSize > 0)
            {
                command.FetchSize = FetchSize;
            }

            return new SqlStatement(this, command) { Sql = sql, Type = type };
        }

        public override string ToLiteral(object value)
        {
            string literal;

            if (value == null)
                literal = "NULL";
            else if (value is int i)
                literal = i.ToString(CultureInfo.InvariantCulture);
            else if (value is long l)
                literal = l.ToString(CultureInfo.InvariantCulture);
            else if (value is decimal d)
                literal = d.ToString(CultureInfo.InvariantCulture);
            else if (value is DateTime dateTime)
                literal = $"TO_TIMESTAMP('{dateTime.ToString("dd/MM/yyyy HH:mm:ss,fffffff", CultureInfo.InvariantCulture)}', 'DD/MM/YYYY HH24:MI:SS,FF7')";
            else if (value is bool boolean)
                literal = boolean ? "1" : "0";
            else if (value is byte[] ba)
            {
                var hex = new StringBuilder(ba.Length * 2);
                foreach (var b in ba)
                    hex.AppendFormat("{0:X2}", b);
                literal = $"HEXTORAW('{hex}')";
            }
            else
                literal = $"'{value.ToString().Replace("'", "''")}'";

            if (literal == "''")
                literal = "NULL";

            return literal;
        }

        public override bool TableExists(string tableName)
        {
            using (var reader = ExecuteReader("select TABLE_NAME from ALL_TABLES where TABLE_NAME = '" + tableName.ToUpper() + "'"))
            {
                return reader.Next();
            }
        }

        public override IDbDataParameter CreateParameter(string name, object value, DbType dbType, int size = 0, ParameterDirection direction = ParameterDirection.Input)
        {
            var parameter = new OracleParameter
            {
                ParameterName = ToParameterName(name),
                Value = value ?? DBNull.Value,
                DbType = dbType,
                Direction = direction
            };

            if (size > 0)
                parameter.Size = size;

            if (value is string && ((string)value).Length > 3000)
                parameter.OracleDbType = OracleDbType.Clob;

            return parameter;
        }

        public override DataTable GetSchema(string collectionName, string[] restrictionValues)
        {
            return _connection.GetSchema(collectionName, restrictionValues);
        }

        public override string ToParameterName(string name)
        {
            return ':' + name;
        }

        public override DbType ToDbType(Type type)
        {
            // Den Basiswert wenn es Nullable ist
            if (type.IsGenericType && type.BaseType == typeof(ValueType))
            {
                type = type.GenericTypeArguments[0];
            }

            if (type == typeof(DateTime))
                return DbType.DateTime2;

            return base.ToDbType(type);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _connection.Dispose();
            }
        }
    }
}
