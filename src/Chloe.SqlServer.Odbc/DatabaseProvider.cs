using Chloe.Infrastructure;
using System.Data;

namespace Chloe.SqlServer.Odbc
{
    class DatabaseProvider : IDatabaseProvider
    {
        IDbConnectionFactory _dbConnectionFactory;
        MsSqlContextProvider _msSqlContextProvider;

        public string DatabaseType { get { return "SqlServer"; } }

        public DatabaseProvider(IDbConnectionFactory dbConnectionFactory, MsSqlContextProvider msSqlContextProvider)
        {
            this._dbConnectionFactory = dbConnectionFactory;
            this._msSqlContextProvider = msSqlContextProvider;
        }
        public IDbConnection CreateConnection()
        {
            return this._dbConnectionFactory.CreateConnection();
        }
        public IDbExpressionTranslator CreateDbExpressionTranslator()
        {
            if (this._msSqlContextProvider.PagingMode == PagingMode.ROW_NUMBER)
            {
                return DbExpressionTranslator.Instance;
            }
            else if (this._msSqlContextProvider.PagingMode == PagingMode.OFFSET_FETCH)
            {
                return DbExpressionTranslator_OffsetFetch.Instance;
            }

            throw new NotSupportedException();
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
