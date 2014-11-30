using System;

namespace Exon.Core.Tests.ContentBase
{
    public abstract class SimpleTestBase
    {
        public readonly Int32 Value;

        protected SimpleTestBase(int value)
        {
            Value = value;
        }
    }
}
