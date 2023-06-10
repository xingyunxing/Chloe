using Chloe.DbExpressions;

namespace Chloe.RDBMS.MethodHandlers
{
    public class Min_HandlerBase : MethodHandlerBase
    {
        public override bool CanProcess(DbMethodCallExpression exp)
        {
            if (exp.Method.DeclaringType != PublicConstants.TypeOfSql)
                return false;

            return true;
        }
    }
}
