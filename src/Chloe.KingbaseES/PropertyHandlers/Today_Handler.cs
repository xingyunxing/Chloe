using Chloe.DbExpressions;
using Chloe.RDBMS;
using Chloe.RDBMS.PropertyHandlers;

namespace Chloe.KingbaseES.PropertyHandlers
{
    class Today_Handler : Today_HandlerBase
    {
        public override void Process(DbMemberExpression exp, SqlGeneratorBase generator)
        {
            generator.SqlBuilder.Append("TRUNC(systimestamp,'dd')");
        }
    }
}
