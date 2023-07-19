using System;
using System.Linq;
using Clutch.Tests.Helpers;
using Clutch.Tests.Models;
using Xunit;

namespace Clutch.Tests
{
    public class BasicSerialization
    {
        [Fact]
        public void SerializeEmptyType()
        {
            var ctx = Checker.BuildsWithoutIssues(s => s.Entity<IEmptyType>());
            var i = ctx.Create<IEmptyType>();

            var json = ctx.Serialize(new [] { i });
            var objects = ctx.Deserialize<IEmptyType>(json);
            Assert.Equal(1, objects.Length);
            Assert.NotNull(objects[0]);
        }

        [Fact]
        public void SerializeList()
        {
            var ctx = Checker.BuildsWithoutIssues(s => s.Entity<IAllPrimitiveTypes>());
            var list = Enumerable.Range(0, 10).Select(s => ctx.Create<IAllPrimitiveTypes>()).ToList();
            list.ForEach(s => s.Int32 = 42);

            var json = ctx.Serialize(list.ToArray());
            var objects = ctx.Deserialize<IAllPrimitiveTypes>(json);
            Assert.Equal(10, objects.Length);
            Assert.All(objects, i => Assert.Equal(42, i.Int32));
        }

        [Fact]
        public void PrimitiveTypesSerialization()
        {
            SerializeDeserialize<IAllPrimitiveTypes>((i, init) =>
                                                     {
                                                         InitOrCheck(() => i.Double, val => i.Double = val, 4.2, init);
                                                         InitOrCheck(() => i.Int16, val => i.Int16 = val, (short)-16, init);
                                                         InitOrCheck(() => i.Int32, val => i.Int32 = val, -32, init);
                                                         InitOrCheck(() => i.Boolean, val => i.Boolean = val, true, init);
                                                         InitOrCheck(() => i.Byte, val => i.Byte = val, (byte)8, init);
                                                         InitOrCheck(() => i.Char, val => i.Char = val, '4', init);
                                                         InitOrCheck(() => i.Int64, val => i.Int64 = val, -64L, init);
                                                         InitOrCheck(() => i.IntPtr, val => i.IntPtr = val, new IntPtr(-42), init);
                                                         InitOrCheck(() => i.Single, val => i.Single = val, 4.242f, init);
                                                         InitOrCheck(() => i.SByte, val => i.SByte = val, (sbyte)-8, init);
                                                         InitOrCheck(() => i.UInt16, val => i.UInt16 = val, (UInt16)16, init);
                                                         InitOrCheck(() => i.UInt32, val => i.UInt32 = val, 32u, init);
                                                         InitOrCheck(() => i.UInt64, val => i.UInt64 = val, 64UL, init);
                                                         InitOrCheck(() => i.UIntPtr, val => i.UIntPtr = val, new UIntPtr(42), init);
                                                     });
        }

        [Fact]
        public void SpecialTypesSerialization()
        {
            var guid = Guid.NewGuid();
            var dateTime = DateTime.Now;
            var dateTimeOffset = DateTimeOffset.UtcNow;
            var byteArray = new byte[] { 4, 2, 4, 2 };
            SerializeDeserialize<ISpecialTypes>((i, init) =>
                                                          {
                                                              InitOrCheck(() => i.Guid, val => i.Guid = val, guid, init);
                                                              InitOrCheck(() => i.Decimal, val => i.Decimal = val, 4.2m, init);
                                                              InitOrCheck(() => i.String, val => i.String = val, "42", init);
                                                              InitOrCheck(() => i.DateTime, val => i.DateTime = val, dateTime, init);
                                                              InitOrCheck(() => i.DateTimeOffset, val => i.DateTimeOffset = val, dateTimeOffset, init);
                                                              InitOrCheck(() => i.ByteArray, val => i.ByteArray = val, byteArray, init);
                                                          });
        }

        [Fact]
        public void StringNullSerialization()
        {
            SerializeDeserialize<ISpecialTypes>((i, init) =>
                                                {
                                                    InitOrCheck(() => i.String, val => i.String = val, null, init);
                                                });
        }

        [Fact]
        public void ByteArrayNullSerialization()
        {
            SerializeDeserialize<ISpecialTypes>((i, init) =>
                                                {
                                                    InitOrCheck(() => i.ByteArray, val => i.ByteArray = val, null, init);
                                                });
        }

        [Fact]
        public void ByteArrayEmptySerialization()
        {
            SerializeDeserialize<ISpecialTypes>((i, init) =>
                                                {
                                                    InitOrCheck(() => i.ByteArray, val => i.ByteArray = val, new byte[0], init);
                                                });
        }

        [Fact]
        public void NullableTypesSerializationAsValues()
        {
            var guid = Guid.NewGuid();
            SerializeDeserialize<INullableTypes>((i, init) =>
                                                 {
                                                     InitOrCheck(() => i.Guid, val => i.Guid = val, guid, init);
                                                     InitOrCheck(() => i.Decimal, val => i.Decimal = val, 4.2m, init);
                                                     InitOrCheck(() => i.Char, val => i.Char = val, '4', init);
                                                     InitOrCheck(() => i.Int32, val => i.Int32 = val, -32, init);
                                                     InitOrCheck(() => i.EnumByte, val => i.EnumByte = val, EnumByte.BVal2, init);
                                                 });
        }

