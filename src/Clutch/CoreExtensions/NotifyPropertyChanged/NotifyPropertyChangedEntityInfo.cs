using System.Reflection;

namespace Clutch.CoreExtensions.NotifyPropertyChanged
{
    class NotifyPropertyChangedEntityInfo
    {
        public bool AddInterface { get; set; }

        public MethodInfo RaisePropertyChangedMethodInfo { get; set; }
    }
}
