namespace Chloe.Sharding.Queries
{
    class KeyQueryResult : QueryResult<List<object>>
    {
        public List<object> Keys => this.Result;
    }
}
