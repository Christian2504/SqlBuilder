using SqlBuilderFramework;
using System;
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

        [Fact]
        public void AddOrder()
        {
            IUnityContainer container = new UnityContainer();

            TestHelper.RegisterDbManager(container);

            var dbManager = container.Resolve<DatabaseManager>();

            var database = dbManager.Connect();

            Assert.Equal(1616, database.Insert
                .In(Tables.orders)
                .Set(
                    Tables.orders.Colcustomer_id.To(1),
                    Tables.orders.Colorder_status.To(2),
                    Tables.orders.Colorder_date.To(new DateTime(2020, 6, 6)),
                    Tables.orders.Colrequired_date.To(new DateTime(2020, 9, 30)),
                    Tables.orders.Colstore_id.To(1),
                    Tables.orders.Colstaff_id.To(1))
                .ReadValue(Tables.orders.Colorder_id));
        }
    }
}
