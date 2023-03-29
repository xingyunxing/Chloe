using System.Reflection;

namespace Chloe.Sharding.Routing
{
    /// <summary>
    /// 分片路由
    /// </summary>
    public interface IShardingRoute
    {
        /// <summary>
        /// 获取所有的分片表
        /// </summary>
        /// <returns></returns>
        IEnumerable<RouteTable> GetTables();
        /// <summary>
        /// 根据实体属性获取相应的路由规则，如果传入的 member 没有路由规则，返回 null 即可。
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        IRoutingStrategy GetStrategy(MemberInfo member);

        /// <summary>
        /// 根据排序字段对路由表重排。
        /// </summary>
        /// <param name="tables"></param>
        /// <param name="orderings"></param>
        /// <returns></returns>
        SortResult SortTables(List<RouteTable> tables, List<Ordering> orderings);
    }

    /// <summary>
    /// 条件关系运算符
    /// </summary>
    public enum ShardingOperator
    {
        Equal,
        NotEqual,
        GreaterThan,
        GreaterThanOrEqual,
        LessThan,
        LessThanOrEqual,
    }

    /// <summary>
    /// 重排结果。
    /// </summary>
    public class SortResult
    {
        /// <summary>
        /// 表示路由表是否为有序的。如果是有序的路由表，在分页查询上会有优化，可减少不必要的查询。
        /// </summary>
        public bool IsOrdered { get; set; }
        /// <summary>
        /// 重排后的路由表
        /// </summary>
        public List<RouteTable> Tables { get; set; }
    }
}
