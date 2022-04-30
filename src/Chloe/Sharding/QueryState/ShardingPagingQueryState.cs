using Chloe.Query.QueryExpressions;
using Chloe.Sharding.Enumerables;

namespace Chloe.Sharding.QueryState
{
    internal class ShardingPagingQueryState : ShardingQueryStateBase
    {
        public ShardingPagingQueryState(ShardingQueryStateBase prevQueryState, PagingExpression exp) : base(prevQueryState)
        {
            this.QueryModel.Skip = exp.Skip;
            this.QueryModel.Take = exp.Take;
        }

        public override IFeatureEnumerable<object> CreateQuery()
        {
            var pagingQueryEnumerable = new PagingQueryEnumerable(this.CreateQueryPlan());
            return pagingQueryEnumerable.Select(a => a.MakeTypedPagingResultObject(this.QueryModel.GetElementType()));
        }
    }
}
