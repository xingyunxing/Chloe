using System.Reflection;

namespace Chloe.Sharding
{
    public interface IShardingConfig
    {
        Type EntityType { get; }
        MemberInfo ShardingKey { get; }
        IShardingRouteFactory RouteFactory { get; }
    }

    internal class ShardingConfig : IShardingConfig
    {
        public ShardingConfig()
        {

        }

        public Type EntityType { get; set; }
        public MemberInfo ShardingKey { get; set; }
        public IShardingRouteFactory RouteFactory { get; set; }
    }
}
