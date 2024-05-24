Set-ExecutionPolicy -Scope CurrentUser Unrestricted
dotnet publish -c Release -r linux-x64 --self-contained false
Get-ChildItem .\src\TestLambda_UltimatePDF_ExternalLogic\bin\Release\net6.0\linux-x64\publish |
    where { $_.Name -notin @("chromium.br", "swiftshader.tar.br", "al2.tar.br", "al2023.tar.br")} | 
        Compress-Archive -DestinationPath TestLambda_UltimatePDF_ExternalLogic.zip -Update -CompressionLevel Optimal