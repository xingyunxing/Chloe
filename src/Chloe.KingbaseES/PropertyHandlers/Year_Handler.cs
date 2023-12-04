using Chloe.DbExpressions;
using Chloe.RDBMS;
using Chloe.RDBMS.PropertyHandlers;

namespace Chloe.KingbaseES.PropertyHandlers
{
    public class Year_Handler : Year_HandlerBase
    {
        public override void Process(DbMemberExpression exp, SqlGeneratorBase generator)
        {
            SqlGenerator.DbFunction_DATEPART(generator, "year", exp.Expression);
        }
    }
}
