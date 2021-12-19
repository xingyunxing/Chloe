using Chloe.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChloeDemo
{
    public static class DbFunctions
    {
        [Chloe.Annotations.DbFunctionAttribute()]
        public static string MyFunction(int value)
        {
            throw new NotImplementedException();
        }

        public static bool StringLike(this string str, string value)
        {
            return str.Contains(value);
        }

        public static bool GroupConcat<T>(T field)
        {
            throw new NotSupportedException("Using in lambda only.");
        }
    }
}
