using Clutch.Tests.Helpers;
using Clutch.Tests.Models;
using Xunit;

namespace Clutch.Tests
{

    public class InterfaceDesignTest
    {
        [Fact]
        public void TheFirstTest()
        {
            var context = Checker.BuildsWithoutIssues(c => c.Entity<IRoot>());

            var root = context.Create<IRoot>();
            root.RootInt = 42;
            root.RootString = "42";
            Assert.Equal(42, root.RootInt);
            Assert.Equal("42", root.RootString);
        }

        [Fact]
        public void CreateWithFactoryMethod()
        {
            var context = Checker.BuildsWithoutIssues(c => c.Entity<IRoot>());

            var factoryMethod = context.GetFactoryMethod<IRoot>();
            var root = factoryMethod();
            root.RootInt = 42;
            Assert.Equal(42, root.RootInt);
        }


        [Fact]
        public void TypeNotFoundViaCreate()
        {
            var context = Checker.BuildsWithoutIssues(c => c.Entity<IRoot>());
            var e = Assert.Throws<ClutchRuntimeException>(() => context.Create<IDerived1>());
            Assert.Contains(nameof(IDerived1), e.Message);
        }

        [Fact]
        public void TypeNotFoundViaFactoryMethod()
        {
            var context = Checker.BuildsWithoutIssues(c => c.Entity<IRoot>());
            var e = Assert.Throws<ClutchRuntimeException>(() => context.GetFactoryMethod<IDerived1>());
            Assert.Contains(nameof(IDerived1), e.Message);
        }

        [Fact]
        public void MultiInheritanceIsNotSupportedCase0()
        {
            Checker.ConfigurationFails(c =>
                                                     {
                                                         c.Entity<IDerived1>();
                                                         c.Entity<IDerived2>();
                                                         c.Entity<IDerivedFinal>();
                                                     },
                                                     CoreIssues.MultiInheritanceIsNotSupported.With(s => s.Type == typeof(IDerivedFinal),
                                                                                                    a => a.type1 == typeof(IDerived1) && a.type2 == typeof(IDerived2)));
        }

        interface ICase1Parent1 { }
        interface ICase1Parent2 { }
        interface ICase1Parent2NotEntity : ICase1Parent2 { }
        interface ICase1Final : ICase1Parent1, ICase1Parent2NotEntity { }

        [Fact]
        public void MultiInheritanceIsNotSupportedCase1()
        {
            Checker.ConfigurationFails(c =>
                                       {
                                           c.Entity<ICase1Parent1>();
                                           c.Entity<ICase1Parent2>();
                                           c.Entity<ICase1Final>();
                                       },
                                       CoreIssues.MultiInheritanceIsNotSupported.WithAny());
        }

        interface ICase2Parent { }
        interface ICase2Parent2 : ICase2Parent { }
        interface ICase2Final : ICase2Parent, ICase2Parent2 { }


        [Fact]
        public void MultiInheritanceIsNotSupportedCase2()
        {
            Checker.BuildsWithoutIssues(c =>
                                       {
                                           c.Entity<ICase2Parent>();
                                           c.Entity<ICase2Final>();
                                       });
        }

        [Fact]
        public void MultiInheritanceIsNotSupportedCase2AllEntities()
        {
            Checker.BuildsWithoutIssues(c =>
                                        {
                                            c.Entity<ICase2Parent>();
                                            c.Entity<ICase2Parent2>();
                                            c.Entity<ICase2Final>();
                                        });
        }

        interface IPropertyCollision
        {
            int Value { get; set; }
        }

        interface IPropertyCollisionDerived : IPropertyCollision
        {
            new int Value { get; set; }
        }

        [Fact]
        public void PropertyCollisionOnInterfaceIsHandled()
        {
            var ctx = Checker.BuildsWithoutIssues(c =>
                                        {
                                            c.Entity<IPropertyCollision>();
                                            c.Entity<IPropertyCollisionDerived>();
                                        });
            var obj = ctx.Create<IPropertyCollisionDerived>();
            obj.Value = 24;
            Assert.Equal(24, ((IPropertyCollision)obj).Value);
        }

        interface IPropertyCollision2
        {
            int Value { get; set; }
        }

        interface IPropertyCollisionDerived2 : IPropertyCollision2
        {
            new string Value { get; set; }
        }

        [Fact]
        public void PropertyCollisionOnInterfaceIsHandled2()
        {
            Checker.ConfigurationFails(c =>
                                       {
                                           c.Entity<IPropertyCollision2>();
                                           c.Entity<IPropertyCollisionDerived2>();
                                       },
                                       CoreIssues.PropertyCollisionBySignatureIsNotSupported.With(s => s.Type == typeof(IPropertyCollisionDerived2),
                                                                                                  a => a.property1 == "String IPropertyCollisionDerived2.Value" && a.property2 == "Int32 IPropertyCollision2.Value"));
        }

    }
}
