using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using Clutch.Building.ProxySupport;
using Clutch.Utility;

namespace Clutch.Building
{
    public static class PropertyTypeHandlerCache<T>
    {
        public static PropertyTypeHandler Value { get; set; }

        public static Func<PropertyTypeHandler> DefaultTypeCreator { get; set; }
    }

    public class PropertyTypeHandlerFactory
    {
        private static JsonEncodedText PropertyName => JsonEncodedText.Encode(string.Empty);

        private readonly Dictionary<Type, PropertyTypeHandler> _handlers = new Dictionary<Type, PropertyTypeHandler>();

        private static readonly Dictionary<Type, Func<PropertyTypeHandlerFactory, PropertyTypeHandlerBuildContext, PropertyTypeHandler>> s_handlerFactories = 
            new Dictionary<Type, Func<PropertyTypeHandlerFactory, PropertyTypeHandlerBuildContext, PropertyTypeHandler>>();

        private readonly Dictionary<Type, PropertyTypeHandlerEnumCacheEntry> _enumCache = new Dictionary<Type, PropertyTypeHandlerEnumCacheEntry>();

        private static readonly LazyMethod s_getPropertyTypeHandlerMethodInfo;

        static PropertyTypeHandlerFactory()
        {
            s_getPropertyTypeHandlerMethodInfo = new LazyMethod(() => TypeInfoHelper.GetInstanceMethod(typeof(PropertyTypeHandlerFactory), nameof(GetPropertyTypeHandler)));

            AddDefaultType<bool>(() => new PropertyTypeHandler
                                       {
                                           CompareAndGotoLabel = PrimitiveTypeHandlers.BeqComparer,
                                           Serialize = PrimitiveTypeHandlers.SimpleSerializer<bool>(w => w.WriteBoolean(PropertyName, false), w => w.WriteBooleanValue(false)),
                                           Deserialize = PrimitiveTypeHandlers.Deserializer(nameof(Utf8JsonReader.GetBoolean)),
                                           LoadDefaultValue = PrimitiveTypeHandlers.LoadDefaultInteger<bool>(s => s ? 1 : 0),
                                           IsSystemDefaultValue = PrimitiveTypeHandlers.IsSystemDefaultSimple
            });

            AddDefaultType<byte>(() => new PropertyTypeHandler
                                       {
                                           CompareAndGotoLabel = PrimitiveTypeHandlers.BeqComparer,
                                           Serialize = PrimitiveTypeHandlers.UnsignedLongSerializer,
                                           Deserialize = PrimitiveTypeHandlers.Deserializer(nameof(Utf8JsonReader.GetUInt32)),
                                           LoadDefaultValue = PrimitiveTypeHandlers.LoadDefaultInteger<byte>(s => s),
                                           IsSystemDefaultValue = PrimitiveTypeHandlers.IsSystemDefaultSimple
                                       });

            AddDefaultType<sbyte>(() => new PropertyTypeHandler
                                        {
                                            CompareAndGotoLabel = PrimitiveTypeHandlers.BeqComparer,
                                            Serialize = PrimitiveTypeHandlers.SignedLongSerializer,
                                            Deserialize = PrimitiveTypeHandlers.Deserializer(nameof(Utf8JsonReader.GetInt32)),
                                            LoadDefaultValue = PrimitiveTypeHandlers.LoadDefaultInteger<sbyte>(s => s),
                                           IsSystemDefaultValue = PrimitiveTypeHandlers.IsSystemDefaultSimple
                                        });


            AddDefaultType<short>(() => new PropertyTypeHandler
                                        {
                                            CompareAndGotoLabel = PrimitiveTypeHandlers.BeqComparer,
                                            Serialize = PrimitiveTypeHandlers.SignedLongSerializer,
                                            Deserialize = PrimitiveTypeHandlers.Deserializer(nameof(Utf8JsonReader.GetInt32)),
                                            LoadDefaultValue = PrimitiveTypeHandlers.LoadDefaultInteger<short>(s => s),
                                            IsSystemDefaultValue = PrimitiveTypeHandlers.IsSystemDefaultSimple
                                        });

            AddDefaultType<ushort>(() => new PropertyTypeHandler
                                         {
                                             CompareAndGotoLabel = PrimitiveTypeHandlers.BeqComparer,
                                             Serialize = PrimitiveTypeHandlers.UnsignedLongSerializer,
                                             Deserialize = PrimitiveTypeHandlers.Deserializer(nameof(Utf8JsonReader.GetUInt32)),
                                             LoadDefaultValue = PrimitiveTypeHandlers.LoadDefaultInteger<ushort>(s => s),
                                             IsSystemDefaultValue = PrimitiveTypeHandlers.IsSystemDefaultSimple
            });

            AddDefaultType<int>(() => new PropertyTypeHandler
                                   {
                                       CompareAndGotoLabel = PrimitiveTypeHandlers.BeqComparer,
                                       Serialize = PrimitiveTypeHandlers.SignedLongSerializer,
                                       Deserialize = PrimitiveTypeHandlers.Deserializer(nameof(Utf8JsonReader.GetInt32)),
                                       LoadDefaultValue = PrimitiveTypeHandlers.LoadDefaultInteger<int>(s => s),
                                       IsSystemDefaultValue = PrimitiveTypeHandlers.IsSystemDefaultSimple
            });

            AddDefaultType<uint>(() => new PropertyTypeHandler
                                   {
                                       CompareAndGotoLabel = PrimitiveTypeHandlers.BeqComparer,
                                       Serialize = PrimitiveTypeHandlers.UnsignedLongSerializer,
                                       Deserialize = PrimitiveTypeHandlers.Deserializer(nameof(Utf8JsonReader.GetUInt32)),
                                       LoadDefaultValue = PrimitiveTypeHandlers.LoadDefaultInteger<uint>(s => unchecked((int)s)),
                                       IsSystemDefaultValue = PrimitiveTypeHandlers.IsSystemDefaultSimple
            });

            AddDefaultType<long>(() => new PropertyTypeHandler
                                   {
                                       CompareAndGotoLabel = PrimitiveTypeHandlers.BeqComparer,
                                       Serialize = PrimitiveTypeHandlers.SignedLongSerializer,
                                       Deserialize = PrimitiveTypeHandlers.Deserializer(nameof(Utf8JsonReader.GetInt64)),
                                       LoadDefaultValue = PrimitiveTypeHandlers.LoadDefaultInt64<long>(s => s),
                                       IsSystemDefaultValue = PrimitiveTypeHandlers.IsSystemDefaultSimple
            });

            AddDefaultType<ulong>(() => new PropertyTypeHandler
                                        {
                                            CompareAndGotoLabel = PrimitiveTypeHandlers.BeqComparer,
                                            Serialize = PrimitiveTypeHandlers.UnsignedLongSerializer,
                                            Deserialize = PrimitiveTypeHandlers.Deserializer(nameof(Utf8JsonReader.GetUInt64)),
                                            LoadDefaultValue = PrimitiveTypeHandlers.LoadDefaultInt64<ulong>(s => unchecked((int)s)),
                                            IsSystemDefaultValue = PrimitiveTypeHandlers.IsSystemDefaultSimple
                                        });

            AddDefaultType<IntPtr>(() => new PropertyTypeHandler
                                         {
                                             CompareAndGotoLabel = PrimitiveTypeHandlers.BeqComparer,
                                             Serialize = PrimitiveTypeHandlers.ConvertSerializer<IntPtr, long>(c => c.ToInt64(), w => w.WriteNumber(PropertyName, 0L), w => w.WriteNumberValue(0L)),
                                             Deserialize = PrimitiveTypeHandlers.Deserializer(nameof(Utf8JsonReader.GetInt64)),
                                             LoadDefaultValue = PrimitiveTypeHandlers.LoadDefaultStruct<IntPtr>(),
                                             IsSystemDefaultValue = PrimitiveTypeHandlers.IsSystemDefaultStruct<IntPtr>(PrimitiveTypeHandlers.BeqComparer)
                                         });

            AddDefaultType<UIntPtr>(() => new PropertyTypeHandler
                                          {
                                              CompareAndGotoLabel = PrimitiveTypeHandlers.BeqComparer,
                                              Serialize = PrimitiveTypeHandlers.ConvertSerializer<UIntPtr, ulong>(c => c.ToUInt64(), w => w.WriteNumber(PropertyName, 0UL), w => w.WriteNumberValue(0UL)),
                                              Deserialize = PrimitiveTypeHandlers.Deserializer(nameof(Utf8JsonReader.GetUInt64)),
                                              LoadDefaultValue = PrimitiveTypeHandlers.LoadDefaultStruct<UIntPtr>(),
                                              IsSystemDefaultValue = PrimitiveTypeHandlers.IsSystemDefaultStruct<IntPtr>(PrimitiveTypeHandlers.BeqComparer)
                                          });

            AddDefaultType<char>(() => new PropertyTypeHandler
                                       {
                                           CompareAndGotoLabel = PrimitiveTypeHandlers.BeqComparer,
                                           Serialize = PrimitiveTypeHandlers.ConvertSerializer<char, string>(c => c.ToString(), w => w.WriteString(PropertyName, string.Empty), w => w.WriteStringValue(string.Empty)),
                                           Deserialize = PrimitiveTypeHandlers.CharDeserializer,
                                           LoadDefaultValue = PrimitiveTypeHandlers.LoadDefaultInteger<char>(s => s),
                                           IsSystemDefaultValue = PrimitiveTypeHandlers.IsSystemDefaultSimple
                                       });

            AddDefaultType<double>(() => new PropertyTypeHandler
                                         {
                                             CompareAndGotoLabel = PrimitiveTypeHandlers.BeqComparer,
                                             Serialize = PrimitiveTypeHandlers.SimpleSerializer<double>(w => w.WriteNumber(PropertyName, 0.0), w => w.WriteNumberValue(0.0)),
                                             Deserialize = PrimitiveTypeHandlers.Deserializer(nameof(Utf8JsonReader.GetDouble)),
                                             LoadDefaultValue = PrimitiveTypeHandlers.LoadDefaultAny<double>((g, val) => g.LoadDouble(val)),
                                             IsSystemDefaultValue = PrimitiveTypeHandlers.IsSystemDefaultAny(g => g.LoadDouble(0.0))
            });

            AddDefaultType<float>(() => new PropertyTypeHandler
                                        {
                                            CompareAndGotoLabel = PrimitiveTypeHandlers.BeqComparer,
                                            Serialize = PrimitiveTypeHandlers.SimpleSerializer<float>(w => w.WriteNumber(PropertyName, (float)0), w => w.WriteNumberValue((float)0)),
                                            Deserialize = PrimitiveTypeHandlers.Deserializer(nameof(Utf8JsonReader.GetSingle)),
                                            LoadDefaultValue = PrimitiveTypeHandlers.LoadDefaultAny<float>((g, val) => g.LoadSingle(val)),
                                            IsSystemDefaultValue = PrimitiveTypeHandlers.IsSystemDefaultAny(g => g.LoadSingle(0.0f))
            });

            AddDefaultType<string>(() => new PropertyTypeHandler
                                         {
                                             CompareAndGotoLabel = PrimitiveTypeHandlers.StringComparer,
                                             Serialize = PrimitiveTypeHandlers.StringSerializer,
                                             Deserialize = PrimitiveTypeHandlers.StringDeserializer,
                                             LoadDefaultValue = PrimitiveTypeHandlers.LoadDefaultAny<string>((g, val) => g.LoadString(val)),
                                             IsSystemDefaultValue = PrimitiveTypeHandlers.IsSystemDefaultSimple
                                         });

            AddDefaultType<Guid>(() => new PropertyTypeHandler
                                       {
                                           CompareAndGotoLabel = PrimitiveTypeHandlers.StructComparer<Guid>(v => v.Equals(v)),
                                           Serialize = PrimitiveTypeHandlers.SimpleSerializer<Guid>(w => w.WriteString(PropertyName, Guid.Empty), w => w.WriteStringValue(Guid.Empty)),
                                           Deserialize = PrimitiveTypeHandlers.Deserializer(nameof(Utf8JsonReader.GetGuid)),
                                           LoadDefaultValue = PrimitiveTypeHandlers.LoadDefaultStruct<Guid>(),
                                           IsSystemDefaultValue = PrimitiveTypeHandlers.IsSystemDefaultStruct<Guid>(PrimitiveTypeHandlers.StructComparer<Guid>(v => v.Equals(v)))
                                       });

            AddDefaultType<decimal>(() => new PropertyTypeHandler
                                          {
                                              CompareAndGotoLabel = PrimitiveTypeHandlers.StructComparer<Decimal>(v => v.Equals(v)),
                                              Serialize = PrimitiveTypeHandlers.SimpleSerializer<decimal>(w => w.WriteNumber(PropertyName, 0m), w => w.WriteNumberValue(0m)),
                                              Deserialize = PrimitiveTypeHandlers.Deserializer(nameof(Utf8JsonReader.GetDecimal)),
                                              LoadDefaultValue = PrimitiveTypeHandlers.LoadDefaultStruct<decimal>(),
                                              IsSystemDefaultValue = PrimitiveTypeHandlers.IsSystemDefaultStruct<decimal>(PrimitiveTypeHandlers.StructComparer<Decimal>(v => v.Equals(v)))
                                          });

            AddDefaultType<DateTime>(() => new PropertyTypeHandler
                                           {
                                               CompareAndGotoLabel = PrimitiveTypeHandlers.StructComparer<DateTime>(v => v.Equals(v)),
                                               Serialize = PrimitiveTypeHandlers.SimpleSerializer<DateTime>(w => w.WriteString(PropertyName, DateTime.MinValue), w => w.WriteStringValue(DateTime.MinValue)),
                                               Deserialize = PrimitiveTypeHandlers.Deserializer(nameof(Utf8JsonReader.GetDateTime)),
                                               LoadDefaultValue = PrimitiveTypeHandlers.LoadDefaultStruct<DateTime>(),
                                               IsSystemDefaultValue = PrimitiveTypeHandlers.IsSystemDefaultStruct<DateTime>(PrimitiveTypeHandlers.StructComparer<DateTime>(v => v.Equals(v)))
                                           });

            AddDefaultType<DateTimeOffset>(() => new PropertyTypeHandler
                                                 {
                                                     CompareAndGotoLabel = PrimitiveTypeHandlers.StructComparer<DateTimeOffset>(v => v.Equals(v)),
                                                     Serialize = PrimitiveTypeHandlers.SimpleSerializer<DateTimeOffset>(w => w.WriteString(PropertyName, DateTimeOffset.MinValue), w => w.WriteStringValue(DateTimeOffset.MinValue)),
                                                     Deserialize = PrimitiveTypeHandlers.Deserializer(nameof(Utf8JsonReader.GetDateTimeOffset)),
                                                     LoadDefaultValue = PrimitiveTypeHandlers.LoadDefaultStruct<DateTimeOffset>(),
                                                     IsSystemDefaultValue = PrimitiveTypeHandlers.IsSystemDefaultStruct<DateTimeOffset>(PrimitiveTypeHandlers.StructComparer<DateTimeOffset>(v => v.Equals(v)))
                                                 });

            AddDefaultType<byte[]>(() => new PropertyTypeHandler
                                                 {
                                                     CompareAndGotoLabel = PrimitiveTypeHandlers.ByteArrayComparer,
                                                     Serialize = PrimitiveTypeHandlers.ByteArraySerializer,
                                                     Deserialize = PrimitiveTypeHandlers.ByteArrayDeserializer,
                                                     LoadDefaultValue = PrimitiveTypeHandlers.LoadDefaultStruct<byte[]>(),
                                                     IsSystemDefaultValue = PrimitiveTypeHandlers.IsSystemDefaultSimple
                                                 });
        }

