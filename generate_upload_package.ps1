$zipPath = ".\UltimatePDF_ExternalLogic.zip"
$projectPath = "src\UltimatePDF_ExternalLogic\UltimatePDF_ExternalLogic.csproj"
$publishDir = ".\src\UltimatePDF_ExternalLogic\bin\Release\net10.0\linux-x64\publish\"

if (Test-Path -Path $zipPath -PathType Leaf) {
    Write-Host "Removing existing package: $zipPath"
    Remove-Item -Path $zipPath -Force
}

Write-Host "Setting execution policy for current user (Unrestricted)..."
Set-ExecutionPolicy -Scope CurrentUser Unrestricted

# Only the main project is needed to produce the upload package (test projects aren't part of it).
Write-Host "Publishing $projectPath for linux-x64 (Release, framework-dependent)..."
dotnet publish $projectPath -c Release -r linux-x64 --self-contained false

# dotnet publish doesn't throw in PowerShell on failure, so without this check a failed build
# would silently get packaged from stale (or missing) publish output.
if ($LASTEXITCODE -ne 0) {
    Write-Host "dotnet publish failed with exit code $LASTEXITCODE. Aborting package creation." -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host "Compressing publish output into $zipPath..."
Compress-Archive -Path (Join-Path $publishDir '*') -Update -DestinationPath $zipPath

Write-Host "Package created: $zipPath" -ForegroundColor Green