using System;
using System.Linq.Expressions;
using Clutch.Tests.Helpers;
using Clutch.Tests.Models;
using Xunit;

namespace Clutch.Tests
{
    public class DefaultValuesRedunduntSetTest
    {
        [Fact]
        public void NullableTypesDefaults()
        {
            CheckDefault<DifferentTypesClass, int>(i => i.Int, "0");
            CheckDefault<DifferentTypesClass, string>(i => i.String, "null");
            CheckDefault<DifferentTypesClass, char>(i => i.Char, "\0");
            CheckDefault<DifferentTypesClass, DateTime>(i => i.DateTime, $"{default(DateTime)}");
            CheckDefault<DifferentTypesClass, EnumByte>(i => i.EnumByte, $"{EnumByte.BVal0}");
            CheckDefault<DifferentTypesClass, Guid?>(i => i.GuidNullable, "null");
            CheckDefault<DifferentTypesClass, EnumLong?>(i => i.EnumLongNullable, "null");
        }

        private static void CheckDefault<T, TReturn>(Expression<Func<T, TReturn>> getter, string args)
        {
            Checker.BuildsWithWarning(s => s.Entity<T>().Property(getter).HasDefaultValue(default(TReturn)),
                                      CoreIssues.DefaultValueIsRedundant.WithArgs(s => s == args));
        }
    }
}
