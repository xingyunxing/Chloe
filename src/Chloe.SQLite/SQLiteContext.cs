using Chloe.Infrastructure;
using Chloe.RDBMS;
using System.Data;

namespace Chloe.SQLite
{
    public class SQLiteContext : DbContext
    {
        public SQLiteContext(Func<IDbConnection> dbConnectionFactory) : this(new DbConnectionFactory(dbConnectionFactory))
        {

        }

        public SQLiteContext(IDbConnectionFactory dbConnectionFactory) : this(new SQLiteOptions() { DbConnectionFactory = dbConnectionFactory })
        {

        }

        public SQLiteContext(SQLiteOptions options) : base(options, new DbContextProviderFactory(options))
        {
            this.Options = options;
        }

        public new SQLiteOptions Options { get; private set; }

        public override DbContext CloneImpl()
        {
            SQLiteOptions options = new SQLiteOptions()
            {
                DbConnectionFactory = this.Options.DbConnectionFactory,
                InsertStrategy = this.Options.InsertStrategy,
                MaxInItems = this.Options.MaxInItems,
                ConcurrencyMode = this.Options.ConcurrencyMode
            };

            SQLiteContext dbContext = new SQLiteContext(options);
            dbContext.ShardingEnabled = this.ShardingEnabled;
            dbContext.ShardingOptions.MaxConnectionsPerDataSource = this.ShardingOptions.MaxConnectionsPerDataSource;

            return dbContext;
        }

        /// <summary>
        /// 设置属性解析器。
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="handler"></param>
        public static void SetPropertyHandler(string propertyName, IPropertyHandler handler)
        {
            SQLiteContextProvider.SetPropertyHandler(propertyName, handler);
        }

        /// <summary>
        /// 设置方法解析器。
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="handler"></param>
        public static void SetMethodHandler(string methodName, IMethodHandler handler)
        {
            SQLiteContextProvider.SetMethodHandler(methodName, handler);
        }
    }

    class DbContextProviderFactory : IDbContextProviderFactory
    {
        SQLiteOptions _options;

        public DbContextProviderFactory(SQLiteOptions options)
        {
            PublicHelper.CheckNull(options);
            this._options = options;
        }

        public IDbContextProvider CreateDbContextProvider()
        {
            return new SQLiteContextProvider(this._options);
        }
    }
}
