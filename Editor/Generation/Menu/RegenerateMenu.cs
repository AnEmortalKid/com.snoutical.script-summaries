using Snoutical.ScriptSummaries.Editor.Generation;
using Snoutical.ScriptSummaries.Generation.Generator;
using UnityEditor;

namespace Snoutical.ScriptSummaries.Generation.Menu
{
    public class RegenerateMenu
    {
        /// <summary>
        ///  Manually regenerate the docs based on settings
        /// </summary>
        [MenuItem("Tools/Script Summaries/Regenerate Docs")]
        public static void RegenerateFiles()
        {
            ScriptSummariesManager.RegenerateAndReload(true);
        }
    }
}