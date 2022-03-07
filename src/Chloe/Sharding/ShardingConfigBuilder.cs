namespace Chloe.Sharding
{
    public class ShardingConfigContainer
    {
        static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, IShardingConfig> InstanceCache = new System.Collections.Concurrent.ConcurrentDictionary<Type, IShardingConfig>();

        public static IShardingConfig Get(Type entityType)
        {
            return InstanceCache[entityType];
        }

        public static IShardingConfig Find(Type entityType)
        {
            InstanceCache.TryGetValue(entityType, out var value);
            return value;
        }

        public static void Add(IShardingConfig config)
        {
            InstanceCache[config.EntityType] = config;
        }

        //public static void Add<TEntity>(Action<ShardingConfigOption> configureOptions)
        //{
        //    Add(typeof(TEntity), configureOptions);
        //}
        //public static void Add(Type entityType, Action<ShardingConfigOption> configureOptions)
        //{
        //    if (configureOptions == null)
        //        throw new ArgumentNullException(nameof(configureOptions));

        //    ShardingConfigOption option = new ShardingConfigOption();
        //    configureOptions(option);

        //    ShardingConfig shardingConfig = new ShardingConfig();
        //    shardingConfig.EntityType = entityType;
        //    shardingConfig.Route = option.Route;

        //    InstanceCache[shardingConfig.EntityType] = shardingConfig;
        //}
    }
}
