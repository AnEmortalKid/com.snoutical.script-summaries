using Snoutical.ScriptSummaries.Editor.UI.Window;
using UnityEditor;
using UnityEngine;
using Snoutical.ScriptSummaries.Generation.API;

namespace Snoutical.ScriptSummaries.Editor.UI
{
    /// <summary>
    /// Adds a show documentation option to the default context menu on an inspector
    /// </summary>
    public class InspectorContextMenuHandler
    {
        [MenuItem("CONTEXT/MonoBehaviour/📜 Show Summary")]
        private static void ShowContextMenu(MenuCommand command)
        {
            MonoBehaviour script = command.context as MonoBehaviour;
            if (script == null)
            {
                return;
            }

            var summary = EditorSummaryAPI.GetEditorSummary(script);
            var scriptName =script.GetType().Name;
            string displayText = string.IsNullOrEmpty(summary) ? "No documentation available." : summary;
            ScriptSummaryPopupWindow.ShowWindow(scriptName, displayText);
        }
    }
}