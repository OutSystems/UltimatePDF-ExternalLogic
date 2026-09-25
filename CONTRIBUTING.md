# Contributing to Ultimate PDF External Logic

Thank you for your interest in contributing to Ultimate PDF External Logic. This document provides guidelines for contributing to this OutSystems Developer Cloud (ODC) external logic component.

## Development Setup

### Prerequisites

- .NET SDK 10.0 or later (project targets `net10.0`)
- PowerShell (for package generation script)
- Git for version control
- Docker (for running integration tests in a container)
- An OutSystems ODC tenant for testing

### Installation

1. Fork the repository from https://github.com/OutSystems/UltimatePDF-ExternalLogic
2. Clone your fork locally:
   ```bash
   git clone https://github.com/YOUR_USERNAME/UltimatePDF-ExternalLogic.git
   cd UltimatePDF-ExternalLogic
   ```
3. Open `src/UltimatePDF_ExternalLogic.sln` in your preferred C# IDE (Visual Studio, Rider, or VS Code)

## Development Workflow

### Branch Naming

Create feature branches from `main` using descriptive names. Based on repository history, common patterns include:
- Issue/ticket references: `RDV-1406/log-improvements`, `rdv-1643-remove-unused-code`
- Descriptive names: `update-readme`, `net8`, `lambda-functions-test`
- Feature branches: `rdv-1438/fonts-as-resource`

### Commit Messages

Write clear, descriptive commit messages that explain what changed and why. Examples from the repository:
- "Added the use of ILogger to the code"
- "Changed the URI validation to also accept HTTP"
- "Update libraries HeadlessChromium.Puppeteer.Lambda.Dotnet and PDFsharp"
- "Fixed white spaces and line breaks inside expressions"

Keep commits focused on a single logical change. Document code changes thoroughly with inline comments.

### Code Standards

This is a C# project targeting .NET 10.0 with nullable reference types, implicit usings, and
analyzers (`AnalysisLevel=latest-recommended`) enabled solution-wide via the root `Directory.Build.props`.
A root `.editorconfig` codifies the codebase's formatting conventions. Follow standard C# conventions:
- Use meaningful variable and method names
- Add XML documentation comments for public interfaces and methods (see `IUltimatePDF_ExternalLogic.cs` for examples)
- Use `OutSystems.ExternalLibraries.SDK` attributes appropriately (`[OSInterface]`, `[OSAction]`, `[OSParameter]`)
- Maintain consistent code formatting with the existing codebase - run `dotnet format whitespace` and
  `dotnet format style` before submitting if your editor doesn't apply `.editorconfig` automatically

Key dependencies:
- `OutSystems.ExternalLibraries.SDK` (v1.7.0) - Core external logic framework
- `HeadlessChromium.Puppeteer.Dotnet.OS.Fork` (v1.2.1.2) - Browser automation (Chromium binaries for AWS Lambda)
- `PDFsharp` (v6.2.4) - PDF manipulation

## Building and Testing

### Build the External Logic Package

Generate the external logic ZIP package for ODC deployment:

```powershell
.\generate_upload_package.ps1
```

This script:
1. Removes any existing `UltimatePDF_ExternalLogic.zip`
2. Publishes the project for Linux x64 runtime
3. Creates a ZIP archive ready for ODC upload

### Manual Build

For development builds without packaging:

```bash
dotnet build src/UltimatePDF_ExternalLogic.sln
dotnet build src/UltimatePDF_ExternalLogic.sln -c Release
dotnet publish src/UltimatePDF_ExternalLogic.sln -c Release -r linux-x64 --self-contained false
```

### Running Tests

The solution uses xUnit v3 on top of .NET 10's Microsoft.Testing.Platform (MTP), configured via the
root `global.json` (`{"test": {"runner": "Microsoft.Testing.Platform"}}`). This changes how `dotnet test`
behaves compared to older .NET SDKs:

