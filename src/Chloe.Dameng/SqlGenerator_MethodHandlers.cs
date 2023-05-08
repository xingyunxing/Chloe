using Chloe.RDBMS;
using System.Reflection;

namespace Chloe.Dameng
{
    //hongyl 调整nameSpace
    internal partial class SqlGenerator : SqlGeneratorBase
    {
        private static Dictionary<string, IMethodHandler> InitMethodHandlers()
        {
            var methodHandlers = new Dictionary<string, IMethodHandler>();
            var nameSpace = "Chloe.Dameng.MethodHandlers";
            var methodHandlerTypes = Assembly.GetExecutingAssembly().GetTypes().Where(a => a.Namespace == nameSpace && a.IsClass && !a.IsAbstract && typeof(IMethodHandler).IsAssignableFrom(a) && a.Name.EndsWith("_Handler") && a.GetConstructor(Type.EmptyTypes) != null);

            foreach (Type methodHandlerType in methodHandlerTypes)
            {
                string handleMethodName = methodHandlerType.Name.Substring(0, methodHandlerType.Name.Length - "_Handler".Length);
                methodHandlers.Add(handleMethodName, (IMethodHandler)Activator.CreateInstance(methodHandlerType));
            }

            var ret = PublicHelper.Clone(methodHandlers);
            return ret;
        }
    }
}