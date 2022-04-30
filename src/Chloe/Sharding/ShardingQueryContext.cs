namespace Chloe.Sharding
{
    class ShardingQueryContext
    {
        public ShardingQueryContext(ShardingDbContextProvider dbContextProvider)
        {
            this.DbContextProvider = dbContextProvider;
        }
        public ShardingDbContextProvider DbContextProvider { get; set; }
    }
}
