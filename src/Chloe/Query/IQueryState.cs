using Chloe.QueryExpressions;

namespace Chloe.Query
{
    interface IQueryState
    {
        IQueryState Accept(WhereExpression exp);
        IQueryState Accept(OrderExpression exp);
        IQueryState Accept(SelectExpression exp);
        IQueryState Accept(SkipExpression exp);
        IQueryState Accept(TakeExpression exp);
        IQueryState Accept(AggregateQueryExpression exp);
        IQueryState Accept(GroupingQueryExpression exp);
        IQueryState Accept(DistinctExpression exp);
        IQueryState Accept(IncludeExpression exp);
        IQueryState Accept(IgnoreAllFiltersExpression exp);
        IQueryState Accept(TrackingExpression exp);
        IQueryState Accept(PagingExpression exp);
        IQueryState Accept(JoinQueryExpression exp);
    }
}
