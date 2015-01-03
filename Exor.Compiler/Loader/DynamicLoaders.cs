using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Common.Logging;
using Exor.Core;

namespace Exor.Compiler.Loader
{
    public static class DynamicLoaders
    {
        public static ExtensionLoader BuildLoader<TCodeSource>(
            ILog logger,
            IEnumerable<ExtensionTypeRecord> typeRecords,
            IEnumerable<TCodeSource> selectedSources,
            IEnumerable<TCodeSource> additionalSources = null,
            CSharpCompilerOptions compilerOptions = null,
            Options options = null)
            where TCodeSource : ICodeSource
        {

            options = options ?? new Options();
            var compiler = new CSharpCompiler<TCodeSource>(compilerOptions ?? new CSharpCompilerOptions());
            var selectedSourcesList = selectedSources as IList<TCodeSource> ?? selectedSources.ToList();
            var results = compiler.Compile(selectedSourcesList, additionalSources).ToDictionary(r => r.Source.UniqueName);

            var assemblies = new List<Assembly>(results.Count);
            var orderedSelected = selectedSourcesList.Select(s => results[s.UniqueName]).ToList();
            if (options.InclusionStrategy == InclusionStrategy.AfterFirst)
            {
                // TODO: implement
                throw new NotImplementedException("non-None inclusion strategies not yet implemented, pull requests welcome.");
            }
            else if (options.InclusionStrategy == InclusionStrategy.AfterLast)
            {
                // TODO: implement
                throw new NotImplementedException("non-None inclusion strategies not yet implemented, pull requests welcome.");
            }

            return new ExtensionLoader(logger, assemblies, typeRecords, options.ThrowOnConstructorMissing);
        }
    }
}