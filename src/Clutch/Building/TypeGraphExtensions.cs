using System;
using System.Collections.Generic;

namespace Clutch.Building
{
    public class TypeGraphExtensions
    {
        private readonly Dictionary<Type, object> _extensionsData = new Dictionary<Type, object>();

        public void SetData<T>(object value)
        {
            if (_extensionsData.ContainsKey(typeof(T)))
                throw new ClutchInternalErrorException($"Extension data for {typeof(T)} is already set");

            _extensionsData[typeof(T)] = value;
        }

        public TValue GetData<T, TValue>()
        {
            if (_extensionsData.TryGetValue(typeof(T), out var val))
                return (TValue)val;
            return default;
        }

    }
}
