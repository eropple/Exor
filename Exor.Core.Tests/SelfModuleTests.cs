using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using Common.Logging.Simple;
using Exon.Core.Tests.ContentBase;
using Exor.Core;
using Exor.Core.Tests.ContentBase;
using NUnit.Framework;

namespace Exon.Core.Tests
{
    [TestFixture]
    public class SelfModuleTests
    {
        private ILog _logger = new ConsoleOutLogger("Logs", LogLevel.All, true, false, false, "u");

        private readonly IEnumerable<ExtensionTypeRecord> TypeRecords = new ExtensionTypeRecord[]
        {
            ExtensionTypeRecord.Create<SimpleTestBase, SimpleEmptyCtorAttribute>(),
            ExtensionTypeRecord.Create<CtorTestBase, CtorTestAttribute>(typeof(Int32), typeof(DateTime))
        };
        
        
        [Test]
        public void NoContentFailure()
        {
            var loader = new ExtensionLoader(_logger, new Assembly[] {}, TypeRecords, true);

            try
            {
                var obj = loader.Load<SimpleTestBase>("A");
                Assert.Fail("Should not have been able to load a SimpleTestBase.");
            }
            catch
            {
                
            }
        }

        [Test]
        public void SingleLoad()
        {
            var assemblies = new[]
            {
                Assembly.Load(new AssemblyName("Exor.Core.Tests.ContentA"))
            };

            var loader = new ExtensionLoader(_logger, assemblies, TypeRecords, true);

            var noCtor = loader.Load<SimpleTestBase>("A");
            Assert.IsTrue(noCtor.GetType().FullName.Contains("SimpleTestA"));
            Assert.AreEqual(0, noCtor.Value);

            var withCtor = loader.Load<CtorTestBase>("A", 5, DateTime.MinValue);
            Assert.IsTrue(withCtor.GetType().FullName.Contains("CtorTestA"));
            Assert.AreEqual(5, withCtor.Value);
            Assert.AreEqual(DateTime.MinValue, withCtor.Time);
        }

        [Test]
        public void MultiLoad()
        {
            var assemblies = new[]
            {
                Assembly.Load(new AssemblyName("Exor.Core.Tests.ContentB")),
                Assembly.Load(new AssemblyName("Exor.Core.Tests.ContentA"))
            };

            var loader = new ExtensionLoader(_logger, assemblies, TypeRecords, true);

            var noCtor = loader.Load<SimpleTestBase>("A");
            Assert.IsTrue(noCtor.GetType().FullName.Contains("SimpleTestB"));
            Assert.AreEqual(0, noCtor.Value);

            var withCtor = loader.Load<CtorTestBase>("A", 5, DateTime.MinValue);
            Assert.IsTrue(withCtor.GetType().FullName.Contains("CtorTestB"));
            Assert.AreEqual(5, withCtor.Value);
            Assert.AreEqual(DateTime.MinValue, withCtor.Time);

            var deepLoad = loader.DeepEagerLoad<CtorTestBase>("A", 10, DateTime.MaxValue);
            Assert.IsTrue(deepLoad.Count == 2, "didn't find 2 instances");
            Assert.IsTrue(deepLoad[0].GetType().FullName.Contains("CtorTestB"));
            Assert.AreEqual(10, deepLoad[0].Value);
            Assert.AreEqual(DateTime.MaxValue, deepLoad[0].Time);

            Assert.IsTrue(deepLoad[1].GetType().FullName.Contains("CtorTestA"));
            Assert.AreEqual(10, deepLoad[1].Value);
            Assert.AreEqual(DateTime.MaxValue, deepLoad[1].Time);

            var mappedLoad = loader.LoadAll<SimpleTestBase>();
            Assert.IsTrue(mappedLoad.Count == 2, "didn't find 2 instances");

            Assert.IsTrue(mappedLoad.ContainsKey("A"), "has key A");
            Assert.IsTrue(mappedLoad["A"].GetType().FullName.Contains("SimpleTestB"));
            Assert.IsTrue(mappedLoad.ContainsKey("A2"), "has key A2");
            Assert.IsTrue(mappedLoad["A2"].GetType().FullName.Contains("OtherSimpleTestA"));

            var deepMappedLoad = loader.DeepLoadAll<SimpleTestBase>();
            Assert.IsTrue(deepMappedLoad.Count == 2, "didn't find 2 keys");

            Assert.IsTrue(deepMappedLoad.ContainsKey("A"), "has key A");
            Assert.IsTrue(deepMappedLoad["A"][0].GetType().FullName.Contains("SimpleTestB"));
            Assert.IsTrue(deepMappedLoad["A"][1].GetType().FullName.Contains("SimpleTestA"));
            Assert.IsTrue(deepMappedLoad.ContainsKey("A2"), "has key A2");
            Assert.IsTrue(deepMappedLoad["A2"][0].GetType().FullName.Contains("OtherSimpleTestA"));
        }
    }
}
