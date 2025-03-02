using System.IO;
using Snoutical.ScriptSummaries.Editor.Common.Logger;
using UnityEditor;
using UnityEngine;

namespace Snoutical.ScriptSummaries.Setup.Settings
{
    public static class ScriptSummariesSettingsUtility
    {
        /// <summary>
        /// Path to write into correctly
        /// </summary>
        public static readonly string SettingsPath =
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

        /// <summary>
        /// Creates a new ScriptSummariesSettings or creates one
        /// </summary>
        /// <returns>a settings object</returns>
        public static ScriptSummariesSettings CreateOrFetch()
        {
            // select it if it exists
            if (File.Exists(SettingsPath))
            {
                ScriptSummariesSettings loaded = FetchSettings();
                return loaded;
            }

            var settings = ScriptableObject.CreateInstance<ScriptSummariesSettings>();

            // Ensure the directory exists
            string directory = Path.GetDirectoryName(SettingsPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            AssetDatabase.CreateAsset(settings, relativePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            ScriptSummariesLogger.Log("✅ Created ScriptSummariesSettings at " + relativePath);
            return settings;
        }
    }
}