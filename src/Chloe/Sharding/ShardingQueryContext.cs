namespace Chloe.Sharding
{
    class ShardingQueryContext
    {
        public ShardingQueryContext(ShardingDbContext dbContext)
        {
            this.DbContext = dbContext;
        }
        public ShardingDbContext DbContext { get; set; }
    }
}
