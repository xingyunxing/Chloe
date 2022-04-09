namespace Chloe.Sharding.Queries
{
    class TableDataQueryPlan<T>
    {
        public DataQueryModel QueryModel { get; set; }

        public SingleTableEntityQuery<T> Query { get; set; }
    }
}
