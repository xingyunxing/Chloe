
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
        /// Exclude field query.
        /// </summary>
        Exclude,
        Aggregate,
        JoinQuery,
        GroupingQuery,
        Distinct,
        IgnoreAllFilters,
        Tracking,
    }
}
