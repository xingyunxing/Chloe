using Chloe.DbExpressions;
using Chloe.RDBMS;

namespace Chloe.RDBMS.MethodHandlers
{
    public class TrimStart_HandlerBase : MethodHandlerBase
    {
        public override bool CanProcess(DbMethodCallExpression exp)
        {
            if (exp.Method != PublicConstants.MethodInfo_String_TrimStart)
                return false;

            return true;
        }
    }
}
