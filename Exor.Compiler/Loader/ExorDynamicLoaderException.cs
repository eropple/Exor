using System;
using Exor.Core;

namespace Exor.Compiler.Loader
{
    public class ExorDynamicLoaderException : ExorLoaderException
    {
        public ExorDynamicLoaderException()
        {
        }

        public ExorDynamicLoaderException(string message) : base(message)
        {
        }

        public ExorDynamicLoaderException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public ExorDynamicLoaderException(Exception innerException, string message, params object[] args) : base(innerException, message, args)
        {
        }

        public ExorDynamicLoaderException(string message, params object[] args) : base(message, args)
        {
        }
    }
}
