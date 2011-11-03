;-----------------------------------------------------------------------------------
; ANIME2 plugin for MediaPortal installer
; WebSite: http://www.otakumm.com/
;-----------------------------------------------------------------------------------
SetCompressor /SOLID LZMA
;-----------------------------------------------------------------------------------
; includes
	!include MUI2.nsh
	!include FileFunc.nsh
	!include Sections.nsh
	!include LogicLib.nsh
	!include TextFunc.nsh
	!include WordFunc.nsh
	!system "GetVersion.exe"
	!include "Version.txt"
	
;-----------------------------------------------------------------------------------	
;common variables
	Var PreviousVersion
	Var DatabaseLocation
	Var SkinLocation
	
	Var /GLOBAL NumLines
	Var /GLOBAL NumCurLine
	Var /GLOBAL AnimeSection
	Var /GLOBAL StrLength
	Var /GLOBAL StrCharCount
	Var /GLOBAL StrChar
	Var /GLOBAL Line
	Var /GLOBAL ThumbsLocation
	Var /GLOBAL Upgrading
	
;-----------------------------------------------------------------------------------	
;general settings
	Name $(NameText)
	OutFile "Release\MyAnime2_v${Version}.exe"
	
	;Defualt install directory
	InstallDir $INSTDIR
	
	;Get install folder from registry
	InstallDirRegKey HKLM "Software\My Anime 2" "InstallDir"
	
	BrandingText $(BrandNameText)
	
	Caption $(CaptionText)
	
	RequestExecutionLevel admin
	
;-----------------------------------------------------------------------------------	
;Interface Settings
	!define MUI_ABORTWARNING
	!define MUI_ICON Resources\anime2InstallIcon.ico
	!define MUI_UNICON Resources\anime2InstallIcon.ico
	
	!insertmacro MUI_RESERVEFILE_LANGDLL
	
;-----------------------------------------------------------------------------------	
;page settings

	!define MUI_HEADERIMAGE
	!define MUI_HEADERIMAGE_BITMAP Resources\chi-flying.bmp
	!define MUI_WELCOMEFINISHPAGE_BITMAP Resources\1rer21.bmp
	
	!define MUI_LANGDLL_REGISTRY_ROOT HKLM
	!define MUI_LANGDLL_REGISTRY_KEY "Software\My Anime 2"
	!define MUI_LANGDLL_REGISTRY_VALUENAME "Language"
	
	!define MUI_FINISHPAGE_NOAUTOCLOSE
	!define MUI_UNFINISHPAGE_NOAUTOCLOSE
	
	!define MUI_FINISHPAGE_TEXT_LARGE
	
	!define MUI_UNWELCOMEFINISHPAGE_BITMAP Resources\1rer21.bmp
	
	!define MUI_LANGDLL_ALWAYSSHOW
	!define MUI_LANGDLL_ALLLANGUAGES

	
	;text
	!define MUI_WELCOMEPAGE_TITLE $(WelcomeTitleText)
	!define MUI_WELCOMEPAGE_TEXT $(WelcomeText)
	
	!define MUI_COMPONENTSPAGE_TEXT_TOP $(ComponentsText)
	!define MUI_DIRECTORYPAGE_TEXT_TOP $(DirectoryText)
	!define MUI_COMPONENTSPAGE_TEXT_DESCRIPTION_INFO $(DESC_Anime2)

	!define MUI_FINISHPAGE_TEXT $(FinishText)
	!define MUI_FINISHPAGE_SHOWREADME http://www.otakumm.com/documentation/guides/71-quickstart
	!define MUI_FINISHPAGE_SHOWREADME_TEXT $(FinishReadmeText)
	;!define MUI_FINISHPAGE_RUN "$INSTDIR\Anime2\PluginLoader - MyAnime.cmd"
	!define MUI_FINISHPAGE_RUN_TEXT $(StartConfigText)
	
	!define MUI_FINISHPAGE_LINK $(FinishLinkText)
	!define MUI_FINISHPAGE_LINK_LOCATION http://www.otakumm.com
	
	!define MUI_UNTEXT_WELCOME_INFO_TEXT $(UnWelcomeText)
	
;-----------------------------------------------------------------------------------	
;Installer pages
	!insertmacro MUI_PAGE_WELCOME
	!insertmacro MUI_PAGE_LICENSE $(LicenseRTF)
	!define MUI_PAGE_CUSTOMFUNCTION_LEAVE DirectorySet
	!insertmacro MUI_PAGE_DIRECTORY
	!insertmacro MUI_PAGE_COMPONENTS
	!insertmacro MUI_PAGE_INSTFILES
	!define MUI_PAGE_CUSTOMFUNCTION_SHOW SetFinishPageOptions
	!insertmacro MUI_PAGE_FINISH
	
