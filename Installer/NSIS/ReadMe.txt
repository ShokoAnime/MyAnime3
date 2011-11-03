About the installer.

Using the NSIS install system, the Anime2.nsi script will generate the install file MyAnime2_v(version number).exe in the release folder.
We use a Unicode build of NSIS, get from http://www.scratchpaper.com/home/downloads.

The current Anime2 installer has the following features.

	1. Automatic detection of Mediaportal install directory.
	2. Detection of installed skins in mediaportal, preselecting them for the user with Anime2 skin support. This means only skins that are needed are installed.
	3. Automatic copying of media. Users just run the installer and their done.
	4. Creates an uninstaller, easily accessable by a startmenu shortcut.
	5. Auotmatic backup of files, mainly the fontengine.dll, and the anime database. Restores fontengine.dll file at uninstall.
	6. Contains project information and link to the forum topic.
	7. Legal information, with the major points of the GPLv3 license given straight up to the user.
	8. Automatic version detection. Will name itself based on anime2 version. Text in installer also updates to version number.
	9. Language support.



---Creating the installer---

Creating the installer is quite easy. Just keep the installer config files in the NSIS folder in the svn.
Make sure you have NSIS Unicode installed.

	1. Run the CreateInstall.bat file

This bat file will run everything needed to create the installer. It will setup the nsis folder for the compiler
and then run the compiler for you. After completing the compile it will prompt you to test the installer.



---Installer behavour---

The installer will backup the currently install version of the database before installing the new version
of the plugin. It will not do this tho if there is no current backup of this version. If there
is a current backup of this version then the installer will restore that database.

Eg
Current installed version is 2.0.13.0 and the installer is installing 2.0.14.0 - an upgrade
Installer action is:
	Copy anime database(v2.0.13.0) from MP database folder to anime2 backup folder in MP root.


Current installed version is 2.0.14.0 and the installer is installing 2.0.13.0 - a downgrade
Installer action is:
	Delete anime database (V2.0.14.0) from MP database folder. 
	Move anime database(v2.0.13.0) from anime2 backup folder to MP database folder.	


"Only two industries refer to their customers as users."