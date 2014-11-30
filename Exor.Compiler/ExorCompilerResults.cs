using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Exor.Compiler
{
    public class ExorCompilerResults<TCodeSource>
        where TCodeSource : ICodeSource
    {
        public readonly TCodeSource Source;
        public readonly Assembly Assembly;
        public readonly IReadOnlyList<CompilerError> Errors;
        public readonly CompilerResults RawResults;

        internal ExorCompilerResults(TCodeSource source, Assembly assembly, CompilerResults rawResults, IReadOnlyList<CompilerError> errors)
        {
            Source = source;
            Assembly = assembly;
            Errors = errors;
            RawResults = rawResults;
        }

        public Boolean Failed { get { return Errors.Count > 0; } }
        public Boolean Succeeded { get { return !Failed; } }
    }
}
