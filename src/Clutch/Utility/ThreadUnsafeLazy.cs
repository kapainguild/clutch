using System;

namespace Clutch.Utility
{
    public class ThreadUnsafeLazy<T> where T : class
    {
        private readonly Func<T> _factory;
        private T _value;

        public ThreadUnsafeLazy(Func<T> factory)
        {
            _factory = factory;
        }

        public T GetValueOrDefault() => _value;


        public T Value
        {
            get
            {
                if (_value == null)
                    _value = _factory();
                return _value;
            }
        }
    }
}
