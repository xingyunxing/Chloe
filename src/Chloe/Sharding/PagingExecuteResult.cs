namespace Chloe.Sharding
{
    internal class PagingExecuteResult<T>
    {
        public PagingExecuteResult()
        {

        }
        public PagingExecuteResult(long count, IFeatureEnumerable<T> result)
        {
            this.Count = count;
            this.Result = result;
        }

        public long Count { get; set; }
        public IFeatureEnumerable<T> Result { get; set; }
    }
}
