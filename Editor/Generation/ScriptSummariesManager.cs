using Snoutical.ScriptSummaries.Generation.Database;
using Snoutical.ScriptSummaries.Generation.Generator;

namespace Snoutical.ScriptSummaries.Editor.Generation
{
    /// <summary>
    /// Component responsible for orchestrating calls to the underlying systems
    /// </summary>
    public static class ScriptSummariesManager
    {
        /// <summary>
        /// Regenerates the documentation and refreshes the summary database
        /// </summary>
        public static void RegenerateAndReload()
        {
            DocumentationGenerator.RunRegeneration();
            InternalSummaryDatabase.ReInitialize();
        }
    }
}