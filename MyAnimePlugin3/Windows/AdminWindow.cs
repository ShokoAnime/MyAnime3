using System;
using System.Collections.Generic;
using MediaPortal.GUI.Library;
using MyAnimePlugin3.DataHelpers;
using System.IO;
using System.ComponentModel;
using MediaPortal.Dialogs;
using MyAnimePlugin3.Providers.TheTvDB;
using System.Xml;
using MyAnimePlugin3.Providers.TheMovieDB;
using BinaryNorthwest;
using Action = MediaPortal.GUI.Library.Action;
using MyAnimePlugin3.ImageManagement;
using MyAnimePlugin3.ViewModel;

namespace MyAnimePlugin3.Windows
{
	public class AdminWindow : GUIWindow
	{
		[SkinControlAttribute(50)] protected GUIFacadeControl m_Facade = null;

		[SkinControlAttribute(3511)] protected GUILabelControl dummyServerStatus = null;
		[SkinControlAttribute(3512)] protected GUILabelControl dummyListUnlinkedFiles = null;

		[SkinControlAttribute(101)] protected GUIButtonControl btnServerStatus = null;
		[SkinControlAttribute(102)] protected GUIButtonControl btnListUnlinkedFiles = null;

		[SkinControlAttribute(6)] protected GUIButtonControl btnRunImport = null;
		[SkinControlAttribute(7)] protected GUIButtonControl btnRetryUnlinkedFiles = null;
		[SkinControlAttribute(8)] protected GUIButtonControl btnMoreOptions = null;

		[SkinControlAttribute(920)] protected GUIButtonControl btnWindowContinueWatching = null;
		//[SkinControlAttribute(921)] protected GUIButtonControl btnWindowUtilities = null;
		[SkinControlAttribute(922)] protected GUIButtonControl btnWindowCalendar = null;
		[SkinControlAttribute(923)] protected GUIButtonControl btnWindowDownloads = null;
		//[SkinControlAttribute(924)] protected GUIButtonControl btnWindowCollectionStats = null;

		private static readonly string SkinImportProgress = "#Anime3.Utilities.ImportProgress";
		private static readonly string SkinMiscProgress = "#Anime3.Utilities.MiscProgress";

		private static readonly string SkinHasherQueueCount = "#Anime3.Utilities.HasherQueueCount";
		private static readonly string SkinHasherQueueStatus = "#Anime3.Utilities.HasherQueueStatus";

		private static readonly string SkinAniDBQueueCount = "#Anime3.Utilities.AniDBQueueCount";
		private static readonly string SkinAniDBQueueStatus = "#Anime3.Utilities.AniDBQueueStatus";

		private static readonly string SkinImageQueueCount = "#Anime3.Utilities.ImageQueueCount";
		private static readonly string SkinImageQueueStatus = "#Anime3.Utilities.ImageQueueStatus";

		private static readonly string SkinTvDBQueueCount = "#Anime3.Utilities.TvDBQueueCount";
		private static readonly string SkinTvDBQueueStatus = "#Anime3.Utilities.TvDBQueueStatus";

		private BackgroundWorker workerRefreshUnlinkedFiles = new BackgroundWorker();

		private bool FirstLoad = true;


		public AdminWindow()
		{
			// get ID of windowplugin belonging to this setup
			// enter your own unique code
			GetID = Constants.WindowIDs.ADMIN;

			MediaPortal.GUI.Library.GUIPropertyManager.SetProperty(SkinMiscProgress, "");
			MediaPortal.GUI.Library.GUIPropertyManager.SetProperty(SkinImportProgress, "");
			MediaPortal.GUI.Library.GUIPropertyManager.SetProperty(SkinHasherQueueStatus, "");
			MediaPortal.GUI.Library.GUIPropertyManager.SetProperty(SkinAniDBQueueStatus, "");
			MediaPortal.GUI.Library.GUIPropertyManager.SetProperty(SkinImageQueueStatus, "");
			MediaPortal.GUI.Library.GUIPropertyManager.SetProperty(SkinTvDBQueueStatus, "");

			workerRefreshUnlinkedFiles = new BackgroundWorker();
			workerRefreshUnlinkedFiles.WorkerReportsProgress = true;
			workerRefreshUnlinkedFiles.WorkerSupportsCancellation = true;

			workerRefreshUnlinkedFiles.DoWork += new DoWorkEventHandler(workerRefreshUnlinkedFiles_DoWork);
			workerRefreshUnlinkedFiles.RunWorkerCompleted += new RunWorkerCompletedEventHandler(workerRefreshUnlinkedFiles_RunWorkerCompleted);
			workerRefreshUnlinkedFiles.ProgressChanged += new ProgressChangedEventHandler(workerRefreshUnlinkedFiles_ProgressChanged);

			
		}

