namespace Chloe.Sharding.Queries
{
    class TableDataQueryPlan<T>
    {
        public DataQueryModel QueryModel { get; set; }

        public SingleTableDataQuery<T> Query { get; set; }
    }
}
