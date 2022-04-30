using System.Reflection;

namespace Chloe.Sharding
{
    public interface IShardingRoute
    {
        IEnumerable<Chloe.Routing.RouteTable> GetTables();
        Chloe.Routing.IRoutingStrategy GetStrategy(MemberInfo member);

        SortResultFacade SortTables(List<Chloe.Routing.RouteTable> tables, List<Ordering> orderings);
    }

    //internal enum ShardingOperator
    //{
    //    Equal,
    //    NotEqual,
    //    GreaterThan,
    //    GreaterThanOrEqual,
    //    LessThan,
    //    LessThanOrEqual,
    //}

    public class SortResultFacade
    {
        public bool IsOrdered { get; set; }
        public List<Chloe.Routing.RouteTable> Tables { get; set; }
    }
}
