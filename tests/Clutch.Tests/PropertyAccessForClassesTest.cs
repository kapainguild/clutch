using System;
using System.Collections.Generic;
using System.Text;
using Clutch.Tests.Helpers;
using Clutch.Tests.Models;
using Xunit;

namespace Clutch.Tests
{
    public class PropertyAccessForClassesTest
    {
        abstract class Abstract
        {
            public int _fieldInteger;
            public string _fieldString;

            public abstract int Value { get; set; }

            public void SomeMethod() { }
        }

        [Theory]
        [InlineData(PropertyAccessMode.Property)]
        [InlineData(PropertyAccessMode.FieldForGetterAndInitializationPropertyForSetter)]
        [InlineData(PropertyAccessMode.FieldForGetterPropertyForSetter)]
        public void NotSupportedAccessModeForAbstractProperty(PropertyAccessMode mode)
        {
            var ctx = Checker.BuildsWithWarning(c => c.Entity<Abstract>().Property(s => s.Value).UsePropertyAccessMode(mode),
                                                CoreIssues.OnlyFieldAccessModeIsAllowedOnInterfaceOrAbstractProperty.With(s => s.PropertyName == nameof(Abstract.Value), args => args == mode));

            var root = ctx.Create<Abstract>();
            root.Value = 42;
            Assert.Equal(42, root.Value);
        }

        [Fact]
        public void FieldSpecificationForAbstractProperty()
        {
            var ctx = Checker.BuildsWithoutIssues(c => c.Entity<Abstract>().Property(s => s.Value).HasField("_fieldInteger"));

            var obj = ctx.Create<Abstract>();
            obj.Value = 42;
            Assert.Equal(42, obj._fieldInteger);
        }

        [Fact]
        public void FieldSpecificationMismatchForAbstractProperty()
        {
            Checker.ConfigurationFails(c => c.Entity<Abstract>().Property(s => s.Value).HasField("_fieldString"),
                                       CoreIssues.PropertyAndFieldTypeMismatch.With(s => s.PropertyName == nameof(Abstract.Value),
                                                                                    args => args.fieldName == "_fieldString" && args.fieldType == typeof(string)));

        }

        [Fact]
        public void FieldSpecificationCollisionForAbstractProperty()
        {
            Checker.ConfigurationFails(c => c.Entity<Abstract>().Property(s => s.Value).HasField("SomeMethod"),
                                       CoreIssues.MemberWithNameIsAlreadyDeclared.With(s => s.PropertyName == nameof(Abstract.Value),
                                                                                    args => args.memberName == "SomeMethod"));

        }

        class SameFieldReference
        {
            public int _field;

            public virtual int Field { get; set; }

            public virtual int Field2 { get; set; }
        }

        [Fact]
        public void SameFieldReferenceWarining()
        {
            Checker.BuildsWithWarning(c => c.Entity<SameFieldReference>(e =>
            {
                e.Property(p => p.Field).HasField("_field");
                e.Property(p => p.Field2).HasField("_field");
            }),
                                      CoreIssues.FieldIsReferencedMoreThanOnce.With(s => s.PropertyName == "Field", a => a.field == "_field" && a.property2 == "Field2"));
        }

        [Fact]
        public void SameFieldReferenceWariningAuto()
        {
            Checker.BuildsWithWarning(c => c.Entity<SameFieldReference>(e =>
                                                                        {
                                                                            e.Property(p => p.Field2).HasField("_field");
                                                                        }),
                                      CoreIssues.FieldIsReferencedMoreThanOnce.With(s => s.PropertyName == "Field", a => a.field == "_field" && a.property2 == "Field2"));
        }

        class NoVirtual
        {
            public int Test { get; set; }
        }

        [Fact]
        public void NoVirtualError()
        {
            Checker.ConfigurationFails(c => c.Entity<NoVirtual>(),
                                       CoreIssues.PropertyMustBeDeclaredAsVirtual.WithSource(s => s.PropertyName == "Test"));
        }

        [Fact]
        public void WrongFieldName()
        {
            Checker.ConfigurationFails(c => c.Entity<RootClass>(e => e.Property(p => p.RootInt).HasField("_wrong")),
                                       CoreIssues.FieldNotFound.With(s => s.PropertyName == "RootInt", args => args == "_wrong"));
        }

        [Fact]
        public void DefaultBackingField()
        {
            var ctx = Checker.BuildsWithoutIssues(c => c.Entity<RootClass>());
            var obj = ctx.Create<RootClass>();
            obj.RootInt = 42;
            Assert.Equal(42, obj.RootInt);
        }

        class Convention1
        {
            public int _field;

            public virtual int Field { get; set; }
        }

