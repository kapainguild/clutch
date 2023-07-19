using System;
using System.Runtime.CompilerServices;

namespace Clutch.Configuration.Issues
{
    class IssueDeclaration
    {
        public static IssueDeclaration<TSource, TArgs> Error<TSource, TArgs>(Func<TSource, TArgs, string> messageGetter, [CallerMemberName]string declarationName = null) =>
            new IssueDeclaration<TSource, TArgs>(declarationName, messageGetter, IssueSeverity.Error);

        public static IssueDeclaration<TSource, TArgs> Warning<TSource, TArgs>(Func<TSource, TArgs, string> messageGetter, [CallerMemberName]string declarationName = null) =>
            new IssueDeclaration<TSource, TArgs>(declarationName, messageGetter, IssueSeverity.Warning);

        public static IssueDeclaration<TSource, TArgs> Info<TSource, TArgs>(Func<TSource, TArgs, string> messageGetter, [CallerMemberName]string declarationName = null) =>
            new IssueDeclaration<TSource, TArgs>(declarationName, messageGetter, IssueSeverity.Info);
    }
}
