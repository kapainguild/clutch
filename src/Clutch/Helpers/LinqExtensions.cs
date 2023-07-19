using System;
using System.Collections.Generic;

namespace Clutch.Helpers
{
    internal static class LinqExtensions
    {
        internal static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var item in enumerable)
                action(item);
        }
    }
}
