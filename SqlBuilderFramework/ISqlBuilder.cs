using System.Collections.Generic;
using System.Data;

namespace SqlBuilderFramework
{
    public interface ISqlBuilder
    {
        ISqlBuilderReader ExecuteReader(IDatabase database);

        int ExecuteNonQuery(IDatabase database);

        void SetValues(DbResultSet resultSet);

        string NextColumnAlias();

        string Sql(List<ParameterEntity> parameterList, IDatabase database);
    }
}
