using System;
using System.Collections.Generic;
using System.IO;
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

        static InternalSummaryDatabase()
        {
            // Unity will reload the App domain so reinitialize from files
            // A doc regeneration will call re-initialize when its appropriate
            // Its possible re-initialize gets called twice when domain reloading
            // so if that starts being a problem we can optimize
            ReInitialize();
        }

        /// <summary>
        /// A helper object for looking up data from the .lookup file
        /// </summary>
        private static LookupLoader lookupLoader = new();

        /// <summary>
        /// A helper object for looking up data from the .xml file
        /// </summary>
        private static XmlDocumentationLoader xmlDocLoader = new();

        /// <summary>
        /// Re-Initializes the Database from files to memory
        /// </summary>
        internal static void ReInitialize()
        {
            isInitialized = false;
            LoadToMemory();
            isInitialized = true;
        }

        private static void ClearSummaries()
        {
            summariesByFileName.Clear();
            summariesByAssemblyTypeKey.Clear();
        }

        /// <summary>
        /// Looks up a scripts stored summary by its relative script path
        /// </summary>
        /// <param name="scriptPath">A path to this script relative to the root, so Assets/Scripts/Blah</param>
        /// <returns>a summary if it exists or null</returns>
        internal static string GetSummaryByPathInternal(string scriptPath)
        {
            if (!isInitialized)
            {
                return null;
            }

            return summariesByFileName.TryGetValue(scriptPath, out var summary) ? summary : null;
        }

        /// <summary>
        /// Looks up a scripts stored summary based on the given monobehaviour
        /// </summary>
        /// <param name="summaryKey">The lookup key (Assembly;T:ClassName)</param>
        /// <returns>a summary if it exists or null</returns>
        internal static string GetSummaryByKeyInternal(string summaryKey)
        {
            if (!isInitialized)
            {
                return null;
            }

            return summariesByAssemblyTypeKey.TryGetValue(summaryKey, out var summary) ? summary : null;
        }

        private static void LoadToMemory()
        {
            // The generation process creates the directory and the files
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
                var dictionary = xmlDocLoader.GetSummaries(xmlPath);
                var storedAssemblyName = Path.GetFileNameWithoutExtension(xmlPath);
                assemblyXmlMappings[storedAssemblyName] = dictionary;
            }

            // reverse each lookup
            var allLookups = Directory.GetFiles(DocumentationGenerator.OutputDirectory, "*.lookup");
            foreach (var lookupFile in allLookups)
            {
                foreach (var result in lookupLoader.GetLookups(lookupFile))
                {
                    var assembly = result.assembly;
                    var typeKey = result.typeName;
                    var scriptPath = result.scriptPath;
                    var namespacedTypeKey = result.namespacedTypeKey;

                    // now lookup the typeKey in the right assembly
                    if (assemblyXmlMappings[assembly].TryGetValue(typeKey, out var summary))
                    {
                        summariesByFileName[scriptPath] = summary;
                        summariesByAssemblyTypeKey[namespacedTypeKey] = summary;
                    }
                }
            }
        }
    }
}