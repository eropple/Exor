using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Versioner;
using Version = Versioner.Version;

namespace Exor.Compiler.Tests.Domain
{
    public class CodeSource : ICodeSource
    {
        public CodeSource(string uniqueName, Version version,
                          IEnumerable<string> codeFiles, 
                          IEnumerable<Dependency> dependencies = null,
                          IEnumerable<String> additionalAssemblies = null)
        {
            AdditionalAssemblies = additionalAssemblies ?? Enumerable.Empty<String>();
            CodeFiles = codeFiles;
            Dependencies = dependencies ?? Enumerable.Empty<Dependency>();
            Version = version;
            UniqueName = uniqueName;
        }

        public string UniqueName { get; private set; }
        public Version Version { get; private set; }
        public IEnumerable<Dependency> Dependencies { get; private set; }
        public IEnumerable<string> CodeFiles { get; private set; }
        public IEnumerable<string> AdditionalAssemblies { get; private set; }
    }
}
