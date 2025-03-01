using System.IO;
using UnityEditor;
using UnityEngine;

namespace Snoutical.ScriptSummaries.Settings
{
    public class ScriptSummariesSettingsUtility
    {
        /// <summary>
        /// Path to write into correctly
        /// </summary>
        private static readonly string settingsPath =
            Path.Combine(Application.dataPath, "ScriptSummariesSettings.asset");

        /// <summary>
        /// Path for lookup from the Asset Database
        /// </summary>
        private static readonly string relativePath = "Assets/ScriptSummariesSettings.asset";

        /// <summary>
        /// Return a reference to the ScriptSummariesSettings if it exists, or null
        /// </summary>
        /// <returns>a settings object or null if it does not exist</returns>
        public static ScriptSummariesSettings FetchSettings()
        {
            return AssetDatabase.LoadAssetAtPath<ScriptSummariesSettings>(relativePath);
        }

        public static ScriptSummariesSettings CreateOrFetch()
        {
            // select it if it exists
            if (File.Exists(settingsPath))
            {
                ScriptSummariesSettings loaded = FetchSettings();
                return loaded;
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
            return settings;
        }
    }
}