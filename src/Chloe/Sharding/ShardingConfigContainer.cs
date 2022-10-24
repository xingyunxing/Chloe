namespace Chloe.Sharding
{
    public class ShardingConfigContainer
    {
        static readonly Dictionary<Type, IShardingConfig> InstanceCache = new Dictionary<Type, IShardingConfig>();

        public static IShardingConfig Get(Type entityType)
        {
            return InstanceCache[entityType];
        }

        public static IShardingConfig Find(Type entityType)
        {
            if (InstanceCache.Count == 0)
                return null;

            InstanceCache.TryGetValue(entityType, out var value);
            return value;
        }

        public static void Add(IShardingConfig config)
        {
            lock (InstanceCache)
            {
                InstanceCache[config.EntityType] = config;
            }
        }
    }
}
