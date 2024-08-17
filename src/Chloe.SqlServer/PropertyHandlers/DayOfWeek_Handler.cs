using Chloe.DbExpressions;
using Chloe.RDBMS;
using Chloe.RDBMS.PropertyHandlers;

namespace Chloe.SqlServer.PropertyHandlers
{
    class DayOfWeek_Handler : DayOfWeek_HandlerBase
    {
        public override void Process(DbMemberAccessExpression exp, SqlGeneratorBase generator)
        {
            generator.SqlBuilder.Append("(");
            SqlGenerator.DbFunction_DATEPART(generator, "WEEKDAY", exp.Expression);
            generator.SqlBuilder.Append(" - 1)");
        }
    }
}
