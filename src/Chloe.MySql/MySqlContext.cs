using Chloe.Infrastructure;
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
