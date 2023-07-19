using System;
using System.Reflection;

namespace Clutch.Utility
{
    class LazyConstructor : ThreadUnsafeLazy<ConstructorInfo>
    {
        public LazyConstructor(Func<ConstructorInfo> factory) : base(factory) { }
    }
}
