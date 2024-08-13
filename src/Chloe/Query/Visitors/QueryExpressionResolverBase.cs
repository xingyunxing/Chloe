using Chloe.QueryExpressions;

namespace Chloe.Query.Visitors
{
    class QueryExpressionResolverBase : QueryExpressionVisitor<IQueryState>
    {
        public override IQueryState VisitWhere(WhereExpression exp)
        {
            IQueryState prevState = exp.PrevExpression.Accept(this);
            IQueryState state = prevState.Accept(exp);
            return state;
        }

        public override IQueryState VisitOrder(OrderExpression exp)
        {
            IQueryState prevState = exp.PrevExpression.Accept(this);
            IQueryState state = prevState.Accept(exp);
            return state;
        }

        public override IQueryState VisitSelect(SelectExpression exp)
        {
            IQueryState prevState = exp.PrevExpression.Accept(this);
            IQueryState state = prevState.Accept(exp);
            return state;
        }

        public override IQueryState VisitSkip(SkipExpression exp)
        {
            IQueryState prevState = exp.PrevExpression.Accept(this);
            IQueryState state = prevState.Accept(exp);
            return state;
        }

        public override IQueryState VisitTake(TakeExpression exp)
        {
            IQueryState prevState = exp.PrevExpression.Accept(this);
            IQueryState state = prevState.Accept(exp);
            return state;
        }

        public override IQueryState VisitAggregateQuery(AggregateQueryExpression exp)
        {
            IQueryState prevState = exp.PrevExpression.Accept(this);
            IQueryState state = prevState.Accept(exp);
            return state;
        }

        public override IQueryState VisitJoinQuery(JoinQueryExpression exp)
        {
            IQueryState prevState = exp.PrevExpression.Accept(this);
            IQueryState state = prevState.Accept(exp);
            return state;
        }

        public override IQueryState VisitGroupingQuery(GroupingQueryExpression exp)
        {
            IQueryState prevState = exp.PrevExpression.Accept(this);
            IQueryState state = prevState.Accept(exp);
            return state;
        }

        public override IQueryState VisitDistinct(DistinctExpression exp)
        {
            IQueryState prevState = exp.PrevExpression.Accept(this);
            IQueryState state = prevState.Accept(exp);
            return state;
        }

        public override IQueryState VisitInclude(IncludeExpression exp)
        {
            IQueryState prevState = exp.PrevExpression.Accept(this);
            IQueryState state = prevState.Accept(exp);
            return state;
        }

        public override IQueryState VisitBindTwoWay(BindTwoWayExpression exp)
        {
            IQueryState prevState = exp.PrevExpression.Accept(this);
            IQueryState state = prevState.Accept(exp);
            return state;
        }

        public override IQueryState VisitExclude(ExcludeExpression exp)
        {
            IQueryState prevState = exp.PrevExpression.Accept(this);
            IQueryState state = prevState.Accept(exp);
            return state;
        }

        public override IQueryState VisitIgnoreAllFilters(IgnoreAllFiltersExpression exp)
        {
            IQueryState prevState = exp.PrevExpression.Accept(this);
            IQueryState state = prevState.Accept(exp);
            return state;
        }

        public override IQueryState VisitTracking(TrackingExpression exp)
        {
            IQueryState prevState = exp.PrevExpression.Accept(this);
            IQueryState state = prevState.Accept(exp);
            return state;
        }

        public override IQueryState VisitPaging(PagingExpression exp)
        {
            IQueryState prevState = exp.PrevExpression.Accept(this);
            IQueryState state = prevState.Accept(exp);
            return state;
        }

    }
}
