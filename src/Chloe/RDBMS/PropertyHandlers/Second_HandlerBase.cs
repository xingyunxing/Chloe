using Chloe.DbExpressions;

namespace Chloe.RDBMS.PropertyHandlers
{
    public class Second_HandlerBase : PropertyHandlerBase
    {
        public override bool CanProcess(DbMemberAccessExpression exp)
        {
            return exp.Member == PublicConstants.PropertyInfo_DateTime_Second || exp.Member == PublicConstants.PropertyInfo_DateTimeOffset_Second;
        }
    }
}
