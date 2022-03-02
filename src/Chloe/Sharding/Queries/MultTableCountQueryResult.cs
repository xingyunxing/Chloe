namespace Chloe.Sharding.Queries
{
    class MultTableCountQueryResult
    {
        public IPhysicTable Table { get; set; }
        public long Count { get; set; }
    }
}
