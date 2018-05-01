@ECHO OFF

set framework=%1
set dllsuffix=%2
set project=%dllsuffix%Shipper

call _clear %framework%

set dotnet="C:\Program Files\dotnet\dotnet.exe"
echo Pack...
%dotnet% pack ..\src\%project%\%project%.csproj -o ..\..\package\%framework%

echo Done