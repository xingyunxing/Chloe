using Chloe.Infrastructure;
using System.Reflection;

namespace Chloe.Mapper
{
    public class MemberMap
    {
        public MemberMap()
        {

        }

        public MemberMap(MemberInfo member, int ordinal, IDbValueConverter dbValueConverter)
        {
            this.Member = member;
            this.Ordinal = ordinal;
            this.DbValueConverter = dbValueConverter;
        }

        public MemberInfo Member { get; set; }
        public int Ordinal { get; set; }
        public IDbValueConverter DbValueConverter { get; set; }
    }

    public class MapInfo
    {
        public MemberMap MemberMap { get; set; }
        public Type ReaderDataType { get; set; }
    }
}
