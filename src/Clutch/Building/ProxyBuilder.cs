using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.Json;
using Clutch.Building.Collections;
using Clutch.Building.ProxySupport;
using Clutch.Configuration;
using Clutch.Helpers;

namespace Clutch.Building
{
    internal interface IUtf8JsonSerializable
    {
        void Serialize(Utf8JsonWriter writer);
    }

    internal class ProxyBuilder
    {
        public static ProxyBuilderContext Build(TypeGraphNode node, ProxyAssemblyContext assemblyContext)
        {
            var typeBuilder = assemblyContext.ModuleBuilder.DefineType(node.Type.Name, TypeAttributes.NotPublic, node.BaseType); 
            var metadata = new EntityTypeData(node.Type);
            var ctx = new ProxyBuilderContext(node, assemblyContext, typeBuilder, metadata);

            PrepareProperties(ctx);

            ctx.Properties.ForEach(s => DeclareBackingFields(s, ctx));

            ctx.Properties.ForEach(s => DeclareDynamicMethods(s, ctx));

            CollectStaticFields(ctx);

            // declaration
            DeclareInterfaces(ctx);
            DeclareDefaultConstructor(ctx);
            DeclareFactoryMethod(ctx);

            assemblyContext.TypeGraph.ProxyInfluencers.GetTypeInfluencers().ForEach(s => s.Generate(ctx));

            ctx.Properties.ForEach(s => DeclareProperty(s, ctx));
            DeclareSerializer(ctx);
            DeclareDeserializer(ctx);

            // compilation
            var typeInfo = ctx.TypeBuilder.CreateTypeInfo();

            // post compilation initialization
            metadata.Creator = (Func<object>)typeInfo.GetDeclaredMethod(ctx.FactoryMethod.Name).CreateDelegate(typeof(Func<object>));

            metadata.ProxyType = typeInfo.AsType();
            metadata.Properties = ctx.Properties.Select(s => s.Metadata).ToArray();

            return ctx;
        }

        private static void DeclareBackingFields(ProxyBuilderPropertyContext property, ProxyBuilderContext ctx)
        {
            var propertyNode = property.PropertyNode;
            if (!String.IsNullOrEmpty(propertyNode.NewBackingField))
            {
                var backField = ctx.TypeBuilder.DefineField(propertyNode.NewBackingField, propertyNode.ReturnType, FieldAttributes.Private);
                propertyNode.BackingField = backField;
            }
        }


        private static void DeclareDeserializer(ProxyBuilderContext ctx)
        {
            var method = ctx.TypeBuilder.DefineMethod("Deserializer",
                                                      MethodAttributes.Public |
                                                      MethodAttributes.HideBySig |
                                                      MethodAttributes.Final,
                                                      null,
                                                      new[] { MethodInfos.Utf8JsonReaderRef });
            int readerArgument = 1;

            ctx.Deserializer = method;
            var g = method.GetILGenerator();

            var localPropertyName = g.DeclareLocal(typeof(ReadOnlySpan<byte>));

            var end = g.DefineLabel();
            var startOfLoopCondition = g.DefineLabel();
            var parseProperty = g.DefineLabel();

            g.Goto(startOfLoopCondition);
            var startOfLoopBody = g.DefineAndMarkLebel();
            {
                //if (reader.TokenType == JsonTokenType.EndObject) break;
                g.LoadArgument(readerArgument);
                g.Call(MethodInfos.Utf8JsonReaderTokenType.Value);
                g.LoadInteger((int)JsonTokenType.EndObject);
                g.IfEqualGoto(end);
                
                //if (reader.TokenType != JsonTokenType.PropertyName) throw new AccessViolationException();
                g.LoadArgument(readerArgument);
                g.Call(MethodInfos.Utf8JsonReaderTokenType.Value);
                g.LoadInteger((int)JsonTokenType.PropertyName);
                g.IfEqualGoto(parseProperty);
                {
                    g.LoadString("PropertyName is expected");
                    g.Call(MethodInfos.HelperThrowString);
                }
                g.MarkLabel(parseProperty);

                //ReadOnlySpan<byte> propertyName = reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan;
                g.LoadArgument(readerArgument);
                g.Call(MethodInfos.HelperGetStringAsReadOnlySpan);
                g.StoreLocal(localPropertyName);

                CallPropertyDeserializer(g, ctx, readerArgument, localPropertyName);

            }
            g.MarkLabel(startOfLoopCondition);
            g.LoadArgument(readerArgument);
            g.Call(MethodInfos.Utf8JsonReaderRead);
            g.IfTrueGoto(startOfLoopBody);

            g.MarkLabel(end);
            g.Return();
        }

