@ECHO OFF
powershell.exe -ExecutionPolicy Bypass %~dp0\build.ps1 %~dp0\..\src\Log4netShipper %1
pause