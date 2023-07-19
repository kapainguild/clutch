using System.Runtime.CompilerServices;

namespace Clutch.Configuration
{
    public class ConfigOptionDeclarationBase : IConfigOptionDeclaration
    {
        public string Name { get; }

        public bool IsPassThrough { get; set; }

        public ConfigOptionDeclarationBase(bool isPassThrough, string callerMemberName)
        {
            IsPassThrough = isPassThrough;
            Name = callerMemberName;
        }

        public override string ToString() => Name;
    }

    public class ConfigOptionDeclaration<TValue> : ConfigOptionDeclarationBase
    {
        public TValue DefaultValue { get; }
        
        public ConfigOptionDeclaration(TValue defaultValue, bool isPassThrough = false, [CallerMemberName] string callerMemberName = null) :
            base(isPassThrough, callerMemberName)
        { 
            DefaultValue = defaultValue;
        }
    }

    public class ConfigOptionDeclarationOpenType : ConfigOptionDeclarationBase
    {
        public ConfigOptionDeclarationOpenType(bool isPassThrough = false, [CallerMemberName] string callerMemberName = null)
            : base(isPassThrough, callerMemberName)
        {
        }
    }
}
