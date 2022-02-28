namespace Chloe.Sharding
{
    public interface IRouteDbContextFactory
    {
        IDbContext CreateDbContext();
    }

    class RouteDbContextFactoryWrapper : IRouteDbContextFactory
    {
        IRouteDbContextFactory _dbContextFactory;
        ShardingDbContext _shardingDbContext;

        public RouteDbContextFactoryWrapper(IRouteDbContextFactory dbContextFactory, ShardingDbContext shardingDbContext)
        {
            this._dbContextFactory = dbContextFactory;
            this._shardingDbContext = shardingDbContext;
        }

        public IDbContext CreateDbContext()
        {
            var dbContext = this._dbContextFactory.CreateDbContext();
            foreach (var kv in (this._shardingDbContext as IDbContextInternal).QueryFilters)
            {
                foreach (var filter in kv.Value)
                {
                    dbContext.HasQueryFilter(kv.Key, filter);
                }
            }

            return dbContext;
        }
    }
}
