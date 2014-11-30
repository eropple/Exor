using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Versioner;

namespace Exor.Compiler
{
    /// <summary>
    /// Should be implemented by the "mod" concept in the consuming application. Exor will
    /// use Versioner to perform dependency resolution and compile all dependencies ahead of 
    /// </summary>
    public interface ICodeSource : IVersioned, IDepending
    {
        /// <summary>
        /// An enumerated list of all C# files to be compiled by this code source.
        /// </summary>
        IEnumerable<String> CodeFiles { get; }
        /// <summary>
        /// The set of dependent assemblies to be loaded on behalf of this code source.
        /// </summary>
        IEnumerable<String> AdditionalAssemblies { get; } 
    }
}
