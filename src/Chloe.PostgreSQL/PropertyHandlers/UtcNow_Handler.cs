using Chloe.DbExpressions;
using Chloe.RDBMS;
using Chloe.RDBMS.PropertyHandlers;
using System.Reflection;

namespace Chloe.PostgreSQL.PropertyHandlers
{
    class UtcNow_Handler : UtcNow_HandlerBase
    {
        public override bool CanProcess(DbMemberExpression exp)
        {
            return false;
        }
    }
}
