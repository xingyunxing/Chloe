using System.Data;

namespace Chloe.Reflection
{
    public delegate void MultMemberMapper(object instance, IDataReader dataReader, IReadingOrdinal readingOrdinal);
}
