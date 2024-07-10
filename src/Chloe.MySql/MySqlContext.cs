using Chloe.Infrastructure;
using Chloe.RDBMS;
using System.Data;

namespace Chloe.MySql
{
    public class MySqlContext : DbContext
    {
        public MySqlContext(Func<IDbConnection> dbConnectionFactory) : this(new DbConnectionFactory(dbConnectionFactory))
        {

        }

        public MySqlContext(IDbConnectionFactory dbConnectionFactory) : this(new MySqlOptions() { DbConnectionFactory = dbConnectionFactory })
        {

        }

        public MySqlContext(MySqlOptions options) : base(options, new DbContextProviderFactory(options))
        {
            this.Options = options;
        }

        public new MySqlOptions Options { get; private set; }

        protected override DbContext CloneImpl()
        {
            MySqlOptions options = new MySqlOptions()
            {
                DbConnectionFactory = this.Options.DbConnectionFactory,
                InsertStrategy = this.Options.InsertStrategy,
                MaxInItems = this.Options.MaxInItems
            };

            MySqlContext dbContext = new MySqlContext(options);
            return dbContext;
        }

        /// <summary>
        /// 设置属性解析器。
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="handler"></param>
        public static void SetPropertyHandler(string propertyName, IPropertyHandler handler)
        {
            MySqlContextProvider.SetPropertyHandler(propertyName, handler);
        }

        /// <summary>
        /// 设置方法解析器。
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="handler"></param>
        public static void SetMethodHandler(string methodName, IMethodHandler handler)
        {
            MySqlContextProvider.SetMethodHandler(methodName, handler);
        }
    }

    class DbContextProviderFactory : IDbContextProviderFactory
    {
        MySqlOptions _options;

        public DbContextProviderFactory(MySqlOptions options)
        {
            PublicHelper.CheckNull(options);
            this._options = options;
        }

        public IDbContextProvider CreateDbContextProvider()
        {
            return new MySqlContextProvider(this._options);
        }
    }
}
