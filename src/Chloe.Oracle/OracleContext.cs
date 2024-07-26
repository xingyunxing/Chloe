using Chloe.Infrastructure;
using Chloe.RDBMS;
using System.Data;

namespace Chloe.Oracle
{
    public class OracleContext : DbContext
    {
        public OracleContext(Func<IDbConnection> dbConnectionFactory) : this(new DbConnectionFactory(dbConnectionFactory))
        {

        }

        public OracleContext(IDbConnectionFactory dbConnectionFactory) : this(new OracleOptions() { DbConnectionFactory = dbConnectionFactory })
        {

        }

        public OracleContext(OracleOptions options) : base(options, new DbContextProviderFactory(options))
        {
            this.Options = options;
        }

        public new OracleOptions Options { get; private set; }

        protected override DbContext CloneImpl()
        {
            OracleOptions options = new OracleOptions()
            {
                DbConnectionFactory = this.Options.DbConnectionFactory,
                InsertStrategy = this.Options.InsertStrategy,
                MaxNumberOfParameters = this.Options.MaxNumberOfParameters,
                MaxInItems = this.Options.MaxInItems,
                DefaultInsertCountPerBatchForInsertRange = this.Options.DefaultInsertCountPerBatchForInsertRange,
                ConvertToUppercase = this.Options.ConvertToUppercase
            };

            OracleContext dbContext = new OracleContext(options);
            return dbContext;
        }

        /// <summary>
        /// 设置属性解析器。
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="handler"></param>
        public static void SetPropertyHandler(string propertyName, IPropertyHandler handler)
        {
            OracleContextProvider.SetPropertyHandler(propertyName, handler);
        }

        /// <summary>
        /// 设置方法解析器。
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="handler"></param>
        public static void SetMethodHandler(string methodName, IMethodHandler handler)
        {
            OracleContextProvider.SetMethodHandler(methodName, handler);
        }
    }

    class DbContextProviderFactory : IDbContextProviderFactory
    {
        OracleOptions _options;

        public DbContextProviderFactory(OracleOptions options)
        {
            PublicHelper.CheckNull(options);
            this._options = options;
        }

        public IDbContextProvider CreateDbContextProvider()
        {
            return new OracleContextProvider(this._options);
        }
    }
}
