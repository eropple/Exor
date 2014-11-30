using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exon.Core.Tests.ContentBase;
using Exor.Core.Tests.ContentA;

namespace Exor.Compiler.Tests.ContentC
{
    [SimpleEmptyCtor("A")]
    public class SimpleTestC : SimpleTestA
    {
        public SimpleTestC() : base()
        {
        }
    }
}
