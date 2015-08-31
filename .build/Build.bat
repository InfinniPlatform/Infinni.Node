call "%VS120COMNTOOLS%VsDevCmd.bat"
nuget.exe restore "..\Infinni.Node.sln"
msbuild "..\Infinni.Node.sln" /t:Clean /p:Configuration=Release
msbuild "..\Infinni.Node.sln" /p:Configuration=Release
IF EXIST "..\Assemblies\Infinni.Node.zip" del /F "..\Assemblies\Infinni.Node.zip"
PowerShell -NoProfile -ExecutionPolicy Bypass -Command "Compress-Archive -Path ..\Assemblies\*.* -DestinationPath ..\Assemblies\Infinni.Node.zip"