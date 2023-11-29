using Chloe.Infrastructure;
using Chloe.RDBMS;
using System.Data;

namespace Chloe.SQLite
{
    public class SQLiteContext : DbContext
    {
        public SQLiteContext(IDbConnectionFactory dbConnectionFactory) : this(dbConnectionFactory, true)
        {

        }
        public SQLiteContext(IDbConnectionFactory dbConnectionFactory, bool concurrencyMode) : base(new DbContextProviderFactory(dbConnectionFactory, concurrencyMode))
        {

        }
        public SQLiteContext(Func<IDbConnection> dbConnectionFactory) : this(new DbConnectionFactory(dbConnectionFactory), true)
        {
        }
        public SQLiteContext(Func<IDbConnection> dbConnectionFactory, bool concurrencyMode) : this(new DbConnectionFactory(dbConnectionFactory), concurrencyMode)
        {
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
        IDbConnectionFactory _dbConnectionFactory;
        bool _concurrencyMode;

        public DbContextProviderFactory(IDbConnectionFactory dbConnectionFactory, bool concurrencyMode)
        {
            PublicHelper.CheckNull(dbConnectionFactory);
            this._dbConnectionFactory = dbConnectionFactory;
            this._concurrencyMode = concurrencyMode;
        }

        public IDbContextProvider CreateDbContextProvider()
        {
            return new SQLiteContextProvider(this._dbConnectionFactory, this._concurrencyMode);
        }
    }
}
