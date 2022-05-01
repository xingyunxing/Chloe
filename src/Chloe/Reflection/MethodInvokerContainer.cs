using System.Reflection;

namespace Chloe.Reflection
{
    public class MethodInvokerContainer
    {
        static readonly Dictionary<MethodInfo, MethodInvoker> Cache = new Dictionary<MethodInfo, MethodInvoker>();
        public static MethodInvoker Get(MethodInfo method)
        {
            MethodInvoker invoker = null;
            if (!Cache.TryGetValue(method, out invoker))
            {
                lock (Cache)
                {
                    if (!Cache.TryGetValue(method, out invoker))
                    {
                        invoker = DefaultDelegateFactory.Instance.CreateInvoker(method);
                        Cache.Add(method, invoker);
                    }
                }
            }

            return invoker;
        }
    }
}
