using Chloe.Query;
using System.Data;

namespace Chloe.Mapper
{
    public interface IMemberBinder
    {
        void Prepare(IDataReader reader);
        ValueTask Bind(QueryContext queryContext, object obj, IDataReader reader, bool @async);
        IMemberBinder Clone();
    }
}
