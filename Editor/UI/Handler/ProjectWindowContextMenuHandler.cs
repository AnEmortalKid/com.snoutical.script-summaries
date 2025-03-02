using System.IO;
using Snoutical.ScriptSummaries.Editor.UI.Window;
using Snoutical.ScriptSummaries.Generation.API;
using UnityEditor;

namespace Snoutical.ScriptSummaries.Editor.UI
{
    /// <summary>
    /// Attaches a menu that shows our stylized popup to the Project Window
    /// </summary>
    public class ProjectWindowContextMenuHandler
    {
        // yeah its over 9000
        [MenuItem("Assets/📜 Show Summary", false, 9001)]
        private static void ShowScriptDocumentation()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);

            // the validate function should ensure we only have this menu when there is a summary
            string summary = EditorSummaryAPI.GetEditorSummary(path);
            var scriptName = Path.GetFileNameWithoutExtension(path);
            ScriptSummaryPopupWindow.ShowWindow(scriptName, summary);
        }

        [MenuItem("Assets/📜 Show Summary", true)]
        private static bool ValidateShowScriptDocumentation()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            if (!path.EndsWith(".cs"))
            {
                return false;
            }

            string summary = EditorSummaryAPI.GetEditorSummary(path);
            return summary != null;
        }
    }
}