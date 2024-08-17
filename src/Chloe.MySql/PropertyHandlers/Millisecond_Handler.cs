using Chloe.DbExpressions;
using Chloe.RDBMS;
using Chloe.RDBMS.PropertyHandlers;

namespace Chloe.MySql.PropertyHandlers
{
    class Millisecond_Handler : Millisecond_HandlerBase
    {
        public override bool CanProcess(DbMemberAccessExpression exp)
        {
            return false;
        }
    }
}
