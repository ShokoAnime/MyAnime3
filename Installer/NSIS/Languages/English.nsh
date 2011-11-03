;Language: English (1033)
;By Craig Emmott

  LangString NameText ${LANG_ENGLISH} "My Anime 2"
  LangString CaptionText ${LANG_ENGLISH} "$(^name) - Bring your anime to life!"
  LangString BrandNameText ${LANG_ENGLISH} "$(^name) v${Version}"
  
  LangString FindMediaPortal ${LANG_ENGLISH} "Please enter mediaportal install directory"

  LangString WelcomeTitleText ${LANG_ENGLISH} "$(^name), the anime plug-in of choice for MediaPortal"
  LangString WelcomeText ${LANG_ENGLISH} "$(^name) is a plugin for MediaPortal allowing you to watch your anime in style. It will find your anime and present them to you in a variety of fantastic skins and views. Series and episode information is gathered for you. Fanart, posters, and banners are shown to spice up your anime list.$\r$\n$\r$\nIf you have any troubles or just want to thank the developers for this great gift, come see us at: $\r$\nhttp://www.otakumm.com$\r$\n$\r$\nProject lead: leowerndly$\r$\nDevelopers: bert_r, maximo.piva, Craige1$\r$\nSpecial thanks: Raytestrak (MediaStream skin), yhoogi (streamedmp fanart skin)"

  ;Uses the same as WelcomeTitleText
  ;LangString MUI_UNTEXT_WELCOME_INFO_TITLE ${LANG_ENGLISH} "$(^name), the anime plug-in of choice for MediaPortal"
  LangString UnWelcomeText ${LANG_ENGLISH} "This wizard will guide you through uninstalling $(^NameDA). NOTE: If you are updating to a newer version, simply run its installer. It will handle previous versions for you."
  
  LangString ComponentsText ${LANG_ENGLISH} "Here you can choose which actions you would like taken and which skins you would like to have supported. Skins you have currently are automatically selected."
  LangString DirectoryText ${LANG_ENGLISH} "Please enter the MediaPortal install directory. This should be automatically detected. If not, please enter it now."

  LangString FinishText ${LANG_ENGLISH} "$(^name) has been installed successfully. You may now setup $(^name) with your aniDB account and set the folders containing your anime. There are many other settings as well, so please take the time to look through them. For more information on setting up $(^name), visit our website, and our Google code site."
  LangString FinishReadmeText ${LANG_ENGLISH} "View QuickStart guide"
  LangString FinishLinkText ${LANG_ENGLISH} "Visit us at our website"
  LangString StartConfigText ${LANG_ENGLISH} "Setup My Anime"
  
    LangString SkinsText ${LANG_ENGLISH} "Skins"
	LangString UninstallShortcutText ${LANG_ENGLISH} "Uninstall $(^name)"
	LangString ForumLinkText ${LANG_ENGLISH} "Forum"
	LangString HomePageLinkText ${LANG_ENGLISH} "Website"
	LangString ManualLinkText ${LANG_ENGLISH} "Manual"
	LangString ConfigLinkText ${LANG_ENGLISH} "Configuration dialog"
	LangString ForumLinkDescriptionText ${LANG_ENGLISH} "Visit our forums for help or to meet the community."
	LangString HomePageLinkDescriptionText ${LANG_ENGLISH} "See our website for the latest news, downloads, and other things relating to My Anime."
	LangString ManualLinkDescriptionText ${LANG_ENGLISH} "Take a look at the manual if you are stuck setting up or using My Anime."
	LangString ConfigLinkDescriptionText ${LANG_ENGLISH} "Setup My Anime."
  
	LangString DESC_Anime2 ${LANG_ENGLISH} "Will install all the necessary files required for $(^name) to run. Can not be disabled."
	LangString DESC_XFactor ${LANG_ENGLISH} "XFactor skin support."
	LangString DESC_Default ${LANG_ENGLISH} "Default skin support."
	LangString DESC_DefaultWide ${LANG_ENGLISH} "DefaultWide skin support."
	LangString DESC_Monochrome ${LANG_ENGLISH} "Monochrome 3.2 skin support. (Requires at least MP 1.1)"
	LangString DESC_Monochrome_Fanart ${LANG_ENGLISH} "Monochrome 3.2 fanart mod."
	LangString DESC_Monochrome_2X ${LANG_ENGLISH} "Earlier versions of Monochrome"
	LangString DESC_MediaStream ${LANG_ENGLISH} "MediaStream skin support."
	LangString DESC_StreamedMP ${LANG_ENGLISH} "StreamedMP skin support."
	LangString DESC_StreamedMP_Fanart ${LANG_ENGLISH} "StreamedMP fanart mod."
	
	LangString NAME_BackupsRestores ${LANG_ENGLISH} "Backups and restores"
	LangString NAME_DatabaseBackup ${LANG_ENGLISH} "Backup database"
	LangString NAME_RestoreBackup ${LANG_ENGLISH} "Restore database"
	LangString NAME_FontEngineBackup ${LANG_ENGLISH} "Backup FontEngine"
	LangString DESC_DatabaseBackup ${LANG_ENGLISH} "Backup the current database to the Anime2 folder in MediaPortal. Unavailable if no database is found."
	LangString DESC_RestoreBackup ${LANG_ENGLISH} "If you have installed this version of $(^name) before, this will restore it's database. Unavailable if this version's database is not found."
	LangString DESC_FontEngineBackup ${LANG_ENGLISH} "Backup up the FontEngine.dll inside MediaPortal to the Anime2 folder. Unavailable if already backed up."
	
	LangString NAME_Uninstall ${LANG_ENGLISH} "Uninstall main files"
	LangString NAME_DeleteBackups ${LANG_ENGLISH} "Delete backups"
	LangString NAME_BackupDatabase ${LANG_ENGLISH} "Backup database"
	LangString NAME_DeleteImages ${LANG_ENGLISH} "Delete images"	
	LangString DESC_Uninstall ${LANG_ENGLISH} "Uninstall $(^name) core files."
	LangString DESC_DeleteBackups ${LANG_ENGLISH} "Delete all backups made by $(^name). Can not backup database with this mode."
	LangString DESC_BackupDatabase ${LANG_ENGLISH} "Backup current database to $(^name) backup folder."
	LangString DESC_DeleteImages ${LANG_ENGLISH} "This will delete all the fanart/posters/banners and other images stored by $(^name)"
	
	
	
  ;These commented lines are already translated to their default by the NSIS language files. Uncomment line to replace defaults.
  
  ;LangString MUI_TEXT_WELCOME_INFO_TITLE ${LANG_ENGLISH} "$(^name), the anime plug-in of choice for MediaPortal"
  ;LangString MUI_TEXT_WELCOME_INFO_TEXT ${LANG_ENGLISH} "$(^name) is a plugin for MediaPortal allowing you to watch your anime in style. It will find your anime and present them to you in a variety of fantastic skins and views. Series and episode information is gathered for you. Fanart, posters, and banners are shown to spice up your anime list.$\r$\n$\r$\nIf you have any troubles or just want to thank the developers for this great gift, come see us at: $\r$\nhttp://forum.team-mediaportal.com/mediaportal-plugins-47/$\r$\nmy-anime-2-a-60793/$\r$\n$\r$\nProject lead: leowerndly$\r$\nDevelopers: bert_r, maximo.piva, Craige1"


  ;LangString MUI_UNTEXT_WELCOME_INFO_TITLE ${LANG_ENGLISH} "$(^name), the anime plug-in of choice for MediaPortal"
  ;LangString MUI_UNTEXT_WELCOME_INFO_TEXT ${LANG_ENGLISH} "This wizard will guide you through uninstalling $(^NameDA). WARNING: This will remove your anime database. If you are updating to a newer version, simply run its installer. It will handle previous versions like this one for you."



  ;LangString MUI_TEXT_LICENSE_TITLE ${LANG_ENGLISH} "License Agreement"
  ;LangString MUI_TEXT_LICENSE_SUBTITLE ${LANG_ENGLISH} "Please review the license terms before installing $(^NameDA)."
  ;LangString MUI_INNERTEXT_LICENSE_BOTTOM ${LANG_ENGLISH} "If you accept the terms of the agreement, click I Agree to continue. You must accept the agreement to install $(^NameDA)."
  ;LangString MUI_INNERTEXT_LICENSE_BOTTOM_CHECKBOX ${LANG_ENGLISH} "If you accept the terms of the agreement, click the check box below. You must accept the agreement to install $(^NameDA). $_CLICK"
  ;LangString MUI_INNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS ${LANG_ENGLISH} "If you accept the terms of the agreement, select the first option below. You must accept the agreement to install $(^NameDA). $_CLICK"



  ;LangString MUI_UNTEXT_LICENSE_TITLE ${LANG_ENGLISH} "License Agreement"
  ;LangString MUI_UNTEXT_LICENSE_SUBTITLE ${LANG_ENGLISH} "Please review the license terms before uninstalling $(^NameDA)."
  ;LangString MUI_UNINNERTEXT_LICENSE_BOTTOM ${LANG_ENGLISH} "If you accept the terms of the agreement, click I Agree to continue. You must accept the agreement to uninstall $(^NameDA)."
  ;LangString MUI_UNINNERTEXT_LICENSE_BOTTOM_CHECKBOX ${LANG_ENGLISH} "If you accept the terms of the agreement, click the check box below. You must accept the agreement to uninstall $(^NameDA). $_CLICK"
  ;LangString MUI_UNINNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS ${LANG_ENGLISH} "If you accept the terms of the agreement, select the first option below. You must accept the agreement to uninstall $(^NameDA). $_CLICK"



  ;LangString MUI_INNERTEXT_LICENSE_TOP ${LANG_ENGLISH} "Press Page Down to see the rest of the agreement."



  ;LangString MUI_TEXT_COMPONENTS_TITLE ${LANG_ENGLISH} "Choose Components"
  ;LangString MUI_TEXT_COMPONENTS_SUBTITLE ${LANG_ENGLISH} "Choose which features of $(^NameDA) you want to install."


  ;LangString MUI_UNTEXT_COMPONENTS_TITLE ${LANG_ENGLISH} "Choose Components"
  ;LangString MUI_UNTEXT_COMPONENTS_SUBTITLE ${LANG_ENGLISH} "Choose which features of $(^NameDA) you want to install."



  ;LangString MUI_INNERTEXT_COMPONENTS_DESCRIPTION_TITLE ${LANG_ENGLISH} "Description"
  ;LangString MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO ${LANG_ENGLISH} "Position your mouse over a component to see its description."
  

  ;LangString MUI_TEXT_DIRECTORY_TITLE ${LANG_ENGLISH} "Choose Install Location"
  ;LangString MUI_TEXT_DIRECTORY_SUBTITLE ${LANG_ENGLISH} "Choose the folder in which to install $(^NameDA)."


  ;LangString MUI_UNTEXT_DIRECTORY_TITLE ${LANG_ENGLISH} "Choose Uninstall Location"
  ;LangString MUI_UNTEXT_DIRECTORY_SUBTITLE ${LANG_ENGLISH} "Choose the folder from which to uninstall $(^NameDA)."



  ;LangString MUI_TEXT_INSTALLING_TITLE ${LANG_ENGLISH} "Installing"
  ;LangString MUI_TEXT_INSTALLING_SUBTITLE ${LANG_ENGLISH} "Please wait while $(^NameDA) is being installed."
  ;LangString MUI_TEXT_FINISH_TITLE ${LANG_ENGLISH} "Installation Complete"
  ;LangString MUI_TEXT_FINISH_SUBTITLE ${LANG_ENGLISH} "Setup was completed successfully."
  ;LangString MUI_TEXT_ABORT_TITLE ${LANG_ENGLISH} "Installation Aborted"
  ;LangString MUI_TEXT_ABORT_SUBTITLE ${LANG_ENGLISH} "Setup was not completed successfully."


  ;LangString MUI_UNTEXT_UNINSTALLING_TITLE ${LANG_ENGLISH} "Uninstalling"
  ;LangString MUI_UNTEXT_UNINSTALLING_SUBTITLE ${LANG_ENGLISH} "Please wait while $(^NameDA) is being uninstalled."
  ;LangString MUI_UNTEXT_FINISH_TITLE ${LANG_ENGLISH} "Uninstallation Complete"
  ;LangString MUI_UNTEXT_FINISH_SUBTITLE ${LANG_ENGLISH} "Uninstall was completed successfully."
  ;LangString MUI_UNTEXT_ABORT_TITLE ${LANG_ENGLISH} "Uninstallation Aborted"
  ;LangString MUI_UNTEXT_ABORT_SUBTITLE ${LANG_ENGLISH} "Uninstall was not completed successfully."



  ;LangString MUI_TEXT_FINISH_INFO_TITLE ${LANG_ENGLISH} "Completing the $(^NameDA) Setup Wizard"
  ;LangString MUI_TEXT_FINISH_INFO_TEXT ${LANG_ENGLISH} "My Anime 2 has been installed successfully. You may now setup anime2 with your aniDB account and set the folders containing your anime. There are many other settings as well, so please take the time to look through them. For more information on setting up My Anime 2, visit our website, and our Google code site."
  ;LangString MUI_TEXT_FINISH_INFO_REBOOT ${LANG_ENGLISH} "Your computer must be restarted in order to complete the installation of $(^NameDA). Do you want to reboot now?"



  ;LangString MUI_UNTEXT_FINISH_INFO_TITLE ${LANG_ENGLISH} "Completing the $(^NameDA) Uninstall Wizard"
  ;LangString MUI_UNTEXT_FINISH_INFO_TEXT ${LANG_ENGLISH} "$(^NameDA) has been uninstalled from your computer.$\r$\n$\r$\nClick Finish to close this wizard."
  ;LangString MUI_UNTEXT_FINISH_INFO_REBOOT ${LANG_ENGLISH} "Your computer must be restarted in order to complete the uninstallation of $(^NameDA). Do you want to reboot now?"



  ;LangString MUI_TEXT_FINISH_REBOOTNOW ${LANG_ENGLISH} "Reboot now"
  ;LangString MUI_TEXT_FINISH_REBOOTLATER ${LANG_ENGLISH} "I want to manually reboot later"
  ;LangString MUI_TEXT_FINISH_RUN ${LANG_ENGLISH} "&Run $(^NameDA)"
  ;LangString MUI_TEXT_FINISH_SHOWREADME ${LANG_ENGLISH} "View QuickStart guide"
  ;LangString MUI_BUTTONTEXT_FINISH ${LANG_ENGLISH} "&Finish"  



  ;LangString MUI_TEXT_STARTMENU_TITLE ${LANG_ENGLISH} "Choose Start Menu Folder"
  ;LangString MUI_TEXT_STARTMENU_SUBTITLE ${LANG_ENGLISH} "Choose a Start Menu folder for the $(^NameDA) shortcuts."
  ;LangString MUI_INNERTEXT_STARTMENU_TOP ${LANG_ENGLISH} "Select the Start Menu folder in which you would like to create the program's shortcuts. You can also enter a name to create a new folder."
  ;LangString MUI_INNERTEXT_STARTMENU_CHECKBOX ${LANG_ENGLISH} "Do not create shortcuts"



  ;LangString MUI_UNTEXT_CONFIRM_TITLE ${LANG_ENGLISH} "Uninstall $(^NameDA)"
  ;LangString MUI_UNTEXT_CONFIRM_SUBTITLE ${LANG_ENGLISH} "Remove $(^NameDA) from your computer."



  ;LangString MUI_TEXT_ABORTWARNING ${LANG_ENGLISH} "Are you sure you want to quit $(^Name) Setup?"



  ;LangString MUI_UNTEXT_ABORTWARNING ${LANG_ENGLISH} "Are you sure you want to quit $(^Name) Uninstall?"



  ;LangString MULTIUSER_TEXT_INSTALLMODE_TITLE ${LANG_ENGLISH} "Choose Users"
  ;LangString MULTIUSER_TEXT_INSTALLMODE_SUBTITLE ${LANG_ENGLISH} "Choose for which users you want to install $(^NameDA)."
  ;LangString MULTIUSER_INNERTEXT_INSTALLMODE_TOP ${LANG_ENGLISH} "Select whether you want to install $(^NameDA) for yourself only or for all users of this computer. $(^ClickNext)"
  ;LangString MULTIUSER_INNERTEXT_INSTALLMODE_ALLUSERS ${LANG_ENGLISH} "Install for anyone using this computer"
  ;LangString MULTIUSER_INNERTEXT_INSTALLMODE_CURRENTUSER ${LANG_ENGLISH} "Install just for me"
  