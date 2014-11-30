using System;

namespace Exor.Core
{
    public class ExorException : Exception
    {
        public ExorException()
        {
        }

        public ExorException(string message)
            : base(message)
        {
        }

        public ExorException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public ExorException(Exception innerException, String message, params Object[] args)
            : base(String.Format(message, args), innerException)
        { }

        public ExorException(String message, params Object[] args)
            : base(String.Format(message, args), null)
        { }
    }
}
