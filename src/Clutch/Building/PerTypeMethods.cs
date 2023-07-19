using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;

namespace Clutch.Building
{
    internal static class PerTypeMethods<T>
    {
        private static MethodInfo s_reader;

        private static MethodInfo s_writerWithProperty;

        private static MethodInfo s_writer;

        private static MethodInfo s_equals;

        public static MethodInfo GetReader(Expression<Func<T>> expression)
        {
            if (s_reader == null)
                s_reader = TypeInfoHelper.GetInstanceMethod(expression);

            return s_reader;
        }

        public static MethodInfo GetWriter(bool withProperty, Expression<Action<Utf8JsonWriter>> expressionWithProperty, Expression<Action<Utf8JsonWriter>> expressionWithoutProperty)
        {
            if (withProperty)
            {
                if (s_writerWithProperty == null)
                    s_writerWithProperty = TypeInfoHelper.GetInstanceMethod(expressionWithProperty);

                return s_writerWithProperty;
            }
            else
            {
                if (s_writer == null)
                    s_writer = TypeInfoHelper.GetInstanceMethod(expressionWithoutProperty);

                return s_writer;
            }
        }

        public static MethodInfo GetEquals(Expression<Func<T, bool>> expression)
        {
            if (s_equals == null)
                s_equals = TypeInfoHelper.GetInstanceMethod(expression);

            return s_equals;
        }
    }
}
