namespace Chloe.Sharding
{
    public class RouteDataSource
    {
        public string Name { get; set; }
        public IRouteDbContextFactory DbContextFactory { get; set; }
    }
}
