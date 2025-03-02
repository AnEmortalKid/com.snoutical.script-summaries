using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Snoutical.ScriptSummaries.Generation.Constants;
using Snoutical.ScriptSummaries.Generation.Generator;

namespace Snoutical.ScriptSummaries.Editor.Test.Generation.Generator
{
    [TestFixture]
    public class SummaryGeneratorTest
    {
        private string tempDirectory;

        [SetUp]
        public void Setup()
        {
            tempDirectory = Path.Combine(Path.GetTempPath(), "SummaryGeneratorTestRoot");
            Directory.CreateDirectory(tempDirectory);
        }

        [TearDown]
        public void Cleanup()
        {
            // Delete everything in the test directory
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, true);
            }
        }

        [Test]
        public void GenerateMapping_Generates_DefaultAssemblyScript()
        {
            string nestedDirs = Path.Combine(tempDirectory, "Nested", "SubDir");
            Directory.CreateDirectory(nestedDirs);

            var tempScriptPath = Path.Combine(nestedDirs, "TestScript.cs");

            string scriptContent = @"
            using UnityEngine;
            /// <summary>Test summary</summary>
            public class TestScript : MonoBehaviour { }
        ";
            File.WriteAllText(tempScriptPath, scriptContent);

            // Set our fake Assets root
            var generator = new SummaryGenerator(tempDirectory);

            var mapping = generator.GenerateMapping(tempScriptPath);

            Assert.IsNotNull(mapping, "Mapping should not be null.");
            Assert.AreEqual(GenerationConstants.FallbackAssemblyName, mapping.assemblyName,
                "Should use fallback assembly.");
            Assert.AreEqual("Assets/Nested/SubDir/TestScript.cs", mapping.relativePath, "Path should be Unity-style.");
            Assert.AreEqual("T:TestScript", mapping.memberIdentifier, "Should detect class name.");
            Assert.AreEqual("Test summary", mapping.summary, "Summary should match.");
        }

        [Test]
        public void GenerateMapping_PicksClosestAssembly()
        {
            string nestedParent = Path.Combine(tempDirectory, "Nested");
            Directory.CreateDirectory(nestedParent);

            // place script in subdir
            string scriptSubdir = Path.Combine(nestedParent, "SubDir");
            Directory.CreateDirectory(scriptSubdir);
            var tempScriptPath = Path.Combine(scriptSubdir, "TestScript.cs");

            var nestedAssembly = Path.Combine(nestedParent, "NestedAssembly.asmdef");
            var subdirAssembly = Path.Combine(scriptSubdir, "SubdirAssembly.asmdef");

            string scriptContent = @"
            using UnityEngine;
            /// <summary>Test summary</summary>
            public class TestScript : MonoBehaviour { }
        ";
            File.WriteAllText(tempScriptPath, scriptContent);

            // Write our assemblies
            string nestedAsmdefContent = @"{ ""name"": ""NestedAssembly"" }";
            File.WriteAllText(nestedAssembly, nestedAsmdefContent);

            string subdirdAsmdefContent = @"{ ""name"": ""SubdirAssembly"" }";
            File.WriteAllText(subdirAssembly, subdirdAsmdefContent);

            // Set our fake Assets root
            var generator = new SummaryGenerator(tempDirectory);

            var mapping = generator.GenerateMapping(tempScriptPath);

            Assert.IsNotNull(mapping, "Mapping should not be null.");
            Assert.AreEqual("SubdirAssembly", mapping.assemblyName,
                "Should use closest assembly.");
        }

        [Test]
        public void GenerateMapping_FindsParentAssembly()
        {
            string nestedParent = Path.Combine(tempDirectory, "Nested");
            Directory.CreateDirectory(nestedParent);

            // place script in subdir
            string scriptSubdir = Path.Combine(nestedParent, "SubDir");
            Directory.CreateDirectory(scriptSubdir);
            var tempScriptPath = Path.Combine(scriptSubdir, "TestScript.cs");

            var nestedAssembly = Path.Combine(nestedParent, "NestedAssembly.asmdef");

            string scriptContent = @"
            using UnityEngine;
            /// <summary>Test summary</summary>
            public class TestScript : MonoBehaviour { }
        ";
            File.WriteAllText(tempScriptPath, scriptContent);

            // Write our assemblies
            string nestedAsmdefContent = @"{ ""name"": ""NestedAssembly"" }";
            File.WriteAllText(nestedAssembly, nestedAsmdefContent);

            // Set our fake Assets root
            var generator = new SummaryGenerator(tempDirectory);

            var mapping = generator.GenerateMapping(tempScriptPath);

            Assert.IsNotNull(mapping, "Mapping should not be null.");
            Assert.AreEqual("NestedAssembly", mapping.assemblyName,
                "Should use closest assembly.");
        }
    }
}