using Chloe.DbExpressions;
using Chloe.RDBMS;
using Chloe.RDBMS.PropertyHandlers;

namespace Chloe.KingbaseES.PropertyHandlers
{
    class DayOfWeek_Handler : DayOfWeek_HandlerBase
    {
        public override void Process(DbMemberAccessExpression exp, SqlGeneratorBase generator)
        {
            generator.SqlBuilder.Append("CAST(");
            SqlGenerator.DbFunction_DATEPART(generator, "dow", exp.Expression);
            generator.SqlBuilder.Append(" AS smallint)");
        }
    }
}
