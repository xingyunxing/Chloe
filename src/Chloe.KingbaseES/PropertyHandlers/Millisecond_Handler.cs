using Chloe.DbExpressions;
using Chloe.RDBMS;
using Chloe.RDBMS.PropertyHandlers;

namespace Chloe.KingbaseES.PropertyHandlers
{
    class Millisecond_Handler : Millisecond_HandlerBase
    {
        public override void Process(DbMemberExpression exp, SqlGeneratorBase generator)
        {
            SqlGenerator.DbFunction_DATEPART(generator, "millisecond", exp.Expression);
        }
    }
}
