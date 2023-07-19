using Clutch.Building;
using System;
using System.Linq.Expressions;

namespace Clutch
{
    public class OwnedTypeBuilder : BaseTypeBuilder
    {
        public const string Character = "OwnedType";

        public OwnedTypeBuilder(Type type, ClutchContextBuilder contextBuilder)
            : base(type,
                   typeof(OwnedTypeSimplePropertyBuilder<>),
                   typeof(OwnedTypeCollectionPropertyBuilder<,>),
                   contextBuilder)
        {
            InitializeAnyBuilders(() => new AnyOwnedTypePropertyBuilder(this));
        }

        public override string BuilderName => Character;

        public IOwnedTypeBasePropertyBuilder Property(string propertyName) => (IOwnedTypeBasePropertyBuilder)PropertyCore(propertyName);

        public IAnyOwnedTypePropertyBuilder AnyProperty(bool includeBaseTypeProperties = false) => (IAnyOwnedTypePropertyBuilder)AnyPropertyCore(includeBaseTypeProperties);
    }

    public class OwnedTypeBuilder<T> : OwnedTypeBuilder
    {
        public OwnedTypeBuilder(ClutchContextBuilder contextBuilder) : base(typeof(T), contextBuilder)
        {
        }

        public IOwnedTypeSimplePropertyBuilder<TReturn> Property<TReturn>(Expression<Func<T, TReturn>> propertyGetter) =>
            (IOwnedTypeSimplePropertyBuilder<TReturn>)GetProperty(propertyGetter, (name) => new OwnedTypeSimplePropertyBuilder<TReturn>(name, this, true));
    }

    public class AnyOwnedTypeBuilder : AnyBaseTypeBuilder
    {
        public AnyOwnedTypeBuilder(ClutchContextBuilder parentBuilder)
            : base(parentBuilder, typeof(AnyOwnedTypeBuilder))
        {
            InitializeAnyBuilders(() => new AnyOwnedTypePropertyBuilder(this));
        }

        public IAnyOwnedTypePropertyBuilder AnyProperty() => (IAnyOwnedTypePropertyBuilder)AnyPropertyCore();
    }
}
