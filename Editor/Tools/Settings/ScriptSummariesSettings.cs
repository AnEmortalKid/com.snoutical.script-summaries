using UnityEditor;
using UnityEngine;

namespace Snoutical.ScriptSummaries.Settings
{
    public class ScriptSummariesSettings : ScriptableObject
    {
        [Tooltip("Folders to scan for summary documentation. Select directories from Assets.")]
        public DefaultAsset[] ScanDirectories;
    }
}