using System;
using System.Collections.Generic;
using System.ComponentModel;
using Clutch.Building;
using Clutch.Configuration;
using Clutch.Configuration.Issues;
using Clutch.Extensibility;
using Clutch.Helpers;
using Clutch.Utility;

namespace Clutch
{
    public class ClutchContextBuilder : IContextApi, IInternalBuilder<IssueSource>
    {
        private readonly Dictionary<Type, BaseTypeBuilder> _typeBuilders = new Dictionary<Type, BaseTypeBuilder>();
        private readonly Dictionary<Type, IExtension> _extensions = new Dictionary<Type, IExtension>();
        private readonly IssueSourceContext<IssueSource> _issueSourceContext;
        private readonly IssueCollector _issues = new IssueCollector();
        private readonly ThreadUnsafeLazy<AnyEntityTypeBuilder> _anyEntityTypeBuilder;
        private readonly ThreadUnsafeLazy<AnyOwnedTypeBuilder> _anyOwnedTypeBuilder;

        IssueSourceContext<IssueSource> IInternalBuilder<IssueSource>.IssueSourceContext => _issueSourceContext;

        ConfigOptionBag IInternalBuilder.Options { get; } = new ConfigOptionBag();

        

        public ClutchContextBuilder()
        {
            _issueSourceContext = new IssueSourceContext<IssueSource>(IssueSource.Context, _issues, true);
            _anyEntityTypeBuilder = new ThreadUnsafeLazy<AnyEntityTypeBuilder>(() => new AnyEntityTypeBuilder(this));
            _anyOwnedTypeBuilder = new ThreadUnsafeLazy<AnyOwnedTypeBuilder>(() => new AnyOwnedTypeBuilder(this));
        }

        //EntityTypeBuilder API
        public AnyEntityTypeBuilder AnyEntityType() => _anyEntityTypeBuilder.Value;

        public EntityTypeBuilder<T> Entity<T>() => GetOrCreateTypeBuilder<T, EntityTypeBuilder<T>>(() => new EntityTypeBuilder<T>(this));

        public void Entity<T>(Action<EntityTypeBuilder<T>> typeConfigurator) => typeConfigurator(Entity<T>());

        public EntityTypeBuilder Entity(Type type) => GetOrCreateTypeBuilder<EntityTypeBuilder>(type, typeof(EntityTypeBuilder<>), null);

        internal AnyEntityTypeBuilder GetAnyEntityTypeOrDefault() => _anyEntityTypeBuilder.GetValueOrDefault();

        //OwnedTypeBuilder API
        public AnyOwnedTypeBuilder AnyOwnedType() => _anyOwnedTypeBuilder.Value;

        public OwnedTypeBuilder<T> OwnedType<T>() => GetOrCreateTypeBuilder<T, OwnedTypeBuilder<T>>(() => new OwnedTypeBuilder<T>(this));

        public void OwnedType<T>(Action<OwnedTypeBuilder<T>> typeConfigurator) => typeConfigurator(OwnedType<T>());

        public OwnedTypeBuilder OwnedType(Type type) => GetOrCreateTypeBuilder<OwnedTypeBuilder>(type, typeof(OwnedTypeBuilder<>), null);

        internal AnyOwnedTypeBuilder GetAnyOwnedTypeOrDefault() => _anyOwnedTypeBuilder.GetValueOrDefault();

        // private implementation
        private TBuilder GetOrCreateTypeBuilder<T, TBuilder>(Func<TBuilder> creator) where TBuilder : BaseTypeBuilder =>
            GetOrCreateTypeBuilder(typeof(T), typeof(TBuilder), creator);

        private TBuilder GetOrCreateTypeBuilder<TBuilder>(Type type, Type builderType, Func<TBuilder> creator) where TBuilder : BaseTypeBuilder
        {
            if (_typeBuilders.TryGetValue(type, out var result))
            {
                if (result.GetType() != typeof(TBuilder))
                    throw result.ToInternal().IssueSourceContext.Exception(CoreIssues.CannotChangeTypeCharacter, result.BuilderName);
            }
            else
            {
                creator = creator ?? (() => CreateGenericEntityBuilder<TBuilder>(type, builderType));
                result = creator();
                _typeBuilders[type] = result;
            }
            return (TBuilder)result;
        }

        private TBuilder CreateGenericEntityBuilder<TBuilder>(Type type, Type builderType)
        {
            var asGeneric = builderType.MakeGenericType(type);
            return (TBuilder)Activator.CreateInstance(asGeneric, this);
        }

        public ClutchContext Build(out IEnumerable<IIssue> issues)
        {
            // we should not continue if there are any errors
            _issues.ThrowIfAnyPendingErrors();

            // build graph
            var typeGraph = TypeGraph.BuildGraph(_typeBuilders, this);

            // we should not continue if there are any errors
            _issues.ThrowIfAnyPendingErrors();

            // build proxies
           

            var proxyContext = ProxyAssemblyBuilder.Build(typeGraph);


            // return result
            var contextData = new ContextData();
            issues = _issues.GetFiltered();
            return new ClutchContext(contextData, proxyContext);
        }

        T IInternalBuilder.GetOrCreateExtension<T>(Func<T> creator) 
        {
            return (T)_extensions.GetOrCreate(typeof(T), () => creator());
        }

        public IEnumerable<IExtension> GetExtensions()
        {
            return _extensions.Values;
        }


        #region Intellisense Friendly overrides

        /// <inheritdoc cref="Object.GetHashCode"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => base.GetHashCode();

        /// <inheritdoc cref="Object.ToString"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => base.ToString();

        /// <inheritdoc cref="Object.Equals(object)" />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object other) => base.Equals(other);
        #endregion

    }
}
