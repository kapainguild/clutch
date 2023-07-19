using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using Clutch.Building.ProxySupport;

namespace Clutch.Building
{
    class PropertyTypeHandlerCollection
    {
        public static PropertyTypeHandler BuildList<T>(PropertyTypeHandlerFactory factory, PropertyTypeHandlerBuildContext ctx)
        {
            var type = typeof(T);
            var underlyingType = type.GetGenericArguments()[0];

            var handler = factory.GetUnderlyingTypeHandler(underlyingType, ctx);
            return new PropertyTypeHandler
                   {
                       CompareAndGotoLabel = c => { }, //TODO?
                       Serialize = c => { }, //TODO?
                       Deserialize = c => { }, //TODO?
                       LoadDefaultValue = c => LoadDefaultValue(underlyingType)(c), //TODO? // this also take part in CompareContext, wrongly
                       IsSystemDefaultValue = c => { }, //TODO?
                   };
        }

        public static Action<LoadDefaultPropertyContext> LoadDefaultValue(Type underlyingType)
        {
            return s =>
                   {
                       var ctor = typeof(List<>).MakeGenericType(underlyingType).GetConstructor(Type.EmptyTypes); // TODO
                       s.Generator.New(ctor);
                   };
        }
    }
}
