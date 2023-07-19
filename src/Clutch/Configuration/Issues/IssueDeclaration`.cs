using System;

namespace Clutch.Configuration.Issues
{
    public class IssueDeclaration<TSource, TArgs>
    {
        private readonly string _declarationName;

        internal Func<TSource, TArgs, string> MessageGetter { get; }

        public IssueSeverity Severity { get; }

        public string Name => $"{_declarationName}";

        public IssueDeclaration(string declarationName, Func<TSource, TArgs, string> messageGetter, IssueSeverity severity)
        {
            _declarationName = declarationName;
            MessageGetter = messageGetter;
            Severity = severity;
        }
    }

    public class EmptyIssueArgs
    {
        public static EmptyIssueArgs Instance { get; } = new EmptyIssueArgs();
    }
}
