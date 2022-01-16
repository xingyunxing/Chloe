using System.Data;

namespace Chloe.Mapper.Activators
{
    public abstract class ObjectActivatorBase : IObjectActivator
    {
        public virtual void Prepare(IDataReader reader)
        {

        }
        public abstract ObjectResultTask CreateInstance(IDataReader reader, bool @async);
    }
}
