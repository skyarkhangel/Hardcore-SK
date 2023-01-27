echo off

REM echo remove unnecessary assemblies
REM DEL ..\*\Assemblies\*.* /Q /F
REM DEL ..\Assemblies\*.* /Q /F

echo build dll: %1
dotnet build .vscode --configuration %1