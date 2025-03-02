using System.Collections.Generic;
using System.Xml;

namespace Snoutical.ScriptSummaries.Generation.Database
{
    /// <summary>
    /// Loads data from our specialty .xml file
    /// Intentionally made an instance for unit testing
    /// </summary>
    public class XmlDocumentationLoader
    {
        /// <summary>
        /// Retrieves a mapping of fully qualified type name to a summary from the given file
        /// </summary>
        /// <param name="xmlPath">the file path to our specialty xml</param>
        /// <returns>a mapping of a T:Namespace.ClassName to the summary for that script</returns>
        public Dictionary<string, string> GetSummaries(string xmlPath)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlPath);

            Dictionary<string, string> xmlSummaries = new();

            // its very possible at some point we may have the full member docs generated
            foreach (XmlNode memberNode in xmlDoc.SelectNodes("/doc/members/member"))
            {
                // It should exist in form T:Namespace?.ClassName
                var memberIdentifier = memberNode.Attributes["name"].Value;
                var summaryNode = memberNode.SelectSingleNode("summary");

                if (memberIdentifier != null && summaryNode != null)
                {
                    xmlSummaries[memberIdentifier] = summaryNode.InnerText.Trim();
                }
            }

            return xmlSummaries;
        }
    }
}