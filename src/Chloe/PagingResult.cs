using System.Collections;

namespace Chloe
{
    internal interface IPagingResult
    {
        long Count { get; set; }
        IList DataList { get; set; }
    }

    public class PagingResult<T> : IPagingResult
    {
        public long Count { get; set; }
        public List<T> DataList { get; set; }
        long IPagingResult.Count { get => this.Count; set => this.Count = value; }
        IList IPagingResult.DataList { get => this.DataList; set => this.DataList = (List<T>)value; }
    }
}
