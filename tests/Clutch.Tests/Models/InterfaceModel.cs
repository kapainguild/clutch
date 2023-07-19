namespace Clutch.Tests.Models
{
    interface IRoot
    {
        int RootInt { get; set; }

        string RootString { get; set; }
    }

    interface IBase : IRoot
    {
        int BaseInt { get; set; }

        string BaseString { get; set; }
    }

    interface IDerived1 : IBase
    {
        int Derived1 { get; set; }
    }

    interface IDerived2 : IBase
    {
        int Derived2 { get; }
    }

    interface IDerivedFinal : IDerived1, IDerived2
    {
        int DerivedFinal { get; set; }
    }

    interface IRoot2
    {
        int Root2Int { get; set; }

        string Root2String { get; set; }
    }
}
