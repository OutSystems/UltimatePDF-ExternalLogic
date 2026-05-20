using System.Collections.Generic;
using System.Text;

namespace OutSystems.UltimatePDF_ExternalLogic.UnitTests.TestHelpers {

    internal readonly record struct PngChunk(string Type, byte[] Data);

    internal static class PngChunkReader {

        public static List<PngChunk> ReadAll(byte[] png) {
            var chunks = new List<PngChunk>();
            int offset = 8; // skip signature
            while (offset + 12 <= png.Length) {
                int length = ReadUInt32BigEndian(png, offset);
                offset += 4;
                var type = Encoding.ASCII.GetString(png, offset, 4);
                offset += 4;
                var data = new byte[length];
                System.Buffer.BlockCopy(png, offset, data, 0, length);
                offset += length;
                offset += 4; // skip CRC
                chunks.Add(new PngChunk(type, data));
                if (type == "IEND") {
                    break;
                }
            }
            return chunks;
        }

        public static (string keyword, string text) DecodeTextChunk(PngChunk chunk) {
            int nulIdx = System.Array.IndexOf(chunk.Data, (byte)0);
            var keyword = Encoding.Latin1.GetString(chunk.Data, 0, nulIdx);
            var text = Encoding.Latin1.GetString(chunk.Data, nulIdx + 1, chunk.Data.Length - nulIdx - 1);
            return (keyword, text);
        }

        public static (string keyword, string text) DecodeITextChunk(PngChunk chunk) {
            int p = 0;
            int nulIdx = System.Array.IndexOf(chunk.Data, (byte)0);
            var keyword = Encoding.Latin1.GetString(chunk.Data, 0, nulIdx);
            p = nulIdx + 1;
            p += 2; // compression flag + method
            // language tag (null-terminated)
            int langEnd = System.Array.IndexOf(chunk.Data, (byte)0, p);
            p = langEnd + 1;
            // translated keyword (null-terminated)
            int transEnd = System.Array.IndexOf(chunk.Data, (byte)0, p);
            p = transEnd + 1;
            var text = Encoding.UTF8.GetString(chunk.Data, p, chunk.Data.Length - p);
            return (keyword, text);
        }

        private static int ReadUInt32BigEndian(byte[] buf, int offset) {
            return (buf[offset] << 24) | (buf[offset + 1] << 16) | (buf[offset + 2] << 8) | buf[offset + 3];
        }
    }
}
