using Chloe.DbExpressions;
using Chloe.Reflection;
using System.Reflection;

namespace Chloe.RDBMS.PropertyHandlers
{
    public class Value_HandlerBase : PropertyHandlerBase
    {
        public override bool CanProcess(DbMemberAccessExpression exp)
        {
            MemberInfo member = exp.Member;

            if (member.Name == "Value" && ReflectionExtension.IsNullable(exp.Expression.Type))
            {
                return true;
            }

            return false;
        }

        public override void Process(DbMemberAccessExpression exp, SqlGeneratorBase generator)
        {
            exp.Expression.Accept(generator);
        }
    }
}
