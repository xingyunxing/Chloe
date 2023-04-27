using Chloe.DbExpressions;
using Chloe.RDBMS;
using Chloe.RDBMS.MethodHandlers;

namespace Chloe.SqlServer.MethodHandlers
{
    class DiffMicroseconds_Handler : DiffMicroseconds_HandlerBase
    {
        public override void Process(DbMethodCallExpression exp, SqlGeneratorBase generator)
        {
            SqlGenerator.DbFunction_DATEDIFF(generator, "MICROSECOND", exp.Arguments[0], exp.Arguments[1]);
        }
    }
}
