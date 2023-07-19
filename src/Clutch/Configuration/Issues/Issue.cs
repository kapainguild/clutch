
namespace Clutch.Configuration.Issues
{
    public class Issue<TSource, TArgs> : IIssue
    {
        public IssueDeclaration<TSource, TArgs> IssueDeclaration { get; }

        public CallerInfo CallerInfo { get; }

        public TArgs Args { get; }

        public TSource Source { get; }

        public string Message => IssueDeclaration.MessageGetter(Source, Args);

        public string DeclarationName => IssueDeclaration.Name;

        public IssueSeverity Severity => IssueDeclaration.Severity;

        public Issue(IssueDeclaration<TSource, TArgs> issueDeclaration, TSource source, TArgs args, CallerInfo callerInfo)
        {
            IssueDeclaration = issueDeclaration;
            Source = source;
            Args = args;
            CallerInfo = callerInfo;
        }
    }
}
