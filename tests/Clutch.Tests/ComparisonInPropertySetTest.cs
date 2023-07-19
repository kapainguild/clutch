using System;
using System.Linq.Expressions;
using Clutch.CoreExtensions.NotifyPropertyChanged;
using Clutch.Tests.Helpers;
using Clutch.Tests.Models;
using Xunit;

namespace Clutch.Tests
{
    public class ComparisonInPropertySetTest
    {
        [Fact]
        public void PrimitiveTypes()
        {
            Check(c => c.Create<IAllPrimitiveTypes>(), s => s.Byte, s => s.Byte = 42, s => s.Byte = 0);
            Check(c => c.Create<IAllPrimitiveTypes>(), s => s.Boolean, s => s.Boolean = true, s => s.Boolean = false);
            Check(c => c.Create<IAllPrimitiveTypes>(), s => s.SByte, s => s.SByte = 42, s => s.SByte = 0);
            Check(c => c.Create<IAllPrimitiveTypes>(), s => s.Int16, s => s.Int16 = 42, s => s.Int16 = 0);
            Check(c => c.Create<IAllPrimitiveTypes>(), s => s.UInt16, s => s.UInt16 = 42, s => s.UInt16 = 0);
            Check(c => c.Create<IAllPrimitiveTypes>(), s => s.Int32, s => s.Int32 = 42, s => s.Int32 = 0);
            Check(c => c.Create<IAllPrimitiveTypes>(), s => s.UInt32, s => s.UInt32 = 42, s => s.UInt32 = 0);
            Check(c => c.Create<IAllPrimitiveTypes>(), s => s.Int64, s => s.Int64 = 42, s => s.Int64 = 0);
            Check(c => c.Create<IAllPrimitiveTypes>(), s => s.UInt64, s => s.UInt64 = 42, s => s.UInt64 = 0);
            Check(c => c.Create<IAllPrimitiveTypes>(), s => s.IntPtr, s => s.IntPtr = new IntPtr(42), s => s.IntPtr = new IntPtr(0));
            Check(c => c.Create<IAllPrimitiveTypes>(), s => s.UIntPtr, s => s.UIntPtr = new UIntPtr(42), s => s.UIntPtr = new UIntPtr(0));
            Check(c => c.Create<IAllPrimitiveTypes>(), s => s.Char, s => s.Char = '4', s => s.Char = '\u0372');
            Check(c => c.Create<IAllPrimitiveTypes>(), s => s.Single, s => s.Single = 42.2F, s => s.Single = Single.MinValue);
            Check(c => c.Create<IAllPrimitiveTypes>(), s => s.Single, s => s.Single = Single.PositiveInfinity, s => s.Single = Single.NegativeInfinity);
            Check(c => c.Create<IAllPrimitiveTypes>(), s => s.Double, s => s.Double = 42.2, s => s.Double = Double.MinValue);
            Check(c => c.Create<IAllPrimitiveTypes>(), s => s.Double, s => s.Double = Double.PositiveInfinity, s => s.Double = Double.NegativeInfinity);
        }

        [Fact]
        public void FloatPointNaN()
        {
            var ctx = Build<IAllPrimitiveTypes>();
            var obj = ctx.Create<IAllPrimitiveTypes>();

            ClutchAssert.RaisesPropertyChanged(obj, s => s.Double, s => s.Double = Double.NaN);
            ClutchAssert.RaisesPropertyChanged(obj, s => s.Double, s => s.Double = Double.NaN); // NaN != NaN

            ClutchAssert.RaisesPropertyChanged(obj, s => s.Single, s => s.Single = Single.NaN);
            ClutchAssert.RaisesPropertyChanged(obj, s => s.Single, s => s.Single = Single.NaN); // NaN != NaN
        }


        [Fact]
        public void EnumTypes()
        {
            Check(c => c.Create<ITypeWithEnums>(), s => s.EnumByte, s => s.EnumByte = EnumByte.BVal2, s => s.EnumByte = EnumByte.BVal1);
            Check(c => c.Create<ITypeWithEnums>(), s => s.EnumFlags, s => s.EnumFlags = EnumFlags.Val2, s => s.EnumFlags = EnumFlags.Val2 | EnumFlags.Val3);
        }

