using System.IO;
using NUnit.Framework;
using Snoutical.ScriptSummaries.Generation.Database;

namespace Snoutical.ScriptSummaries.Editor.Test.Generation.Database
{
    [TestFixture]
    public class XmlDocumentationLoaderTest
    {
        private string tempXmlPath;
        
        [SetUp]
        public void Setup()
        {
            tempXmlPath = Path.Combine(Path.GetTempPath(), "TestSummaries.xml");
        }

        [TearDown]
        public void Cleanup()
        {
            if (File.Exists(tempXmlPath))
            {
                File.Delete(tempXmlPath);
            }
        }
        
        [Test]
        public void GetSummaries_LoadsDataCorrectly()
        {
            string xmlContent = @"
            <doc>
              <members>
                <member name=""T:Namespace.TestClass"">
                  <summary>Test summary for TestClass</summary>
                </member>
                <member name=""T:Namespace.SecondClass"">
                  <summary>Another summary for SecondClass</summary>
                </member>
              </members>
            </doc>";
            
            File.WriteAllText(tempXmlPath, xmlContent);

            var loader = new XmlDocumentationLoader();

            var summaries = loader.GetSummaries(tempXmlPath);
            Assert.AreEqual(2, summaries.Count);
            Assert.AreEqual("Test summary for TestClass", summaries["T:Namespace.TestClass"]);
            Assert.AreEqual("Another summary for SecondClass", summaries["T:Namespace.SecondClass"]);
        }
    }
}