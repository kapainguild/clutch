using Clutch.Configuration.Issues;

namespace Clutch.Configuration
{
    public interface IConfigOption
    {
        void Merge(IConfigOption fromOptionValue);

        (string optionName, CallerInfo callerInfo, object value) GetLastCallIfMoreThanOnce();

        IConfigOption CreateOption();
    }
}
