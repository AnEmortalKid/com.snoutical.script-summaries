using Snoutical.ScriptSummaries.Generation.Generator;
using Snoutical.ScriptSummaries.Setup.Settings;
using UnityEditor;
using UnityEngine;

namespace Snoutical.ScriptSummaries.Editor.Generation.Handler
{
    /// <summary>
    /// Attaches a listener to afterAssemblyReload event to auto regenerate docs when an assembly reloads
    /// if the configuration settings allow it
    /// </summary>
    [InitializeOnLoad]
    public class ScriptSummariesReloadHandler
    {
        static ScriptSummariesReloadHandler()
        {
            // Prevent multiple subscriptions
            AssemblyReloadEvents.afterAssemblyReload -= OnScriptsReloaded;
            AssemblyReloadEvents.afterAssemblyReload += OnScriptsReloaded;
        }

        private static void OnScriptsReloaded()
        {
            var settings = ScriptSummariesSettingsUtility.FetchSettings();
            // do nothing since its the same as default false
            if (settings == null)
            {
                return;
            }

            if (!settings.AutoGenerateOnReload)
            {
                return;
            }

            Debug.Log("Scripts reloaded, regenerating documentation...");
            ScriptSummariesManager.RegenerateAndReload();
            // not calling asset database refresh since our files land in Library
        }
    }
}