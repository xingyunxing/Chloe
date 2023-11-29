using Chloe.DbExpressions;
using Chloe.RDBMS;
using Chloe.RDBMS.PropertyHandlers;

namespace Chloe.SqlServer.PropertyHandlers
{
    class Date_Handler : Date_HandlerBase
    {
        public override void Process(DbMemberExpression exp, SqlGeneratorBase generator)
        {
            SqlGeneratorBase.BuildCastState(generator, exp.Expression, "DATE");
        }
    }
}
