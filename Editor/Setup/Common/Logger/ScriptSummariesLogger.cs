using Snoutical.ScriptSummaries.Setup.Settings;
using UnityEngine;

namespace Snoutical.ScriptSummaries.Editor.Common.Logger
{
    /// <summary>
    /// Common logger to have consistent branding in our messages
    /// It lives in this assembly since we reference the settings in order to toggle logging on or off
    /// </summary>
    public static class ScriptSummariesLogger
    {
        private const string Prefix = "📜 [ScriptSummaries]";

        /// <summary>
        /// Logs a Log level message
        /// </summary>
        /// <param name="message">the formatted message</param>
        /// <param name="forced">whether we should log regardless of user desire</param>
        public static void Log(string message, bool forced = false)
        {
            if (forced || AllowsLogging())
            {
                Debug.Log($"{Prefix} {message}");    
            }
        }

        /// <summary>
        /// Logs a Warning level message
        /// </summary>
        /// <param name="message">the formatted message</param>
        /// <param name="forced">whether we should log regardless of user desire</param>
        public static void LogWarning(string message, bool forced = false)
        {
            if (forced || AllowsLogging())
            {
                Debug.LogWarning($"{Prefix} {message}");
            }
        }

        /// <summary>
        /// Logs an Error level message
        /// </summary>
        /// <param name="message">the formatted message</param>
        /// <param name="forced">whether we should log regardless of user desire</param> 
        public static void LogError(string message, bool forced = false)
        {
            if (forced || AllowsLogging())
            {
                Debug.LogError($"{Prefix} {message}");
            }
        }

        private static bool AllowsLogging()
        {
            var settings = ScriptSummariesSettingsUtility.FetchSettings();
            return settings != null && settings.EnableDebugLogging;
        }
    }
}