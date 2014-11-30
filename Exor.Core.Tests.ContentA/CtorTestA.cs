using System;
using Exor.Core.Tests.ContentBase;

namespace Exor.Core.Tests.ContentA
{
    [CtorTest("A")]
    public class CtorTestA : CtorTestBase
    {
        public CtorTestA(Int32 value, DateTime time) : base(value, time)
        {
        }
    }
}
