using Chloe.Infrastructure;
using Chloe.RDBMS;
using System.Data;

namespace Chloe.PostgreSQL
{
    public class PostgreSQLContext : DbContext
    {
        public PostgreSQLContext(IDbConnectionFactory dbConnectionFactory) : base(new DbContextProviderFactory(dbConnectionFactory))
        {

        }
        public PostgreSQLContext(Func<IDbConnection> dbConnectionFactory) : this(new DbConnectionFactory(dbConnectionFactory))
        {
        }

        /// <summary>
        /// 设置方法解析器。
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="handler"></param>
        public static void SetMethodHandler(string methodName, IMethodHandler handler)
        {
            PostgreSQLContextProvider.SetMethodHandler(methodName, handler);
        }

        /// <summary>
        /// 是否将 sql 中的表名/字段名转成小写。默认为 true。
        /// </summary>
        public bool ConvertToLowercase
        {
            get
            {
                return (this.DefaultDbContextProvider as PostgreSQLContextProvider).ConvertToLowercase;
            }
            set
            {
                (this.DefaultDbContextProvider as PostgreSQLContextProvider).ConvertToLowercase = value;
            }
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
            return new PostgreSQLContextProvider(this._dbConnectionFactory);
        }
    }
}
