using Chloe.Descriptors;
using Chloe.Exceptions;
using System.Reflection;

namespace Chloe.Sharding
{
    internal interface IShardingContext
    {
        TypeDescriptor TypeDescriptor { get; }
        ShardingDbContext DbContext { get; }
        IShardingConfig ShardingConfig { get; }
        IShardingRoute Route { get; }
        int MaxInItems { get; }
        bool IsPrimaryKey(MemberInfo member);
        bool IsShardingMember(MemberInfo member);
        bool IsUniqueIndex(MemberInfo member);
        List<IDbContext> CreateDbContextProviders(IPhysicDataSource dataSource, int desiredCount);
    }

    class ShardingContext : IShardingContext
    {
        public ShardingContext(ShardingDbContext dbContext, IShardingConfig shardingConfig, TypeDescriptor typeDescriptor)
        {
            this.DbContext = dbContext;
            this.TypeDescriptor = typeDescriptor;
            this.ShardingConfig = shardingConfig;
            this.Route = shardingConfig.RouteFactory.CreateRoute(dbContext);
        }

        public TypeDescriptor TypeDescriptor { get; set; }
        public ShardingDbContext DbContext { get; set; }
        public IShardingRoute Route { get; private set; }

        public int MaxInItems { get { return this.DbContext.Options.MaxInItems; } }

        public IShardingConfig ShardingConfig { get; private set; }

        public bool IsShardingMember(MemberInfo member)
        {
            return this.ShardingConfig.ShardingKey == member;
        }

        public bool IsPrimaryKey(MemberInfo member)
        {
            return this.TypeDescriptor.IsPrimaryKey(member);
        }

        public bool IsUniqueIndex(MemberInfo member)
        {
            return this.TypeDescriptor.IsUniqueIndex(member);
        }

        public List<IDbContext> CreateDbContextProviders(IPhysicDataSource dataSource, int desiredCount)
        {
            return this.DbContext.CreateDbContextProviders(dataSource, desiredCount);
        }
    }

    static class ShardingContextExtension
    {
        public static RouteTable GetEntityTable(this IShardingContext shardingContext, object entity, bool throwExceptionIfNotFound = false)
        {
            var shardingPropertyDescriptor = shardingContext.TypeDescriptor.GetPrimitivePropertyDescriptor(shardingContext.ShardingConfig.ShardingKey);

            var shardingKeyValue = shardingPropertyDescriptor.GetValue(entity);

            RouteTable routeTable = shardingContext.GetTable(shardingKeyValue, throwExceptionIfNotFound);
            return routeTable;
        }
        public static IEnumerable<RouteTable> GetTables(this IShardingContext shardingContext)
        {
            return shardingContext.Route.GetTables();
        }

        public static RouteTable GetTable(this IShardingContext shardingContext, object shardingValue, bool throwExceptionIfNotFound = false)
        {
            IShardingStrategy shardingStrategy = shardingContext.Route.GetShardingStrategy(shardingContext.ShardingConfig.ShardingKey);

            RouteTable routeTable = null;

            if (shardingStrategy == null)
            {
                routeTable = null;
            }
            else
            {
                routeTable = shardingStrategy.ForEqual(shardingValue).FirstOrDefault();
            }

            if (routeTable == null && throwExceptionIfNotFound)
            {
                throw new ChloeException($"Corresponding table not found for sharding key '{shardingValue}'.");
            }

            return routeTable;
        }
        public static IEnumerable<RouteTable> GetTablesByKey(this IShardingContext shardingContext, object keyValue)
        {
            IShardingStrategy shardingStrategy = shardingContext.Route.GetShardingStrategy(shardingContext.TypeDescriptor.PrimaryKeys.First().Definition.Property);

            if (shardingStrategy == null)
            {
                return shardingContext.Route.GetTables();
            }

            return shardingStrategy.ForEqual(keyValue);
        }
        public static SortResult SortTables(this IShardingContext shardingContext, List<RouteTable> tables, List<Ordering> orderings)
        {
            return shardingContext.Route.SortTables(tables, orderings);
        }
    }
}
