using Chloe.Query;
using System.Data;

namespace Chloe.Mapper
{
    public interface IObjectActivator
    {
        void Prepare(IDataReader reader);
        ObjectResultTask CreateInstance(QueryContext queryContext, IDataReader reader, bool @async);

        IObjectActivator Clone();
    }
}
