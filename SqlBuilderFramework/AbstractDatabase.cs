using System;
using System.Data;

namespace SqlBuilderFramework
{
    public abstract class AbstractDatabase : IDatabase
    {
        public abstract string DatabaseName { get; }

        public string ConnectionString { get; protected set; }

        public abstract DatabaseProvider Provider { get; }

        public abstract string SchemaName { get; }

        public SqlBuilder Select => new SqlBuilder(SqlBuilder.SqlType.Select, this);
        public SqlBuilder Insert => new SqlBuilder(SqlBuilder.SqlType.Insert, this);
        public SqlBuilder Update => new SqlBuilder(SqlBuilder.SqlType.Update, this);
        public SqlBuilder Delete => new SqlBuilder(SqlBuilder.SqlType.Delete, this);

        public abstract bool IsConnected { get; }

        public abstract ISqlStatement CreateStatement(string sql = null, CommandType type = CommandType.Text);

        public abstract string ToLiteral(object value);

        public static string EscapeForLike(string text)
        {
            return text?.Replace("%", "[%]").Replace("_", "[_]").Replace("[", "[[]");
        }

        public abstract IDbDataParameter CreateParameter(string name, object value, DbType dbType, int size = 0, ParameterDirection direction = ParameterDirection.Input);

        public abstract bool TableExists(string tableName);

        public abstract DataTable GetSchema(string collectionName, string[] restrictionValues);

        public virtual Exception CreateException(string sqlStatement, Exception exception)
        {
            return new ApplicationException(exception.Message + "\n\n" + sqlStatement);
        }

        public virtual DbType ToDbType(Type type)
        {
            // Den Basiswert wenn es Nullable ist
            if (type.IsGenericType && type.BaseType == typeof(ValueType))
            {
                type = type.GenericTypeArguments[0];
            }

            if (type == typeof(int))
                return DbType.Int32;
            if (type == typeof(long))
                return DbType.Int64;
            if (type == typeof(double))
                return DbType.Double;
            if (type == typeof(decimal))
                return DbType.Decimal;
            if (type == typeof(bool))
                return DbType.Boolean;
            if (type == typeof(char))
                return DbType.StringFixedLength;
            if (type == typeof(DateTime))
                return DbType.DateTime;
            if (type == typeof(byte[]))
                return DbType.Binary;
            if (type == typeof(Guid))
                return DbType.Guid;
            if (type == typeof(object))
                return DbType.Object;
            return DbType.String;
        }

        public virtual string ToParameterName(string name)
        {
            return ':' + name;
        }

        public abstract IDbTransaction CreateTransaction();

        public abstract IDbTransaction CreateTransaction(IsolationLevel il);

        public bool DeleteFromTable(string tableName)
        {
            using (var statement = CreateStatement("DELETE FROM " + tableName))
            {
                statement.ExecuteNonQuery();
            }
            return true;
        }

        public ISqlBuilderReader ExecuteReader(ISqlBuilder sql)
        {
            return sql.ExecuteReader(this);
        }

        public DbResultSet ExecuteReader(string sql)
        {
            using (var statement = CreateStatement(sql))
            {
                return statement.ExecuteReader();
            }
        }

        public int ExecuteNonQuery(ISqlBuilder sql)
        {
            return sql.ExecuteNonQuery(this);
        }

        public int ExecuteNonQuery(string sql)
        {
            using (var statement = CreateStatement(sql))
            {
                return statement.ExecuteNonQuery();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract void Dispose(bool disposing);
    }
}
