namespace Chloe.Sharding.Queries
{
    class MultTableKeyQueryResult
    {
        public RouteTable Table { get; set; }
        public List<object> Keys { get; set; } = new List<object>();
    }
}