;-----------------------------------------------------------------------------------	
;Uninstaller Pages
	!insertmacro MUI_UNPAGE_WELCOME
	!insertmacro MUI_UNPAGE_CONFIRM
	!insertmacro MUI_UNPAGE_COMPONENTS
	!insertmacro MUI_UNPAGE_INSTFILES
	
	
;-----------------------------------------------------------------------------------	
;Languages	
	!insertmacro MUI_LANGUAGE "English"
	!insertmacro MUI_LANGUAGE "Japanese"
	!insertmacro MUI_LANGUAGE "German"
	
	!include languages\English.nsh
	!include languages\Japanese.nsh
	!include languages\German.nsh
	
	LicenseLangString LicenseRTF ${LANG_ENGLISH} "languages\english license.rtf" 
	LicenseLangString LicenseRTF ${LANG_JAPANESE} "languages\japanese license.rtf" 
	LicenseLangString LicenseRTF ${LANG_GERMAN} "languages\german license.rtf" 


;-----------------------------------------------------------------------------------	
;version information	
VIProductVersion "${Version}"
VIAddVersionKey /LANG=${LANG_ENGLISH} "ProductName" "My Anime 2"
VIAddVersionKey /LANG=${LANG_ENGLISH} "Comments" "My Anime 2, watch your anime in style!"
VIAddVersionKey /LANG=${LANG_ENGLISH} "CompanyName" "My Anime 2 Dev Team"
VIAddVersionKey /LANG=${LANG_ENGLISH} "ProductVersion" "${Version}"
VIAddVersionKey /LANG=${LANG_ENGLISH} "FileVersion" "${Version}"
VIAddVersionKey /LANG=${LANG_ENGLISH} "FileDescription" "Installs My Anime 2 for MediaPortal."
VIAddVersionKey /LANG=${LANG_ENGLISH} "LegalCopyright" "Files are under GPLv3 Licence"

;-----------------------------------------------------------------------------------	
;Reserver files

;-----------------------------------------------------------------------------------	
;Installer sections
Section "!$(^name)" Anime2
	;Create backup folder
	SetOutPath $INSTDIR\Anime2
	
	;Clean old versions
	IfFileExists "$INSTDIR\AniDBAPI.dll" 0 +2
	Delete "$INSTDIR\AniDBAPI.dll"
	
	IfFileExists "$INSTDIR\AniDBAPI.pdb" 0 +2
	Delete "$INSTDIR\AniDBAPI.pdb"
	
	IfFileExists "$SMPROGRAMS\Team MediaPortal\MediaPortal\Uninstall My Anime 2.lnk" 0 +2
	Delete "$SMPROGRAMS\Team MediaPortal\MediaPortal\Uninstall My Anime 2.lnk"
	
	;support files that belong in anime2 folder
	File "Resources\PluginLoader - MyAnime.cmd"
	
	;support files
	SetOutPath $INSTDIR
	
	;All the required support dll's
	File Plugin\*.dll
	
	;Other support files
	File Resources\PluginConfigLoader.exe
	File Resources\CorFlags.exe
	
	;check first, user might have newer version
	IfFileExists "$INSTDIR\ICSharpCode.SharpZipLib.dll" +2 0
	File ICSharpCode.SharpZipLib.dll
	
	;plugin dll
	SetOutPath $INSTDIR\plugins\Windows
	File Plugin\plugins\Windows\*.*
	File Plugin\hasher.dll
	
	;Write installation folder
	WriteRegStr HKLM "Software\My Anime 2" "InstallDir" $INSTDIR
	
	;Make uninstaller
	WriteUninstaller $INSTDIR\Anime2\$(^name)_Uninstall.exe
	
	;Register uninstaller for add/remove programs
	WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Anime2" "DisplayName" "$(^name) Plugin for MediaPortal"
	WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Anime2" "UninstallString" "$\"$INSTDIR\Anime2\$(^name)_Uninstall.exe$\""
	WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Anime2" "URLInfoAbout" "http://www.otakumm.com"
	WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Anime2" "NoModify" 1
	WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Anime2" "NoRepair" 1
	WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Anime2" "EstimatedSize" 6656 ;6,5 MB

	;Create startmenu link
	CreateDirectory "$SMPROGRAMS\Team MediaPortal\MediaPortal Plugins\My Anime"
	CreateShortCut "$SMPROGRAMS\Team MediaPortal\MediaPortal Plugins\My Anime\$(UninstallShortcutText).lnk" "$INSTDIR\Anime2\$(^name)_Uninstall.exe"
	CreateShortCut "$SMPROGRAMS\Team MediaPortal\MediaPortal Plugins\My Anime\$(ForumLinkText).lnk" "http://www.otakumm.com/forum" "" "$WINDIR\System32\Shell32.dll" 14 SW_SHOWNORMAL "" "$(ForumLinkDescriptionText)"
	CreateShortCut "$SMPROGRAMS\Team MediaPortal\MediaPortal Plugins\My Anime\$(HomePageLinkText).lnk" "http://www.otakumm.com" "" "$WINDIR\System32\Shell32.dll" 14 SW_SHOWNORMAL "" "$(HomePageLinkDescriptionText)"
	CreateShortCut "$SMPROGRAMS\Team MediaPortal\MediaPortal Plugins\My Anime\$(ManualLinkText).lnk" "http://www.otakumm.com/Anime2Wiki/Manual" "" "$WINDIR\System32\Shell32.dll" 14 SW_SHOWNORMAL "" "$(ManualLinkDescriptionText)"
	CreateShortCut "$SMPROGRAMS\Team MediaPortal\MediaPortal Plugins\My Anime\$(ConfigLinkText).lnk" "$INSTDIR\Anime2\PluginLoader - MyAnime.cmd" "" "" "" SW_SHOWMINIMIZED "" "$(ConfigLinkDescriptionText)"
