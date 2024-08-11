using Chloe.Query;
using Chloe.Reflection;
using System.Data;

namespace Chloe.Mapper.Binders
{
    public class MemberBinder : IMemberBinder
    {
        MemberSetter _setter;
        IObjectActivator _activtor;

        public MemberBinder(MemberSetter setter, IObjectActivator activtor)
        {
            this._setter = setter;
            this._activtor = activtor;
        }
        public virtual void Prepare(IDataReader reader)
        {
            this._activtor.Prepare(reader);
        }
        public virtual async ValueTask Bind(QueryContext queryContext, object obj, IDataReader reader, bool @async)
        {
            object val = await this._activtor.CreateInstance(queryContext, reader, @async);
            this._setter(obj, val);
        }

        public virtual IMemberBinder Clone()
        {
            MemberBinder memberBinder = new MemberBinder(this._setter, this._activtor.Clone());
            return memberBinder;
        }
    }
}
