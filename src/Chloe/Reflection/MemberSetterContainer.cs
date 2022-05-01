using System.Reflection;

namespace Chloe.Reflection
{
    public class MemberSetterContainer
    {
        static readonly Dictionary<MemberInfo, MemberSetter> Cache = new Dictionary<MemberInfo, MemberSetter>();
        public static MemberSetter Get(MemberInfo memberInfo)
        {
            MemberSetter setter = null;
            if (!Cache.TryGetValue(memberInfo, out setter))
            {
                lock (Cache)
                {
                    if (!Cache.TryGetValue(memberInfo, out setter))
                    {
                        setter = DefaultDelegateFactory.Instance.CreateSetter(memberInfo);
                        Cache.Add(memberInfo, setter);
                    }
                }
            }

            return setter;
        }
    }
}
