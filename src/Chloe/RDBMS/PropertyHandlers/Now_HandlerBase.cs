using System.Reflection;

namespace Chloe.RDBMS.PropertyHandlers
{
    public class Now_HandlerBase : PropertyHandlerBase
    {
        public override MemberInfo GetCanProcessProperty()
        {
            return PublicConstants.PropertyInfo_DateTime_Now;
        }
    }
}
