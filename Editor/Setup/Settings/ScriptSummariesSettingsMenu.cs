using System.IO;
using UnityEditor;

namespace Snoutical.ScriptSummaries.Setup.Settings
{
    /// <summary>
    /// A menu for generating the settings object since the scriptable object's
    /// menu does not get read from the Assets/Packages dir
    /// </summary>
    public class ScriptSummariesSettingsMenu
    {
        [MenuItem("Tools/Script Summaries/Create Settings Asset")]
        public static void CreateSettings()
        {
            string relativePath = "Assets/ScriptSummariesSettings.asset";
            // select it if it exists
            if (File.Exists(ScriptSummariesSettingsUtility.SettingsPath))
            {
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<ScriptSummariesSettings>(relativePath);
                return;
            }

            var settings = ScriptSummariesSettingsUtility.CreateOrFetch();
            Selection.activeObject = settings;
        }
    }
}