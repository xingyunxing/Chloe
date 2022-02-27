using Chloe.Descriptors;
using System.Reflection;

namespace Chloe.Sharding
{
    internal interface IShardingContext
    {
        TypeDescriptor TypeDescriptor { get; }
        ShardingDbContext DbContext { get; }
        IShardingRoute Route { get; }
        int MaxConnectionsPerDatabase { get; }
        int MaxInItems { get; }
        bool IsPrimaryKey(MemberInfo member);
        bool IsShardingMember(MemberInfo member);
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

        public int MaxConnectionsPerDatabase { get { return this.DbContext.Options.MaxConnectionsPerDatabase; } }
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
    }

    static class ShardingContextExtension
    {
        //public static bool IsShardingMember(this IShardingContext shardingContext, MemberInfo member)
        //{
        //    return shardingContext.Route.is
        //}
        public static List<PhysicTable> GetPhysicTables(this IShardingContext shardingContext)
        {
            return shardingContext.Route.GetPhysicTables(shardingContext.DbContext);
        }
        public static List<PhysicTable> GetPhysicTables(this IShardingContext shardingContext, object shardingValue, ShardingOperator shardingOperator)
        {
            return shardingContext.Route.GetPhysicTables(shardingContext.DbContext, shardingValue, shardingOperator);
        }
        public static PhysicTable GetPhysicTable(this IShardingContext shardingContext, object shardingValue)
        {
            return shardingContext.Route.GetPhysicTable(shardingContext.DbContext, shardingValue);
        }
        public static List<PhysicTable> GetPhysicTableByKey(this IShardingContext shardingContext, object keyValue)
        {
            return shardingContext.Route.GetPhysicTableByKey(shardingContext.DbContext, keyValue);
        }
        public static SortResult SortTables(this IShardingContext shardingContext, List<PhysicTable> physicTables, List<Ordering> orderings)
        {
            return shardingContext.Route.SortTables(shardingContext.DbContext, physicTables, orderings);
        }
    }

    //public interface PhysicTable
    //{
    //    string Name { get; set; }
    //    string Schema { get; set; }
    //    //string Database { get; set; }
    //    DataSource DataSource { get; set; }
    //}

    public class PhysicTable
    {
        public string Name { get; set; }
        public string Schema { get; set; }
        public DataSource DataSource { get; set; }
    }

    public class DataSource
    {
        public string Name { get; set; }
        public IPhysicDbContextFactory DbContextFactory { get; set; }
    }

    public enum ShardingOperator
    {
        Equal,
        NotEqual,
        GreaterThan,
        GreaterThanOrEqual,
        LessThan,
        LessThanOrEqual,
    }

    public class SortResult
    {
        public bool IsOrdered { get; set; }
        public List<PhysicTable> Tables { get; set; }
    }
}
