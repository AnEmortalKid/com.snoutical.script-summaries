using System.Collections.Generic;
using System.Text.RegularExpressions;
using Snoutical.ScriptSummaries.Generation.API;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Snoutical.ScriptSummaries.Editor.UI.Window
{
    /// <summary>
    /// An editor window that displays all summaries attached to monobehaviours
    /// based on the selected object
    /// </summary>
    public class ScriptSummariesWindow : EditorWindow
    {
        [MenuItem("Tools/Script Summaries/Summary Window")]
        public static void CreateWindow()
        {
            ScriptSummariesWindow wnd = GetWindowReference();
            wnd.titleContent = new GUIContent("Script Summaries");
        }

        private bool initialized;
        private Label titleLabel;
        
        private ListView leftPane;

        private ScrollView contentScrolLView;
        private Label contentLabel;

        private List<ScriptSummaryWindowItem> loadedItems = new();
        private HashSet<string> processedScripts = new();

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            root.style.flexDirection = FlexDirection.Column;
            root.style.paddingLeft = 5;
            root.style.paddingRight = 5;
            root.style.paddingTop = 5;
            root.style.paddingBottom = 5;


            titleLabel = new Label
            {
                style =
                {
                    fontSize = 16,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    marginBottom = 10
                }
            };
            root.Add(titleLabel);

            // Create a two-pane view with the left pane being fixed.
            var splitView = new TwoPaneSplitView(0, 250, TwoPaneSplitViewOrientation.Horizontal);
            root.Add(splitView);

            leftPane = new ListView
            {
                style =
                {
                    backgroundColor = new Color(0.15f, 0.15f, 0.15f, 1f),
                    borderRightColor = Color.gray,
                    borderRightWidth = 1,
                    paddingTop = 5,
                    paddingBottom = 5
                },
                selectionType = SelectionType.Single
            };
            splitView.Add(leftPane);
            //leftPane.viewDataKey = "notes-selection";
            leftPane.makeItem = () => new Label()
            {
                style =
                {
                    fontSize = 14,
                    paddingLeft = 10,
                    paddingTop = 4,
                    paddingBottom = 4
                }
            };
            leftPane.bindItem = (item, index) =>
            {
                if (item is Label label)
                {
                    label.text = PascalCaseToSpaced(loadedItems[index].Name);
                    label.style.unityTextAlign = TextAnchor.MiddleLeft;
                    label.style.paddingLeft = 10;

                    // Highlight effect when selected
                    label.RegisterCallback<PointerEnterEvent>(evt =>
                        label.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1f));
                    label.RegisterCallback<PointerLeaveEvent>(evt => label.style.backgroundColor = Color.clear);
                }
            };
            leftPane.itemsSource = loadedItems;
            leftPane.selectionChanged += OnMonoSelectionChange;

            VisualElement rightPane = new VisualElement()
            {
                style =
                {
                    flexGrow = 1,
                    paddingTop = 5,
                    paddingLeft = 10,
                    paddingRight = 10,
                    backgroundColor = new Color(0.1f, 0.1f, 0.1f, 1f)
                }
            };
            splitView.Add(rightPane);

            contentScrolLView = new ScrollView(ScrollViewMode.Vertical)
            {
                style =
                {
                    flexGrow = 1, // Ensures it expands to fill the pane
                    backgroundColor = new Color(0.1f, 0.1f, 0.1f, 1f),
                    paddingLeft = 10,
                    paddingRight = 10,
                    paddingTop = 5,
                    paddingBottom = 5
                }
            };
            rightPane.Add(contentScrolLView);

            contentLabel = new Label()
            {
                style =
                {
                    fontSize = 14,
                    // autowrap
                    whiteSpace = WhiteSpace.Normal
                }
            };
            contentScrolLView.Add(contentLabel);

            initialized = true;
        }

        private void OnSelectionChange()
        {
            DefaultHandler();
        }

        private void OnHierarchyChange()
        {
            DefaultHandler();
        }

        private void OnProjectChange()
        {
            DefaultHandler();
        }

        private void DefaultHandler()
        {
            if (!initialized)
            {
                return;
            }

            var selected = Selection.activeGameObject;
            // nothing selected should we clear it
            if (selected == null)
            {
                ClearItems();
                return;
            }

            // Always regenerate since its fast?
            // maybe do a thing where we cache by name
            RegenerateItems(selected);
        }

        private void ClearItems()
        {
            ClearPanesAndData();
            SetTabTitle("");
            leftPane.RefreshItems();
        }

        private void ClearPanesAndData()
        {
            loadedItems.Clear();
            ResetContentParts();
            leftPane.ClearSelection();
            processedScripts.Clear();
        }

        private void RegenerateItems(GameObject selectedObj)
        {
            ClearPanesAndData();

            // Try to read the notes, cache behaviour copies
            var monoBehaviours = selectedObj.GetComponents<MonoBehaviour>();
            foreach (var mono in monoBehaviours)
            {
                // did the script get deleted
                if (mono == null)
                {
                    continue;
                }

                var monoType = mono.GetType();
                if (processedScripts.Contains(monoType.Name))
                {
                    continue;
                }

                var summary = EditorSummaryAPI.GetEditorSummary(mono);
                if (summary == null)
                {
                    continue;
                }

                var summaryItem = new ScriptSummaryWindowItem();
                summaryItem.Name = monoType.Name;
                summaryItem.Summary = summary;
                loadedItems.Add(summaryItem);

                processedScripts.Add(monoType.Name);
            }

            SetTabTitle(selectedObj.name);
            leftPane.RefreshItems();
        }


        private void ResetContentParts()
        {
            contentLabel.text = "";
            // scroll back to top
            contentScrolLView.scrollOffset = Vector2.zero;
        }
        
        private void OnMonoSelectionChange(IEnumerable<object> selectedItems)
        {
            ResetContentParts();

            using var enumerator = selectedItems.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                return;
            }

            if (enumerator.Current is not ScriptSummaryWindowItem selected)
            {
                return;
            }

            contentLabel.text = selected.Summary;
        }

        private static string PascalCaseToSpaced(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            // insert spaces before uppercase letters
            return Regex.Replace(input, "([a-z])([A-Z])", "$1 $2");
        }

        private static ScriptSummariesWindow GetWindowReference()
        {
            // Always get a reference without focus to avoid interrupting keybinds
            return GetWindow<ScriptSummariesWindow>("Script Summaries", false);
        }

        private void SetTabTitle(string windowTitle)
        {
            titleLabel.text = windowTitle;
        }
    }
}