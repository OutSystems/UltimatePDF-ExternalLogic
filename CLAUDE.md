# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

UltimatePDF-ExternalLogic is an OutSystems Developer Cloud (ODC) external logic component that generates PDFs from web pages using Chromium's rendering engine. The code is written in C# targeting .NET 8.0 and runs within ODC's managed infrastructure.

For system architecture and design patterns, see [ARCHITECTURE.md](./ARCHITECTURE.md). For development workflow and contribution guidelines, see [CONTRIBUTING.md](./CONTRIBUTING.md).

## Quick Command Reference

```bash
# Build external logic package for ODC deployment
.\generate_upload_package.ps1

# Manual build commands
dotnet build src/UltimatePDF_ExternalLogic.sln
dotnet build src/UltimatePDF_ExternalLogic.sln -c Release
dotnet publish src/UltimatePDF_ExternalLogic.sln -c Release -r linux-x64 --self-contained false
```

The build script produces `UltimatePDF_ExternalLogic.zip` which can be uploaded to ODC Portal as external logic.

## Repository Structure

```
src/                             # C# source code
├── UltimatePDF_ExternalLogic.sln   # Solution file
└── UltimatePDF_ExternalLogic/      # Main C# external logic project
    ├── IUltimatePDF_ExternalLogic.cs   # Public interface (4 actions: PrintPDF, PrintPDF_ToRest, PrintPDF_ToS3, ScreenshotPNG)
    ├── UltimatePDF_ExternalLogic.cs    # Implementation class (only public class)
    ├── BrowserExecution/               # Browser pooling and Chromium automation
    ├── LayoutPrintPipeline/            # Multi-stage PDF generation pipeline
    ├── Management/Troubleshooting/     # Logging infrastructure
    ├── Utils/                          # REST/S3 senders, URL validation, async helpers
    ├── Structures/                     # ODC data structures (Viewport, Paper, Cookie, etc.)
    └── resources/                      # Embedded resources (version, icon)

oml/                             # OutSystems modules
├── Ultimate PDF.oml                # Library with wrapper actions and UI blocks
├── Template_UltimatePDF.oml        # Application template with REST API and token auth
└── Ultimate PDF Tests.oml          # Test scenarios for validation

TestLambda_UltimatePDF_ExternalLogic/    # Test project (not for production)
UltimatePDFLambdaAuthorizer/             # Lambda infrastructure (not for production)
UltimatePDFLambdaFunctions/              # Lambda infrastructure (not for production)
```

The main project contains 31 C# files. All implementation classes are `internal` except the interface implementation.

## Key Technical Context

### OutSystems External Logic SDK Integration

The public API surface is defined through `IUltimatePDF_ExternalLogic` using OutSystems SDK attributes:
- `[OSInterface]` - Declares the external logic interface visible in ODC
- `[OSAction]` - Exposes methods as server actions in ODC Studio
- `[OSParameter]` - Configures parameter metadata (data types, descriptions)

When modifying the interface, changes automatically propagate to ODC Studio after uploading the ZIP package.

### Platform Constraints

