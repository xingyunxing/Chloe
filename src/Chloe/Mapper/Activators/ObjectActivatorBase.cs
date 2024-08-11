using Chloe.Query;
using System.Data;

namespace Chloe.Mapper.Activators
{
    public abstract class ObjectActivatorBase : IObjectActivator
    {
        public virtual void Prepare(IDataReader reader)
        {

        }

        public abstract ObjectResultTask CreateInstance(QueryContext queryContext, IDataReader reader, bool @async);

        public abstract IObjectActivator Clone();
    }
}
