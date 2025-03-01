#if UNITY_EDITOR && SCRIPT_SUMMARIES_INSTALLED
using UnityEditor;

namespace Snoutical.ScriptSummaries.Tools.Generation.Menu
{
    public class RegenerateMenu
    {
        /// <summary>
        ///  Manually regenerate the docs based on settings
        /// </summary>
        [MenuItem("Tools/Script Summaries/Regenerate Docs")]
        public static void RegenerateFiles()
        {
            DocumentationGenerator.RunRegeneration();
        }
    }
}
#endif