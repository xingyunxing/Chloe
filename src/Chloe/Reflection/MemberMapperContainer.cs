using System.Reflection;

namespace Chloe.Reflection
{
    public class MemberMapperContainer
    {
        static readonly Dictionary<MemberInfo, MemberMapper> Cache = new Dictionary<MemberInfo, MemberMapper>();
        public static MemberMapper Get(MemberInfo memberInfo)
        {
            MemberMapper mapper = null;
            if (!Cache.TryGetValue(memberInfo, out mapper))
            {
                lock (Cache)
                {
                    if (!Cache.TryGetValue(memberInfo, out mapper))
                    {
                        mapper = DefaultDelegateFactory.Instance.CreateMapper(memberInfo);
                        Cache.Add(memberInfo, mapper);
                    }
                }
            }

            return mapper;
        }
    }
}
