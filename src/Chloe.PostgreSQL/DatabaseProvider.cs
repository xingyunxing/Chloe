using Chloe.Infrastructure;
using System.Data;

namespace Chloe.PostgreSQL
{
    class DatabaseProvider : IDatabaseProvider
    {
        IDbConnectionFactory _dbConnectionFactory;
        PostgreSQLContextProvider _contextProvider;

        public string DatabaseType { get { return "PostgreSQL"; } }

        public DatabaseProvider(IDbConnectionFactory dbConnectionFactory, PostgreSQLContextProvider contextProvider)
        {
            this._dbConnectionFactory = dbConnectionFactory;
            this._contextProvider = contextProvider;
        }
        public IDbConnection CreateConnection()
        {
            return this._dbConnectionFactory.CreateConnection();
        }
        public IDbExpressionTranslator CreateDbExpressionTranslator()
        {
            return new DbExpressionTranslator(this._contextProvider);
        }
        public string CreateParameterName(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            if (name[0] == UtilConstants.ParameterNamePlaceholer[0])
            {
                return name;
            }

            return UtilConstants.ParameterNamePlaceholer + name;
        }
    }
}
