using Chloe.Infrastructure;
using System.Data;

namespace Chloe.Dameng
{
    class DatabaseProvider : IDatabaseProvider
    {
        IDbConnectionFactory _dbConnectionFactory;

        public string DatabaseType { get { return "Dameng"; } }

        public DatabaseProvider(IDbConnectionFactory dbConnectionFactory)
        {
            this._dbConnectionFactory = dbConnectionFactory;
        }
        public IDbConnection CreateConnection()
        {
            IDbConnection conn = this._dbConnectionFactory.CreateConnection();
            return conn;
        }
        public IDbExpressionTranslator CreateDbExpressionTranslator()
        {
            return DbExpressionTranslator.Instance;
        }
        public string CreateParameterName(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            if (name[0] == UtilConstants.ParameterNamePlaceholer[0])
            {
                return name;
            }

            return UtilConstants.ParameterNamePlaceholer + name;
        }
    }
}