SectionEnd

SectionGroup /e $(NAME_BackupsRestores)

Section /o $(NAME_DatabaseBackup) DatabaseBackup	
	;Backup current
	CopyFiles "$DatabaseLocation" $INSTDIR\anime2
	${WordFind} $DatabaseLocation "\" "-1" $1
	Rename $INSTDIR\anime2\$1 $INSTDIR\anime2\AnimeDatabaseV20_$PreviousVersion.db3
SectionEnd

Section /o $(NAME_RestoreBackup) RestoreBackup	
	;Restore this version's backed up database. Used when user is installing an older version to replace newer version. He might have had problems with newer version.
	Delete "$DatabaseLocation"
	Rename $INSTDIR\anime2\AnimeDatabaseV20_${Version}.db3 "$DatabaseLocation"
SectionEnd

SectionGroupEnd

!cd ..\..\MyAnimePlugin2

SectionGroup /e $(SkinsText)

Section /o "Blue3" Blue3
	SetOutPath $SkinLocation\Blue3
	File Skins\Blue3\*.*
	
	SetOutPath $SkinLocation\Blue3\media
	File /nonfatal "Skins\[ Media ]\*.*"
	File /nonfatal Skins\Blue3\media\*.*

	SetOutPath $SkinLocation\Blue3\media\MyAnime
	File /nonfatal "Skins\[ Media ]\MyAnime\*.*"
	File /nonfatal Skins\Blue3\media\MyAnime\*.*
	
	RMDir /r "$APPDATA\Team MediaPortal\MediaPortal\Cache\Blue3"
SectionEnd

Section /o "Blue3Wide" Blue3Wide
	SetOutPath $SkinLocation\Blue3Wide
	File Skins\Blue3Wide\*.*
	
	SetOutPath $SkinLocation\Blue3Wide\media
	File /nonfatal "Skins\[ Media ]\*.*"
	File /nonfatal Skins\Blue3Wide\media\*.*

	SetOutPath $SkinLocation\Blue3Wide\media\MyAnime
	File /nonfatal "Skins\[ Media ]\MyAnime\*.*"
	File /nonfatal Skins\Blue3Wide\media\MyAnime\*.*
	
	RMDir /r "$APPDATA\Team MediaPortal\MediaPortal\Cache\Blue3Wide"
SectionEnd

Section /o "XFactor" XFactor
	SetOutPath $SkinLocation\XFactor
	File Skins\XFactor\*.*
	
	SetOutPath $SkinLocation\XFactor\media
	File /nonfatal "Skins\[ Media ]\*.*"
	File /nonfatal Skins\XFactor\media\*.*

	SetOutPath $SkinLocation\XFactor\media\MyAnime
	File /nonfatal "Skins\[ Media ]\MyAnime\*.*"
	File /nonfatal Skins\XFactor\media\MyAnime\*.*
	
	RMDir /r "$APPDATA\Team MediaPortal\MediaPortal\Cache\XFactor"
SectionEnd

