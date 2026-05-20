using System;
using System.IO;
using System.Text;

namespace OutSystems.UltimatePDF_ExternalLogic.Utils {
    /// <summary>
    /// Low-level PNG chunk encoding helpers. Centralized so the production code and the
    /// PNG fixtures in the unit-test project share a single implementation of CRC32 and
    /// big-endian length / type / data / CRC framing.
    /// </summary>
    internal static class PngEncoding {

        /// <summary>
        /// Writes one PNG chunk (length, type, data, CRC32 over type+data) to the output stream.
        /// </summary>
        internal static void WriteChunk(MemoryStream output, string type, byte[] data) {
            var typeBytes = Encoding.ASCII.GetBytes(type);

            WriteUInt32BigEndian(output, (uint)data.Length);
            output.Write(typeBytes, 0, typeBytes.Length);
            output.Write(data, 0, data.Length);

            var crcInput = new byte[typeBytes.Length + data.Length];
            Buffer.BlockCopy(typeBytes, 0, crcInput, 0, typeBytes.Length);
            Buffer.BlockCopy(data, 0, crcInput, typeBytes.Length, data.Length);
            WriteUInt32BigEndian(output, Crc32(crcInput));
        }

        /// <summary>
        /// Writes a 32-bit unsigned integer in big-endian order (PNG chunk length / CRC layout).
        /// </summary>
        internal static void WriteUInt32BigEndian(MemoryStream output, uint value) {
            output.WriteByte((byte)(value >> 24));
            output.WriteByte((byte)(value >> 16));
            output.WriteByte((byte)(value >> 8));
            output.WriteByte((byte)value);
        }

        /// <summary>
        /// Computes the CRC-32 used by PNG chunks (polynomial 0xEDB88320, initial 0xFFFFFFFF, output XOR'd with 0xFFFFFFFF).
        /// </summary>
        internal static uint Crc32(byte[] data) {
            uint crc = 0xFFFFFFFFu;
            for (int i = 0; i < data.Length; i++) {
                crc ^= data[i];
                for (int j = 0; j < 8; j++) {
                    crc = (crc & 1u) != 0 ? (crc >> 1) ^ 0xEDB88320u : (crc >> 1);
                }
            }
            return ~crc;
        }
    }
}
