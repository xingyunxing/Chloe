using Chloe.Query;
using System.Data;

namespace Chloe.Mapper.Activators
{
    public class NullValueObjectActivator : IObjectActivator
    {
        private object _value;

        public NullValueObjectActivator(Type primitiveType)
        {
            this._value = primitiveType.IsValueType ? Activator.CreateInstance(primitiveType) : null;
        }

        NullValueObjectActivator(object value)
        {
            this._value = value;
        }

        public virtual void Prepare(IDataReader reader)
        {
        }

        public async ObjectResultTask CreateInstance(QueryContext queryContext, IDataReader reader, bool @async) => _value;

        public IObjectActivator Clone()
        {
            return new NullValueObjectActivator(this._value);
        }
    }
}