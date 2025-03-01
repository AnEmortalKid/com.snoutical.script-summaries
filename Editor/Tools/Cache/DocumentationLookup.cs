using System.Collections.Generic;

namespace Snoutical.ScriptSummaries.Tools.Generation
{
    /// <summary>
    /// Holds a reference to our summaries, this is just a wrapper around a dictionary
    /// since it felt yucky to me to access a public static dictionary somewhere else
    /// </summary>
    public static class DocumentationLookup
    {
        private static Dictionary<string, string> summariesByFileName = new Dictionary<string, string>();

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

        public static string GetSummary(string scriptName)
        {
            return summariesByFileName.TryGetValue(scriptName, out var summary) ? summary : null;
        }
    }
}