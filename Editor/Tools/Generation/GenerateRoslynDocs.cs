#if UNITY_EDITOR && SCRIPT_SUMMARIES_INSTALLED
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using UnityEditor;
using UnityEngine;

namespace Snoutical.ScriptSummaries.Tools.Generation
{
    public class GenerateRoslynDocs
    {
        private static readonly string outputPath = "Assets/Roslyn.xml";
        public static Dictionary<string, string> SummaryLookup = new Dictionary<string, string>();

        [MenuItem("Tools/Generate Roslyn Docs")]
        public static void GenerateDocs()
        {
            Dictionary<string, string> docEntries = new Dictionary<string, string>();

            string[] scriptPaths = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories)
                .Where(path =>
                    path.StartsWith(Path.Combine(Application.dataPath, "Scripts"), StringComparison.OrdinalIgnoreCase))
                .ToArray();

          DocumentationGenerator.GenerateScriptSummaries(scriptPaths);
        }


        private static void LoadXmlToMemory()
        {
            if (!File.Exists(outputPath)) return;

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(outputPath);

            foreach (XmlNode member in xmlDoc.SelectNodes("/doc/members/member"))
            {
                string name = member.Attributes["name"]?.Value.Replace("T:", "");
                XmlNode summaryNode = member.SelectSingleNode("summary");
                if (name != null && summaryNode != null)
                {
                    SummaryLookup[name] = summaryNode.InnerText.Trim();
                }
            }
        }
    }
}
#endif