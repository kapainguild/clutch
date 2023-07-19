using Clutch.Configuration;
using Clutch.Configuration.Issues;

namespace Clutch.Building
{
    public class AnyEntityPropertyBuilder : AnyPropertyBuilder, IAnyEntityTypePropertyBuilder, IBaseApi
    {
        internal AnyEntityPropertyBuilder(BaseBuilder<ClutchContextBuilder, IssueSourceType> parentBuilder) : base(parentBuilder)
        {
        }

        IAnyEntityTypePropertyBuilder IAnyPropertyBuilderMethods<IAnyEntityTypePropertyBuilder>.UseDefaultValueHandling(DefaultValueHandling handling)
            => PropertyApi.UseDefaultValueHandling(this, handling);

        IAnyEntityTypePropertyBuilder IAnyPropertyBuilderMethods<IAnyEntityTypePropertyBuilder>.UsePropertyAccessMode(PropertyAccessMode mode)
            => PropertyApi.UsePropertyAccessMode(this, mode);

        IAnyEntityTypePropertyBuilder IAnyPropertyBuilderMethods<IAnyEntityTypePropertyBuilder>.UsePropertySetterMode(PropertySetterMode mode)
            => PropertyApi.UsePropertySetterMode(this, mode);
    }

    public class EntitySimplePropertyBuilder<TReturn> : PropertyBuilder<TReturn>, IEntitySimplePropertyBuilder<TReturn>, IEntityBasePropertyBuilder, IBaseApi
    {
        public EntitySimplePropertyBuilder(string propertyName, EntityTypeBuilder baseTypeBuilder, bool collectCallerInfo) :
            base(propertyName, baseTypeBuilder, collectCallerInfo)
        {
        }

        IEntitySimplePropertyBuilder<TReturn> ISimplePropertyBuilderMethods<IEntitySimplePropertyBuilder<TReturn>, TReturn>.HasDefaultValue(TReturn value)
            => PropertyApi.HasDefaultValue(this, value);

        IEntitySimplePropertyBuilder<TReturn> IPropertyBuilderMethods<IEntitySimplePropertyBuilder<TReturn>>.HasField(string value)
            => PropertyApi.HasField(this, value);

        IEntityBasePropertyBuilder IPropertyBuilderMethods<IEntityBasePropertyBuilder>.HasField(string value)
            => PropertyApi.HasField(this, value);

        IEntitySimplePropertyBuilder<TReturn> IAnyPropertyBuilderMethods<IEntitySimplePropertyBuilder<TReturn>>.UseDefaultValueHandling(DefaultValueHandling handling)
            => PropertyApi.UseDefaultValueHandling(this, handling);

        IEntityBasePropertyBuilder IAnyPropertyBuilderMethods<IEntityBasePropertyBuilder>.UseDefaultValueHandling(DefaultValueHandling handling)
            => PropertyApi.UseDefaultValueHandling(this, handling);

        IEntitySimplePropertyBuilder<TReturn> IAnyPropertyBuilderMethods<IEntitySimplePropertyBuilder<TReturn>>.UsePropertyAccessMode(PropertyAccessMode mode)
            => PropertyApi.UsePropertyAccessMode(this, mode);

        IEntityBasePropertyBuilder IAnyPropertyBuilderMethods<IEntityBasePropertyBuilder>.UsePropertyAccessMode(PropertyAccessMode mode)
            => PropertyApi.UsePropertyAccessMode(this, mode);

        IEntitySimplePropertyBuilder<TReturn> IAnyPropertyBuilderMethods<IEntitySimplePropertyBuilder<TReturn>>.UsePropertySetterMode(PropertySetterMode mode)
            => PropertyApi.UsePropertySetterMode(this, mode);

        IEntityBasePropertyBuilder IAnyPropertyBuilderMethods<IEntityBasePropertyBuilder>.UsePropertySetterMode(PropertySetterMode mode)
            => PropertyApi.UsePropertySetterMode(this, mode);
    }


    public class EntityCollectionPropertyBuilder<TCollection, TCollectionElement> : PropertyBuilder<TCollection>, IEntityCollectionPropertyBuilder<TCollection, TCollectionElement>, IEntityBasePropertyBuilder, IBaseApi
    {
        public EntityCollectionPropertyBuilder(string propertyName, EntityTypeBuilder baseTypeBuilder, bool collectCallerInfo) :
            base(propertyName, baseTypeBuilder, collectCallerInfo)
        {
        }

