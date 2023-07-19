using System;

namespace Clutch
{
    public class ClutchRuntimeException : ClutchException
    {
        public ClutchRuntimeException(string message)
            : base(message)
        {
        }

        public ClutchRuntimeException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
