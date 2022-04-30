namespace Chloe.Sharding
{
    public interface IShardingRouteFactory
    {
        IShardingRoute CreateRoute(ShardingDbContextProvider shardingDbContextProvider);
    }
    class ShardingRouteFactoryFacade : IShardingRouteFactory
    {
        Func<IShardingRoute> _routeFactory;

        public ShardingRouteFactoryFacade(Func<IShardingRoute> routeFactory)
        {
            this._routeFactory = routeFactory;
        }
        public IShardingRoute CreateRoute(ShardingDbContextProvider shardingDbContextProvider)
        {
            return this._routeFactory();
        }
    }
}
