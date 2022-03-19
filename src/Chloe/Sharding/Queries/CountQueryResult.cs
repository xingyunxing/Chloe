namespace Chloe.Sharding.Queries
{
    class CountQueryResult
    {
        public IPhysicTable Table { get; set; }
        public long Count { get; set; }
    }

    class QueryResult<T>
    {
        public IPhysicTable Table { get; set; }
        public T Result { get; set; }
    }
}
