using System;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace UltimatePDF_ExternalLogic.Utils;
internal class UrlUtils {
    internal static string BuildUrl(string baseUrl, string module, string path) {
        using var activity = Activity.Current?.Source.StartActivity("UrlUtils.BuildUrl");
        var urlBuilder = new StringBuilder();
        
        if (!baseUrl.StartsWith("https://") && !baseUrl.StartsWith("http://")) {
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
        using var activity = Activity.Current?.Source.StartActivity("UrlUtils.GetFilenameFromUrl");
        return new Uri(url).Segments.Last().ToString();
    }

    private static string RemoveEndDash(string str) {
        using var activity = Activity.Current?.Source.StartActivity("UrlUtils.RemoveEndDash");
        return str.EndsWith("/") ? str.Remove(str.Length-1) : str;
    }

    /// <summary>
    /// Validates if a string is a well-formed, absolute URI with an HTTP or HTTPS scheme.
    /// </summary>
    /// <param name="uriString">The string to validate.</param>
    /// <returns>True if the string is a valid HTTP or HTTPS URI; otherwise, false.</returns>
    internal static bool IsValidHttpsUri(string? uriString) {
        using var activity = Activity.Current?.Source.StartActivity("UrlUtils.IsValidHttpsUri");
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
        // uriResult will be null if TryCreate fails.
        return isWellFormedUri && (uriResult?.Scheme == Uri.UriSchemeHttps || uriResult?.Scheme == Uri.UriSchemeHttp);
    }
}
