set NAME_TARGET=%1
set NAME_PROJECT=%2
set DIR_TARGET=%3
set NAME_CONFIG=%4
set TOOLKIT_PATH=%5

@echo off
echo Creating release package...
mkdir DeploymentPackage_%NAME_CONFIG%
mkdir DeploymentPackage_%NAME_CONFIG%\version
mkdir DeploymentPackage_%NAME_CONFIG%\SQL

echo %NAME_CONFIG% | find "Debug"

if errorlevel 1 goto :release

:debug
echo Copy in client folder
IF EXIST %DIR_TARGET%%NAME_TARGET%.pdb xcopy %DIR_TARGET%%NAME_TARGET%.pdb %TOOLKIT_PATH%\..\..\ /Y /C
IF EXIST %DIR_TARGET%%NAME_TARGET%.dll xcopy %DIR_TARGET%%NAME_TARGET%.dll %TOOLKIT_PATH%\..\..\ /Y /C

echo Copy in debug deployment package
IF EXIST %DIR_TARGET%%NAME_TARGET%.pdb xcopy %DIR_TARGET%%NAME_TARGET%.pdb DeploymentPackage_%NAME_CONFIG% /Y /C

goto :end

:release
echo Copy in release deployment package

:end

xcopy %DIR_TARGET%%NAME_TARGET%.dll DeploymentPackage_%NAME_CONFIG% /Y /C
xcopy %DIR_TARGET%%NAME_TARGET%.lib DeploymentPackage_%NAME_CONFIG% /Y /C
xcopy %DIR_TARGET%..\..\Version\ReleaseNotes.%NAME_PROJECT%.txt DeploymentPackage_%NAME_CONFIG%\version\ /Y /C
xcopy %DIR_TARGET%..\..\Resource\%NAME_PROJECT%.txt DeploymentPackage_%NAME_CONFIG% /Y /C
xcopy %DIR_TARGET%..\..\SQL\%NAME_PROJECT%Script.sql DeploymentPackage_%NAME_CONFIG%\SQL\ /Y /C
xcopy %DIR_TARGET%..\..\SQL\Rollback_%NAME_PROJECT%Script.sql DeploymentPackage_%NAME_CONFIG%\SQL\ /Y /C

echo Package created in folder DeploymentPackage_%NAME_CONFIG%.