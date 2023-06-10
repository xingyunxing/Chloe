using Chloe.DbExpressions;

namespace Chloe.RDBMS.MethodHandlers
{
    public class Trim_HandlerBase : MethodHandlerBase
    {
        public override bool CanProcess(DbMethodCallExpression exp)
        {
            if (exp.Method != PublicConstants.MethodInfo_String_Trim)
                return false;

            return true;
        }
    }
}
