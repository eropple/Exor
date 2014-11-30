using System;
using Exon.Core.Tests.ContentBase;

namespace Exor.Core.Tests.ContentA
{
    [SimpleEmptyCtor("A2")]
    public class OtherSimpleTestA : SimpleTestBase
    {
        public OtherSimpleTestA() : base(0)
        {
        }

        public OtherSimpleTestA(Int32 foo) : base(foo)
        {
            
        }
    }
}
