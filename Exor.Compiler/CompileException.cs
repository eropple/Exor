using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exor.Core;

namespace Exor.Compiler
{
    public class CompileException<TCodeSource> : ExorLoaderException
        where TCodeSource : ICodeSource
    {
        public IEnumerable<ExorCompilerResults<TCodeSource>> Results { get; private set; }

        public CompileException(IEnumerable<ExorCompilerResults<TCodeSource>> results)
            : base("One or more code sources failed to compile.")
        {
            Results = results;
        }
    }
}
