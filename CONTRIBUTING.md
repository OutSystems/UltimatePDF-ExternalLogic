# Contributing to Ultimate PDF External Logic

Thank you for your interest in contributing to Ultimate PDF External Logic. This document provides guidelines for contributing to this OutSystems Developer Cloud (ODC) external logic component.

## Development Setup

### Prerequisites

- .NET SDK 8.0 or later (project targets `net8.0`)
- PowerShell (for package generation script)
- Git for version control
- An OutSystems ODC tenant for testing

### Installation

1. Fork the repository from https://github.com/OutSystems/UltimatePDF-ExternalLogic
2. Clone your fork locally:
   ```bash
   git clone https://github.com/YOUR_USERNAME/UltimatePDF-ExternalLogic.git
   cd UltimatePDF-ExternalLogic
   ```
3. Open `UltimatePDF_ExternalLogic.sln` in your preferred C# IDE (Visual Studio, Rider, or VS Code)

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

This is a C# project targeting .NET 8.0 with nullable reference types enabled. Follow standard C# conventions:
- Use meaningful variable and method names
- Add XML documentation comments for public interfaces and methods (see `IUltimatePDF_ExternalLogic.cs` for examples)
- Use `OutSystems.ExternalLibraries.SDK` attributes appropriately (`[OSInterface]`, `[OSAction]`, `[OSParameter]`)
- Maintain consistent code formatting with the existing codebase

Key dependencies:
- `OutSystems.ExternalLibraries.SDK` (v1.5.0) - Core external logic framework
- `HeadlessChromium.Puppeteer.Lambda.Dotnet` (v1.1.0.97) - Browser automation
- `PDFsharp` (v6.2.0) - PDF manipulation

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
dotnet build
dotnet build -c Release
dotnet publish -c Release -r linux-x64 --self-contained false
```

### Testing Your Changes

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
2. Test your changes thoroughly in an ODC tenant
3. Update documentation if you've changed functionality
4. Verify the external logic package builds successfully

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
| `dotnet build` | Build the solution |
| `dotnet build -c Release` | Build release configuration |
| `dotnet publish -c Release -r linux-x64 --self-contained false` | Publish for Linux runtime (ODC target) |
| `git log --oneline -20` | View recent commit history |

## Project Structure

- `UltimatePDF_ExternalLogic/` - Main C# external logic project
  - `IUltimatePDF_ExternalLogic.cs` - Public interface defining ODC server actions
  - `BrowserExecution/` - Browser instance management and pooling
  - `LayoutPrintPipeline/` - PDF generation pipeline
  - `Cleanup/` - Resource cleanup tasks
  - `Structures/` - Data structures for ODC integration
  - `resources/` - Embedded resources (version info, icons)
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
