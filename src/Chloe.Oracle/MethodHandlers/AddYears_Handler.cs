using Chloe.DbExpressions;
using Chloe.RDBMS;
using Chloe.RDBMS.MethodHandlers;

namespace Chloe.Oracle.MethodHandlers
{
    class AddYears_Handler : AddYears_HandlerBase
    {
        public override void Process(DbMethodCallExpression exp, SqlGeneratorBase generator)
        {
            /* add_months(systimestamp,12 * 2) */
            generator.SqlBuilder.Append("ADD_MONTHS(");
            exp.Object.Accept(generator);
            generator.SqlBuilder.Append(",12 * ");
            exp.Arguments[0].Accept(generator);
            generator.SqlBuilder.Append(")");
        }
    }
}
