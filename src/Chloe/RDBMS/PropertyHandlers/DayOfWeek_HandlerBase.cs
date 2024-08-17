using Chloe.DbExpressions;

namespace Chloe.RDBMS.PropertyHandlers
{
    public class DayOfWeek_HandlerBase : PropertyHandlerBase
    {
        public override bool CanProcess(DbMemberAccessExpression exp)
        {
            return exp.Member == PublicConstants.PropertyInfo_DateTime_DayOfWeek || exp.Member == PublicConstants.PropertyInfo_DateTimeOffset_DayOfWeek;
        }
    }
}
