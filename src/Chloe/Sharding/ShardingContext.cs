using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Chloe.Sharding
{
    internal interface IShardingContext
    {
        ShardingDbContext DbContext { get; }
        IShardingRoute Route { get; }
        bool IsShardingMember(MemberInfo member);
    }

    class ShardingContext : IShardingContext
    {
        public ShardingContext(ShardingDbContext dbContext, IShardingConfig shardingConfig)
        {
            this.DbContext = dbContext;
            this.ShardingConfig = shardingConfig;
            this.Route = shardingConfig.RouteFactory.CreateRoute();
        }

        public ShardingDbContext DbContext { get; set; }

        public IShardingRoute Route { get; private set; }

        public IShardingConfig ShardingConfig { get; private set; }

        public bool IsShardingMember(MemberInfo member)
        {
            return this.ShardingConfig.ShardingKey == member;
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
        public bool IsSorted { get; set; }
        public List<PhysicTable> Tables { get; set; }
    }
}
