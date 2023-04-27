using Chloe.DbExpressions;
using Chloe.RDBMS;
using Chloe.RDBMS.MethodHandlers;

namespace Chloe.PostgreSQL.MethodHandlers
{
    class DiffHours_Handler : DiffHours_HandlerBase
    {
        public override bool CanProcess(DbMethodCallExpression exp)
        {
            return false;
        }
    }
}
