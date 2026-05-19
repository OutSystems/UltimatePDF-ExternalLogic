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
        public static byte[] ApplyMetadata(byte[] pngBytes, DocumentProperties properties, Logger? logger = null) {
            if (pngBytes == null || pngBytes.Length < IhdrEnd) {
                return pngBytes;
            }

            if (IsEmptyProperties(properties)) {
                return pngBytes;
            }

            try {
                for (int i = 0; i < PngSignature.Length; i++) {
                    if (pngBytes[i] != PngSignature[i]) {
                        return pngBytes;
                    }
                }

                if (pngBytes[12] != (byte)'I' || pngBytes[13] != (byte)'H'
                 || pngBytes[14] != (byte)'D' || pngBytes[15] != (byte)'R') {
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

                var creationTime = DateTime.Now.ToString("R", CultureInfo.InvariantCulture);
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
                WriteChunk(output, "tEXt", data);
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
                WriteChunk(output, "iTXt", data);
            }
        }

        private static void WriteChunk(MemoryStream output, string type, byte[] data) {
            var typeBytes = Encoding.ASCII.GetBytes(type);

            WriteUInt32BigEndian(output, (uint)data.Length);
            output.Write(typeBytes, 0, typeBytes.Length);
            output.Write(data, 0, data.Length);

            var crcInput = new byte[typeBytes.Length + data.Length];
            Buffer.BlockCopy(typeBytes, 0, crcInput, 0, typeBytes.Length);
            Buffer.BlockCopy(data, 0, crcInput, typeBytes.Length, data.Length);
            WriteUInt32BigEndian(output, Crc32(crcInput));
        }

        private static void WriteUInt32BigEndian(MemoryStream output, uint value) {
            output.WriteByte((byte)(value >> 24));
            output.WriteByte((byte)(value >> 16));
            output.WriteByte((byte)(value >> 8));
            output.WriteByte((byte)value);
        }

        private static uint Crc32(byte[] data) {
            uint crc = 0xFFFFFFFFu;
            for (int i = 0; i < data.Length; i++) {
                crc ^= data[i];
                for (int j = 0; j < 8; j++) {
                    crc = (crc & 1u) != 0 ? (crc >> 1) ^ 0xEDB88320u : (crc >> 1);
                }
            }
            return ~crc;
        }

        private static bool IsEmptyProperties(DocumentProperties properties) {
            return string.IsNullOrWhiteSpace(properties.Title) &&
                   string.IsNullOrWhiteSpace(properties.Author) &&
                   string.IsNullOrWhiteSpace(properties.Subject) &&
                   string.IsNullOrWhiteSpace(properties.Keywords) &&
                   string.IsNullOrWhiteSpace(properties.Creator) &&
                   string.IsNullOrWhiteSpace(properties.Company) &&
                   string.IsNullOrWhiteSpace(properties.Producer) &&
                   string.IsNullOrWhiteSpace(properties.Copyright) &&
                   string.IsNullOrWhiteSpace(properties.Language) &&
                   string.IsNullOrWhiteSpace(properties.Source);
        }
    }
}