		void Instance_ServerStatusEvent(Events.ServerStatusEventArgs ev)
		{
			string msg = string.Format("JMM Server Status: {0}/{1} -- {2}/{3}", ev.GeneralQueueState, ev.GeneralQueueCount, ev.HasherQueueState, ev.HasherQueueCount);
			BaseConfig.MyAnimeLog.Write(msg);

			/*
			clearGUIProperty("Utilities.Status.HasherQueueCount");
			clearGUIProperty("Utilities.Status.HasherQueueState");
			clearGUIProperty("Utilities.Status.HasherQueueRunning");

			clearGUIProperty("Utilities.Status.GeneralQueueCount");
			clearGUIProperty("Utilities.Status.GeneralQueueState");
			clearGUIProperty("Utilities.Status.GeneralQueueRunning");*/

			setGUIProperty("Utilities.Status.HasherQueueCount", ev.HasherQueueCount.ToString());
			setGUIProperty("Utilities.Status.HasherQueueState", ev.HasherQueueState);
			setGUIProperty("Utilities.Status.HasherQueueRunning", ev.HasherQueueRunning ? "Running" : "Paused");

			setGUIProperty("Utilities.Status.GeneralQueueCount", ev.GeneralQueueCount.ToString());
			setGUIProperty("Utilities.Status.GeneralQueueState", ev.GeneralQueueState);
			setGUIProperty("Utilities.Status.GeneralQueueRunning", ev.GeneralQueueRunning ? "Running" : "Paused");

			setGUIProperty("Utilities.Status.ImagesQueueCount", ev.ImagesQueueCount.ToString());
			setGUIProperty("Utilities.Status.ImagesQueueState", ev.ImagesQueueState);
			setGUIProperty("Utilities.Status.ImagesQueueRunning", ev.ImagesQueueRunning ? "Running" : "Paused");

		}


		void workerScanDrop_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			MediaPortal.GUI.Library.GUIPropertyManager.SetProperty(SkinMiscProgress, e.UserState as string);
		}

		void workerScanDrop_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{	
		}

		

        public override void OnAction(MediaPortal.GUI.Library.Action action)
        {
            switch (action.wID)
            {
                case MediaPortal.GUI.Library.Action.ActionType.ACTION_MOVE_DOWN:
                case MediaPortal.GUI.Library.Action.ActionType.ACTION_MOVE_LEFT:
                case MediaPortal.GUI.Library.Action.ActionType.ACTION_MOVE_RIGHT:
                case MediaPortal.GUI.Library.Action.ActionType.ACTION_MOVE_UP:


                    base.OnAction(action);
                    break;
                default:
                    base.OnAction(action);
                    break;
            }
        }

		private void ShowPageServerStatus()
		{
			setGUIProperty("Utilities.CurrentView", "Server Status");
			if (dummyServerStatus != null) dummyServerStatus.Visible = true;
			if (dummyListUnlinkedFiles != null) dummyListUnlinkedFiles.Visible = false;
		}

		private void ShowPageUnlinkedFiles()
		{
			setGUIProperty("Utilities.CurrentView", "Unlinked Files");
			if (dummyServerStatus != null) dummyServerStatus.Visible = false;
			if (dummyListUnlinkedFiles != null) dummyListUnlinkedFiles.Visible = true;

			RefreshUnlinkedFiles();
		}

		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			if (MA3WindowManager.HandleWindowChangeButton(control))
				return;

			if (this.btnServerStatus != null && control == this.btnServerStatus)
			{
				ShowPageServerStatus();
				this.btnServerStatus.IsFocused = false;
				m_Facade.Focus = true;
			}

			if (this.btnListUnlinkedFiles != null && control == this.btnListUnlinkedFiles)
			{
				ShowPageUnlinkedFiles();
				this.btnListUnlinkedFiles.IsFocused = false;
				m_Facade.Focus = true;
			}

			if (this.btnRunImport != null && control == this.btnRunImport)
			{
				this.btnRunImport.IsFocused = false;
				m_Facade.Focus = true;
				JMMServerVM.Instance.clientBinaryHTTP.RunImport();

				ShowPageServerStatus();
				
				Utils.DialogMsg("Done", "Process is running on the server");
                return;
			}

