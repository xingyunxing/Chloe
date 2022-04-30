namespace System.Collections.Generic
{
    static class DictionaryExtension
    {
        /// <summary>
        /// 如果找到 key 对应的value，则返回 value。否则返回 default(TValue)
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dic"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static TValue FindValue<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key)
        {
            TValue value;
            if (!dic.TryGetValue(key, out value))
                value = default(TValue);

            return value;
        }

        public static Dictionary<TKey, TValue> Clone<TKey, TValue>(this Dictionary<TKey, TValue> source)
        {
            Dictionary<TKey, TValue> ret = Clone<TKey, TValue>(source, source.Count);
            return ret;
        }
        public static Dictionary<TKey, TValue> Clone<TKey, TValue>(this Dictionary<TKey, TValue> source, int capacity)
        {
            Dictionary<TKey, TValue> ret = new Dictionary<TKey, TValue>(capacity);

            foreach (var kv in source)
            {
                ret.Add(kv.Key, kv.Value);
            }

            return ret;
        }
    }
}
