using Clutch.Building;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Clutch
{
    public class EntityTypeBuilder : BaseTypeBuilder
    {
        public const string Character = "Entity";

        public EntityTypeBuilder(Type type, ClutchContextBuilder contextBuilder)
            : base(type, 
                   typeof(EntitySimplePropertyBuilder<>),
                   typeof(EntityCollectionPropertyBuilder<,>),
                   contextBuilder)
        {
            InitializeAnyBuilders(() => new AnyEntityPropertyBuilder(this));
        }

        public override string BuilderName => Character;

        public PropertyBuilder Property(string propertyName) => PropertyCore(propertyName);

        public IAnyEntityTypePropertyBuilder AnyProperty(bool includeBaseTypeProperties = false) => (AnyEntityPropertyBuilder)AnyPropertyCore(includeBaseTypeProperties);
    }

    public class EntityTypeBuilder<T> : EntityTypeBuilder
    {
        public EntityTypeBuilder(ClutchContextBuilder contextBuilder) : base(typeof(T), contextBuilder)
        {
        }

        public IEntitySimplePropertyBuilder<TReturn> Property<TReturn>(Expression<Func<T, TReturn>> propertyGetter) =>
            (IEntitySimplePropertyBuilder<TReturn>)GetProperty(propertyGetter, (name) => new EntitySimplePropertyBuilder<TReturn>(name, this, true));

        public IEntityCollectionPropertyBuilder<IList<TCollectionElement>, TCollectionElement> Property<TCollectionElement>(Expression <Func<T, IList<TCollectionElement>>> propertyGetter) =>
            (IEntityCollectionPropertyBuilder<IList<TCollectionElement>, TCollectionElement>)GetProperty(propertyGetter, (name) => new EntityCollectionPropertyBuilder<IList<TCollectionElement>, TCollectionElement>(name, this, true));

        public IEntityCollectionPropertyBuilder<ICollection<TCollectionElement>, TCollectionElement> Property<TCollectionElement>(Expression<Func<T, ICollection<TCollectionElement>>> propertyGetter) =>
            (IEntityCollectionPropertyBuilder<ICollection<TCollectionElement>, TCollectionElement>)GetProperty(propertyGetter, (name) => new EntityCollectionPropertyBuilder<ICollection<TCollectionElement>, TCollectionElement>(name, this, true));
    }

    public class AnyEntityTypeBuilder : AnyBaseTypeBuilder
    {
        public AnyEntityTypeBuilder(ClutchContextBuilder parentBuilder)
            : base(parentBuilder, typeof(AnyEntityTypeBuilder))
        {
            InitializeAnyBuilders(() => new AnyEntityPropertyBuilder(this));
        }

        public IAnyEntityTypePropertyBuilder AnyProperty() => (AnyEntityPropertyBuilder)AnyPropertyCore();
    }
}
