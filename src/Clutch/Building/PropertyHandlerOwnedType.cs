using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using Clutch.Building.ProxySupport;
using Clutch.Helpers;

namespace Clutch.Building
{
    class PropertyHandlerOwnedType<T> : IPropertyHandler
    {
        //TODO: move out
        private static readonly JsonEncodedText s_discriminator = JsonEncodedText.Encode(ProxyBuilderConstants.DiscriminatorPropertyName);

        public PropertyHandlerOwnedType(TypeGraphNode node, TypeGraphProperty property)
        {
        }

        public void GenerateInit(LoadDefaultPropertyContext ctx)
        {
        }

        public void GenerateSerialize(SerializePropertyContext ctx)
        {
            var g = ctx.Generator;
            PrimitiveTypeHandlers.LoadWriterAndPropertyNameAndValue(ctx);

            var nullSerializetion = g.DefineLabel();
            var end = g.DefineLabel();

            g.Duplicate(); // duplicate value

            g.IfFalseGoto(nullSerializetion);
            {
                g.Call(TypeInfoHelper.GetStaticMethod(() => WriteObject(new Utf8JsonWriter(Stream.Null, new JsonWriterOptions()), PrimitiveTypeHandlers.LambdaText(), null))); //TODO:cache
                g.Goto(end);
            }
            g.MarkLabel(nullSerializetion);
            {
                g.Pop();
                g.Call(PerTypeMethods<object>.GetWriter(ctx.WithPropertyName, w => w.WriteNull(PrimitiveTypeHandlers.LambdaText()), w => w.WriteNullValue()));
            }
            g.MarkLabel(end);
        }

        public void GenerateDeserialize(DeserializePropertyContext ctx)
        {
            var g = ctx.Generator;

            g.LoadThis();
            
            var end = g.DefineLabel();
            var parseValue = g.DefineLabel();

            ctx.ReaderLoader();
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
                ctx.ReaderLoader();
                ctx.Generator.Call(TypeInfoHelper.GetStaticMethod(typeof(PropertyTypeHandlerContextType), nameof(ReadObject), new[] { MethodInfos.Utf8JsonReaderRef })); //TODO:cache
                ctx.ReaderLoader();
                ctx.Generator.Call(ctx.Property.AssemblyContext.Serializer.MethodBuilder);
            }
            g.MarkLabel(end);
            ctx.PropertySetter(g, ctx.Property, true);
        }

        public void GenerateSetter(PropertySetterContext ctx)
        {
            CallSetterInfluencers(ctx, PropertySetterStage.BeforeComparison);

            var g = ctx.Generator;
            var endOfComparison = g.DefineLabel();
            {
                if (ctx.SetterMode == PropertySetterMode.CompareAndSet)
                {
                    // TODO
                }

                //compare here
                CallSetterInfluencers(ctx, PropertySetterStage.BeforeSet);

                g.LoadThis();
                ctx.LoadValueArgument(g);
                ctx.PropertySetter(g, ctx.Property, false);
                CallSetterInfluencers(ctx, PropertySetterStage.AfterSet);
            }
            g.MarkLabel(endOfComparison);
            CallSetterInfluencers(ctx, PropertySetterStage.AfterComparison);
        }


        private static void WriteObject(Utf8JsonWriter writer, JsonEncodedText property, object obj)
        {
            writer.WriteStartObject(property);

            IUtf8JsonSerializable provider = (IUtf8JsonSerializable)obj;
            provider.Serialize(writer);

            writer.WriteEndObject();
        }

        private static void ReadObject(ref Utf8JsonReader reader)
        {
            JsonTokenType tokenType = reader.TokenType;

            if (tokenType != JsonTokenType.StartObject)
                throw new ClutchRuntimeException("Start of object '{' is expected");

            reader.Read();
            tokenType = reader.TokenType;
            if (tokenType != JsonTokenType.PropertyName) //TODO: absence of Discriminator (or int discriminator)
                throw new ClutchRuntimeException("Discriminator (property name) is expected");

            if (reader.ValueTextEquals(s_discriminator.EncodedUtf8Bytes))
            {
                // fast reading
                reader.Read();
                if (reader.TokenType != JsonTokenType.String)
                    throw new ClutchRuntimeException("String value for discriminator (property name) is expected");
            }
            else
                throw new ClutchRuntimeException($"Discriminator ('{ProxyBuilderConstants.DiscriminatorPropertyName}') should by the first property in an object");
        }

        //TODO: move out
        private static void CallSetterInfluencers(PropertySetterContext setterContext, PropertySetterStage stage)
        {
            setterContext.Stage = stage;
            setterContext.Ctx.AssemblyContext.TypeGraph.ProxyInfluencers.GetPropertySetterInfluencers().ForEach(s => s.Generate(setterContext));
        }
    }
}
