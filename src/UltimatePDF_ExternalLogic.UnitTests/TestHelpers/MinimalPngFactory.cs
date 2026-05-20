using System;
using System.IO;
using System.IO.Compression;
using OutSystems.UltimatePDF_ExternalLogic.Utils;

namespace OutSystems.UltimatePDF_ExternalLogic.UnitTests.TestHelpers {

    /// <summary>
    /// Synthesizes a minimal but structurally valid 1x1 RGBA PNG for tests. Reuses the
    /// production <see cref="PngEncoding"/> helpers so the resulting file is byte-for-byte
    /// identical to what production code would emit for the same chunks.
    /// </summary>
    internal static class MinimalPngFactory {

        public static byte[] Create() {
            using var output = new MemoryStream();
            // signature
            output.Write(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A });

            // IHDR: 1x1, 8-bit, RGBA
            var ihdr = new byte[] {
                0x00, 0x00, 0x00, 0x01, // width 1
                0x00, 0x00, 0x00, 0x01, // height 1
                0x08,                   // bit depth 8
                0x06,                   // color type RGBA
                0x00, 0x00, 0x00,       // compression, filter, interlace
            };
            PngEncoding.WriteChunk(output, "IHDR", ihdr);

            // IDAT: 1 pixel = filter byte (0) + R G B A (4 bytes) = 5 raw bytes
            // wrapped in zlib (deflate) — easiest: use DeflateStream + manual zlib header/Adler32
            byte[] rawScanline = new byte[] { 0x00, 0xFF, 0x00, 0x00, 0xFF };
            byte[] zlib = ZlibWrap(rawScanline);
            PngEncoding.WriteChunk(output, "IDAT", zlib);

            // IEND
            PngEncoding.WriteChunk(output, "IEND", Array.Empty<byte>());

            return output.ToArray();
        }

        private static byte[] ZlibWrap(byte[] raw) {
            using var ms = new MemoryStream();
            ms.WriteByte(0x78); // CMF: deflate, 32K window
            ms.WriteByte(0x9C); // FLG
            using (var deflate = new DeflateStream(ms, CompressionLevel.Fastest, leaveOpen: true)) {
                deflate.Write(raw, 0, raw.Length);
            }
            uint adler = Adler32(raw);
            ms.WriteByte((byte)(adler >> 24));
            ms.WriteByte((byte)(adler >> 16));
            ms.WriteByte((byte)(adler >> 8));
            ms.WriteByte((byte)adler);
            return ms.ToArray();
        }

        private static uint Adler32(byte[] data) {
            uint a = 1, b = 0;
            const uint MOD = 65521;
            for (int i = 0; i < data.Length; i++) {
                a = (a + data[i]) % MOD;
                b = (b + a) % MOD;
            }
            return (b << 16) | a;
        }
    }
}
