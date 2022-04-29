using System.Collections;
using System.Reflection;
using Chloe.Reflection;

namespace Chloe
{
    internal class PagingResult
    {
        static MethodInfo MakeTypePagingResultMethod;

        static PagingResult()
        {
            MakeTypePagingResultMethod = typeof(PagingResult).GetMethod(nameof(PagingResult.MakeTypedPagingResult), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        }

        public long Count { get; set; }
        public IList DataList { get; set; }

        internal object MakeTypedPagingResultObject(Type elementType)
        {
            var ret = MakeTypePagingResultMethod.MakeGenericMethod(elementType).FastInvoke(this);
            return ret;
        }
        internal PagingResult<T> MakeTypedPagingResult<T>()
        {
            PagingResult<T> pagingResult = new PagingResult<T>();
            pagingResult.Count = Count;

            if (this.DataList is List<T> dataList)
            {
                pagingResult.DataList = dataList;

                return pagingResult;
            }

            pagingResult.DataList.Capacity = this.DataList.Count;

            foreach (var item in this.DataList)
            {
                pagingResult.DataList.Add((T)item);
            }
            return pagingResult;
        }
    }

    public class PagingResult<T>
    {
        public long Count { get; set; }
        public List<T> DataList { get; set; } = new List<T>();
    }
}
