using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Clutch.CoreExtensions.NotifyPropertyChanged;
using Clutch.Tests.Helpers;
using Clutch.Tests.Models;
using Xunit;

namespace Clutch.Tests
{
    public class PropertyAccessForInterfacesTest
    {
        [Theory]
        [InlineData(PropertyAccessMode.Property)]
        [InlineData(PropertyAccessMode.FieldForGetterAndInitializationPropertyForSetter)]
        [InlineData(PropertyAccessMode.FieldForGetterPropertyForSetter)]
        public void NotSupportedAccessMode(PropertyAccessMode mode)
        {
            var ctx = Checker.BuildsWithWarning(c => c.Entity<IRoot>().Property(s => s.RootInt).UsePropertyAccessMode(mode),
                                                CoreIssues.OnlyFieldAccessModeIsAllowedOnInterfaceOrAbstractProperty.With(s => s.PropertyName == nameof(IRoot.RootInt), args => args == mode));

            var root = ctx.Create<IRoot>();
            root.RootInt = 42;
            Assert.Equal(42, root.RootInt);
        }

        [Fact]
        public void OnlySupportedAccessMode()
        {
            Checker.BuildsWithoutIssues(c => c.Entity<IRoot>().Property(s => s.RootInt).UsePropertyAccessMode(PropertyAccessMode.Field));
        }

        interface IInterfaceWithCollision
        {
            int _test { get; set; }

            int Test { get; set; }
        }

        [Fact]
        public void InterfaceWithCollision()
        {
            var ctx = Checker.BuildsWithoutIssues(c => c.Entity<IInterfaceWithCollision>());

            var obj = ctx.Create<IInterfaceWithCollision>();
            CheckInterface(obj);

            Assert.NotNull(obj.GetType().GetField("_test1", BindingFlags.NonPublic | BindingFlags.Instance));
            Assert.NotNull(obj.GetType().GetField("__test", BindingFlags.NonPublic | BindingFlags.Instance));
        }

        [Fact]
        public void InterfaceWithCollisionAndImplicitName()
        {
            var ctx = Checker.BuildsWithoutIssues(c => c.Entity<IInterfaceWithCollision>().Property(s => s.Test).HasField("_myName"));

            var obj = ctx.Create<IInterfaceWithCollision>();
            CheckInterface(obj);

            Assert.NotNull(obj.GetType().GetField("_myName", BindingFlags.NonPublic | BindingFlags.Instance));
            Assert.NotNull(obj.GetType().GetField("__test", BindingFlags.NonPublic | BindingFlags.Instance));
        }

        [Fact]
        public void InterfaceFailsWithReusedName()
        {
            Checker.ConfigurationFails(c => c.Entity<IInterfaceWithCollision>().Property(s => s.Test).HasField("_test"),
                                                 CoreIssues.MemberWithNameIsAlreadyDeclared.With(s => s.PropertyName == nameof(IInterfaceWithCollision.Test),
                                                                                                 args => args.memberName == "_test" && args.declaration == "RuntimePropertyInfo _test"));
        }

        private static void CheckInterface(IInterfaceWithCollision obj)
        {
            obj.Test = 1;
            obj._test = 2;
            Assert.Equal(1, obj.Test);
            Assert.Equal(2, obj._test);
        }
    }
}
