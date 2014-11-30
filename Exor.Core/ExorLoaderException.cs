using System;

namespace Exor.Core
{
    public class ExorLoaderException : ExorException
    {
        public ExorLoaderException()
        {
        }

        public ExorLoaderException(string message) : base(message)
        {
        }

        public ExorLoaderException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public ExorLoaderException(Exception innerException, string message, params object[] args) : base(innerException, message, args)
        {
        }

        public ExorLoaderException(string message, params object[] args) : base(message, args)
        {
        }
    }
}
