using Chloe.Query;
using Chloe.QueryExpressions;
using Chloe.Sharding.Visitors;

namespace Chloe.Sharding.QueryState
{
    internal class ShardingRootQueryState : ShardingQueryStateBase
    {
        public ShardingRootQueryState(ShardingQueryContext queryContext, RootQueryExpression exp) : base(queryContext, CreateQueryModel(exp))
        {

        }

        static ShardingQueryModel CreateQueryModel(RootQueryExpression exp)
        {
            ShardingQueryModel queryModel = new ShardingQueryModel(exp.ElementType);
            queryModel.Lock = exp.Lock;
            return queryModel;
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
            return new ShardingSelectQueryState(this, exp);
        }

        public override IQueryState Accept(SkipExpression exp)
        {
            return new ShardingSkipQueryState(this, exp);
        }

        public override IQueryState Accept(TakeExpression exp)
        {
            return new ShardingTakeQueryState(this, exp);
        }

        public override IQueryState Accept(AggregateQueryExpression exp)
        {
            return new ShardingAggregateQueryState(this, exp);
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

            this.QueryModel.GroupKeySelectors.AppendRange(exp.GroupKeySelectors);
            this.QueryModel.Selector = exp.Selector;

            return new ShardingGroupQueryState(this);
        }

        public override IQueryState Accept(PagingExpression exp)
        {
            return new ShardingPagingQueryState(this, exp);
        }

        public override IFeatureEnumerable<object> CreateQuery()
        {
            return this.CreateNoPagingQuery();
        }
    }
}
