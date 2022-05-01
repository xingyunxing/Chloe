using Chloe.Sharding.Routing;

namespace Chloe
{
    public interface IPhysicDataSource
    {
        string Name { get; }
        IDbContextProviderFactory DbContextProviderFactory { get; }
    }

    class PhysicDataSource : IPhysicDataSource
    {
        public PhysicDataSource(string name, IDbContextProviderFactory dbContextProviderFactory)
        {
            this.Name = name;
            this.DbContextProviderFactory = dbContextProviderFactory;
        }
        public PhysicDataSource(RouteDataSource routeDataSource)
        {
            this.Name = routeDataSource.Name;
            this.DbContextProviderFactory = routeDataSource.DbContextProviderFactory;
        }

        public string Name { get; set; }
        public IDbContextProviderFactory DbContextProviderFactory { get; set; }
    }
}