Section /o "MediaStream" MediaStream
	SetOutPath $SkinLocation\MediaStream
	File Skins\MediaStream\*.*
	
	SetOutPath $SkinLocation\MediaStream\media
	File /nonfatal "Skins\[ Media ]\*.*"
	File /nonfatal Skins\MediaStream\media\*.*

	SetOutPath $SkinLocation\MediaStream\media\MyAnime
	File /nonfatal "Skins\[ Media ]\MyAnime\*.*"
	File /nonfatal Skins\MediaStream\media\MyAnime\*.*
	
	RMDir /r "$APPDATA\Team MediaPortal\MediaPortal\Cache\MediaStream"
SectionEnd

Section /o "Monochrome 2.X" Monochrome_2X
	SetOutPath $SkinLocation\Monochrome
	File Skins\Monochrome_2X\*.*
	
	SetOutPath $SkinLocation\Monochrome\media
	File /nonfatal "Skins\[ Media ]\*.*"
	File /nonfatal Skins\Monochrome_2X\media\*.*
	
	SetOutPath $SkinLocation\Monochrome\media\MyAnime
	File /nonfatal "Skins\[ Media ]\MyAnime\*.*"
	File /nonfatal Skins\Monochrome_2X\media\MyAnime\*.*
	
	RMDir /r "$APPDATA\Team MediaPortal\MediaPortal\Cache\Monochrome"
SectionEnd

SectionGroup "Monochrome 3.2" MonochromeMPGroup

Section /o "Monochrome 3.2" Monochrome
	SetOutPath $SkinLocation\Monochrome
	File Skins\Monochrome\*.*
	
	SetOutPath $SkinLocation\Monochrome\media
	File /nonfatal "Skins\[ Media ]\*.*"
	File /nonfatal Skins\Monochrome\media\*.*
	
	SetOutPath $SkinLocation\Monochrome\media\MyAnime
	File /nonfatal "Skins\[ Media ]\MyAnime\*.*"
	File /nonfatal Skins\Monochrome\media\MyAnime\*.*

	SetOutPath $SkinLocation\Monochrome\media\Logos
	File /nonfatal "Skins\[ Media ]\Logos\*.*"
	File /nonfatal Skins\Monochrome\media\Logos\*.*
	
	RMDir /r "$APPDATA\Team MediaPortal\MediaPortal\Cache\Monochrome"
SectionEnd

Section /o "Monochrome  3.2 fanart mod" Monochrome_Fanart
	SetOutPath $SkinLocation\Monochrome
	Rename $SkinLocation\Monochrome\Anime2_Main.xml $SkinLocation\Monochrome\Anime2_Main_orignal.xml
	File /oname=Anime2_Main.xml Skins\Monochrome\Anime2_Main_Fanart.xml 
SectionEnd

SectionGroupEnd

SectionGroup "StreamedMP" StreamedMPGroup

Section /o "StreamedMP" StreamedMP
	SetOutPath $SkinLocation\StreamedMP
	File Skins\StreamedMP\*.*
	
	SetOutPath $SkinLocation\StreamedMP\media
	File /nonfatal "Skins\[ Media ]\*.*"
	File /nonfatal Skins\StreamedMP\media\*.*

	SetOutPath $SkinLocation\StreamedMP\media\MyAnime
	File /nonfatal "Skins\[ Media ]\MyAnime\*.*"
	File /nonfatal Skins\StreamedMP\media\MyAnime\*.*
	
	RMDir /r "$APPDATA\Team MediaPortal\MediaPortal\Cache\StreamedMP"
SectionEnd

Section /o "StreamedMP fanart mod" StreamedMP_Fanart
	SetOutPath $SkinLocation\StreamedMP
	Rename $SkinLocation\StreamedMP\Anime2_Main.xml $SkinLocation\StreamedMP\Anime2_Main_orignal.xml
	File /oname=Anime2_Main.xml Skins\StreamedMP\Anime2_Main_Fanart.xml 
	File /oname=Anime2_Shadows.xml Skins\StreamedMP\Anime2_Shadows_Fanart.xml 
	File /oname=Anime2_ShadowsWithJapanese.xml Skins\StreamedMP\Anime2_ShadowsWithJapanese_Fanart.xml 
	File /oname=Anime2_MainWithJapanese.xml Skins\StreamedMP\Anime2_MainWithJapanese_Fanart.xml
SectionEnd

SectionGroupEnd

Section /o "Black Glass" BlackGlass
	SetOutPath "$SkinLocation\Black Glass"
	File "Skins\Black Glass\*.*"
	
	SetOutPath "$SkinLocation\Black Glass\media"
	File /nonfatal "Skins\[ Media ]\*.*"
	File /nonfatal "Skins\Black Glass\media\*.*"

	SetOutPath "$SkinLocation\Black Glass\media\MyAnime"
	File /nonfatal "Skins\[ Media ]\MyAnime\*.*"
	File /nonfatal "Skins\Black Glass\media\MyAnime\*.*"
	
	RMDir /r "$APPDATA\Team MediaPortal\MediaPortal\Cache\Black Glass"
