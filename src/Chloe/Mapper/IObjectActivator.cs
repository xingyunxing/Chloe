using System.Data;

namespace Chloe.Mapper
{
    public interface IObjectActivator
    {
        void Prepare(IDataReader reader);
        ObjectResultTask CreateInstance(IDataReader reader, bool @async);
    }
}
