using System;
using System.Data;
using System.Data.SQLite;
using System.Globalization;
using System.Text;
using SqlBuilderFramework;

namespace SqlBuilderSamplesAndTests
{
    public class SqLiteDatabase : AbstractDatabase
    {
        private readonly SQLiteConnection _connection;

        /// <summary>
        /// NEVER make the connection public!!!
        /// Only this class controls the connection!!!
        /// </summary>
        protected override IDbConnection Connection => _connection;

        public override string DatabaseName => _connection.FileName;

        public override DatabaseProvider Provider => DatabaseProvider.Sqlite;

        /// <inheritdoc />
        public override string SchemaName => string.Empty;

        /// <summary>
        /// Remark: Do not close the connection of an in-memory database.
        /// </summary>
        /// <param name="name"></param>
        public SqLiteDatabase(string name)
        {
            ConnectionString = $"Data Source={name}";

            _connection = new SQLiteConnection(ConnectionString); // "FullUri=file::memory:?cache=shared"

            _connection.Open();
        }

        ~SqLiteDatabase()
        {
            Dispose(false);
        }

        public override ISqlStatement CreateStatement(string sql = null, CommandType type = CommandType.Text)
        {
            if (type == CommandType.StoredProcedure)
                return new SqliteProcedure(this) {Sql = sql};

            var command = _connection.CreateCommand();

            return new SqlStatement(this, command) {Sql = sql, Type = type};
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
                literal = $"'{dateTime.ToString("yyyy-MM-dd HH:mm:ss.fffffff", CultureInfo.InvariantCulture)}'";
            else if (value is bool boolean)
                literal = boolean ? "1" : "0";
            else if (value is byte[] ba)
            {
                var hex = new StringBuilder(ba.Length * 2);
                foreach (var b in ba)
                    hex.AppendFormat("{0:X2}", b);
                literal = $"X'{hex}'";
            }
            else
                literal = $"'{value.ToString().Replace("'", "''")}'";

            return literal;
        }

        public override bool TableExists(string tableName)
        {
            using (
                var reader =
                    ExecuteReader("SELECT name FROM sqlite_master WHERE type = 'table' AND name = '" +
                                  tableName.ToUpper() + "'"))
            {
                return reader.Next();
            }
        }

        public override IDbDataParameter CreateParameter(string name, object value, DbType dbType, int size = 0, ParameterDirection direction = ParameterDirection.Input)
        {
            var parameter = new SQLiteParameter
            {
                ParameterName = ToParameterName(name),
                Value = value,
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _connection.Dispose();
            }
        }
    }
}