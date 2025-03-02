using Snoutical.ScriptSummaries.Editor.UI.Util;
using UnityEditor;
using UnityEngine;
using Snoutical.ScriptSummaries.Generation.API;
using Snoutical.ScriptSummaries.Setup.Settings;

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
                    var trimLines = GetTrimMaxLines();
                    if (trimLines > 0)
                    {
                        summary = SummaryUtils.TrimSummary(summary, trimLines);
                    }

                    // blank text to not override script name but use summary for hover
                    GUIContent content = new GUIContent("", summary);
                    GUI.Label(selectionRect, content);
                }
            }
        }

        private static int GetTrimMaxLines()
        {
            var settings = ScriptSummariesSettingsUtility.FetchSettings();
            if (settings != null)
            {
                return settings.TooltipLineLength;
            }

            return 0;
        }
    }
}