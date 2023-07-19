using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Emit;
using System.Text;
using System.Text.Json;
using Clutch.Building.ProxySupport;
using Clutch.Configuration;

namespace Clutch.Building
{
    class PropertyHandlerCollection<T, TElement> : IPropertyHandler
    {
        private IPropertyHandler _underlyingPropertyHandler;

        public PropertyHandlerCollection(IPropertyHandler underlyingPropertyHandler)
        {
            this._underlyingPropertyHandler = underlyingPropertyHandler;
        }

        public void GenerateInit(LoadDefaultPropertyContext ctx)
        {
            var option = ctx.Property.PropertyNode.Options.GetValueOrDefault(ConfigOptionDeclarations
                    .HasCollectionDefaultValue);

            if (option == CollectionDefaultValue.Empty)
            {
                var g = ctx.Generator;
                g.LoadThis();

                var ctor = typeof(List<>).MakeGenericType(typeof(TElement)).GetConstructor(Type.EmptyTypes); // TODO
                g.New(ctor);

                ctx.PropertySetter(g, ctx.Property, true);
            }
        }

        public void GenerateSerialize(SerializePropertyContext ctx)
        {
            var g = ctx.Generator;

            var nullSerializetion = g.DefineLabel();
            var end = g.DefineLabel();

            var localList = g.DeclareLocal(typeof(List<TElement>));

            ctx.GenerateGetPropertyValue(false);
            g.StoreLocal(localList);

            g.LoadLocal(localList); // duplicate value

            g.IfFalseGoto(nullSerializetion);
            {
                g.LoadArgument(1);
                if (ctx.WithPropertyName)
                    ctx.Generator.LoadStaticField(ctx.Property.EscapedPropertyNameField.FieldBuilder);

                g.Call(PerTypeMethods<Array>.GetWriter(ctx.WithPropertyName, w => w.WriteStartArray(PrimitiveTypeHandlers.LambdaText()), w => w.WriteStartArray()));

                GenerateLoop(ctx, localList);

                g.LoadArgument(1);
                g.Call(TypeInfoHelper.GetInstanceMethod(() => new Utf8JsonWriter(Stream.Null, new JsonWriterOptions()).WriteEndArray()));
                g.Goto(end);
            }
            g.MarkLabel(nullSerializetion);
            {
                g.LoadArgument(1);
                if (ctx.WithPropertyName)
                    ctx.Generator.LoadStaticField(ctx.Property.EscapedPropertyNameField.FieldBuilder);

                g.Call(PerTypeMethods<object>.GetWriter(ctx.WithPropertyName, w => w.WriteNull(PrimitiveTypeHandlers.LambdaText()), w => w.WriteNullValue()));
            }
            g.MarkLabel(end);
        }

        private void GenerateLoop(SerializePropertyContext ctx, LocalBuilder localList)
        {
            var g = ctx.Generator;
            g.LoadLocal(localList);
            var enumerator = g.DeclareLocal(typeof(List<TElement>.Enumerator));
            var current = g.DeclareLocal(typeof(TElement));
            g.Call(TypeInfoHelper.GetInstanceMethod(() => new List<TElement>().GetEnumerator()));
            g.StoreLocal(enumerator);

            var startOfLoop = g.DefineLabel();

            g.Goto(startOfLoop);

            var sample = new List<TElement>().GetEnumerator();

            var getCurrent = g.DefineAndMarkLebel();

            g.LoadLocalAddress(enumerator);
            g.Call(TypeInfoHelper.GetPropertyGetter(() => sample.Current));
            g.StoreLocal(current);

            // locad current and serialize it;
            var ctxInner = new SerializePropertyContext
            {
                GenerateGetPropertyValue = a =>
                {
                    if (a)
                        g.LoadLocalAddress(current);
                    else g.LoadLocal(current);
                },
                Generator = g,
                Property = null,
                WithPropertyName = false
            };

            _underlyingPropertyHandler.GenerateSerialize(ctxInner);


            // move next
            g.MarkLabel(startOfLoop);

            g.LoadLocalAddress(enumerator);
            g.Call(TypeInfoHelper.GetInstanceMethod(() => sample.MoveNext()));
            g.IfTrueGoto(getCurrent);


        }

        private static void Read(ref Utf8JsonReader reader)
        {
            List<int> results = new List<int>();

            var token = reader.TokenType;
            if (token == JsonTokenType.Null)
            {

            }
            else if (token != JsonTokenType.StartArray)
            {
                throw new Exception();
            }

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                    break;

                results.Add(reader.GetInt32());
            }

        }

        private static void Write(Utf8JsonWriter writer, JsonEncodedText property, List<int> er)
        {
            var x = er;
            writer.WriteStartArray(property);

            foreach (var VARIABLE in x)
            {
                writer.WriteNumberValue(VARIABLE);

            }
            writer.WriteEndArray();
        }

        public void GenerateDeserialize(DeserializePropertyContext ctx)
        {
            var g = ctx.Generator;

            g.LoadThis();

            var end = g.DefineLabel();
            var parseValue = g.DefineLabel();

            var token = g.DeclareLocal(typeof(int));

            ctx.ReaderLoader();
            g.Call(MethodInfos.Utf8JsonReaderTokenType.Value);
            g.StoreLocal(token);
            g.LoadLocal(token);
            g.LoadInteger((int)JsonTokenType.Null);
            g.IfNotEqualGoto(parseValue);
            {
                // handle null
                g.LoadNull();
                g.Goto(end);
            }
            g.MarkLabel(parseValue);
            {
                var localList = g.DeclareLocal(typeof(List<TElement>));
                var ctor = typeof(List<TElement>).GetConstructor(Type.EmptyTypes); // TODO
                g.New(ctor);
                g.StoreLocal(localList);

                var end2 = g.DefineLabel();

                g.LoadLocal(token);
                g.LoadInteger((int)JsonTokenType.StartArray);

                var startOfLoop = g.DefineLabel();
                g.IfEqualGoto(startOfLoop);

                g.LoadString("StartArray was expected");
                g.Call(MethodInfos.HelperThrowString);

                var deserializeValue = g.DefineAndMarkLebel();

                ctx.ReaderLoader();
                g.Call(MethodInfos.Utf8JsonReaderTokenType.Value);
                g.LoadInteger((int)JsonTokenType.EndArray);

                g.IfEqualGoto(end2);

                // call inner

                var val = g.DeclareLocal(typeof(TElement));

                var inner = new DeserializePropertyContext()
                {
                    Property = null,
                    Generator = g,
                    ReaderLoader = ctx.ReaderLoader,
                    PropertySetter = (il, c, a) =>
                    {
                        g.StoreLocal(val);
                        g.LoadLocal(localList);
                        g.LoadLocal(val);
                        g.Call(TypeInfoHelper.GetInstanceMethod(() => new List<TElement>().Add(default(TElement))));
                        g.Pop(); // TODO: very bad, interfaces needs to be changed
                    }
                };

                _underlyingPropertyHandler.GenerateDeserialize(inner);

                g.MarkLabel(startOfLoop);
                ctx.ReaderLoader();
                ctx.Generator.Call(MethodInfos.Utf8JsonReaderRead); 
                g.IfTrueGoto(deserializeValue);

                g.MarkLabel(end2);
                g.LoadLocal(localList);
            }
            g.MarkLabel(end);
            ctx.PropertySetter(g, ctx.Property, true);
        }

        public void GenerateSetter(PropertySetterContext ctx)
        {
            var g = ctx.Generator;
            g.LoadThis();
            ctx.LoadValueArgument(g);
            ctx.PropertySetter(g, ctx.Property, false);
        }
    }
}
