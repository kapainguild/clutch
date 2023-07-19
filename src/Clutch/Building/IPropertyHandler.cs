using System;
using System.Collections.Generic;
using System.Text;

namespace Clutch.Building
{
    public interface IPropertyHandler
    {
        void GenerateInit(LoadDefaultPropertyContext ctx);

        void GenerateSerialize(SerializePropertyContext ctx);

        void GenerateDeserialize(DeserializePropertyContext ctx);

        void GenerateSetter(PropertySetterContext ctx);
    }
}
