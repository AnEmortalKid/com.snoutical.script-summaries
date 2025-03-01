namespace Snoutical.ScriptSummaries.Generation.Constants
{
    public static class GenerationConstants
    {
        /// <summary>
        /// Directory where we store our generated files
        /// </summary>
        public static readonly string OutputDirectory = "Library/ScriptSummaries/";

        /// <summary>
        /// The Assembly name to assign to scripts and behaviors that don't have one defined
        /// An XML doc and Lookup doc will be generated with this name when a script doesn't have an assembly def we can find
        /// </summary>
        public static readonly string FallbackAssemblyName = "Assembly-CSharp";
    }
}