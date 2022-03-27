namespace Chloe.Sharding.Queries
{
    class QueryResult<T>
    {
        public IPhysicTable Table { get; set; }
        public T Result { get; set; }
    }
}
