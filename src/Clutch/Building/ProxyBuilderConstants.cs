using System.Reflection;

namespace Clutch.Building
{
    static class ProxyBuilderConstants
    {
        public const string DiscriminatorPropertyName = "_t";

        public const string StaticMethodPrefix = "__";
        public const string StaticFieldPrefix = "_";

        public const string StaticFieldGetMethodPrefix = StaticMethodPrefix + "Get";
        public const string StaticFieldSetMethodPrefix = StaticMethodPrefix + "Set";

        public const string AssemblyName = "ClutchProxies";
        public const string FactoryMethodName = StaticMethodPrefix + "CreateInstance";

        public const string EntityTypeDataFieldName = "entityTypeData";
        


        public const MethodAttributes DefaultStaticMethodAttributes = MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig;

        public const FieldAttributes DefaultStaticFieldAttributes = FieldAttributes.Public| FieldAttributes.Static;

    }
}
