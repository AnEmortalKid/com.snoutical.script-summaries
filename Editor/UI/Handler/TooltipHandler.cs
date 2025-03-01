using Snoutical.ScriptSummaries.Tools.Generation.API;
using UnityEditor;
using UnityEngine;

namespace Snoutical.ScriptSummaries.Editor.UI
{
    /// <summary>
    /// Adds a tooltip to scripts in the assets view with a small preview summary
    /// </summary>
    [InitializeOnLoad]
    public class TooltipHandler
    {
        static TooltipHandler()
        {
            EditorApplication.projectWindowItemOnGUI += ShowTooltipForScripts;
        }

        private static void ShowTooltipForScripts(string guid, Rect selectionRect)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            // Only check .cs files
            if (path.EndsWith(".cs"))
            {
                string summary = EditorSummaryAPI.GetEditorSummary(path);

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