using Chloe.DbExpressions;

namespace Chloe.RDBMS.PropertyHandlers
{
    public class Minute_HandlerBase : PropertyHandlerBase
    {
        public override bool CanProcess(DbMemberAccessExpression exp)
        {
            return exp.Member == PublicConstants.PropertyInfo_DateTime_Minute || exp.Member == PublicConstants.PropertyInfo_DateTimeOffset_Minute;
        }
    }
}
