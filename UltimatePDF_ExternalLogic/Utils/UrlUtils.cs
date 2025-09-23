using System;
using System.Linq;
using System.Text;

namespace UltimatePDF_ExternalLogic.Utils {
    internal class UrlUtils {
        internal static string BuildUrl(string baseUrl, string module, string path) {
            var urlBuilder = new StringBuilder();
            
            if (!baseUrl.StartsWith("https")) {
                urlBuilder.Append("https://");
            }
            
            urlBuilder.Append(RemoveEndDash(baseUrl));
            
            if(!module.StartsWith("/")) {
                urlBuilder.Append('/');
            }

            urlBuilder.Append(RemoveEndDash(module));

            if (!path.StartsWith("/")) {
                urlBuilder.Append('/');
            }

            return urlBuilder.Append(path).ToString();
        }

        internal static string GetFilenameFromUrl(string url) {
            return new Uri(url).Segments.Last().ToString();
        }

        private static string RemoveEndDash(string str) {
            return str.EndsWith("/") ? str.Remove(str.Length-1) : str;
        }

        /// <summary>
        /// Validates if a string is a well-formed, absolute URI with an HTTPS scheme.
        /// </summary>
        /// <param name="uriString">The string to validate.</param>
        /// <returns>True if the string is a valid HTTPS URI; otherwise, false.</returns>
        internal static bool IsValidHttpsUri(string? uriString) {
            // 1. Check for null or empty string.
            // An empty or null string is not a valid URI.
            if (string.IsNullOrWhiteSpace(uriString)) {
                return false;
            }

            // 2. Use Uri.TryCreate for safe parsing.
            // This method is secure because it doesn't throw exceptions for malformed strings.
            // We specify UriKind.Absolute to ensure it's a full URI (e.g., "https://example.com")
            // and not a relative one (e.g., "/path/to/resource").
            bool isWellFormedUri = Uri.TryCreate(uriString, UriKind.Absolute, out Uri? uriResult);

            // 3. Check the scheme.
            // If the URI was parsed successfully, we check if its scheme is HTTPS.
            // uriResult will be null if TryCreate fails.
            return isWellFormedUri && uriResult?.Scheme == Uri.UriSchemeHttps;
        }
    }
}
