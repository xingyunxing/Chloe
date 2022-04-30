namespace Chloe.Sharding
{
    public class RouteTableEqualityComparer : IEqualityComparer<RouteTable>
    {
        public static readonly RouteTableEqualityComparer Instance = new RouteTableEqualityComparer();

        RouteTableEqualityComparer()
        {

        }

        public bool Equals(RouteTable x, RouteTable y)
        {
            return x.Name == y.Name && x.Schema == y.Schema && x.DataSource.Name == y.DataSource.Name;
        }

        public int GetHashCode(RouteTable obj)
        {
            return $"{obj.Name}_{obj.Schema}_{obj.DataSource.Name}".GetHashCode();
            //return obj.GetHashCode();
        }
    }
}
