using System.Collections.Generic;
using Clutch.Helpers;

namespace Clutch.Configuration
{
    public class ConfigOptionBag
    {
        private readonly Dictionary<IConfigOptionDeclaration, IConfigOption> _options = new Dictionary<IConfigOptionDeclaration, IConfigOption>();

        public void Set<TValue>(ConfigOptionDeclaration<TValue> declaration, TValue value)
        {
            var option = (ConfigOption<TValue>)_options.GetOrCreate(declaration, () => new ConfigOption<TValue>(declaration));
            option.Set(value);
        }

        public void Set<TValue>(ConfigOptionDeclarationOpenType declaration, TValue value)
        {
            var option = (ConfigOption<TValue>)_options.GetOrCreate(declaration, () => new ConfigOption<TValue>(declaration));
            option.Set(value);
        }

        public ConfigOptionCall<TValue> Get<TValue>(ConfigOptionDeclaration<TValue> declaration)
        {
            if (TryGet(declaration, out var value))
            {
                return value;
            }
            return null;
        }

        public TValue GetValueOrDefault<TValue>(ConfigOptionDeclaration<TValue> declaration)
        {
            if (TryGet(declaration, out var value))
            {
                return value.Value;
            }
            return declaration.DefaultValue;
        }

        public bool TryGet<TValue>(ConfigOptionDeclaration<TValue> declaration, out ConfigOptionCall<TValue> value)
        {
            if (_options.TryGetValue(declaration, out var option))
            {
                ConfigOption<TValue> typed = (ConfigOption<TValue>)option;
                value = typed.Get();
                if (value == null)
                    throw new ClutchInternalErrorException($"Value for {declaration} is expected but was null");
                return true;
            }

            value = null;
            return false;
        }

        public bool TryGet(IConfigOptionDeclaration declaration, out IConfigOption value)
        {
            return _options.TryGetValue(declaration, out value);
        }

        public IEnumerable<IConfigOption> GetAll()
        {
            return _options.Values;
        }

        public void Merge(ConfigOptionBag from)
        {
            foreach (var fromOption in from._options)
            {
                var thisOption = _options.GetOrCreate(fromOption.Key, () => fromOption.Value.CreateOption());
                thisOption.Merge(fromOption.Value);
            }
        }
    }
}
