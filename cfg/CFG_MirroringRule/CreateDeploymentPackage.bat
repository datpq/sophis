set TOOLKIT_PATH=%1
set NAME_PLATFORM=%2
set NAME_CONFIG=%3
set NAME_PROJECT=%4
set DIR_TARGET=%5
set NAME_TARGET=%6

@echo off
echo Creating release package...
mkdir %NAME_TARGET%_%NAME_PLATFORM%_%NAME_CONFIG%
mkdir %NAME_TARGET%_%NAME_PLATFORM%_%NAME_CONFIG%\SQL


echo %NAME_CONFIG% | find "Debug"

echo %NAME_CONFIG% | find "Debug"

if errorlevel 1 goto :release

:debug
echo Copy in client folder
IF EXIST %DIR_TARGET%%NAME_TARGET%.pdb xcopy %DIR_TARGET%%NAME_TARGET%.pdb %TOOLKIT_PATH%\..\..\ /Y /C
IF EXIST %DIR_TARGET%%NAME_TARGET%.dll xcopy %DIR_TARGET%%NAME_TARGET%.dll %TOOLKIT_PATH%\..\..\ /Y /C

echo Copy in debug deployment package
IF EXIST %DIR_TARGET%%NAME_TARGET%.pdb xcopy %DIR_TARGET%%NAME_TARGET%.pdb %NAME_TARGET%_%NAME_PLATFORM%_%NAME_CONFIG% /Y /C

goto :end

:release
echo Copy in release deployment package

:end

xcopy %DIR_TARGET%%NAME_TARGET%.dll %NAME_TARGET%_%NAME_PLATFORM%_%NAME_CONFIG% /Y /C
xcopy %DIR_TARGET%%NAME_TARGET%.lib %NAME_TARGET%_%NAME_PLATFORM%_%NAME_CONFIG% /Y /C
xcopy %DIR_TARGET%..\..\Version\ReleaseNotes.%NAME_PROJECT%.txt %NAME_TARGET%_%NAME_PLATFORM%_%NAME_CONFIG% /Y /C
xcopy %DIR_TARGET%..\..\Resource\%NAME_PROJECT%.txt %NAME_TARGET%_%NAME_PLATFORM%_%NAME_CONFIG% /Y /C
xcopy %DIR_TARGET%..\..\SQL\%NAME_PROJECT%Script.sql %NAME_TARGET%_%NAME_PLATFORM%_%NAME_CONFIG%\SQL\ /Y /C
xcopy %DIR_TARGET%..\..\SQL\Rollback_%NAME_PROJECT%Script.sql %NAME_TARGET%_%NAME_PLATFORM%_%NAME_CONFIG%\SQL\ /Y /C


echo Package created in folder %NAME_TARGET%_%NAME_PLATFORM%_%NAME_CONFIG%.