using System;
using Clutch.Configuration.Issues;
using Clutch.Extensibility;

namespace Clutch.Configuration
{
    public interface IInternalBuilder
    {
        ConfigOptionBag Options { get; }

        T GetOrCreateExtension<T>(Func<T> creator) where T : IExtension;
    }

    public interface IInternalBuilder<TSource> : IInternalBuilder
    {
        IssueSourceContext<TSource> IssueSourceContext { get; }
    }
}
