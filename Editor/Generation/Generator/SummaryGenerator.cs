using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Snoutical.ScriptSummaries.Editor.Common.Logger;
using Snoutical.ScriptSummaries.Editor.Generation.Util;
using Snoutical.ScriptSummaries.Generation.Constants;
using UnityEngine;

namespace Snoutical.ScriptSummaries.Generation.Generator
{
    /// <summary>
    /// Generates SummaryMapping
    /// Intentionally made an instance for unit testing
    /// </summary>
    public class SummaryGenerator
    {
        /// <summary>
        /// Stores the value of the dataPath, usable for testing
        /// </summary>
        private string rootDataPath;

        public SummaryGenerator(string dataPath = null)
        {
            rootDataPath = PathUtils.NormalizePath(dataPath ?? Application.dataPath);
        }

        /// <summary>
        /// Generates a SummaryMapping for the given script if the script has a summary
        /// </summary>
        /// <param name="scriptPath">the path to the script to generate a summary for</param>
        /// <returns>a summary mapping or null if one cannot be generated</returns>
        public SummaryMapping GenerateMapping(string scriptPath)
        {
            try
            {
                string fileContent = File.ReadAllText(scriptPath);
                var syntaxTree = CSharpSyntaxTree.ParseText(fileContent);
                var root = syntaxTree.GetRoot();

                SummaryMapping currentMapping = new();

                // Get the assembly name the script belongs to
                string assemblyName = GetAssemblyNameForScript(scriptPath);

                if (string.IsNullOrEmpty(assemblyName))
                {
                    ScriptSummariesLogger.LogWarning($"Could not compute assembly for path: {scriptPath}");
                    return null;
                }

                currentMapping.assemblyName = assemblyName;

                var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
                foreach (var classNode in classes)
                {
                    var summary = GetXmlSummary(classNode);
                    if (!string.IsNullOrEmpty(summary))
                    {
                        // Normalize path so we can look it up later
                        string relativeScriptPath = PathUtils.NormalizePath(scriptPath);
                        // store path with Assets/ItsPath since that's what we'll look up
                        relativeScriptPath = relativeScriptPath.Replace(rootDataPath + "/", "Assets/");
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

                        // only if we have a summary put us in the list
                        return currentMapping;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                ScriptSummariesLogger.LogError($"Error processing {scriptPath}: {ex.Message}");
            }

            return null;
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