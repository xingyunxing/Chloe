using Chloe.DbExpressions;
using Chloe.RDBMS;
using Chloe.RDBMS.MethodHandlers;

namespace Chloe.PostgreSQL.MethodHandlers
{
    class AddDays_Handler : AddDays_HandlerBase
    {
        public override void Process(DbMethodCallExpression exp, SqlGeneratorBase generator)
        {
            List<DbExpression> arguments = new List<DbExpression>(exp.Arguments.Count);
            arguments.Add(new DbConvertExpression(typeof(int), exp.Arguments[0]));
            DbMethodCallExpression e = new DbMethodCallExpression(exp.Object, exp.Method, arguments);

            SqlGenerator.DbFunction_DATEADD(generator, "days", e);
        }
    }
}
