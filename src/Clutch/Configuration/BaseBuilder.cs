using System;
using System.ComponentModel;
using Clutch.Configuration.Issues;

namespace Clutch.Configuration
{
    public class BaseBuilder<TParent, TSource> : IInternalBuilder<TSource> where TParent : IInternalBuilder
    {
        private readonly TParent _parentBuilder;
        protected readonly IssueSourceContext<TSource> _issueSourceContext;

        public BaseBuilder(TParent parentBuilder, IssueSourceContext<TSource> issueSourceContext)
        {
            _parentBuilder = parentBuilder;
            _issueSourceContext = issueSourceContext;
        }

        IssueSourceContext<TSource> IInternalBuilder<TSource>.IssueSourceContext => _issueSourceContext;

        ConfigOptionBag IInternalBuilder.Options { get; } = new ConfigOptionBag();

        T IInternalBuilder.GetOrCreateExtension<T>(Func<T> creator) => _parentBuilder.GetOrCreateExtension(creator);

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
