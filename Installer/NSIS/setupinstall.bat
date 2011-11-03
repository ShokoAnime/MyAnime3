@echo off
echo ---------------------------------------------------------------
echo ---------------------------------------------------------------
echo ---- This batch file will copy files from the anime2 svn   ----
echo ---- folder to this NSIS script directory. Once complete   ----
echo ---- you can run the nsis script to generate the installer.----
echo ---- Remeber to compile the project first, and keep this   ----
echo ---- batch file in the NSIS directory in your local SVN.   ----
echo ---------------------------------------------------------------
echo ---------------------------------------------------------------
IF NOT "%1" == "true" pause       
echo.

set ProjectReleaseDir=..\..\Release
:releasecheck
for %%? in (hasher.dll) do if exist %ProjectReleaseDir%\%%? goto Continue

echo Could not find hasher.dll in anime2 project release directory. Have
echo You compiled the project?. If so please enter release directory.
set /P ProjectReleaseDir=Release Directory location:
goto releasecheck

echo.
:continue

echo Starting file copy.
echo %ProjectReleaseDir%
xcopy %ProjectReleaseDir% Plugin /S /I /Y /G

xcopy ..\..\Dependencies Plugin /S /I /Y /G
echo.

echo Batch complete. All the necessary files for the installer
echo should now be in the NSIS folder that this batch file
echo resides.
IF NOT "%1" == "true" pause