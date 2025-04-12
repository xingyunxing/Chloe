using Chloe.Infrastructure;
using System.Data;

namespace Chloe.PostgreSQL
{
    class DatabaseProvider : IDatabaseProvider
    {
        PostgreSQLContextProvider _contextProvider;

        public string DatabaseType { get { return "PostgreSQL"; } }

        public DatabaseProvider(PostgreSQLContextProvider contextProvider)
        {
            this._contextProvider = contextProvider;
        }

        public IDbConnection CreateConnection()
        {
            IDbConnection conn = this._contextProvider.Options.DbConnectionFactory.CreateConnection();
            return conn;
        }

        public IDbExpressionTranslator CreateDbExpressionTranslator()
        {
            return new DbExpressionTranslator(this._contextProvider);
        }

        public string CreateParameterName(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            if (name[0] == UtilConstants.ParameterNamePlaceholder[0])
            {
                return name;
            }

            return UtilConstants.ParameterNamePlaceholder + name;
        }
    }
}
