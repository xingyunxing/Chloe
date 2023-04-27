using Chloe.DbExpressions;
using Chloe.RDBMS;

namespace Chloe.RDBMS.MethodHandlers
{
    public class NewGuid_HandlerBase : MethodHandlerBase
    {
        public override bool CanProcess(DbMethodCallExpression exp)
        {
            if (exp.Method != PublicConstants.MethodInfo_Guid_NewGuid)
                return false;

            return true;
        }
    }
}
