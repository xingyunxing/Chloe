namespace Chloe.Sharding
{
    public interface IPhysicDbContextFactory
    {
        IDbContext CreateDbContext();
    }

    class PhysicDbContextFactoryWrapper : IPhysicDbContextFactory
    {
        IPhysicDbContextFactory _dbContextFactory;
        ShardingDbContext _shardingDbContext;

        public PhysicDbContextFactoryWrapper(IPhysicDbContextFactory dbContextFactory, ShardingDbContext shardingDbContext)
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
