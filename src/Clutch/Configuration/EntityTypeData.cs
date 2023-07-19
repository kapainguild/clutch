using System;

namespace Clutch.Configuration
{
    public class EntityTypeData : StructuralTypeData
    {
        public EntityTypeData(Type type)
        {
            Type = type;
        }

        public Type Type { get; }

        public PropertyData[] Properties { get; set; }

        public Type ProxyType { get; set; }

        public Func<object> Creator { get; set; }
    }
}
