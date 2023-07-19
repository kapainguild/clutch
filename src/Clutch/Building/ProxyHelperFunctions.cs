using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace Clutch.Building
{
    public static class ProxyHelperFunctions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ReadOnlySpan<byte> GetStringAsReadOnlySpan(ref Utf8JsonReader reader)
        {
            return reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool SequenceEqual(ReadOnlySpan<byte> str, ReadOnlySpan<byte> str2) => str.SequenceEqual(str2);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong GetFastHashCode(ReadOnlySpan<byte> str)
        {
            var result = (ulong)str.Length;
            if (str.Length >= 8)
                result ^= Unsafe.ReadUnaligned<ulong>(ref MemoryMarshal.GetReference(str)); 
            else if (str.Length >= 4)
                result |= (((ulong)Unsafe.ReadUnaligned<uint>(ref MemoryMarshal.GetReference(str))) << 16); 
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong GetFastHashCodeRef(ref ReadOnlySpan<byte> str)
        {
            var result = (ulong)str.Length;
            if (str.Length >= 8)
                result ^= Unsafe.ReadUnaligned<ulong>(ref MemoryMarshal.GetReference(str));
            else if (str.Length >= 4)
                result |= (((ulong)Unsafe.ReadUnaligned<uint>(ref MemoryMarshal.GetReference(str))) << 16);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool SequenceEquals(ref ReadOnlySpan<byte> str1, ReadOnlySpan<byte> str2)
        {
            return str1.SequenceEqual(str2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CompareByteArrays(byte[] array1, byte[] array2)
        {
            if (array1 == null)
            {
                return array2 == null;
            }
            else
            {
                if (array2 == null)
                    return false;
                return new ReadOnlySpan<byte>(array1).SequenceEqual(array2);
            }
        }

        public static T ParseEnum<T>(ref ReadOnlySpan<byte> utf8Escaped) where T : struct, Enum
        {
            var text = JsonEncodedText.Encode(utf8Escaped);
            if (Enum.TryParse<T>(text.ToString(), out var t))
                return t;
            throw new ClutchRuntimeException($"Unable to parse '{text}' enum value for the '{typeof(T)}' enum type");
        }

        internal static char ReadChar(ref Utf8JsonReader reader)
        {
            var str = reader.GetString();
            if (str.Length != 1)
                throw new ClutchRuntimeException($"Error while reading char property. String length is {str.Length} (!= 1)");
            return str[0];
        }

        internal static void HelperThrowPropertyNotFound(ReadOnlySpan<byte> property)
        {
            var text = JsonEncodedText.Encode(property);
            throw new ClutchRuntimeException($"Property '{text}' not found on an Entity");
        }

        internal static void HelperThrowTypeNotFound(ReadOnlySpan<byte> property)
        {
            var text = JsonEncodedText.Encode(property);
            throw new ClutchRuntimeException($"Type '{text}' not found");
        }

        internal static void ThrowString(string message)
        {
            throw new ClutchRuntimeException(message);
        }

        internal static void ThrowUnknownType(Type type)
        {
            throw new ClutchRuntimeException($"Type '{type}' is not registered as Entity");
        }
    }
}
