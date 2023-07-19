using System;
using System.Collections.Generic;
using System.Linq;


namespace Clutch.Building
{
    class CollectionsHelper
    {
        public static bool IsCollectionType(Type propertyReturnType, out Type elementType)
        {
            elementType = null;
            if (propertyReturnType.IsArray)
                return false;

            var first = propertyReturnType.GetInterfaces().FirstOrDefault(x =>
                  x.IsGenericType &&
                  x.GetGenericTypeDefinition() == typeof(ICollection<>));

            if (first != null)
            {
                elementType = first.GenericTypeArguments[0];
                return true;
            }

            return false;
        }
    }
}
