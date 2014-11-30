using System;
using Exor.Core.Tests.ContentBase;

namespace Exor.Core.Tests.ContentB
{
    [CtorTest("A")]
    public class CtorTestB : CtorTestBase
    {
        public CtorTestB(Int32 value, DateTime time) : base(value, time)
        {
        }
    }
}
