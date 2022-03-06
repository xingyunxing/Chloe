using Chloe.Descriptors;
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
        List<IDbContext> CreateDbContextProviders(IPhysicDataSource dataSource, int count);
    }

    class ShardingContext : IShardingContext
    {
        public ShardingContext(ShardingDbContext dbContext, IShardingConfig shardingConfig, TypeDescriptor typeDescriptor)
        {
            this.DbContext = dbContext;
            this.TypeDescriptor = typeDescriptor;
            this.ShardingConfig = shardingConfig;
            this.Route = shardingConfig.RouteFactory.CreateRoute();
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

        public List<IDbContext> CreateDbContextProviders(IPhysicDataSource dataSource, int count)
        {
            return this.DbContext.CreateDbContextProviders(dataSource, count);
        }
    }

    static class ShardingContextExtension
    {
        public static RouteTable GetEntityTable(this IShardingContext shardingContext, object entity)
        {
            var shardingPropertyDescriptor = shardingContext.TypeDescriptor.GetPrimitivePropertyDescriptor(shardingContext.ShardingConfig.ShardingKey);

            var shardingKeyValue = shardingPropertyDescriptor.GetValue(entity);

            RouteTable routeTable = shardingContext.GetTable(shardingKeyValue);

            return routeTable;
        }
        public static List<RouteTable> GetTables(this IShardingContext shardingContext)
        {
            return shardingContext.Route.GetTables(shardingContext.DbContext);
        }
        public static List<RouteTable> GetTables(this IShardingContext shardingContext, object shardingValue, ShardingOperator shardingOperator)
        {
            return shardingContext.Route.GetTables(shardingContext.DbContext, shardingValue, shardingOperator);
        }
        public static RouteTable GetTable(this IShardingContext shardingContext, object shardingValue)
        {
            return shardingContext.Route.GetTable(shardingContext.DbContext, shardingValue);
        }
        public static List<RouteTable> GetTablesByKey(this IShardingContext shardingContext, object keyValue)
        {
            return shardingContext.Route.GetTablesByKey(shardingContext.DbContext, keyValue);
        }
        public static SortResult SortTables(this IShardingContext shardingContext, List<RouteTable> tables, List<Ordering> orderings)
        {
            return shardingContext.Route.SortTables(shardingContext.DbContext, tables, orderings);
        }
    }
}
