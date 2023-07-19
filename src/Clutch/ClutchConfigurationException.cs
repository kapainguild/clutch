using System.Collections.Generic;
using Clutch.Configuration.Issues;

namespace Clutch
{
    public class ClutchConfigurationException : ClutchException
    {
        public IIssue Issue { get; }

        public IEnumerable<IIssue> AllIssues { get; }

        public ClutchConfigurationException(IIssue issue, IEnumerable<IIssue> issues) : base(issue.Message)
        {
            Issue = issue;
            AllIssues = issues;
        }
    }
}
