using System;
using Clutch.Tests.Helpers;
using Clutch.Tests.Models;
using Xunit;

namespace Clutch.Tests
{
    public class SerializationExceptions
    {
        private const string Replacement = "Replacement";

        [Fact]
        public void PartialData()
        {
            var (ctx, str) = Init();

            for (int q = 0; q < str.Length - 1; q++)
            {
                var part = str.Substring(0, q);
                Assert.Throws<ClutchRuntimeException>(() => ctx.Deserialize<DifferentTypesClass>(part));
            }
        }

        [Fact]
        public void AlteredData()
        {
            var (ctx, str) = Init();

            // insert
            for (int q = 0; q < str.Length - 1; q++)
            {
                var altered = str.Insert(q, "Y");
                Assert.Throws<ClutchRuntimeException>(() => ctx.Deserialize<DifferentTypesClass>(altered));
            }

            //replace
            for (int q = 0; q < str.Length - 1; q++)
            {
                char[] chars = str.ToCharArray();
                chars[q] = 'Y';
                Assert.Throws<ClutchRuntimeException>(() => ctx.Deserialize<DifferentTypesClass>(new string(chars)));
            }
        }

        [Fact]
        public void ExtraData()
        {
            var (ctx, str) = Init();

            str = str + "[]";
            Assert.Throws<ClutchRuntimeException>(() => ctx.Deserialize<DifferentTypesClass>(str));
        }

        [Fact]
        public void PropertyNotFound()
        {
            var (ctx, str) = Init();

            str = str.Replace(nameof(DifferentTypesClass.Char), Replacement);
            var ex = Assert.Throws<ClutchRuntimeException>(() => ctx.Deserialize<DifferentTypesClass>(str));
            Assert.Contains(Replacement, ex.Message);
        }

        [Fact]
        public void TypeNotFound()
        {
            var (ctx, str) = Init();

            str = str.Replace(nameof(DifferentTypesClass), Replacement);
            var ex = Assert.Throws<ClutchRuntimeException>(() => ctx.Deserialize<DifferentTypesClass>(str));
            Assert.Contains(Replacement, ex.Message);
        }

        [Fact]
        public void DiscriminatorNotFound()
        {
            var (ctx, str) = Init();

            str = str.Replace("_t", Replacement);
            var ex = Assert.Throws<ClutchRuntimeException>(() => ctx.Deserialize<DifferentTypesClass>(str));
            Assert.Contains("_t", ex.Message);
        }

        [Fact]
        public void DiscriminatorNotString()
        {
            var (ctx, str) = Init();

            str = str.Replace('\"' + nameof(DifferentTypesClass) + '\"', "42");
            var ex = Assert.Throws<ClutchRuntimeException>(() => ctx.Deserialize<DifferentTypesClass>(str));
            Assert.Contains("discriminator", ex.Message);
        }

        [Fact]
        public void LongChar()
        {
            var (ctx, str) = Init();

            str = str.Replace("\\u0000", Replacement);
            var ex = Assert.Throws<ClutchRuntimeException>(() => ctx.Deserialize<DifferentTypesClass>(str));
            Assert.Contains("Error while reading char property", ex.Message);
        }

        [Fact]
        public void BadStructure()
        {
            var (ctx, _) = Init();
            Assert.Throws<ClutchRuntimeException>(() => ctx.Deserialize<DifferentTypesClass>("[[]]"));
            Assert.Throws<ClutchRuntimeException>(() => ctx.Deserialize<DifferentTypesClass>("{}"));
            Assert.Throws<ClutchRuntimeException>(() => ctx.Deserialize<DifferentTypesClass>("[{}]"));
        }

        private static (ClutchContext, string) Init()
        {
            var ctx = Checker.BuildsWithoutIssues(c => c.Entity<DifferentTypesClass>());
            var obj = ctx.Create<DifferentTypesClass>();
            obj.DateTime = new DateTime(2019,07,11,10,33,0);
            obj.GuidNullable = Guid.Empty;
            obj.EnumLongNullable = EnumLong.LVal2;
            var str = ctx.Serialize(new[] { obj });
            return (ctx, str);
        }
    }
}
