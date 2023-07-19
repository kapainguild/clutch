using System;
using System.Reflection.Emit;

namespace Clutch.Building
{
    public class StaticMethodDeclaration
    {
        public Type DelegateType { get; set; }

        public MethodBuilder MethodBuilder { get; set; }

        public Delegate UntypedDelegate { get; set; }

        public bool RequiresDelegate { get; set; }

        public string Name { get; set; }
    }
}
