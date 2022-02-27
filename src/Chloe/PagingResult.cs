namespace Chloe
{
    public class PagingResult<T>
    {
        public long Count { get; set; }
        public List<T> DataList { get; set; }
    }
}
