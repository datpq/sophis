@echo off

set NAME_TARGET=%1
set NAME_PROJECT=%2
set DIR_TARGET=%3
set NAME_CONFIG=%4
set TOOLKIT_PATH=%5

mkdir ..\..\%NAME_PROJECT%_%NAME_CONFIG%
mkdir ..\..\%NAME_PROJECT%_%NAME_CONFIG%\SQL
mkdir ..\..\%NAME_PROJECT%_%NAME_CONFIG%\version

echo %NAME_CONFIG% | find "Debug"


echo NAME_TARGET %NAME_TARGET%
echo NAME_PROJECT %NAME_PROJECT%
echo DIR_TARGET %DIR_TARGET%
echo NAME_CONFIG %NAME_CONFIG%
echo TOOLKIT_PATH %TOOLKIT_PATH%

if errorlevel 1 goto :release

:debug

IF EXIST %DIR_TARGET%%NAME_TARGET%.pdb xcopy %DIR_TARGET%%NAME_TARGET%.pdb ..\..\%NAME_PROJECT%_%NAME_CONFIG%\ /Y /C
IF EXIST %DIR_TARGET%%NAME_TARGET%.pdb xcopy %DIR_TARGET%%NAME_TARGET%.pdb %TOOLKIT_PATH%\..\..\ /Y /C

goto :end

:release

:end

IF EXIST %DIR_TARGET%%NAME_TARGET%.dll xcopy %DIR_TARGET%%NAME_TARGET%.dll %TOOLKIT_PATH%\..\..\ /Y /C
IF EXIST %DIR_TARGET%%NAME_TARGET%.dll xcopy %DIR_TARGET%%NAME_TARGET%.dll ..\..\%NAME_PROJECT%_%NAME_CONFIG% /Y /C
IF EXIST %DIR_TARGET%%NAME_TARGET%.lib xcopy %DIR_TARGET%%NAME_TARGET%.lib ..\..\%NAME_PROJECT%_%NAME_CONFIG% /Y /C
IF EXIST ..\..\Version\ReleaseNotes.%NAME_PROJECT%.txt xcopy ..\..\Version\ReleaseNotes.%NAME_PROJECT%.txt ..\..\%NAME_PROJECT%_%NAME_CONFIG%\version /Y /C
IF EXIST ..\..\SQL\%NAME_PROJECT%Script.sql xcopy ..\..\SQL\%NAME_PROJECT%Script.sql ..\..\%NAME_PROJECT%_%NAME_CONFIG%\SQL /Y /C
IF EXIST ..\..\SQL\Rollback_%NAME_PROJECT%Script.sql xcopy ..\..\SQL\Rollback_%NAME_PROJECT%Script.sql ..\..\%NAME_PROJECT%_%NAME_CONFIG%\SQL /Y /C
echo Package created in folder %NAME_PROJECT%_%NAME_CONFIG%.
