using System.Reflection;

namespace Chloe.Sharding
{
    class ShardingKey
    {
        public MemberInfo Member { get; set; }
        public object Value { get; set; }
    }
}
