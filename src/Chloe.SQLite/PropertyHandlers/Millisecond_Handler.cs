using Chloe.DbExpressions;
using Chloe.RDBMS;
using Chloe.RDBMS.PropertyHandlers;

namespace Chloe.SQLite.PropertyHandlers
{
    class Millisecond_Handler : Millisecond_HandlerBase
    {
        public override bool CanProcess(DbMemberExpression exp)
        {
            return false;
        }
    }
}
