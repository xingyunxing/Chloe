using System.Data;

namespace Chloe.RDBMS
{
    public interface IDbParamCollection
    {
        int Count { get; }
        DbParam Find(object value, Type paramType, DbType? dbType);
        void Add(DbParam param);
        List<DbParam> ToParameterList();
    }
}
