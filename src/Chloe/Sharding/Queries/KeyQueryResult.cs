namespace Chloe.Sharding.Queries
{
    class KeyQueryResult
    {
        public IPhysicTable Table { get; set; }
        public List<object> Keys { get; set; } = new List<object>();
    }
}
