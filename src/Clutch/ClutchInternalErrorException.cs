using System;

namespace Clutch
{
    class ClutchInternalErrorException : Exception
    {
        public ClutchInternalErrorException(string message)
            : base(message)
        {
        }

        public ClutchInternalErrorException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
