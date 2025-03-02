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
            wnd.titleContent = new GUIContent("Script Notes");
        }

        private bool initialized;
        private VisualElement rightPane;
        private ListView leftPane;

        private List<ScriptSummaryWindowItem> loadedItems = new();
        private HashSet<string> processedScripts = new();

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;

            // Create a two-pane view with the left pane being fixed.
            var splitView = new TwoPaneSplitView(0, 250, TwoPaneSplitViewOrientation.Horizontal);
            root.Add(splitView);

            leftPane = new ListView();
            splitView.Add(leftPane);
            //leftPane.viewDataKey = "notes-selection";
            leftPane.makeItem = () => new Label()
            {
                style = { fontSize = 14 }
            };
            leftPane.bindItem = (item, index) =>
            {
                (item as Label).text = PascalCaseToSpaced(loadedItems[index].Name);
            };
            leftPane.itemsSource = loadedItems;
            leftPane.selectionChanged += OnMonoSelectionChange;

            rightPane = new VisualElement();
            splitView.Add(rightPane);

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
            SetTabTitle("Script Notes");
            leftPane.RefreshItems();
        }

        private void ClearPanesAndData()
        {
            loadedItems.Clear();
            rightPane.Clear();
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

            SetTabTitle(selectedObj.name + " Notes");
            leftPane.RefreshItems();
        }


        private void OnMonoSelectionChange(IEnumerable<object> selectedItems)
        {
            rightPane.Clear();

            using var enumerator = selectedItems.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                return;
            }

            if (enumerator.Current is not ScriptSummaryWindowItem selected)
            {
                return;
            }

            var label = new Label()
            {
                style =
                {
                    fontSize = 14,
                    // autowrap
                    whiteSpace = WhiteSpace.Normal
                }
            };
            label.text = selected.Summary;
            rightPane.Add(label);
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
            return GetWindow<ScriptSummariesWindow>("Script Notes", false);
        }

        private static void SetTabTitle(string windowTitle)
        {
            ScriptSummariesWindow wnd = GetWindowReference();
            wnd.titleContent.text = windowTitle;
        }
    }
}