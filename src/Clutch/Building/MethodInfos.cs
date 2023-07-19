using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using Clutch.Utility;

namespace Clutch.Building
{
    static class MethodInfos
    {
        public static readonly Type ReadOnlySpanByte = typeof(ReadOnlySpan<byte>);

        public static readonly Type ReadOnlySpanByteRef = typeof(ReadOnlySpan<byte>).MakeByRefType();

        public static readonly Type Utf8JsonReaderRef = typeof(Utf8JsonReader).MakeByRefType();

        public static readonly ConstructorInfo CtorOfObject =
            TypeInfoHelper.GetConstructor(() => new object());

        public static readonly ConstructorInfo CtorOfIgnoresAccessChecksToAttribute =
            TypeInfoHelper.GetConstructor(() => new IgnoresAccessChecksToAttribute(string.Empty));

        public static readonly LazyMethod TypeGetTypeFromHandle =
            Lazy(() => TypeInfoHelper.GetStaticMethod(() => Type.GetTypeFromHandle(new RuntimeTypeHandle())));

        public static readonly LazyMethod DelegateCombine =
            Lazy(() => TypeInfoHelper.GetStaticMethod(() => Delegate.Combine(null, null)));

        public static readonly LazyMethod DelegateRemove =
            Lazy(() => TypeInfoHelper.GetStaticMethod(() => Delegate.Remove(null, null)));

        public static readonly LazyMethod InterlockedCompareExchange =
            Lazy(() => TypeInfoHelper.GetGenericStaticMethod<PropertyChangingEventHandler, PropertyChangingEventHandler>(param => Interlocked.CompareExchange(ref param, null, null)));

        public static readonly MethodInfo MemoryExtensionsSequenceEqual =
          TypeInfoHelper.GetStaticMethod(typeof(ProxyHelperFunctions), nameof(ProxyHelperFunctions.SequenceEqual), new[] { typeof(ReadOnlySpan<byte>), typeof (ReadOnlySpan<byte>)});

        public static readonly MethodInfo HelperGetFastHashCode = 
            TypeInfoHelper.GetStaticMethod(typeof(ProxyHelperFunctions), nameof(ProxyHelperFunctions.GetFastHashCode), new[] { typeof(ReadOnlySpan<byte>) });

        public static readonly MethodInfo HelperGetFastHashCodeRef = 
            TypeInfoHelper.GetStaticMethod(typeof(ProxyHelperFunctions), nameof(ProxyHelperFunctions.GetFastHashCodeRef), new [] { ReadOnlySpanByteRef });

        public static readonly MethodInfo HelperSequenceEquals = 
            TypeInfoHelper.GetStaticMethod(typeof(ProxyHelperFunctions), nameof(ProxyHelperFunctions.SequenceEquals), new [] { ReadOnlySpanByteRef, ReadOnlySpanByte });

        public static readonly LazyMethod HelperParseEnum =
            Lazy(() => TypeInfoHelper.GetStaticMethod(typeof(ProxyHelperFunctions), nameof(ProxyHelperFunctions.ParseEnum), new[] { ReadOnlySpanByteRef }));

        public static readonly LazyMethod HelperReadChar =
            Lazy(() => TypeInfoHelper.GetStaticMethod(typeof(ProxyHelperFunctions), nameof(ProxyHelperFunctions.ReadChar), new [] { Utf8JsonReaderRef }));

        public static readonly LazyMethod HelperCompareByteArrays =
            Lazy(() => TypeInfoHelper.GetStaticMethod(() => ProxyHelperFunctions.CompareByteArrays(null, null)));

        public static readonly MethodInfo HelperGetStringAsReadOnlySpan =
            TypeInfoHelper.GetStaticMethod(typeof(ProxyHelperFunctions), nameof(ProxyHelperFunctions.GetStringAsReadOnlySpan), new [] { Utf8JsonReaderRef });

        public static readonly MethodInfo HelperThrowTypeNotFound =
            TypeInfoHelper.GetStaticMethod(typeof(ProxyHelperFunctions), nameof(ProxyHelperFunctions.HelperThrowTypeNotFound), new[] { typeof(ReadOnlySpan<byte>) });

        public static readonly MethodInfo HelperThrowPropertyNotFound =
            TypeInfoHelper.GetStaticMethod(typeof(ProxyHelperFunctions), nameof(ProxyHelperFunctions.HelperThrowPropertyNotFound), new[] { typeof(ReadOnlySpan<byte>) });

        public static readonly MethodInfo HelperThrowString =
            TypeInfoHelper.GetStaticMethod(() => ProxyHelperFunctions.ThrowString(string.Empty));

        public static readonly MethodInfo HelperThrowUnknownType =
            TypeInfoHelper.GetStaticMethod(() => ProxyHelperFunctions.ThrowUnknownType(typeof(ProxyHelperFunctions)));

        public static readonly MethodInfo Utf8JsonReaderRead =
            TypeInfoHelper.GetInstanceMethod(typeof(Utf8JsonReader), nameof(Utf8JsonReader.Read));

        public static readonly LazyMethod Utf8JsonReaderTokenType =
            Lazy(() => TypeInfoHelper.GetPropertyGetter(typeof(Utf8JsonReader), nameof(Utf8JsonReader.TokenType)));

        public static readonly MethodInfo JsonEncodedTextToReadOnlySpan =
            TypeInfoHelper.GetPropertyGetter(typeof(JsonEncodedText), nameof(JsonEncodedText.EncodedUtf8Bytes));

        public static readonly MethodInfo WriteJsonEncodedText = 
            TypeInfoHelper.GetInstanceMethod(() => new Utf8JsonWriter(Stream.Null, new JsonWriterOptions()).WriteString(JsonEncodedText.Encode(string.Empty, null), JsonEncodedText.Encode(string.Empty, null)));

        public static readonly LazyConstructor ReadOnlySpanOfByteConstructor =
            Lazy(() => TypeInfoHelper.GetConstructor(typeof(ReadOnlySpan<byte>), new[] { typeof(byte[]) }));

        // no need for thread safety as the data is static. Sure there is a change that initializer will be called several times.
        private static LazyMethod Lazy(Func<MethodInfo> func) => new LazyMethod(func);

        private static LazyConstructor Lazy(Func<ConstructorInfo> func) => new LazyConstructor(func);
    }
}