- `dotnet test` now takes `--project <csproj>` or `--solution <sln>` instead of a positional path.
- **Recommended**: build once, then run the produced test executable directly - this is the most
  reliable option and works identically for every project in the solution:
  ```bash
  dotnet build src/UltimatePDF_ExternalLogic.sln -c Release
  dotnet exec src/UltimatePDF_ExternalLogic.UnitTests/bin/Release/net10.0/UltimatePDF_ExternalLogic.UnitTests.dll
  dotnet exec src/UltimatePDF_ExternalLogic.IntegrationTests/bin/Release/net10.0/UltimatePDF_ExternalLogic.IntegrationTests.dll
  ```
- `dotnet test --project <csproj>` also works, but as of this writing it reliably hangs *after* all
  tests have already passed for `IntegrationTests`/`E2ETests` specifically (both host a real Kestrel
  web server via `LoopbackWebHostFactory`/`WebApplicationFactory`) - an interaction between the MTP
  `dotnet test` orchestrator and the hosted web server, not a bug in this repo's fixtures (they already
  dispose their `WebApplication` instances correctly). It's fine for `UnitTests`, which has no web host.
  If you hit this, redirect subprocess output to a file rather than piping through another command
  (e.g. `| tail`) to rule out output buffering before assuming it's actually hung.

### Unit Tests

```bash
dotnet exec src/UltimatePDF_ExternalLogic.UnitTests/bin/Release/net10.0/UltimatePDF_ExternalLogic.UnitTests.dll
```

### Integration Tests

The integration test project (`UltimatePDF_ExternalLogic.IntegrationTests`) exercises the public API end-to-end: a real URL is fed through headless Chromium and the output PDF is validated. The HTML under test is served in-process on a dynamic loopback port, so there is no public-network dependency beyond Chromium.

#### Run on the Host

Requires the Chromium OS shared libraries (`libnss3`, `libatk-1.0`, `fontconfig`, …) to be present, which is the case on most Linux hosts and macOS. On Windows, prefer the container path below.

```bash
dotnet build src/UltimatePDF_ExternalLogic.IntegrationTests/UltimatePDF_ExternalLogic.IntegrationTests.csproj -c Release
dotnet exec src/UltimatePDF_ExternalLogic.IntegrationTests/bin/Release/net10.0/UltimatePDF_ExternalLogic.IntegrationTests.dll
```

#### Run in a Container

The integration tests are designed to run inside the AWS Lambda .NET 10 image — the same family that `HeadlessChromium.Puppeteer.Dotnet.OS.Fork` was built against and that ships all required Chromium OS dependencies. This matches the ODC production environment.

**Step 1 — Publish the test assembly**

```bash
dotnet publish src/UltimatePDF_ExternalLogic.IntegrationTests/UltimatePDF_ExternalLogic.IntegrationTests.csproj \
  -c Release -r linux-x64 --no-self-contained \
  -o ./IntegrationTests.Publish
```

**Step 2 — Run inside the Lambda image**

```bash
docker run --rm \
  --entrypoint /tests/UltimatePDF_ExternalLogic.IntegrationTests \
  -v "$PWD/IntegrationTests.Publish":/tests \
  -w /tests \
  -e HOME=/tmp \
  public.ecr.aws/lambda/dotnet:10.2026.09.24.16
```

Two flags worth noting:

- `--entrypoint` overrides the image's default `/lambda-entrypoint.sh` and points at the xUnit v3 apphost embedded in the test assembly. The .NET SDK is not required inside the image.
- `HOME=/tmp` gives the headless Chromium launcher a writable directory for download/cache files.

Expected output (verified directly against this image):

```
xUnit.net v3 In-Process Runner v4.0.1 (64-bit .NET 10.0.12)
  Discovering: UltimatePDF_ExternalLogic.IntegrationTests
  Discovered:  UltimatePDF_ExternalLogic.IntegrationTests
  Starting:    UltimatePDF_ExternalLogic.IntegrationTests
  Finished:    UltimatePDF_ExternalLogic.IntegrationTests
=== TEST EXECUTION SUMMARY ===
   UltimatePDF_ExternalLogic.IntegrationTests  Total: 19, Errors: 0, Failed: 0, Skipped: 0, Not Run: 0
```

