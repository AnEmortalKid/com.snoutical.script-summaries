using System;
using System.Linq;

namespace Snoutical.ScriptSummaries.Editor.UI.Util
{
    /// <summary>
    /// Utilities for operating on summary text
    /// </summary>
    public static class SummaryUtils
    {
        /// <summary>
        /// Trims the summary to the desired amount of lines if it exceeds the max
        /// </summary>
        /// <param name="summary">the contents of the summary</param>
        /// <param name="maxLines">how many lines to keep</param>
        /// <returns>a trimmed summary to the desired maximum amount of lines</returns>
        public static string TrimSummary(string summary, int maxLines)
        {
            if (string.IsNullOrEmpty(summary) || maxLines <= 0)
            {
                return "";
            }

            // Split summary into lines
            string[] lines = summary.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            // Trim to max lines
            if (lines.Length > maxLines)
            {
                return string.Join("\n", lines.Take(maxLines)) + "\n...";
            }

            return summary;
        }
    }
}