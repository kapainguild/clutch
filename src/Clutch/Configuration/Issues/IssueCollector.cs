using System.Collections.Generic;
using System.Linq;

namespace Clutch.Configuration.Issues
{
    public class IssueCollector
    {
        private readonly List<IIssue> _issues = new List<IIssue>();

        public void Add<TSource, TArgs>(Issue<TSource, TArgs> issue)
        {
            _issues.Add(issue);
        }

        public IEnumerable<IIssue> GetFiltered()
        {
            return _issues.ToArray();
        }

        public void ThrowIfAnyPendingErrors()
        {
            var first = _issues.FirstOrDefault(s => s.Severity == IssueSeverity.Error);
            if (first != null)
                throw new ClutchConfigurationException(first, GetFiltered());
        }
    }
}
