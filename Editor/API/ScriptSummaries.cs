﻿using UnityEngine;

#if SCRIPT_SUMMARIES_INSTALLED
using Snoutical.ScriptSummaries.Generation.API;
#endif

namespace Snoutical.ScriptSummaries.API
{
    /// <summary>
    /// Publicly exposed API for retrieving summary's for scripts
    /// Intended so others can build editor tooling around this library
    /// </summary>
    public static class ScriptSummaries
    {
        /// <summary>
        /// Retrieves the summary docs for the given MonoBehaviour if they exist
        /// </summary>
        /// <param name="monoBehaviour">The MonoBehaviour to lookup a summary for</param>
        /// <returns>the summary documentation for the MonoBehaviour, or null if a summary is not defined
        /// OR the setup for this package failed</returns>
        public static string GetSummary(MonoBehaviour monoBehaviour)
        {
#if SCRIPT_SUMMARIES_INSTALLED
            return EditorSummaryAPI.GetEditorSummary(monoBehaviour);
#else
            // fallback for define missing
            return null;
#endif
        }

        /// <summary>
        /// Retrieves the summary docs for the script by its relative script path from the project root,
        /// e.g. Assets/Scripts/MyScript.cs
        /// </summary>
        /// <param name="scriptPath">A path to this script relative to the root, so Assets/Scripts/Blah</param>
        /// <returns>the summary documentation for the script, or null if a summary is not defined
        /// OR the setup for this package failed</returns>
        public static string GetSummary(string scriptPath)
        {
#if SCRIPT_SUMMARIES_INSTALLED
            return EditorSummaryAPI.GetEditorSummary(scriptPath);
#else
            // fallback for define missing
            return null;
#endif
        }
    }
}