namespace Snoutical.ScriptSummaries.Editor.Generation.Util
{
    /// <summary>
    /// Utilities to work with paths consistently
    /// </summary>
    public class PathUtils
    {
        /// <summary>
        /// Converts a path into forward slash syntax
        /// </summary>
        /// <param name="rawPath">the path value</param>
        /// <returns>a forward slash based path</returns>
        public static string NormalizePath(string rawPath)
        {
            return rawPath.Replace("\\", "/");
        }
    }
}