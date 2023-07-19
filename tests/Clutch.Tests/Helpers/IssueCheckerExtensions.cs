using System;
using Clutch.Configuration.Issues;

namespace Clutch.Tests.Helpers
{
    public static class IssueCheckerExtensions
    {
        public static IssueChecker<TSource, TArgs> With<TSource, TArgs>(this IssueDeclaration<TSource, TArgs> declaration, Func<TSource, bool> sourceFilter, Func<TArgs, bool> argsFilter)
        {
            return new IssueChecker<TSource, TArgs>
                   {
                       Declaration = declaration,
                       SourceFilter = sourceFilter,
                       ArgsFilter = argsFilter
                   };
        }

        public static IssueChecker<TSource, TArgs> WithAny<TSource, TArgs>(this IssueDeclaration<TSource, TArgs> declaration)
        {
            return new IssueChecker<TSource, TArgs>
                   {
                       Declaration = declaration,
                   };
        }

        public static IssueChecker<TSource, TArgs> WithSource<TSource, TArgs>(this IssueDeclaration<TSource, TArgs> declaration, Func<TSource, bool> sourceFilter)
        {
            return new IssueChecker<TSource, TArgs>
                   {
                       Declaration = declaration,
                       SourceFilter = sourceFilter,
                   };
        }

        public static IssueChecker<TSource, TArgs> WithArgs<TSource, TArgs>(this IssueDeclaration<TSource, TArgs> declaration, Func<TArgs, bool> argsFilter)
        {
            return new IssueChecker<TSource, TArgs>
                   {
                       Declaration = declaration,
                       ArgsFilter = argsFilter
                   };
        }

        public static IssueChecker<TSource, TArgs> AndMessage<TSource, TArgs>(this IssueChecker<TSource, TArgs> checker, string message)
        {
            checker.Message = message;
            return checker;
        }
    }
}
