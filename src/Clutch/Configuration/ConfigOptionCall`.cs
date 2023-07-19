using Clutch.Configuration.Issues;

namespace Clutch.Configuration
{
    public class ConfigOptionCall<TValue>
    {
        public TValue Value { get; }

        public CallerInfo CallerInfo { get; }

        public ConfigOptionCall(TValue value, CallerInfo callerInfo)
        {
            Value = value;
            CallerInfo = callerInfo;
        }
    }
}
