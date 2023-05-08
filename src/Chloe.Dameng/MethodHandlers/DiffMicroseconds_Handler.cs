using Chloe.DbExpressions;
using Chloe.RDBMS;
using Chloe.RDBMS.MethodHandlers;

namespace Chloe.Dameng.MethodHandlers
{
    class DiffMicroseconds_Handler : DiffMicroseconds_HandlerBase
    {
        public override void Process(DbMethodCallExpression exp, SqlGeneratorBase generator)
        {
            SqlGenerator.DbFunction_DATEDIFF(generator, "MILLISECOND", exp.Arguments[0], exp.Arguments[1]);
        }
    }
}