        [Fact]
        public void NullableTypesSerializationAsNulls()
        {
            SerializeDeserialize<INullableTypes>((i, init) =>
                                                 {
                                                     InitOrCheck(() => i.Guid, val => i.Guid = val, null, init);
                                                     InitOrCheck(() => i.Decimal, val => i.Decimal = val, null, init);
                                                     InitOrCheck(() => i.Char, val => i.Char = val, null, init);
                                                     InitOrCheck(() => i.Int32, val => i.Int32 = val, null, init);
                                                     InitOrCheck(() => i.EnumByte, val => i.EnumByte = val, null, init);
                                                 });
        }

        [Theory]
        [InlineData(EnumByte.BValM5, EnumLong.LValM5, EnumULong.UValM5)]
        [InlineData(EnumByte.BValM4, EnumLong.LValM4, EnumULong.UValM4)]
        [InlineData(EnumByte.BValM3, EnumLong.LValM3, EnumULong.UValM3)]
        [InlineData(EnumByte.BValM2, EnumLong.LValM2, EnumULong.UValM2)]
        [InlineData(EnumByte.BValM1, EnumLong.LValM1, EnumULong.UValM1)]
        [InlineData(EnumByte.BVal0, EnumLong.LVal0, EnumULong.UVal0)]
        [InlineData(EnumByte.BVal1, EnumLong.LVal1, EnumULong.UVal1)]
        [InlineData(EnumByte.BVal2, EnumLong.LVal2, EnumULong.UVal2)]
        [InlineData(EnumByte.BVal3, EnumLong.LVal3, EnumULong.UVal3)]
        [InlineData(EnumByte.BVal4, EnumLong.LVal4, EnumULong.UVal4)]
        [InlineData(EnumByte.BVal5, EnumLong.LVal5, EnumULong.UVal5)]
        public void EnumTypesSerializationAllValuesForTreeSwitcher(EnumByte eByte, EnumLong eLong, EnumULong eULong)
        {
            SerializeDeserialize<ITypeWithEnums>((i, init) =>
                                                {
                                                    InitOrCheck(() => i.EnumByte, val => i.EnumByte = val, eByte, init);
                                                    InitOrCheck(() => i.EnumLong, val => i.EnumLong = val, eLong, init);
                                                    InitOrCheck(() => i.EnumULong, val => i.EnumULong = val, eULong, init);
                                                });
        }




        [Fact]
        public void EnumTypesSerializationFlags()
        {
            SerializeDeserialize<ITypeWithEnums>((i, init) =>
                                                {
                                                    InitOrCheck(() => i.EnumFlags, val => i.EnumFlags = val, EnumFlags.Val2 | EnumFlags.Val3, init);
                                                });
        }

        [Fact]
        public void EnumTypesSerializationFlagsWithCommas()
        {
            var ctx = Checker.BuildsWithoutIssues(s => s.Entity<ITypeWithEnums>());
            var de = ctx.Deserialize<ITypeWithEnums>(@"[{""_t"":""ITypeWithEnums"", ""EnumFlags"":""Val2,Val3""}]");
            Assert.Single(de);
            Assert.Equal(EnumFlags.Val2 | EnumFlags.Val3, de[0].EnumFlags);
        }


        [Fact]
        public void ByteArraySerializationError()
        {
            var ctx = Checker.BuildsWithoutIssues(s => s.Entity<ISpecialTypes>());
            Assert.Throws<ClutchRuntimeException>(() => ctx.Deserialize<ISpecialTypes>(@"[{""_t"":""ISpecialTypes"", ""ByteArray"":""NotValidBase64""}]"));
        }

        [Theory]
        [InlineData(EnumULong.UValM5Dublicate)]
        [InlineData(EnumULong.UVal5Dublicate)]
        public void EnumTypesSerializationAllValuesForTreeSwitcherDuplicates(EnumULong eULong)
        {
            SerializeDeserialize<ITypeWithEnums>((i, init) =>
                                                {
                                                    InitOrCheck(() => i.EnumULong, val => i.EnumULong = val, eULong, init);
                                                });
        }

        [Fact]
        public void EnumTypesSerializationAllValuesDoesNotExist()
        {
            SerializeDeserialize<ITypeWithEnums>((i, init) =>
                                                {
                                                    InitOrCheck(() => i.EnumByte, val => i.EnumByte = val, (EnumByte)100, init);
                                                    InitOrCheck(() => i.EnumLong, val => i.EnumLong = val, (EnumLong)110, init);
                                                    InitOrCheck(() => i.EnumULong, val => i.EnumULong = val, (EnumULong)120, init);
                                                });
        }

        private static void InitOrCheck<T>(Func<T> getter, Action<T> setter, T value, bool init)
        {
            if (init)
            {
                setter(value);
            }
            else
            {
                var get = getter();
                Assert.Equal(value, get);
            }
        }

        private static void SerializeDeserialize<T>(Action<T, bool> initOrChecker)
        {
            var ctx = Checker.BuildsWithoutIssues(s => s.Entity<T>());
            var list = new[] { ctx.Create<T>() };
            initOrChecker(list[0], true);
            var str = ctx.Serialize(list);
            var de = ctx.Deserialize<T>(str);
            Assert.Single(de);
            initOrChecker(de[0], false);
        }
    }
}
