# com.snoutical.script-summaries (Script Summaries)

For User facing documentation, see [User Docs](./Documentation~).

If you're interested in making a contribution, keep reading for some info.

## Development

This package uses `Roslyn` and some DLLs to regenerate the XML documentation for scripts.

Tips for development:

1. Install this as a `submodule` under your `Assets/Packages` for easier development.
    2. You can reference [this project](https://github.com/AnEmortalKid/ScriptSummariesDev) which is what I used for
       development

### Assembly Info

#### Setup

The `Setup` assembly is responsible for installing the DLLs to `Assets/Plugins/Editor`
and defining the compiler flag `SCRIPT_SUMMARIES_INSTALLED`

#### Generation

The `Generation` assembly deals with generating the XML, saving files and serving a lookup class
that can get the documentation for a script.

This assembly requires the `SCRIPT_SUMMARIES_INSTALLED` flag for compilation as well,
otherwise nothing will compile.

#### UI

The `UI` assembly holds our UI enhancements for the editor, since it calls an API in the `Generation`
assembly, it also relies on the `SCRIPT_SUMMARIES_INSTALLED` as well.

#### API

This is the publicly granted API, it should **NOT** conditionally compile based on the compiler define.
Instead, functions exposed in that assembly should defensively return a default value if the symbol is missing.


## Style

You'll notice there's a few helper classes with public methods being referenced in static fields
by other static classes. Most of the core logic is handled through static method calls.

For testability, I found it useful to make these helper classes so I could instantiate them to
test a piece of logic under certain conditions. 

Then the static class just references an instance of a helper when it needs to do something.

## Testing

### Testing Define Symbol 

If you want to test whether your exposed API can handle the symbol not being present,
write a quick utility that removes the symbol. Alternatively, you could use the Uninstall 
menu to remove the DLLs and define.

Here's a sample tool you can use:

```csharp
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Snoutical.ScriptSummaries.Setup.Debugging
{
    public class ScriptSummariesDebugMenu
    {
        private const string DefineSymbol = "SCRIPT_SUMMARIES_INSTALLED";
        private const string MenuPath = "Tools/Script Summaries/Debug/Toggle Installed Define";

        [MenuItem(MenuPath)]
        private static void ToggleScriptSummariesDefine()
        {
            string defines = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Standalone);

            if (defines.Contains(DefineSymbol))
            {
                // ✅ Remove define
                defines = defines.Replace(DefineSymbol, "").Replace(";;", ";").Trim(';');
                Debug.Log("❌ Removed " + DefineSymbol);
            }
            else
            {
                // ✅ Add define
                defines = string.IsNullOrEmpty(defines) ? DefineSymbol : defines + ";" + DefineSymbol;
                Debug.Log("✅ Added " + DefineSymbol);
            }

            // Apply the new define symbols
            PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Standalone, defines);
            AssetDatabase.Refresh();
        }

        [MenuItem(MenuPath, true)]
        private static bool ValidateMenu()
        {
            string defines =
                PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Standalone);
            Menu.SetChecked(MenuPath, defines.Contains(DefineSymbol));
            return true;
        }
    }
}
```

### Unit Tests

There's a `Test` assembly as well, if you can write something Unit testable, go for it.
