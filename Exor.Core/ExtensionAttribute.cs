using System;

namespace Exor.Core
{
    /// <summary>
    /// Parent class for the attributes to which classes can be tagged in order
    /// to inform Loadrix that that class can satisfy an extension dependency.
    /// </summary>
    /// <remarks>
    /// Every Exor extension has a string key. This key is a string against which
    /// Loadrix can search, in order to look up the class that correctly
    /// satisfies the request.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, 
                    AllowMultiple = false, Inherited = false)]
    public abstract class ExtensionAttribute : Attribute
    {
        public readonly String Key;

        protected ExtensionAttribute(String key)
        {
            Key = key;
        }
    }
}
