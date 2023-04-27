using Chloe.DbExpressions;
using Chloe.RDBMS;
using Chloe.RDBMS.MethodHandlers;

namespace Chloe.Oracle.MethodHandlers
{
    class AddDays_Handler : AddDays_HandlerBase
    {
        public override void Process(DbMethodCallExpression exp, SqlGeneratorBase generator)
        {
            /* (systimestamp + 3) */
            generator.SqlBuilder.Append("(");
            exp.Object.Accept(generator);
            generator.SqlBuilder.Append(" + ");
            exp.Arguments[0].Accept(generator);
            generator.SqlBuilder.Append(")");
        }
    }
}
