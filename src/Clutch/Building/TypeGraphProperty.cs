using System;
using System.Reflection;
using Clutch.Configuration;
using Clutch.Configuration.Issues;
using Clutch.Helpers;

namespace Clutch.Building
{
    public class TypeGraphProperty
    {
        public TypeGraphProperty(PropertyInfo propertyInfo, PropertyBuilder builder)
        {
            PropertyInfo = propertyInfo;
            Builder = builder;
        }

        public string Name => PropertyInfo.Name;

        public Type ReturnType => PropertyInfo.PropertyType;

        public string NewBackingField { get; set; }

        public FieldInfo BackingField { get; set; }

        public PropertyInfo PropertyInfo { get; }

        public PropertyBuilder Builder { get; set; }

        public TypeGraphExtensions Extensions { get; } = new TypeGraphExtensions();

        public PropertyAccessMode PropertyAccessMode { get; set; }

        public IConfigOption DefaultValue { get; set; }

        public ConfigOptionBag Options => Builder.ToInternal().Options;

        public IPropertyHandler PropertyHandler { get; set; }

        public PropertyTypeHandler PropertyTypeHandler { get; set; }

        public void Issue<TArgs>(IssueDeclaration<IssueSourceProperty, TArgs> issueDeclaration, TArgs args, CallerInfo callerInfo = null) 
                                       => Builder.ToInternal().IssueSourceContext.Issue(issueDeclaration, args, callerInfo);

        public override string ToString() => $"{Name}:{ReturnType.Name}";
    }
}
