using System;

namespace Clutch
{
    public class ClutchException : Exception
    {
        protected ClutchException(string message) : base(message)
        {
        }

        protected ClutchException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
