using System.Collections.Generic;
using System.IO;
using System.Xml;
using Snoutical.ScriptSummaries.Generation.Generator;
using UnityEngine;

namespace Snoutical.ScriptSummaries.Generation.Database
{
    /// <summary>
    /// Internal component that manages storing our summaries
    /// </summary>
    internal static class InternalSummaryDatabase
    {
        /// <summary>
        /// Store summaries by script relative to the project root so Assets/Scripts/MyScript.cs
        /// </summary>
        private static Dictionary<string, string> summariesByFileName = new();

        /// <summary>
        /// Store summaries by the Assembly;T:Namespace.ClassName key
        /// </summary>
        private static Dictionary<string, string> summariesByAssemblyTypeKey = new();

        private static bool isInitialized;

        public static void Initialize()
        {
            // TODO find a way to not call this every time we do a lookup
            LoadToMemory();
            isInitialized = true;
        }

        public static void ClearSummaries()
        {
            summariesByFileName.Clear();
        }

        /// <summary>
        /// Looks up a scripts stored summary by its relative script path
        /// </summary>
        /// <param name="scriptPath">A path to this script relative to the root, so Assets/Scripts/Blah</param>
        /// <returns>a summary if it exists or null</returns>
        internal static string GetSummaryByPathInternal(string scriptPath)
        {
            Initialize();
            return summariesByFileName.TryGetValue(scriptPath, out var summary) ? summary : null;
        }

        /// <summary>
        /// Looks up a scripts stored summary based on the given monobehaviour
        /// </summary>
        /// <param name="summaryKey">The lookup key (Assembly;T:ClassName)</param>
        /// <returns>a summary if it exists or null</returns>
        internal static string GetSummaryByKeyInternal(string summaryKey)
        {
            Initialize();
            return summariesByAssemblyTypeKey.TryGetValue(summaryKey, out var summary) ? summary : null;
        }

        private static void LoadToMemory()
        {
            if (!Directory.Exists(DocumentationGenerator.OutputDirectory))
            {
                return;
            }

            ClearSummaries();

            // Get each XMLs dictionary of Identifier to Summary
            var allXmls = Directory.GetFiles(DocumentationGenerator.OutputDirectory, "*.xml");

            // this dictionary's yucky atm we'll fix it tho
            var assemblyXmlMappings = new Dictionary<string, Dictionary<string, string>>();
            foreach (var xmlPath in allXmls)
            {
                var dictionary = FetchXmlEntries(xmlPath);
                var storedAssemblyName = Path.GetFileNameWithoutExtension(xmlPath);
                assemblyXmlMappings[storedAssemblyName] = dictionary;
            }

            // reverse each lookup
            var allLookups = Directory.GetFiles(DocumentationGenerator.OutputDirectory, "*.lookup");
            foreach (var lookupFile in allLookups)
            {
                foreach (var line in File.ReadLines(lookupFile))
                {
                    // i wrote it so it should be ok right
                    var parts = line.Split('=');

                    string scriptPath = parts[0].Trim();
                    string namespacedTypeKey = parts[1].Trim();

                    var namespacedParts = namespacedTypeKey.Split(";");
                    var assembly = namespacedParts[0];
                    var typeKey = namespacedParts[1];

                    // now lookup the typeKey in the right assembly
                    if (assemblyXmlMappings[assembly].TryGetValue(typeKey, out var summary))
                    {
                        summariesByFileName[scriptPath] = summary;
                        summariesByAssemblyTypeKey[namespacedTypeKey] = summary;
                    }
                }
            }
        }

        private static Dictionary<string, string> FetchXmlEntries(string xmlPath)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlPath);

            Dictionary<string, string> xmlSummaries = new();

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