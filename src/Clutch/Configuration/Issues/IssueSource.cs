using System;

namespace Clutch.Configuration.Issues
{
    public class IssueSource
    {
        public static IssueSource Context { get; } = new IssueSource();

        public override string ToString() => nameof(ClutchContextBuilder);
    }

    public class IssueSourceType : IssueSource
    {
        public Type Type { get; }

        public IssueSourceType(Type type)
        {
            Type = type;
        }

        public override string ToString() => Type.Name;
    }

    public class IssueSourceProperty : IssueSourceType
    {
        public string PropertyName { get; }

        public IssueSourceProperty(Type type, string propertyName)
            : base(type)
        {
            PropertyName = propertyName;
        }

        public override string ToString() => $"{Type.Name}.{PropertyName}";
    }
}
