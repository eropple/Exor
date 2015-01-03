using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CSharp;
using Versioner;

namespace Exor.Compiler
{
    public class CSharpCompiler<TCodeSource>
        where TCodeSource : ICodeSource
    {
        public readonly CSharpCompilerOptions Options;

        private readonly CSharpCodeProvider _compiler;

        private readonly IReadOnlyList<String> RequiredAssemblies = new List<String>
        {
            "Exor.Core.dll", 
            
            "System.dll", 
            "System.Runtime.dll",
            "mscorlib.dll", 
            "System.Linq.dll", 
            "System.Core.dll"
        };

        public CSharpCompiler(CSharpCompilerOptions options)
        {
            Options = options;

            _compiler = new CSharpCodeProvider(new Dictionary<String, String>
            {
                { "CompilerVersion", "v4.0" }
            });
        }


        public Boolean TryCompileSources(IEnumerable<TCodeSource> selectedSources,
                                         out List<ExorCompilerResults<TCodeSource>> results)
        {
            return TryCompileSources(selectedSources, Enumerable.Empty<TCodeSource>(), out results);
        }
        public Boolean TryCompileSources(IEnumerable<TCodeSource> selectedSources,
                                         IEnumerable<TCodeSource> additionalSources,
                                         out List<ExorCompilerResults<TCodeSource>> results)
        {
            // TODO: when Common.Services.Logging gets their act together, get this logged nice and verbosely.

            if (!Directory.Exists(Options.CachePath))
            {
                Directory.CreateDirectory(Options.CachePath);
            }

            var selected = selectedSources.ToList();
            var additional = (additionalSources ?? Enumerable.Empty<TCodeSource>()).ToList();

            // this is the whole reason I made Versioner. man, I'm a dork.
            var loadOrder = Resolver.ComputePriorityOrderWithDependencies(selected, additional);

            var dirtySet = new HashSet<TCodeSource>();

            results = loadOrder.Select(s => Compile(s, dirtySet)).ToList();

            return results.All(r => r.Succeeded);
        }

        public List<ExorCompilerResults<TCodeSource>> Compile(IEnumerable<TCodeSource> selectedSources,
                                                              IEnumerable<TCodeSource> additionalSources = null)
        {
            List<ExorCompilerResults<TCodeSource>> results;
            if (!TryCompileSources(selectedSources, additionalSources, out results))
            {
                throw new CompileException<TCodeSource>(results);
            }
            return results;
        }

        public void DeleteCache()
        {
            if (Directory.Exists(Options.CachePath))
            {
                Directory.Delete(Options.CachePath, true);
            }
        }

        private ExorCompilerResults<TCodeSource> Compile(TCodeSource source, HashSet<TCodeSource> dirtySet)
        {
            Boolean mustRecompile = false;
            var sourceFiles = source.CodeFiles.ToArray();

            var cacheAssemblyPath = CacheNameFor(source.UniqueName);
            if (!File.Exists(cacheAssemblyPath))
            {
                mustRecompile = true;
            }
            else
            {
                var cacheDate = File.GetLastWriteTimeUtc(cacheAssemblyPath);
                mustRecompile = sourceFiles.Any(f => File.GetLastWriteTimeUtc(f) >= cacheDate) ||
                                source.Dependencies.Any(dep => File.GetLastWriteTimeUtc(CacheNameFor(dep.UniqueName)) >= cacheDate);
            }

            if (!mustRecompile) return new ExorCompilerResults<TCodeSource>(source, Assembly.LoadFile(CacheNameFor(source.UniqueName)), null, new List<CompilerError>());

            dirtySet.Add(source);

            var referencedAssemblies =  RequiredAssemblies
                .Concat<String>(Options.UniversalAssemblies)
                .Concat<String>(source.AdditionalAssemblies)
                .Concat<String>(source.Dependencies.Select(dep => CacheNameFor(dep.UniqueName)))
                .ToArray();

            var opts = new CompilerParameters(referencedAssemblies, cacheAssemblyPath, false)
            {
                IncludeDebugInformation = Options.ProvideDebugInformation
            };

            var rawResult = _compiler.CompileAssemblyFromFile(opts, sourceFiles);

            var errorList = new List<CompilerError>(rawResult.Errors.Cast<object>().Cast<CompilerError>());
            Assembly assembly;
            try
            {
                assembly = rawResult.CompiledAssembly;
            }
            catch (FileNotFoundException)
            {
                assembly = null;
            }
            return new ExorCompilerResults<TCodeSource>(source, assembly, rawResult, errorList);
        }

        private String CacheNameFor(String name)
        {
            return Path.Combine(Options.CachePath, name + ".exonmod.dll");
        }
    }

    public class CSharpCompilerOptions
    {
        public readonly String CachePath;
        public readonly IReadOnlyList<String> UniversalAssemblies;
        public readonly Boolean ProvideDebugInformation;
        public readonly Boolean ForceRecompile;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="provideDebugInformation">
        /// If true, generate PDBs.
        /// </param>
        /// <param name="cachePath">
        /// The path in which cached copies of compiled assemblies should be stored. If this
        /// is null, a cache name will be generated in the temporary directory. (The cache
        /// is used for dependency loading and is not optional.)
        /// </param>
        /// <param name="universalAssemblies">
        /// A list of assemblies that should be added to the compilation path for all code
        /// modules built by the compiler. If this is null, no assemblies will be added.
        /// </param>
        /// <param name="forceRecompile">
        /// Overrides the date-checking cache behaviors of the compiler. All sources will
        /// be recompiled.
        /// </param>
        public CSharpCompilerOptions(String cachePath = null, IEnumerable<String> universalAssemblies = null, Boolean forceRecompile = false, bool provideDebugInformation = true)
        {
            ProvideDebugInformation = provideDebugInformation;
            ForceRecompile = forceRecompile;
            CachePath = cachePath ?? FindCachePath();
            UniversalAssemblies = (universalAssemblies ?? Enumerable.Empty<String>()).ToList();
        }

        private static String FindCachePath()
        {
            var entryAssembly = Assembly.GetEntryAssembly();
            var segments = new String[]
            {
                Path.GetTempPath(),
                entryAssembly?.GetName()?.Name,
                entryAssembly?.GetCustomAttribute<AssemblyVersionAttribute>()?.Version
            }.Where(s => !String.IsNullOrWhiteSpace(s)).ToArray();

            return Path.Combine(segments);
        }
    }
}
