using Chloe.DbExpressions;
using Chloe.RDBMS;
using Chloe.RDBMS.PropertyHandlers;
using System.Reflection;

namespace Chloe.KingbaseES.PropertyHandlers
{
    class UtcNow_Handler : UtcNow_HandlerBase
    {
        public override void Process(DbMemberExpression exp, SqlGeneratorBase generator)
        {
            generator.SqlBuilder.Append("(current_timestamp AT TIME ZONE 'UTC')");
        }
    }
}
