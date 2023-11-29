using System.Reflection;

namespace Chloe.RDBMS.PropertyHandlers
{
    public class Millisecond_HandlerBase : PropertyHandlerBase
    {
        public override MemberInfo GetCanProcessProperty()
        {
            return PublicConstants.PropertyInfo_DateTime_Millisecond;
        }
    }
}
