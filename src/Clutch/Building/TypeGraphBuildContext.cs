using System;
using System.Collections.Generic;
using System.Reflection;
using Clutch.Configuration;
using Clutch.Configuration.Issues;
using Clutch.Helpers;

namespace Clutch.Building
{
    public class TypeGraphBuildContext
    {
        public Dictionary<Type, TypeGraphNode> TypesDictionary { get; set; }

        public HashSet<Assembly> RequiresInternalVisiblity { get; } = new HashSet<Assembly>();

        public ClutchContextBuilder Builder { get; internal set; }

        public void AddInaccessibleType(Type type)
        {
            RequiresInternalVisiblity.Add(type.Assembly);
        }

        public List<TypeGraphNode> Types { get; set; }

        public ProxyInfluencers ProxyInfluencers { get; } = new ProxyInfluencers();

        public ConfigOptionBag Options => Builder.ToInternal().Options;

        public PropertyTypeHandlerFactory PropertyTypeHandlerFactory { get; set; }

        public void Issue<TArgs>(IssueDeclaration<IssueSource, TArgs> issueDeclaration, TArgs args, CallerInfo callerInfo = null)
            => Builder.ToInternal().IssueSourceContext.Issue(issueDeclaration, args, callerInfo);
    }
}
