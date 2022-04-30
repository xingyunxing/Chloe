using Chloe.Descriptors;
using Chloe.Exceptions;
using Chloe.Routing;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

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
        List<IDbContextProvider> CreateDbContextProviders(Chloe.Routing.IPhysicDataSource dataSource, int desiredCount);
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

        public int MaxInItems { get { return this.DbContextProvider.Options.MaxInItems; } }

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

        public List<IDbContextProvider> CreateDbContextProviders(Chloe.Routing.IPhysicDataSource dataSource, int desiredCount)
        {
            return this.DbContextProvider.CreateDbContextProviders(dataSource, desiredCount);
        }
    }

    static class ShardingContextExtensionFacade
    {
        public static Chloe.Routing.RouteTable GetEntityTable(this IShardingContext shardingContext, object entity, bool throwExceptionIfNotFound = false)
        {
            var shardingPropertyDescriptor = shardingContext.TypeDescriptor.GetPrimitivePropertyDescriptor(shardingContext.ShardingConfig.ShardingKey);

            var shardingKeyValue = shardingPropertyDescriptor.GetValue(entity);

            Chloe.Routing.RouteTable routeTable = shardingContext.GetTable(shardingKeyValue, throwExceptionIfNotFound);
            return routeTable;
        }
        public static IEnumerable<Chloe.Routing.RouteTable> GetTables(this IShardingContext shardingContext)
        {
            return shardingContext.Route.GetTables();
        }

        public static Chloe.Routing.RouteTable GetTable(this IShardingContext shardingContext, object shardingValue, bool throwExceptionIfNotFound = false)
        {
            Chloe.Routing.IRoutingStrategy routingStrategy = shardingContext.Route.GetStrategy(shardingContext.ShardingConfig.ShardingKey);

            Chloe.Routing.RouteTable routeTable = null;

            if (routingStrategy == null)
            {
                routeTable = null;
            }
            else
            {
                routeTable = routingStrategy.ForEqual(shardingValue).FirstOrDefault();
            }

            if (routeTable == null && throwExceptionIfNotFound)
            {
                throw new ChloeException($"Corresponding table not found for sharding key '{shardingValue}'.");
            }

            return routeTable;
        }
        public static IEnumerable<Chloe.Routing.RouteTable> GetTablesByKey(this IShardingContext shardingContext, object keyValue)
        {
            Chloe.Routing.IRoutingStrategy routingStrategy = shardingContext.Route.GetStrategy(shardingContext.TypeDescriptor.PrimaryKeys.First().Definition.Property);

            if (routingStrategy == null)
            {
                return shardingContext.Route.GetTables();
            }

            return routingStrategy.ForEqual(keyValue);
        }
        public static SortResultFacade SortTables(this IShardingContext shardingContext, List<Chloe.Routing.RouteTable> tables, List<Ordering> orderings)
        {
            return shardingContext.Route.SortTables(tables, orderings);
        }
    }
}
