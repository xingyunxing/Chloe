using Chloe.Descriptors;
using Chloe.Infrastructure;

namespace Chloe.Sharding
{
    public partial class ShardingDbContext
    {
        Dictionary<Type, IShardingContext> _shardingContextMap = new Dictionary<Type, IShardingContext>();

        internal IShardingContext CreateShardingContext(Type entityType)
        {
            IShardingContext shardingContext = this._shardingContextMap.FindValue(entityType);
            if (shardingContext == null)
            {
                TypeDescriptor typeDescriptor = EntityTypeContainer.GetDescriptor(entityType);
                IShardingConfig shardingConfig = ShardingConfigContainer.Get(entityType);
                shardingContext = new ShardingContext(this, shardingConfig, typeDescriptor);

                this._shardingContextMap.Add(entityType, shardingContext);
            }

            return shardingContext;
        }

        internal List<IDbContext> CreateDbContextProviders(IPhysicDataSource dataSource, int count)
        {
            if (this.DbSessionProvider.IsInTransaction)
            {
                return new List<IDbContext>(1) { this.GetPersistedDbContextProvider(dataSource) };
            }

            return this.CreateTransientDbContextProviders(dataSource, count);
        }
        internal IDbContext GetPersistedDbContextProvider(RouteTable routeTable)
        {
            return this.GetPersistedDbContextProvider(new PhysicDataSource(routeTable.DataSource));
        }
        internal IDbContext GetPersistedDbContextProvider(IPhysicDataSource dataSource)
        {
            var routeDbContextFactory = (dataSource as PhysicDataSource).DataSource.DbContextFactory;

            DataSourceDbContextPair pair = this.DbSessionProvider.PersistedDbContexts.FirstOrDefault(a => a.DataSource.Name == dataSource.Name);
            if (pair == null)
            {
                IDbContext dbContext = routeDbContextFactory.CreateDbContext();
                this.AppendFeatures(dbContext);

                pair = new DataSourceDbContextPair(dataSource, dbContext);

                this.DbSessionProvider.PersistedDbContexts.Add(pair);
            }

            if (pair.DbContext.Session.IsInTransaction)
            {
                pair.DbContext.Session.BeginTransaction(this.DbSessionProvider.IL);
            }

            return new PersistedDbContext(pair.DbContext);
        }
        internal List<IDbContext> CreateTransientDbContextProviders(IPhysicDataSource dataSource, int count)
        {
            var routeDbContextFactory = (dataSource as PhysicDataSource).DataSource.DbContextFactory;

            int connectionCount = Math.Min(count, this.Options.MaxConnectionsPerDataSource);

            List<IDbContext> dbContexts = new List<IDbContext>(connectionCount);

            for (int i = 0; i < connectionCount; i++)
            {
                IDbContext dbContext = routeDbContextFactory.CreateDbContext();
                this.AppendFeatures(dbContext);

                dbContexts.Add(dbContext);
            }

            return dbContexts;
        }

        void AppendFeatures(IDbContext dbContext)
        {
            dbContext.Session.CommandTimeout = this.Session.CommandTimeout;
            this.AppendQueryFilters(dbContext);
            this.AppendSessionInterceptors(dbContext);
        }
        void AppendQueryFilters(IDbContext dbContext)
        {
            foreach (var kv in (this as IDbContextInternal).QueryFilters)
            {
                foreach (var filter in kv.Value)
                {
                    dbContext.HasQueryFilter(kv.Key, filter);
                }
            }
        }
        void AppendSessionInterceptors(IDbContext dbContext)
        {
            foreach (var interceptor in this.DbSessionProvider.SessionInterceptors)
            {
                dbContext.Session.AddInterceptor(interceptor);
            }
        }

        class PersistedDbContext : DbContextDecorator, IDbContext
        {
            public PersistedDbContext(IDbContext dbContext) : base(dbContext)
            {
            }

            protected override void Dispose(bool disposing)
            {

            }
        }
    }
}
