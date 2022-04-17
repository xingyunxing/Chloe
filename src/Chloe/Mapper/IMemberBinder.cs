using System.Data;

namespace Chloe.Mapper
{
    public interface IMemberBinder
    {
        void Prepare(IDataReader reader);
        ValueTask Bind(object obj, IDataReader reader, bool @async);
    }
}
