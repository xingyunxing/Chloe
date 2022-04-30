namespace Chloe.Routing
{
    public interface IPhysicDataSource
    {
        string Name { get; }
    }

    class PhysicDataSource : IPhysicDataSource
    {
        public PhysicDataSource(RouteDataSource dataSource)
        {
            this.DataSource = dataSource;
        }

        public RouteDataSource DataSource { get; set; }
        public string Name { get { return this.DataSource.Name; } }
    }
}
