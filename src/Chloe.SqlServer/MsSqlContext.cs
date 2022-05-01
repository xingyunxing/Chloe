using Chloe.Infrastructure;
using System.Data;

namespace Chloe.SqlServer
{
    public class MsSqlContext : DbContext
    {
        public MsSqlContext(string connString) : this(new DefaultDbConnectionFactory(connString))
        {
        }

        public MsSqlContext(IDbConnectionFactory dbConnectionFactory) : base(new DbContextProviderFactory(dbConnectionFactory))
        {

        }
        public MsSqlContext(Func<IDbConnection> dbConnectionFactory) : this(new DbConnectionFactory(dbConnectionFactory))
        {
        }

        /// <summary>
        /// 分页模式。
        /// </summary>
        public PagingMode PagingMode
        {
            get
            {
                return (this.DefaultDbContextProvider as MsSqlContextProvider).PagingMode;
            }
            set
            {
                (this.DefaultDbContextProvider as MsSqlContextProvider).PagingMode = value;
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
            return new MsSqlContextProvider(this._dbConnectionFactory);
        }
    }
}
