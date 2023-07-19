using System;
using System.Reflection.Emit;
using Clutch.Configuration;

namespace Clutch.Building
{
    public class GeneratorPropertyContext
    {
        public ILGenerator Generator { get; set; }

        public ProxyBuilderPropertyContext Property { get; set; }
    }

    public class SerializePropertyContext : GeneratorPropertyContext
    {
        public bool WithPropertyName { get; set; }

        public Action<bool> GenerateGetPropertyValue { get; set; }
    }

    public class DeserializePropertyContext : GeneratorPropertyContext
    {
        public Action ReaderLoader { get; set; }

        public Action<ILGenerator, ProxyBuilderPropertyContext, bool> PropertySetter { get; set; }
    }

    public class ComparePropertyContext : GeneratorPropertyContext
    {
        public Action<ILGenerator, bool> LoadValue1 { get; set; }

        public Action<ILGenerator> LoadValue2 { get; set; }

        public Label GotoLabelIfEqual { get; set; }
    }

    public class LoadDefaultPropertyContext : GeneratorPropertyContext
    {
        public T GetDefaultValue<T>() => ((ConfigOption<T>)Property.PropertyNode.DefaultValue).Get().Value;

        public Action<ILGenerator, ProxyBuilderPropertyContext, bool> PropertySetter { get; set; }
    }

    public class IsSystemDefaultPropertyContext : GeneratorPropertyContext
    {
        public Action<ILGenerator, bool> LoadValue { get; set; }

        public Label GotoLabelIfEqual { get; set; }
    }
}
