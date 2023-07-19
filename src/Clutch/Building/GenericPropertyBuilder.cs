using System;
using Clutch.Configuration;

namespace Clutch.Building
{
    class GenericPropertyBuilder : ITypeGraphPropertyGenericProcessor
    {
        private readonly TypeGraphProperty _property;
        private readonly TypeGraphBuildContext _ctx;

        public GenericPropertyBuilder(TypeGraphProperty property, TypeGraphBuildContext ctx)
        {
            _property = property;
            _ctx = ctx;
        }

        public void Process<T>()
        {
            if (_property.Options.TryGet(ConfigOptionDeclarations.HasDefaultValue, out var configOption))
            {
                var typed = ((ConfigOption<T>)configOption).Get().Value;

                if (Equals(typed, default(T)))
                    _property.Issue(CoreIssues.DefaultValueIsRedundant, typed?.ToString() ?? "null");
                else
                    _property.DefaultValue = configOption;
            }

            var ctx = new PropertyTypeHandlerBuildContext()
                      {
                          Property = _property
                      };

            
        }

        public void ProcessCollection<T, TElement>()
        {
            Process<T>();


        }
    }
}
