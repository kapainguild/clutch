using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Clutch.Building.ProxySupport;

namespace Clutch.Building
{
    class PropertyGetterAccessor
    {
        private readonly ILGenerator _generator;
        private readonly Dictionary<Type, LocalBuilder> _locals = new Dictionary<Type, LocalBuilder>();
        private ProxyBuilderPropertyContext _lastProperty;

        public PropertyGetterAccessor(ILGenerator generator)
        {
            _generator = generator;
        }

        public void LoadPropertyValue(ProxyBuilderPropertyContext ctx, ILGenerator g, bool address)
        {
            if (_generator != g)
                throw new ClutchInternalErrorException($"Method context is changed while generating getter for {ctx}");
            if (ctx.PropertyNode.PropertyAccessMode != PropertyAccessMode.Property)
            {
                g.LoadThis();
                if (address)
                    g.LoadFieldAddress(ctx.BackingField);
                else
                    g.LoadField(ctx.BackingField);
            }
            else
            {
                if (_locals.TryGetValue(ctx.ReturnType, out var local))
                {
                    if (_lastProperty != ctx)
                        LoadAndStoreProperty(g, ctx, local);
                    // else property is already stored
                }
                else
                {
                    local = g.DeclareLocal(ctx.ReturnType);
                    _locals[ctx.ReturnType] = local;
                    LoadAndStoreProperty(g, ctx, local);
                }

                _lastProperty = ctx;
                if (address) g.LoadLocalAddress(local);
                else g.LoadLocal(local);
            }
        }

        public static void LoadPropertyValue(ProxyBuilderPropertyContext property, ILGenerator g)
        {
            g.LoadThis();
            if (property.PropertyNode.PropertyAccessMode == PropertyAccessMode.Property)
            {
                g.Call(property.PropertyNode.PropertyInfo.GetMethod);
            }
            else
            {
                g.LoadField(property.BackingField);
            }
        }

        private static void LoadAndStoreProperty(ILGenerator g, ProxyBuilderPropertyContext property, LocalBuilder local)
        {
            LoadPropertyValue(property, g);
            g.StoreLocal(local);
        }
    }
}
