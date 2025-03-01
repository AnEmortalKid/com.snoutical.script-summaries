#if UNITY_EDITOR && SCRIPT_SUMMARIES_INSTALLED
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using UnityEngine;
using UnityEditor;

namespace Snoutical.ScriptSummaries.Tools.Generation
{
    public static class DocumentationGenerator
    {
        /// <summary>
        /// We'll store an XML file for each assembly here with all the summaries for each assembly in one XML
        /// </summary>
        public static readonly string OutputDirectory = "Library/ScriptSummaries/";

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

            var xmlBuilders = GetXmlsByAssembly(scriptPaths);

            // write them out
            foreach (var entry in xmlBuilders)
            {
                string outputPath = Path.Combine(OutputDirectory, $"{entry.Key}.xml");
                File.WriteAllText(outputPath, entry.Value.ToString());
            }

            AssetDatabase.Refresh();
            Debug.Log($"XML documentation generated in {OutputDirectory}");
        }

        private static Dictionary<string, StringBuilder> GetXmlsByAssembly(string[] scriptPaths)
        {
            // store the content of each XML as we process files
            var xmlDocBuilders = new Dictionary<string, StringBuilder>();
            foreach (var script in scriptPaths)
            {
                try
                {
                    string fileContent = File.ReadAllText(script);
                    var syntaxTree = CSharpSyntaxTree.ParseText(fileContent);
                    var root = syntaxTree.GetRoot();

                    // Get the assembly name the script belongs to
                    string assemblyName = GetAssemblyNameForScript(script);

                    if (string.IsNullOrEmpty(assemblyName))
                    {
                        Debug.LogWarning($"Could not compute assembly for path: {script}");
                        continue;
                    }

                    if (!xmlDocBuilders.ContainsKey(assemblyName))
                    {
                        xmlDocBuilders[assemblyName] = new StringBuilder();
                        xmlDocBuilders[assemblyName].AppendLine("<doc>");
                    }

                    var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
                    foreach (var classNode in classes)
                    {
                        var summary = GetXmlSummary(classNode);
                        if (!string.IsNullOrEmpty(summary))
                        {
                            xmlDocBuilders[assemblyName].AppendLine($"  <class name=\"{classNode.Identifier.Text}\">");
                            xmlDocBuilders[assemblyName].AppendLine($"    <summary>{summary}</summary>");
                            xmlDocBuilders[assemblyName].AppendLine("  </class>");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error processing {script}: {ex.Message}");
                }
            }

            // close the xml document
            foreach (var entry in xmlDocBuilders)
            {
                entry.Value.AppendLine("</doc>");
            }

            return xmlDocBuilders;
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

            return "ScriptSummaries_Fallback";
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
#endif