See [ARCHITECTURE.md - Platform Constraints](./ARCHITECTURE.md#platform-constraints) for ODC execution limits (95s timeout, 5.5MB payload, HTTPS-only).

### Testing Changes

After building the package:
1. Upload `UltimatePDF_ExternalLogic.zip` to ODC Portal
2. Open `oml/Ultimate PDF Tests.oml` in ODC Studio
3. Publish to your tenant and run test scenarios

All pull requests are validated against these tests.

### Browser Instance Lifecycle

The `BrowserInstancePool` maintains reusable Chromium instances across invocations. Key files:
- `BrowserExecution/BrowserInstancePool.cs` - Thread-safe pooling logic with health checks
- `BrowserExecution/PooledBrowserInstance.cs` - Wrapper for browser lifecycle
- `BrowserExecution/PooledPage.cs` - Per-request page context

Browser instances remain healthy for 10-15 minutes (typical infrastructure lifecycle). First cold-start takes 10+ seconds; subsequent requests within the window complete in under 5 seconds.

### PDF Generation Pipeline

The pipeline in `LayoutPrintPipeline/Pipeline.cs` orchestrates multi-stage rendering:
1. Render main content with Chromium
2. Merge page backgrounds using PDFsharp
3. Calculate pagination from rendered content
4. Merge headers and footers with page numbers

This separation allows dynamic pagination (e.g., "Page 3 of 10") without re-rendering. Detection logic in `ODCUltimatePDFExecutionContext.PrintPDF` checks for layout markers via JavaScript.

### Storage Strategies

Three actions support different storage patterns:
- `PrintPDF` - Returns binary directly (fails if >5.5MB)
- `PrintPDF_ToRest` - Sends to REST endpoint asynchronously (`Utils/RestSender.cs`)
- `PrintPDF_ToS3` - Uploads to S3 via presigned URLs (`Utils/S3Sender.cs`)

All async operations are wrapped in synchronous calls by `Utils/AsyncUtils.StartAndWait` to meet ODC external logic requirements.

### Logging

Logging is opt-in via `collectLogs` and `attachFilesLogs` parameters. When enabled, `Management/Troubleshooting/Logger.cs` captures execution traces, browser logs, and artifacts (HTML, PDF, screenshots) in a ZIP file. Returns `NullLogger` when disabled to minimize overhead.

## Common Development Patterns

### Adding a New Action

1. Add method signature to `IUltimatePDF_ExternalLogic` with SDK attributes
2. Implement in `UltimatePDF_ExternalLogic.cs`
3. Add structures to `Structures/` if new data types are needed
4. Update `oml/Ultimate PDF.oml` with wrapper action if creating convenience helpers
5. Add test scenario to `oml/Ultimate PDF Tests.oml`

### Modifying Browser Behavior

Browser automation uses PuppeteerSharp (wraps Chrome DevTools Protocol). Key patterns:
- Access browser pool via `BrowserInstancePool.NewPooledPage(logger)`
- Use `pooled.Page` for PuppeteerSharp API calls
- Always wrap operations in try-finally to ensure cleanup
- Log important operations to troubleshooting logger

### Working with PDFsharp

PDF manipulation happens after Chromium rendering. Common operations:
- Open PDF: `PdfDocument.Open(stream, PdfDocumentOpenMode.Import)`
- Create graphics: `XGraphics.FromPdfPage(page)`
- Import as form: `XPdfForm.FromStream(stream)`
- Draw: `gfx.DrawImage(form, rect)`

See `LayoutPrintPipeline/Pipeline.cs` methods (`MergeBackground`, `MergeHeaders`, `MergeFooters`) for examples.

## Known Gotchas

- **IP Filtering not supported** - External logic runs outside tenant network ([README Known Limitations](./README.md#known-limitations))
- **No authentication on print screens** - Use token-based protection (see `Template_UltimatePDF.oml` for pattern)
- **Cold start penalty** - First execution after idle takes 10+ seconds; configure Server Request Timeout accordingly
- **Google Fonts dependency** - Custom fonts require Google Fonts or resource embedding (see [README - Add Fonts](./README.md#add-fonts-to-the-report))
- **Map widget rendering** - Multiple Map widget instances fail; use Static Map instead ([Issue #15](https://github.com/OutSystems/UltimatePDF-ExternalLogic/issues/15))

## Technology Stack

- **.NET 8.0** - Target framework for ODC external logic
- **PuppeteerSharp** - Browser automation via Chrome DevTools Protocol
- **HeadlessChromium.Puppeteer.Lambda.Dotnet** (v1.1.0.97) - Chromium binaries for AWS Lambda (ODC infrastructure)
- **PDFsharp** (v6.2.0) - PDF layer composition
- **OutSystems.ExternalLibraries.SDK** (v1.5.0) - ODC external logic framework

## Additional Resources

- [OutSystems ODC External Logic Documentation](https://success.outsystems.com/documentation/outsystems_developer_cloud/building_apps/extend_your_apps_with_external_logic_using_custom_code/)
- [README.md](./README.md) - Usage examples, advance scenarios, public elements reference
- [O11 Ultimate PDF (Forge)](https://www.outsystems.com/forge/component-overview/5641/ultimate-pdf) - Original component this is based on

For questions or issues, email vanguard@outsystems.com or [submit an issue](https://github.com/OutSystems/UltimatePDF-ExternalLogic/issues).
