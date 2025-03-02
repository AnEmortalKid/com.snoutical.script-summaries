using System.Collections.Generic;
using System.Xml.Linq;
using NUnit.Framework;
using Snoutical.ScriptSummaries.Generation.Generator;
using UnityEditor.VersionControl;

namespace Snoutical.ScriptSummaries.Editor.Test.Generation.Generator
{
    [TestFixture]
    public class DatabaseFilesGeneratorTest
    {
        [Test]
        public void GetLookupContent_ReturnsSummaryInfoCorrectly()
        {
            var summaryMappings = new List<SummaryMapping>
            {
                new()
                {
                    assemblyName = "TestAssembly", memberIdentifier = "T:Namespace.TestClass",
                    relativePath = "Assets/Scripts/Test.cs", summary = "Some Fake Summary"
                },
                new()
                {
                    assemblyName = "TestAssembly", memberIdentifier = "T:Namespace.SecondClass",
                    relativePath = "Assets/Scripts/SecondTest.cs", summary = "Some Second Summary"
                },
                new()
                {
                    assemblyName = "DifferentAssembly", memberIdentifier = "T:Namespace.DifferentClass",
                    relativePath = "Assets/Other/Different.cs", summary = "A secondary assembly"
                },
            };

            var fileGenerator = new DatabaseFilesGenerator();

            var lookupContents = fileGenerator.GetLookupContent(summaryMappings);

            // has 2 keys
            Assert.AreEqual(2, lookupContents.Count);

            var testAssemblyContent = lookupContents["TestAssembly"];
            Assert.IsTrue(testAssemblyContent.Contains("Assets/Scripts/Test.cs=TestAssembly;T:Namespace.TestClass"));
            Assert.IsTrue(
                testAssemblyContent.Contains("Assets/Scripts/SecondTest.cs=TestAssembly;T:Namespace.SecondClass"));

            var differentAssemblyContent = lookupContents["DifferentAssembly"];
            Assert.IsTrue(
                differentAssemblyContent.Contains(
                    "Assets/Other/Different.cs=DifferentAssembly;T:Namespace.DifferentClass"));
        }

        [Test]
        public void GetXmlContent_ReturnsSummaryInfoCorrectly()
        {
            var summaryMappings = new List<SummaryMapping>
            {
                new()
                {
                    assemblyName = "TestAssembly", memberIdentifier = "T:Namespace.TestClass",
                    relativePath = "Assets/Scripts/Test.cs", summary = "Some Fake Summary"
                },
                new()
                {
                    assemblyName = "TestAssembly", memberIdentifier = "T:Namespace.SecondClass",
                    relativePath = "Assets/Scripts/SecondTest.cs", summary = "Some Second Summary"
                },
                new()
                {
                    assemblyName = "DifferentAssembly", memberIdentifier = "T:Namespace.DifferentClass",
                    relativePath = "Assets/Other/Different.cs", summary = "A secondary assembly"
                },
            };

            var fileGenerator = new DatabaseFilesGenerator();

            var xmlContents = fileGenerator.GetXmlDocContent(summaryMappings);
            var expectTestXml = NormalizeXml(@"
            <doc>
              <members>
                <member name=""T:Namespace.TestClass"">
                  <summary>Some Fake Summary</summary>
                </member>
                <member name=""T:Namespace.SecondClass"">
                  <summary>Some Second Summary</summary>
                </member>
              </members>
            </doc>");

            var expectDifferentXml = NormalizeXml(@"
             <doc>
              <members>
                <member name=""T:Namespace.DifferentClass"">
                  <summary>A secondary assembly</summary>
                </member>
              </members>
            </doc>");

            var actualTestXml = NormalizeXml(xmlContents["TestAssembly"]);
            var actualDifferentXml = NormalizeXml(xmlContents["DifferentAssembly"]);
            Assert.AreEqual(expectTestXml, actualTestXml);
            Assert.AreEqual(expectDifferentXml, actualDifferentXml);
        }

        private string NormalizeXml(string xml)
        {
            return XDocument.Parse(xml).ToString(SaveOptions.DisableFormatting);
        }
    }
}