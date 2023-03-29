using Chloe.Descriptors;
using Chloe.Exceptions;
using Chloe.Sharding.Routing;
using System.Reflection;

namespace Chloe.Sharding
{
    internal interface IShardingContext
    {
        TypeDescriptor TypeDescriptor { get; }
        ShardingDbContextProvider DbContextProvider { get; }
        IShardingConfig ShardingConfig { get; }
        IShardingRoute Route { get; }
        int MaxInItems { get; }
        bool IsPrimaryKey(MemberInfo member);
        bool IsShardingMember(MemberInfo member);
        bool IsUniqueIndex(MemberInfo member);
        ISharedDbContextProviderPool GetDbContextProviderPool(IPhysicDataSource dataSource);
    }

    class ShardingContextFacade : IShardingContext
    {
        public ShardingContextFacade(ShardingDbContextProvider dbContextProvider, IShardingConfig shardingConfig, TypeDescriptor typeDescriptor)
        {
            this.DbContextProvider = dbContextProvider;
            this.TypeDescriptor = typeDescriptor;
            this.ShardingConfig = shardingConfig;
            this.Route = shardingConfig.RouteFactory.CreateRoute(dbContextProvider);
        }

        public TypeDescriptor TypeDescriptor { get; set; }
        public ShardingDbContextProvider DbContextProvider { get; set; }
        public IShardingRoute Route { get; private set; }

        public int MaxInItems { get { return this.DbContextProvider.DbContext.ShardingOptions.MaxInItems; } }

        public IShardingConfig ShardingConfig { get; private set; }

        public bool IsShardingMember(MemberInfo member)
        {
            return this.ShardingConfig.ShardingKeys.Contains(member);
        }

        public bool IsPrimaryKey(MemberInfo member)
        {
            return this.TypeDescriptor.IsPrimaryKey(member);
        }

        public bool IsUniqueIndex(MemberInfo member)
        {
            return this.TypeDescriptor.IsUniqueIndex(member);
        }

        public ISharedDbContextProviderPool GetDbContextProviderPool(IPhysicDataSource dataSource)
        {
            return this.DbContextProvider.DbContext.Butler.GetDbContextProviderPool(dataSource);
        }
    }

    static class ShardingContextExtensionFacade
    {
        public static RouteTable GetEntityTable(this IShardingContext shardingContext, object entity)
        {
            List<ShardingKey> shardingKeys = GetEntityShardingKeys(shardingContext, entity);
            RouteTable routeTable = shardingContext.GetTable(shardingKeys);
            return routeTable;
        }
        static List<ShardingKey> GetEntityShardingKeys(this IShardingContext shardingContext, object entity)
        {
            List<ShardingKey> shardingKeys = new List<ShardingKey>(shardingContext.ShardingConfig.ShardingKeys.Count);

            for (int i = 0; i < shardingContext.ShardingConfig.ShardingKeys.Count; i++)
            {
                MemberInfo shardingKeyMember = shardingContext.ShardingConfig.ShardingKeys[i];
                var shardingPropertyDescriptor = shardingContext.TypeDescriptor.GetPrimitivePropertyDescriptor(shardingKeyMember);
                var shardingKeyValue = shardingPropertyDescriptor.GetValue(entity);

                if (shardingKeyValue == null)
                {
                    throw new ArgumentException($"The sharding key '{shardingPropertyDescriptor.Property.Name.ToString()}' value can not be null.");
                }

                ShardingKey shardingKey = new ShardingKey() { Member = shardingKeyMember, Value = shardingKeyValue };
                shardingKeys.Add(shardingKey);
            }

            return shardingKeys;
        }

        public static IEnumerable<RouteTable> GetTables(this IShardingContext shardingContext)
        {
            return shardingContext.Route.GetTables();
        }

        public static RouteTable GetTable(this IShardingContext shardingContext, List<ShardingKey> shardingKeys)
        {
            IEnumerable<RouteTable> routeTables = null;
            for (int i = 0; i < shardingKeys.Count; i++)
            {
                ShardingKey shardingKey = shardingKeys[i];
                IRoutingStrategy routingStrategy = shardingContext.Route.GetStrategy(shardingKey.Member);

                object shardingValue = shardingKey.Value;

                var keyRouteTables = routingStrategy.ForEqual(shardingValue);

                if (routeTables == null)
                {
                    routeTables = keyRouteTables;
                }
                else
                {
                    routeTables = ShardingHelpers.Intersect(routeTables, keyRouteTables);
                }
            }

            RouteTable matchedTable = null;
            foreach (RouteTable routeTable in routeTables)
            {
                if (matchedTable == null)
                {
                    matchedTable = routeTable;
                    continue;
                }

                throw new ChloeException($"There is not only one table matched for entity '{shardingContext.ShardingConfig.EntityType.FullName}'.");
            }

            if (matchedTable == null)
            {
                throw new ChloeException($"There is not table matched for entity '{shardingContext.ShardingConfig.EntityType.FullName}'.");
            }

            return matchedTable;
        }

        public static SortResult SortTables(this IShardingContext shardingContext, List<RouteTable> tables, List<Ordering> orderings)
        {
            return shardingContext.Route.SortTables(tables, orderings);
        }
    }
}
