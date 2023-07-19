using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.Json;
using Clutch.Building.ProxySupport;

namespace Clutch.Building
{
    class ProxyAssemblyBuilder
    {
        internal delegate object SerializerDelegate(ref Utf8JsonReader reader);

        public static ProxyAssemblyContext Build(TypeGraphBuildContext typeGraph)
        {
            AssemblyName assemblyName = new AssemblyName(ProxyBuilderConstants.AssemblyName);
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name);

            ProxyAssemblyContext ctx = new ProxyAssemblyContext
                               {
                                   TypeGraph = typeGraph,
                                   ModuleBuilder = moduleBuilder,
                                   
                                   MemoryManager = new MemoryManager()
                               };

            ctx.StaticFields = new StaticFieldBuilder(moduleBuilder, ctx.MemoryManager);
            ctx.DiscriminatorStaticField = ctx.StaticFields.AllocateString(ProxyBuilderConstants.DiscriminatorPropertyName);

            ImplementVisibility(assemblyBuilder, typeGraph);

            DeclareSerializer(ctx);

            foreach (var node in typeGraph.TypesDictionary.Values.Where(s => s.IsDeclaredInContext))
            {
                var proxyCtx = ProxyBuilder.Build(node, ctx);
                ctx.ProxyContexts.Add(proxyCtx);
            }

            ctx.EntityDatas = ctx.ProxyContexts.Select(s => s.Metadata).ToList();

            ImpelementSerializer(ctx);

            ctx.StaticFields.Initialize();

            FactoryMethodBuilder.Build(ctx);

            return ctx;
        }

        private static void DeclareSerializer(ProxyAssemblyContext ctx)
        {
            ctx.Serializer = ctx.StaticFields.DeclareMethod("Serializer", typeof(SerializerDelegate), typeof(object), new[] { MethodInfos.Utf8JsonReaderRef }, true);
        }

        private static void ImpelementSerializer(ProxyAssemblyContext ctx)
        {
            var serializer = ctx.Serializer;

            var proxies = ctx.ProxyContexts.Select(s => new { ProxyCtx = s, Code = ProxyHelperFunctions.GetFastHashCode(JsonEncodedText.Encode(s.TypeDiscriminatorMemoryHandle.Value).EncodedUtf8Bytes) }).
                                            GroupBy(s => s.Code).
                                            OrderBy(s => s.Key).
                                            Select(s => new { s.Key, Proxies = s.Select(r => r.ProxyCtx).ToList()}).
                                            ToArray();

            var gen = serializer.MethodBuilder.GetILGenerator();
            var typeLocal = gen.DeclareLocal(typeof(ReadOnlySpan<byte>));
            var codeLocal = gen.DeclareLocal(typeof(ulong));
            var resultLocal = gen.DeclareLocal(typeof(object));

            //var type = ProxyHelperFunctions.GetStringAsReadOnlySpan(ref reader);
            gen.LoadArgument(0);
            gen.Call(MethodInfos.HelperGetStringAsReadOnlySpan);
            gen.StoreLocal(typeLocal);

            //var code = ProxyHelperFunctions.GetFastHashCode(ref type);
            gen.LoadLocalAddress(typeLocal);
            gen.Call(MethodInfos.HelperGetFastHashCodeRef);
            gen.StoreLocal(codeLocal);


            TreeSwitchBuilder.GenerateTree(gen, proxies,
                                           (g, entry, label) => TreeSwitchBuilder.ULongGoIfGreater(g, entry.Key, label, () => g.LoadLocal(codeLocal)),
                                           (g, entry, label) => TreeSwitchBuilder.ULongGoIfNotEqual(g, entry.Key, label, () => g.LoadLocal(codeLocal)),
                                           (g, entry, returnTrue, returnFalse) =>
                                           {
                                               foreach (var proxy in entry.Proxies)
                                               {
                                                   var next = g.DefineLabel();

                                                   // compare type names
                                                   g.LoadStaticFieldAddress(proxy.TypeDiscriminator.FieldBuilder);
                                                   g.Call(MethodInfos.JsonEncodedTextToReadOnlySpan);
                                                   g.LoadLocal(typeLocal);
                                                   g.Call(MethodInfos.MemoryExtensionsSequenceEqual);

                                                   g.IfFalseGoto(next);

                                                   // create object
                                                   g.New(proxy.DefaultConstructor);
                                                   g.StoreLocal(resultLocal);

                                                   // call deserialization
                                                   g.LoadLocal(resultLocal);
                                                   g.LoadArgument(0);
                                                   g.Call(proxy.Deserializer);

                                                   g.Goto(returnTrue);

                                                   g.MarkLabel(next);
                                               }

                                               g.Goto(returnFalse);
                                           },
                                           g => g.LoadLocal(resultLocal),
                                           g =>
                                           {
                                               g.LoadNull(); // result is required
                                               g.LoadLocal(typeLocal);
                                               g.Call(MethodInfos.HelperThrowTypeNotFound);
                                           });
            gen.Return();
            
        }

        private static void ImplementVisibility(AssemblyBuilder assemblyBuilder, TypeGraphBuildContext typeGraph)
        {
            var asms = typeGraph.RequiresInternalVisiblity.Union(new[] { typeof(Nullable<>).Assembly, typeof(Utf8JsonWriter).Assembly, typeof(ProxyAssemblyBuilder).Assembly });

            foreach (var assembly in asms)
            {
                var attributeBuilder = new CustomAttributeBuilder(MethodInfos.CtorOfIgnoresAccessChecksToAttribute, new object[] { assembly.GetName().Name });
                assemblyBuilder.SetCustomAttribute(attributeBuilder);
            }
        }

    }
}
