using Chloe.Mapper;

namespace Chloe.Reflection
{
    public class MultMemberMapperContainer
    {
        static readonly Dictionary<MultMemberMapperCacheKey, MultMemberMapper> Cache = new Dictionary<MultMemberMapperCacheKey, MultMemberMapper>();

        public static MultMemberMapper GetOrAdd(MultMemberMapperCacheKey key, Func<MultMemberMapper> mapperFactory)
        {
            MultMemberMapper mapper = null;
            if (!Cache.TryGetValue(key, out mapper))
            {
                lock (Cache)
                {
                    if (!Cache.TryGetValue(key, out mapper))
                    {
                        mapper = mapperFactory();
                        Cache.Add(key, mapper);
                    }
                }
            }

            return mapper;
        }
    }

    public struct MultMemberMapperCacheKey : IEquatable<MultMemberMapperCacheKey>
    {
        Type _objectType;
        MapInfo[] _mapInfos;

        public MultMemberMapperCacheKey(Type objectType, MapInfo[] mapInfos)
        {
            this._objectType = objectType;
            this._mapInfos = mapInfos;
        }

        public override bool Equals(object? obj)
        {
            return obj is MultMemberMapperCacheKey other && Equals(other);
        }

        public bool Equals(MultMemberMapperCacheKey other)
        {
            if (this._objectType != other._objectType)
                return false;

            if (this._mapInfos.Length != other._mapInfos.Length)
                return false;

            for (int i = 0; i < this._mapInfos.Length; i++)
            {
                MemberMap memberMap = this._mapInfos[i].MemberMap;
                MemberMap otherMemberMap = other._mapInfos[i].MemberMap;
                if (memberMap.Member != otherMemberMap.Member)
                    return false;

                if (this._mapInfos[i].ReaderDataType != other._mapInfos[i].ReaderDataType)
                    return false;

                if (memberMap.Ordinal != otherMemberMap.Ordinal)
                    return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();

            hash.Add(this._objectType);
            hash.Add(this._mapInfos.Length);

            for (int i = 0; i < this._mapInfos.Length; i++)
            {
                MemberMap memberMap = this._mapInfos[i].MemberMap;
                hash.Add(memberMap.Member);
                hash.Add(this._mapInfos[i].ReaderDataType);
                hash.Add(memberMap.Ordinal);
            }

            return hash.ToHashCode();
        }


    }

}
