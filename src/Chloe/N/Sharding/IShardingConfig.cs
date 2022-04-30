using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Chloe.Sharding
{
    public interface IShardingConfig
    {
        Type EntityType { get; }
        MemberInfo ShardingKey { get; }
        IShardingRouteFactory RouteFactory { get; }
    }

    internal class ShardingConfigFacade : IShardingConfig
    {
        public ShardingConfigFacade()
        {

        }

        public Type EntityType { get; set; }
        public MemberInfo ShardingKey { get; set; }
        public IShardingRouteFactory RouteFactory { get; set; }
    }
}
