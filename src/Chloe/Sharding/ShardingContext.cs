using Chloe.Descriptors;
using System.Reflection;

namespace Chloe.Sharding
{
    internal interface IShardingContext
    {
        TypeDescriptor TypeDescriptor { get; }
        ShardingDbContext DbContext { get; }
        IShardingRoute Route { get; }
        int MaxInItems { get; }
        bool IsPrimaryKey(MemberInfo member);
        bool IsShardingMember(MemberInfo member);
        List<IDbContext> CreateDbContexts(IPhysicDataSource dataSource, int count);
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

        public List<IDbContext> CreateDbContexts(IPhysicDataSource dataSource, int count)
        {
            if (this.DbContext.DbSessionProvider.IsInTransaction)
            {
                return this.CreateTransactionDbContexts(dataSource);
            }

            return this.CreateNonTransactionDbContexts(dataSource, count);
        }
        List<IDbContext> CreateTransactionDbContexts(IPhysicDataSource dataSource)
        {
            var routeDbContextFactory = (dataSource as PhysicDataSource).DataSource.DbContextFactory;

            DataSourceDbContextPair pair = this.DbContext.DbSessionProvider.HoldDbContexts.FirstOrDefault(a => a.DataSource.Name == dataSource.Name);
            if (pair == null)
            {
                IDbContext dbContext = routeDbContextFactory.CreateDbContext();
                this.AppendFutures(dbContext);

                dbContext.Session.BeginTransaction(this.DbContext.DbSessionProvider.IL);

                this.DbContext.DbSessionProvider.HoldDbContexts.Add(new DataSourceDbContextPair(dataSource, dbContext));
            }

            return new List<IDbContext>(1) { pair.DbContext };
        }
        List<IDbContext> CreateNonTransactionDbContexts(IPhysicDataSource dataSource, int count)
        {
            var routeDbContextFactory = (dataSource as PhysicDataSource).DataSource.DbContextFactory;

            int connectionCount = Math.Min(count, this.DbContext.Options.MaxConnectionsPerDataSource);

            List<IDbContext> dbContexts = new List<IDbContext>(connectionCount);

            for (int i = 0; i < connectionCount; i++)
            {
                IDbContext dbContext = routeDbContextFactory.CreateDbContext();
                this.AppendFutures(dbContext);

                dbContexts.Add(dbContext);
            }

            return dbContexts;
        }

        void AppendFutures(IDbContext dbContext)
        {
            dbContext.Session.CommandTimeout = this.DbContext.Session.CommandTimeout;
            this.AppendQueryFilters(dbContext);
            this.AppendSessionInterceptors(dbContext);
        }
        void AppendQueryFilters(IDbContext dbContext)
        {
            foreach (var kv in (this.DbContext as IDbContextInternal).QueryFilters)
            {
                foreach (var filter in kv.Value)
                {
                    dbContext.HasQueryFilter(kv.Key, filter);
                }
            }
        }
        void AppendSessionInterceptors(IDbContext dbContext)
        {
            foreach (var interceptor in this.DbContext.DbSessionProvider.SessionInterceptors)
            {
                dbContext.Session.AddInterceptor(interceptor);
            }
        }
    }

    static class ShardingContextExtension
    {
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
