namespace System.Collections
{
    internal static class EnumerableExtension
    {
        public static IEnumerable<object> AsGenericEnumerable(this IEnumerable source)
        {
            foreach (object item in source)
            {
                yield return item;
            }
        }

        public static IEnumerable<T> AsGenericEnumerable<T>(this IEnumerable source)
        {
            foreach (object item in source)
            {
                yield return (T)item;
            }
        }
    }
}
