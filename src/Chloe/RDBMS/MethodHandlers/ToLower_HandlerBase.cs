using Chloe.DbExpressions;

namespace Chloe.RDBMS.MethodHandlers
{
    public class ToLower_HandlerBase : MethodHandlerBase
    {
        public override bool CanProcess(DbMethodCallExpression exp)
        {
            if (exp.Method != PublicConstants.MethodInfo_String_ToLower)
                return false;

            return true;
        }
    }
}
