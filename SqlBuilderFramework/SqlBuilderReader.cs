namespace SqlBuilderFramework
{
    public class SqlBuilderReader : ISqlBuilderReader
    {
        private DbResultSet _resultSet;
        private ISqlBuilder _sqlBuilder;

        public SqlBuilderReader(DbResultSet resultSet, ISqlBuilder sqlBuilder)
        {
            _resultSet = resultSet;
            _sqlBuilder = sqlBuilder;
        }

        public bool Next()
        {
            if (!(_resultSet?.Next() ?? false))
                return false;

            _sqlBuilder?.SetValues(_resultSet);

            return true;
        }

        public void Dispose()
        {
            _resultSet?.Dispose();
        }
    }
}
