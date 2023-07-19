using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.Json;
using Clutch.Building.ProxySupport;

namespace Clutch.Building
{
    public static class PrimitiveTypeHandlers
    {
        internal static JsonEncodedText LambdaText() => JsonEncodedText.Encode(string.Empty);

        public static void BeqComparer(ComparePropertyContext ctx)
        {
            ctx.LoadValue1(ctx.Generator, false); 
            ctx.LoadValue2(ctx.Generator);
            ctx.Generator.IfEqualGoto(ctx.GotoLabelIfEqual);
        }

        public static void IsSystemDefaultSimple(IsSystemDefaultPropertyContext ctx)
        {
            ctx.LoadValue(ctx.Generator, false);
            ctx.Generator.IfFalseGoto(ctx.GotoLabelIfEqual);
        }

        public static void StringComparer(ComparePropertyContext ctx)
        {
            var g = ctx.Generator;
            ctx.LoadValue1(g, false);
            ctx.LoadValue2(g);
            g.Call(PerTypeMethods<string>.GetEquals(s => string.Equals(string.Empty, string.Empty)));
            g.IfTrueGoto(ctx.GotoLabelIfEqual);
        }

        public static void ByteArrayComparer(ComparePropertyContext ctx)
        {
            var g = ctx.Generator;
            ctx.LoadValue1(g, false);
            ctx.LoadValue2(g);
            g.Call(MethodInfos.HelperCompareByteArrays.Value);
            g.IfTrueGoto(ctx.GotoLabelIfEqual);
        }

        public static Action<ComparePropertyContext> StructComparer<T>(Expression<Func<T, bool>> expression)
        {
            return ctx =>
                   {
                       var g = ctx.Generator;
                       ctx.LoadValue1(g, true);
                       ctx.LoadValue2(g);
                       g.Call(PerTypeMethods<T>.GetEquals(expression));
                       g.IfTrueGoto(ctx.GotoLabelIfEqual);
                   };
        }

        public static void SignedLongSerializer(SerializePropertyContext context)
        {
            LoadWriterAndPropertyNameAndValue(context);
            if (context.Property.ReturnType != typeof(long))
                context.Generator.Emit(OpCodes.Conv_I8);
            context.Generator.Call(PerTypeMethods<long>.GetWriter(context.WithPropertyName, w => w.WriteNumber(LambdaText(), 0L), w => w.WriteNumberValue(0L)));
        }

        public static void UnsignedLongSerializer(SerializePropertyContext context)
        {
            LoadWriterAndPropertyNameAndValue(context);
            if (context.Property.ReturnType != typeof(ulong))
                context.Generator.Emit(OpCodes.Conv_U8);
            context.Generator.Call(PerTypeMethods<ulong>.GetWriter(context.WithPropertyName, w => w.WriteNumber(LambdaText(), 0UL), w => w.WriteNumberValue(0UL)));
        }

        public static void CharDeserializer(DeserializePropertyContext ctx)
        {
            ctx.ReaderLoader();
            ctx.Generator.Call(MethodInfos.HelperReadChar.Value);
        }

        public static Action<SerializePropertyContext> SimpleSerializer<T>(Expression<Action<Utf8JsonWriter>> expressionWithProperty, Expression<Action<Utf8JsonWriter>> expressionWithoutProperty)
        {
            return s =>
                   {
                       LoadWriterAndPropertyNameAndValue(s);
                       s.Generator.Call(PerTypeMethods<T>.GetWriter(s.WithPropertyName, expressionWithProperty, expressionWithoutProperty));
                   };
        }

        public static Action<SerializePropertyContext> ReferenceTypeSerializer(Action<ILGenerator> serializeNotNull)
        {
            return s =>
                   {
                       var g = s.Generator;
                       LoadWriterAndPropertyNameAndValue(s);

                       var nullSerializetion = g.DefineLabel();
                       var end = g.DefineLabel();

                       g.Duplicate(); // duplicate value

                       g.IfFalseGoto(nullSerializetion);
                       {
                           serializeNotNull(g);
                           
                           g.Goto(end);
                       }
                       g.MarkLabel(nullSerializetion);
                       {
                           g.Pop();
                           g.Call(PerTypeMethods<object>.GetWriter(s.WithPropertyName, w => w.WriteNull(LambdaText()), w => w.WriteNullValue()));
                       }
                       g.MarkLabel(end);

                   };
        }

        public static void StringSerializer(SerializePropertyContext ctx)
        {
            ReferenceTypeSerializer(g => g.Call(PerTypeMethods<string>.GetWriter(ctx.WithPropertyName, w => w.WriteString(LambdaText(), string.Empty), w => w.WriteStringValue(string.Empty))))(ctx);
        }

        public static void ByteArraySerializer(SerializePropertyContext ctx)
        {
            ReferenceTypeSerializer(g =>
                                            {
                                                g.New(MethodInfos.ReadOnlySpanOfByteConstructor.Value);
                                                g.Call(PerTypeMethods<byte[]>.GetWriter(ctx.WithPropertyName, w => w.WriteBase64String(LambdaText(), Array.Empty<byte>()), w => w.WriteBase64StringValue(Array.Empty<byte>())));
                                            })(ctx);
        }

