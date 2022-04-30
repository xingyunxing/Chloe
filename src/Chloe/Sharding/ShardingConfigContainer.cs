//namespace Chloe.Sharding
//{
//    public class ShardingConfigContainer
//    {
//        static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, IShardingConfig> InstanceCache = new System.Collections.Concurrent.ConcurrentDictionary<Type, IShardingConfig>();

//        public static IShardingConfig Get(Type entityType)
//        {
//            return InstanceCache[entityType];
//        }

//        public static IShardingConfig Find(Type entityType)
//        {
//            InstanceCache.TryGetValue(entityType, out var value);
//            return value;
//        }

//        public static void Add(IShardingConfig config)
//        {
//            InstanceCache[config.EntityType] = config;
//        }
//    }
//}
