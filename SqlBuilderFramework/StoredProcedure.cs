using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SqlBuilderFramework
{
    /// <summary>
    /// Aufruf einer StoredProcedure
    /// Todo: noch ungetestet!
    /// </summary>
    public class StoredProcedure : IDisposable
    {
        private readonly List<IDbDataParameter> _outParams;

        private ISqlStatement _statement;

        private readonly string _name;

        /// <summary>
        /// Konstruktor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="database"></param>
        public StoredProcedure(string name, IDatabase database)
        {
            _statement = database.CreateStatement(name, CommandType.StoredProcedure);
            _outParams = new List<IDbDataParameter>();
            _name = name;
        }

        /// <summary>
        /// Statischer Aufruf
        /// </summary>
        /// <param name="name"></param>
        /// <param name="database"></param>
        /// <returns></returns>
        public static StoredProcedure Call(string name, IDatabase database)
        {
            return new StoredProcedure(name, database);
        }

        /// <summary>
        /// In Parameter hinzufügen
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public StoredProcedure In(string name, object value)
        {
            _statement.AddParameter(name, value, _statement.Database.ToDbType(value.GetType()));
            return this;
        }

        /// <summary>
        /// Out Parameter hinzufügen
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public StoredProcedure Out(string name, Type type, int size = 0)
        {
            _outParams.Add(_statement.AddParameter(name, null, _statement.Database.ToDbType(type), size, ParameterDirection.Output));
            return this;
        }

        /// <summary>
        /// InOut Parameter hinzufügen
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public StoredProcedure InOut(string name, Type type, object value, int size = 0)
        {
            _outParams.Add(_statement.AddParameter(name, value, _statement.Database.ToDbType(type), size, ParameterDirection.InputOutput));
            return this;
        }

        //public StoredProcedure OutClob(string name)
        //{
        //    var param = new OracleParameter
        //    {
        //        ParameterName = name,
        //        OracleType = OracleType.Clob,
        //        Direction = ParameterDirection.Output
        //    };
        //    _command.Parameters.Add(param);
        //    _outParams.Add(new OutParam(param));
        //    return this;
        //}

        /// <summary>
        /// Ausführen
        /// </summary>
        /// <returns></returns>
        public int Execute()
        {
            var result = _statement.ExecuteNonQuery();
            return result;
        }

        public DbResultSet ExecuteReader()
        {
            return _statement.ExecuteReader();
        }

       /// <summary>
       /// Gibt den Wert des Out-Parameter zurück
       /// </summary>
       /// <returns></returns>
        public object GetOutParam()
        {
            return _outParams.ElementAt(0).Value;
        }

        /// <summary>
        /// Gibt die Werte als Array der Out-Parameter zurück
        /// </summary>
        /// <returns></returns>
        public object[] GetOutParams()
        {
            return _outParams.Select(outParam => outParam.Value).ToArray();
        }

        /// <summary>
        /// ToString wurde überschrieben
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (_statement != null && !string.IsNullOrEmpty(_statement.Sql))
                return _statement.Sql;
            return string.Empty;
        }

        /// <summary>
        /// Speicher aufräumen
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(disposing)
            {
                if (_statement != null)
                {
                    _statement.Dispose();
                    _statement = null;
                }
            }
        }
    }
}
