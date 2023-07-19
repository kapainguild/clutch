using Clutch.Tests.Helpers;
using Clutch.Tests.Models;
using Xunit;

namespace Clutch.Tests
{
    public class ClassDesignTest
    {
        class ClassWithoutParameterlessConstructor
        {
            public ClassWithoutParameterlessConstructor(int i)
            {
            }
        }

        [Fact]
        public void ParameterlessConstructorNotFound()
        {
            Checker.ConfigurationFails(c => c.Entity<ClassWithoutParameterlessConstructor>(),
                                       CoreIssues.ParameterlessConstructorNotFound.WithSource(s => s.Type == typeof(ClassWithoutParameterlessConstructor)));
        }

        class ClassWithPrivateConstructor
        {
            public int _someField;

            private ClassWithPrivateConstructor()
            {
                _someField = 42;
            }
        }

        [Fact]
        public void PrivateConstructorAreSupported()
        {
            var ctx = Checker.BuildsWithoutIssues(c => c.Entity<ClassWithPrivateConstructor>());
            var obj = ctx.Create<ClassWithPrivateConstructor>();
            Assert.Equal(42, obj._someField);
        }

        sealed class SealedClass
        {
        }

        [Fact]
        public void SealedClassesAreNotSupported()
        {
            Checker.ConfigurationFails(c => c.Entity<SealedClass>(),
                                       CoreIssues.SealedClassesAreNotSupported.WithSource(s => s.Type == typeof(SealedClass)));
        }

        class NotSupportedClass { }

        class NotSupportedClassEntity
        {
            public virtual NotSupportedClass Prop { get; set; }
        }

        [Fact]
        public void NotSupportedClassTest()
        {
            Checker.ConfigurationFails(c => c.Entity<NotSupportedClassEntity>(),
                                       CoreIssues.TypeIsNotSupported.WithArgs(s => s == typeof(NotSupportedClass)));
        }

        struct NotSupportedStruct { }

        class NotSupportedNullableEntity
        {
            public virtual NotSupportedStruct? Prop { get; set; }
        }

        [Fact]
        public void NotSupportedNullableTest()
        {
            Checker.ConfigurationFails(c => c.Entity<NotSupportedNullableEntity>(),
                                       CoreIssues.TypeIsNotSupported.WithArgs(s => s == typeof(NotSupportedStruct)));
        }
    }
}
