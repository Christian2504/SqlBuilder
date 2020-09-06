using System;
using System.Data;

namespace SqlBuilderFramework
{
    public abstract class AbstractDatabase : IDatabase
    {
        /// <summary>
        /// Wichtig: NIEMALS die Connection öffentlich machen!!!
        /// Sie darf nur von dieser Klasse kontrolliert werden!!!
        /// </summary>
        protected abstract IDbConnection Connection { get; }

        #region Public Properties

        /// <summary>
        /// Datenbankname
        /// </summary>
        public abstract string DatabaseName { get; }

        /// <summary>
        /// Der ConnectionString für die Connection
        /// </summary>
        public string ConnectionString { get; protected set; }

        /// <summary>
        /// Flag, ob eine Connection existiert.
        /// </summary>
        public bool IsConnected => Connection.State == ConnectionState.Open;

        /// <summary>
        /// Datenbank-Provider (Oracle, MSSQL)
        /// </summary>
        public abstract DatabaseProvider Provider { get; }

        /// <summary>
        /// Schemaname in der Datenbank
        /// </summary>
        public abstract string SchemaName { get; }

        #endregion

        public SqlBuilder Select => new SqlBuilder(SqlBuilder.SqlType.Select, this);
        public SqlBuilder Insert => new SqlBuilder(SqlBuilder.SqlType.Insert, this);
        public SqlBuilder Update => new SqlBuilder(SqlBuilder.SqlType.Update, this);
        public SqlBuilder Delete => new SqlBuilder(SqlBuilder.SqlType.Delete, this);

        public abstract ISqlStatement CreateStatement(string sql = null, CommandType type = CommandType.Text);

        /// <summary>
        /// Wandelt value in ein String-Literal um das in SQL-Ausdrücken verwendet werden kann.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public abstract string ToLiteral(object value);

        public static string EscapeForLike(string text)
        {
            return text?.Replace("%", "[%]").Replace("_", "[_]").Replace("[", "[[]");
        }

        /// <summary>
        /// Parameter erzeugen und dem Command hinzufügen
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="dbType"></param>
        /// <param name="size"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public abstract IDbDataParameter CreateParameter(string name, object value, DbType dbType, int size = 0, ParameterDirection direction = ParameterDirection.Input);

        /// <summary>
        /// Prüft, ob eine Tabelle existiert.
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public abstract bool TableExists(string tableName);

        /// <summary>
        /// Gibt das Schema zurück
        /// </summary>
        /// <param name="collectionName"></param>
        /// <param name="restrictionValues"></param>
        /// <returns></returns>
        public abstract DataTable GetSchema(string collectionName, string[] restrictionValues);

        /// <summary>
        /// Erzeugt je nach Backend die entsprechenden Exceptions
        /// </summary>
        /// <param name="sqlStatement"></param>
        /// <param name="exception"></param>
        public virtual Exception CreateException(string sqlStatement, Exception exception)
        {
            return new ApplicationException(exception.Message + "\n\n" + sqlStatement);
        }

        /// <summary>
        /// Gibt den Datenbankdatentyp zurück
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Erstellt einen datenbankspezifischen Parameternamen.
        /// Parameternamen NICHT kürzen, da er sonst nicht mehr eindeutig ist.
        /// </summary>
        /// <param name="name">Name des Parameters. Er muss bereits gültig sein! Nur die datenbankspezifische Kennung wird vorangestellt (z.B. @ bei MsSql).</param>
        /// <returns></returns>
        public virtual string ToParameterName(string name)
        {
            // Standardimplementation die für SQLite und Oracle gültig ist.
            return ':' + name;
        }

        /// <summary>
        /// Erstellt eine Transaktion
        /// </summary>
        /// <returns>IDbTransaction</returns>
        public virtual IDbTransaction CreateTransaction()
        {
            return Connection.BeginTransaction();
        }

        /// <summary>
        /// TODO: Was kann man mit dem IsolationLevel machen.
        /// TODO: Diese Methode wird nicht verwendet.
        /// TODO: Diese Methode ist nicht in der Schnittstelle IDatabase
        /// </summary>
        /// <param name="il"></param>
        /// <returns></returns>
        public virtual IDbTransaction CreateTransaction(IsolationLevel il)
        {
            return Connection.BeginTransaction(il);
        }

        /// <summary>
        /// Inhalt der Tabelle löschen
        /// </summary>
        /// <param name="tableName">Datenbanktabelle</param>
        /// <returns></returns>
        public bool DeleteFromTable(string tableName)
        {
            using (var statement = CreateStatement("DELETE FROM " + tableName))
            {
                statement.ExecuteNonQuery();
            }
            return true;
        }

        /// <summary>
        /// Daten auslesen
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public SqlBuilderReader ExecuteReader(ISqlBuilder sql)
        {
            return sql.ExecuteReader(this);
        }

        /// <summary>
        /// Daten auslesen
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public DbResultSet ExecuteReader(string sql)
        {
            using (var statement = CreateStatement(sql))
            {
                return statement.ExecuteReader();
            }
        }

        /// <summary>
        /// Insert, Update, Delete Befehl ausführen
        /// </summary>
        /// <param name="sql"></param>
        /// <returns>Anzahl der betroffenen Datensätze</returns>
        public int ExecuteNonQuery(ISqlBuilder sql)
        {
            return sql.ExecuteNonQuery(this);
        }

        /// <summary>
        /// Insert, Update, Delete Befehl ausführen
        /// </summary>
        /// <param name="sql"></param>
        /// <returns>Anzahl der betroffenen Datensätze</returns>
        public int ExecuteNonQuery(string sql)
        {
            using (var statement = CreateStatement(sql))
            {
                return statement.ExecuteNonQuery();
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
        protected abstract void Dispose(bool disposing);
    }
}
