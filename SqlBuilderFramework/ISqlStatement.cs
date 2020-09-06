using System;
using System.Collections.Generic;
using System.Data;

namespace SqlBuilderFramework
{
    public interface ISqlStatement : IDisposable
    {
        IDatabase Database { get; }

        string Sql { get; set; }

        CommandType Type { get; set; }

        IEnumerable<IDbDataParameter> Parameters { get; }

        IDbDataParameter AddParameter(string name, object value, DbType dbType, int size = 0, ParameterDirection direction = ParameterDirection.Input);

        DbResultSet ExecuteReader();

        int ExecuteNonQuery();
    }
}
