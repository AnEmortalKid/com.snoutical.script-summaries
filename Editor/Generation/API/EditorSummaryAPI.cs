using Snoutical.ScriptSummaries.Generation.Constants;
using Snoutical.ScriptSummaries.Generation.Database;
using UnityEngine;

namespace Snoutical.ScriptSummaries.Generation.API
{
    /// <summary>
    /// API over the InternalSummaryDatabase so other assemblies in this package can fetch summaries
    /// </summary>
    public static class EditorSummaryAPI
    {
        /// <summary>
        /// Looks up a scripts stored summary by its relative script path from the project root, e.g. Assets/Scripts/MyScript.cs
        /// </summary>
        /// <param name="scriptPath">A path to this script relative to the root, so Assets/Scripts/Blah</param>
        /// <returns>a summary if it exists or null</returns>
        public static string GetEditorSummary(string scriptPath)
        {
#if !SCRIPT_SUMMARIES_INSTALLED
        Debug.LogWarning("⚠️ Script Summaries system is not installed. Returning null.");
        return null;
#endif

            return InternalSummaryDatabase.GetSummaryByPathInternal(scriptPath);
        }

        /// <summary>
        /// Retrieves a script summary from the internal database for the given MonoBehaviour.
        /// </summary>
        /// <param name="monoBehaviour">The MonoBehaviour to lookup a summary for</param>
        /// <returns>Summary text or null if not found.</returns>
        public static string GetEditorSummary(MonoBehaviour monoBehaviour)
        {
#if !SCRIPT_SUMMARIES_INSTALLED
        Debug.LogWarning("⚠️ Script Summaries system is not installed. Returning null.");
        return null;
#endif

            System.Type type = monoBehaviour.GetType();
            // get namespaced class
            string className = type.FullName;
            string assemblyName = type.Assembly.GetName().Name;

            if (string.IsNullOrEmpty(assemblyName))
            {
                assemblyName = GenerationConstants.FallbackAssemblyName;
            }

            string summaryKey = $"{assemblyName};T:{className}";
            return InternalSummaryDatabase.GetSummaryByKeyInternal(summaryKey);
        }
    }
}