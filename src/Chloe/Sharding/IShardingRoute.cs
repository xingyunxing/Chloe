using System.Reflection;

namespace Chloe.Sharding
{
    public interface IShardingRoute
    {
        IEnumerable<RouteTable> GetTables();
        IShardingStrategy GetShardingStrategy(MemberInfo member);

        SortResult SortTables(List<RouteTable> tables, List<Ordering> orderings);
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
