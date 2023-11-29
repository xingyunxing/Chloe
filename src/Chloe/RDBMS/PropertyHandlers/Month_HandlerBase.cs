using Chloe.DbExpressions;

namespace Chloe.RDBMS.PropertyHandlers
{
    public class Month_HandlerBase : PropertyHandlerBase
    {
        public override bool CanProcess(DbMemberExpression exp)
        {
            return exp.Member == PublicConstants.PropertyInfo_DateTime_Month || exp.Member == PublicConstants.PropertyInfo_DateTimeOffset_Month;
        }
    }
}
