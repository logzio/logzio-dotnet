@ECHO OFF
powershell.exe -ExecutionPolicy Bypass %~dp0\build.ps1 %~dp0\..\src\NLogShipper %1
pause