Set-ExecutionPolicy -Scope CurrentUser Unrestricted
dotnet publish -c Release -r linux-x64 --self-contained false
Compress-Archive -Path .\src\TestLambda_UltimatePDF_ExternalLogic\bin\Release\net8.0\linux-x64\publish\* -Update -DestinationPath TestLambda_UltimatePDF_ExternalLogic.zip -CompressionLevel Optimal