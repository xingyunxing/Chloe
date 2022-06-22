using Chloe.Sharding.Routing;
using System.Reflection;

namespace Chloe.Sharding
{
    public interface IShardingConfig
    {
        /// <summary>
        /// 实体类型
        /// </summary>
        Type EntityType { get; }
        /// <summary>
        /// 分片字段
        /// </summary>
        IList<MemberInfo> ShardingKeys { get; }
        /// <summary>
        /// 路由工厂
        /// </summary>
        IShardingRouteFactory RouteFactory { get; }
    }

    internal class ShardingConfig : IShardingConfig
    {
        public ShardingConfig()
        {

        }

        public Type EntityType { get; set; }
        public IList<MemberInfo> ShardingKeys { get; set; }
        public IShardingRouteFactory RouteFactory { get; set; }
    }
}
