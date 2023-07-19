using System;
using Clutch.Configuration;
using Clutch.Configuration.Issues;
using Clutch.Extensibility;

namespace Clutch.Helpers
{
    internal static class InternalExtensions
    {
        public static T SetExtensionOption<T, TValue, TExtension>(this T builder, ConfigOptionDeclaration<TValue> option, TValue value, Func<TExtension> creator)
            where T : IBaseApi where TExtension : IExtension
        {
            builder.GetOrCreateExtension(creator);
            builder.Options.Set(option, value);
            return builder;
        }

        public static T SetOption<T, TValue>(this T builder, ConfigOptionDeclaration<TValue> option, TValue value)
            where T : IBaseApi 
        {
            builder.Options.Set(option, value);
            return builder;
        }

        public static T SetOption<T, TValue>(this T builder, ConfigOptionDeclarationOpenType option, TValue value)
            where T : IBaseApi
        {
            builder.Options.Set(option, value);
            return builder;
        }

        public static IInternalBuilder<TSource> ToInternal<TParent, TSource>(this BaseBuilder<TParent, TSource> builder) where TParent : IInternalBuilder
        {
            return builder;
        }

        public static IInternalBuilder<IssueSource> ToInternal(this ClutchContextBuilder builder)
        {
            return builder;
        }
    }
}