        internal PropertyTypeHandlerEnumCacheEntry GetOrCreateEnumEntry(Type type, Func<PropertyTypeHandlerEnumCacheEntry> builder)
        {
            if (!_enumCache.TryGetValue(type, out var entry))
            {
                entry = builder();
                _enumCache[type] = entry;
            }

            return entry;
        }

        private static void AddDefaultType<T>(Func<PropertyTypeHandler> creator)
        {
            PropertyTypeHandlerCache<T>.DefaultTypeCreator = creator;
            s_handlerFactories[typeof(T)] = (f, ctx) => f.GetPropertyTypeHandler<T>(ctx);
        }

        public PropertyTypeHandler GetPropertyTypeHandler<T>(PropertyTypeHandlerBuildContext ctx)
        {
            PropertyTypeHandler result = PropertyTypeHandlerCache<T>.Value;
            if (result != null)
                return result;

            if (PropertyTypeHandlerCache<T>.DefaultTypeCreator != null)
                result = PropertyTypeHandlerCache<T>.DefaultTypeCreator();
            else
                result = BuildPropertyTypeHandler<T>(ctx);

            PropertyTypeHandlerCache<T>.Value = result;
            _handlers[typeof(T)] = result;
            return result;
        }

        private PropertyTypeHandler BuildPropertyTypeHandler<T>(PropertyTypeHandlerBuildContext ctx)
        {
            var type = typeof(T);
            if (type.IsPrimitive)
            {
                // types must be prebuilt
                throw new ClutchInternalErrorException($"Unexpected primitive type '{type}'");
            }

            if (type.IsEnum)
                return PropertyTypeHandlerEnum.Build<T>(this, ctx); 

            if (type.IsGenericType && !type.IsGenericTypeDefinition && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                return PropertyTypeHandlerNullable.Build<T>(this, ctx);

            if (type.IsGenericType && !type.IsGenericTypeDefinition && type.GetGenericTypeDefinition() == typeof(IList<>))
                return PropertyTypeHandlerCollection.BuildList<T>(this, ctx);

            // type is not supported...
            ctx.Property.Issue(CoreIssues.TypeIsNotSupported, type);

            return PropertyTypeHandler.Null;
        }

        public PropertyTypeHandler GetUnderlyingTypeHandler(Type type, PropertyTypeHandlerBuildContext ctx)
        {
            if (_handlers.TryGetValue(type, out var handler))
                return handler;

            if (s_handlerFactories.TryGetValue(type, out var factory))
                return factory(this, ctx);

            // not a default or cached (already created), so reflection
            var method = s_getPropertyTypeHandlerMethodInfo.Value;
            var generic = method.MakeGenericMethod(type);
            try
            {
                return (PropertyTypeHandler)generic.Invoke(this, new object []{ ctx });
            }
            catch (TargetInvocationException exception)
            {
                throw new ClutchInternalErrorException($"Exception while requesting PropertyTypeHandler for {type}. See inner exception for details.", exception.InnerException);
            }
        }

        
    }
}
