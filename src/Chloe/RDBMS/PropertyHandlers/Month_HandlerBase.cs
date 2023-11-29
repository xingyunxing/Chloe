using System.Reflection;

namespace Chloe.RDBMS.PropertyHandlers
{
    public class Month_HandlerBase : PropertyHandlerBase
    {
        public override MemberInfo GetCanProcessProperty()
        {
            return PublicConstants.PropertyInfo_DateTime_Month;
        }
    }
}
