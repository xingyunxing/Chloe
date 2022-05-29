namespace Chloe.Sharding
{
    internal class TrackableFeatureEnumerator<T> : FeatureEnumerator<T>
    {
        public TrackableFeatureEnumerator(ShardingQueryPlan queryPlan)
        {
            this.QueryPlan = queryPlan;
        }

        protected ShardingQueryPlan QueryPlan { get; set; }
        protected IShardingContext ShardingContext { get { return this.QueryPlan.ShardingContext; } }
        protected ShardingQueryModel QueryModel { get { return this.QueryPlan.QueryModel; } }

        protected override async BoolResultTask MoveNext(bool @async)
        {
            var hasNext = await base.MoveNext(@async);

            if (this.QueryModel.IsTracking)
            {
                if (hasNext)
                {
                    this.QueryPlan.ShardingContext.DbContextProvider.TrackEntity(this.Current);
                }
            }

            return hasNext;
        }
    }
}
