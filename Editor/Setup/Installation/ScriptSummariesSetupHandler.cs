using Snoutical.ScriptSummaries.Editor.Common.Logger;
using Snoutical.ScriptSummaries.Setup.Installation;
using UnityEditor;

namespace Snoutical.ScriptSummaries.Setup
{
    /// <summary>
    /// Responsible for invoking the installer and listening to editor events to determine install
    /// </summary>
    [InitializeOnLoad]
    public class ScriptSummariesSetupHandler
    {
        static ScriptSummariesSetupHandler()
        {
            // Attempt to run but not when in playmode
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.update += OnEditorLoaded;
            }
        }

        private static void OnEditorLoaded()
        {
            // Stop listening, probably wanna do this conditionally
            EditorApplication.update -= OnEditorLoaded;

            CheckAndInstallDependencies();
        }

        private static void CheckAndInstallDependencies()
        {
            if (ScriptSummariesInstaller.IsInstalled())
            {
                return;
            }

            bool install = EditorUtility.DisplayDialog(
                "Missing Dependencies",
                "Script Summaries requires additional dependencies. Would you like to install them now?",
                "Yes, Install",
                "Cancel"
            );

            if (!install)
            {
                ScriptSummariesLogger.LogError(
                    "❌ Required dependencies were not installed. The package may not function correctly.");
                return;
            }


            ScriptSummariesInstaller.Install(out bool installSucceeded);
            if (installSucceeded)
            {
                AssetDatabase.Refresh();
                ScriptSummariesLogger.Log("✅ Installation complete!");
                EditorUtility.DisplayDialog("Installation Complete",
                    "All dependencies have been installed successfully.", "OK");
            }
            else
            {
                ScriptSummariesLogger.LogError(
                    "❌ Missing dependencies still, package will not work correctly.");
            }
        }
    }
}