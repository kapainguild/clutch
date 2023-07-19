using Clutch.Configuration;

namespace Clutch
{
    public interface IBaseApi : IInternalBuilder { }

    public interface IContextApi : IBaseApi{ }


    public interface IAnyPropertyBuilderMethods<TFluent>
    {
        TFluent UsePropertyAccessMode(PropertyAccessMode mode);

        TFluent UsePropertySetterMode(PropertySetterMode mode);

        TFluent UseDefaultValueHandling(DefaultValueHandling handling);
    }

    public interface IPropertyBuilderMethods<TFluent>
    {
        TFluent HasField(string value);
    }

    public interface IEntityBasePropertyBuilder :
        IAnyPropertyBuilderMethods<IEntityBasePropertyBuilder>,
        IPropertyBuilderMethods<IEntityBasePropertyBuilder>
    {
    }

    public interface IAnyEntityTypePropertyBuilder : IBaseApi,
        IAnyPropertyBuilderMethods<IAnyEntityTypePropertyBuilder>
    {
    }

    public interface ISimplePropertyBuilderMethods<TFluent, TReturn>
    {
        TFluent HasDefaultValue(TReturn value);
    }

    public interface ICollectionPropertyBuilderMethods<TFluent, TReturn, TElement>
    {
        TFluent HasDefaultValue(CollectionDefaultValue value);
    }

    public interface IEntitySimplePropertyBuilder<TReturn> : IBaseApi,
        IAnyPropertyBuilderMethods<IEntitySimplePropertyBuilder<TReturn>>,
        IPropertyBuilderMethods<IEntitySimplePropertyBuilder<TReturn>>,
        ISimplePropertyBuilderMethods<IEntitySimplePropertyBuilder<TReturn>, TReturn>
    {
    }

    public interface IEntityCollectionPropertyBuilder<TCollection, TCollectionElement> : IBaseApi,
        IAnyPropertyBuilderMethods<IEntityCollectionPropertyBuilder<TCollection, TCollectionElement>>,
        IPropertyBuilderMethods<IEntityCollectionPropertyBuilder<TCollection, TCollectionElement>>,
        ICollectionPropertyBuilderMethods<IEntityCollectionPropertyBuilder<TCollection, TCollectionElement>, TCollection, TCollectionElement>
    {
    }

    public interface IOwnedTypeBasePropertyBuilder : IBaseApi,
        IAnyPropertyBuilderMethods<IOwnedTypeBasePropertyBuilder>,
        IPropertyBuilderMethods<IOwnedTypeBasePropertyBuilder>
    {
    }

    public interface IAnyOwnedTypePropertyBuilder : IBaseApi,
        IAnyPropertyBuilderMethods<IOwnedTypeBasePropertyBuilder>
    {
    }

    public interface IOwnedTypeSimplePropertyBuilder<TReturn> : IBaseApi,
        IAnyPropertyBuilderMethods<IOwnedTypeSimplePropertyBuilder<TReturn>>,
        IPropertyBuilderMethods<IOwnedTypeSimplePropertyBuilder<TReturn>>,
        ISimplePropertyBuilderMethods<IOwnedTypeSimplePropertyBuilder<TReturn>, TReturn>
    {
    }

    public interface IOwnedTypeCollectionPropertyBuilder<TCollection, TCollectionElement> : IBaseApi,
        IAnyPropertyBuilderMethods<IOwnedTypeCollectionPropertyBuilder<TCollection, TCollectionElement>>,
        IPropertyBuilderMethods<IOwnedTypeCollectionPropertyBuilder<TCollection, TCollectionElement>>,
        ICollectionPropertyBuilderMethods<IOwnedTypeCollectionPropertyBuilder<TCollection, TCollectionElement>, TCollection, TCollectionElement>
    {
    }

}
