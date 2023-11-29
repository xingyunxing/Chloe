using Chloe.DbExpressions;
using Chloe.RDBMS;
using Chloe.RDBMS.PropertyHandlers;

namespace Chloe.PostgreSQL.PropertyHandlers
{
    class DayOfWeek_Handler : DayOfWeek_HandlerBase
    {
        public override void Process(DbMemberExpression exp, SqlGeneratorBase generator)
        {
            SqlGenerator.DbFunction_DATEPART(generator, "DOW", exp.Expression);
        }
    }
}
