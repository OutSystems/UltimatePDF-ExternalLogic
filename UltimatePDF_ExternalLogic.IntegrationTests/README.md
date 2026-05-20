# UltimatePDF_ExternalLogic.IntegrationTests

xUnit v3 integration tests that drive the public `PrintPDF` entry point end-to-end:
URL → headless Chromium → PDF → metadata embedding. The HTML under test is served
in-process by a minimal ASP.NET Core `WebApplication` on a dynamic loopback port,
so the suite has no public-network dependency beyond Chromium acquisition by the
SUT library.

Two tests live here:

- **`PrintPDF_HelloWorld_ReturnsValidPdf`** — asserts the returned byte array starts
  with `%PDF-`, is larger than 1000 bytes, and opens with PdfSharp with at least
  one page.
- **`PrintPDF_WithDocumentProperties_EmbedsAllMetadata`** — passes a fully
  populated `DocumentProperties` and asserts all 10 fields round-trip into the
  PDF's Info dictionary / Catalog via PdfSharp.

## Prerequisites

- .NET 8 SDK to build / publish.
- One of:
  - .NET 8 runtime on the host (to run via `dotnet test`), **or**
  - Docker to run inside a container that ships Chromium OS dependencies
    (`libnss3`, `libatk-1.0`, `fontconfig`, …).

## Run on the host

From the repository root:

```bash
dotnet test UltimatePDF_ExternalLogic.IntegrationTests/UltimatePDF_ExternalLogic.IntegrationTests.csproj
```

To run alongside the unit tests:

```bash
dotnet test UltimatePDF_ExternalLogic.sln
```

The fixture binds to `http://127.0.0.1:0` and reads the assigned port back from
`IServerAddressesFeature`, so it does not collide with other listeners on the
host.

## Run inside a container

The tests are intended to run inside the AWS Lambda .NET 8 image, which is the
same family the SUT's `HeadlessChromium.Puppeteer.Lambda.Dotnet` package was
built against and ships the required Chromium OS dependencies.

### 1. Publish

```bash
dotnet publish UltimatePDF_ExternalLogic.IntegrationTests/UltimatePDF_ExternalLogic.IntegrationTests.csproj \
  -c Release -r linux-x64 --no-self-contained \
  -o ./IntegrationTests.Publish
```

### 2. Run

```bash
docker run --rm \
  --entrypoint /tests/UltimatePDF_ExternalLogic.IntegrationTests \
  -v "$PWD/IntegrationTests.Publish":/tests \
  -w /tests \
  -e HOME=/tmp \
  public.ecr.aws/lambda/dotnet:8.2026.05.19.13
```

Two flags worth calling out:

- `--entrypoint` overrides the image's default `/lambda-entrypoint.sh` and points
  at the published xunit.v3 apphost. xunit.v3 builds an in-proc test runner into
  the test assembly, so the .NET SDK is not required inside the image.
- `HOME=/tmp` gives the headless Chromium launcher a writable directory for its
  download / cache files.

Expected output (≈6 s on first run):

```
xUnit.net v3 In-Process Runner v3.2.2 (64-bit .NET 8.0.x)
  Discovering: UltimatePDF_ExternalLogic.IntegrationTests
  Discovered:  UltimatePDF_ExternalLogic.IntegrationTests
  Starting:    UltimatePDF_ExternalLogic.IntegrationTests
  Finished:    UltimatePDF_ExternalLogic.IntegrationTests
=== TEST EXECUTION SUMMARY ===
   UltimatePDF_ExternalLogic.IntegrationTests  Total: 2, Errors: 0, Failed: 0, Skipped: 0, Not Run: 0
```

### Running against a non-net8 image

When running inside an image whose runtime is .NET 10 (or any major version
ahead of net8.0), add `-e DOTNET_ROLL_FORWARD=Major` so the published net8
binaries roll forward to the available runtime. The Lambda dotnet:8 image
above is native .NET 8, so no roll-forward flag is needed.

## Notes

- The fixture uses `WebApplication.CreateEmptyBuilder` with explicit
  `UseKestrelCore()` + `AddRoutingCore()` so it does not register the default
  JSON-config file watchers. This avoids `IOException: configured user limit
  on the number of inotify instances has been reached` on hosts with a low
  `fs.inotify.max_user_instances` limit (WSL2 and many container hosts).
- Chromium itself is acquired on demand by
  `HeadlessChromium.Puppeteer.Lambda.Dotnet`; no manual install step is
  required.
