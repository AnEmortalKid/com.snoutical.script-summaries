using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Snoutical.ScriptSummaries.Editor.UI.Window
{
    /// <summary>
    /// Window to display as a popup for the context menu
    /// </summary>
    public class ScriptSummaryPopupWindow : EditorWindow
    {
        private string displayText;
        private string scriptName;

        public static void ShowWindow(string scriptName, string content)
        {
            ScriptSummaryPopupWindow window = CreateInstance<ScriptSummaryPopupWindow>();
            window.titleContent = new GUIContent("📜 Script Summary");
            window.displayText = content;
            window.scriptName = scriptName;
            window.minSize = new Vector2(400, 300);
            window.ShowUtility();
        }

        private void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            root.style.paddingTop = 10;
            root.style.paddingLeft = 10;
            root.style.paddingRight = 10;
            root.style.paddingBottom = 10;

            Label titleLabel = new Label(scriptName)
            {
                style =
                {
                    fontSize = 16,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    marginBottom = 10
                }
            };
            root.Add(titleLabel);

            ScrollView scrollView = new ScrollView();
            scrollView.style.flexGrow = 1;
            scrollView.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 1f);
            scrollView.style.borderTopColor = Color.gray;
            scrollView.style.borderTopWidth = 1;
            scrollView.style.borderBottomColor = Color.gray;
            scrollView.style.borderBottomWidth = 1;
            scrollView.style.paddingTop = 5;
            scrollView.style.paddingBottom = 5;
            root.Add(scrollView);

            Label contentLabel = new Label(displayText)
            {
                style =
                {
                    fontSize = 14,
                    whiteSpace = WhiteSpace.Normal,
                    unityTextAlign = TextAnchor.MiddleLeft,
                    marginBottom = 5,
                    paddingLeft = 5,
                    paddingRight = 5
                }
            };
            scrollView.Add(contentLabel);

            Button closeButton = new Button(() => Close())
            {
                text = "Close",
                style =
                {
                    fontSize = 14,
                    marginTop = 10,
                    alignSelf = Align.FlexEnd
                }
            };
            root.Add(closeButton);
        }
    }
}