        internal override void CallGenericProcessor(ITypeGraphPropertyGenericProcessor processor) => processor.ProcessCollection<TCollection,TCollectionElement>();

        IEntityCollectionPropertyBuilder<TCollection, TCollectionElement> IPropertyBuilderMethods<IEntityCollectionPropertyBuilder<TCollection, TCollectionElement>>.HasField(string value)
            => PropertyApi.HasField(this, value);

        IEntityBasePropertyBuilder IPropertyBuilderMethods<IEntityBasePropertyBuilder>.HasField(string value)
            => PropertyApi.HasField(this, value);

        IEntityCollectionPropertyBuilder<TCollection, TCollectionElement> IAnyPropertyBuilderMethods<IEntityCollectionPropertyBuilder<TCollection, TCollectionElement>>.UseDefaultValueHandling(DefaultValueHandling handling)
            => PropertyApi.UseDefaultValueHandling(this, handling);

        IEntityBasePropertyBuilder IAnyPropertyBuilderMethods<IEntityBasePropertyBuilder>.UseDefaultValueHandling(DefaultValueHandling handling)
            => PropertyApi.UseDefaultValueHandling(this, handling);

        IEntityCollectionPropertyBuilder<TCollection, TCollectionElement> IAnyPropertyBuilderMethods<IEntityCollectionPropertyBuilder<TCollection, TCollectionElement>>.UsePropertyAccessMode(PropertyAccessMode mode)
            => PropertyApi.UsePropertyAccessMode(this, mode);

        IEntityBasePropertyBuilder IAnyPropertyBuilderMethods<IEntityBasePropertyBuilder>.UsePropertyAccessMode(PropertyAccessMode mode)
            => PropertyApi.UsePropertyAccessMode(this, mode);

        IEntityCollectionPropertyBuilder<TCollection, TCollectionElement> IAnyPropertyBuilderMethods<IEntityCollectionPropertyBuilder<TCollection, TCollectionElement>>.UsePropertySetterMode(PropertySetterMode mode)
            => PropertyApi.UsePropertySetterMode(this, mode);

        IEntityBasePropertyBuilder IAnyPropertyBuilderMethods<IEntityBasePropertyBuilder>.UsePropertySetterMode(PropertySetterMode mode)
            => PropertyApi.UsePropertySetterMode(this, mode);

        public IEntityCollectionPropertyBuilder<TCollection, TCollectionElement> HasDefaultValue(CollectionDefaultValue value)
            => PropertyApi.HasDefaultValue(this, value);
    }

    public class AnyOwnedTypePropertyBuilder : AnyPropertyBuilder
    {
        internal AnyOwnedTypePropertyBuilder(BaseBuilder<ClutchContextBuilder, IssueSourceType> parentBuilder) :
            base(parentBuilder)
        {
        }
    }


    public class OwnedTypeSimplePropertyBuilder<TReturn> : PropertyBuilder<TReturn>, IOwnedTypeSimplePropertyBuilder<TReturn>, IOwnedTypeBasePropertyBuilder, IBaseApi
    {
        public OwnedTypeSimplePropertyBuilder(string propertyName, OwnedTypeBuilder baseTypeBuilder, bool collectCallerInfo) :
            base(propertyName, baseTypeBuilder, collectCallerInfo)
        {
        }

        IOwnedTypeSimplePropertyBuilder<TReturn> ISimplePropertyBuilderMethods<IOwnedTypeSimplePropertyBuilder<TReturn>, TReturn>.HasDefaultValue(TReturn value)
            => PropertyApi.HasDefaultValue(this, value);

        IOwnedTypeSimplePropertyBuilder<TReturn> IPropertyBuilderMethods<IOwnedTypeSimplePropertyBuilder<TReturn>>.HasField(string value)
            => PropertyApi.HasField(this, value);

        IOwnedTypeBasePropertyBuilder IPropertyBuilderMethods<IOwnedTypeBasePropertyBuilder>.HasField(string value)
            => PropertyApi.HasField(this, value);

        IOwnedTypeSimplePropertyBuilder<TReturn> IAnyPropertyBuilderMethods<IOwnedTypeSimplePropertyBuilder<TReturn>>.UseDefaultValueHandling(DefaultValueHandling handling)
            => PropertyApi.UseDefaultValueHandling(this, handling);

        IOwnedTypeBasePropertyBuilder IAnyPropertyBuilderMethods<IOwnedTypeBasePropertyBuilder>.UseDefaultValueHandling(DefaultValueHandling handling)
            => PropertyApi.UseDefaultValueHandling(this, handling);

