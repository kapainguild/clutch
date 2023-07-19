using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Clutch.Building;
using Clutch.Configuration;
using Clutch.Configuration.Issues;
using Clutch.Helpers;
using Clutch.Utility;

namespace Clutch
{
    public abstract class BaseTypeBuilder : BaseBuilder<ClutchContextBuilder, IssueSourceType>
    {
        private ThreadUnsafeLazy<AnyPropertyBuilder> _anyPropertyBuilder;
        private ThreadUnsafeLazy<AnyPropertyBuilder> _anyDeclaredPropertyBuilder;

        private readonly Type _genericPropertyType;
        private readonly Type _genericCollectionPropertyType;

        public Type Type { get; }

        public abstract string BuilderName { get; }

        protected readonly Dictionary<string, PropertyBuilder> _propertyBuilders = new Dictionary<string, PropertyBuilder>();

        protected BaseTypeBuilder(Type type, Type genericPropertyType, Type genericCollectionPropertyType, ClutchContextBuilder contextBuilder) :
            base(contextBuilder, contextBuilder.ToInternal().IssueSourceContext.Create(new IssueSourceType(type)))
        {
            Type = type;
            _genericPropertyType = genericPropertyType;
            _genericCollectionPropertyType = genericCollectionPropertyType;
        }

        protected void InitializeAnyBuilders(Func<AnyPropertyBuilder> creator)
        {
            _anyPropertyBuilder = new ThreadUnsafeLazy<AnyPropertyBuilder>(creator);
            _anyDeclaredPropertyBuilder = new ThreadUnsafeLazy<AnyPropertyBuilder>(creator);
        }

        
        protected PropertyBuilder PropertyCore(string propertyName)
        {
            var propertyInfo = Type.GetProperty(propertyName);
            if (propertyInfo == null && Type.IsInterface)
            {
                propertyInfo = Type.GetInterfaces().SelectMany(s => s.GetProperties()).FirstOrDefault(s => s.Name == propertyName);
            }

            if (propertyInfo == null)
                throw _issueSourceContext.Exception(CoreIssues.PropertyNotFound, propertyName);

            return _propertyBuilders.GetOrCreate(propertyName, () => CreateGenericPropertyBuilder(propertyInfo.PropertyType, propertyName, true));
        }

        internal AnyPropertyBuilder GetAnyPropertyOrDefault(bool includeBaseTypeProperties) => includeBaseTypeProperties ? _anyPropertyBuilder.GetValueOrDefault() : _anyDeclaredPropertyBuilder.GetValueOrDefault();

        protected AnyPropertyBuilder AnyPropertyCore(bool includeBaseTypeProperties = false) => includeBaseTypeProperties ? _anyPropertyBuilder.Value : _anyDeclaredPropertyBuilder.Value;

        protected PropertyBuilder GetProperty(LambdaExpression propertyGetter, Func<string, PropertyBuilder> creator)
        {
            if (propertyGetter.Body is MemberExpression memberAccess) //TODO: exceptions
            {
                var propertyInfo = memberAccess.Member as PropertyInfo;
                return _propertyBuilders.GetOrCreate(propertyInfo.Name, () => creator(propertyInfo.Name));
            }
            return null;
        }

        internal PropertyBuilder CreateGenericPropertyBuilder(Type returnType, string propertyName, bool collectCallerInfo)
        {
            Type generic;
            if (CollectionsHelper.IsCollectionType(returnType, out var elementType))
            {
                generic = _genericCollectionPropertyType.MakeGenericType(returnType, elementType);
            }
            else
            {
                generic = _genericPropertyType.MakeGenericType(returnType);
            }
            try
            {
                return (PropertyBuilder)Activator.CreateInstance(generic, propertyName, this, collectCallerInfo);
            }
            catch (TargetInvocationException exception)
            {
                throw new ClutchInternalErrorException($"Exception while creating generic PropertyBuilder for {returnType}. See inner exception for details.", exception.InnerException);
            }
        }

        public IReadOnlyDictionary<string, PropertyBuilder> GetProperties() => _propertyBuilders;
    }

    public class AnyBaseTypeBuilder : BaseBuilder<ClutchContextBuilder, IssueSourceType>
    {
        private ThreadUnsafeLazy<AnyPropertyBuilder> _anyPropertyBuilder;

        public AnyBaseTypeBuilder(ClutchContextBuilder parentBuilder, Type sourceType)
            : base(parentBuilder, parentBuilder.ToInternal().IssueSourceContext.Create(new IssueSourceType(sourceType)))
        {
        }

        protected void InitializeAnyBuilders(Func<AnyPropertyBuilder> creator)
        {
            _anyPropertyBuilder = new ThreadUnsafeLazy<AnyPropertyBuilder>(creator);
        }

        protected AnyPropertyBuilder AnyPropertyCore() => _anyPropertyBuilder.Value;

        internal AnyPropertyBuilder GetAnyPropertyOrDefault() => _anyPropertyBuilder.GetValueOrDefault();
    }
}
