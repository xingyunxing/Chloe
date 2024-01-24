using Chloe.RDBMS;
using System.Data;

namespace Chloe.SqlServer
{
    class DbParamCollectionWithoutReuse : IDbParamCollection
    {
        List<DbParam> _dbParams = new List<DbParam>();

        public int Count { get => _dbParams.Count; }

        public DbParam Find(object value, Type paramType, DbType? dbType)
        {
            List<DbParam> dbParamList = this._dbParams;
            if (value == DBNull.Value)
            {
                return dbParamList.Find(a => a.Type == paramType);
            }
            else
            {
                return dbParamList.Find(a => a.DbType == dbType);
            }
        }

        public void Add(DbParam param)
        {
            this._dbParams.Add(param);
        }

        public List<DbParam> ToParameterList()
        {
            return this._dbParams;
        }
    }
}
