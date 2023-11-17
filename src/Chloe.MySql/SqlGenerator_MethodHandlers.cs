using Chloe.RDBMS;
using System.Reflection;

namespace Chloe.MySql
{
    partial class SqlGenerator : SqlGeneratorBase
    {
        static Dictionary<string, IMethodHandler[]> InitMethodHandlers()
        {
            var methodHandlerMap = new Dictionary<string, List<IMethodHandler>>();

            var methodHandlerTypes = Assembly.GetExecutingAssembly().GetTypes().Where(a => a.IsClass && !a.IsAbstract && typeof(IMethodHandler).IsAssignableFrom(a) && a.Name.EndsWith("_Handler") && a.GetConstructor(Type.EmptyTypes) != null);

            foreach (Type methodHandlerType in methodHandlerTypes)
            {
                string handleMethodName = methodHandlerType.Name.Substring(0, methodHandlerType.Name.Length - "_Handler".Length);

                List<IMethodHandler> methodHandlers;
                if (!methodHandlerMap.TryGetValue(handleMethodName, out methodHandlers))
                {
                    methodHandlers = new List<IMethodHandler>();
                    methodHandlerMap.Add(handleMethodName, methodHandlers);
                }

                methodHandlers.Add((IMethodHandler)Activator.CreateInstance(methodHandlerType));
            }

            Dictionary<string, IMethodHandler[]> ret = new Dictionary<string, IMethodHandler[]>(methodHandlerMap.Count);
            foreach (var kv in methodHandlerMap)
            {
                ret.Add(kv.Key, kv.Value.ToArray());
            }

            return ret;
        }
    }
}
