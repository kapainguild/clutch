using System;
using Clutch.Configuration.Issues;
using Xunit;

namespace Clutch.Tests.Helpers
{
    public class IssueChecker<TSource, TArgs>
    {
        public IssueDeclaration<TSource, TArgs> Declaration { get; set; }

        public Func<TSource, bool> SourceFilter { get; set; }

        public Func<TArgs, bool> ArgsFilter { get; set; }

        public string Message { get; set; }

        public void Check(IIssue issueValue)
        {
            if (issueValue is Issue<TSource, TArgs> issue)
            {
                Assert.Equal(Declaration, issue.IssueDeclaration);

                if (SourceFilter != null)
                    Assert.True(SourceFilter(issue.Source), "Unexpected Source of the issue");

                if (ArgsFilter != null)
                    Assert.True(ArgsFilter(issue.Args), "Unexpected arguments of the issue");

                if (Message != null)
                    Assert.Equal(Message, issue.Message);
            }
            else
            {
                Assert.Equal(typeof(Issue<TSource, TArgs>), issueValue.GetType());
            }
        }
    }
}
