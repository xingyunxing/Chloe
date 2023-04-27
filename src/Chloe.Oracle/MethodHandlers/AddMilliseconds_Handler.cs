using Chloe.DbExpressions;
using Chloe.RDBMS;
using Chloe.RDBMS.MethodHandlers;

namespace Chloe.Oracle.MethodHandlers
{
    class AddMilliseconds_Handler : AddMilliseconds_HandlerBase
    {
        public override bool CanProcess(DbMethodCallExpression exp)
        {
            return false;
        }
    }
}
