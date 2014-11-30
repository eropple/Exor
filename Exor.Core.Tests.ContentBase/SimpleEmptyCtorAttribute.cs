using System;
using Exor.Core;

namespace Exon.Core.Tests.ContentBase
{
    public class SimpleEmptyCtorAttribute : ExtensionAttribute
    {
        public SimpleEmptyCtorAttribute(String key) : base(key)
        {
        }
    }
}
