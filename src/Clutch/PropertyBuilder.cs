using System;
using Clutch.Building;
using Clutch.Configuration;
using Clutch.Configuration.Issues;
using Clutch.Helpers;

namespace Clutch
{
    public abstract class PropertyBuilder : BaseBuilder<BaseTypeBuilder, IssueSourceProperty>
    {
        public Type ReturnType { get; }

        public string PropertyName { get; }

        internal PropertyBuilder(Type returnType, string propertyName, BaseTypeBuilder baseTypeBuilder, bool collectCallerInfo) :
            base(baseTypeBuilder, baseTypeBuilder.ToInternal().IssueSourceContext.Create(new IssueSourceProperty(baseTypeBuilder.Type, propertyName), collectCallerInfo))
        {
            ReturnType = returnType;
            PropertyName = propertyName;
        }

        internal abstract void CallGenericProcessor(ITypeGraphPropertyGenericProcessor processor);
    }

    public abstract class PropertyBuilder<TReturn> : PropertyBuilder
    {
        public PropertyBuilder(string propertyName, BaseTypeBuilder baseTypeBuilder, bool collectCallerInfo) :
            base(typeof(TReturn), propertyName, baseTypeBuilder, collectCallerInfo)
        {
        }

        internal override void CallGenericProcessor(ITypeGraphPropertyGenericProcessor processor) => processor.Process<TReturn>();
    }

    public class AnyPropertyBuilder : BaseBuilder<BaseBuilder<ClutchContextBuilder, IssueSourceType>, IssueSourceProperty>
    {
        internal AnyPropertyBuilder(BaseBuilder<ClutchContextBuilder, IssueSourceType> parentBuilder) : 
            base(parentBuilder, 
                 parentBuilder.ToInternal().IssueSourceContext.Create(new IssueSourceProperty(parentBuilder.ToInternal().IssueSourceContext.Source.Type, "[Any property]")))
        {
        }
    }
}
