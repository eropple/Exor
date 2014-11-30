using System;

namespace Exor.Core.Tests.ContentBase
{
    public abstract class CtorTestBase
    {
        public readonly Int32 Value;
        public readonly DateTime Time;

        protected CtorTestBase(int value, DateTime time)
        {
            Value = value;
            Time = time;
        }
    }
}
