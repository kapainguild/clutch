using System;
using System.Reflection;
using System.Text.Json;
using Clutch.Building.ProxySupport;
using Clutch.Utility;

namespace Clutch.Building
{
    static class PropertyTypeHandlerNullableCache<T>
    {
        public static ThreadUnsafeLazy<FieldInfo> HasValueField { get; set; }

        public static ThreadUnsafeLazy<FieldInfo> ValueField { get; set; }

        static PropertyTypeHandlerNullableCache()
        {
            // very risky: TODO: check in all runtimes
            HasValueField = new ThreadUnsafeLazy<FieldInfo>(() => GetField("hasValue"));
            ValueField = new ThreadUnsafeLazy<FieldInfo>(() => GetField("value"));
        }

        static FieldInfo GetField(string name)
        {
            var result = typeof(T).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            if (result == null)
            {
                throw new ClutchInternalErrorException($"Field '{name}' not found on '{typeof(T)}'");
            }
            return result;
        }
    }

    static class PropertyTypeHandlerNullable
    {
        public static PropertyTypeHandler Build<T>(PropertyTypeHandlerFactory factory, PropertyTypeHandlerBuildContext ctx)
        {
            var type = typeof(T);
            var underlyingType = type.GetGenericArguments()[0];

            var handler = factory.GetUnderlyingTypeHandler(underlyingType, ctx);
            return new PropertyTypeHandler
                   {
                       CompareAndGotoLabel = c => NullableComparer<T>(c, handler),
                       Serialize = c => NullableSerializer<T>(c, handler),
                       Deserialize = c => NullableDeserializer(c, handler, underlyingType),
                       LoadDefaultValue = PrimitiveTypeHandlers.LoadDefaultStruct<T>(),
                       IsSystemDefaultValue = c => NullableIsSystemDefault<T>(c, handler)
                   };
        }

        public static void NullableComparer<T>(ComparePropertyContext ctx, PropertyTypeHandler handler)
        {
            var g = ctx.Generator;
            var hasValuesEqual = g.DefineLabel();
            var beginNotEqual = g.DefineLabel();
            var bothHasValues = g.DefineLabel();
            var hasValue = PropertyTypeHandlerNullableCache<T>.HasValueField.Value;
            var getValueOrDefault = PropertyTypeHandlerNullableCache<T>.ValueField.Value;

            ctx.LoadValue1(g, false);
            g.LoadField(hasValue);

            ctx.LoadValue2(g);
            g.LoadField(hasValue);

            g.IfEqualGoto(hasValuesEqual);
            g.Goto(beginNotEqual);

            g.MarkLabel(hasValuesEqual);
            ctx.LoadValue1(g, false);
            g.LoadField(hasValue);
            g.IfTrueGoto(bothHasValues);
            g.Goto(ctx.GotoLabelIfEqual);

            g.MarkLabel(bothHasValues);

            var underlyingCtx = new ComparePropertyContext
            {
                Property = ctx.Property,
                Generator = g,
                LoadValue1 = (gen, a) =>
                {
                    ctx.LoadValue1(gen, false);
                    if (a) gen.LoadFieldAddress(getValueOrDefault);
                    else gen.LoadField(getValueOrDefault);
                },
                LoadValue2 = (gen) =>
                {
                    ctx.LoadValue2(gen);
                    gen.LoadField(getValueOrDefault);
                },
                GotoLabelIfEqual = ctx.GotoLabelIfEqual
            };

            handler.CompareAndGotoLabel(underlyingCtx);
            g.MarkLabel(beginNotEqual);
        }

        public static void NullableIsSystemDefault<T>(IsSystemDefaultPropertyContext ctx, PropertyTypeHandler handler)
        {
            var g = ctx.Generator;
            ctx.LoadValue(g, false);
            g.LoadField(PropertyTypeHandlerNullableCache<T>.HasValueField.Value);
            g.IfFalseGoto(ctx.GotoLabelIfEqual);
        }

        internal static void NullableDeserializer(DeserializePropertyContext ctx, PropertyTypeHandler handler, Type underlyingType)
        {
            var g = ctx.Generator;

            var localDefault = g.DeclareLocal(ctx.Property.ReturnType);

            var end = g.DefineLabel();
            var parseValue = g.DefineLabel();

            ctx.ReaderLoader();
            g.Call(MethodInfos.Utf8JsonReaderTokenType.Value);
            g.LoadInteger((int)JsonTokenType.Null);
            g.IfNotEqualGoto(parseValue);
            // handle null
            g.LoadLocal(localDefault);

            g.Goto(end);
            g.MarkLabel(parseValue);

            //parse value
            handler.Deserialize(ctx);

            g.New(TypeInfoHelper.GetConstructor(ctx.Property.ReturnType, new[] { underlyingType }));

            g.MarkLabel(end);
        }

        public static void NullableSerializer<T>(SerializePropertyContext ctx, PropertyTypeHandler handler)
        {
            var g = ctx.Generator;
            var hasValue = PropertyTypeHandlerNullableCache<T>.HasValueField.Value;
            var getValueOrDefault = PropertyTypeHandlerNullableCache<T>.ValueField.Value;

            var hasValueLabel = g.DefineLabel();
            var end = g.DefineLabel();

            ctx.GenerateGetPropertyValue(false);
            g.LoadField(hasValue);

            g.IfTrueGoto(hasValueLabel);

            g.LoadArgument(1);

            if (ctx.WithPropertyName)
            {
                g.LoadStaticField(ctx.Property.EscapedPropertyNameField.FieldBuilder);
                g.Call(PerTypeMethods<object>.GetWriter(true, w => w.WriteNull(JsonEncodedText.Encode("", null)), null));
            }
            else
                g.Call(PerTypeMethods<object>.GetWriter(false, null, w => w.WriteNullValue()));

            g.Goto(end);

            g.MarkLabel(hasValueLabel);

            var ctxInner = new SerializePropertyContext
            {
                WithPropertyName = ctx.WithPropertyName,
                Generator = g,
                Property = ctx.Property,
                GenerateGetPropertyValue = a =>
                {
                    ctx.GenerateGetPropertyValue(false);
                    if (a) g.LoadFieldAddress(getValueOrDefault);
                    else g.LoadField(getValueOrDefault);
                }
            };

            handler.Serialize(ctxInner);
            g.MarkLabel(end);
        }
    }
}
