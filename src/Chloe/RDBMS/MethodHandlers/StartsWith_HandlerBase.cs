using Chloe.DbExpressions;

namespace Chloe.RDBMS.MethodHandlers
{
    public class StartsWith_HandlerBase : MethodHandlerBase
    {
        public override bool CanProcess(DbMethodCallExpression exp)
        {
            if (exp.Method != PublicConstants.MethodInfo_String_StartsWith)
                return false;

            return true;
        }
    }
}
