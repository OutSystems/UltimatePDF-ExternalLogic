using System;

namespace OutSystems.UltimatePDF_ExternalLogic.Utils {

    /// <summary>
    /// Naming and validation for the artifacts the File actions store. Kept apart from the action
    /// bodies because none of it touches storage, so all of it is testable off-platform — which the
    /// actions themselves are not, since they need a browser.
    /// </summary>
    internal static class FileArtifacts {

        internal const string PdfContentType = "application/pdf";

        internal const string PngContentType = "image/png";

        /// <summary>
        /// The platform caps a response payload at 5.5MB. The File actions store the artifact itself,
        /// so the troubleshooting ZIP is the only output of theirs that still crosses the boundary.
        /// </summary>
        internal const int MaxLogsPayloadBytes = 5 * 1024 * 1024 + 512 * 1024;

        internal const string DefaultPdfFileName = "document.pdf";

        internal const string DefaultPngFileName = "screenshot.png";

        /// <summary>
        /// Returns <paramref name="fileName"/>, or <paramref name="fallback"/> when the caller left it
        /// empty. ODC sends an unfilled optional Text parameter as an empty string, so blank is the
        /// normal case rather than a mistake, and whitespace is treated as blank for the same reason.
        /// </summary>
        internal static string EnsureFileName(string? fileName, string fallback) {
            var name = (fileName ?? string.Empty).Trim();

            return name.Length == 0 ? fallback : name;
        }

        /// <summary>
        /// True when the troubleshooting ZIP is too large to cross the platform boundary.
        /// </summary>
        /// <remarks>
        /// AttachFilesLogs puts the finished artifact plus every intermediate render into the ZIP, so
        /// it is routinely larger than the artifact itself and can exceed the limit on reports that
        /// are well within it.
        /// </remarks>
        internal static bool LogsExceedPayloadLimit(byte[] logsZip) {
            return logsZip.Length > MaxLogsPayloadBytes;
        }

        /// <summary>
        /// Rejects empty content before anything is stored.
        /// </summary>
        /// <remarks>
        /// The render helpers log a failure and return an empty array rather than throwing. That is
        /// harmless for the Binary Data actions, where the app receives empty binary data and can see
        /// that for itself, but a File action would store a 0 byte file and hand back a reference to
        /// it, reporting success. The underlying cause is already in the ODC logs via the platform
        /// logger, so this only has to stop the empty write.
        /// </remarks>
        internal static void EnsureNotEmpty(byte[] content, string artifact) {
            if (content.Length > 0) {
                return;
            }

            throw new Exception($"Something went wrong generating the {artifact}, so no file was stored. " +
                "Check the application logs in ODC Portal, or enable CollectLogs for details.");
        }
    }
}
