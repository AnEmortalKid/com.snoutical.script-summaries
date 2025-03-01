using Snoutical.ScriptSummaries.Tools.Generation;
using Snoutical.ScriptSummaries.Tools.Generation.API;
using UnityEditor;
using UnityEngine;

namespace Snoutical.ScriptSummaries.Editor.UI
{
    /// <summary>
    /// Adds a show documentation option to the default context menu
    /// </summary>
    public class ContextMenuHandler
    {
        [MenuItem("CONTEXT/MonoBehaviour/📜 Show Docs")]
        private static void ShowContextMenu(MenuCommand command)
        {
            MonoBehaviour script = command.context as MonoBehaviour;
            if (script == null)
            {
                return;
            }

            var summary = EditorSummaryAPI.GetEditorSummary(script);
            string displayText = string.IsNullOrEmpty(summary) ? "No documentation available." : summary;
            EditorUtility.DisplayDialog("📖 Script Documentation", displayText, "OK");
        }
    }
}