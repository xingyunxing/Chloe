using Chloe.Query.Visitors;

namespace Chloe.QueryExpressions
{
    public interface IQueryExpressionVisitor<T>
    {
        T Visit(RootQueryExpression exp);

        T Visit(WhereExpression exp);

        T Visit(SelectExpression exp);

        T Visit(TakeExpression exp);

        T Visit(SkipExpression exp);

        T Visit(OrderExpression exp);

        T Visit(AggregateQueryExpression exp);

        T Visit(JoinQueryExpression exp);

        T Visit(GroupingQueryExpression exp);

        T Visit(DistinctExpression exp);

        T Visit(IncludeExpression exp);

        T Visit(BindTwoWayExpression exp);

        T Visit(ExcludeExpression exp);

        T Visit(IgnoreAllFiltersExpression exp);

        T Visit(TrackingExpression exp);

        T Visit(PagingExpression exp);
    }

    public abstract class QueryExpressionVisitor<T> : IQueryExpressionVisitor<T>
    {
        public virtual T Visit(RootQueryExpression exp)
        {
            throw new NotImplementedException();
        }
        public virtual T Visit(WhereExpression exp)
        {
            throw new NotImplementedException();
        }
        public virtual T Visit(SelectExpression exp)
        {
            throw new NotImplementedException();
        }
        public virtual T Visit(TakeExpression exp)
        {
            throw new NotImplementedException();
        }
        public virtual T Visit(SkipExpression exp)
        {
            throw new NotImplementedException();
        }
        public virtual T Visit(OrderExpression exp)
        {
            throw new NotImplementedException();
        }
        public virtual T Visit(AggregateQueryExpression exp)
        {
            throw new NotImplementedException();
        }
        public virtual T Visit(JoinQueryExpression exp)
        {
            throw new NotImplementedException();
        }
        public virtual T Visit(GroupingQueryExpression exp)
        {
            throw new NotImplementedException();
        }
        public virtual T Visit(DistinctExpression exp)
        {
            throw new NotImplementedException();
        }

        public virtual T Visit(IncludeExpression exp)
        {
            throw new NotImplementedException();
        }
        public virtual T Visit(BindTwoWayExpression exp)
        {
            throw new NotImplementedException();
        }

        public virtual T Visit(ExcludeExpression exp)
        {
            throw new NotImplementedException();
        }

        public virtual T Visit(IgnoreAllFiltersExpression exp)
        {
            throw new NotImplementedException();
        }

        public virtual T Visit(TrackingExpression exp)
        {
            throw new NotImplementedException();
        }

        public virtual T Visit(PagingExpression exp)
        {
            throw new NotImplementedException();
        }

    }
}
