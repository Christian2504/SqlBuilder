using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using SqlBuilderFramework;

namespace SqlBuilderSamplesAndTests
{
    /// <summary>
    /// Replacement for stored procedures in SQLite
    /// </summary>
    public class SqliteProcedure : ISqlStatement
    {
        public IDatabase Database { get; private set; }

        public string Sql { get; set; }

        public CommandType Type { get; set; }

        public IEnumerable<IDbDataParameter> Parameters { get { return _parameters;} }
        private readonly IList<SQLiteParameter> _parameters;

        public SqliteProcedure(IDatabase database)
        {
            Database = database;
            _parameters = new List<SQLiteParameter>();
            Type = CommandType.StoredProcedure;
        }

        public IDbDataParameter AddParameter(string name, object value, DbType dbType, int size = 0, ParameterDirection direction = ParameterDirection.Input)
        {
            var parameter = new SQLiteParameter(dbType, value)
            {
                ParameterName = name,
                Direction = direction
            };

            if (size > 0)
                parameter.Size = size;

            _parameters.Add(parameter);

            return parameter;
        }

        public DbResultSet ExecuteReader()
        {
            throw new NotImplementedException();
        }

        public int ExecuteNonQuery()
        {
            // Strip schema name
            var dotpos = Sql.IndexOf(".", StringComparison.InvariantCulture);
            var procedureName = Sql.Substring(dotpos + 1).ToUpper();

            switch (procedureName)
            {
                case "DELETEDATA":
                    return DeleteData();
            }

            return 0;
        }

        public DataTable ToDataTable()
        {
            throw new NotImplementedException();
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private int DeleteData()
        {
            var sql = string.Format("DELETE FROM {0} WHERE SYSTEM_ID = :systemId AND {1} = :id", StringParameter("pi_Table"), StringParameter("pi_Column"));
            using (var stmt = Database.CreateStatement(sql))
            {
                stmt.AddParameter("systemId", LongParameter("pi_SystemID"), DbType.Int64);
                stmt.AddParameter("id", LongParameter("pi_ID"), DbType.Int64);

                return stmt.ExecuteNonQuery();
            }
        }

        private long? LongParameter(string name)
        {
            var parameter = _parameters.SingleOrDefault(par => par.ParameterName == name);

            if (parameter == null)
                return null;
            return parameter.Value as long?;
        }

        private string StringParameter(string name)
        {
            var parameter = _parameters.SingleOrDefault(par => par.ParameterName == name);

            if (parameter == null)
                return null;
            return parameter.Value as string;
        }
    }
}
