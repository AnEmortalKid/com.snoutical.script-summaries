# Extending this Package

Script Summaries provides an assembly you can reference to build your own tooling. 


1. Reference `ScriptSummaries.Editor.API` if you're using assembly definitions.
2. Use the `ScriptSummaries` class from the `Snoutical.ScriptSummaries.API` to retrieve summaries


The `ScriptSummaries` class has been designed to passively handle if the packages dependencies
dont exist or if the package has been uninstalled. 

Here's a sample of a custom inspector that adds helper tooltips to behaviours if they have a summary
attached to them.

```csharp
using Snoutical.ScriptSummaries.API;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(YourScriptClassHere))]
public class ScriptWithTooltipInspector : Editor
{
    public override void OnInspectorGUI()
    {
        MonoBehaviour targetScript = (MonoBehaviour)target;
        string summary = ScriptSummaries.GetSummary(targetScript);

        EditorGUILayout.HelpBox("This uses the runtime api conditionally", MessageType.None);
        // If the script has a summary, display it as a tooltip at the top
        if (!string.IsNullOrEmpty(summary))
        {
            EditorGUILayout.HelpBox(summary, MessageType.Info);
        }

        // Draw default Inspector UI
        DrawDefaultInspector();
    }
}
```

You could turn this into an inspector for all `MonoBehaviour` objects by changing it to be an
inspector for anything extending from MonoBehaviour with `[CustomEditor(typeof(MonoBehaviour), true)]`
