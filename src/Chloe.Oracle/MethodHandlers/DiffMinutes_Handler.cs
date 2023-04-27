using Chloe.DbExpressions;
using Chloe.RDBMS;
using Chloe.RDBMS.MethodHandlers;

namespace Chloe.Oracle.MethodHandlers
{
    class DiffMinutes_Handler : DiffMinutes_HandlerBase
    {
        public override void Process(DbMethodCallExpression exp, SqlGeneratorBase generator)
        {
            throw new NotSupportedException(MethodHandlerHelper.AppendNotSupportedDbFunctionsMsg(exp.Method, "TotalMinutes"));
        }
    }
}
