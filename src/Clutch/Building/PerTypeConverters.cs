using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Clutch.Building
{
    internal static class PerTypeConverters<TFrom, TTo>
    {
        private static MethodInfo s_converter;

        public static MethodInfo Get(Expression<Func<TFrom, TTo>> expression)
        {
            if (s_converter == null)
                s_converter = TypeInfoHelper.GetInstanceMethod(expression);

            return s_converter;
        }
    }
}
