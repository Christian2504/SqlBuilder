using SqlBuilderFramework;
using Unity;
using Xunit;

namespace SqlBuilderSamplesAndTests
{
    public class BasicTests
    {
        [Fact]
        public void DeleteUnusedEntities()
        {
            IUnityContainer container = new UnityContainer();

            TestHelper.RegisterDbManager(container);

            var dbManager = container.Resolve<DatabaseManager>();

            var database = dbManager.Connect();

            Assert.Equal(313, database.Delete.From(Tables.stocks).Where(Tables.stocks.Colstore_id == 1).ExecuteNonQuery());
            var deleter = database.Delete.From(Tables.products).Where(Tables.products.Colproduct_id.NotIn(SqlBuilder.Select.From(Tables.stocks).AddSelectColumn(Tables.stocks.Colproduct_id)));
            Assert.Equal("DELETE FROM PRODUCTS WHERE (PRODUCTS.product_id NOT IN (SELECT STOCKS.product_id FROM STOCKS))", deleter.Sql(null));
            Assert.Equal(8, deleter.ExecuteNonQuery());
        }

        [Fact]
        public void ConcatUserName()
        {
            IUnityContainer container = new UnityContainer();

            TestHelper.RegisterDbManager(container);

            var dbManager = container.Resolve<DatabaseManager>();

            var database = dbManager.Connect();

            var tblUsers = new TblUsers();

            var userNames = database.Select
                .From(tblUsers)
                .ReadValues(
                    DbColumn.CaseWhen<string>(
                        tblUsers.ColLastName.IsNull() | tblUsers.ColLastName == string.Empty,
                        tblUsers.ColFirstName,
                        tblUsers.ColFirstName.ConcatWith(" ").ConcatWith(tblUsers.ColLastName)));

            Assert.Equal(new[] { "Armin Administrator", "Max Mustermann", "Anton Tester" }, userNames);
        }
    }
}