        private static void CallPropertyDeserializer(ILGenerator gen, ProxyBuilderContext ctx, int readerArgument, LocalBuilder localPropertyName)
        {
            var props = ctx.Properties.Select(s => new { ProxyCtx = s, Code = ProxyHelperFunctions.GetFastHashCode(JsonEncodedText.Encode(s.EscapedPropertyNameMemoryHandle.Value).EncodedUtf8Bytes) }).
                GroupBy(s => s.Code).
                OrderBy(s => s.Key).
                Select(s => new { s.Key, Proxies = s.Select(r => r.ProxyCtx).ToList() }).
                ToArray();

            
            gen.LoadLocalAddress(localPropertyName);
            gen.Call(MethodInfos.HelperGetFastHashCodeRef);

            var codeLocal = gen.DeclareLocal(typeof(ulong));
            gen.StoreLocal(codeLocal);

            TreeSwitchBuilder.GenerateTree(gen, props,
                                           (g, entry, label) => TreeSwitchBuilder.ULongGoIfGreater(g, entry.Key, label, () => g.LoadLocal(codeLocal)),
                                           (g, entry, label) => TreeSwitchBuilder.ULongGoIfNotEqual(g, entry.Key, label, () => g.LoadLocal(codeLocal)),
                                           (g, entry, returnTrue, returnFalse) =>
                                           {
                                               foreach (var proxy in entry.Proxies)
                                               {
                                                   var next = g.DefineLabel();

                                                   // compare prop names
                                                   g.LoadStaticFieldAddress(proxy.EscapedPropertyNameField.FieldBuilder);
                                                   g.Call(MethodInfos.JsonEncodedTextToReadOnlySpan);
                                                   g.LoadLocal(localPropertyName);
                                                   g.Call(MethodInfos.MemoryExtensionsSequenceEqual);
                                                   g.IfFalseGoto(next);

                                                   // read value
                                                   g.LoadArgument(readerArgument);
                                                   g.Call(MethodInfos.Utf8JsonReaderRead);
                                                   var parseValue = g.DefineLabel();
                                                   g.IfTrueGoto(parseValue);
                                                   {
                                                       g.LoadString("Unable to read next token (unexpected end of stream)");
                                                       g.Call(MethodInfos.HelperThrowString);
                                                   }

                                                   //parse value
                                                   g.MarkLabel(parseValue);

                                                   var deserializeContext = new DeserializePropertyContext
                                                   {
                                                       Generator = g,
                                                       Property = proxy,
                                                       ReaderLoader = () => g.LoadArgument(readerArgument),
                                                       PropertySetter = GenerateSetProperty
                                                   };
                                                   proxy.PropertyNode.PropertyHandler.GenerateDeserialize(deserializeContext);

                                                   g.Goto(returnTrue);

                                                   g.MarkLabel(next);
                                               }

                                               g.Goto(returnFalse);
                                           },
                                           g => { },
                                           g =>
                                           {
                                               g.LoadLocal(localPropertyName);
                                               g.Call(MethodInfos.HelperThrowPropertyNotFound);
                                           });
        }

