using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Clutch.Configuration.Issues;
using Clutch.CoreExtensions.NotifyPropertyChanged;
using Clutch.Tests.Helpers;
using Clutch.Tests.Models;
using Xunit;

namespace Clutch.Tests
{
    public class NotifyPropertyChangedTest
    {
        [Fact]
        public void EnableNotifyPropertyChanged()
        {
            var context = Checker.BuildsWithoutIssues(c =>
                                                      {
                                                          c.UseNotifyPropertyChanged(NotifyPropertyChangedBehavior.ImplementOnAllEntities);
                                                          c.Entity<IRoot>();
                                                      });
            var root = context.Create<IRoot>();
            ClutchAssert.ImplementsNotifyPropertyChanged(root, out var npc);

            Assert.PropertyChanged(npc, nameof(IRoot.RootInt), () => root.RootInt = 42);
            Assert.PropertyChanged(npc, nameof(IRoot.RootString), () => root.RootString = "42");
        }

        [Fact]
        public void NotifyPropertyChangedRaisedAfterSet()
        {
            var context = Checker.BuildsWithoutIssues(c =>
                                                      {
                                                          c.UseNotifyPropertyChanged(NotifyPropertyChangedBehavior.ImplementOnAllEntities);
                                                          c.Entity<IRoot>();
                                                      });
            var root = context.Create<IRoot>();
            ClutchAssert.ImplementsNotifyPropertyChanged(root, out var npc);

            npc.PropertyChanged += (s, e) =>
                                   {
                                       Assert.Equal(42, root.RootInt);
                                   };
            root.RootInt = 42;
        }

        [Fact]
        public void ImplicitEnablingOfNpcToAllEntities()
        {
            var ctx = Checker.BuildsWithWarning(c =>
                                                {
                                                    c.Entity<IRoot>();
                                                    c.Entity<IRoot2>(e => e.Property(p => p.Root2String).EnableNotifyPropertyChanged(false));
                                                },
                                                CoreIssues.ExtensionIsNotEnabledExplicitely.With(s => s == IssueSource.Context,
                                                                                                 args => args.enableExtensionMethod == "UseNotifyPropertyChanged" &&
                                                                                                         args.extensionOptionCall == "EnableNotifyPropertyChanged"));
            // check wheather it was enabled on all properties

            var root = ctx.Create<IRoot>();
            var root2 = ctx.Create<IRoot2>();

            ClutchAssert.RaisesPropertyChanged(root, r => r.RootInt, r => r.RootInt = 42);
            ClutchAssert.RaisesPropertyChanged(root, r => r.RootString, r => r.RootString = "42");

            ClutchAssert.RaisesPropertyChanged(root2, r => r.Root2Int, r => r.Root2Int = 42);
            ClutchAssert.DoesNotRaisePropertyChanged(root2, r => r.Root2String, r => r.Root2String = "42");
        }