        [Fact]
        public void BackingFieldConvention1()
        {
            var ctx = Checker.BuildsWithoutIssues(c => c.Entity<Convention1>().AnyProperty().UsePropertyAccessMode(PropertyAccessMode.Field));
            var obj = ctx.Create<Convention1>();
            obj.Field = 42;
            Assert.Equal(42, obj._field);
        }

        class Convention2
        {
            public int field;

            public virtual int Field { get; set; }
        }

        [Fact]
        public void BackingFieldConvention2()
        {
            var ctx = Checker.BuildsWithoutIssues(c => c.Entity<Convention2>().AnyProperty().UsePropertyAccessMode(PropertyAccessMode.Field));
            var obj = ctx.Create<Convention2>();
            obj.Field = 42;
            Assert.Equal(42, obj.field);
        }

        class Convention3
        {
            public int m_field;

            public virtual int Field { get; set; }
        }

        [Fact]
        public void BackingFieldConvention3()
        {
            var ctx = Checker.BuildsWithoutIssues(c => c.Entity<Convention3>().AnyProperty().UsePropertyAccessMode(PropertyAccessMode.Field));
            var obj = ctx.Create<Convention3>();
            obj.Field = 42;
            Assert.Equal(42, obj.m_field);
        }

        class FieldNotFound
        {
            public virtual int Test { get => 10; set { } }
        }

        [Fact]
        public void BackingFieldNotFound()
        {
            Checker.ConfigurationFails(c => c.Entity<FieldNotFound>(),
                                       CoreIssues.BackingFieldNotFound.WithSource(s => s.PropertyName == "Test"));
        }

        [Fact]
        public void FieldNameOptionIsRedundant()
        {
            Checker.BuildsWithWarning(c => c.Entity<RootClass>(e => e.Property(s => s.RootInt).UsePropertyAccessMode(PropertyAccessMode.Property).HasField("some")),
                                       CoreIssues.CallIsRedundantOnProperty.With(s => s.PropertyName == "RootInt", args => args == "HasField"));
        }

        class GetterOnly
        {
            public int _test;

            public virtual int Test
            {
                get => _test;
            }
        }

        [Fact]
        public void PropertyMustHaveSetter()
        {
            Checker.ConfigurationFails(c => c.Entity<GetterOnly>().AnyProperty().UsePropertyAccessMode(PropertyAccessMode.Property),
                                       CoreIssues.PropertyMustHaveSetter.WithSource(s => s.PropertyName == "Test"));
            Checker.ConfigurationFails(c => c.Entity<GetterOnly>(),
                                       CoreIssues.PropertyMustHaveSetter.WithSource(s => s.PropertyName == "Test"));
        }

        [Fact]
        public void PropertyMayHaveNoSetter()
        {
            var ctx = Checker.BuildsWithoutIssues(c => c.Entity<GetterOnly>().AnyProperty().UsePropertyAccessMode(PropertyAccessMode.Field));
            var obj = ctx.Create<GetterOnly>();
            obj._test = 42;
            Assert.Equal(42, obj.Test);
        }

        class SetterOnly
        {
            public int _test;

            public virtual int Test
            {
                set { _test = value; }
            }
        }

        [Fact]
        public void PropertyMustHaveGetter()
        {
            Checker.ConfigurationFails(c => c.Entity<SetterOnly>().AnyProperty().UsePropertyAccessMode(PropertyAccessMode.Property),
                                       CoreIssues.PropertyMustHaveGetter.WithSource(s => s.PropertyName == "Test"));
            
        }

        [Fact]
        public void PropertyMayHaveNoGetter()
        {
            var ctx = Checker.BuildsWithoutIssues(c => c.Entity<SetterOnly>());
            var obj = ctx.Create<SetterOnly>();
            obj.Test = 42;
            Assert.Equal(42, obj._test);
        }

        class PropertyAndFieldTypeMismatched
        {
            public string _field;

            public virtual int Field { get; set; }
        }

        [Fact]
        public void PropertyAndFieldTypeMismatch()
        {
            Checker.ConfigurationFails(c => c.Entity<PropertyAndFieldTypeMismatched>(),
                                       CoreIssues.PropertyAndFieldTypeMismatch.With(s => s.PropertyName == "Field",
                                                                                    args => args.fieldName == "_field" && args.fieldType == typeof(string)));
        }

        [Fact]
        public void PropertyAccessModeProperty()
        {
            var ctx = Checker.BuildsWithoutIssues(c => c.Entity<PropertiesWithCounters>().AnyProperty().UsePropertyAccessMode(PropertyAccessMode.Property));
            var obj = ctx.Create<PropertiesWithCounters>();
            obj.Test = 42;
            Assert.Equal(42, obj.Test);
            Assert.Equal(1, obj._getterCalled);
            Assert.Equal(1, obj._setterCalled);
        }

