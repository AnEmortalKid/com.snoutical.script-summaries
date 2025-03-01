using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Snoutical.ScriptSummaries.Generation.Constants;
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

        public static void RunRegeneration()
        {
            string[] userDefined = GetScanPaths();
            // normalize paths 
            string assetsBasePath = NormalizePath(Application.dataPath);

            // do everything if there's nothing specified to filter
            string[] allScripts = Directory.GetFiles(assetsBasePath, "*.cs", SearchOption.AllDirectories);

            // TODO support excluding later
            string[] filteredPaths = allScripts
                .Where(path =>
                {
                    var normalizedPath = NormalizePath(path);

                    // compare normalized paths, scanDir should also come back normalized
                    return userDefined.Any(
                        scanDir => normalizedPath.StartsWith(
                            NormalizePath(Path.Combine(assetsBasePath, scanDir)), StringComparison.OrdinalIgnoreCase));
                })
                .ToArray();

            GenerateScriptSummaries(filteredPaths);
        }

        private static string NormalizePath(string rawPath)
        {
            return rawPath.Replace("\\", "/");
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
                    .Select(path => NormalizePath(path.Replace("Assets/", "")))
                    .ToArray();
        }

        /// <summary>
        /// Generates the XML summary for each script in the given path.
        /// Each script has its summary stored in an XML file based on its closest assembly.
        /// If an assembly cannot be found, the summary lands in a fallback XML file
        /// </summary>
        /// <param name="scriptPaths">the array of paths to csharp files (.cs) to generate docs for</param>
        public static void GenerateScriptSummaries(string[] scriptPaths)
        {
            if (!Directory.Exists(OutputDirectory))
            {
                Directory.CreateDirectory(OutputDirectory);
            }
            // TODO do we need to clean if assemblies or whatever change
            // probably

            // Potentially eventually store the full XML and show it somewhere
            var summaries = GenerateMappings(scriptPaths);
            GenerateXmls(summaries);
            GenerateLookupFiles(summaries);

            AssetDatabase.Refresh();
            Debug.Log($"XML documentation generated in {OutputDirectory}");
        }

        private static void GenerateXmls(List<SummaryMapping> summaryMappings)
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

            // close the xmls before we write them
            foreach (var entry in xmlDocBuilders)
            {
                entry.Value.AppendLine("  </members>");
                entry.Value.AppendLine("</doc>");
            }

            foreach (var entry in xmlDocBuilders)
            {
                string outputPath = Path.Combine(OutputDirectory, $"{entry.Key}.xml");
                File.WriteAllText(outputPath, entry.Value.ToString());
            }
        }

        private static void GenerateLookupFiles(List<SummaryMapping> summaryMappings)
        {
            // store the content of each lookup file as we parse through
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

            foreach (var entry in lookupBuilders)
            {
                string outputPath = Path.Combine(OutputDirectory, $"{entry.Key}.lookup");
                File.WriteAllText(outputPath, entry.Value.ToString());
            }
        }

        private static List<SummaryMapping> GenerateMappings(string[] scriptPaths)
        {
            List<SummaryMapping> summaryMappings = new();

            foreach (var script in scriptPaths)
            {
                try
                {
                    string fileContent = File.ReadAllText(script);
                    var syntaxTree = CSharpSyntaxTree.ParseText(fileContent);
                    var root = syntaxTree.GetRoot();

                    SummaryMapping currentMapping = new();

                    // Get the assembly name the script belongs to
                    string assemblyName = GetAssemblyNameForScript(script);

                    if (string.IsNullOrEmpty(assemblyName))
                    {
                        Debug.LogWarning($"Could not compute assembly for path: {script}");
                        continue;
                    }

                    summaryMappings.Add(currentMapping);
                    currentMapping.assemblyName = assemblyName;

                    var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
                    foreach (var classNode in classes)
                    {
                        var summary = GetXmlSummary(classNode);
                        if (!string.IsNullOrEmpty(summary))
                        {
                            // Normalize path so we can look it up later
                            string relativeScriptPath = script.Replace("\\", "/");
                            // store path with Assets/ItsPath since thats what we'll look up
                            relativeScriptPath = relativeScriptPath.Replace(Application.dataPath + "/", "Assets/");
                            currentMapping.relativePath = relativeScriptPath;

                            var namespaceNode = classNode.Ancestors().OfType<NamespaceDeclarationSyntax>()
                                .FirstOrDefault();
                            string namespaceName =
                                namespaceNode != null ? namespaceNode.Name.ToString() : ""; // Empty if no namespace
                            string className = classNode.Identifier.Text;

                            // Include namespace if available
                            var memberId = !string.IsNullOrEmpty(namespaceName)
                                ? $"T:{namespaceName}.{className}"
                                : $"T:{className}";
                            currentMapping.memberIdentifier = memberId;

                            currentMapping.summary = summary;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error processing {script}: {ex.Message}");
                }
            }

            return summaryMappings;
        }

        private static string GetAssemblyNameForScript(string scriptPath)
        {
            // Look for the closest .asmdef file in parent directories
            string directory = Path.GetDirectoryName(scriptPath);
            while (!string.IsNullOrEmpty(directory))
            {
                var asmdefFiles = Directory.GetFiles(directory, "*.asmdef", SearchOption.TopDirectoryOnly);
                if (asmdefFiles.Length > 0)
                {
                    string asmdefContent = File.ReadAllText(asmdefFiles[0]);
                    var assemblyDefinition = JsonUtility.FromJson<AssemblyDefinition>(asmdefContent);
                    return assemblyDefinition.name;
                }

                directory = Directory.GetParent(directory)?.FullName;
            }

            return GenerationConstants.FallbackAssemblyName;
        }

        /// <summary>
        /// Either gets the XML summary or an empty string
        /// </summary>
        private static string GetXmlSummary(SyntaxNode node)
        {
            var trivia = node.GetLeadingTrivia()
                .Select(t => t.GetStructure())
                .OfType<DocumentationCommentTriviaSyntax>()
                .FirstOrDefault();

            if (trivia == null)
            {
                return string.Empty;
            }

            var summaryNode = trivia.Content.OfType<XmlElementSyntax>()
                .FirstOrDefault(e => e.StartTag.Name.LocalName.Text == "summary");

            if (summaryNode != null)
            {
                var rawSummary = summaryNode.Content.ToString().Trim();
                return CleanXmlSummary(rawSummary);
            }

            return string.Empty;
        }

        /// <summary>
        /// Cleans extracted XML summary by removing `///` prefixes and extra spacing.
        /// </summary>
        private static string CleanXmlSummary(string rawComment)
        {
            if (string.IsNullOrWhiteSpace(rawComment))
            {
                return string.Empty;
            }

            // clean all the ///
            string extracted = Regex.Replace(rawComment, @"^\s*///\s?", "", RegexOptions.Multiline);

            return extracted.Trim();
        }
    }
}