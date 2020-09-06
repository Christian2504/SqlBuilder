using System;
using System.Data;

namespace SqlBuilderFramework
{
    public interface IDatabase : IDisposable
    {
        /// <summary>
        /// Datenbankname
        /// </summary>
        string DatabaseName { get; }

        /// <summary>
        /// ConnectionString
        /// </summary>
        string ConnectionString { get; }

        /// <summary>
        /// Datenbankprovider (Oracle, SQLite)
        /// </summary>
        DatabaseProvider Provider { get; }

        /// <summary>
        /// Schemaname in der Datenbank
        /// </summary>
        string SchemaName { get; }

        SqlBuilder Select { get; }
        SqlBuilder Insert { get; }
        SqlBuilder Update { get; }
        SqlBuilder Delete { get; }

        /// <summary>
        /// Gibt zurück, ob eine Datenbankverbindung offen ist.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Inhalt der Tabelle löschen
        /// </summary>
        /// <param name="tableName">Datenbanktabelle</param>
        /// <returns></returns>
        bool DeleteFromTable(string tableName);

        /// <summary>
        /// Erstellt eine Transaktion
        /// </summary>
        /// <returns>IDbTransaction</returns>
        IDbTransaction CreateTransaction();

        /// <summary>
        /// Erstellt ein Statement
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        ISqlStatement CreateStatement(string sql = null, CommandType type = CommandType.Text);

        /// <summary>
        /// Daten auslesen
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        SqlBuilderReader ExecuteReader(ISqlBuilder sql);

        /// <summary>
        /// Daten auslesen
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        DbResultSet ExecuteReader(string sql);

        /// <summary>
        /// Insert, Update, Delete Befehl ausführen
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        int ExecuteNonQuery(ISqlBuilder sql);

        /// <summary>
        /// Insert, Update, Delete Befehl ausführen
        /// </summary>
        /// <param name="sql">SQL-Kommand als String</param>
        /// <returns></returns>
        int ExecuteNonQuery(string sql);

        /// <summary>
        /// Gibt den Datenbandatentyp zurück
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        DbType ToDbType(Type type);

        /// <summary>
        /// Wandelt value in ein String-Literal um das in SQL-Ausdrücken verwendet werden kann.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        string ToLiteral(object value);

        /// <summary>
        /// Erstellt einen Parameternamen.
        /// Der Parametername sollte bereits dem SQL standard entsprechen.
        /// Es wird lediglich der datenbankspezifische Präfix (:, @, etc.) vorangestellt.
        /// </summary>
        /// <param name="name">Name des Parameters</param>
        /// <returns></returns>
        string ToParameterName(string name);

        /// <summary>
        /// Parameter erzeugen und dem Command hinzufügen
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="dbType"></param>
        /// <param name="size"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        IDbDataParameter CreateParameter(string name, object value, DbType dbType,
            int size = 0, ParameterDirection direction = ParameterDirection.Input);

        /// <summary>
        /// Prüft, ob eine Tabelle existiert.
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        bool TableExists(string tableName);

        /// <summary>
        /// Gibt das Schema zurück
        /// </summary>
        /// <param name="collectionName"></param>
        /// <param name="restrictionValues"></param>
        /// <returns></returns>
        DataTable GetSchema(string collectionName, string[] restrictionValues);

        /// <summary>
        /// Erzeugt je nach Backend die entsprechenden Exceptions
        /// </summary>
        /// <param name="sqlStatement"></param>
        /// <param name="exception"></param>
        Exception CreateException(string sqlStatement, Exception exception);
    }
}