@Echo Off
REM Script to safely use 'PluginConfigLoader' by verifying that MediaPortal is *NOT* running.
REM This will not prevent MediaPortal from being started before plugin is closed, so try not to do that :p
REM Last Modified: Jan 2nd 2010
REM Author: RoChess

:SafetyCheck
	REM --- Check if MediaPortal is still running
	tasklist | find /i /c "MediaPortal.exe" &&goto MediaPortalIsRunning

	REM --- Locating MediaPortal folder
	IF EXIST "%ProgramFiles%\Team MediaPortal\MediaPortal\PluginConfigLoader.exe"  (
		start "My Anime Config" "%ProgramFiles%\Team MediaPortal\MediaPortal\PluginConfigLoader.exe" /plugin="%ProgramFiles%\Team MediaPortal\MediaPortal\plugins\Windows\Anime2.dll"
	) ELSE (
		IF EXIST "%ProgramFiles(x86)%\Team MediaPortal\MediaPortal\PluginConfigLoader.exe"  (
			start "My Anime Config" "%ProgramFiles(x86)%\Team MediaPortal\MediaPortal\PluginConfigLoader.exe" /plugin="%ProgramFiles(x86)%\Team MediaPortal\MediaPortal\plugins\Windows\Anime2.dll"
		) ELSE (
			Goto MediaPortalNotFound
		)
	)
goto EndBat


:MediaPortalIsRunning
	cls
	echo.
	echo   WARNING: MediaPortal is still running!!
	echo.
	echo     It would be dangerous to start any of the MediaPortal
	echo     plugins with the main program still running.
	echo.
	echo   Please close MediaPortal, and try again.
	echo.
	pause
goto EndBat


:MediaPortalNotFound
	cls
	echo.
	echo   WARNING: MediaPortal *NOT* Found!!
	echo.
	echo     Unable to locate MediaPortal in the default location.
	echo.
	echo   Please adjust this script manually to use your custom folder.
	echo.
	pause
goto EndBat


:EndBat
REM --- All done