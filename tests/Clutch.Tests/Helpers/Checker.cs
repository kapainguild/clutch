using System;
using System.Linq;
using Xunit;

namespace Clutch.Tests.Helpers
{
    static class Checker
    {
        public static void ConfigurationFails<TSource, TArgs>(Action<ClutchContextBuilder> buildAction, IssueChecker<TSource, TArgs> checker)
        {
            bool thrown = false;
            try
            {
                ClutchContext.CreateContext(buildAction, out _);
            }
            catch (ClutchConfigurationException exception)
            {
                thrown = true;
                Assert.NotNull(exception.Issue);
                Assert.Single(exception.AllIssues);
                Assert.Equal(exception.Issue, exception.AllIssues.First());
                checker.Check(exception.Issue);
            }

            Assert.True(thrown, $"Configuration building does not fails while should");
        }

        public static ClutchContext BuildsWithoutIssues(Action<ClutchContextBuilder> buildAction)
        {
            var result = ClutchContext.CreateContext(buildAction, out var issues);
            Assert.Empty(issues);
            return result;
        }

        public static ClutchContext BuildsWithWarning<TSource, TArgs>(Action<ClutchContextBuilder> buildAction, IssueChecker<TSource, TArgs> checker)
        {
            var result = ClutchContext.CreateContext(buildAction, out var issues);
            Assert.Single(issues);
            checker.Check(issues.First());
            return result;
        }
    }
}