        public static Action<DeserializePropertyContext> ReferenceTypeDeserializer(MethodInfo readerMethod)
        {
            return s =>
            {
                var g = s.Generator;
                var end = g.DefineLabel();
                var parseValue = g.DefineLabel();

                s.ReaderLoader();
                g.Call(MethodInfos.Utf8JsonReaderTokenType.Value);
                g.LoadInteger((int)JsonTokenType.Null);
                g.IfNotEqualGoto(parseValue);
                {
                    // handle null
                    g.LoadNull();
                    g.Goto(end);
                }
                g.MarkLabel(parseValue);
                {
                    s.ReaderLoader();
                    s.Generator.Call(readerMethod);
                }
                g.MarkLabel(end);
            };
        }

        public static Action<DeserializePropertyContext> ReferenceTypeDeserializer<T>(Expression<Func<T>> expression)
        {
            return ReferenceTypeDeserializer(GetReaderMethod(expression));
        }

        public static void StringDeserializer(DeserializePropertyContext ctx)
        {
            ReferenceTypeDeserializer(TypeInfoHelper.GetInstanceMethod(typeof(Utf8JsonReader), nameof(Utf8JsonReader.GetString)))(ctx);
        }

        public static void ByteArrayDeserializer(DeserializePropertyContext ctx)
        {
            ReferenceTypeDeserializer(TypeInfoHelper.GetInstanceMethod(typeof(Utf8JsonReader), nameof(Utf8JsonReader.GetBytesFromBase64)))(ctx);
        }


        public static Action<SerializePropertyContext> ConvertSerializer<TFrom, TTo>(Expression<Func<TFrom,TTo>> converter, Expression<Action<Utf8JsonWriter>> expressionWithProperty, Expression<Action<Utf8JsonWriter>> expressionWithoutProperty)
        {
            return s =>
                   {
                       LoadWriterAndPropertyNameAndValue(s, true);
                       s.Generator.Call(PerTypeConverters<TFrom, TTo>.Get(converter));
                       s.Generator.Call(PerTypeMethods<TTo>.GetWriter(s.WithPropertyName, expressionWithProperty, expressionWithoutProperty));
                   };
        }


        internal static void LoadWriterAndPropertyNameAndValue(SerializePropertyContext context, bool address = false)
        {
            context.Generator.LoadArgument(1);
            if (context.WithPropertyName)
                context.Generator.LoadStaticField(context.Property.EscapedPropertyNameField.FieldBuilder);
            context.GenerateGetPropertyValue(address);
        }

        public static Action<DeserializePropertyContext> Deserializer<T>(Expression<Func<T>> expression) =>
            Deserializer(GetReaderMethod(expression));

        public static Action<DeserializePropertyContext> Deserializer(MethodInfo readerMethod)
        {
            return s =>
            {
                s.ReaderLoader();
                s.Generator.Call(readerMethod);
            };
        }

        public static Action<DeserializePropertyContext> Deserializer(string utf8JsonReaderMethod) =>
            Deserializer(TypeInfoHelper.GetInstanceMethod(typeof(Utf8JsonReader), utf8JsonReaderMethod));

        private static MethodInfo GetReaderMethod<T>(Expression<Func<T>> expression) => PerTypeMethods<T>.GetReader(expression);

        public static Action<LoadDefaultPropertyContext> LoadDefaultInteger<T>(Func<T, int> converter)
        {
            return s =>
                   {
                       var load = converter(s.GetDefaultValue<T>());
                       s.Generator.LoadInteger(load);
                   };
        }

        public static Action<LoadDefaultPropertyContext> LoadDefaultInt64<T>(Func<T, long> converter)
        {
            return s =>
                   {
                       var load = converter(s.GetDefaultValue<T>());
                       s.Generator.LoadInt64(load);
                   };
        }

        public static Action<LoadDefaultPropertyContext> LoadDefaultAny<T>(Action<ILGenerator, T> loader)
        {
            return s => loader(s.Generator, s.GetDefaultValue<T>());
        }

        public static Action<IsSystemDefaultPropertyContext> IsSystemDefaultAny(Action<ILGenerator> loader)
        {
            return s =>
                   {
                       s.LoadValue(s.Generator, false);
                       loader(s.Generator);
                       s.Generator.IfEqualGoto(s.GotoLabelIfEqual);
                   };
        }

        public static Action<LoadDefaultPropertyContext> LoadDefaultStruct<T>()
        {
            return s =>
                   {
                       var val = s.GetDefaultValue<T>();
                       var staticField = s.Property.AssemblyContext.StaticFields.AddGroupedByValue("Default" + s, () => val);
                       s.Generator.LoadStaticField(staticField.FieldBuilder);
                   };
        }

        public static Action<IsSystemDefaultPropertyContext> IsSystemDefaultStruct<T>(Action<ComparePropertyContext> comparer)
        {
            return s =>
                   {
                       var staticField = s.Property.AssemblyContext.StaticFields.AddGroupedByValue("SystemDefault" + s, () => default(T));

                       var ctx = new ComparePropertyContext
                                 {
                                     Generator = s.Generator,
                                     Property = s.Property,
                                     LoadValue1 = s.LoadValue,
                                     LoadValue2 = g => g.LoadStaticField(staticField.FieldBuilder),
                                     GotoLabelIfEqual = s.GotoLabelIfEqual
                                 };
                       comparer(ctx);
                   };
        }
    }
}
