using Microsoft.Extensions.Options;
using Unity;

namespace SqlBuilderSamplesAndTests
{
    public class Options<T> : IOptions<T> where T: class, new()
    {
        public T Value { get; set; }
    }

    public class start
    {
        public IUnityContainer Container = new UnityContainer();

        public void InitIocContainer()
        {
            var databaseOptions = new Options<DatabaseSettings>();
            databaseOptions.Value = new DatabaseSettings
            {
                Provider = "SQLite",
                Database = ":memory:"
            };

            Container.RegisterInstance<IOptions<DatabaseSettings>>(databaseOptions);
            Container.RegisterSingleton<DatabaseManager>();
        }
    }
}
