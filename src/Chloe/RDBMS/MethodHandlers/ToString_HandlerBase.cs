using Chloe.DbExpressions;
using Chloe.Reflection;

namespace Chloe.RDBMS.MethodHandlers
{
    public class ToString_HandlerBase : MethodHandlerBase
    {
        public override bool CanProcess(DbMethodCallExpression exp)
        {
            if (exp.Arguments.Count != 0)
            {
                return false;
            }

            if (exp.Object.Type == PublicConstants.TypeOfString)
            {
                return true;
            }

            if (!PublicHelper.IsNumericType(exp.Object.Type.GetUnderlyingType()))
            {
                return false;
            }

            return true;
        }

        public override void Process(DbMethodCallExpression exp, SqlGeneratorBase generator)
        {
            if (exp.Object.Type == PublicConstants.TypeOfString)
            {
                exp.Object.Accept(generator);
                return;
            }

            DbConvertExpression c = new DbConvertExpression(PublicConstants.TypeOfString, exp.Object);
            c.Accept(generator);
        }
    }
}
