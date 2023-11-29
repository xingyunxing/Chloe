using Chloe.DbExpressions;
using System.Reflection;

namespace Chloe.RDBMS.PropertyHandlers
{
    public class Length_HandlerBase : PropertyHandlerBase
    {
        public override MemberInfo GetCanProcessProperty()
        {
            return PublicConstants.PropertyInfo_String_Length;
        }

        public override void Process(DbMemberExpression exp, SqlGeneratorBase generator)
        {
            generator.SqlBuilder.Append("LENGTH(");
            exp.Expression.Accept(generator);
            generator.SqlBuilder.Append(")");
        }
    }
}
