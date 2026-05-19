using System;
using System.Globalization;
using System.IO;
using System.Text;
using OutSystems.UltimatePDF_ExternalLogic.Management.Troubleshooting;
using OutSystems.UltimatePDF_ExternalLogic.Structures;

namespace OutSystems.UltimatePDF_ExternalLogic.Utils {
    internal static class PNGMetadataUtil {

        private static readonly byte[] PngSignature = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        private const int IhdrEnd = 33;

        /// <summary>
        /// Applies metadata properties to a PNG image by inserting tEXt / iTXt text chunks
        /// after the IHDR chunk. Returns the original bytes on any error or when input is empty.
        /// </summary>
        /// <remarks>
        /// This method is NOT idempotent: calling it twice on the same input produces duplicate
        /// text chunks (PNG decoders accept duplicates for most keywords). The library's only
        /// caller is <c>ScreenshotPNG</c>, which always passes a freshly-rendered PNG straight
        /// from PuppeteerSharp and therefore never reapplies metadata on already-tagged bytes.
        /// </remarks>
        public static byte[] ApplyMetadata(byte[] pngBytes, DocumentProperties properties, Logger? logger = null) {
            if (pngBytes == null) {
                return pngBytes;
            }

            if (pngBytes.Length < IhdrEnd) {
                logger?.Warning("PNG metadata: input too short to be a valid PNG; metadata not embedded.");
                return pngBytes;
            }

            if (properties.IsEmpty()) {
                return pngBytes;
            }

            try {
                for (int i = 0; i < PngSignature.Length; i++) {
                    if (pngBytes[i] != PngSignature[i]) {
                        logger?.Warning("PNG metadata: invalid PNG signature; metadata not embedded.");
                        return pngBytes;
                    }
                }

                if (pngBytes[12] != (byte)'I' || pngBytes[13] != (byte)'H'
                 || pngBytes[14] != (byte)'D' || pngBytes[15] != (byte)'R') {
                    logger?.Warning("PNG metadata: missing IHDR chunk; metadata not embedded.");
                    return pngBytes;
                }

                using var output = new MemoryStream(pngBytes.Length + 1024);

                output.Write(pngBytes, 0, IhdrEnd);

                WriteTextChunkIfNotEmpty(output, "Title", properties.Title);
                WriteTextChunkIfNotEmpty(output, "Author", properties.Author);
                WriteTextChunkIfNotEmpty(output, "Description", properties.Subject);
                WriteTextChunkIfNotEmpty(output, "Keywords", properties.Keywords);
                WriteTextChunkIfNotEmpty(output, "Creator", properties.Creator);
                WriteTextChunkIfNotEmpty(output, "Company", properties.Company);
                WriteTextChunkIfNotEmpty(output, "Software", properties.Producer);
                WriteTextChunkIfNotEmpty(output, "Copyright", properties.Copyright);
                WriteTextChunkIfNotEmpty(output, "Language", properties.Language);
                WriteTextChunkIfNotEmpty(output, "Source", properties.Source);

                var creationTime = DateTime.UtcNow.ToString("R", CultureInfo.InvariantCulture);
                WriteTextChunk(output, "Creation Time", creationTime);

                output.Write(pngBytes, IhdrEnd, pngBytes.Length - IhdrEnd);

                return output.ToArray();
            } catch (Exception ex) {
                logger?.Warning(ex, "Failed to embed PNG metadata; returning original bytes.");
                return pngBytes;
            }
        }

        private static void WriteTextChunkIfNotEmpty(MemoryStream output, string keyword, string text) {
            if (string.IsNullOrWhiteSpace(text)) {
                return;
            }
            WriteTextChunk(output, keyword, text);
        }

        private static void WriteTextChunk(MemoryStream output, string keyword, string text) {
            bool isAscii = true;
            for (int i = 0; i < text.Length; i++) {
                char c = text[i];
                if (c < 0x20 || c > 0x7E) {
                    isAscii = false;
                    break;
                }
            }

            if (isAscii) {
                var keywordBytes = Encoding.Latin1.GetBytes(keyword);
                var textBytes = Encoding.Latin1.GetBytes(text);
                var data = new byte[keywordBytes.Length + 1 + textBytes.Length];
                Buffer.BlockCopy(keywordBytes, 0, data, 0, keywordBytes.Length);
                data[keywordBytes.Length] = 0;
                Buffer.BlockCopy(textBytes, 0, data, keywordBytes.Length + 1, textBytes.Length);
                PngEncoding.WriteChunk(output, "tEXt", data);
            } else {
                var keywordBytes = Encoding.Latin1.GetBytes(keyword);
                var textBytes = Encoding.UTF8.GetBytes(text);
                var data = new byte[keywordBytes.Length + 1 + 1 + 1 + 1 + 1 + textBytes.Length];
                int p = 0;
                Buffer.BlockCopy(keywordBytes, 0, data, p, keywordBytes.Length);
                p += keywordBytes.Length;
                data[p++] = 0;   // keyword null separator
                data[p++] = 0;   // compression flag (uncompressed)
                data[p++] = 0;   // compression method
                data[p++] = 0;   // language tag (empty) null terminator
                data[p++] = 0;   // translated keyword (empty) null terminator
                Buffer.BlockCopy(textBytes, 0, data, p, textBytes.Length);
                PngEncoding.WriteChunk(output, "iTXt", data);
            }
        }
    }
}
