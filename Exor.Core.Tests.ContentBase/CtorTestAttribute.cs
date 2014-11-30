using System;

namespace Exor.Core.Tests.ContentBase
{
    public class CtorTestAttribute : ExtensionAttribute
    {
        public CtorTestAttribute(String key) : base(key)
        {
        }
    }
}
