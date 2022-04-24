using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

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

        public static long LongCount(this IQuery query)
        {
            throw new NotImplementedException();
        }
        public static async Task<long> LongCountAsync(this IQuery query)
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
    }
}
