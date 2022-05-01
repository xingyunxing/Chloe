namespace Chloe.Sharding.Routing
{
    public class RouteDataSource
    {
        public string Name { get; set; }
        public IDbContextProviderFactory DbContextProviderFactory { get; set; }
    }
}