SectionEnd

SectionGroupEnd

!cd ..\installer\NSIS

;-----------------------------------------------------------------------------------
;Uninstaller Section



Section "un.$(NAME_Uninstall)" Uninstall

	Delete "$INSTDIR\ed2k.dll"
	Delete "$INSTDIR\plugins\Windows\Anime2.dll"
	Delete "$INSTDIR\plugins\Windows\hasher.dll"
	  
	Delete "$SkinLocation\XFactor\Anime2*"
	Delete "$SkinLocation\XFactor\media\anime2*"
	Delete "$SkinLocation\XFactor\media\MyAnime\*"
	
	Delete "$SkinLocation\Blue3Wide\Anime2*"
	Delete "$SkinLocation\Blue3Wide\media\anime2*"
	Delete "$SkinLocation\Blue3Wide\media\MyAnime\*"
	
	Delete "$SkinLocation\Blue3\Anime2*"
	Delete "$SkinLocation\Blue3\media\anime2*"
	Delete "$SkinLocation\Blue3\media\MyAnime\*"
	
	Delete "$SkinLocation\MediaStream\Anime2*"
	Delete "$SkinLocation\MediaStream\media\anime2*"
	Delete "$SkinLocation\MediaStream\media\MyAnime\*"
	
	Delete "$SkinLocation\Monochrome\Anime2*"
	Delete "$SkinLocation\Monochrome\media\anime2*"
	Delete "$SkinLocation\Monochrome\media\MyAnime\*"
	
	Delete "$SkinLocation\StreamedMP\Anime2*"
	Delete "$SkinLocation\StreamedMP\media\anime2*"
	Delete "$SkinLocation\StreamedMP\media\MyAnime\*"
	
	Delete "$SkinLocation\Black Glass\Anime2*"
	Delete "$SkinLocation\Black Glass\media\anime2*"
	Delete "$SkinLocation\Black Glass\media\MyAnime\*"

	Delete $INSTDIR\AniDBAPI.dll
	Delete $INSTDIR\Anime2Schema.dll
	Delete $INSTDIR\Anime2SQLLite.dll
	Delete $INSTDIR\Newtonsoft.Json.Net20.dll
	Delete "$DatabaseLocation"
	
	Delete "$SMPROGRAMS\Team MediaPortal\MediaPortal\$(UninstallShortcutText).lnk"
	
	Delete "$INSTDIR\Anime2\$(^name)_Uninstall.exe"

	DeleteRegKey HKLM "Software\My Anime 2"

SectionEnd

Section "un.$(NAME_DeleteBackups)" DeleteBackups
	Delete "$INSTDIR\Anime2\*"
SectionEnd

Section "un.$(NAME_DeleteImages)" DeleteImages
	RMDir /r $ThumbsLocation
SectionEnd

Section /o "un.$(NAME_BackupDatabase)" BackupDatabase
	CopyFiles "$DatabaseLocation" "$INSTDIR\Anime2"
	${WordFind} $DatabaseLocation "\" "-1" $1
	Rename "$INSTDIR\Anime2\$1" "$INSTDIR\Anime2\AnimeDatabaseV20_${Version}.db3"
SectionEnd

Section "un."
	${DirState} "$INSTDIR\Anime2" $R0
	IntCmp $R0 0 0 +2
	RMDir "$INSTDIR\Anime2"
SectionEnd

!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
  !insertmacro MUI_DESCRIPTION_TEXT ${Anime2} $(DESC_Anime2)
  !insertmacro MUI_DESCRIPTION_TEXT ${XFactor} $(DESC_XFactor)
  !insertmacro MUI_DESCRIPTION_TEXT ${Blue3} $(DESC_Blue3)
  !insertmacro MUI_DESCRIPTION_TEXT ${Blue3Wide} $(DESC_Blue3Wide)
  !insertmacro MUI_DESCRIPTION_TEXT ${MediaStream} $(DESC_MediaStream)
  !insertmacro MUI_DESCRIPTION_TEXT ${Monochrome} $(DESC_Monochrome)
  !insertmacro MUI_DESCRIPTION_TEXT ${Monochrome_Fanart} $(DESC_Monochrome_Fanart)
  !insertmacro MUI_DESCRIPTION_TEXT ${Monochrome_2X} $(DESC_Monochrome_2X)
  !insertmacro MUI_DESCRIPTION_TEXT ${StreamedMP} $(DESC_StreamedMP)
  !insertmacro MUI_DESCRIPTION_TEXT ${StreamedMP_Fanart} $(DESC_StreamedMP_Fanart)
  
  !insertmacro MUI_DESCRIPTION_TEXT ${DatabaseBackup} $(DESC_DatabaseBackup)
  !insertmacro MUI_DESCRIPTION_TEXT ${RestoreBackup} $(DESC_RestoreBackup)
