using Chloe.Infrastructure;
using Chloe.RDBMS;
using System.Data;

namespace Chloe.MySql
{
    public class MySqlContext : DbContext
    {
        public MySqlContext(IDbConnectionFactory dbConnectionFactory) : base(new DbContextProviderFactory(dbConnectionFactory))
        {

        }
        public MySqlContext(Func<IDbConnection> dbConnectionFactory) : this(new DbConnectionFactory(dbConnectionFactory))
        {
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
        IDbConnectionFactory _dbConnectionFactory;

        public DbContextProviderFactory(IDbConnectionFactory dbConnectionFactory)
        {
            PublicHelper.CheckNull(dbConnectionFactory);
            this._dbConnectionFactory = dbConnectionFactory;
        }

        public IDbContextProvider CreateDbContextProvider()
        {
            return new MySqlContextProvider(this._dbConnectionFactory);
        }
    }
}
