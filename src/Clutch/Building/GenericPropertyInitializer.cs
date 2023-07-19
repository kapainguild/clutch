using System;
using System.Collections.Generic;
using System.Text;

namespace Clutch.Building
{
    class GenericPropertyInitializer : ITypeGraphPropertyGenericProcessor
    {
        private readonly TypeGraphProperty _property;
        private readonly TypeGraphBuildContext _ctx;

        public GenericPropertyInitializer(TypeGraphProperty property, TypeGraphBuildContext ctx)
        {
            _property = property;
            _ctx = ctx;
        }

        public void Process<T>()
        {
            _property.PropertyHandler = CreateHandler<T>();
        }

        private IPropertyHandler CreateHandler<T>()
        {
            // check whether it is Context type (entity or Owned)
            if (_ctx.TypesDictionary.TryGetValue(typeof(T), out var node))
            {
                return new PropertyHandlerOwnedType<T>(node, _property);
            }
            else
            {
                var primitive = _ctx.PropertyTypeHandlerFactory.GetPropertyTypeHandler<T>(new PropertyTypeHandlerBuildContext {Property = _property});
                return new PropertyHandlerPrimitive<T>(primitive);
            }
        }

        public void ProcessCollection<T, TElement>()
        {
            _property.PropertyHandler = new PropertyHandlerCollection<T, TElement>(CreateHandler<TElement>());
        }
    }
}