!insertmacro MUI_FUNCTION_DESCRIPTION_END

!insertmacro MUI_UNFUNCTION_DESCRIPTION_BEGIN
  !insertmacro MUI_DESCRIPTION_TEXT ${Uninstall} $(DESC_Uninstall)
  !insertmacro MUI_DESCRIPTION_TEXT ${DeleteBackups} $(DESC_DeleteBackups)
  !insertmacro MUI_DESCRIPTION_TEXT ${BackupDatabase} $(DESC_BackupDatabase)
  !insertmacro MUI_DESCRIPTION_TEXT ${DeleteImages} $(DESC_DeleteImages)
!insertmacro MUI_UNFUNCTION_DESCRIPTION_END

Function .onInit
	SetShellVarContext all

	ReadRegStr $INSTDIR HKLM "Software\Team MediaPortal\MediaPortal" "ApplicationDir"
	StrCmp $INSTDIR "" 0 skin
	
	IfFileExists "$PROGRAMFILES\Team MediaPortal\MediaPortal\mediaportal.exe" 0 +3
	StrCpy $INSTDIR "$PROGRAMFILES\Team MediaPortal\MediaPortal"
	Goto skin
	
	StrCpy $INSTDIR $(FindMediaPortal)
	
	skin:
	!insertmacro MUI_LANGDLL_DISPLAY
	
	IfSilent 0 +2
	IntOp $language 0 + 1033
	
	;Check for older versions, if so, set the upgrading varible to 1
	IfFileExists "$INSTDIR\plugins\Windows\Anime2.dll" 0 +2
	IntOp $Upgrading 0 + 1
FunctionEnd

Function .onGUIEnd

FunctionEnd

Function un.onInit
	!insertmacro MUI_UNGETLANGUAGE
	
	SetShellVarContext all
	ReadRegStr $INSTDIR HKLM "Software\My Anime 2" "InstallDir"
	
	;check if MP 1.1 or newer
	IfFileExists "$APPDATA\Team MediaPortal\MediaPortal\skin\*.*" 0 +3
	StrCpy $SkinLocation "$APPDATA\Team MediaPortal\MediaPortal\skin"
	Goto SkinSection
	
	StrCpy $SkinLocation "$INSTDIR\skin"
	
	SkinSection:
	
	StrCpy $0 "$APPDATA\Team MediaPortal\MediaPortal\MediaPortal.xml"
	Push $0		
	Call un.ReadMediaPortalXml
	Pop $1
	Pop $0
	StrCpy $DatabaseLocation $0
	StrCmp $DatabaseLocation "" 0 +2
	StrCpy $DatabaseLocation "$APPDATA\Team MediaPortal\MediaPortal\database\AnimeDatabaseV20.db3"

	StrCpy $ThumbsLocation $1
	StrCmp $ThumbsLocation "" 0 +2
	StrCpy $thumbsLocation "$APPDATA\Team MediaPortal\MediaPortal\thumbs\AnimeThumbs"
	
	!insertmacro SetSectionFlag ${Uninstall} ${SF_RO}
	
	IfFileExists "$DatabaseLocation" End 0
	!insertmacro UnselectSection ${BackupDatabase}
	!insertmacro SetSectionFlag ${BackupDatabase} ${SF_RO}
	End:
FunctionEnd

Function un.onGUIEnd

FunctionEnd


Function un.onSelChange
	${if} ${SectionIsSelected} ${BackupDatabase}
	!insertmacro UnselectSection ${DeleteBackups}
	!insertmacro SetSectionFlag ${DeleteBackups} ${SF_RO}
	${EndIf}	
	
	${if} ${SectionIsSelected} ${DeleteBackups}
	!insertmacro UnselectSection ${BackupDatabase}
	!insertmacro SetSectionFlag ${BackupDatabase} ${SF_RO}
	${EndIf}
	
	${IfNot} ${SectionIsSelected} ${DeleteBackups}
	!insertmacro ClearSectionFlag ${BackupDatabase} ${SF_RO}
	${EndIf}
	
	${IfNot} ${SectionIsSelected} ${BackupDatabase}
	!insertmacro ClearSectionFlag ${DeleteBackups} ${SF_RO}
	${EndIf}
FunctionEnd

