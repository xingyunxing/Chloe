using Chloe.DbExpressions;

namespace Chloe.RDBMS.PropertyHandlers
{
    public class Date_HandlerBase : PropertyHandlerBase
    {
        public override bool CanProcess(DbMemberAccessExpression exp)
        {
            return exp.Member == PublicConstants.PropertyInfo_DateTime_Date || exp.Member == PublicConstants.PropertyInfo_DateTimeOffset_Date;
        }
    }
}
