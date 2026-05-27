Remove-Item -Recurse -Force coverage-report -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force src/UltimatePDF_ExternalLogic.UnitTests/TestResults -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force src/UltimatePDF_ExternalLogic.IntegrationTests/TestResults -ErrorAction SilentlyContinue

dotnet test src/UltimatePDF_ExternalLogic.UnitTests `
    --collect:"XPlat Code Coverage" `
    --settings coverage.runsettings

dotnet test src/UltimatePDF_ExternalLogic.IntegrationTests `
    --collect:"XPlat Code Coverage" `
    --settings coverage.runsettings

dotnet tool run reportgenerator `
    -reports:"src/**/TestResults/**/coverage.cobertura.xml" `
    -targetdir:coverage-report `
    -reporttypes:Html

Start-Process coverage-report/index.html
