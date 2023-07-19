namespace Clutch.Tests.Models
{
    /// <summary>
    /// class for testing propertyaccess modes
    /// </summary>
    class PropertiesWithCounters
    {
        public int _test;
        public int _getterCalled;
        public int _setterCalled;

        public virtual int Test
        {
            get
            {
                _getterCalled++;
                return _test;
            }
            set
            {
                _setterCalled++;
                _test = value;
            }
        }
    }

    class PropertiesWithCountersDerived : PropertiesWithCounters
    {

    }
}
