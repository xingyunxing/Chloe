using System.Collections;
using System.Linq.Expressions;

namespace Chloe
{
    public static class QueryExtension
    {
        public static IEnumerable AsEnumerable(this IQuery query)
        {
            throw new NotImplementedException();
        }
        public static IList ToList(this IQuery query)
        {
            throw new NotImplementedException();
        }
        public static async Task<IList> ToListAsync(this IQuery query)
        {
            throw new NotImplementedException();
        }

        public static int Count(this IQuery query)
        {
            throw new NotImplementedException();
        }
        public static async Task<int> CountAsync(this IQuery query)
        {
            throw new NotImplementedException();
        }

        public static long LongCount(this IQuery query)
        {
            throw new NotImplementedException();
        }
        public static async Task<long> LongCountAsync(this IQuery query)
        {
            throw new NotImplementedException();
        }

        public static object Sum(this IQuery query, LambdaExpression selector)
        {
            throw new NotImplementedException();
        }
        public static Task<object> SumAsync(this IQuery query, LambdaExpression selector)
        {
            throw new NotImplementedException();
        }

        public static object Max(this IQuery query, LambdaExpression selector)
        {
            throw new NotImplementedException();
        }
        public static Task<object> MaxAsync(this IQuery query, LambdaExpression selector)
        {
            throw new NotImplementedException();
        }
        public static object Min(this IQuery query, LambdaExpression selector)
        {
            throw new NotImplementedException();
        }
        public static Task<object> MinAsync(this IQuery query, LambdaExpression selector)
        {
            throw new NotImplementedException();
        }

        public static bool Any(this IQuery query)
        {
            throw new NotImplementedException();
        }
        public static async Task<bool> AnyAsync(this IQuery query)
        {
            throw new NotImplementedException();
        }

        public static object First(this IQuery query)
        {
            throw new NotImplementedException();
        }
        public static Task<object> FirstAsync(this IQuery query)
        {
            throw new NotImplementedException();
        }

        public static IQuery Select(this IQuery query, LambdaExpression selector)
        {
            throw new NotImplementedException();
        }
    }
}
