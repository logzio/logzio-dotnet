param (
    [string]$ProjectName,
    [string]$Version
)

msbuild /t:Restore,Pack $ProjectName /p:Version=$Version /p:targetFrameworks='"net5.0;netcoreapp3.1;net45;netstandard2.0;netstandard1.3"' /p:Configuration=Release /p:PackageOutputPath=..\..\package /verbosity:minimal