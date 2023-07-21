using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UltimatePDF_ExternalLogic.Utils {
    internal class UrlUtils {
        internal static string BuildUrl(string baseUrl, string module, string path) {
            var urlBuilder = new StringBuilder();
            
            if (!baseUrl.StartsWith("https")) {
                urlBuilder.Append("https://");
            }
            
            urlBuilder.Append(RemoveEndDash(baseUrl));
            
            if(!module.StartsWith("/")) {
                urlBuilder.Append("/");
            }

            urlBuilder.Append(RemoveEndDash(module));

            if (!path.StartsWith("/")) {
                urlBuilder.Append("/");
            }

            return urlBuilder.Append(path).ToString();
        }

        private static string RemoveEndDash(string str) {
            return str.EndsWith("/") ? str.Remove(str.Length-1) : str;
        }
    }
}