        private static void DeclareSerializer(ProxyBuilderContext ctx)
        {
            ctx.TypeDiscriminatorMemoryHandle = ctx.AssemblyContext.MemoryManager.Allocate(ctx.Node.Discriminator);
            ctx.TypeDiscriminator = ctx.AssemblyContext.StaticFields.Add("DiscriminatorFor" + ctx.Node.Discriminator, () => ctx.TypeDiscriminatorMemoryHandle.EncodedText);

            ctx.TypeBuilder.AddInterfaceImplementation(typeof(IUtf8JsonSerializable));
            MethodBuilder ser = ctx.TypeBuilder.DefineMethod(nameof(IUtf8JsonSerializable.Serialize),
                                                    MethodAttributes.Public |
                                                    MethodAttributes.HideBySig |
                                                    MethodAttributes.Virtual |
                                                    MethodAttributes.NewSlot |
                                                    MethodAttributes.Final,
                                                    null,
                                                    new[] { typeof(Utf8JsonWriter) });

            var g = ser.GetILGenerator();
            var propertyAccessor = new PropertyGetterAccessor(g);

            //inject type info
            //TODO: discriminator is not always required, e.g for terminal entities
            g.LoadArgument(1);
            g.LoadStaticField(ctx.AssemblyContext.DiscriminatorStaticField.FieldBuilder);
            g.LoadStaticField(ctx.TypeDiscriminator.FieldBuilder);
            g.Call(MethodInfos.WriteJsonEncodedText);

            // serialize properties
            foreach (var property in ctx.Properties)
            {
                var context = new SerializePropertyContext
                              {
                                  WithPropertyName = true,
                                  Generator = g,
                                  Property = property,
                                  GenerateGetPropertyValue = address => propertyAccessor.LoadPropertyValue(property, g, address)
                              };

                property.PropertyNode.PropertyHandler.GenerateSerialize(context);
            }

            g.Return();
        }

        private static void PrepareProperties(ProxyBuilderContext ctx)
        {
            var flat = ctx.Node.Properties.Values.Select((s, index) => new ProxyBuilderPropertyContext
                                                              {
                                                                  ProxyTypeBuilder = ctx,
                                                                  PropertyNode = s,
                                                                  Metadata = new PropertyData(index)
                                                              }).ToList();
            ctx.Properties = flat.ToList();
        }

        private static void DeclareProperty(ProxyBuilderPropertyContext property, ProxyBuilderContext ctx)
        {
            var propertyNode = property.PropertyNode;
            var returnType = property.ReturnType;
            var typeBuilder = ctx.TypeBuilder;

            DeclareDynamicMethods(property, ctx);

            var prop = typeBuilder.DefineProperty(property.Name, PropertyAttributes.None, returnType, null);

            // Define the "get" accessor method
            if (propertyNode.PropertyInfo.GetMethod != null)
            {
                MethodBuilder getter = typeBuilder.DefineMethod(propertyNode.PropertyInfo.GetMethod.Name,
                                                                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig |
                                                                MethodAttributes.Virtual | MethodAttributes.NewSlot | MethodAttributes.Final,
                                                                returnType,
                                                                Type.EmptyTypes);
                ILGenerator g = getter.GetILGenerator();
                PropertyGetterAccessor.LoadPropertyValue(property, g);
                g.Return();

                typeBuilder.DefineMethodOverride(getter, propertyNode.PropertyInfo.GetMethod);
                prop.SetGetMethod(getter);
            }

            if (propertyNode.PropertyInfo.SetMethod != null)
            {
                PropertySetterMode setterMode = propertyNode.Options.GetValueOrDefault(ConfigOptionDeclarations.UsePropertySetterMode);

                MethodBuilder setter = typeBuilder.DefineMethod(propertyNode.PropertyInfo.SetMethod.Name,
                                                                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig |
                                                                MethodAttributes.Virtual | MethodAttributes.NewSlot | MethodAttributes.Final,
                                                                null,
                                                                new[] { returnType });

                ILGenerator g = setter.GetILGenerator();
                var propertyAccessor = new PropertyGetterAccessor(g);
                var setterContext = new PropertySetterContext(g, property, setterMode, ctx)
                                    {
                                        PropertyGetterAccessor = propertyAccessor,
                                        LoadValueArgument = generator => generator.LoadArgument(1),
                                        PropertySetter = GenerateSetProperty
            };

                property.PropertyNode.PropertyHandler.GenerateSetter(setterContext);
                
                g.Return();

                typeBuilder.DefineMethodOverride(setter, propertyNode.PropertyInfo.SetMethod);
                prop.SetSetMethod(setter);
            }
        }

        private static void GenerateSetProperty(ILGenerator g, ProxyBuilderPropertyContext property, bool initialize)
        {
            if (property.PropertyNode.PropertyAccessMode == PropertyAccessMode.Field ||
                property.PropertyNode.PropertyAccessMode == PropertyAccessMode.FieldForGetterAndInitializationPropertyForSetter && initialize)
            {
                g.StoreField(property.BackingField);
            }
            else
            {
                g.Call(property.PropertyNode.PropertyInfo.SetMethod);
            }
        }