### Tenant Tests

The `UltimatePDF_ExternalLogic.E2ETests` project runs smoke tests against a real, deployed ODC tenant. It calls the `UltimatePDFTests` REST API exposed by `oml/Ultimate PDF Tests.oml` and validates the end-to-end response — including PDF byte signatures and embedded metadata. These tests require a live ODC environment and valid API credentials; they are not run in the container path above.

#### Configure `appsettings.json`

A template file is provided at
`src/UltimatePDF_ExternalLogic.E2ETests/appsettings.template.json`. Copy it to
`appsettings.json` in the same directory and fill in your tenant details (the copy is gitignored
and will never be committed):

```bash
cp src/UltimatePDF_ExternalLogic.E2ETests/appsettings.template.json \
   src/UltimatePDF_ExternalLogic.E2ETests/appsettings.json
# then edit appsettings.json with real values
```

```json
{
  "TenantEndpoint": "https://<your-tenant>.outsystems.dev",
  "ApiClientId": "<ODC Portal API client ID>",
  "ApiClientSecret": "<ODC Portal API client secret>",
  "TestPageUrl": "https://<any reachable URL to render as PDF>"
}
```

| Field | Where to find it |
|-------|-----------------|
| `TenantEndpoint` | Your ODC tenant base URL, e.g. `https://my-org.outsystems.dev` |
| `ApiClientId` / `ApiClientSecret` | ODC Portal → **Users** → **Service Accounts** → create or copy an existing account with _Environment Configuration_ and _Deployment_ API scopes |
| `TestPageUrl` | Any HTTPS page reachable from ODC (e.g. `https://google.com` or your tenant app URL) |

> **Security:** `appsettings.json` contains secrets. It is listed in `.gitignore` — never commit
> it. For CI pipelines, override individual values with environment variables (the fixture calls
> `AddEnvironmentVariables()` after the JSON file, so any key such as `ApiClientSecret` can be
> set as an environment variable with the same name).

#### Run Tenant Tests

```bash
dotnet build src/UltimatePDF_ExternalLogic.E2ETests/UltimatePDF_ExternalLogic.E2ETests.csproj -c Release
dotnet exec src/UltimatePDF_ExternalLogic.E2ETests/bin/Release/net10.0/UltimatePDF_ExternalLogic.E2ETests.dll
```

The fixture authenticates with the ODC tenant, pushes a one-time secret to the app configuration, waits for it to propagate, then runs the tests. Total fixture setup takes roughly 10–15 seconds before any test executes.

### Testing Your Changes in ODC

