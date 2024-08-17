using Chloe.DbExpressions;

namespace Chloe.RDBMS.PropertyHandlers
{
    public class Hour_HandlerBase : PropertyHandlerBase
    {
        public override bool CanProcess(DbMemberAccessExpression exp)
        {
            return exp.Member == PublicConstants.PropertyInfo_DateTime_Hour || exp.Member == PublicConstants.PropertyInfo_DateTimeOffset_Hour;
        }
    }
}
