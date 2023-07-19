using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Clutch.Configuration;

namespace Clutch.Building
{
    public class ProxyBuilderContext
    {
        public TypeGraphNode Node { get; }

        public ProxyAssemblyContext AssemblyContext { get; }

        public TypeBuilder TypeBuilder { get; }

        public EntityTypeData Metadata { get; }

        public ProxyBuilderContext(TypeGraphNode node, ProxyAssemblyContext assemblyContext, TypeBuilder typeBuilder, EntityTypeData metadata)
        {
            Node = node;
            AssemblyContext = assemblyContext;
            TypeBuilder = typeBuilder;
            Metadata = metadata;
        }

        public ConstructorBuilder DefaultConstructor { get; set; }

        public MethodBuilder FactoryMethod { get; set; }

        public List<ProxyBuilderPropertyContext> Properties { get; set; }

        public StaticFieldDeclaration TypeDiscriminator { get; set; }

        public MemoryHandle TypeDiscriminatorMemoryHandle { get; set; }

        public MethodBuilder Deserializer { get; set; }

        public Type Type => Node.Type;

        public override string ToString() => Node.ToString();
    }
}
