using Chloe.Infrastructure;
using System.Data;

namespace Chloe.SQLite
{
    class DatabaseProvider : IDatabaseProvider
    {
        SQLiteContextProvider _contextProvider;

        public string DatabaseType { get { return "SQLite"; } }

        public DatabaseProvider(SQLiteContextProvider contextProvider)
        {
            this._contextProvider = contextProvider;
        }

        public IDbConnection CreateConnection()
        {
            IDbConnection conn = this._contextProvider.Options.DbConnectionFactory.CreateConnection();
            if (this._contextProvider.Options.ConcurrencyMode == true)
            {
                conn = new ChloeSQLiteConcurrentConnection(conn);
            }

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
