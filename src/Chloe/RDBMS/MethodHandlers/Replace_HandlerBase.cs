using Chloe.DbExpressions;
using Chloe.RDBMS;

namespace Chloe.RDBMS.MethodHandlers
{
    public class Replace_HandlerBase : MethodHandlerBase
    {
        public override bool CanProcess(DbMethodCallExpression exp)
        {
            if (exp.Method != PublicConstants.MethodInfo_String_Replace)
                return false;

            return true;
        }
    }
}
