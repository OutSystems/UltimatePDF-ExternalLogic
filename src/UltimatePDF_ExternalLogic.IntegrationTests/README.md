# UltimatePDF_ExternalLogic.IntegrationTests

xUnit v3 integration tests that drive the public `PrintPDF` entry point end-to-end:
URL → headless Chromium → PDF → metadata embedding. The HTML under test is served
in-process by a minimal ASP.NET Core `WebApplication` on a dynamic loopback port,
so the suite has no public-network dependency beyond Chromium acquisition by the
UltimatePDF library.

19 tests across three test classes:

**`PrintPDFIntegrationTests`** (11 tests) — drives the `PrintPDF` and `PrintPDF_ToS3` entry
points end-to-end, covering: basic rendering, document-property embedding, custom paper/margins,
cookies, collect-logs modes, S3 presigned-URL upload, locale, and timezone.

**`PrintPDFToRestIntegrationTests`** (4 tests) — drives `PrintPDF_ToRest` against an
in-process loopback stub, covering: happy path, collect-logs, attach-files-logs, and 4xx
error-response logging.

**`ScreenshotPNGIntegrationTests`** (4 tests) — drives `ScreenshotPNG`, covering: basic
rendering, PNG metadata embedding (verified via tEXt/iTXt chunk decode), and both log modes.

## Prerequisites

- .NET 10 SDK to build / publish.
- One of:
  - .NET 10 runtime on the host (to run the built test executable directly), **or**
  - Docker to run inside a container that ships Chromium OS dependencies
    (`libnss3`, `libatk-1.0`, `fontconfig`, …).

## Run on the host

From the repository root (see the main [CONTRIBUTING.md](../../CONTRIBUTING.md#running-tests) for why
this project runs via the built executable rather than `dotnet test` directly):

```bash
dotnet build src/UltimatePDF_ExternalLogic.IntegrationTests/UltimatePDF_ExternalLogic.IntegrationTests.csproj -c Release
dotnet exec src/UltimatePDF_ExternalLogic.IntegrationTests/bin/Release/net10.0/UltimatePDF_ExternalLogic.IntegrationTests.dll
```

The fixture binds to `http://127.0.0.1:0` and reads the assigned port back from
`IServerAddressesFeature`, so it does not collide with other listeners on the
host.

## Run inside a container

The tests are intended to run inside the AWS Lambda .NET 8 image, which is the
same family the `HeadlessChromium.Puppeteer.Dotnet.OS.Fork` package was
built against and ships the required Chromium OS dependencies.

### 1. Publish

```bash
dotnet publish src/UltimatePDF_ExternalLogic.IntegrationTests/UltimatePDF_ExternalLogic.IntegrationTests.csproj \
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
xUnit.net v3 In-Process Runner v4.0.1 (64-bit .NET 8.0.x)
  Discovering: UltimatePDF_ExternalLogic.IntegrationTests
  Discovered:  UltimatePDF_ExternalLogic.IntegrationTests
  Starting:    UltimatePDF_ExternalLogic.IntegrationTests
  Finished:    UltimatePDF_ExternalLogic.IntegrationTests
=== TEST EXECUTION SUMMARY ===
   UltimatePDF_ExternalLogic.IntegrationTests  Total: 19, Errors: 0, Failed: 0, Skipped: 0, Not Run: 0
```

> **Known gap (confirmed broken)**: this project now targets `net10.0` (see the root
> `Directory.Build.props`), but the Lambda container image above is still `.NET 8`-based (AWS has not
> shipped a .NET 10 Lambda base image at the time of writing). Verified directly: publishing and
> running the steps above fails with
> `You must install or update .NET ... Framework: 'Microsoft.NETCore.App', version '10.0.0' ... The
> following frameworks were found: 8.0.27`, with or without `DOTNET_ROLL_FORWARD=Major` (roll-forward
> only lets a published app run on a *newer* installed runtime than it targets, never an older one, so
> it can't bridge a `net10.0` app onto a `net8.0` runtime). The container workflow above does not work
> until AWS ships a .NET 10 Lambda base image; use the host-based run in the previous section instead.

## Notes

- The fixture uses `WebApplication.CreateEmptyBuilder` with explicit
  `UseKestrelCore()` + `AddRoutingCore()` so it does not register the default
  JSON-config file watchers. This avoids `IOException: configured user limit
  on the number of inotify instances has been reached` on hosts with a low
  `fs.inotify.max_user_instances` limit (WSL2 and many container hosts).
- Chromium itself is acquired on demand by
  `HeadlessChromium.Puppeteer.Dotnet.OS.Fork`; no manual install step is
  required.
