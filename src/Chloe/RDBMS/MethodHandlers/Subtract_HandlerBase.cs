using Chloe.DbExpressions;
using Chloe.RDBMS;

namespace Chloe.RDBMS.MethodHandlers
{
    public class Subtract_HandlerBase : MethodHandlerBase
    {
        public override bool CanProcess(DbMethodCallExpression exp)
        {
            if (exp.Method != PublicConstants.MethodInfo_DateTime_Subtract_DateTime)
                return false;

            return true;
        }
        public override void Process(DbMethodCallExpression exp, SqlGeneratorBase generator)
        {
            DbSubtractExpression dbSubtract = new DbSubtractExpression(exp.Type, exp.Object, exp.Arguments[0]);
            dbSubtract.Accept(generator);
        }
    }
}
