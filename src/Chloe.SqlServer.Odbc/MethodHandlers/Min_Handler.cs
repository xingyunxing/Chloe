using Chloe.DbExpressions;
using Chloe.RDBMS;
using Chloe.RDBMS.MethodHandlers;

namespace Chloe.SqlServer.Odbc.MethodHandlers
{
    class Min_Handler : Min_HandlerBase
    {
        public override void Process(DbMethodCallExpression exp, SqlGeneratorBase generator)
        {
            SqlGenerator.Aggregate_Min(generator, exp.Arguments.First(), exp.Method.ReturnType);
        }
    }
}
