SQL Builder Framework
=====================

This framework is for developers who are familiar with SQL but need a type safe environment to create database commands.

Example
-------

    // The database interface (usually a member of your controlling class retrieved from an IOC container)

    var database = new SqliteDatabase(":memory:");

    // MyData is your business logic data class.
    // It is required to have a default constructor
    // but can otherwise be as complex as you want.
    // This is not a DTO.
    // The DTOs can be auto-generated from an SQL script with the TableGenerator.

    class MyData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime? Timestamp { get; set; }
        public int? Number { get; set; }
    }

    // Your query of MyDbTable mapped to MyData
    var mapper = new DbMapper<MyData>()
        .map(Tables.MyDbTable.ColId, (p, x) => p.Id = x)
        .map(Tables.MyDbTable.ColName, (p, x) => p.Name = x)
        .map(Tables.MyDbTable.ColTimestamp, (p, x) => p.Timestamp = x)
        .map(Tables.MyDbTable.ColNumber, (p, x) => p.Number = x);

    var query = database.Select.From(Tables.MyDbTable)
        .where(Tables.MyDbTable.ColTimestamp < QDateTime(2020, 1, 1));

    // Executing the query

    IEnumerable<MyData> result = query.ReadAll(mapper);
