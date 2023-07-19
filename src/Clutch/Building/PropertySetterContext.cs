using System;
using System.Reflection.Emit;

namespace Clutch.Building
{
    public class PropertySetterContext
    {
        public PropertySetterContext(ILGenerator generator, ProxyBuilderPropertyContext property, PropertySetterMode setterMode, ProxyBuilderContext ctx)
        {
            Generator = generator;
            Property = property;
            SetterMode = setterMode;
            Ctx = ctx;
        }

        public PropertySetterStage Stage { get; set; }

        public ILGenerator Generator { get; }

        public ProxyBuilderPropertyContext Property { get; }

        public PropertySetterMode SetterMode { get; }

        public ProxyBuilderContext Ctx { get; }

        internal PropertyGetterAccessor PropertyGetterAccessor { get; set; }

        public Action<ILGenerator> LoadValueArgument { get; set; }

        public Action<ILGenerator, ProxyBuilderPropertyContext, bool> PropertySetter { get; set; }
    }
}
