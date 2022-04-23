using Chloe.Query.QueryExpressions;
using Chloe.Query.QueryState;

namespace Chloe.Sharding.QueryState
{
    internal class ShardingRootQueryState : ShardingQueryStateBase
    {
        public ShardingRootQueryState(ShardingQueryContext context) : base(context, new ShardingQueryModel())
        {

        }

        public override IQueryState Accept(WhereExpression exp)
        {
            this.QueryModel.Conditions.Add(exp.Predicate);
            return this;
        }

        public override IQueryState Accept(OrderExpression exp)
        {
            if (exp.NodeType == QueryExpressionType.OrderBy || exp.NodeType == QueryExpressionType.OrderByDesc)
            {
                this.QueryModel.Orderings.Clear();
            }

            Ordering ordering = new Ordering();
            ordering.KeySelector = exp.KeySelector;

            var memberExp = exp.KeySelector.Body as System.Linq.Expressions.MemberExpression;
            if (memberExp == null || memberExp.Expression.NodeType != System.Linq.Expressions.ExpressionType.Parameter)
            {
                throw new NotSupportedException(exp.KeySelector.ToString());
            }

            ordering.Member = memberExp.Member;

            if (exp.NodeType == QueryExpressionType.OrderBy || exp.NodeType == QueryExpressionType.ThenBy)
            {
                ordering.Ascending = true;
            }
            else if (exp.NodeType == QueryExpressionType.OrderByDesc || exp.NodeType == QueryExpressionType.ThenByDesc)
            {
                ordering.Ascending = false;
            }

            this.QueryModel.Orderings.Add(ordering);

            return this;
        }

        public override IQueryState Accept(SelectExpression exp)
        {
            this.QueryModel.Selector = exp.Selector;
            return new ShardingGeneralQueryState(this.Context, this.QueryModel);
        }

        public override IQueryState Accept(SkipExpression exp)
        {
            return new ShardingSkipQueryState(this.Context, this.QueryModel, exp.Count);
        }

        public override IQueryState Accept(TakeExpression exp)
        {
            return new ShardingTakeQueryState(this.Context, this.QueryModel, exp.Count);
        }

        public override IQueryState Accept(AggregateQueryExpression exp)
        {
            return new ShardingAggregateQueryState(this.Context, this.QueryModel);
        }

        public override IQueryState Accept(GroupingQueryExpression exp)
        {
            if (exp.HavingPredicates.Count > 0)
            {
                throw new NotSupportedException($"{nameof(IGroupingQuery<object>.Having)}");
            }

            if (exp.Orderings.Count > 0)
            {
                throw new NotSupportedException($"{nameof(IGroupingQuery<object>.OrderBy)} or {nameof(IGroupingQuery<object>.OrderByDesc)}");
            }

            this.QueryModel.GroupKeySelectors.AddRange(exp.GroupKeySelectors);
            this.QueryModel.Selector = exp.Selector;

            return new ShardingGroupQueryState(this.Context, this.QueryModel);
        }
    }
}
