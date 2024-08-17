using Chloe.DbExpressions;

namespace Chloe.RDBMS.PropertyHandlers
{
    public class Day_HandlerBase : PropertyHandlerBase
    {
        public override bool CanProcess(DbMemberAccessExpression exp)
        {
            return exp.Member == PublicConstants.PropertyInfo_DateTime_Day || exp.Member == PublicConstants.PropertyInfo_DateTimeOffset_Day;
        }
    }
}
