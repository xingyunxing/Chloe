using Chloe.Annotations;
using Chloe.DbExpressions;
using System.Collections;
using System.Data;
using System.Reflection;

namespace Chloe.RDBMS.MethodHandlers
{
    public  class FindInSet_HandlerBase : MethodHandlerBase
    {

        public override bool CanProcess(DbMethodCallExpression exp) => exp.Method.IsDefined(typeof(DbFunctionAttribute));


    }
}
