@ECHO OFF
echo Clearing previous files...
rmdir log4net\out /S /Q
rmdir log4net\package /S /Q

echo Making directory tree...
mkdir log4net
mkdir log4net\out
mkdir log4net\package
mkdir log4net\package\lib

set msbuild=%windir%\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe
set ilmerge=..\src\packages\ILMerge.2.14.1208\tools\ILMerge.exe
set nuget=..\src\packages\NuGet.CommandLine.3.4.3\tools\NuGet.exe

echo Building project...
%msbuild% ..\src\Log4net\Log4netShipper.csproj /p:OutDir=..\..\package\log4net\out;Configuration=Release /t:Rebuild

echo Merging DLLs...
%ilmerge% /out:log4net\package\lib\Logzio.DotNet.Log4net.dll log4net\out\Logzio.DotNet.Log4net.dll log4net\out\Logzio.DotNet.Core.dll 

copy log4net\Logzio.DotNet.Log4net.nuspec log4net\package\Logzio.DotNet.Log4net.nuspec

%nuget% pack log4net\package\Logzio.DotNet.Log4net.nuspec -OutputDirectory log4net

echo Done
pause