namespace Snoutical.ScriptSummaries.Generation.Generator
{
    /// <summary>
    /// Tracks the summary for a given script and attributes we use to store that data in our various files
    /// </summary>
    public class SummaryMapping
    {
        /// <summary>
        /// The text value of the summary without leading /
        /// </summary>
        public string summary;

        /// <summary>
        /// Path where this summary was fetched from, relative from root project, so probably Assets/Scripts
        /// </summary>
        public string relativePath;

        /// <summary>
        /// The name of the assembly this script belongs to, if one could be found, or null
        /// </summary>
        public string assemblyName;

        /// <summary>
        /// An identifier following the csharp xml doc ID string, e.g. T:Namespace.Class
        /// </summary>
        public string memberIdentifier;
    }
}