
using Chloe.Query;
using Chloe.QueryExpressions;

namespace Chloe.Sharding.QueryState
{
    class ShardingSelectQueryState : ShardingQueryStateBase
    {
        public ShardingSelectQueryState(ShardingQueryStateBase prevQueryState, SelectExpression exp) : base(prevQueryState)
        {
            this.QueryModel.Selector = exp.Selector;
        }

        public override IQueryState Accept(SkipExpression exp)
        {
            return new ShardingSkipQueryState(this, exp);
        }

        public override IQueryState Accept(TakeExpression exp)
        {
            return new ShardingTakeQueryState(this, exp);
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
