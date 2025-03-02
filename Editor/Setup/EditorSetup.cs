using System;
using System.IO;
using System.Net;
using Snoutical.ScriptSummaries.Editor.Common.Logger;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Snoutical.ScriptSummaries.Setup
{
    /// <summary>
    /// Responsible for downloading Roslyn and its dependencies and defining the compiler symbol so the asset works.
    /// Since we can't redistribute DLLs we don't own, we will download them to the Assets/Plugins/Editor section
    /// and make it easier for users to get those dependencies without NuGet
    /// </summary>
    [InitializeOnLoad]
    public class EditorSetup
    {
        private static string pluginsPath;

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

        // Required DLLs that must be installed
        private static readonly string[] RequiredDlls =
        {
            "Microsoft.CodeAnalysis.dll",
            "Microsoft.CodeAnalysis.CSharp.dll",
            "System.Reflection.Metadata.dll",
            "System.Collections.Immutable.dll",
            "System.Runtime.CompilerServices.Unsafe.dll"
        };

        static EditorSetup()
        {
            // Run on application load once
            EditorApplication.update += OnEditorLoaded;
        }

        private static void OnEditorLoaded()
        {
            // Stop listening, probably wanna do this conditionally
            EditorApplication.update -= OnEditorLoaded;

            pluginsPath = Path.Combine(Application.dataPath, "Plugins", "Editor");

            CheckAndInstallDependencies();
        }

        /// <summary>
        /// Determines whether we have all required dependencies or not
        /// </summary>
        /// <returns>true if we are missing any DLL, false otherwise</returns>
        private static bool IsMissingDependencies()
        {
            bool missingDlls = false;

            foreach (string dll in RequiredDlls)
            {
                if (!File.Exists(Path.Combine(pluginsPath, dll)))
                {
                    missingDlls = true;
                    break;
                }
            }

            return missingDlls;
        }

        private static void CheckAndInstallDependencies()
        {
            if (!IsMissingDependencies())
            {
                // setup complete
                return;
            }

            bool install = EditorUtility.DisplayDialog(
                "Missing Dependencies",
                "Script Summaries requires additional dependencies. Would you like to install them now?",
                "Yes, Install",
                "Cancel"
            );

            if (!install)
            {
                ScriptSummariesLogger.LogError(
                    "❌ Required dependencies were not installed. The package may not function correctly.");
                return;
            }

            InstallDependencies();

            // See if we can add the define now
            if (!IsMissingDependencies())
            {
                AddCompilerDefine("SCRIPT_SUMMARIES_INSTALLED");

                AssetDatabase.Refresh();
                ScriptSummariesLogger.Log("✅ Installation complete!");
                EditorUtility.DisplayDialog("Installation Complete",
                    "All dependencies have been installed successfully.", "OK");
            }
            else
            {
                ScriptSummariesLogger.LogError(
                    "❌ Missing dependencies still, package will not work correctly.");
            }
        }

        private static void InstallDependencies()
        {
            try
            {
                if (!Directory.Exists(pluginsPath))
                {
                    Directory.CreateDirectory(pluginsPath);
                }

                foreach (string package in Packages)
                {
                    DownloadDependency(package);
                }
            }
            catch (Exception ex)
            {
                // Forcing the user to see this
                ScriptSummariesLogger.LogError("❌ Installation failed: " + ex.Message, true);
            }
        }

        private static void DownloadDependency(string package)
        {
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
                Directory.Delete(extractPath, true);

            System.IO.Compression.ZipFile.ExtractToDirectory(packageDownloadPath, extractPath);
            string dllSourcePath = Path.Combine(extractPath, "lib/netstandard2.0/");

            if (Directory.Exists(dllSourcePath))
            {
                foreach (string dll in RequiredDlls)
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
            }
        }

        private static void AddCompilerDefine(string defineSymbol)
        {
            string symbols = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Standalone);

            if (!symbols.Contains(defineSymbol))
            {
                symbols += $";{defineSymbol}";
                PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Standalone, symbols);
                ScriptSummariesLogger.Log($"✅ Added compiler define: {defineSymbol}");
            }
        }
    }
}