			if (this.btnRetryUnlinkedFiles != null && control == this.btnRetryUnlinkedFiles)
			{
				this.btnRunImport.IsFocused = false;
				m_Facade.Focus = true;

				JMMServerVM.Instance.clientBinaryHTTP.RescanUnlinkedFiles();

				ShowPageServerStatus();

				Utils.DialogMsg("Done", "Process is running on the server");

				return;
			}

			if (this.btnMoreOptions != null && control == this.btnMoreOptions)
			{
				ShowMoreOptionsMenu();
			}



			if (actionType == MediaPortal.GUI.Library.Action.ActionType.ACTION_SELECT_ITEM)
				OnShowContextMenu();

			base.OnClicked(controlId, control, actionType);
		}

		private void ShowMoreOptionsMenu()
		{
			GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);

			//keep showing the dialog until the user closes it
			while (true)
			{
				dlg.Reset();
				dlg.SetHeading("Options");

				dlg.Add("Remove records without physical file");
				dlg.Add("Sync Votes from AniDB");
				dlg.Add("Sync MyList from AniDB");
				dlg.Add("Sync Trakt Info");
				
				dlg.DoModal(GUIWindowManager.ActiveWindow);

				BaseConfig.MyAnimeLog.Write("dlg.SelectedLabel: {0}", dlg.SelectedLabel.ToString());

				switch (dlg.SelectedLabel)
				{
					case 0:
                       
						return;

					case 1:
						
						m_Facade.Focus = true;
						return;

					case 2:
						
						m_Facade.Focus = true;
						return;

					case 3:
						
						m_Facade.Focus = true;
						return;

					case 4:
						
						m_Facade.Focus = true;
						return;
                        

					default:
						//don't reopen dialog
						return;
				}
			}
		}

		public override int GetID
		{
			get { return Constants.WindowIDs.ADMIN; }
			set { base.GetID = value; }
		}



		protected override void OnPageLoad()
		{
			System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();

			if (a != null)
			{
				MediaPortal.GUI.Library.GUIPropertyManager.SetProperty("#Anime3.VersionNumber", Utils.GetApplicationVersion(a));
			}

			setGUIProperty("Utilities.CurrentView", "Unlinked Files");

			//TODO
            //MediaPortal.GUI.Library.GUIPropertyManager.SetProperty("#Anime3.DBVersionNumber", Database.DBVersion.ToString());

			MediaPortal.GUI.Library.GUIPropertyManager.SetProperty("#Anime3.LatestVersionNumber", "-");
			MediaPortal.GUI.Library.GUIPropertyManager.SetProperty("#Anime3.LatestVersionText", "Plugin up to date");


			base.OnPageLoad();
			//TODO
            //if (!workerPluginVersion.IsBusy)
			//    workerPluginVersion.RunWorkerAsync();

			if (dummyServerStatus != null) dummyServerStatus.Visible = false;
			if (dummyListUnlinkedFiles != null) dummyListUnlinkedFiles.Visible = true;

			RefreshUnlinkedFiles();

			if (FirstLoad)
			{
				JMMServerVM.Instance.ServerStatusEvent += new JMMServerVM.ServerStatusEventHandler(Instance_ServerStatusEvent);
				FirstLoad = false;
			}
		}

		void workerPluginVersion_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			PluginVersion pVersion = e.Result as PluginVersion;
			if (pVersion != null)
			{
				System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
				if (a != null)
				{
					if (pVersion.VersionNumber.Trim().Length > 0)
					{
						MediaPortal.GUI.Library.GUIPropertyManager.SetProperty("#Anime3.LatestVersionNumber", pVersion.VersionNumber.Trim());
						if (Utils.GetApplicationVersion(a) != pVersion.VersionNumber.Trim())
							MediaPortal.GUI.Library.GUIPropertyManager.SetProperty("#Anime3.LatestVersionText", "Version " + pVersion.VersionNumber.Trim() + " Available");
					}
				}
			}
		}

		void workerPluginVersion_DoWork(object sender, DoWorkEventArgs e)
		{
			//PluginVersion pVersion = XMLService.Get_PluginVersion();
			//e.Result = pVersion;
			
		}

		#region TvDB

		private void RefreshUnlinkedFiles()
		{
            if (!workerRefreshUnlinkedFiles.IsBusy)
            {
                if (m_Facade == null) return;
				m_Facade.Clear();

				setGUIProperty("Utilities.UnlinkedFilesCount", "Loading...");

				clearGUIProperty("Utilities.UnlinkedFile.Folder");
				clearGUIProperty("Utilities.UnlinkedFile.FileName");
				clearGUIProperty("Utilities.UnlinkedFile.Size");
				clearGUIProperty("Utilities.UnlinkedFile.Hash");
				clearGUIProperty("Utilities.UnlinkedFile.FileExists");

				workerRefreshUnlinkedFiles.RunWorkerAsync();
            }
		}

		void workerRefreshUnlinkedFiles_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{

		}

		void workerRefreshUnlinkedFiles_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			List<GUIListItem> listItems = e.Result as List<GUIListItem>;

			foreach (GUIListItem itm in listItems)
			{
				m_Facade.Add(itm);
			}

			setGUIProperty("Utilities.UnlinkedFilesCount", listItems.Count.ToString());
			if (listItems.Count > 0)
			{
				m_Facade.SelectedListItemIndex = 0;
				m_Facade.Focus = true;
			}
		}

		void workerRefreshUnlinkedFiles_DoWork(object sender, DoWorkEventArgs e)
		{
			List<VideoLocalVM> unlinkedVideos = JMMServerHelper.GetUnlinkedVideos();

			List<GUIListItem> listItems = new List<GUIListItem>();
			GUIListItem itm = null;

			foreach (VideoLocalVM locFile in unlinkedVideos)
			{
				string fileNameFull = Path.GetFileName(locFile.FullPath);
				string fileName = fileNameFull;
				if (File.Exists(fileNameFull))
				{
					FileInfo fi = new FileInfo(fileNameFull);
					fileName = fi.Name;
				}
				

				itm = new GUIListItem(fileName);
				itm.TVTag = locFile;
				listItems.Add(itm);
			}

			e.Result = listItems;

		}

		public override bool OnMessage(GUIMessage message)
		{
			switch (message.Message)
			{
				case GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS_CHANGED:
					{
						int iControl = message.SenderControlId;
						if (iControl == (int)m_Facade.GetID)
						{
							GUIListItem item = m_Facade.SelectedListItem;

							if (item == null || item.TVTag == null) return true;

							// unlinked files
							if (item.TVTag.GetType() == typeof(VideoLocalVM))
							{
								VideoLocalVM vid = item.TVTag as VideoLocalVM;
								if (vid != null)
								{
									setGUIProperty("Utilities.UnlinkedFile.Folder", Path.GetDirectoryName(vid.FullPath));
									setGUIProperty("Utilities.UnlinkedFile.FileName", Path.GetFileName(vid.FullPath));
									setGUIProperty("Utilities.UnlinkedFile.Size", Utils.FormatFileSize(vid.FileSize));
									setGUIProperty("Utilities.UnlinkedFile.Hash", vid.Hash);
									setGUIProperty("Utilities.UnlinkedFile.FileExists", File.Exists(vid.FullPath) ? "YES" : "NO");
								}
							}

						}
					}

					return true;

				default:
					return base.OnMessage(message);
			}
		}

		protected override void OnShowContextMenu()
		{
			try
			{
				
				GUIListItem currentitem = this.m_Facade.SelectedListItem;
				if (currentitem == null)
					return;

				VideoLocalVM vid = currentitem.TVTag as VideoLocalVM;
				if (vid == null)
					return;

				GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
				dlg.Reset();

				dlg.SetHeading("File options");			
				dlg.Add("Play file");
				//dlg.Add("Rehash file");
				//dlg.Add("Delete file from disk");

				dlg.DoModal(GUIWindowManager.ActiveWindow);
                   
                if (dlg.SelectedId == 1)
                {
                    MainWindow.vidHandler.ResumeOrPlay(vid);
                    return;
                }

                if (dlg.SelectedId == 2)
                {
                    //TODO
                    return;
                }

				if (dlg.SelectedId == 3)
				{
					if (!Utils.DialogConfirm("Are you sure you want to delete this file?")) return;

					//TODO
					RefreshUnlinkedFiles();
				}

            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write("Error in menu: {0}", ex);
            }
        }


        public static void setGUIProperty(string which, string value)
        {
            MediaPortal.GUI.Library.GUIPropertyManager.SetProperty("#Anime3." + which, value);
        }

        public static void clearGUIProperty(string which)
        {
            setGUIProperty(which, "-");
        }

		#endregion

		public override bool Init()
		{
            BaseConfig.MyAnimeLog.Write("INIT UTILITIES WINDOW");
			return Load(GUIGraphicsContext.Skin + @"\Anime3_Admin.xml");
		}

	}
}
