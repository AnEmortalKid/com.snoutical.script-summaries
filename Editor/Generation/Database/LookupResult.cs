namespace Snoutical.ScriptSummaries.Generation.Database
{
    /// <summary>
    /// Stores an individual lookup item
    /// </summary>
    public class LookupResult
    {
        /// <summary>
        /// The path to the script from the root of the project,
        /// this also is the left hand key in our file
        /// </summary>
        public string scriptPath;

        /// <summary>
        /// The assembly and type name combined key,
        /// this is the right hand value in our file
        /// </summary>
        public string namespacedTypeKey;

        /// <summary>
        /// The assembly the file belongs to
        /// </summary>
        public string assembly;

        /// <summary>
        /// The qualified type name the script defines
        /// </summary>
        public string typeName;
    }
}