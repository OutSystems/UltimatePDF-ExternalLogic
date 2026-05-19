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
        public void ApplyMetadata_CorruptInput_ReturnsInputAndDoesNotThrow() {
            // Arrange — too short to even be a PNG signature
            var input = new byte[] { 0, 1, 2 };
            var properties = new DocumentProperties { Title = "X" };
            var spy = new LoggerSpy();

            // Act
            var output = PNGMetadataUtil.ApplyMetadata(input, properties, spy);

            // Assert — early return path; no warning expected since the data is < IhdrEnd
            Assert.Same(input, output);
        }

        [Fact]
        public void ApplyMetadata_InvalidSignature_ReturnsInputUnchanged() {
            // Arrange: 33 bytes of garbage (large enough to pass length check but not a PNG)
            var input = new byte[33];
            for (int i = 0; i < input.Length; i++) {
                input[i] = (byte)i;
            }
            var properties = new DocumentProperties { Title = "X" };

            // Act
            var output = PNGMetadataUtil.ApplyMetadata(input, properties);

            // Assert
            Assert.Same(input, output);
        }
    }
}