        [Fact]
        public void SpecialTypes()
        {
            Check(c => c.Create<ISpecialTypes>(), s => s.String, s => s.String = "42", s => s.String = null);

            var someGuid = Guid.NewGuid();
            Check(c => c.Create<ISpecialTypes>(), s => s.Guid, s => s.Guid = someGuid, s => s.Guid = Guid.Empty);

            Check(c => c.Create<ISpecialTypes>(), s => s.Decimal, s => s.Decimal = 4.2m, s => s.Decimal = Decimal.Zero);

            var now = DateTime.Now;
            Check(c => c.Create<ISpecialTypes>(), s => s.DateTime, s => s.DateTime = now, s => s.DateTime = DateTime.MinValue);

            var dtOffset = DateTimeOffset.UtcNow;
            Check(c => c.Create<ISpecialTypes>(), s => s.DateTimeOffset, s => s.DateTimeOffset = dtOffset, s => s.DateTimeOffset = DateTimeOffset.MinValue);

            var byteArray = new byte[] { 4, 2, 4, 2 };
            Check(c => c.Create<ISpecialTypes>(), s => s.ByteArray, s => s.ByteArray = byteArray, s => s.ByteArray = null);
        }

        [Fact]
        public void NullableTypes()
        {
            Check(c => c.Create<INullableTypes>(), s => s.Decimal, s => s.Decimal = 4.2m, s => s.Decimal = null);
            Check(c => c.Create<INullableTypes>(), s => s.Decimal, s => s.Decimal = 4.2m, s => s.Decimal = 2.4m);

            Check(c => c.Create<INullableTypes>(), s => s.Int32, s => s.Int32 = 42, s => s.Int32 = null);
            Check(c => c.Create<INullableTypes>(), s => s.Int32, s => s.Int32 = 42, s => s.Int32 = 24);

            Check(c => c.Create<INullableTypes>(), s => s.Char, s => s.Char = '4', s => s.Char = null);
            Check(c => c.Create<INullableTypes>(), s => s.Char, s => s.Char = '4', s => s.Char = '2');

            Check(c => c.Create<INullableTypes>(), s => s.EnumByte, s => s.EnumByte = EnumByte.BVal1, s => s.EnumByte = null);
            Check(c => c.Create<INullableTypes>(), s => s.EnumByte, s => s.EnumByte = EnumByte.BVal1, s => s.EnumByte = EnumByte.BVal3);

            var someGuid = Guid.NewGuid();
            var someGuid2 = Guid.NewGuid();
            Check(c => c.Create<INullableTypes>(), s => s.Guid, s => s.Guid = someGuid, s => s.Guid = null);
            Check(c => c.Create<INullableTypes>(), s => s.Guid, s => s.Guid = someGuid, s => s.Guid = someGuid2);

        }



        private static void Check<TInterface, TProperty>(Func<ClutchContext, TInterface> creator, Expression<Func<TInterface, TProperty>> getter, Action<TInterface> firstSet, Action<TInterface> secondSet)
        {
            var ctx = Build<TInterface>();
            var obj = creator(ctx);

            ClutchAssert.RaisesPropertyChanged(obj, getter, firstSet);
            ClutchAssert.DoesNotRaisePropertyChanged(obj, getter, firstSet);

            ClutchAssert.RaisesPropertyChanged(obj, getter, secondSet);
            ClutchAssert.DoesNotRaisePropertyChanged(obj, getter, secondSet);
        }

        private static ClutchContext Build<T>()
        {
            return Checker.BuildsWithoutIssues(c =>
                                               {
                                                   c.UseNotifyPropertyChanged(NotifyPropertyChangedBehavior.ImplementOnAllEntities);
                                                   c.AnyEntityType().AnyProperty().UsePropertySetterMode(PropertySetterMode.CompareAndSet);
                                                   c.Entity<T>();
                                               });
        }

    }
}
