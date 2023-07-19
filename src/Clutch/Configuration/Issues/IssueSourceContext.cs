using System.Diagnostics;

namespace Clutch.Configuration.Issues
{
    public class IssueSourceContext<TSource>
    {
        public TSource Source { get; }

        public IssueCollector Collector { get; }

        public CallerInfo CallerInfo { get; protected set; }

        public IssueSourceContext<TTargetSource> Create<TTargetSource>(TTargetSource source, bool collectCallerInfo = true)
        {
            return new IssueSourceContext<TTargetSource>(source, Collector, collectCallerInfo);
        }

        public IssueSourceContext(TSource source, IssueCollector collector, bool collectCallerInfo)
        {
            Collector = collector;
            Source = source;
            if (collectCallerInfo)
                CallerInfo = CallerInfo.GetCurrent();
        }

        public void Issue<TArgs>(IssueDeclaration<TSource, TArgs> issueDeclaration, TArgs args, CallerInfo callerInfo)
        {
            CreateAndProcessIssue(issueDeclaration, args, callerInfo ?? CallerInfo ?? CallerInfo.GetCurrent());
        }

        public ClutchConfigurationException Exception<TArgs>(IssueDeclaration<TSource, TArgs> issueDeclaration, TArgs args)
        {
            var issue = CreateAndProcessIssue(issueDeclaration, args, CallerInfo.GetCurrent());

            return new ClutchConfigurationException(issue, Collector.GetFiltered());
        }

        private Issue<TSource, TArgs> CreateAndProcessIssue<TArgs>(IssueDeclaration<TSource, TArgs> issueDeclaration, TArgs args, CallerInfo callerInfo)
        {
            var issue = new Issue<TSource, TArgs>(issueDeclaration, Source, args, callerInfo);
            Collector.Add(issue);

            Trace.WriteLine($"{callerInfo.GetTracePrefix()}{issue.Severity}: {issue.DeclarationName}: {issue.Message}");

            return issue;
        }
    }
}
