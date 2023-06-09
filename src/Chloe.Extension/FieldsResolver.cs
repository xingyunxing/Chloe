using Chloe.Extensions;
using Chloe.Reflection;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Chloe.Extension
{
    class FieldsResolver
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldsLambdaExpression">a => new { a.Name, a.Age } or a => new object[] { a.Name, a.Age }</param>
        /// <returns></returns>
        public static List<string> Resolve(LambdaExpression fieldsLambdaExpression)
        {
            return PublicHelper.ResolveFields(fieldsLambdaExpression).Select(a => a.Name).ToList();
        }
    }
}
