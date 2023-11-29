using Chloe.DbExpressions;
using Chloe.RDBMS;
using Chloe.RDBMS.PropertyHandlers;

namespace Chloe.Oracle.PropertyHandlers
{
    class DayOfWeek_Handler : DayOfWeek_HandlerBase
    {
        public override void Process(DbMemberExpression exp, SqlGeneratorBase generator)
        {
            // CAST(TO_CHAR(SYSDATE,'D') AS NUMBER) - 1
            generator.SqlBuilder.Append("(");
            SqlGenerator.DbFunction_DATEPART(generator, "D", exp.Expression);
            generator.SqlBuilder.Append(" - 1");
            generator.SqlBuilder.Append(")");
        }
    }
}
