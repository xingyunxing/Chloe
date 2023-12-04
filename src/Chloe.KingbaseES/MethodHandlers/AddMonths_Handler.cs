using Chloe.DbExpressions;
using Chloe.RDBMS;
using Chloe.RDBMS.MethodHandlers;

namespace Chloe.KingbaseES.MethodHandlers
{
    class AddMonths_Handler : AddMonths_HandlerBase
    {
        public override void Process(DbMethodCallExpression exp, SqlGeneratorBase generator)
        {
            SqlGenerator.DbFunction_DATEADD(generator, "months", exp);
        }
    }
}
