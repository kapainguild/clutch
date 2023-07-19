using Clutch.Tests.Helpers;
using Clutch.Tests.Models;
using Xunit;

namespace Clutch.Tests
{
    public class OwnedTypeTests
    {

        [Fact]
        public void CannotChangeType()
        {
            Checker.ConfigurationFails(c =>
                                       {
                                           c.Entity<IRoot>();
                                           c.OwnedType<IRoot>();
                                       }, CoreIssues.CannotChangeTypeCharacter.With(s => s.Type == typeof(IRoot), a => a == EntityTypeBuilder.Character));

            Checker.ConfigurationFails(c =>
                                       {
                                           c.OwnedType<IRoot>();
                                           c.Entity<IRoot>();
                                       }, CoreIssues.CannotChangeTypeCharacter.With(s => s.Type == typeof(IRoot), a => a == OwnedTypeBuilder.Character));
        }

        [Fact]
        public void MonotonicInheritance()
        {
            Checker.ConfigurationFails(c =>
                                       {
                                           c.Entity<IRoot>();
                                           c.OwnedType<IDerived1>();
                                       }, CoreIssues.CannotDeriveFromOtherTypeCharacter.With(s => s.Type == typeof(IDerived1),
                                                                                             args => args.baseType == typeof(IRoot) &&
                                                                                                     args.characterBase == EntityTypeBuilder.Character &&
                                                                                                     args.characterCurrent == OwnedTypeBuilder.Character));
        }

        interface IRootEntity
        {
            IOwnedType1 OwnedType1 { get; set; }
        }

        interface IOwnedType1
        {
            IOwnedType2 OwnedType2 { get; set; }
        }

        interface IOwnedType2
        {
            int Int { get; set; }
        }

        [Fact]
        public void CascadeSerialization()
        {
            var ctx = Checker.BuildsWithoutIssues(c =>
                                       {
                                           c.Entity<IRootEntity>();
                                           c.OwnedType<IOwnedType1>();
                                           c.OwnedType<IOwnedType2>();
                                       });
            var obj = ctx.Create<IRootEntity>();
            obj.OwnedType1 = ctx.Create<IOwnedType1>();
            obj.OwnedType1.OwnedType2 = ctx.Create<IOwnedType2>();
            obj.OwnedType1.OwnedType2.Int = 42;

            var str = ctx.Serialize(new[] { obj });
            var des = ctx.Deserialize<IRootEntity>(str)[0];
            Assert.NotNull(des.OwnedType1);
            Assert.NotNull(des.OwnedType1.OwnedType2);
            Assert.Equal(42, des.OwnedType1.OwnedType2.Int);
        }
    }
}
