using System;
using System.Reflection;
using Clutch.Configuration;

namespace Clutch.Building
{
    public class ProxyBuilderPropertyContext
    {
        public TypeGraphProperty PropertyNode { get; set; }

        public PropertyData Metadata { get; set; }

        public MethodInfo DynamicGetterInvoke { get; set; }

        public MethodInfo DynamicSetterInvoke { get; set; }

        public Delegate DynamicGetterDelegate { get; set; }

        public Delegate DynamicSetterDelegate { get; set; }

        public StaticFieldDeclaration EscapedPropertyNameField { get; set; }

        public MemoryHandle EscapedPropertyNameMemoryHandle { get; set; }

        public ProxyBuilderContext ProxyTypeBuilder { get; set; }

        public PropertyTypeHandler TypeHandler => PropertyNode.PropertyTypeHandler;

        public ProxyAssemblyContext AssemblyContext => ProxyTypeBuilder.AssemblyContext;

        public Type ReturnType => PropertyNode.ReturnType;

        public string Name => PropertyNode.Name;

        public FieldInfo BackingField => PropertyNode.BackingField;

        public override string ToString() => PropertyNode.ToString();
    }
}
