using Chloe.Query.Visitors;

namespace Chloe.QueryExpressions
{
    public interface IQueryExpressionVisitor<T>
    {
        T VisitRootQuery(RootQueryExpression exp);

        T VisitWhere(WhereExpression exp);

        T VisitSelect(SelectExpression exp);

        T VisitTake(TakeExpression exp);

        T VisitSkip(SkipExpression exp);

        T VisitOrder(OrderExpression exp);

        T VisitAggregateQuery(AggregateQueryExpression exp);

        T VisitJoinQuery(JoinQueryExpression exp);

        T VisitGroupingQuery(GroupingQueryExpression exp);

        T VisitDistinct(DistinctExpression exp);

        T VisitInclude(IncludeExpression exp);

        T VisitBindTwoWay(BindTwoWayExpression exp);

        T VisitExclude(ExcludeExpression exp);

        T VisitIgnoreAllFilters(IgnoreAllFiltersExpression exp);

        T VisitTracking(TrackingExpression exp);

        T VisitPaging(PagingExpression exp);

        T VisitSplitQuery(SplitQueryExpression exp);
    }

    public abstract class QueryExpressionVisitor<T> : IQueryExpressionVisitor<T>
    {
        public virtual T VisitRootQuery(RootQueryExpression exp)
        {
            throw new NotImplementedException();
        }
        public virtual T VisitWhere(WhereExpression exp)
        {
            throw new NotImplementedException();
        }
        public virtual T VisitSelect(SelectExpression exp)
        {
            throw new NotImplementedException();
        }
        public virtual T VisitTake(TakeExpression exp)
        {
            throw new NotImplementedException();
        }
        public virtual T VisitSkip(SkipExpression exp)
        {
            throw new NotImplementedException();
        }
        public virtual T VisitOrder(OrderExpression exp)
        {
            throw new NotImplementedException();
        }
        public virtual T VisitAggregateQuery(AggregateQueryExpression exp)
        {
            throw new NotImplementedException();
        }
        public virtual T VisitJoinQuery(JoinQueryExpression exp)
        {
            throw new NotImplementedException();
        }
        public virtual T VisitGroupingQuery(GroupingQueryExpression exp)
        {
            throw new NotImplementedException();
        }
        public virtual T VisitDistinct(DistinctExpression exp)
        {
            throw new NotImplementedException();
        }

        public virtual T VisitInclude(IncludeExpression exp)
        {
            throw new NotImplementedException();
        }

        public virtual T VisitExclude(ExcludeExpression exp)
        {
            throw new NotImplementedException();
        }

        public virtual T VisitBindTwoWay(BindTwoWayExpression exp)
        {
            throw new NotImplementedException();
        }

        public virtual T VisitIgnoreAllFilters(IgnoreAllFiltersExpression exp)
        {
            throw new NotImplementedException();
        }

        public virtual T VisitTracking(TrackingExpression exp)
        {
            throw new NotImplementedException();
        }

        public virtual T VisitPaging(PagingExpression exp)
        {
            throw new NotImplementedException();
        }

        public virtual T VisitSplitQuery(SplitQueryExpression exp)
        {
            throw new NotImplementedException();
        }

    }
}
