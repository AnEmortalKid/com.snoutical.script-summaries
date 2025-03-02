using System.Collections.Generic;
using System.Text;

namespace Snoutical.ScriptSummaries.Generation.Generator
{
    /// <summary>
    /// Responsible for generating the contents for our database files
    /// Intentionally made an instance for unit testing
    /// </summary>
    public class DatabaseFilesGenerator
    {
        /// <summary>
        /// Generates the data for our lookup files, keyed by the assembly name.
        /// Each entry in the dictionary will have a value that is the full contents of the file.
        /// </summary>
        /// <param name="summaryMappings">mappings containing information for a script and its summary</param>
        /// <returns>a dictionary of assembly name to contents a lookup file for that assembly should have</returns>
        public Dictionary<string, string> GetLookupContent(List<SummaryMapping> summaryMappings)
        {
            // store the content of each lookup file as we go through all the data
            var lookupBuilders = new Dictionary<string, StringBuilder>();

            foreach (var mapping in summaryMappings)
            {
                var assemblyName = mapping.assemblyName;

                if (!lookupBuilders.ContainsKey(assemblyName))
                {
                    lookupBuilders[assemblyName] = new StringBuilder();
                }

                var lookupKey = mapping.assemblyName + ";" + mapping.memberIdentifier;
                lookupBuilders[assemblyName].AppendLine($"{mapping.relativePath}={lookupKey}");
            }

            // we've processed all the files for all assemblies, finalize the content
            var lookupContent = new Dictionary<string, string>();
            foreach (var entry in lookupBuilders)
            {
                var content = entry.Value.ToString();
                lookupContent.Add(entry.Key, content);
            }

            return lookupContent;
        }

        /// <summary>
        /// Generates the data for our xml documentation files, keyed by the assembly name.
        /// Each entry in the dictionary will have a value that is the full contents of the file.
        /// </summary>
        /// <param name="summaryMappings">mappings containing information for a script and its summary</param>
        /// <returns>a dictionary of assembly name to contents a documentation file for that assembly should have</returns>
        public Dictionary<string, string> GetXmlDocContent(List<SummaryMapping> summaryMappings)
        {
            // store the content of each XML as we process files
            var xmlDocBuilders = new Dictionary<string, StringBuilder>();

            foreach (var mapping in summaryMappings)
            {
                var assemblyName = mapping.assemblyName;
                if (!xmlDocBuilders.ContainsKey(assemblyName))
                {
                    xmlDocBuilders[assemblyName] = new StringBuilder();
                    xmlDocBuilders[assemblyName].AppendLine("<doc>");
                    xmlDocBuilders[assemblyName].AppendLine("  <members>");
                }

                xmlDocBuilders[assemblyName].AppendLine($"    <member name=\"{mapping.memberIdentifier}\">");
                xmlDocBuilders[assemblyName].AppendLine($"      <summary>{mapping.summary}</summary>");
                xmlDocBuilders[assemblyName].AppendLine("    </member>");
            }

            var xmlContent = new Dictionary<string, string>();
            // close xmls and then store the entries 
            foreach (var entry in xmlDocBuilders)
            {
                entry.Value.AppendLine("  </members>");
                entry.Value.AppendLine("</doc>");

                xmlContent.Add(entry.Key, entry.Value.ToString());
            }

            return xmlContent;
        }
    }
}