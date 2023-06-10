using Chloe.DbExpressions;

namespace Chloe.RDBMS.MethodHandlers
{
    public class AddSeconds_HandlerBase : MethodHandlerBase
    {
        public override bool CanProcess(DbMethodCallExpression exp)
        {
            if (exp.Method.DeclaringType != PublicConstants.TypeOfDateTime)
                return false;

            return true;
        }
    }
}