        [Fact]
        public void DisableNpcWithNoPropertyCalls()
        {
            var ctx = Checker.BuildsWithoutIssues(c =>
                                                  {
                                                      c.UseNotifyPropertyChanged(NotifyPropertyChangedBehavior.Disable);
                                                      c.Entity<IRoot>();
                                                  });

            ClutchAssert.DoesNotImplementNotifyPropertyChanged(ctx.Create<IRoot>());
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void DisableNpcWithWarningOnUsage(bool enablePropertychanged)
        {
            var ctx = Checker.BuildsWithWarning(c =>
                                                {
                                                    c.UseNotifyPropertyChanged(NotifyPropertyChangedBehavior.Disable);
                                                    c.Entity<IRoot>().Property(s => s.RootInt).EnableNotifyPropertyChanged(enablePropertychanged);
                                                },
                                                CoreIssues.ExtensionIsUsedWhileDisabled.With(s => s == IssueSource.Context,
                                                                                             args => args.enableExtensionMethod == "UseNotifyPropertyChanged" &&
                                                                                                     args.extensionOptionCall == "EnableNotifyPropertyChanged"));

            ClutchAssert.DoesNotImplementNotifyPropertyChanged(ctx.Create<IRoot>());
        }

        [Fact]
        public void ImplementOnlyOnEntitiesWithEnabledProperties()
        {
            var ctx = Checker.BuildsWithoutIssues(c =>
                                                  {
                                                      c.UseNotifyPropertyChanged(NotifyPropertyChangedBehavior.ImplementOnlyOnEntitiesWithEnabledProperties);
                                                      c.Entity<IRoot>();
                                                      c.Entity<IRoot2>().Property(s => s.Root2Int).EnableNotifyPropertyChanged(true);
                                                  });

            ClutchAssert.DoesNotImplementNotifyPropertyChanged(ctx.Create<IRoot>());

            ClutchAssert.RaisesPropertyChanged(ctx.Create<IRoot2>(), r => r.Root2Int, r => r.Root2Int = 42);
            ClutchAssert.DoesNotRaisePropertyChanged(ctx.Create<IRoot2>(), r => r.Root2String, r => r.Root2String = "42");
        }

        [Fact]
        public void ImplementOnlyOnEntitiesWithEnabledPropertiesAndNoEntities()
        {
            var ctx = Checker.BuildsWithWarning(c =>
                                                {
                                                    c.UseNotifyPropertyChanged(NotifyPropertyChangedBehavior.ImplementOnlyOnEntitiesWithEnabledProperties);
                                                    c.Entity<IRoot2>().Property(s => s.Root2Int).EnableNotifyPropertyChanged(false);
                                                },
                                                CoreIssues.CallIsRedundantOnProperty.With(s => s.PropertyName == nameof(IRoot2.Root2Int), args => args == "EnableNotifyPropertyChanged"));

            ClutchAssert.DoesNotImplementNotifyPropertyChanged(ctx.Create<IRoot2>());
        }

        [Fact]
        public void ImplementOnAllEntitiesWithEnabledProperties()
        {
            var ctx = Checker.BuildsWithWarning(c =>
                                                {
                                                    c.UseNotifyPropertyChanged(NotifyPropertyChangedBehavior.ImplementOnAllEntities);
                                                    c.Entity<IRoot2>().Property(s => s.Root2Int).EnableNotifyPropertyChanged(true);
                                                },
                                                CoreIssues.CallIsRedundantOnProperty.With(s => s.PropertyName == nameof(IRoot2.Root2Int), args => args == "EnableNotifyPropertyChanged"));

            ClutchAssert.ImplementsNotifyPropertyChanged(ctx.Create<IRoot2>());
        }

        [Fact]
        public void PropertyChangedSupportsMultipleSubscribtions()
        {
            var ctx = Checker.BuildsWithoutIssues(c =>
                                                  {
                                                      c.UseNotifyPropertyChanged(NotifyPropertyChangedBehavior.ImplementOnAllEntities);
                                                      c.Entity<IRoot>();
                                                  });

            var root = ctx.Create<IRoot>();
            ClutchAssert.ImplementsNotifyPropertyChanged(root, out var npc);

            int handler1Count = 0;
            int handler2Count = 0;

            void Handler1(object sender, PropertyChangedEventArgs args)
            {
                Assert.Equal(root, sender);
                Assert.Equal(nameof(IRoot.RootInt), args.PropertyName);
                handler1Count++;
            }

            void Handler2(object sender, PropertyChangedEventArgs args)
            {
                Assert.Equal(root, sender);
                Assert.Equal(nameof(IRoot.RootInt), args.PropertyName);
                handler2Count++;
            }

            npc.PropertyChanged += Handler1;
            root.RootInt = 42;
            Assert.Equal(1, handler1Count);

            npc.PropertyChanged += Handler2;
            root.RootInt = 43;
            Assert.Equal(2, handler1Count);
            Assert.Equal(1, handler2Count);

            npc.PropertyChanged -= Handler1;
            root.RootInt = 44;
            Assert.Equal(2, handler1Count);
            Assert.Equal(2, handler2Count);

            npc.PropertyChanged -= Handler2;
            root.RootInt = 45;
            Assert.Equal(2, handler1Count);
            Assert.Equal(2, handler2Count);
        }


        interface IWithNotify : INotifyPropertyChanged
        {
            int Value { get; set; }
        }

        [Fact]
        public void InterfaceWithINotifyPropertyChangedIsSupported()
        {
            var ctx = Checker.BuildsWithoutIssues(c =>
                                                  {
                                                      c.UseNotifyPropertyChanged(NotifyPropertyChangedBehavior.ImplementOnAllEntities);
                                                      c.Entity<IWithNotify>();
                                                  });

            ClutchAssert.RaisesPropertyChanged(ctx.Create<IWithNotify>(), r => r.Value, r => r.Value = 42);
        }


        [Fact]
        public void EnableNotifyPropertyChangedWithClass()
        {
            var ctx = Checker.BuildsWithoutIssues(c =>
                                                  {
                                                      c.UseNotifyPropertyChanged(NotifyPropertyChangedBehavior.ImplementOnAllEntities);
                                                      c.Entity<RootClass>();
                                                  });

            ClutchAssert.RaisesPropertyChanged(ctx.Create<RootClass>(), r => r.RootInt, r => r.RootInt = 42);
        }


        class ClassWithNotify : INotifyPropertyChanged
        {
            public virtual int Value { get; set; }

            public event PropertyChangedEventHandler PropertyChanged;
        }

        [Fact]
        public void ErrorWithClassThatImplementsPropertyChanged()
        {
            Checker.ConfigurationFails(c =>
                                       {
                                           c.UseNotifyPropertyChanged(NotifyPropertyChangedBehavior.ImplementOnAllEntities);
                                           c.Entity<ClassWithNotify>();
                                       },
                                       NotifyPropertyChangedIssues.RaisePropertyChangedNotFound.WithSource(s => s.Type == typeof(ClassWithNotify)));
        }

        class ClassWithNotifyAndRaise : INotifyPropertyChanged
        {
            private Action<string> _raiseCalled;

            public virtual int Value { get; set; }

            public event PropertyChangedEventHandler PropertyChanged;

            public void SetChecker(Action<string> raiseCalled)
            {
                _raiseCalled = raiseCalled;
            }

            private void RaisePropertyChanged(string name) => _raiseCalled(name);
        }

        [Fact]
        public void ClassThatImplementsPropertyChangedAndRaiseMethod()
        {
            var ctx = Checker.BuildsWithoutIssues(c =>
                                       {
                                           c.UseNotifyPropertyChanged(NotifyPropertyChangedBehavior.ImplementOnAllEntities);
                                           c.Entity<ClassWithNotifyAndRaise>();
                                       });
            int counter = 0;
            var cl = ctx.Create<ClassWithNotifyAndRaise>();
            cl.SetChecker(s =>
                          {
                              if (s == nameof(ClassWithNotifyAndRaise.Value))
                                  counter++;
                          });

            cl.Value = 42;
            Assert.Equal(1, counter);
        }

        class ClassWithPropertyChangedEvent
        {
            public event EventHandler PropertyChanged;
        }

        [Fact]
        public void ErrorWithClassThatHasPropertyChangedEvent()
        {
            Checker.ConfigurationFails(c =>
                                       {
                                           c.UseNotifyPropertyChanged(NotifyPropertyChangedBehavior.ImplementOnAllEntities);
                                           c.Entity<ClassWithPropertyChangedEvent>();
                                       },
                                       NotifyPropertyChangedIssues.ContainsPropertyChangedEvent.WithSource(s => s.Type == typeof(ClassWithPropertyChangedEvent)));
        }
    }
}
