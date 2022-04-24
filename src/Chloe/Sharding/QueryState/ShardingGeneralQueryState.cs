
using Chloe.Query.QueryExpressions;
using Chloe.Query.QueryState;

namespace Chloe.Sharding.QueryState
{
    class ShardingGeneralQueryState : ShardingQueryStateBase
    {
        public ShardingGeneralQueryState(ShardingQueryContext context, ShardingQueryModel queryModel) : base(context, queryModel)
        {
        }

        public override IQueryState Accept(SkipExpression exp)
        {
            return new ShardingSkipQueryState(this.Context, this.QueryModel, exp.Count);
        }

        public override IQueryState Accept(TakeExpression exp)
        {
            return new ShardingTakeQueryState(this.Context, this.QueryModel, exp.Count);
        }

        public override IQueryState Accept(PagingExpression exp)
        {
            return new ShardingPagingQueryState(this.Context, this.QueryModel, exp.Skip, exp.Take);
        }
    }
}
