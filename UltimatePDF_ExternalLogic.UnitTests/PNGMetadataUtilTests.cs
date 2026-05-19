using System.Collections.Generic;
using System.Linq;
using OutSystems.UltimatePDF_ExternalLogic.Structures;
using OutSystems.UltimatePDF_ExternalLogic.UnitTests.TestHelpers;
using OutSystems.UltimatePDF_ExternalLogic.Utils;

namespace OutSystems.UltimatePDF_ExternalLogic.UnitTests {
    public class PNGMetadataUtilTests {

        private static Dictionary<string, string> CollectTextChunks(byte[] png) {
            var result = new Dictionary<string, string>();
            foreach (var chunk in PngChunkReader.ReadAll(png)) {
                if (chunk.Type == "tEXt") {
                    var (k, v) = PngChunkReader.DecodeTextChunk(chunk);
                    result[k] = v;
                } else if (chunk.Type == "iTXt") {
                    var (k, v) = PngChunkReader.DecodeITextChunk(chunk);
                    result[k] = v;
                }
            }
            return result;
        }

        [Fact]
        public void ApplyMetadata_AllFieldsPopulated_WritesAllChunks() {
            // Arrange
            var input = MinimalPngFactory.Create();
            var properties = new DocumentProperties {
                Title = "Q1 Report",
                Author = "Acme",
                Subject = "Quarterly results",
                Keywords = "finance, q1",
                Creator = "Ultimate PDF",
                Company = "Acme Corp",
                Producer = "Ultimate PDF 1.x",
                Copyright = "(c) 2026 Acme",
                Language = "en-US",
                Source = "ERP-Prod",
            };

            // Act
            var output = PNGMetadataUtil.ApplyMetadata(input, properties);

            // Assert
            var chunks = CollectTextChunks(output);
            Assert.Equal("Q1 Report", chunks["Title"]);
            Assert.Equal("Acme", chunks["Author"]);
            Assert.Equal("Quarterly results", chunks["Description"]);
            Assert.Equal("finance, q1", chunks["Keywords"]);
            Assert.Equal("Ultimate PDF", chunks["Creator"]);
            Assert.Equal("Acme Corp", chunks["Company"]);
            Assert.Equal("Ultimate PDF 1.x", chunks["Software"]);
            Assert.Equal("(c) 2026 Acme", chunks["Copyright"]);
            Assert.Equal("en-US", chunks["Language"]);
            Assert.Equal("ERP-Prod", chunks["Source"]);
            Assert.True(chunks.ContainsKey("Creation Time"));
        }

        [Fact]
        public void ApplyMetadata_AllFieldsEmpty_ReturnsInputUnchanged() {
            // Arrange
            var input = MinimalPngFactory.Create();
            var properties = new DocumentProperties();

            // Act
            var output = PNGMetadataUtil.ApplyMetadata(input, properties);

            // Assert
            Assert.Same(input, output);
        }

        [Fact]
        public void ApplyMetadata_MixedFields_WritesOnlyPopulatedChunks() {
            // Arrange
            var input = MinimalPngFactory.Create();
            var properties = new DocumentProperties {
                Title = "Only Title",
                Author = "Only Author",
            };

            // Act
            var output = PNGMetadataUtil.ApplyMetadata(input, properties);

            // Assert
            var chunks = CollectTextChunks(output);
            Assert.Equal("Only Title", chunks["Title"]);
            Assert.Equal("Only Author", chunks["Author"]);
            Assert.False(chunks.ContainsKey("Description"));
            Assert.False(chunks.ContainsKey("Keywords"));
            Assert.False(chunks.ContainsKey("Software"));
            Assert.False(chunks.ContainsKey("Copyright"));
            Assert.False(chunks.ContainsKey("Source"));
            Assert.False(chunks.ContainsKey("Language"));
        }

        [Fact]
        public void ApplyMetadata_NonAscii_UsesITextChunk() {
            // Arrange
            var input = MinimalPngFactory.Create();
            var properties = new DocumentProperties {
                Title = "Relatório Trimestral",
                Author = "José",
                Subject = "Pure ASCII subject",
            };

            // Act
            var output = PNGMetadataUtil.ApplyMetadata(input, properties);

            // Assert
            var chunks = PngChunkReader.ReadAll(output);
            var titleChunk = chunks.Single(c =>
                (c.Type == "tEXt" || c.Type == "iTXt") &&
                ((c.Type == "tEXt" ? PngChunkReader.DecodeTextChunk(c).keyword
                                  : PngChunkReader.DecodeITextChunk(c).keyword) == "Title"));
            Assert.Equal("iTXt", titleChunk.Type);
            Assert.Equal("Relatório Trimestral", PngChunkReader.DecodeITextChunk(titleChunk).text);

            var subjectChunk = chunks.Single(c =>
                (c.Type == "tEXt" || c.Type == "iTXt") &&
                ((c.Type == "tEXt" ? PngChunkReader.DecodeTextChunk(c).keyword
                                  : PngChunkReader.DecodeITextChunk(c).keyword) == "Description"));
            Assert.Equal("tEXt", subjectChunk.Type);
            Assert.Equal("Pure ASCII subject", PngChunkReader.DecodeTextChunk(subjectChunk).text);
        }

