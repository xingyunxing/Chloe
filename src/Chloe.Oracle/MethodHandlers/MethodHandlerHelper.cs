using System.Reflection;

namespace Chloe.Oracle.MethodHandlers
{
    class MethodHandlerHelper
    {
        public static string AppendNotSupportedDbFunctionsMsg(MethodInfo method, string insteadProperty)
        {
            return string.Format("'{0}' is not supported. Instead of using '{1}.{2}'.", Utils.ToMethodString(method), Utils.ToMethodString(PublicConstants.MethodInfo_DateTime_Subtract_DateTime), insteadProperty);
        }
    }
}
