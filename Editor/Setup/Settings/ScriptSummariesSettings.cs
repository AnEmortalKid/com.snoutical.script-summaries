using UnityEditor;
using UnityEngine;

namespace Snoutical.ScriptSummaries.Setup.Settings
{
    public class ScriptSummariesSettings : ScriptableObject
    {
        [Header("Generation")]
        [Tooltip("Folders to scan for summary documentation. Select directories from Assets.")]
        public DefaultAsset[] ScanDirectories;

        [Tooltip("Whether documentation should be regenerated when an assembly reload is detected or not.")]
        public bool AutoGenerateOnReload;

        [Header("Display")]
        [Tooltip(@"How many lines of a summary should when hovering over a script on the Assets view 
            within the Project tab.
            Default value of 0 will show all lines.
        ")]
        public int TooltipLineLength;

        [Header("Logging")]
        [Tooltip("Whether the asset should log messages upon generation or not")]
        public bool EnableDebugLogging;
    }
}