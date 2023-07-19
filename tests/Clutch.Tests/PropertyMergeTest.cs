using System;
using System.Collections.Generic;
using System.Text;
using Clutch.Tests.Helpers;
using Clutch.Tests.Models;
using Xunit;

namespace Clutch.Tests
{
    public class PropertyMergeTest
    {
        [Fact]
        public void AnyPropertyOnAnyEntity() =>
            AccessModeIsProperty(c =>
                                 {
                                     c.AnyEntityType().AnyProperty().UsePropertyAccessMode(PropertyAccessMode.Property);
                                     c.Entity<PropertiesWithCountersDerived>();
                                 });

        [Fact]
        public void AnyPropertyOnAnyEntityOverriddenByConcreteEntity() =>
            AccessModeIsField(c =>
                                 {
                                     c.AnyEntityType().AnyProperty().UsePropertyAccessMode(PropertyAccessMode.Property);
                                     c.Entity<PropertiesWithCountersDerived>().AnyProperty(true).UsePropertyAccessMode(PropertyAccessMode.Field);
                                 });

        [Fact]
        public void AnyPropertyOnAnyEntityOverriddenByConcreteEntityOtherOrder() =>
            AccessModeIsField(c =>
                              {
                                  c.Entity<PropertiesWithCountersDerived>().AnyProperty(true).UsePropertyAccessMode(PropertyAccessMode.Field);
                                  c.AnyEntityType().AnyProperty().UsePropertyAccessMode(PropertyAccessMode.Property);
                              });

        [Fact]
        public void ParentPropertiesAreOverriden() =>
            AccessModeIsField(c =>
                              {
                                  c.Entity<PropertiesWithCounters>().AnyProperty().UsePropertyAccessMode(PropertyAccessMode.Property);
                                  c.Entity<PropertiesWithCountersDerived>().AnyProperty(true).UsePropertyAccessMode(PropertyAccessMode.Field);
                              });

        [Fact]
        public void ParentPropertiesAreOverridenByConcreteProperty() =>
            AccessModeIsField(c =>
                              {
                                  c.Entity<PropertiesWithCounters>().AnyProperty().UsePropertyAccessMode(PropertyAccessMode.Property);
                                  c.Entity<PropertiesWithCountersDerived>().Property(s => s.Test).UsePropertyAccessMode(PropertyAccessMode.Field);
                              });

        [Fact]
        public void ParentConcretePropertiesAreOverridenByDerivedAnyProperty() =>
            AccessModeIsField(c =>
                              {
                                  c.Entity<PropertiesWithCounters>().Property(s => s.Test).UsePropertyAccessMode(PropertyAccessMode.Property);
                                  c.Entity<PropertiesWithCountersDerived>().AnyProperty(true).UsePropertyAccessMode(PropertyAccessMode.Field);
                              });

        [Fact]
        public void ParentPropertiesAreNotOverriden() =>
            AccessModeIsProperty(c =>
                              {
                                  c.Entity<PropertiesWithCounters>().AnyProperty().UsePropertyAccessMode(PropertyAccessMode.Property);
                                  c.Entity<PropertiesWithCountersDerived>().AnyProperty(false).UsePropertyAccessMode(PropertyAccessMode.Field);
                              });


        [Fact]
        public void AnyPropertyOnAnyEntityOverriddenByConcreteProperty() =>
            AccessModeIsField(c =>
                              {
                                  c.AnyEntityType().AnyProperty().UsePropertyAccessMode(PropertyAccessMode.Property);
                                  c.Entity<PropertiesWithCountersDerived>().Property(s => s.Test).UsePropertyAccessMode(PropertyAccessMode.Field);
                              });

        [Fact]
        public void AnyPropertyOnEntity() =>
            AccessModeIsProperty(c =>
                              {
                                  c.Entity<PropertiesWithCountersDerived>().AnyProperty(true).UsePropertyAccessMode(PropertyAccessMode.Property);
                              });

        [Fact]
        public void AnyPropertyOnEntityOverriddenByConcreteProperty() =>
            AccessModeIsField(c =>
                                 {
                                     c.Entity<PropertiesWithCountersDerived>().AnyProperty(true).UsePropertyAccessMode(PropertyAccessMode.Property);
                                     c.Entity<PropertiesWithCountersDerived>().Property(s => s.Test).UsePropertyAccessMode(PropertyAccessMode.Field);
                                 });

        [Fact]
        public void AnyPropertyOnEntityOverriddenByConcretePropertyOtherOrder() =>
            AccessModeIsField(c =>
                              {
                                  c.Entity<PropertiesWithCountersDerived>().Property(s => s.Test).UsePropertyAccessMode(PropertyAccessMode.Field);
                                  c.Entity<PropertiesWithCountersDerived>().AnyProperty(true).UsePropertyAccessMode(PropertyAccessMode.Property);
                              });



        private void AccessModeIsProperty(Action<ClutchContextBuilder> buildAction)
        {
            var ctx = Checker.BuildsWithoutIssues(buildAction);
            var obj = ctx.Create<PropertiesWithCountersDerived>();
            obj.Test = 42;
            Assert.Equal(42, obj.Test);
            Assert.Equal(1, obj._getterCalled);
            Assert.Equal(1, obj._setterCalled);
        }

        private void AccessModeIsField(Action<ClutchContextBuilder> buildAction)
        {
            var ctx = Checker.BuildsWithoutIssues(buildAction);
            var obj = ctx.Create<PropertiesWithCountersDerived>();
            obj.Test = 42;
            Assert.Equal(42, obj.Test);
            Assert.Equal(0, obj._getterCalled);
            Assert.Equal(0, obj._setterCalled);
        }
    }
}
