using System;
using System.Reflection;

namespace Clutch.Utility
{
    class LazyMethod : ThreadUnsafeLazy<MethodInfo>
    {
        public LazyMethod(Func<MethodInfo> factory) : base(factory) { }
    }
}
