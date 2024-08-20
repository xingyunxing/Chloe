
namespace Chloe.QueryExpressions
{
    public enum QueryExpressionType
    {
        Root = 1,
        Where,
        Take,
        Skip,
        Paging,
        OrderBy,
        OrderByDesc,
        ThenBy,
        ThenByDesc,
        Select,
        /// <summary>
        /// Include navigation property query.
        /// </summary>
        Include,
        /// <summary>
        /// 导航查询时，子对象将父对象设置到子对象对应的属性上
        /// </summary>
        BindTwoWay,
        /// <summary>
        /// Exclude field query.
        /// </summary>
        Exclude,
        Aggregate,
        JoinQuery,
        GroupingQuery,
        Distinct,
        IgnoreAllFilters,
        Tracking,
        SplitQuery
    }
}
