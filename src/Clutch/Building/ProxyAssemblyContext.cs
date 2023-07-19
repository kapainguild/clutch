using System.Collections.Generic;
using System.Reflection.Emit;
using Clutch.Configuration;

namespace Clutch.Building
{
    public class ProxyAssemblyContext
    {
        public PropertyTypeHandlerFactory PropertyTypeHandlerFactory => TypeGraph.PropertyTypeHandlerFactory;

        public MemoryManager MemoryManager { get; set; }

        public StaticFieldBuilder StaticFields { get; set; }

        public List<EntityTypeData> EntityDatas { get; set; } 

        public List<ProxyBuilderContext> ProxyContexts { get; } = new List<ProxyBuilderContext>();

        public TypeGraphBuildContext TypeGraph { get; set; }

        public ModuleBuilder ModuleBuilder { get; set; }

        public StaticFieldDeclaration DiscriminatorStaticField { get; set; }

        public StaticMethodDeclaration Serializer { get; set; }

        public IProxyFactory ProxyFactory { get; set; }
    }
}
