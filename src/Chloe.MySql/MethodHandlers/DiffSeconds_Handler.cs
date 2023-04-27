using Chloe.DbExpressions;
using Chloe.RDBMS;
using Chloe.RDBMS.MethodHandlers;

namespace Chloe.MySql.MethodHandlers
{
    class DiffSeconds_Handler : DiffSeconds_HandlerBase
    {
        public override void Process(DbMethodCallExpression exp, SqlGeneratorBase generator)
        {
            SqlGenerator.DbFunction_DATEDIFF(generator, "SECOND", exp.Arguments[0], exp.Arguments[1]);
        }
    }
}
