Remove-Item -Path .\UltimatePDF_ExternalLogic.zip -Force
Set-ExecutionPolicy -Scope CurrentUser Unrestricted
dotnet publish src\UltimatePDF_ExternalLogic.sln -c Release -r linux-x64 --self-contained false
Compress-Archive -Path .\src\UltimatePDF_ExternalLogic\bin\Release\net8.0\linux-x64\publish\* -Update -DestinationPath UltimatePDF_ExternalLogic.zip