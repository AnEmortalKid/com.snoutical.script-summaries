using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Snoutical.ScriptSummaries.Editor.Common.Logger;
using Snoutical.ScriptSummaries.Editor.Generation.Util;
using Snoutical.ScriptSummaries.Setup.Settings;
using UnityEditor;
using UnityEngine;

namespace Snoutical.ScriptSummaries.Generation.Generator
{
    public static class DocumentationGenerator
    {
        /// <summary>
        /// We'll store an XML file for each assembly here with all the summaries for each assembly in one XML
        /// </summary>
        public static readonly string OutputDirectory = "Library/ScriptSummaries/";

        /// <summary>
        /// A helper object to generate summaries for scripts and all metadata needed for retrieval
        /// </summary>
        private static SummaryGenerator summaryGenerator = new();

        /// <summary>
        /// A helper object to generate the contents of our xml and lookup files
        /// </summary>
        private static DatabaseFilesGenerator databaseFilesGenerator = new();

        /// <summary>
        /// Regenerates all the documentation objects
        /// <param name="isManual">whether invocation is manual or automatic</param>
        /// </summary>
        public static void RunRegeneration(bool isManual = false)
        {
            CleanLibrary();

            string[] userDefined = GetScanPaths();
            if (userDefined.Length == 0)
            {
                // only force if it was manually asked for
                ScriptSummariesLogger.LogWarning("No Scan Directories defined, no docs will be generated.", isManual);
                return;
            }

            // normalize paths 
            string assetsBasePath = PathUtils.NormalizePath(Application.dataPath);

            // do everything if there's nothing specified to filter
            string[] allScripts = Directory.GetFiles(assetsBasePath, "*.cs", SearchOption.AllDirectories);

            // TODO support excluding later
            string[] filteredPaths = allScripts
                .Where(path =>
                {
                    var normalizedPath = PathUtils.NormalizePath(path);

                    // compare normalized paths, scanDir should also come back normalized
                    return userDefined.Any(
                        scanDir => normalizedPath.StartsWith(
                            PathUtils.NormalizePath(Path.Combine(assetsBasePath, scanDir)),
                            StringComparison.OrdinalIgnoreCase));
                })
                .ToArray();

            GenerateScriptSummaries(filteredPaths);
        }

        /// <summary>
        /// Gets paths to scan relative to Assets
        /// </summary>
        /// <returns>names of scan folders in relation to the Assets/ dir</returns>
        private static string[] GetScanPaths()
        {
            var settings = ScriptSummariesSettingsUtility.FetchSettings();
            if (settings == null)
            {
                return Array.Empty<string>();
            }

            // convert paths to strings
            var scanDirs = settings.ScanDirectories;

            return
                // safeguard against bad entries
                scanDirs.Where(dir => dir != null)
                    // ensure its a path not an object since we do any asset pick
                    .Select(AssetDatabase.GetAssetPath)
                    // Only care about folders
                    .Where(AssetDatabase.IsValidFolder)
                    // Drop Assets/ from the path so we can then match properly by using the Application.dataPath
                    .Select(path => PathUtils.NormalizePath(path.Replace("Assets/", "")))
                    .ToArray();
        }

        private static void CleanLibrary()
        {
            if (!Directory.Exists(OutputDirectory))
            {
                return;
            }

            string[] oldFiles = Directory.GetFiles(OutputDirectory, "*.xml")
                .Concat(Directory.GetFiles(OutputDirectory, "*.lookup"))
                .ToArray();

            foreach (string file in oldFiles)
            {
                File.Delete(file);
            }
        }

        /// <summary>
        /// Generates the XML summary for each script in the given path.
        /// Each script has its summary stored in an XML file based on its closest assembly.
        /// If an assembly cannot be found, the summary lands in a fallback XML file
        /// </summary>
        /// <param name="scriptPaths">the array of paths to csharp files (.cs) to generate docs for</param>
        private static void GenerateScriptSummaries(string[] scriptPaths)
        {
            if (!Directory.Exists(OutputDirectory))
            {
                Directory.CreateDirectory(OutputDirectory);
            }

            // Potentially eventually store the full XML and show it somewhere
            var summaries = GenerateMappings(scriptPaths);
            GenerateXmls(summaries);
            GenerateLookupFiles(summaries);

            AssetDatabase.Refresh();
            ScriptSummariesLogger.Log($"XML documentation generated in {OutputDirectory}");
        }

        private static void GenerateXmls(List<SummaryMapping> summaryMappings)
        {
            var xmlContents = databaseFilesGenerator.GetXmlDocContent(summaryMappings);

            foreach (var entry in xmlContents)
            {
                string outputPath = Path.Combine(OutputDirectory, $"{entry.Key}.xml");
                File.WriteAllText(outputPath, entry.Value);
            }
        }

        private static void GenerateLookupFiles(List<SummaryMapping> summaryMappings)
        {
            var lookupContents = databaseFilesGenerator.GetLookupContent(summaryMappings);

            foreach (var entry in lookupContents)
            {
                string outputPath = Path.Combine(OutputDirectory, $"{entry.Key}.lookup");
                File.WriteAllText(outputPath, entry.Value);
            }
        }

        private static List<SummaryMapping> GenerateMappings(string[] scriptPaths)
        {
            List<SummaryMapping> summaryMappings = new();

            foreach (var scriptPath in scriptPaths)
            {
                var summaryMapping = summaryGenerator.GenerateMapping(scriptPath);
                if (summaryMapping != null)
                {
                    summaryMappings.Add(summaryMapping);
                }
            }

            return summaryMappings;
        }
    }
}