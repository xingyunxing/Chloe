using Chloe.Infrastructure;
using Chloe.RDBMS;
using System.Data;

namespace Chloe.PostgreSQL
{
    public class PostgreSQLContext : DbContext
    {
        public PostgreSQLContext(Func<IDbConnection> dbConnectionFactory) : this(new DbConnectionFactory(dbConnectionFactory))
        {

        }

        public PostgreSQLContext(IDbConnectionFactory dbConnectionFactory) : this(new PostgreSQLOptions() { DbConnectionFactory = dbConnectionFactory })
        {

        }

        public PostgreSQLContext(PostgreSQLOptions options) : base(options, new DbContextProviderFactory(options))
        {
            this.Options = options;
        }

        public new PostgreSQLOptions Options { get; private set; }

        protected override DbContext CloneImpl()
        {
            PostgreSQLOptions options = new PostgreSQLOptions()
            {
                DbConnectionFactory = this.Options.DbConnectionFactory,
                InsertStrategy = this.Options.InsertStrategy,
                MaxInItems = this.Options.MaxInItems,
                DefaultInsertCountPerBatchForInsertRange = this.Options.DefaultInsertCountPerBatchForInsertRange,
                ConvertToLowercase = this.Options.ConvertToLowercase
            };

            PostgreSQLContext dbContext = new PostgreSQLContext(options);
            return dbContext;
        }

        /// <summary>
        /// 设置属性解析器。
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="handler"></param>
        public static void SetPropertyHandler(string propertyName, IPropertyHandler handler)
        {
            PostgreSQLContextProvider.SetPropertyHandler(propertyName, handler);
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
    }

    class DbContextProviderFactory : IDbContextProviderFactory
    {
        PostgreSQLOptions _options;

        public DbContextProviderFactory(PostgreSQLOptions options)
        {
            PublicHelper.CheckNull(options);
            this._options = options;
        }

        public IDbContextProvider CreateDbContextProvider()
        {
            return new PostgreSQLContextProvider(this._options);
        }
    }
}
