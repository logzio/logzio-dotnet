@ECHO OFF

set framework=%1

echo Clearing previous files...
rmdir %framework%\out /S /Q
rmdir %framework%\package /S /Q
del %framework%\*.nupkg