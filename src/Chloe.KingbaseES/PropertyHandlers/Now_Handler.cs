using Chloe.DbExpressions;
using Chloe.RDBMS;
using Chloe.RDBMS.PropertyHandlers;

namespace Chloe.KingbaseES.PropertyHandlers
{
    class Now_Handler : Now_HandlerBase
    {
        public override void Process(DbMemberExpression exp, SqlGeneratorBase generator)
        {
            generator.SqlBuilder.Append("systimestamp");
        }
    }
}