        [Fact]
        public void ApplyMetadata_TooShort_ReturnsInputAndLogsWarning() {
            // Arrange — exercises the length-check branch (input shorter than IhdrEnd)
            var input = new byte[] { 0, 1, 2 };
            var properties = new DocumentProperties { Title = "X" };
            var spy = new LoggerSpy();

            // Act
            var output = PNGMetadataUtil.ApplyMetadata(input, properties, spy);

            // Assert
            Assert.Same(input, output);
            Assert.Equal(1, spy.WarningCalls);
        }

        [Fact]
        public void ApplyMetadata_InvalidSignature_ReturnsInputAndLogsWarning() {
            // Arrange: 33 bytes of garbage (large enough to pass length check but not a PNG)
            var input = new byte[33];
            for (int i = 0; i < input.Length; i++) {
                input[i] = (byte)i;
            }
            var properties = new DocumentProperties { Title = "X" };
            var spy = new LoggerSpy();

            // Act
            var output = PNGMetadataUtil.ApplyMetadata(input, properties, spy);

            // Assert
            Assert.Same(input, output);
            Assert.Equal(1, spy.WarningCalls);
        }

        [Fact]
        public void ApplyMetadata_MissingIHDR_ReturnsInputAndLogsWarning() {
            // Arrange — 33 bytes: valid PNG signature (8 bytes) + 4-byte length + 4-byte type that is NOT "IHDR"
            var input = new byte[33];
            // PNG signature
            byte[] signature = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
            System.Buffer.BlockCopy(signature, 0, input, 0, 8);
            // Bytes 12-15 contain "XXXX" instead of "IHDR"
            input[12] = (byte)'X';
            input[13] = (byte)'X';
            input[14] = (byte)'X';
            input[15] = (byte)'X';
            var properties = new DocumentProperties { Title = "X" };
            var spy = new LoggerSpy();

            // Act
            var output = PNGMetadataUtil.ApplyMetadata(input, properties, spy);

            // Assert
            Assert.Same(input, output);
            Assert.Equal(1, spy.WarningCalls);
        }

        [Fact]
        public void ApplyMetadata_TitleWithEmbeddedNul_PinsCurrentBehavior() {
            // Pin test: when a field value contains an embedded NUL byte, the current
            // tEXt encoder treats the input as ASCII (NUL is < 0x20, which actually
            // makes it non-printable; the isAscii check uses < 0x20 OR > 0x7E so NUL
            // forces the iTXt path). The embedded NUL is written verbatim into the
            // chunk data. The PNG spec discourages NULs in text values; this pin
            // exists so a deliberate future change (e.g. sanitizing NULs, rejecting
            // the field) shows up as an intentional shift in observable behavior.
            // Arrange
            var input = MinimalPngFactory.Create();
            var properties = new DocumentProperties {
                Title = "Bad\0Title",
            };

            // Act
            var output = PNGMetadataUtil.ApplyMetadata(input, properties);

            // Assert
            var chunks = CollectTextChunks(output);
            Assert.True(chunks.ContainsKey("Title"));
            // Embedded NUL is preserved verbatim in the decoded chunk text.
            Assert.Equal("Bad\0Title", chunks["Title"]);
        }

        [Fact]
        public void ApplyMetadata_OnlyWhitespaceFields_TreatedAsEmpty() {
            // Arrange — every field whitespace; IsEmpty() must treat the whole struct as empty
            var input = MinimalPngFactory.Create();
            var properties = new DocumentProperties {
                Title = "   ",
                Author = "\t",
                Subject = "\n",
                Keywords = " ",
                Creator = "  ",
                Company = "\r\n",
                Producer = "    ",
                Copyright = " ",
                Language = " ",
                Source = " ",
            };

            // Act
            var output = PNGMetadataUtil.ApplyMetadata(input, properties);

            // Assert
            Assert.Same(input, output);
        }

        [Fact]
        public void ApplyMetadata_TitleWithUtf8Bom_RoundTrips() {
            // Arrange — Title contains a U+FEFF zero-width no-break space (UTF-8 BOM)
            // which is a non-ASCII character, so the iTXt path is used.
            var input = MinimalPngFactory.Create();
            var properties = new DocumentProperties {
                Title = "﻿Actual Title",
            };

            // Act
            var output = PNGMetadataUtil.ApplyMetadata(input, properties);

            // Assert
            var chunks = PngChunkReader.ReadAll(output);
            var titleChunk = chunks.Single(c =>
                c.Type == "iTXt" &&
                PngChunkReader.DecodeITextChunk(c).keyword == "Title");
            Assert.Equal("﻿Actual Title", PngChunkReader.DecodeITextChunk(titleChunk).text);
        }
    }
}
