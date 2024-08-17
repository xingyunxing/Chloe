using Chloe.DbExpressions;

namespace Chloe.RDBMS.PropertyHandlers
{
    public class Year_HandlerBase : PropertyHandlerBase
    {
        public override bool CanProcess(DbMemberAccessExpression exp)
        {
            return exp.Member == PublicConstants.PropertyInfo_DateTime_Year || exp.Member == PublicConstants.PropertyInfo_DateTimeOffset_Year;
        }
    }
}
