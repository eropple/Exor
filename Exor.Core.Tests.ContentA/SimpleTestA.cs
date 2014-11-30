using System;
using Exon.Core.Tests.ContentBase;

namespace Exor.Core.Tests.ContentA
{
    [SimpleEmptyCtor("A")]
    public class SimpleTestA : SimpleTestBase
    {
        public SimpleTestA() : base(0)
        {
        }
    }
}
