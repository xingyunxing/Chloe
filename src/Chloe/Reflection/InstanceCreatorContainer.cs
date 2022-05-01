using System.Reflection;

namespace Chloe.Reflection
{
    public class InstanceCreatorContainer
    {
        static readonly Dictionary<ConstructorInfo, InstanceCreator> Cache = new Dictionary<ConstructorInfo, InstanceCreator>();
        public static InstanceCreator Get(ConstructorInfo constructor)
        {
            InstanceCreator creator = null;
            if (!Cache.TryGetValue(constructor, out creator))
            {
                lock (Cache)
                {
                    if (!Cache.TryGetValue(constructor, out creator))
                    {
                        creator = DefaultDelegateFactory.Instance.CreateCreator(constructor);
                        Cache.Add(constructor, creator);
                    }
                }
            }

            return creator;
        }
    }
}
