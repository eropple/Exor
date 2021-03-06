﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using Common.Logging.Simple;
using Exon.Core.Tests.ContentBase;
using Exor.Compiler.Tests.Domain;
using Exor.Core;
using Exor.Core.Tests.ContentBase;
using NUnit.Framework;
using Versioner;
using Version = Versioner.Version;

namespace Exor.Compiler.Tests
{
    [SetUpFixture]
    public class SetUpFixture
    {
        [SetUp]
        public void BeforeAllTests()
        {
            if (Directory.Exists(CompilerTests.CachePath)) Directory.Delete(CompilerTests.CachePath, true);
        }
    }

    [TestFixture]
    public class CompilerTests
    {
        private ILog _logger;

        public readonly String SolutionDirectory =
            Path.Combine(Path.GetDirectoryName(AppDomain.CurrentDomain.SetupInformation.ApplicationBase), "..", "..");

        public readonly String AssetDirectory =
            Path.Combine(Path.GetDirectoryName(AppDomain.CurrentDomain.SetupInformation.ApplicationBase), "Assets");

        private readonly IEnumerable<ExtensionTypeRecord> TypeRecords = new ExtensionTypeRecord[]
        {
            ExtensionTypeRecord.Create<SimpleTestBase, SimpleEmptyCtorAttribute>(),
            ExtensionTypeRecord.Create<CtorTestBase, CtorTestAttribute>(typeof(Int32), typeof(DateTime))
        };

        public static readonly String CachePath = Path.Combine(Path.GetTempPath(), "ExorTests");

        [SetUp]
        public void BeforeEachTest()
        {
            _logger = new ConsoleOutLogger("Logs", LogLevel.All, true, false, false, "u");

            if (Directory.Exists(AssetDirectory)) Directory.Delete(AssetDirectory, true);

            var dict = new Dictionary<String, String>
            {
                {"Exor.Compiler.Tests.ContentC", "ContentC"},
                {"Exor.Core.Tests.ContentB", "ContentB"},
                {"Exor.Core.Tests.ContentA", "ContentA"}
            };

            foreach (var kvp in dict)
            {
                var source = new DirectoryInfo(Path.Combine(SolutionDirectory, kvp.Key));
                var dest = new DirectoryInfo(Path.Combine(AssetDirectory, kvp.Value));

                CopyFilesRecursively(source, dest);

                var bin = Path.Combine(dest.ToString(), "bin");
                var obj = Path.Combine(dest.ToString(), "obj");

                if (Directory.Exists(bin)) Directory.Delete(bin, true);
                if (Directory.Exists(obj)) Directory.Delete(obj, true);
            }
        }

        [Test]
        public void SimpleCompile()
        {
            Directory.CreateDirectory(CachePath);
            var compiler = new CSharpCompiler<CodeSource>(new CSharpCompilerOptions(forceRecompile: true,
                universalAssemblies: new[] { "Exor.Core.Tests.ContentBase.dll" }, cachePath: Path.Combine(CachePath, "SimpleCompile")));

            var contentA = new CodeSource("A", Version.Parse("1.0.0"),
                Directory.GetFiles(Path.Combine(AssetDirectory, "ContentA")).Where(f => Path.GetExtension(f) == ".cs"));

            List<ExorCompilerResults<CodeSource>> results;
            if (!compiler.TryCompileSources(new[] {contentA}, out results))
            {
                Assert.Fail("Compile of ContentA failed.");
            }

            var loader = new ExtensionLoader(_logger, results.Select(r => r.Assembly), TypeRecords);

            var obj = loader.Load<SimpleTestBase>("A");
            Assert.IsTrue(obj.GetType().FullName.Contains("SimpleTestA"));
        }

        [Test]
        public void DependentCompile()
        {
            Directory.CreateDirectory(CachePath);
            var compiler = new CSharpCompiler<CodeSource>(new CSharpCompilerOptions(forceRecompile: true,
                universalAssemblies: new[] { "Exor.Core.Tests.ContentBase.dll" }, cachePath: Path.Combine(CachePath, "DependentCompile")));

            var contentA = new CodeSource("A", Version.Parse("1.0.0"),
                Directory.GetFiles(Path.Combine(AssetDirectory, "ContentA")).Where(f => Path.GetExtension(f) == ".cs"));
            var contentB = new CodeSource("B", Version.Parse("1.0.0"),
                Directory.GetFiles(Path.Combine(AssetDirectory, "ContentB")).Where(f => Path.GetExtension(f) == ".cs"));
            var contentC = new CodeSource("C", Version.Parse("1.0.0"),
                Directory.GetFiles(Path.Combine(AssetDirectory, "ContentC")).Where(f => Path.GetExtension(f) == ".cs"),
                    new[]
                    {
                        new Dependency("A", "==", Version.Parse("1.0.0")), 
                    }
                );

            var selected = new[] {contentC, contentB};
            var additional = new[] {contentA};

            List<ExorCompilerResults<CodeSource>> results;
            if (!compiler.TryCompileSources(selected, additional, out results))
            {
                Assert.Fail("Compile failed.");
            }

            var resultsMap = results.ToDictionary(r => r.Source.UniqueName);
            var loadOrder = selected.Select(c => resultsMap[c.UniqueName].Assembly);

            var loader = new ExtensionLoader(_logger, loadOrder, TypeRecords);

            var obj = loader.DeepLoad<SimpleTestBase>("A").ToList();
            Assert.IsTrue(obj[0].GetType().FullName.Contains("SimpleTestC"));
            Assert.IsTrue(obj[0].GetType().GetTypeInfo().BaseType.FullName.Contains("SimpleTestA"));
            Assert.IsTrue(obj[1].GetType().FullName.Contains("SimpleTestB"));
        }



        public static void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target)
        {
            foreach (var dir in source.GetDirectories())
                CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name));
            foreach (var file in source.GetFiles())
                file.CopyTo(Path.Combine(target.FullName, file.Name));
        }
    }
}
