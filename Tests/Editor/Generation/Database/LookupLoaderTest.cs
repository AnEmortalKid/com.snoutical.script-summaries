using System.IO;
using NUnit.Framework;
using Snoutical.ScriptSummaries.Generation.Database;
using UnityEditor;

namespace Snoutical.ScriptSummaries.Editor.Test.Generation.Database
{
    [TestFixture]
    public class LookupLoaderTest
    {
        [Test]
        public void GetLookups_ReturnsResults()
        {
            string tempFilePath = Path.Combine(Path.GetTempPath(), "lookup_test.lookup");

            try
            {
                string fileContent = @"
              Assets/Scripts/SingleLineScript.cs=Assembly-CSharp;T:SingleLineScript
              Assets/OtherScriptsDog/NotInScriptsBehavior.cs=Assembly-CSharp;T:NotInScriptsBehavior
            ".Trim();

                File.WriteAllText(tempFilePath, fileContent);

                var loader = new LookupLoader();
                var results = loader.GetLookups(tempFilePath);

                Assert.AreEqual(2, results.Count);

                var first = results[0];
                Assert.AreEqual("Assets/Scripts/SingleLineScript.cs", first.scriptPath);
                Assert.AreEqual("Assembly-CSharp;T:SingleLineScript", first.namespacedTypeKey);
                Assert.AreEqual("Assembly-CSharp", first.assembly);
                Assert.AreEqual("T:SingleLineScript", first.typeName);

                var second = results[1];
                Assert.AreEqual("Assets/OtherScriptsDog/NotInScriptsBehavior.cs", second.scriptPath);
                Assert.AreEqual("Assembly-CSharp;T:NotInScriptsBehavior", second.namespacedTypeKey);
                Assert.AreEqual("Assembly-CSharp", second.assembly);
                Assert.AreEqual("T:NotInScriptsBehavior", second.typeName);
            }
            finally
            {
                File.Delete(tempFilePath);
            }
        }
    }
}