#if UNITY_EDITOR && SCRIPT_SUMMARIES_INSTALLED
using System.IO;
using Snoutical.ScriptSummaries.Settings;
using UnityEditor;
using UnityEngine;

namespace Snoutical.ScriptSummaries.Tools.Generation.Menu
{
    /// <summary>
    /// A menu for generating the settings object since the scriptable object's
    /// menu does not get read from the Assets/Packages dir
    /// </summary>
    public class ScriptSummariesSettingsMenu
    {
        private static readonly string settingsPath =
            Path.Combine(Application.dataPath, "ScriptSummariesSettings.asset");

        [MenuItem("Tools/Script Summaries/Create Settings Asset")]
        public static void CreateSettings()
        {
            string relativePath = "Assets/ScriptSummariesSettings.asset";
            // select it if it exists
            if (File.Exists(settingsPath))
            {
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<ScriptSummariesSettings>(relativePath);
                return;
            }

            var settings = ScriptableObject.CreateInstance<ScriptSummariesSettings>();

            // Ensure the directory exists
            string directory = Path.GetDirectoryName(settingsPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            AssetDatabase.CreateAsset(settings, relativePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("✅ Created ScriptSummariesSettings at " + relativePath);
            Selection.activeObject = settings; // Select in Inspector
        }
    }
}
#endif