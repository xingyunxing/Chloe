using Chloe.DbExpressions;

namespace Chloe.RDBMS.MethodHandlers
{
    public class ToUpper_HandlerBase : MethodHandlerBase
    {
        public override bool CanProcess(DbMethodCallExpression exp)
        {
            if (exp.Method != PublicConstants.MethodInfo_String_ToUpper)
                return false;

            return true;
        }
    }
}
