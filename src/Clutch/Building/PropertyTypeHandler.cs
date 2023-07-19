using System;

namespace Clutch.Building
{
    public class PropertyTypeHandler
    {
        public static PropertyTypeHandler Null { get; }= new PropertyTypeHandler();

        public Action<ComparePropertyContext> CompareAndGotoLabel { get; set; }

        public Action<SerializePropertyContext> Serialize { get; set; }

        public Action<DeserializePropertyContext> Deserialize { get; set; }

        public Action<LoadDefaultPropertyContext> LoadDefaultValue { get; set; }

        public Action<IsSystemDefaultPropertyContext> IsSystemDefaultValue { get; set; }
    }
}
