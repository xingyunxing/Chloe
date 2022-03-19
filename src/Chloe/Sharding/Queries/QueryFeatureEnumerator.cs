namespace Chloe.Sharding
{
    internal class QueryFeatureEnumerator<T> : FeatureEnumerator<T>
    {
        public QueryFeatureEnumerator(ShardingQueryPlan queryPlan)
        {
            this.QueryPlan = queryPlan;
        }

        protected ShardingQueryPlan QueryPlan { get; set; }
        protected IShardingContext ShardingContext { get { return this.QueryPlan.ShardingContext; } }
        protected ShardingQueryModel QueryModel { get { return this.QueryPlan.QueryModel; } }

        protected override async BoolResultTask MoveNext(bool @async)
        {
            var hasNext = await base.MoveNext(@async);

            if (this.QueryPlan.IsTrackingQuery && hasNext)
            {
                if (hasNext)
                {
                    this.QueryPlan.ShardingContext.DbContext.TrackEntity(this.Current);
                }
            }

            return hasNext;
        }
    }
}
