using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;

namespace Snoutical.ScriptSummaries.Tools.Generation
{
    // TODO make this an InternalDatabase and have a Runtime that can fetch these summaries
    // so a user could build the monobehavior inspector with tooltip but i dont wanna force that
    // on everyone
    /// <summary>
    /// Holds a reference to our summaries, this is just a wrapper around a dictionary
    /// since it felt yucky to me to access a public static dictionary somewhere else
    /// </summary>
    public static class DocumentationLookup
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

        public static int GetSummaryCount()
        {
            return summariesByFileName.Count;
        }

        public static void StoreSummary(string fileName, string summary)
        {
            summariesByFileName[fileName] = summary;
        }

        /// <summary>
        /// Looks up a scripts stored summary by its relative script path
        /// </summary>
        /// <param name="scriptPath">A path to this script relative to the root, so Assets/Scripts/Blah</param>
        /// <returns></returns>
        public static string GetSummaryByPath(string scriptPath)
        {
            Initialize();
            return summariesByFileName.TryGetValue(scriptPath, out var summary) ? summary : null;
        }

        public static string GetSummary(MonoBehaviour monoBehaviour)
        {
            Initialize();

            System.Type type = monoBehaviour.GetType();
            // get namespaced class
            string className = type.FullName;
            string assemblyName = type.Assembly.GetName().Name;

            if (string.IsNullOrEmpty(assemblyName))
            {
                assemblyName = GenerationConstants.FallbackAssemblyName;
            }

            string summaryKey = $"{assemblyName};T:{className}";
            if (summariesByAssemblyTypeKey.TryGetValue(summaryKey, out var summary))
            {
                return summary;
            }

            ;

            return null;
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