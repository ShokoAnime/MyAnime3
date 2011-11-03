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
		[SkinControlAttribute(7)] protected GUIButtonControl btnScanDropFolder = null;
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

		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			if (MA3WindowManager.HandleWindowChangeButton(control))
				return;

			if (this.btnServerStatus != null && control == this.btnServerStatus)
			{
				setGUIProperty("Utilities.CurrentView", "Server Status");
				if (dummyServerStatus != null) dummyServerStatus.Visible = true;
				if (dummyListUnlinkedFiles != null) dummyListUnlinkedFiles.Visible = false;

				this.btnServerStatus.IsFocused = false;
				m_Facade.Focus = true;

				//RefreshGroups();
			}

			if (this.btnListUnlinkedFiles != null && control == this.btnListUnlinkedFiles)
			{
				setGUIProperty("Utilities.CurrentView", "Unlinked Files");
				if (dummyServerStatus != null) dummyServerStatus.Visible = false;
				if (dummyListUnlinkedFiles != null) dummyListUnlinkedFiles.Visible = true;

				this.btnListUnlinkedFiles.IsFocused = false;
				m_Facade.Focus = true;

				RefreshUnlinkedFiles();
			}

			if (this.btnRunImport != null && control == this.btnRunImport)
			{
				
				this.btnRunImport.IsFocused = false;
				m_Facade.Focus = true;
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

				dlg.Add("Scan Drop Folder");
				dlg.Add("Remove records without physical file");
				dlg.Add("Re-scan unrecognized Files");
				dlg.Add("Rehash unrecognized Files");
				dlg.Add("Download Votes/Ratings from AniDB");
				
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

			//RefreshGroups();
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
			/*try
			{
				
				bool istvdb = false;
				if (dummyListTvDB != null) istvdb = dummyListTvDB.Visible;

				if (istvdb)
				{


					GUIListItem currentitem = this.lstFacade.SelectedListItem;
					if (currentitem == null)
						return;

					AnimeGroupVM grp = currentitem.TVTag as AnimeGroup;
					if (grp == null)
						return;

					GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
					dlg.Reset();

					dlg.SetHeading("Group options");
					dlg.Add("Search The TvDB");
					dlg.Add("Search The MovieDB");
					dlg.DoModal(GUIWindowManager.ActiveWindow);
					if (dlg.SelectedId == 1)
						SearchTheTvDB(grp);
					else if (dlg.SelectedId == 2)
						SearchTheMovieDB(grp);
				}
				else
				{
					GUIListItem currentitem = this.lstFacade.SelectedListItem;
					if (currentitem == null)
						return;

					FileLocal file = currentitem.TVTag as FileLocal;
					if (file == null)
						return;

					GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
					dlg.Reset();

					dlg.SetHeading("File options");
                    dlg.Add("Associate episode with file");					
					dlg.Add("Play file");
					dlg.Add("Rehash file");
					dlg.Add("Delete file from disk");

					dlg.DoModal(GUIWindowManager.ActiveWindow);
                   


                    if (dlg.SelectedId == 1)
                    {
                        // ask the user which series
                        IDialogbox dlg2 = (IDialogbox)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
                        dlg2.Reset();
                        dlg2.SetHeading("Select Series");
                        GUIListItem pItem2 = null;

                        List<AnimeSeries> seriesList = AnimeSeries.GetAll();
                        if (seriesList.Count == 0)
                        {
                            GUIDialogOK dlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
                            if (null == dlgOK)
                                return;
                            dlgOK.SetHeading("Error");
                            dlgOK.SetLine(1, string.Empty);
                            dlgOK.SetLine(2, "No series found");
                            dlgOK.DoModal(GUIWindowManager.ActiveWindow);
                            return;
                        }
						
						// get a list of animes which closely match by name
						List<AnimeSeries> suggestedSeries = new List<AnimeSeries>();
						if (File.Exists(file.FileNameFull))
						{
							// check if this file is in the Drop Folder
							FileInfo fi = new FileInfo(file.FileNameFull);

							suggestedSeries.AddRange(AnimeSeries.BestEightLevenshteinDistanceMatches(fi.Name));
						}
						


                        // now sort the series by name
                        List<SortPropOrFieldAndDirection> sortCriteria = new List<SortPropOrFieldAndDirection>();
                        sortCriteria.Add(new SortPropOrFieldAndDirection("SortName", false, SortType.eString));

                        seriesList = Sorting.MultiSort<AnimeSeries>(seriesList, sortCriteria);

						List<AnimeSeries> displayedSeries = new List<AnimeSeries>();

                        // display the last selected series at the top
                        if (curAnimeSeries != null)
                        {
                            pItem2 = new GUIListItem(curAnimeSeries.FormattedName);
							displayedSeries.Add(curAnimeSeries);
                            dlg2.Add(pItem2);
                        }

						// display suggestions
						if (suggestedSeries.Count > 0)
						{
							pItem2 = new GUIListItem("------ Suggestions ------");
							displayedSeries.Add(null);
							dlg2.Add(pItem2);

							foreach (AnimeSeries ser in suggestedSeries)
							{
								pItem2 = new GUIListItem(ser.FormattedName);
								displayedSeries.Add(ser);
								dlg2.Add(pItem2);
							}
						}


						// display all series
						pItem2 = new GUIListItem("----- Other Series -----");
						displayedSeries.Add(null);
						dlg2.Add(pItem2);

                        foreach (AnimeSeries ser in seriesList)
                        {
                            pItem2 = new GUIListItem(ser.FormattedName);
							displayedSeries.Add(ser);
                            dlg2.Add(pItem2);
                        }

                        dlg2.DoModal(GUIWindowManager.ActiveWindow);

                        if (dlg2.SelectedId > 0)
                        {
                            AnimeSeries selSeries = null;
							selSeries = displayedSeries[dlg2.SelectedId - 1];
							if (selSeries == null) return;

							BaseConfig.MyAnimeLog.Write("Selected series: {0} - {1}", dlg2.SelectedId, selSeries);

                            curAnimeSeries = selSeries;

                            List<AnimeEpisode> episodeList = curAnimeSeries.Episodes;
                            
                            if (episodeList.Count == 0)
                            {
                                GUIDialogOK dlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
                                if (null == dlgOK)
                                    return;
                                dlgOK.SetHeading("Error");
                                dlgOK.SetLine(1, string.Empty);
                                dlgOK.SetLine(2, "No episodes found");
                                dlgOK.DoModal(GUIWindowManager.ActiveWindow);
                                return;
                            }

                            // ask the user which episode
							bool showingMissing = true;
							bool showMenu = true;
							IDialogbox dlg3 = null;
							List<AnimeEpisode> episodeListFinal = null;

							while (showMenu)
							{
								episodeListFinal = new List<AnimeEpisode>();
								dlg3 = (IDialogbox)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
								dlg3.Reset();
								dlg3.SetHeading(curAnimeSeries.FormattedName);
								GUIListItem pItem3 = null;

								// exclude episodes where we already have a file
								if (showingMissing)
									pItem3 = new GUIListItem("Show All Episodes >>>");	
								else
									pItem3 = new GUIListItem("Show Missing Episodes >>>");
								dlg3.Add(pItem3);

								
								if (showingMissing)
								{
									foreach (AnimeEpisode ep in episodeList)
									{
										if (ep.FileLocals.Count == 0)
										{
											episodeListFinal.Add(ep);
										}
									}
								}
								else
								{
									episodeListFinal = episodeList;
								}

								foreach (AnimeEpisode ep in episodeListFinal)
								{
									pItem3 = new GUIListItem(ep.DisplayNameAniDBPopup);
									dlg3.Add(pItem3);
								}

								dlg3.DoModal(GUIWindowManager.ActiveWindow);
								if (dlg3.SelectedId <= 0) showMenu = false;
								if (dlg3.SelectedId == 1) showingMissing = !showingMissing;
								if (dlg3.SelectedId > 1) showMenu = false;
							}

                            if (dlg3.SelectedId > 1)
                            {
								//BaseConfig.MyAnimeLog.Write("dlg3.SelectedId: {0}", dlg3.SelectedId);

								AnimeEpisode selectedEp = episodeListFinal[dlg3.SelectedId - 2];
                                CrossRef_Episode_FileHash.UpdateEpisode(selectedEp, file);
								if (selectedEp.AniDB_EpisodeID.HasValue)
									CrossRef_Episode_FileHash.AssignFileToEpisode(file.ED2KHash, selectedEp.AniDB_EpisodeID.Value);
                                RefreshUnrecognizedFiles();
                                return;
                            }

                        }
                        return;
                    }

                    if (dlg.SelectedId == 2)
                    {
                        MainWindow.vidHandler.ResumeOrPlay(file);
                        return;
                    }

                    if (dlg.SelectedId == 3)
                    {
                        FileHash fh = file.FileHash;
                        if (fh!=null)
                            fh.Delete();
                        file.Delete();
                        MainWindow.vidHasher.UpdateFileDataIfRequired(file.FileNameFull, false);
                        return;
                    }

					if (dlg.SelectedId == 4)
					{
						if (!Utils.DialogConfirm("Are you sure you want to delete this file?")) return;

						File.Delete(file.FileNameFull);
						file.Delete();

						RefreshUnrecognizedFiles();
					}
                }

            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write("Error in menu: {0}", ex);
            }*/
        }

		/*private void SearchTheTvDB(AnimeGroup grp)
		{
			string searchCriteria = "";
			int aniDBID = -1;

			searchCriteria = grp.GroupName;
			if (grp.AniDB_ID.HasValue)
				aniDBID = grp.AniDB_ID.Value;

			GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			dlg.Reset();

			dlg.SetHeading("Search The TvDB");
			dlg.Add("Search using:   " + searchCriteria);
			dlg.Add("Manual Search");

			TvDBSeries tvSeries = null;
            if (BaseConfig.Settings.UseWebCache && BaseConfig.Settings.CommunityGetCrossRef && aniDBID > 1)
			{

				try
				{
					CrossRef_Series_AniDB_TvDB crossRef = XMLService.Get_CrossRefSeries(aniDBID);
					if (crossRef != null)
					{
						XmlDocument doc = TVDB.GetSeriesInfoOnline(crossRef.TvDB_ID);
						if (doc != null)
						{
							tvSeries = new TvDBSeries(doc);
							dlg.Add("Community Says:   " + tvSeries.SeriesName);
						}
					}
				}
				catch (Exception)
				{
				}
			}

			dlg.DoModal(GUIWindowManager.ActiveWindow);

			if (dlg.SelectedId == 1)
			{
				SearchTheTvDB(grp, searchCriteria);
			}
			if (dlg.SelectedId == 2)
			{
				VirtualKeyboard keyBoard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_VIRTUAL_KEYBOARD);
				if (keyBoard == null) return;
				keyBoard.IsSearchKeyboard = true;
				keyBoard.Reset();
				keyBoard.Text = grp.FormattedName;
				keyBoard.DoModal(GetID); // show it...
				if (keyBoard.IsConfirmed)
				{
					SearchTheTvDB(grp, keyBoard.Text);
				}
			}
			if (dlg.SelectedId == 3)
			{
				UseTvDBSearchResult(grp, int.Parse(tvSeries.Id));
			}

		}

		private void SearchTheTvDB(AnimeGroup grp, string searchCriteria)
		{
			if (searchCriteria.Length == 0) return;

			bool foundMatches = false;

			List<TVDBSeriesSearchResult> results = MainWindow.tvHelper.SearchSeries(searchCriteria);
			BaseConfig.MyAnimeLog.Write("Found {0} tvdb results for {1}", results.Count, searchCriteria);
			if (results.Count == 0)
			{
				if (grp.AniDB_ID.HasValue)
				{
					AniDB_Anime anime = new AniDB_Anime();
					if (anime.Load(grp.AniDB_ID.Value))
					{
						// lets try the the romaji title
						if (searchCriteria.ToUpper() != anime.RomajiName.ToUpper() && anime.RomajiName.Trim().Length > 0)
						{
							results = MainWindow.tvHelper.SearchSeries(anime.RomajiName);
							if (results.Count > 0) foundMatches = true;
							BaseConfig.MyAnimeLog.Write("Found {0} tvdb results for RomajiName search {1}", results.Count, anime.RomajiName);
						}

						// lets try the the english title
						if (!foundMatches && searchCriteria.ToUpper() != anime.EnglishName.ToUpper() && anime.EnglishName.Trim().Length > 0)
						{
							results = MainWindow.tvHelper.SearchSeries(anime.EnglishName);
							if (results.Count > 0) foundMatches = true;
							BaseConfig.MyAnimeLog.Write("Found {0} tvdb results for EnglishName search {1}", results.Count, anime.EnglishName);
						}
					}
				}
				
			}
			else
				foundMatches = true;


			if (foundMatches)
			{
				IDialogbox dlg = (IDialogbox)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
				if (dlg == null) return;

				dlg.Reset();
				dlg.SetHeading("Search Results");
				GUIListItem pItem = null;

				foreach (TVDBSeriesSearchResult res in results)
				{
					pItem = new GUIListItem(res.SeriesName); dlg.Add(pItem);
				}

				dlg.DoModal(GUIWindowManager.ActiveWindow);

				if (dlg.SelectedId > 0)
				{
					TVDBSeriesSearchResult res = results[dlg.SelectedId - 1];
					UseTvDBSearchResult(grp, res.SeriesID);
				}
			}
			else
			{
				GUIDialogOK dlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
				if (null == dlgOK)
					return;
				dlgOK.SetHeading("Search Results");
				dlgOK.SetLine(1, string.Empty);
				dlgOK.SetLine(2, "No Results found");
				dlgOK.DoModal(GUIWindowManager.ActiveWindow);

			}
		}

		private void SearchTheMovieDB(AnimeGroup grp)
		{
			string searchCriteria = "";
			int aniDBID = -1;

			searchCriteria = grp.GroupName;
			if (grp.AniDB_ID.HasValue)
				aniDBID = grp.AniDB_ID.Value;

			GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			dlg.Reset();

			dlg.SetHeading("Search The MovieDB");
			dlg.Add("Search using:   " + searchCriteria);
			dlg.Add("Manual Search");

			dlg.DoModal(GUIWindowManager.ActiveWindow);

			if (dlg.SelectedId == 1)
			{
				SearchTheMovieDB(grp, searchCriteria);
				return;
			}
			if (dlg.SelectedId == 2)
			{
				VirtualKeyboard keyBoard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_VIRTUAL_KEYBOARD);
				if (keyBoard == null) return;
				keyBoard.IsSearchKeyboard = true;
				keyBoard.Reset();
				keyBoard.Text = grp.FormattedName;
				keyBoard.DoModal(GetID); // show it...
				if (keyBoard.IsConfirmed)
				{
					SearchTheMovieDB(grp, keyBoard.Text);
				}
				return;
			}

		}

		private void SearchTheMovieDB(AnimeGroup grp, string searchCriteria)
		{
			if (searchCriteria.Length == 0) return;

			int aniDBID = -1;
			if (grp.AniDB_ID.HasValue)
				aniDBID = grp.AniDB_ID.Value;

			bool foundMatches = false;

			List<MovieDBSearchResult> results = MovieDB.Search(searchCriteria);
			BaseConfig.MyAnimeLog.Write("Found {0} moviedb results for {1}", results.Count, searchCriteria);
			if (results.Count == 0)
			{
				if (aniDBID > 1)
				{
					AniDB_Anime anime = new AniDB_Anime();
					if (anime.Load(aniDBID))
					{
						// lets try the the romaji title
						if (searchCriteria.ToUpper() != anime.RomajiName.ToUpper() && anime.RomajiName.Trim().Length > 0)
						{
							results = MovieDB.Search(anime.RomajiName);
							if (results.Count > 0) foundMatches = true;
							BaseConfig.MyAnimeLog.Write("Found {0} moviedb results for RomajiName search {1}", results.Count, anime.RomajiName);
						}

						// lets try the the english title
						if (!foundMatches && searchCriteria.ToUpper() != anime.EnglishName.ToUpper() && anime.EnglishName.Trim().Length > 0)
						{
							results = MovieDB.Search(anime.EnglishName);
							if (results.Count > 0) foundMatches = true;
							BaseConfig.MyAnimeLog.Write("Found {0} moviedb results for EnglishName search {1}", results.Count, anime.EnglishName);
						}
					}
				}

			}
			else
				foundMatches = true;


			if (foundMatches)
			{
				IDialogbox dlg = (IDialogbox)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
				if (dlg == null) return;

				dlg.Reset();
				dlg.SetHeading("Search Results");
				GUIListItem pItem = null;

				foreach (MovieDBSearchResult res in results)
				{
					pItem = new GUIListItem(res.Title); dlg.Add(pItem);
				}

				dlg.DoModal(GUIWindowManager.ActiveWindow);

				if (dlg.SelectedId > 0)
				{
					MovieDBSearchResult res = results[dlg.SelectedId - 1];

					MovieDB.UseSearchResult(grp, res.Id, res.Title);

					ShowFanartWindow(grp);
				}
			}
			else
			{
				GUIDialogOK dlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
				if (null == dlgOK)
					return;
				dlgOK.SetHeading("Search Results");
				dlgOK.SetLine(1, string.Empty);
				dlgOK.SetLine(2, "No Results found");
				dlgOK.DoModal(GUIWindowManager.ActiveWindow);
			}
		}

		private void UseTvDBSearchResult(AnimeGroup grp, int tvDBID)
		{

			grp.TvDB_ID = tvDBID;
			grp.TvDB_SeasonNumber = 1; // default
			grp.Save();

			// if this group's AniDB ID is the same as any child series
			// and the series doesn't have a TvDB_ID yet, update that as well
		   foreach (AnimeSeries ser in grp.Series)
			{
				if (!ser.TvDB_ID.HasValue && ser.AniDB_ID.HasValue && grp.AniDB_ID.HasValue)
				{
					if (ser.AniDB_ID.Value == grp.AniDB_ID.Value)
					{
						ser.TvDB_ID = tvDBID;
						ser.TvDB_SeasonNumber = 1; // default
						ser.Save();
					}
				}
			}

			// associate this AniDB_ID with the TVDB ID
			CrossRef_Series_AniDB_TvDB crossref = new CrossRef_Series_AniDB_TvDB();
			crossref.AniDB_ID = grp.AniDB_ID.Value;
			crossref.TvDB_ID = tvDBID;
			crossref.Save();

			ShowFanartWindow(grp);
		}*/

        

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