        IOwnedTypeSimplePropertyBuilder<TReturn> IAnyPropertyBuilderMethods<IOwnedTypeSimplePropertyBuilder<TReturn>>.UsePropertyAccessMode(PropertyAccessMode mode)
            => PropertyApi.UsePropertyAccessMode(this, mode);

        IOwnedTypeBasePropertyBuilder IAnyPropertyBuilderMethods<IOwnedTypeBasePropertyBuilder>.UsePropertyAccessMode(PropertyAccessMode mode)
            => PropertyApi.UsePropertyAccessMode(this, mode);

        IOwnedTypeSimplePropertyBuilder<TReturn> IAnyPropertyBuilderMethods<IOwnedTypeSimplePropertyBuilder<TReturn>>.UsePropertySetterMode(PropertySetterMode mode)
            => PropertyApi.UsePropertySetterMode(this, mode);

        IOwnedTypeBasePropertyBuilder IAnyPropertyBuilderMethods<IOwnedTypeBasePropertyBuilder>.UsePropertySetterMode(PropertySetterMode mode)
            => this.UsePropertySetterMode(mode);
    }


    public class OwnedTypeCollectionPropertyBuilder<TCollection, TCollectionElement> : PropertyBuilder<TCollection>, IOwnedTypeCollectionPropertyBuilder<TCollection, TCollectionElement>, IOwnedTypeBasePropertyBuilder, IBaseApi
    {
        public OwnedTypeCollectionPropertyBuilder(string propertyName, EntityTypeBuilder baseTypeBuilder, bool collectCallerInfo) :
            base(propertyName, baseTypeBuilder, collectCallerInfo)
        {
        }

        internal override void CallGenericProcessor(ITypeGraphPropertyGenericProcessor processor) => processor.ProcessCollection<TCollection, TCollectionElement>();

        IOwnedTypeCollectionPropertyBuilder<TCollection, TCollectionElement> IPropertyBuilderMethods<IOwnedTypeCollectionPropertyBuilder<TCollection, TCollectionElement>>.HasField(string value)
            => PropertyApi.HasField(this, value);

        IOwnedTypeBasePropertyBuilder IPropertyBuilderMethods<IOwnedTypeBasePropertyBuilder>.HasField(string value)
            => PropertyApi.HasField(this, value);

        IOwnedTypeCollectionPropertyBuilder<TCollection, TCollectionElement> IAnyPropertyBuilderMethods<IOwnedTypeCollectionPropertyBuilder<TCollection, TCollectionElement>>.UseDefaultValueHandling(DefaultValueHandling handling)
            => PropertyApi.UseDefaultValueHandling(this, handling);

        IOwnedTypeBasePropertyBuilder IAnyPropertyBuilderMethods<IOwnedTypeBasePropertyBuilder>.UseDefaultValueHandling(DefaultValueHandling handling)
            => PropertyApi.UseDefaultValueHandling(this, handling);

        IOwnedTypeCollectionPropertyBuilder<TCollection, TCollectionElement> IAnyPropertyBuilderMethods<IOwnedTypeCollectionPropertyBuilder<TCollection, TCollectionElement>>.UsePropertyAccessMode(PropertyAccessMode mode)
            => PropertyApi.UsePropertyAccessMode(this, mode);

        IOwnedTypeBasePropertyBuilder IAnyPropertyBuilderMethods<IOwnedTypeBasePropertyBuilder>.UsePropertyAccessMode(PropertyAccessMode mode)
            => PropertyApi.UsePropertyAccessMode(this, mode);

        IOwnedTypeCollectionPropertyBuilder<TCollection, TCollectionElement> IAnyPropertyBuilderMethods<IOwnedTypeCollectionPropertyBuilder<TCollection, TCollectionElement>>.UsePropertySetterMode(PropertySetterMode mode)
            => PropertyApi.UsePropertySetterMode(this, mode);

        IOwnedTypeBasePropertyBuilder IAnyPropertyBuilderMethods<IOwnedTypeBasePropertyBuilder>.UsePropertySetterMode(PropertySetterMode mode)
            => PropertyApi.UsePropertySetterMode(this, mode);

        public IOwnedTypeCollectionPropertyBuilder<TCollection, TCollectionElement> HasDefaultValue(CollectionDefaultValue value)
            => PropertyApi.HasDefaultValue(this, value);
    }
}
