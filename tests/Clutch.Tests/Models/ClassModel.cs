namespace Clutch.Tests.Models
{
    class RootClass
    {
        public virtual int RootInt { get; set; }

        public virtual string RootString { get; set; }
    }

    class BaseClass : RootClass
    {
        public virtual int BaseInt { get; set; }

        public virtual string BaseString { get; set; }
    }

    abstract class DerivedAbstractClass : BaseClass
    {
        public abstract int Abstruct { get; set; }
    }
}