        private static void DeclareDynamicMethods(ProxyBuilderPropertyContext property, ProxyBuilderContext ctx)
        {
            if (property.BackingField is FieldBuilder)
                return;

            DynamicMethod method = new DynamicMethod("Get" + ctx.Type.Name + property, property.ReturnType, new []{ ctx.Type }, ctx.Type.Module, true);
            var gil = method.GetILGenerator();
            PropertyGetterAccessor.LoadPropertyValue(property, gil);
            gil.Return();

            var getterType = typeof(Func<,>).MakeGenericType(ctx.Type, property.ReturnType);

            property.DynamicGetterDelegate = method.CreateDelegate(getterType);
            property.DynamicGetterInvoke = getterType.GetMethod("Invoke");

            DynamicMethod smethod = new DynamicMethod("Set" + ctx.Type.Name + property, null, new[] { ctx.Type, property.ReturnType }, ctx.TypeBuilder.Module, true);
            var sil = smethod.GetILGenerator();

            sil.Return();

            var setterType = typeof(Action<,>).MakeGenericType(ctx.Type, property.ReturnType);
            property.DynamicSetterDelegate = smethod.CreateDelegate(setterType);
            property.DynamicSetterInvoke = setterType.GetMethod("Invoke");
        }

        private static void CollectStaticFields(ProxyBuilderContext ctx)
        {
            //ctx..AssemblyContext.Add(ProxyBuilderConstants.EntityTypeDataFieldName, () => ctx.Metadata);

            ctx.Properties.ForEach(p =>
                                   {
                                       //p.DynamicGetterStaticField = ctx.StaticFields.Add(ctx.TypeBuilder.Name, p.DynamicGetterDelegate);

                                       //p.DynamicSetterStaticField = ctx.StaticFields.Add(ctx.TypeBuilder.Name, p.DynamicSetterDelegate);

                                       var name = p.Name;

                                       p.EscapedPropertyNameMemoryHandle = ctx.AssemblyContext.MemoryManager.Allocate(name);
                                       p.EscapedPropertyNameField = ctx.AssemblyContext.StaticFields.Add("Utf8Escaped" + name, () => p.EscapedPropertyNameMemoryHandle.EncodedText);
                                   });
        }

        private static void DeclareInterfaces(ProxyBuilderContext ctx)
        {
            if (ctx.Type.IsInterface)
            {
                ctx.TypeBuilder.AddInterfaceImplementation(ctx.Type);
            }
        }

        private static void DeclareDefaultConstructor(ProxyBuilderContext ctx)
        {
            var ctor = ctx.TypeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, Type.EmptyTypes);
            var g = ctor.GetILGenerator();

            //call base constructor
            g.LoadThis();
            g.Emit(OpCodes.Call, ctx.Node.BaseTypeConstructor);

            //set defaults
            foreach (var property in ctx.Properties)
            {
                property.PropertyNode.PropertyHandler.GenerateInit(new LoadDefaultPropertyContext
                {
                    Generator = g, Property = property,
                    PropertySetter = GenerateSetProperty
                });
                //if (!property.PropertyNode.IsCollection)
                {
                }
                /*else
                {
                    var option =
                        property.PropertyNode.Options.GetValueOrDefault(ConfigOptionDeclarations
                            .HasCollectionDefaultValue);

                    if (option == CollectionDefaultValue.Empty)
                    {
                        g.LoadThis();

                        CollectionsHelper.IsCollectionType(property.ReturnType, out Type elementType);

                        //var type = typeof(ProxyList<>)

                        GenerateSetProperty(g, property, true);
                    }
                }*/
            }
            g.Return();

            ctx.DefaultConstructor = ctor;
        }

        private static void DeclareFactoryMethod(ProxyBuilderContext ctx)
        {
            var factoryMethod = ctx.TypeBuilder.DefineMethod(ProxyBuilderConstants.FactoryMethodName,
                                                             ProxyBuilderConstants.DefaultStaticMethodAttributes,
                                                             typeof(object),
                                                             Type.EmptyTypes);

            var g = factoryMethod.GetILGenerator();
            g.New(ctx.DefaultConstructor);
            g.Return();

            ctx.FactoryMethod = factoryMethod;
        }
    }
}
