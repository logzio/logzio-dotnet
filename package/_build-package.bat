@ECHO OFF

set framework=%1
set dllsuffix=%2
set project=%dllsuffix%Shipper

call _clear %framework%


echo Making directory tree...
mkdir %framework%
mkdir %framework%\out
mkdir %framework%\package
mkdir %framework%\package\lib
mkdir %framework%\package\lib\net45
mkdir %framework%\package\lib\net46

set msbuild="C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe"
set ilmerge=..\src\packages\ILMerge.2.14.1208\tools\ILMerge.exe
set nuget=..\src\packages\NuGet.CommandLine.3.4.3\tools\NuGet.exe


echo Building project...
%msbuild% ..\src\%project%\%project%.csproj /p:OutDir=..\..\package\%framework%\out\46;Configuration=Release;TargetFrameworkVersion=4.6 /t:Rebuild
%msbuild% ..\src\%project%\%project%.csproj /p:OutDir=..\..\package\%framework%\out\45;Configuration=Release;TargetFrameworkVersion=4.5 /t:Rebuild

echo Merging DLLs...
%ilmerge% /out:%framework%\package\lib\net45\Logzio.DotNet.%dllsuffix%.dll %framework%\out\45\Logzio.DotNet.%dllsuffix%.dll %framework%\out\45\Logzio.DotNet.Core.dll 
%ilmerge% /out:%framework%\package\lib\net46\Logzio.DotNet.%dllsuffix%.dll %framework%\out\46\Logzio.DotNet.%dllsuffix%.dll %framework%\out\46\Logzio.DotNet.Core.dll 


copy %framework%\Logzio.DotNet.%dllsuffix%.nuspec %framework%\package\Logzio.DotNet.%dllsuffix%.nuspec
%nuget% pack %framework%\package\Logzio.DotNet.%dllsuffix%.nuspec -OutputDirectory %framework%


:nuget setApiKey Your-API-Key
:nuget push YourPackage.nupkg -Source https://www.nuget.org/api/v2/package

echo Done