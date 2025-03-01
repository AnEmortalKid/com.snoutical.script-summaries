using System;

#if UNITY_EDITOR && SCRIPT_SUMMARIES_INSTALLED

namespace Snoutical.ScriptSummaries.Tools.Generation
{
    /// <summary>
    /// A wrapper around the json assembly def so we can read some properties
    /// </summary>
    [Serializable]
    public class AssemblyDefinition
    {
        public string name;
    }
}
#endif