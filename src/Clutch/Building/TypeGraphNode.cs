using System;
using System.Collections.Generic;
using System.Reflection;
using Clutch.Configuration;
using Clutch.Configuration.Issues;
using Clutch.Helpers;

namespace Clutch.Building
{
    public class TypeGraphNode
    {
        public Type Type { get; set; }

        public Type BaseType { get; set; }

        public ConstructorInfo BaseTypeConstructor { get; set; }

        public BaseTypeBuilder Builder { get; set; }

        public bool IsProcessed { get; set; }

        public bool IsDeclaredInContext { get; set; }

        public List<TypeGraphNode> BaseNodes { get; } = new List<TypeGraphNode>();

        public List<TypeGraphNode> BaseNodesWithSelf { get; } = new List<TypeGraphNode>();

        public List<TypeGraphNode> DerivedNodes { get; } = new List<TypeGraphNode>();

        public Dictionary<string, TypeGraphProperty> Properties { get; set; }

        public override string ToString() => Type.Name;

        public TypeGraphExtensions Extensions { get; } = new TypeGraphExtensions();

        public MembersInfo MembersInfo { get; set; }

        public ConfigOptionBag Options => Builder.ToInternal().Options;

        public string Discriminator { get; set; }

        public void Issue<TArgs>(IssueDeclaration<IssueSourceType, TArgs> issueDeclaration, TArgs args, CallerInfo callerInfo = null)
            => Builder.ToInternal().IssueSourceContext.Issue(issueDeclaration, args, callerInfo);
    }
}
