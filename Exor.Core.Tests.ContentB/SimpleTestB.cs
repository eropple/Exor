using System;
using Exon.Core.Tests.ContentBase;

namespace Exor.Core.Tests.ContentB
{
    [SimpleEmptyCtor("A")]
    public class SimpleTestB : SimpleTestBase
    {
        public SimpleTestB() : base(0)
        {
        }

        public SimpleTestB(Int32 foo) : base(foo)
        {
            
        }
    }
}
