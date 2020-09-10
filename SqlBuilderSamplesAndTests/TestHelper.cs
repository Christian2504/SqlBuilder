using Microsoft.Extensions.Options;
using Unity;

namespace SqlBuilderSamplesAndTests
{
    public class Options<T> : IOptions<T> where T: class, new()
    {
        public T Value { get; set; }
    }

    public static class TestHelper
    {
        public static void RegisterDbManager(IUnityContainer container)
        {
            var databaseOptions = new Options<DatabaseSettings>();
            databaseOptions.Value = new DatabaseSettings
            {
                Provider = "SQLite",
                Database = ":memory:"
            };

            container.RegisterInstance<IOptions<DatabaseSettings>>(databaseOptions);
            container.RegisterSingleton<DatabaseManager>();
        }
    }
}
