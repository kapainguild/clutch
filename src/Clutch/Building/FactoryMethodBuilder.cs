using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Clutch.Building.ProxySupport;

namespace Clutch.Building
{
    public interface IProxyFactory
    {
        T Create<T>();
    }

    static class FactoryMethodBuilder
    {
        public static void Build(ProxyAssemblyContext ctx)
        {
            // declare "class StaticTypedCache<T> { public static int _index; }"
            TypeBuilder staticTypedCache = ctx.ModuleBuilder.DefineType("StaticTypedCache", TypeAttributes.Public);
            staticTypedCache.DefineGenericParameters("T");

            // Add a static index field
            FieldBuilder typeIndexField = staticTypedCache.DefineField("_index", typeof(int), FieldAttributes.Public | FieldAttributes.Static);

            // declare "class ProxyFactory : IProxyFactory { public T Create<T>() {...} }"
            TypeBuilder proxyFactory = ctx.ModuleBuilder.DefineType("ProxyFactory", TypeAttributes.Public);
            proxyFactory.AddInterfaceImplementation(typeof(IProxyFactory));

            GenerateFactoryMethod(ctx, proxyFactory, staticTypedCache, typeIndexField);
            GenerateConstructor(ctx, proxyFactory, staticTypedCache, typeIndexField);

            // compile
            staticTypedCache.CreateTypeInfo().AsType();
            Type factoryType = proxyFactory.CreateTypeInfo().AsType();

            // instantiate the factory. No exceptions expected
            ctx.ProxyFactory = (IProxyFactory)Activator.CreateInstance(factoryType);
        }

        private static void GenerateFactoryMethod(ProxyAssemblyContext ctx, TypeBuilder proxyFactory, TypeBuilder staticTypedCache, FieldBuilder typeIndexField)
        {
            var factoryMethod = proxyFactory.DefineMethod(nameof(IProxyFactory.Create), MethodAttributes.Public | MethodAttributes.ReuseSlot |
                                                                                        MethodAttributes.Virtual | MethodAttributes.HideBySig);
            var param = factoryMethod.DefineGenericParameters("T");
            factoryMethod.SetReturnType(param[0]);

            var g = factoryMethod.GetILGenerator();

            var hash = staticTypedCache.MakeGenericType(param[0]);
            var field = TypeBuilder.GetField(hash, typeIndexField);
            g.LoadStaticField(field);

            var labels = Enumerable.Range(0, ctx.ProxyContexts.Count + 1).Select(i => g.DefineLabel()).ToArray();
            g.Switch(labels);
            g.MarkLabel(labels[0]);
            g.Emit(OpCodes.Ldtoken, param[0]);
            g.Call(MethodInfos.TypeGetTypeFromHandle.Value);
            g.Call(MethodInfos.HelperThrowUnknownType);

            for (int q = 0; q < ctx.ProxyContexts.Count; q++)
            {
                var proxy = ctx.ProxyContexts[q];
                g.MarkLabel(labels[q + 1]);
                g.New(proxy.DefaultConstructor);
                g.Return();
            }
        }

        private static void GenerateConstructor(ProxyAssemblyContext ctx, TypeBuilder proxyFactory, TypeBuilder staticTypedCache, FieldBuilder typeIndexField)
        {
            var ctor = proxyFactory.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, Type.EmptyTypes);
            var g = ctor.GetILGenerator();
            for (int q = 0; q < ctx.ProxyContexts.Count; q++)
            {
                var hashToSet = staticTypedCache.MakeGenericType(ctx.ProxyContexts[q].Type);
                var fieldToSet = TypeBuilder.GetField(hashToSet, typeIndexField);
                g.LoadInteger(q + 1);
                g.StoreStaticField(fieldToSet);
            }
            g.Return();
        }
    }
}
