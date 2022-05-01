using Chloe.Infrastructure;
using System.Data;

namespace Chloe.Oracle
{
    public class OracleContext : DbContext
    {
        public OracleContext(IDbConnectionFactory dbConnectionFactory) : base(new DbContextProviderFactory(dbConnectionFactory))
        {

        }
        public OracleContext(Func<IDbConnection> dbConnectionFactory) : this(new DbConnectionFactory(dbConnectionFactory))
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
            return new OracleContextProvider(this._dbConnectionFactory);
        }
    }
}