1. Run `.\generate_upload_package.ps1` to build the package
2. Upload `UltimatePDF_ExternalLogic.zip` to your ODC Portal as external logic ([ODC documentation](https://success.outsystems.com/documentation/outsystems_developer_cloud/building_apps/extend_your_apps_with_external_logic_using_custom_code/))
3. Test using the provided test application:
   - Open `oml/Ultimate PDF Tests.oml` in ODC Studio
   - Publish to your tenant
   - Run the test scenarios to validate your changes

The `Ultimate PDF Tests.oml` application contains multiple examples and test scenarios. All pull requests are validated against these tests.

## Pull Request Process

### Before Submitting

1. Ensure your branch is up to date with `main`:
   ```bash
   git fetch origin
   git rebase origin/main
   ```
2. Run the full test suite and confirm all tests pass (see [Running Tests](#running-tests) above):
   ```bash
   dotnet build src/UltimatePDF_ExternalLogic.sln -c Release
   dotnet exec src/UltimatePDF_ExternalLogic.UnitTests/bin/Release/net10.0/UltimatePDF_ExternalLogic.UnitTests.dll
   dotnet exec src/UltimatePDF_ExternalLogic.IntegrationTests/bin/Release/net10.0/UltimatePDF_ExternalLogic.IntegrationTests.dll
   ```
3. Run the integration tests in a container to validate against the ODC runtime environment
4. Update documentation if you've changed functionality
5. Verify the external logic package builds successfully

### Creating a Pull Request

1. Push your branch to your fork
2. Open a pull request against the `main` branch
3. In your PR description, include:
   - What behavior or issue you're addressing
   - What you changed and why
   - Testing evidence or steps to reproduce
   - Any relevant screenshots or examples
4. Reference any related issues using `#issue_number`

### Review Process

- Address code review feedback promptly
- Keep your PR synchronized with the upstream `main` branch
- Be responsive to questions and suggestions from maintainers

## Useful Commands

| Command | Description |
|---------|-------------|
| `.\generate_upload_package.ps1` | Build and package external logic for ODC deployment |
| `dotnet build src/UltimatePDF_ExternalLogic.sln` | Build the full solution |
| `dotnet build src/UltimatePDF_ExternalLogic.sln -c Release` | Build release configuration |
| `dotnet publish src/UltimatePDF_ExternalLogic.sln -c Release -r linux-x64 --self-contained false` | Publish for Linux runtime (ODC target) |
| `dotnet exec src/UltimatePDF_ExternalLogic.UnitTests/bin/Release/net10.0/UltimatePDF_ExternalLogic.UnitTests.dll` | Run unit tests only (build first) |
| `dotnet exec src/UltimatePDF_ExternalLogic.IntegrationTests/bin/Release/net10.0/UltimatePDF_ExternalLogic.IntegrationTests.dll` | Run integration tests on the host (build first) |
| `dotnet exec src/UltimatePDF_ExternalLogic.E2ETests/bin/Release/net10.0/UltimatePDF_ExternalLogic.E2ETests.dll` | Run tenant smoke tests (requires `appsettings.json` copied from template; build first) |
| `git log --oneline -20` | View recent commit history |

## Project Structure

- `src/` - C# source code
  - `UltimatePDF_ExternalLogic.sln` - Solution file (main project + unit tests + integration tests + tenant tests)
  - `UltimatePDF_ExternalLogic/` - Main C# external logic project
    - `IUltimatePDF_ExternalLogic.cs` - Public interface defining ODC server actions
    - `BrowserExecution/` - Browser instance management and pooling
    - `LayoutPrintPipeline/` - PDF generation pipeline
    - `Cleanup/` - Resource cleanup tasks
    - `Structures/` - Data structures for ODC integration
    - `Utils/` - REST/S3 senders, URL validation, async helpers
    - `resources/` - Embedded resources (version info, icons)
  - `UltimatePDF_ExternalLogic.UnitTests/` - xUnit v3 unit tests (Moq, WireMock.Net)
  - `UltimatePDF_ExternalLogic.IntegrationTests/` - xUnit v3 end-to-end tests (Chromium + PDF validation)
  - `UltimatePDF_ExternalLogic.E2ETests/` - xUnit v3 smoke tests against a live ODC tenant (requires appsettings.json)
- `oml/` - OutSystems modules
  - `Ultimate PDF.oml` - Library with accelerators and actions
  - `Template_UltimatePDF.oml` - Application template
  - `Ultimate PDF Tests.oml` - Test scenarios
- `generate_upload_package.ps1` - Package build script

## Getting Help

- [Submit an issue](https://github.com/OutSystems/UltimatePDF-ExternalLogic/issues) with detailed information about problems
- Email the team at vanguard@outsystems.com with questions or feedback

## Code of Conduct

This project is maintained by OutSystems. Please be respectful and constructive in all interactions.

## License

This project uses the BSD-3-Clause license. See [LICENSE](LICENSE) for details. By contributing, you agree that your contributions will be licensed under the same terms.