        [Fact]
        public void PropertyAccessModePropertyAndCompare()
        {
            var ctx = Checker.BuildsWithoutIssues(c => c.Entity<PropertiesWithCounters>().AnyProperty().UsePropertyAccessMode(PropertyAccessMode.Property).UsePropertySetterMode(PropertySetterMode.CompareAndSet));
            var obj = ctx.Create<PropertiesWithCounters>();
            obj.Test = 42;
            Assert.Equal(42, obj.Test);
            Assert.Equal(2, obj._getterCalled);
            Assert.Equal(1, obj._setterCalled);
        }

        [Fact]
        public void PropertyAccessModeField()
        {
            var ctx = Checker.BuildsWithoutIssues(c => c.Entity<PropertiesWithCounters>().AnyProperty().UsePropertyAccessMode(PropertyAccessMode.Field));
            var obj = ctx.Create<PropertiesWithCounters>();
            obj.Test = 42;
            Assert.Equal(42, obj.Test);
            Assert.Equal(0, obj._getterCalled);
            Assert.Equal(0, obj._setterCalled);
        }

        [Fact]
        public void PropertyAccessModeFieldForGetterPropertyForSetter()
        {
            var ctx = Checker.BuildsWithoutIssues(c => c.Entity<PropertiesWithCounters>().AnyProperty().UsePropertyAccessMode(PropertyAccessMode.FieldForGetterPropertyForSetter));
            var obj = ctx.Create<PropertiesWithCounters>();
            obj.Test = 42;
            Assert.Equal(42, obj.Test);
            Assert.Equal(0, obj._getterCalled);
            Assert.Equal(1, obj._setterCalled);
        }

        [Fact]
        public void PropertyAccessModeFieldForGetterAndInitializationPropertyForSetter()
        {
            var ctx = Checker.BuildsWithoutIssues(c => c.Entity<PropertiesWithCounters>().AnyProperty().UsePropertyAccessMode(PropertyAccessMode.FieldForGetterAndInitializationPropertyForSetter));
            var obj = ctx.Create<PropertiesWithCounters>();
            obj.Test = 42;
            Assert.Equal(42, obj.Test);
            Assert.Equal(0, obj._getterCalled);
            Assert.Equal(1, obj._setterCalled);
        }

        [Theory]
        [InlineData(PropertyAccessMode.Property, 1, 1, 2)]
        [InlineData(PropertyAccessMode.FieldForGetterPropertyForSetter, 1, 0, 2)]
        [InlineData(PropertyAccessMode.Field, 0, 0, 0)]
        [InlineData(PropertyAccessMode.FieldForGetterAndInitializationPropertyForSetter, 0, 0, 0)]
        public void PropertyAccessModePropertyDefaultValueAndSerialization(PropertyAccessMode mode, int creationSetter, int serializeGetter, int deserializationSetter)
        {
            var ctx = Checker.BuildsWithoutIssues(c => c.Entity<PropertiesWithCounters>().Property(s => s.Test).UsePropertyAccessMode(mode).HasDefaultValue(42));
            var obj = ctx.Create<PropertiesWithCounters>();
            Assert.Equal(0, obj._getterCalled);
            Assert.Equal(creationSetter, obj._setterCalled);

            var str = ctx.Serialize(new[] { obj });
            Assert.Equal(serializeGetter, obj._getterCalled);
            Assert.Equal(creationSetter, obj._setterCalled);

            var des = ctx.Deserialize<PropertiesWithCounters>(str)[0];
            Assert.Equal(0, des._getterCalled);
            Assert.Equal(deserializationSetter, des._setterCalled); // one is set in ctor, one in deserialization
        }

        class BaseClassFields
        {
            private int _field;

            protected int _protectedField;

            public virtual int Field
            {
                get => _field; 
                set => _field = value;
            }

            public virtual int AutoProperty { get; set; }
        }

        class DerivedClassFields : BaseClassFields
        {
            public virtual int ProtectedField
            {
                get => _protectedField; 
                set => _protectedField = value;
            }
        }

        [Fact]
        public void BaseClassFieldsAreRespected()
        {
            var ctx = Checker.BuildsWithoutIssues(c => c.Entity<DerivedClassFields>());
            var obj = ctx.Create<DerivedClassFields>();
            obj.Field = 42;
            obj.ProtectedField = 43;
            obj.AutoProperty = 44;
            Assert.Equal(42, obj.Field);
            Assert.Equal(43, obj.ProtectedField);
            Assert.Equal(44, obj.AutoProperty);
        }
    }
}
