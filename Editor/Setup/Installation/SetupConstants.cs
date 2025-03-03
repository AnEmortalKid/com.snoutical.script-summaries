using System.IO;
using UnityEngine;

namespace Snoutical.ScriptSummaries.Setup.Installation
{
    public class SetupConstants
    {
        /// <summary>
        /// Compiler symbol we Define or Undefine to flex logic around
        /// </summary>
        public const string DefineSymbol = "SCRIPT_SUMMARIES_INSTALLED";

        /// <summary>
        /// Set of DLLs we install or remove
        /// </summary>
        public static readonly string[] RequiredDlls =
        {
            "Microsoft.CodeAnalysis.dll",
            "Microsoft.CodeAnalysis.CSharp.dll",
            "System.Reflection.Metadata.dll",
            "System.Collections.Immutable.dll",
            "System.Runtime.CompilerServices.Unsafe.dll"
        };

        /// <summary>
        /// Returns a resolved path to where our DLLs should be installed
        /// Callers should cache this value in the execution method
        /// </summary>
        /// <returns>a full path to where our Dlls should be installed</returns>
        public static string GetPluginsPath()
        {
            return Path.Combine(Application.dataPath, "Plugins", "Editor");
        }
    }
}