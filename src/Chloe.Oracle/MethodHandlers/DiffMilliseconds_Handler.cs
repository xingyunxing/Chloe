using Chloe.DbExpressions;
using Chloe.RDBMS;
using Chloe.RDBMS.MethodHandlers;

namespace Chloe.Oracle.MethodHandlers
{
    class DiffMilliseconds_Handler : DiffMilliseconds_HandlerBase
    {
        public override void Process(DbMethodCallExpression exp, SqlGeneratorBase generator)
        {
            throw new NotSupportedException(MethodHandlerHelper.AppendNotSupportedDbFunctionsMsg(exp.Method, "TotalMilliseconds"));
        }
    }
}
