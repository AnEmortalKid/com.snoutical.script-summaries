#if UNITY_EDITOR && SCRIPT_SUMMARIES_INSTALLED
using Snoutical.ScriptSummaries.Tools.Generation;
using UnityEditor;
using UnityEngine;

namespace Snoutical.ScriptSummaries.Editor.Tools.UI
{
    /// <summary>
    /// Attaches logic to the contextualPropertyMenu  for the editor if we have a summary, so
    /// monobehaviours can have their summary displayed with a context menu button
    /// </summary>
    public class MonoBehaviourSummaryContextMenu
    {
        [InitializeOnLoadMethod]
        private static void RunOnLoad()
        {
            Debug.Log("🔥 Unity just loaded or scripts recompiled!");
        }

        [InitializeOnLoadMethod]
        private static void AttachContextMenu()
        {
            EditorApplication.contextualPropertyMenu += (menu, property) =>
            {
                if (property.serializedObject.targetObject is MonoBehaviour monoBehaviour)
                {
                    AddDocumentationMenu(menu, monoBehaviour);
                }
            };
        }

        [MenuItem("CONTEXT/MonoBehaviour/📜 Show Documentation")]
        private static void ShowContextMenu(MenuCommand command)
        {
            MonoBehaviour script = command.context as MonoBehaviour;
            if (script == null) return;
            var summary = DocumentationLookup.GetSummary(script);
            string displayText = string.IsNullOrEmpty(summary) ? "No documentation available." : summary;
            EditorUtility.DisplayDialog("📖 Script Documentation", displayText, "OK");
        }

        private static void AddDocumentationMenu(GenericMenu menu, MonoBehaviour script)
        {
            var summary = DocumentationLookup.GetSummary(script);
            if (!string.IsNullOrEmpty(summary))
            {
                menu.AddItem(new GUIContent("📜 Show Documentation"), false,
                    () => { EditorUtility.DisplayDialog("📖 Script Documentation", summary, "OK"); });
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("❌ No Documentation Available"));
            }
        }
    }
}
#endif