using SqlBuilderFramework;
using Unity;
using Xunit;

namespace SqlBuilderSamplesAndTests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var t = new start();

            t.InitIocContainer();

            var dbManager = t.Container.Resolve<DatabaseManager>();

            var database = dbManager.Connect();

            Assert.Equal(313, database.Delete.From(Tables.stocks).Where(Tables.stocks.Colstore_id == 1).ExecuteNonQuery());
            var deleter = database.Delete.From(Tables.products).Where(Tables.products.Colproduct_id.NotIn(SqlBuilder.Select.From(Tables.stocks).AddSelectColumn(Tables.stocks.Colproduct_id)));
            Assert.Equal("DELETE FROM PRODUCTS WHERE (PRODUCTS.product_id NOT IN (SELECT STOCKS.product_id FROM STOCKS))", deleter.Sql(null));
            Assert.Equal(8, deleter.ExecuteNonQuery());
        }
    }
}
