using System;
using System.Collections.Generic;
using System.Text;

namespace Clutch.Tests.Models
{
    public interface IEmptyType
    {

    }

    public interface IAllPrimitiveTypes
    {
        Int32 Int32 { get; set; }
        Byte Byte { get; set; }
        Double Double { get; set; }
        Boolean Boolean { get; set; }
        SByte SByte { get; set; }
        Int16 Int16 { get; set; }
        UInt16 UInt16 { get; set; }
        UInt32 UInt32 { get; set; }
        Int64 Int64 { get; set; }
        UInt64 UInt64 { get; set; }
        IntPtr IntPtr { get; set; }
        UIntPtr UIntPtr { get; set; }
        Char Char { get; set; }
        Single Single { get; set; }
    }

    public enum EnumByte : byte
    {
        BValM5 = unchecked((byte)-5),
        BValM4 = unchecked((byte)-4),
        BValM3 = unchecked((byte)-3),
        BValM2 = unchecked((byte)-2),
        BValM1 = unchecked((byte)-1),
        BVal0 = 0,
        BVal1 = 1,
        BVal2 = 2,
        BVal3 = 3,
        BVal4 = 4,
        BVal5 = 5,
    }

    public enum EnumULong : ulong
    {
        UValM5Dublicate = unchecked((ulong)-5),
        UValM5 = unchecked((ulong)-5),
        UValM4 = unchecked((ulong)-4),
        UValM3 = unchecked((ulong)-3),
        UValM2 = unchecked((ulong)-2),
        UValM1 = unchecked((ulong)-1),
        UVal0 = 0,
        UVal1 = 1,
        UVal2 = 2,
        UVal3 = 3,
        UVal4 = 4,
        UVal5 = 5,
        UVal5Dublicate = 5,
    }

    public enum EnumLong : long
    {
        LValM5 = -5,
        LValM4 = -4,
        LValM3 = -3,
        LValM2 = -2,
        LValM1 = -1,
        LVal0 = 0,
        LVal1 = 1,
        LVal2 = 2,
        LVal3 = 3,
        LVal4 = 4,
        LVal5 = 5,
    }

    [Flags]
    public enum EnumFlags
    {
        Val0 = 1,
        Val1 = 2,
        Val2 = 4,
        Val3 = 8,
    }

    public interface ITypeWithEnums
    {
        EnumByte EnumByte { get; set; }

        EnumULong EnumULong { get; set; }

        EnumLong EnumLong { get; set; }

        EnumFlags EnumFlags { get; set; }
    }

    public interface ISpecialTypes
    {
        String String { get; set; }

        Guid Guid { get; set; }

        Decimal Decimal { get; set; }

        DateTime DateTime { get; set; }

        DateTimeOffset DateTimeOffset { get; set; }

        byte[] ByteArray { get; set; }
    }

    public interface INullableTypes
    {
        EnumByte? EnumByte { get; set; }

        Decimal? Decimal { get; set; }

        Int32? Int32 { get; set; }

        Char? Char { get; set; }

        Guid? Guid { get; set; }
    }

    public class DifferentTypesClass
    {
        public virtual int Int { get; set; }

        public virtual char Char { get; set; }

        public virtual string String { get; set; }

        public virtual DateTime DateTime { get; set; }

        public virtual EnumByte EnumByte { get; set; }

        public virtual Guid? GuidNullable { get; set; }

        public virtual EnumLong? EnumLongNullable { get; set; }
    }

}
