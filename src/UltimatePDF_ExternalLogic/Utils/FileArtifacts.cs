using System;
using System.IO;

namespace OutSystems.UltimatePDF_ExternalLogic.Utils {

    /// <summary>
    /// Naming and validation for the artifacts the File actions store. Kept apart from the action
    /// bodies because none of it touches storage, so all of it is testable off-platform — which the
    /// actions themselves are not, since they need a browser.
    /// </summary>
    internal static class FileArtifacts {

        internal const string PdfContentType = "application/pdf";

        internal const string PngContentType = "image/png";

        internal const string ZipContentType = "application/zip";

        internal const string DefaultPdfFileName = "document.pdf";

        internal const string DefaultPngFileName = "screenshot.png";

        /// <summary>
        /// Returns the given PDF file name, or <see cref="DefaultPdfFileName"/> when the caller left
        /// it empty.
        /// </summary>
        internal static string EnsurePdfFileName(string? fileName) {
            return EnsureName(fileName, DefaultPdfFileName);
        }

        /// <summary>
        /// Returns the given PNG file name, or <see cref="DefaultPngFileName"/> when the caller left
        /// it empty.
        /// </summary>
        internal static string EnsurePngFileName(string? fileName) {
            return EnsureName(fileName, DefaultPngFileName);
        }

        /// <summary>
        /// Names the troubleshooting ZIP after the artifact it describes, so the two are recognisable
        /// as a pair in storage.
        /// </summary>
        internal static string LogsFileNameFor(string artifactName) {
            var stem = Path.GetFileNameWithoutExtension(artifactName);

            // GetFileNameWithoutExtension returns empty for a name that is only an extension, such as
            // ".pdf". Falling back to the whole name keeps the result from being just "-logs.zip".
            if (stem.Length == 0) {
                stem = artifactName;
            }

            return $"{stem}-logs.zip";
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

        /// <summary>
        /// ODC sends an unfilled optional Text parameter as an empty string, so blank is the normal
        /// case rather than a mistake, and whitespace is treated as blank for the same reason.
        /// </summary>
        private static string EnsureName(string? fileName, string fallback) {
            var name = (fileName ?? string.Empty).Trim();

            return name.Length == 0 ? fallback : name;
        }
    }
}
