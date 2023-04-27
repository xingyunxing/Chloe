using Chloe.DbExpressions;
using Chloe.RDBMS;

namespace Chloe.RDBMS.MethodHandlers
{
    public class Substring_HandlerBase : MethodHandlerBase
    {
        public override bool CanProcess(DbMethodCallExpression exp)
        {
            if (exp.Method != PublicConstants.MethodInfo_String_Substring_Int32 && exp.Method != PublicConstants.MethodInfo_String_Substring_Int32_Int32)
                return false;

            return true;
        }
    }
}
