SQL Builder Framework
=====================

This framework is for developers who are familiar with SQL but want a type safe environment to create database commands.

This is not a code first approach putting the database into a straitjacket. This is a database first approach using the database schema in a properly typed C# code environment thus preserving all database capabilities and still have the C# compilier do the work for you.

If you love AutoMapper, SqlBuilder may not be the tool for you.

If you see that AutoMapper just leads to a lot of similar but not identical classes, making changes awkward and obfuscating data conversions, SqlBuilder may help you.

SqlBuilder does not attempt to guess how database columns are to be mapped to your business classes. It provides an easy way to explicitely map the properties of your business class to database columns (select expressions) and profiting from sql features like data conversions, sql expressions and entire select statements.

This is why with SqlBuilder you can build very fast database applications.

SqlBuilderFramework abstain from using reflection and expression parsing (linq) techniques so that its easily changeable and bugs can quickly be fixed.


Features
--------

Bulk insert.

An example
---------

    // The database interface (usually a member of your controlling class retrieved from an IOC container)

    var database = new SqliteDatabase(":memory:");

    // MyData is your business logic data class.
    // The only requirement is to have a default constructor.
    // This is not a DTO.
    // The DTOs can be auto-generated from an SQL script with the TableGenerator.

    class MyData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime? Timestamp { get; set; }
        public int? Number { get; set; }
    }

    // Create a mapper that defines how to map the values from the database into MyData

    var mapper = new DbMapper<MyData>()
        .map(Tables.MyDbTable.ColId, (p, x) => p.Id = x)
        .map(Tables.MyDbTable.ColName, (p, x) => p.Name = x)
        .map(Tables.MyDbTable.ColTimestamp, (p, x) => p.Timestamp = x)
        .map(Tables.MyDbTable.ColNumber, (p, x) => p.Number = x);

    // Create the query

    var query = database.Select
        .From(Tables.MyDbTable)
        .Where(Tables.MyDbTable.ColTimestamp < DateTime(2020, 1, 1));

    // Execute the query

    IEnumerable<MyData> result = query.ReadAll(mapper);

TODOs
-----

- Default join columns based on foreign keys.
- More readable syntax for setting values.
- Boilerplate DTO class with datasource, map, filter and assignment functions.
