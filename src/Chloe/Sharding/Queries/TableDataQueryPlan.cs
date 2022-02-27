namespace Chloe.Sharding.Queries
{
    class TableDataQueryPlan<T>
    {
        public PhysicTable Table { get; set; }
        public DataQueryModel QueryModel { get; set; }

        public SingleTableDataQuery<T> Query { get; set; }
    }
}
