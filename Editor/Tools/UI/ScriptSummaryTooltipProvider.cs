#if UNITY_EDITOR && SCRIPT_SUMMARIES_INSTALLED
using Snoutical.ScriptSummaries.Tools.Generation;
using UnityEditor;
using UnityEngine;

namespace Snoutical.ScriptSummaries.Editor.Tools.UI
{
    [InitializeOnLoad]
    public class ScriptSummaryTooltipProvider : MonoBehaviour
    {
        static ScriptSummaryTooltipProvider()
        {
            EditorApplication.projectWindowItemOnGUI += ShowTooltipForScripts;
        }

        private static void ShowTooltipForScripts(string guid, Rect selectionRect)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            // Only check .cs files
            if (path.EndsWith(".cs"))
            {
                string summary = DocumentationLookup.GetSummaryByPath(path);

                if (!string.IsNullOrEmpty(summary) && selectionRect.Contains(Event.current.mousePosition))
                {
                    // blank text to not override script name but use summary for hover
                    GUIContent content = new GUIContent("", summary);
                    GUI.Label(selectionRect, content);
                }
            }
        }
    }
}
#endif