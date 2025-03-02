using System.Collections.Generic;
using System.IO;

namespace Snoutical.ScriptSummaries.Generation.Database
{
    /// <summary>
    /// Loads data from our specialty .lookup file
    /// Intentionally made an instance for unit testing
    /// </summary>
    public class LookupLoader
    {
        /// <summary>
        /// Retrieves our lookup data from a given file
        /// </summary>
        /// <param name="lookupFilePath">the path to the file</param>
        /// <returns>all lookups within the file</returns>
        public List<LookupResult> GetLookups(string lookupFilePath)
        {
            List<LookupResult> results = new();

            foreach (var line in File.ReadLines(lookupFilePath))
            {
                var result = new LookupResult();

                // i wrote it so it should be ok right
                var parts = line.Split('=');

                result.scriptPath = parts[0].Trim();
                result.namespacedTypeKey = parts[1].Trim();

                var namespacedParts = result.namespacedTypeKey.Split(";");
                result.assembly = namespacedParts[0];
                result.typeName = namespacedParts[1];

                results.Add(result);
            }

            return results;
        }
    }
}