using Chloe.Infrastructure;
using System.Data;

namespace Chloe.SqlServer
{
    class DatabaseProvider : IDatabaseProvider
    {
        MsSqlContextProvider _contextProvider;

        public string DatabaseType { get { return "SqlServer"; } }

        public DatabaseProvider(MsSqlContextProvider _contextProvider)
        {
            this._contextProvider = _contextProvider;
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

            if (name[0] == UtilConstants.ParameterNamePlaceholer[0])
            {
                return name;
            }

            return UtilConstants.ParameterNamePlaceholer + name;
        }
    }
}
