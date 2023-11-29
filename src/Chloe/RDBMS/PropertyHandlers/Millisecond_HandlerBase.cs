using Chloe.DbExpressions;

namespace Chloe.RDBMS.PropertyHandlers
{
    public class Millisecond_HandlerBase : PropertyHandlerBase
    {
        public override bool CanProcess(DbMemberExpression exp)
        {
            return exp.Member == PublicConstants.PropertyInfo_DateTime_Millisecond || exp.Member == PublicConstants.PropertyInfo_DateTimeOffset_Millisecond;
        }
    }
}
