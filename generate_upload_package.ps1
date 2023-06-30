Set-ExecutionPolicy -Scope CurrentUser Unrestricted
dotnet publish -c Release -r linux-x64 --self-contained false
Compress-Archive -Path .\ODC_UltimatePDF_Service\bin\Release\net6.0\linux-x64\publish\* -Update -DestinationPath ODC_UltimatePDF_Service.zip