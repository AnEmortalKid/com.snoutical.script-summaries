using UnityEditor;
using UnityEngine;

namespace Snoutical.ScriptSummaries.Setup.Settings
{
    public class ScriptSummariesSettings : ScriptableObject
    {
        [Tooltip("Folders to scan for summary documentation. Select directories from Assets.")]
        public DefaultAsset[] ScanDirectories;
    }
}