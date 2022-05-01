using System.Reflection;

namespace Chloe.Reflection
{
    public class MemberGetterContainer
    {
        static readonly Dictionary<MemberInfo, MemberGetter> Cache = new Dictionary<MemberInfo, MemberGetter>();
        public static MemberGetter Get(MemberInfo memberInfo)
        {
            MemberGetter getter = null;
            if (!Cache.TryGetValue(memberInfo, out getter))
            {
                lock (Cache)
                {
                    if (!Cache.TryGetValue(memberInfo, out getter))
                    {
                        getter = DefaultDelegateFactory.Instance.CreateGetter(memberInfo);
                        Cache.Add(memberInfo, getter);
                    }
                }
            }

            return getter;
        }
    }
}
