@ECHO OFF
echo Clearing previous files...
rmdir log4net /S /Q

echo Making directory tree...
mkdir log4net
mkdir log4net\package
mkdir log4net\out

set msbuild=%windir%\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe
set ilmerge=..\src\packages\ILMerge.2.14.1208\tools\ILMerge.exe

echo Building project...
%msbuild% ..\src\Log4net\Log4netShipper.csproj /p:OutDir=..\..\package\log4net\out

echo Merging DLLs...
%ilmerge% /out:log4net\package\Logzio.DotNet.Log4net.dll log4net\out\Logzio.DotNet.Log4net.dll log4net\out\Logzio.DotNet.Core.dll 

echo Done
pause