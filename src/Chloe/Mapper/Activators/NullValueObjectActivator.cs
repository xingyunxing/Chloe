using System.Data;

namespace Chloe.Mapper.Activators
{
    public class NullValueObjectActivator : IObjectActivator
    {
        private object _value;

        public NullValueObjectActivator(Type primitiveType)
        {
            _value = primitiveType.IsValueType ? Activator.CreateInstance(primitiveType) : null;
        }

        public virtual void Prepare(IDataReader reader)
        {
        }

        public async ObjectResultTask CreateInstance(IDataReader reader, bool @async) => _value;
    }
}