using System;
using System.Collections.Generic;
using System.Text;
using Clutch.Building.ProxySupport;
using Clutch.Configuration;
using Clutch.Helpers;

namespace Clutch.Building
{
    class PropertyHandlerPrimitive<T> : IPropertyHandler
    {
        private readonly PropertyTypeHandler _typeHandler;

        public PropertyHandlerPrimitive(PropertyTypeHandler typeHandler)
        {
            _typeHandler = typeHandler;
        }

        public void GenerateInit(LoadDefaultPropertyContext ctx)
        {
            var defaultValue = ctx.Property.PropertyNode.DefaultValue;
            if (defaultValue != null)
            {
                ctx.Generator.LoadThis();
                _typeHandler.LoadDefaultValue(ctx);
                ctx.PropertySetter(ctx.Generator, ctx.Property, true);
            }
        }

        public void GenerateSerialize(SerializePropertyContext ctx)
        {
            if (!ctx.WithPropertyName) // TODO
            {
                _typeHandler.Serialize(ctx);
                return;
            }

            var defaultsHandling = ctx.Property.PropertyNode.Options.GetValueOrDefault(ConfigOptionDeclarations.UseDefaultValueHandling);
            if (defaultsHandling == DefaultValueHandling.IgnoreAndPopulate)
            {
                var g = ctx.Generator;
                var property = ctx.Property;
                var endOfComparison = g.DefineLabel();

                // we should not serialize property if it is default
                var defaultValue = ctx.Property.PropertyNode.DefaultValue;
                if (defaultValue != null)
                {
                    var compareContext = new ComparePropertyContext
                    {
                        Property = property,
                        Generator = g,
                        LoadValue1 = (gen, a) => ctx.GenerateGetPropertyValue(a),
                        LoadValue2 = gen => _typeHandler.LoadDefaultValue(new LoadDefaultPropertyContext { Generator = gen, Property = property }),
                        GotoLabelIfEqual = endOfComparison
                    };
                    _typeHandler.CompareAndGotoLabel(compareContext);
                }
                else
                {
                    var systemDefaultContext = new IsSystemDefaultPropertyContext
                    {
                        Property = property,
                        Generator = g,
                        LoadValue = (gen, a) => ctx.GenerateGetPropertyValue(a),
                        GotoLabelIfEqual = endOfComparison
                    };
                    _typeHandler.IsSystemDefaultValue(systemDefaultContext);
                }
                _typeHandler.Serialize(ctx);
                g.MarkLabel(endOfComparison);
            }
            else
                _typeHandler.Serialize(ctx);
        }

        public void GenerateDeserialize(DeserializePropertyContext ctx)
        {
            ctx.Generator.LoadThis();
            _typeHandler.Deserialize(ctx);
            ctx.PropertySetter(ctx.Generator, ctx.Property, true);
        }

        public void GenerateSetter(PropertySetterContext ctx)
        {
            CallSetterInfluencers(ctx, PropertySetterStage.BeforeComparison);

            var g = ctx.Generator;
            var endOfComparison = g.DefineLabel();
            {
                if (ctx.SetterMode == PropertySetterMode.CompareAndSet)
                {
                    var compareContext = new ComparePropertyContext
                    {
                        Property = ctx.Property,
                        Generator = g,
                        LoadValue1 = (gen, addr) => ctx.PropertyGetterAccessor.LoadPropertyValue(ctx.Property, gen, addr),
                        LoadValue2 = ctx.LoadValueArgument,
                        GotoLabelIfEqual = endOfComparison
                    };
                    _typeHandler.CompareAndGotoLabel(compareContext);
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

        //TODO: move out
        private static void CallSetterInfluencers(PropertySetterContext setterContext, PropertySetterStage stage)
        {
            setterContext.Stage = stage;
            setterContext.Ctx.AssemblyContext.TypeGraph.ProxyInfluencers.GetPropertySetterInfluencers().ForEach(s => s.Generate(setterContext));
        }

    }
}
