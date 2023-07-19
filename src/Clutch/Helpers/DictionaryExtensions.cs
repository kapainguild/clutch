using System;
using System.Collections.Generic;

namespace Clutch.Helpers
{
    internal static class DictionaryExtensions
    {
        public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> creator)
        {
            if (dictionary.TryGetValue(key, out var result))
            {
                return result;
            }
            result = creator();
            dictionary.Add(key, result);
            return result;
        }
    }
}