Function .onSelChange
	${If} ${SectionIsSelected} ${DatabaseBackup}
	!insertmacro UnselectSection ${RestoreBackup}
	!insertmacro SetSectionFlag ${RestoreBackup} ${SF_RO}
	Goto RestoreBackup
	${EndIf}
	
	!insertmacro ClearSectionFlag ${RestoreBackup} ${SF_RO}
		
	RestoreBackup:
	
	${If} ${SectionIsSelected} ${RestoreBackup}
	!insertmacro UnselectSection ${DatabaseBackup}
	!insertmacro SetSectionFlag ${DatabaseBackup} ${SF_RO}
	
	
	${Else}
	!insertmacro ClearSectionFlag ${DatabaseBackup} ${SF_RO}
	${EndIf}
FunctionEnd

Function ReadMediaPortalXml
	Pop $R0
	#$DatabaseLocation
	
	IntOp $NumCurLine 0 + 0
	
	${LineSum} $R0 $NumLines
	
	
	FileOpen $8 $R0 r

	Push ""
	
	loop:
	IntOp $NumCurLine $NumCurLine + 1
	IntCmp $NumCurLine $NumLines 0 +4
	Close:
	FileClose $8
	Push ""
	Return
	
	
	FileRead $8 $Line 8000
	
	StrCpy $1 $Line
	
	;removes blank space
	StrLen $StrLength $1
	IntOp $StrCharCount 0 + 0
	Find<:
	IntCmp $StrCharCount $StrLength loop 0
	StrCpy $StrChar $1 1 $StrCharCount
	StrCmp $StrChar "<" +3 0
	IntOp $StrCharCount $StrCharCount + 1
	Goto Find<
	
	
	;we have found opening bracket now to see if it is a section or a entry	
	StrCpy $StrChar $1 "" $StrCharCount
	${TrimNewLines} $StrChar $StrChar
	
	
	
	IntCmp $AnimeSection 1 +3 0
	StrCmp $StrChar '<section name="Anime2">' 0 loop
	IntOp $AnimeSection 0 + 1
	
	
	StrCmp $StrChar "</section>" 0 +2
	Goto Close
	
	
	StrCpy $Line $StrChar 27
	StrCmp $Line '<entry name="DatabaseFile">' 0 loop
	
	StrCpy $Line $StrChar "" -8
	StrCmp $Line </entry> 0 +4
	StrCpy $1 $StrChar "-8" 27	
	Push $1
	Return
	
	StrCpy $1 ""
	Push $1
FunctionEnd

Function un.ReadMediaPortalXml
	Pop $R0
	#$DatabaseLocation

	
	IntOp $NumCurLine 0 + 0
	
	${LineSum} $R0 $NumLines
	
	
	FileOpen $8 $R0 r

	Push ""
	Push ""
	
	loop:
	IntOp $NumCurLine $NumCurLine + 1
	IntCmp $NumCurLine $NumLines 0 +4
	Close:
	FileClose $8
	Push ""
	Return
	
	
	FileRead $8 $Line 8000
	
	StrCpy $1 $Line
	
	;removes blank space
	StrLen $StrLength $1
	IntOp $StrCharCount 0 + 0
	Find<:
	IntCmp $StrCharCount $StrLength loop 0
	StrCpy $StrChar $1 1 $StrCharCount
	StrCmp $StrChar "<" +3 0
	IntOp $StrCharCount $StrCharCount + 1
	Goto Find<
	
	
	;we have found opening bracket now to see if it is a section or a entry	
	StrCpy $StrChar $1 "" $StrCharCount
	${TrimNewLines} $StrChar $StrChar
	
	IntCmp $AnimeSection 1 +3 0
	StrCmp $StrChar '<section name="Anime2">' 0 loop
	IntOp $AnimeSection 0 + 1
	
	
	StrCmp $StrChar "</section>" 0 +2
	Goto Close
	
	
	StrCpy $Line $StrChar 27
	StrCmp $Line '<entry name="DatabaseFile">' 0 Thumbs
	
	StrCpy $Line $StrChar "" -8
	StrCmp $Line </entry> 0 +4
	StrCpy $1 $StrChar "-8" 27	
	Push $1
	Goto loop
	
	StrCpy $1 ""
	Push $1
	Goto loop
	
	Thumbs:
	StrCpy $Line $StrChar 27
	StrCmp $Line '<entry name="ThumbsFolder">' 0 loop
	
	StrCpy $Line $StrChar "" -8
	StrCmp $Line </entry> 0 +4
	StrCpy $1 $StrChar "-8" 27	
	Push $1
	Goto loop
	
	StrCpy $1 ""
	Push $1
	Goto loop
FunctionEnd

