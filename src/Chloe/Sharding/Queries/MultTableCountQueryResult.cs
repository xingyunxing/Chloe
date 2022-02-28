namespace Chloe.Sharding.Queries
{
    class MultTableCountQueryResult
    {
        public RouteTable Table { get; set; }
        public long Count { get; set; }
    }
}
