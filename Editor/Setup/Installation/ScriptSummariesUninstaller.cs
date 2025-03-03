using System.IO;
using Snoutical.ScriptSummaries.Editor.Common.Logger;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Snoutical.ScriptSummaries.Setup.Installation
{
    /// <summary>
    /// Handles logic for uninstalling and cleaning up after ourselves
    /// </summary>
    public static class ScriptSummariesUninstaller
    {
        // Place it all the way at the bottom
        [MenuItem("Tools/Script Summaries/Uninstall", false, 2000)]
        private static void Uninstall()
        {
            if (EditorUtility.DisplayDialog(
                    "Uninstall Script Summaries",
                    "Are you sure you want to uninstall Script Summaries? This will delete the package files and reset script defines.",
                    "Uninstall", "Cancel"))
            {
                DeleteInstalledDlls();
                RemoveDefineSymbol();

                EditorUtility.DisplayDialog("Uninstall Complete", "Script Summaries has been successfully uninstalled.",
                    "OK");

                // force a reload
                AssetDatabase.Refresh();
            }
        }

        private static void DeleteInstalledDlls()
        {
            string pluginsPath = Path.Combine(Application.dataPath, "Plugins", "Editor");
            if (Directory.Exists(pluginsPath))
            {
                foreach (string dllName in SetupConstants.RequiredDlls)
                {
                    string filePath = Path.Combine(pluginsPath, dllName);
                    string metaFilePath = filePath + ".meta";
                    try
                    {
                        File.Delete(filePath);
                        File.Delete(metaFilePath);
                        ScriptSummariesLogger.Log($"✅ Removed {dllName} from {pluginsPath}");
                    }
                    catch (IOException e)
                    {
                        ScriptSummariesLogger.LogError($"❌ Failed to delete {filePath}: {e.Message}", true);
                    }
                }
            }
        }

        private static void RemoveDefineSymbol()
        {
            string currentSymbols = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Standalone);
            if (currentSymbols.Contains(SetupConstants.DefineSymbol))
            {
                // wipe it out then clean up double ;;
                currentSymbols = currentSymbols.Replace(SetupConstants.DefineSymbol, "").Replace(";;", ";")
                    .TrimEnd(';');
                PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Standalone, currentSymbols);
            }
        }
    }
}