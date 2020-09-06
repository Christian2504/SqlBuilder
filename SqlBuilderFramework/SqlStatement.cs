using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SqlBuilderFramework
{
    /// <summary>
    /// Führt alle SQL-Befehle (SELECT, INSERT, UPDATE, DELETE) für alle Datenbanken (Oracle, SQLite) aus.
    /// Nur diese Klasse darf SQL-Befehle ausführen.
    /// </summary>
    public sealed class SqlStatement : ISqlStatement
    {
        /// <summary>
        /// Datenbank
        /// </summary>
        public IDatabase Database { get; private set; }

        /// <summary>
        /// Command
        /// </summary>
        private IDbCommand _command;

        /// <summary>
        /// SQL
        /// </summary>
        public string Sql { get { return _command == null ? string.Empty : _command.CommandText; } set { if (_command != null) _command.CommandText = value; } }

        /// <summary>
        /// CommandType
        /// </summary>
        public CommandType Type { get { return _command == null ? CommandType.Text : _command.CommandType; } set { if (_command != null) _command.CommandType = value; } }

        /// <summary>
        /// Parameter
        /// </summary>
        public IEnumerable<IDbDataParameter> Parameters
        {
            get
            {
                if (_command == null)
                    return null;
                return _command.Parameters.Cast<IDbDataParameter>();
            }
        }

        /// <summary>
        /// Konstruktor
        /// </summary>
        /// <param name="database"></param>
        /// <param name="command"></param>
        public SqlStatement(IDatabase database, IDbCommand command)
        {
            Database = database;
            _command = command;
            Type = CommandType.Text;
        }

        /// <summary>
        /// Parameter hinzufügen
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="dbType"></param>
        /// <param name="size"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public IDbDataParameter AddParameter(string name, object value, DbType dbType, int size = 0, ParameterDirection direction = ParameterDirection.Input)
        {
            if (_command == null)
                return null;

            var parameter = Database.CreateParameter(name, value, dbType, size, direction);

            _command.Parameters.Add(parameter);

            return parameter;
        }

        /// <summary>
        /// Daten auslesen über DbResultSet
        /// </summary>
        /// <param name="sqlBuilder"></param>
        /// <returns></returns>
        public DbResultSet ExecuteReader()
        {
            if (_command == null)
                return null;

            try
            {
                return new DbResultSet(_command.ExecuteReader());
            }
            catch (Exception exception)
            {
                throw Database.CreateException(_command.CommandText, exception);
            }
        }

        /// <summary>
        /// Insert, Update, Delete Befehl ausführen
        /// </summary>
        /// <returns></returns>
        public int ExecuteNonQuery()
        {
            if (_command == null)
                return 0;

            try
            {
                return _command.ExecuteNonQuery();
            }
            catch (Exception exception)
            {
                throw Database.CreateException(_command.CommandText, exception);
            }
        }

        /// <summary>
        /// Speicher aufräumen
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Speicher aufräumen
        /// </summary>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_command != null)
                {
                    _command.Dispose();
                    _command = null;
                }
            }
        }
    }
}
