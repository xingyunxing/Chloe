namespace Chloe.Routing
{
    public interface IPhysicTable
    {
        string Name { get; }
        string Schema { get; }
        IPhysicDataSource DataSource { get; }
    }

    class PhysicTable : IPhysicTable
    {
        public PhysicTable(RouteTable routeTable)
        {
            this.Table = routeTable;
            this.DataSource = new PhysicDataSource(routeTable.DataSource);
        }

        public RouteTable Table { get; set; }

        public string Name { get { return this.Table.Name; } }
        public string Schema { get { return this.Table.Schema; } }
        public IPhysicDataSource DataSource { get; set; }
    }
}
