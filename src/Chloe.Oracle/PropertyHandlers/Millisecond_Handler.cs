using Chloe.DbExpressions;
using Chloe.RDBMS;
using Chloe.RDBMS.PropertyHandlers;

namespace Chloe.Oracle.PropertyHandlers
{
    class Millisecond_Handler : Millisecond_HandlerBase
    {
        public override void Process(DbMemberExpression exp, SqlGeneratorBase generator)
        {
            /* exp.Expression must be TIMESTAMP,otherwise there will be an error occurred. */
            SqlGenerator.DbFunction_DATEPART(generator, "ff3", exp.Expression, true);
        }
    }
}
