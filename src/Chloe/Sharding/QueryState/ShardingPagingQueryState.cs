using Chloe.QueryExpressions;
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
            ShardingQueryPlan queryPlan = this.CreateQueryPlan();

            FeatureEnumerable<PagingResult> pagingQueryEnumerable = null;
            if (queryPlan.Tables.Count == 1)
            {
                pagingQueryEnumerable = new SingleTablePagingQueryEnumerable(queryPlan);
            }
            else
            {
                pagingQueryEnumerable = new PagingQueryEnumerable(queryPlan);
            }

            return pagingQueryEnumerable.Select(a => a.MakeTypedPagingResultObject(this.QueryModel.GetElementType()));
        }
    }
}
