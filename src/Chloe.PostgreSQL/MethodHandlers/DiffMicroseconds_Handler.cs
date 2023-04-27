using Chloe.DbExpressions;
using Chloe.RDBMS;
using Chloe.RDBMS.MethodHandlers;

namespace Chloe.PostgreSQL.MethodHandlers
{
    class DiffMicroseconds_Handler : DiffMicroseconds_HandlerBase
    {
        public override bool CanProcess(DbMethodCallExpression exp)
        {
            return false;
        }
    }
}
