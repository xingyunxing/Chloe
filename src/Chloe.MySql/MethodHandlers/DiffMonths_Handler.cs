using Chloe.DbExpressions;
using Chloe.RDBMS;
using Chloe.RDBMS.MethodHandlers;

namespace Chloe.MySql.MethodHandlers
{
    class DiffMonths_Handler : DiffMonths_HandlerBase
    {
        public override void Process(DbMethodCallExpression exp, SqlGeneratorBase generator)
        {
            SqlGenerator.DbFunction_DATEDIFF(generator, "MONTH", exp.Arguments[0], exp.Arguments[1]);
        }
    }
}
