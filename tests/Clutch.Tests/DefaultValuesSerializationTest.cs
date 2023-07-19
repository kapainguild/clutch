using System;
using System.Linq.Expressions;
using Clutch.Tests.Helpers;
using Clutch.Tests.Models;
using Xunit;

namespace Clutch.Tests
{
    public class DefaultValuesSerializationTest
    {
        [Fact]
        public void PrimitiveTypesDefaults()
        {
            CheckDefault<IAllPrimitiveTypes, double>(i => i.Double, 4.2);
            CheckDefault<IAllPrimitiveTypes, short>(i => i.Int16, -16);
            CheckDefault<IAllPrimitiveTypes, int>(i => i.Int32, -32);
            CheckDefault<IAllPrimitiveTypes, bool>(i => i.Boolean, true);
            CheckDefault<IAllPrimitiveTypes, byte>(i => i.Byte, 8);
            CheckDefault<IAllPrimitiveTypes, char>(i => i.Char, '4');
            CheckDefault<IAllPrimitiveTypes, long>(i => i.Int64, -64L);
            CheckDefault<IAllPrimitiveTypes, IntPtr>(i => i.IntPtr, new IntPtr(-42));
            CheckDefault<IAllPrimitiveTypes, float>(i => i.Single, 4.242f);
            CheckDefault<IAllPrimitiveTypes, sbyte>(i => i.SByte, -8);
            CheckDefault<IAllPrimitiveTypes, ushort>(i => i.UInt16, 16);
            CheckDefault<IAllPrimitiveTypes, uint>(i => i.UInt32, uint.MaxValue);
            CheckDefault<IAllPrimitiveTypes, ulong>(i => i.UInt64, 64UL);
            CheckDefault<IAllPrimitiveTypes, UIntPtr>(i => i.UIntPtr, new UIntPtr(42));
        }

        [Fact]
        public void SpecialTypesDefaults()
        {
            CheckDefault<ISpecialTypes, string>(i => i.String, "test");
            CheckDefault<ISpecialTypes, Guid>(i => i.Guid, Guid.NewGuid());
            CheckDefault<ISpecialTypes, decimal>(i => i.Decimal, 4.2m);
            CheckDefault<ISpecialTypes, DateTime>(i => i.DateTime, DateTime.Now);
            CheckDefault<ISpecialTypes, DateTimeOffset>(i => i.DateTimeOffset, DateTimeOffset.Now);
            CheckDefault<ISpecialTypes, byte[]>(i => i.ByteArray, new byte[] { 4, 2, 4, 2 });
        }

        [Fact]
        public void TypeWithEnumsDefaults()
        {
            CheckDefault<ITypeWithEnums, EnumByte>(i => i.EnumByte, EnumByte.BValM2);
            CheckDefault<ITypeWithEnums, EnumLong>(i => i.EnumLong, EnumLong.LValM5);
            CheckDefault<ITypeWithEnums, EnumULong>(i => i.EnumULong, EnumULong.UValM5);
            CheckDefault<ITypeWithEnums, EnumFlags>(i => i.EnumFlags, EnumFlags.Val1 | EnumFlags.Val3);
        }

        [Fact]
        public void NullableTypesDefaults()
        {
            CheckDefault<INullableTypes, Guid?>(i => i.Guid, Guid.NewGuid());
            CheckDefault<INullableTypes, decimal?>(i => i.Decimal, 4.2m);
            CheckDefault<INullableTypes, char?>(i => i.Char, '3');
            CheckDefault<INullableTypes, int?>(i => i.Int32, 42);
            CheckDefault<INullableTypes, EnumByte?>(i => i.EnumByte, EnumByte.BVal3);
        }

        [Theory]
        [InlineData(PropertyAccessMode.Property)]
        [InlineData(PropertyAccessMode.FieldForGetterAndInitializationPropertyForSetter)]
        [InlineData(PropertyAccessMode.Field)]
        [InlineData(PropertyAccessMode.FieldForGetterPropertyForSetter)]
        public void DifferentTypesClassDefaultsWithAccessMode(PropertyAccessMode mode)
        {
            CheckDefault<DifferentTypesClass, int>(i => i.Int, 42, mode);
            CheckDefault<DifferentTypesClass, string>(i => i.String, "test", mode);
            CheckDefault<DifferentTypesClass, char>(i => i.Char, '4', mode);
            CheckDefault<DifferentTypesClass, DateTime>(i => i.DateTime, DateTime.MaxValue, mode);
            CheckDefault<DifferentTypesClass, EnumByte>(i => i.EnumByte, EnumByte.BValM2, mode);
            CheckDefault<DifferentTypesClass, Guid?>(i => i.GuidNullable, Guid.NewGuid(), mode);
            CheckDefault<DifferentTypesClass, EnumLong?>(i => i.EnumLongNullable, EnumLong.LVal2, mode);
        }

        private static void CheckDefault<T, TReturn>(Expression<Func<T, TReturn>> getter, TReturn defaultValue, PropertyAccessMode mode = PropertyAccessMode.Field)
        {
            var ctx = Checker.BuildsWithoutIssues(s =>
                                                  {
                                                      s.AnyEntityType().AnyProperty().UseDefaultValueHandling(DefaultValueHandling.IgnoreAndPopulate).UsePropertyAccessMode(mode);
                                                      s.Entity<T>().Property(getter).HasDefaultValue(defaultValue);
                                                  });
            var obj = ctx.Create<T>();
            var compiled = getter.Compile();
            Assert.Equal(defaultValue, compiled(obj));

            var ser = ctx.Serialize(new[] { obj });

            Assert.Equal($"[{{\"_t\":\"{typeof(T).Name}\"}}]", ser);

            var des = ctx.Deserialize<T>(ser);
            
            Assert.Equal(defaultValue, compiled(des[0]));
        }

    }
}
