using System.Collections.Generic;

namespace LazyBones.Extensions
{
    public static class ExtensionForDictionary
    {
        public static T GetValue<T>(this IDictionary<object, object> dictionary, object key)
            where T : new()
        {
            T defaultValue = default(T);
            return GetValue<T>(dictionary, key, defaultValue);
        }

        public static T GetValue<T>(this IDictionary<object, object> dictionary, object key, T defaultValue)
        {
            object valueObj;

            if (!dictionary.TryGetValue(key, out valueObj))
            {
                return defaultValue;
            }
            else
            {
                return (T)valueObj;
            }
        }

        public static TValue GetValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
        {
            TValue value;
            if (dictionary.TryGetValue(key, out value))
                return value;
            return defaultValue;
        }

        public static TValue GetValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue value;
            if (dictionary.TryGetValue(key, out value))
                return value;
            return default(TValue);
        }
    }
}
