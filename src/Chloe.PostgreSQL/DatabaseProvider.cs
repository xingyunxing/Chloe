using Chloe.Infrastructure;
using System.Data;

namespace Chloe.PostgreSQL
{
    class DatabaseProvider : IDatabaseProvider
    {
        IDbConnectionFactory _dbConnectionFactory;
        PostgreSQLContextProvider _postgreSQLContextProvider;

        public string DatabaseType { get { return "PostgreSQL"; } }

        public DatabaseProvider(IDbConnectionFactory dbConnectionFactory, PostgreSQLContextProvider postgreSQLContextProvider)
        {
            this._dbConnectionFactory = dbConnectionFactory;
            this._postgreSQLContextProvider = postgreSQLContextProvider;
        }
        public IDbConnection CreateConnection()
        {
            return this._dbConnectionFactory.CreateConnection();
        }
        public IDbExpressionTranslator CreateDbExpressionTranslator()
        {
            if (this._postgreSQLContextProvider.ConvertToLowercase == true)
            {
                return DbExpressionTranslator_ConvertToLowercase.Instance;
            }
            else
            {
                return DbExpressionTranslator.Instance;
            }
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
