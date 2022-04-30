using Chloe.Sharding;

namespace Chloe.Routing
{
    public class RouteDataSource
    {
        public string Name { get; set; }
        public IDbContextProviderFactory DbContextProviderFactory { get; set; }
    }
}
