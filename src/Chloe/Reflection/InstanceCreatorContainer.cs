using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Chloe.Reflection
{
    public class InstanceCreatorContainer
    {
        static readonly System.Collections.Concurrent.ConcurrentDictionary<ConstructorInfo, InstanceCreator> Cache = new System.Collections.Concurrent.ConcurrentDictionary<ConstructorInfo, InstanceCreator>();
        public static InstanceCreator GetCreator(ConstructorInfo constructor)
        {
            InstanceCreator creator = null;
            if (!Cache.TryGetValue(constructor, out creator))
            {
                lock (constructor)
                {
                    if (!Cache.TryGetValue(constructor, out creator))
                    {
                        creator = DefaultDelegateFactory.Instance.CreateCreator(constructor);
                        Cache.GetOrAdd(constructor, creator);
                    }
                }
            }

            return creator;
        }
    }
}
