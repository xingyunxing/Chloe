using System.Reflection;

namespace Chloe.RDBMS.PropertyHandlers
{
    public class DayOfWeek_HandlerBase : PropertyHandlerBase
    {
        public override MemberInfo GetCanProcessProperty()
        {
            return PublicConstants.PropertyInfo_DateTime_DayOfWeek;
        }
    }
}
