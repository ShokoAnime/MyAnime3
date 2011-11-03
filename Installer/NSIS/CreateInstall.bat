@echo off
echo -----------------------------------------------------------------------------
echo This script will create the Anime2 installer.
echo -----------------------------------------------------------------------------
echo.
set NSISPATH=%ProgramFiles%\nsis\unicode\makensis.exe
if exist "%NSISPATH%" goto continue
set NSISPATH=
if exist nsislocation.txt goto readsettings
for %%D in ("C:\program files" "D:\program files" "E:\Program files" "F:\Program files") DO DIR %%D\makensis.exe /B /P /S >> nsislocation.txt

echo.
:readsettings
for /F "usebackq delims=" %%g in (nsislocation.txt) do set NSISPATH=%%g

if not "%NSISPATH%"=="" goto continue

:prompt
echo.
echo Could not find NSIS Unicode compiler. Please put enter directory of makensis.exe
set /P NSISPATH=makensis.exe location:
if not exist "%NSISPATH%\makensis.exe" goto prompt
set NSISPATH=%NSISPATH%\makensis.exe

:continue
call setupinstall.bat true

if not exist "Release" mkdir Release

echo.
echo start compiling
"%NSISPATH%" Anime2.nsi

echo.
echo ----------------------------------------------------------------------------
echo ----------------------------------------------------------------------------
echo.
echo Installer should be created and sitting in the "Release" folder
echo.
if "%1" == "n" goto end
echo Would you like to test the installer?
set /P ANWSER=(y/n):
if %ANWSER%==n goto end
for /F "tokens=3" %%i in (Version.txt) do "release\MyAnime2_v%%~i.exe"
:end