namespace System.Collections.Generic
{
    internal static class ListExtension
    {
        public static void AppendRange<T>(this List<T> list, IEnumerable<T> source)
        {
            if (source is ICollection sourceList)
            {
                int estimateSize = list.Count + sourceList.Count;

                if (list.Capacity < estimateSize)
                {
                    list.Capacity = estimateSize;
                }
            }

            list.AddRange(source);
        }
        public static List<T> CloneAndAppendOne<T>(this List<T> source, T t)
        {
            List<T> ret = new List<T>(source.Count + 1);
            ret.AddRange(source);
            ret.Add(t);
            return ret;
        }

        public static List<T> Clone<T>(this List<T> source, int? capacity = null)
        {
            List<T> ret = new List<T>(capacity ?? source.Count);
            ret.AddRange(source);
            return ret;
        }
    }
}
