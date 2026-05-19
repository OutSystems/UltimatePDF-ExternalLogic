using System.Diagnostics;
using System.IO;
using OutSystems.UltimatePDF_ExternalLogic.Structures;
using OutSystems.UltimatePDF_ExternalLogic.UnitTests.TestHelpers;
using OutSystems.UltimatePDF_ExternalLogic.Utils;
using PdfSharp.Pdf;

// PERF: these tests use wall-clock assertions and are noisy on shared CI runners.
// They are skipped unless the RUN_PERF_TESTS environment variable is set to "1".
// To run them locally:
//     RUN_PERF_TESTS=1 dotnet test --filter FullyQualifiedName~MetadataPerfTests

namespace OutSystems.UltimatePDF_ExternalLogic.UnitTests {

    [Trait("Category", "Perf")]
    public class MetadataPerfTests {

        private static bool PerfEnabled =>
            System.Environment.GetEnvironmentVariable("RUN_PERF_TESTS") == "1";

        private static byte[] CreateSmallPdf() {
            using var stream = new MemoryStream();
            var document = new PdfDocument();
            for (int i = 0; i < 10; i++) {
                document.AddPage();
            }
            document.Save(stream, false);
            return stream.ToArray();
        }

        [Fact]
        public void ApplyPdfMetadata_SmallInput_CompletesUnder100Ms() {
            if (!PerfEnabled) {
                return; // skipped: set RUN_PERF_TESTS=1 to enable
            }

            // Arrange
            var input = CreateSmallPdf();
            var properties = new DocumentProperties {
                Title = "Perf",
                Author = "Author",
                Producer = "Producer",
            };

            // Warm-up to avoid JIT bias on the measured call
            _ = PDFMetadataUtil.ApplyMetadata(input, properties);

            // Act
            var sw = Stopwatch.StartNew();
            var output = PDFMetadataUtil.ApplyMetadata(input, properties);
            sw.Stop();

            // Assert
            Assert.NotNull(output);
            Assert.True(sw.ElapsedMilliseconds < 100,
                $"PDF metadata embedding took {sw.ElapsedMilliseconds}ms, expected <100ms");
        }

        [Fact]
        public void ApplyPngMetadata_SmallInput_CompletesUnder100Ms() {
            if (!PerfEnabled) {
                return; // skipped: set RUN_PERF_TESTS=1 to enable
            }

            // Arrange
            var input = MinimalPngFactory.Create();
            var properties = new DocumentProperties {
                Title = "Perf",
                Author = "Author",
                Producer = "Producer",
            };

            _ = PNGMetadataUtil.ApplyMetadata(input, properties);

            // Act
            var sw = Stopwatch.StartNew();
            var output = PNGMetadataUtil.ApplyMetadata(input, properties);
            sw.Stop();

            // Assert
            Assert.NotNull(output);
            Assert.True(sw.ElapsedMilliseconds < 100,
                $"PNG metadata embedding took {sw.ElapsedMilliseconds}ms, expected <100ms");
        }
    }
}
