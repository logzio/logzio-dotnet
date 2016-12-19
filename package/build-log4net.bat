@ECHO OFF
echo Clearing previous files...
rmdir log4net\out /S /Q
rmdir log4net\package /S /Q
del log4net\*.nupkg

echo Making directory tree...
mkdir log4net
mkdir log4net\out
mkdir log4net\package
mkdir log4net\package\lib
mkdir log4net\package\lib\net45
mkdir log4net\package\lib\net46

set msbuild="C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe"
set ilmerge=..\src\packages\ILMerge.2.14.1208\tools\ILMerge.exe
set nuget=..\src\packages\NuGet.CommandLine.3.4.3\tools\NuGet.exe


echo Building project...
%msbuild% ..\src\Log4net\Log4netShipper.csproj /p:OutDir=..\..\package\log4net\out\46;Configuration=Release;TargetFrameworkVersion=4.6 /t:Rebuild
%msbuild% ..\src\Log4net\Log4netShipper.csproj /p:OutDir=..\..\package\log4net\out\45;Configuration=Release;TargetFrameworkVersion=4.5 /t:Rebuild

echo Merging DLLs...
%ilmerge% /out:log4net\package\lib\net45\Logzio.DotNet.Log4net.dll log4net\out\45\Logzio.DotNet.Log4net.dll log4net\out\45\Logzio.DotNet.Core.dll 
%ilmerge% /out:log4net\package\lib\net46\Logzio.DotNet.Log4net.dll log4net\out\46\Logzio.DotNet.Log4net.dll log4net\out\46\Logzio.DotNet.Core.dll 


copy log4net\Logzio.DotNet.Log4net.nuspec log4net\package\Logzio.DotNet.Log4net.nuspec
%nuget% pack log4net\package\Logzio.DotNet.Log4net.nuspec -OutputDirectory log4net

echo Done
pause