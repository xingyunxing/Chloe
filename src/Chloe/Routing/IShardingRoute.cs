using System.Reflection;

namespace Chloe.Routing
{
    public interface IShardingRoute
    {
        IEnumerable<RouteTable> GetTables();
        IRoutingStrategy GetStrategy(MemberInfo member);

        SortResult SortTables(List<RouteTable> tables, List<Chloe.Sharding.Ordering> orderings);
    }

    internal enum ShardingOperator
    {
        Equal,
        NotEqual,
        GreaterThan,
        GreaterThanOrEqual,
        LessThan,
        LessThanOrEqual,
    }

    public class SortResult
    {
        public bool IsOrdered { get; set; }
        public List<RouteTable> Tables { get; set; }
    }
}
