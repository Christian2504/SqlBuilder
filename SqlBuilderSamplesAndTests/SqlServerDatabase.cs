using System;
using System.Data;
using System.Globalization;
using System.Text;
using SqlBuilderFramework;
using Microsoft.Data.SqlClient;

namespace SqlBuilderSamplesAndTests
{
    public class SqlServerDatabase : AbstractDatabase
    {
        private SqlTransaction _transaction;

        private readonly SqlConnection _connection;

        /// <summary>
        /// NEVER make the connection public!!!
        /// Only this class controls the connection!!!
        /// </summary>
        protected override IDbConnection Connection => _connection;

        public override string DatabaseName => _connection.Database;

        public override DatabaseProvider Provider => DatabaseProvider.MsSql;

        /// <inheritdoc />
        public override string SchemaName => "dbo";

        public SqlServerDatabase(string serverName, string databaseName, string user, string password, bool trustedConnection)
        {
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = serverName,
                InitialCatalog = databaseName,
                MultipleActiveResultSets = true,
                PersistSecurityInfo = false,
                UserID = user,
                Password = password,
                TrustServerCertificate = trustedConnection,
                ConnectRetryCount = 0
            };

            ConnectionString = builder.ConnectionString;

            _connection = new SqlConnection(ConnectionString);

            _connection.Open();
        }

        public SqlServerDatabase(string connectionString)
        {
            ConnectionString = connectionString;

            _connection = new SqlConnection(ConnectionString);

            _connection.Open();
        }

        ~SqlServerDatabase()
        {
            Dispose(false);
        }

        public override IDbTransaction CreateTransaction()
        {
            _transaction = _connection.BeginTransaction();
            return _transaction;
        }

        public override IDbTransaction CreateTransaction(IsolationLevel il)
        {
            _transaction = _connection.BeginTransaction(il);
            return _transaction;
        }

        public override ISqlStatement CreateStatement(string sql = null, CommandType type = CommandType.Text)
        {
            if (_transaction != null && _transaction.Connection == null)
                _transaction = null;

            var command = _connection.CreateCommand();

            command.Transaction = _transaction;

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
                literal = $"CONVERT(DateTime2, '{dateTime.ToString("yyyy-MM-dd HH:mm:ss.fffffff", CultureInfo.InvariantCulture)}', 121)";
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
                literal = "'" + value.ToString().Replace("\\\r\n", "\\\\\r\n\r\n").Replace("\\\n", "\\\\\n\n").Replace("'", "''") + "'";

            return literal;
        }

        public override bool TableExists(string tableName)
        {
            using (var reader = ExecuteReader("SELECT TABLE_NAME from INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '" + tableName.ToUpper() + "'"))
            {
                return reader.Next();
            }
        }

        public override IDbDataParameter CreateParameter(string name, object value, DbType dbType, int size = 0, ParameterDirection direction = ParameterDirection.Input)
        {
            // DateTime.MinValue muss mit null übergeben werden
            if (value  is DateTime &&  (DateTime)value == DateTime.MinValue)
            {
                value = null;
            }

            var parameter = new SqlParameter
            {
                ParameterName = ToParameterName(name),
                Value = value ?? DBNull.Value,
                DbType = dbType,
                Direction = direction
            };

            if (size > 0)
                parameter.Size = size;

            return parameter;
        }

        public override DataTable GetSchema(string collectionName, string[] restrictionValues)
        {
            return _connection.GetSchema(collectionName, restrictionValues);
        }

        public override string ToParameterName(string name)
        {
            return '@' + name;
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
                _transaction?.Dispose();
                _connection.Dispose();
            }
        }
    }
}
