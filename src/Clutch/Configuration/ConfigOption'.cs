using System.Collections.Generic;
using Clutch.Configuration.Issues;

namespace Clutch.Configuration
{
    public class ConfigOption<TValue> : IConfigOption
    {
        private readonly List<ConfigOptionCall<TValue>> _calls = new List<ConfigOptionCall<TValue>>();

        public IConfigOptionDeclaration Declaration { get; }

        public bool IsValueRetrieved { get; private set; }

        public ConfigOption(IConfigOptionDeclaration declaration)
        {
            Declaration = declaration;
        }

        public void Set(TValue value)
        {
            _calls.Add(new ConfigOptionCall<TValue>(value, CallerInfo.GetCurrent()));
        }

        public ConfigOptionCall<TValue> Get()
        {
            IsValueRetrieved = true;
            return GetLast();
        }

        public void Merge(IConfigOption fromOption)
        {
            if (fromOption is ConfigOption<TValue> from)
            {
                if (_calls.Count == 0)
                {
                    _calls.Add(from.GetLast());
                }
            }
            else 
                throw new ClutchInternalErrorException($"Option '{Declaration.Name}' of type {fromOption.GetType()} does not correspond to current type {GetType()}");
        }

        public (string optionName, CallerInfo callerInfo, object value) GetLastCallIfMoreThanOnce()
        {
            if (_calls.Count > 1)
                return (Declaration.Name, GetLast().CallerInfo, GetLast().Value);
            return (null, null, null);
        }

        public IConfigOption CreateOption() => new ConfigOption<TValue>(Declaration);

        private ConfigOptionCall<TValue> GetLast()
        {
            if (_calls.Count == 0)
                throw new ClutchInternalErrorException($"Option '{Declaration.Name}' exists without value");
            return _calls[_calls.Count - 1];
        } 
    }
}