Function DirectorySet
	StrCpy $SkinLocation "$INSTDIR\skin"
	
	## Get previous version
	GetDllVersion "$INSTDIR\plugins\windows\Anime2.dll" $R0 $R1
	IntOp $R2 $R0 / 0x00010000
	IntOp $R3 $R0 & 0x0000FFFF
	IntOp $R4 $R1 / 0x00010000
	IntOp $R5 $R1 & 0x0000FFFF
	StrCpy $PreviousVersion "$R2.$R3.$R4.$R5"
	
	;check if MP 1.1 or newer
	IfFileExists "$APPDATA\Team MediaPortal\MediaPortal\skin\*.*" 0 +3
	StrCpy $SkinLocation "$APPDATA\Team MediaPortal\MediaPortal\skin"
	Goto BackupSection
	
	DetailPrint "Backup database operations"
	
	BackupSection:
	;---Backup database section---
	DetailPrint "Getting database location"
	StrCpy $0 "$APPDATA\Team MediaPortal\MediaPortal\MediaPortal.xml"
	Push $0		
	Call ReadMediaPortalXml
	Pop $0
	StrCpy $DatabaseLocation $0
	StrCmp $DatabaseLocation "" 0 +2
	StrCpy $DatabaseLocation "$APPDATA\Team MediaPortal\MediaPortal\database\AnimeDatabaseV20.db3"
	
	IfFileExists $DatabaseLocation 0 BackupDatabaseDisable
	!insertmacro selectSection ${DatabaseBackup}
	!insertmacro SetSectionFlag ${RestoreBackup} ${SF_RO}
	Goto restore
	
	BackupDatabaseDisable:
	!insertmacro SetSectionFlag ${DatabaseBackup} ${SF_RO}
	
	restore:
	#!insertmacro ClearSectionFlag ${RestoreBackup} ${SF_RO}
	IfFileExists $INSTDIR\anime2\AnimeDatabaseV20_${Version}.db3 0 RestoreBackupSet
	${Unless} ${SectionIsSelected} ${DatabaseBackup}
	!insertmacro SetSectionFlag ${DatabaseBackup} ${SF_RO}
	!insertmacro selectSection ${RestoreBackup}
	${ENDIF}
	Goto DefaultFlags
	
	RestoreBackupSet:
	!insertmacro SetSectionFlag ${RestoreBackup} ${SF_RO}	
	
	DefaultFlags:
	IntOp $0 $0 | ${SF_SELECTED}
	IntOp $0 $0 | ${SF_RO}
	SectionSetFlags ${Anime2} $0
	
	IntOp $0 0 + 0
	IfFileExists $SkinLocation\xfactor\basichome.xml 0 +3
	IntOp $0 $0 | ${SF_SELECTED}
	SectionSetFlags ${XFactor} $0
	
	IntOp $0 0 + 0
	IfFileExists $SkinLocation\Blue3wide\basichome.xml 0 +3
	IntOp $0 $0 | ${SF_SELECTED}
	SectionSetFlags ${Blue3wide} $0
	
	IntOp $0 0 + 0
	IfFileExists $SkinLocation\Blue3\basichome.xml 0 +3
	IntOp $0 $0 | ${SF_SELECTED}
	SectionSetFlags ${Blue3} $0
	
	IntOp $0 0 + 0
	IfFileExists $SkinLocation\MediaStream\basichome.xml 0 +3
	IntOp $0 $0 | ${SF_SELECTED}
	SectionSetFlags ${MediaStream} $0
	
	IntOp $0 0 + 0
	IfFileExists $SkinLocation\Monochrome\basichome.xml 0 +3
	IntOp $0 $0 | ${SF_SELECTED}
	SectionSetFlags ${Monochrome} $0
	
	IntOp $0 0 + 0
	IfFileExists "$SkinLocation\Black Glass\basichome.xml" 0 +3
	IntOp $0 $0 | ${SF_SELECTED}
	SectionSetFlags ${BlackGlass} $0
	
	IntOp $0 0 + 0
	IfFileExists $SkinLocation\StreamedMP\basichome.xml 0 +7
	IntOp $0 $0 | ${SF_SELECTED}
	SectionSetFlags ${StreamedMP} $0
	IntOp $0 0 + 0
	SectionGetFlags ${StreamedMPGroup} $0
	IntOp $0 $0 | ${SF_EXPAND}
	SectionSetFlags ${StreamedMPGroup} $0
FunctionEnd

Function SetFinishPageOptions
	${If} $Upgrading = 1
		SendMessage $mui.FinishPage.ShowReadme ${BM_SETCHECK} ${BST_UNCHECKED} 0 ;uncheck quickstart guide
	${EndIf}
FunctionEnd