using System;
using System.IO;
using System.Net;
using Snoutical.ScriptSummaries.Editor.Common.Logger;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Snoutical.ScriptSummaries.Setup.Installation
{
    /// <summary>
    /// Handles logic for downloading dependencies and defining the compile symbol
    ///
    /// Responsible for downloading Roslyn and its dependencies and defining the compiler symbol so the asset works.
    /// Since we can't redistribute DLLs we don't own, we will download them to the Assets/Plugins/Editor section
    /// and make it easier for users to get those dependencies without NuGet
    /// </summary>
    public static class ScriptSummariesInstaller
    {
        private static readonly string NuGetUrl = "https://www.nuget.org/api/v2/package/";

        // Packages required for Roslyn to work
        private static readonly string[] Packages =
        {
            "Microsoft.CodeAnalysis.CSharp/4.0.1",
            "Microsoft.CodeAnalysis.Common/4.0.1",
            "System.Reflection.Metadata/1.6.0",
            "System.Collections.Immutable/8.0.0",
            "System.Runtime.CompilerServices.Unsafe/6.0.0"
        };

        /// <summary>
        /// Attempts to install our dependencies and compiler define,
        /// call IsInstalled before invoking this
        /// </summary>
        /// <returns>true if everything installed correctly (or was installed previously), false otherwise</returns>
        public static void Install(out bool success)
        {
            if (IsMissingDependencies())
            {
                // exit early if our deps failed
                bool dependencySuccess = InstallDependencies();
                if (!dependencySuccess)
                {
                    success = false;
                    return;
                }
            }

            if (!HasCompilerDefine())
            {
                AddCompilerDefine();
            }

            success = true;
        }

        /// <summary>
        /// Determines whether the required parts are installed for our tool to work or not
        /// </summary>
        /// <returns>true if all our components are installed, false otherwise</returns>
        public static bool IsInstalled()
        {
            return !IsMissingDependencies() && HasCompilerDefine();
        }


        /// <summary>
        /// Determines whether we have all required dependencies or not
        /// </summary>
        /// <returns>true if we are missing any DLL, false otherwise</returns>
        public static bool IsMissingDependencies()
        {
            bool missingDlls = false;
            var pluginsPath = Path.Combine(Application.dataPath, "Plugins", "Editor");

            foreach (string dll in SetupConstants.RequiredDlls)
            {
                if (!File.Exists(Path.Combine(pluginsPath, dll)))
                {
                    missingDlls = true;
                    break;
                }
            }

            return missingDlls;
        }


        private static bool InstallDependencies()
        {
            var pluginsPath = SetupConstants.GetPluginsPath();
            try
            {
                if (!Directory.Exists(pluginsPath))
                {
                    Directory.CreateDirectory(pluginsPath);
                }

                foreach (string package in Packages)
                {
                    bool downloadSucceed = DownloadDependency(package);
                    if (!downloadSucceed)
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                // Forcing the user to see this
                ScriptSummariesLogger.LogError("❌ Installation failed: " + ex.Message, true);
                return false;
            }

            return true;
        }

        private static bool DownloadDependency(string package)
        {
            var pluginsPath = SetupConstants.GetPluginsPath();

            string packageDownloadPath = Path.Combine(Application.temporaryCachePath,
                $"{package.Replace("/", "-")}.nupkg");

            using (WebClient client = new WebClient())
            {
                string packageUrl = NuGetUrl + package;
                ScriptSummariesLogger.Log($"🔹 Downloading {package} from {packageUrl}...");
                client.DownloadFile(packageUrl, packageDownloadPath);
            }

            string extractPath = Path.Combine(Application.temporaryCachePath,
                $"{package.Replace("/", "-")}-Extracted");

            if (Directory.Exists(extractPath))
            {
                Directory.Delete(extractPath, true);
            }


            System.IO.Compression.ZipFile.ExtractToDirectory(packageDownloadPath, extractPath);
            string dllSourcePath = Path.Combine(extractPath, "lib/netstandard2.0/");

            if (Directory.Exists(dllSourcePath))
            {
                foreach (string dll in SetupConstants.RequiredDlls)
                {
                    string sourceFile = Path.Combine(dllSourcePath, dll);
                    string destFile = Path.Combine(pluginsPath, dll);

                    if (File.Exists(sourceFile))
                    {
                        File.Copy(sourceFile, destFile, true);
                        ScriptSummariesLogger.Log($"✅ Installed {dll} from {package} to {pluginsPath}");
                    }
                }
            }
            else
            {
                ScriptSummariesLogger.LogError(
                    $"❌ Extraction failed for {package}. Could not find the expected directory.");
                return false;
            }

            // assume this succeeded
            return true;
        }

        private static void AddCompilerDefine()
        {
            string symbols = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Standalone);

            if (!symbols.Contains(SetupConstants.DefineSymbol))
            {
                symbols += $";{SetupConstants.DefineSymbol}";
                PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Standalone, symbols);
                ScriptSummariesLogger.Log($"✅ Added compiler define: {SetupConstants.DefineSymbol}");
            }
        }

        private static bool HasCompilerDefine()
        {
            string symbols = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Standalone);
            return symbols.Contains(SetupConstants.DefineSymbol);
        }
    }
}