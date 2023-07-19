using Clutch.Tests.Helpers;
using Clutch.Tests.Models;
using Xunit;

namespace Clutch.Tests
{
    public class ConfigurationErrorsTest
    {
        [Fact]
        public void PropertyNotAvailableOnType() => 
            Checker.ConfigurationFails(c => c.Entity<IRoot>().Property("NotAvailable"),
                                       CoreIssues.PropertyNotFound.With(s => s.Type == typeof(IRoot), args => args == "NotAvailable"));

    }
}
