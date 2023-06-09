using Chloe.QueryExpressions;

namespace Chloe.Query.Visitors
{
    class QueryExpressionVisitor : QueryExpressionVisitor<IQueryState>
    {
        public override IQueryState Visit(WhereExpression exp)
        {
            IQueryState prevState = exp.PrevExpression.Accept(this);
            IQueryState state = prevState.Accept(exp);
            return state;
        }

        public override IQueryState Visit(OrderExpression exp)
        {
            IQueryState prevState = exp.PrevExpression.Accept(this);
            IQueryState state = prevState.Accept(exp);
            return state;
        }

        public override IQueryState Visit(SelectExpression exp)
        {
            IQueryState prevState = exp.PrevExpression.Accept(this);
            IQueryState state = prevState.Accept(exp);
            return state;
        }

        public override IQueryState Visit(SkipExpression exp)
        {
            IQueryState prevState = exp.PrevExpression.Accept(this);
            IQueryState state = prevState.Accept(exp);
            return state;
        }

        public override IQueryState Visit(TakeExpression exp)
        {
            IQueryState prevState = exp.PrevExpression.Accept(this);
            IQueryState state = prevState.Accept(exp);
            return state;
        }

        public override IQueryState Visit(AggregateQueryExpression exp)
        {
            IQueryState prevState = exp.PrevExpression.Accept(this);
            IQueryState state = prevState.Accept(exp);
            return state;
        }

        public override IQueryState Visit(JoinQueryExpression exp)
        {
            IQueryState prevState = exp.PrevExpression.Accept(this);
            IQueryState state = prevState.Accept(exp);
            return state;
        }

        public override IQueryState Visit(GroupingQueryExpression exp)
        {
            IQueryState prevState = exp.PrevExpression.Accept(this);
            IQueryState state = prevState.Accept(exp);
            return state;
        }

        public override IQueryState Visit(DistinctExpression exp)
        {
            IQueryState prevState = exp.PrevExpression.Accept(this);
            IQueryState state = prevState.Accept(exp);
            return state;
        }

        public override IQueryState Visit(IncludeExpression exp)
        {
            IQueryState prevState = exp.PrevExpression.Accept(this);
            IQueryState state = prevState.Accept(exp);
            return state;
        }

        public override IQueryState Visit(ExcludeExpression exp)
        {
            IQueryState prevState = exp.PrevExpression.Accept(this);
            IQueryState state = prevState.Accept(exp);
            return state;
        }

        public override IQueryState Visit(IgnoreAllFiltersExpression exp)
        {
            IQueryState prevState = exp.PrevExpression.Accept(this);
            IQueryState state = prevState.Accept(exp);
            return state;
        }

        public override IQueryState Visit(TrackingExpression exp)
        {
            IQueryState prevState = exp.PrevExpression.Accept(this);
            IQueryState state = prevState.Accept(exp);
            return state;
        }

        public override IQueryState Visit(PagingExpression exp)
        {
            IQueryState prevState = exp.PrevExpression.Accept(this);
            IQueryState state = prevState.Accept(exp);
            return state;
        }
    }
}
