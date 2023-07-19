using Clutch.Tests.Helpers;
using Clutch.Tests.Models;
using Xunit;

namespace Clutch.Tests
{
    public class DoublePropertySetTest
    {
        [Fact]
        public void DoubleSetOnProperty()
        {
            Checker.BuildsWithWarning(c => c.Entity<RootClass>().Property(s => s.RootInt).UsePropertyAccessMode(PropertyAccessMode.Field).UsePropertyAccessMode(PropertyAccessMode.Property),
                                      CoreIssues.OptionIsSetMoreThanOnceOnProperty.WithArgs(s => s.optionName == "UsePropertyAccessMode" && (PropertyAccessMode)s.lastValue == PropertyAccessMode.Property));
        }

        [Fact]
        public void DoubleSetOnAnyProperty()
        {
            Checker.BuildsWithWarning(c => c.Entity<RootClass>().AnyProperty(true).UsePropertyAccessMode(PropertyAccessMode.Field).UsePropertyAccessMode(PropertyAccessMode.Property),
                                      CoreIssues.OptionIsSetMoreThanOnceOnProperty.WithArgs(s => s.optionName == "UsePropertyAccessMode" && (PropertyAccessMode)s.lastValue == PropertyAccessMode.Property));
        }

        [Fact]
        public void DoubleSetOnAnyDeclaredProperty()
        {
            Checker.BuildsWithWarning(c => c.Entity<RootClass>().AnyProperty(false).UsePropertyAccessMode(PropertyAccessMode.Field).UsePropertyAccessMode(PropertyAccessMode.Property),
                                      CoreIssues.OptionIsSetMoreThanOnceOnProperty.WithArgs(s => s.optionName == "UsePropertyAccessMode" && (PropertyAccessMode)s.lastValue == PropertyAccessMode.Property));
        }

        [Fact]
        public void DoubleSetOnAnyPropertyOfAnyEntity()
        {
            Checker.BuildsWithWarning(c => c.AnyEntityType().AnyProperty().UsePropertyAccessMode(PropertyAccessMode.Field).UsePropertyAccessMode(PropertyAccessMode.Property),
                                      CoreIssues.OptionIsSetMoreThanOnceOnProperty.WithArgs(s => s.optionName == "UsePropertyAccessMode" && (PropertyAccessMode)s.lastValue == PropertyAccessMode.Property));
        }
    }
}
