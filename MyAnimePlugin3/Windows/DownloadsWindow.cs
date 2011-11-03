using System;
using System.Collections.Generic;

using MediaPortal.GUI.Library;

using MyAnimePlugin3.Downloads;
using System.Threading;
using MediaPortal.Dialogs;

using System.ComponentModel;
using System.IO;
using Action = MediaPortal.GUI.Library.Action;

namespace MyAnimePlugin3.Windows
{
	public class DownloadsWindow : GUIWindow
	{
		[SkinControlAttribute(50)] protected GUIFacadeControl m_Facade = null;

		[SkinControlAttribute(1600)] protected GUILabelControl dummyPageTorrents = null;
		[SkinControlAttribute(1601)] protected GUILabelControl dummyPageSearch = null;
		[SkinControlAttribute(1602)] protected GUILabelControl dummyPageBrowse = null;
		[SkinControlAttribute(1603)] protected GUILabelControl dummyPageTorrentFiles = null;

		[SkinControlAttribute(801)] protected GUIButtonControl btnTorrentsUIPage = null;
		[SkinControlAttribute(802)] protected GUIButtonControl btnSearchPage = null;
		[SkinControlAttribute(803)] protected GUIButtonControl btnBrowseTorrentsPage = null;

		[SkinControlAttribute(920)] protected GUIButtonControl btnWindowContinueWatching = null;
		[SkinControlAttribute(921)] protected GUIButtonControl btnWindowUtilities = null;
		[SkinControlAttribute(922)] protected GUIButtonControl btnWindowCalendar = null;
		//[SkinControlAttribute(923)] protected GUIButtonControl btnWindowDownloads = null;
		//[SkinControlAttribute(924)] protected GUIButtonControl btnWindowCollectionStats = null;

		private Torrent curTorrent = null;
		private TorrentSource curBrowseSource = TorrentSource.TokyoToshokan;
		private readonly string UpFolder = "------ BACK TO TORRENTS ------";
        private int SelectedItem = 0;


		public DownloadsWindow()
		{
			// get ID of windowplugin belonging to this setup
			// enter your own unique code
			GetID = Constants.WindowIDs.DOWNLOADS;

			MainWindow.uTorrent.ListRefreshedEvent += new UTorrentHelper.ListRefreshedEventHandler(uTorrent_ListRefreshedEvent);
		}

		void uTorrent_ListRefreshedEvent(ListRefreshedEventArgs ev)
		{
			ShowUTorrentList(ev.Torrents);
		}

		public override int GetID
		{
			get { return Constants.WindowIDs.DOWNLOADS; }
			set { base.GetID = value; }
		}

		protected override void OnPageLoad()
		{
			base.OnPageLoad();

            curBrowseSource = BaseConfig.Settings.DefaultTorrentSource;
			
			clearGUIProperty("Download.Status");

			setGUIProperty("Torrent.Summary", "Starting...");
			setGUIProperty("Search.ResultDescription", "0 Results");
			setGUIProperty("Search.Summary", "-");

			clearGUIProperty("TorrentLink.Name");
			clearGUIProperty("TorrentLink.Size");
			clearGUIProperty("TorrentLink.Seeders");
			clearGUIProperty("TorrentLink.Leechers");
			clearGUIProperty("TorrentLink.Source");
			clearGUIProperty("TorrentLink.SourceLong");

			// torrent details
			clearGUIProperty("Torrent.Name");
			clearGUIProperty("Torrent.Size");
			clearGUIProperty("Torrent.Done");
			clearGUIProperty("Torrent.DownloadSpeed");
			clearGUIProperty("Torrent.UploadSpeed");
			clearGUIProperty("Torrent.Downloaded");
			clearGUIProperty("Torrent.Uploaded");
			clearGUIProperty("Torrent.Ratio");
			clearGUIProperty("Torrent.Seeds");
			clearGUIProperty("Torrent.SInSwarm");
			clearGUIProperty("Torrent.Peers");
			clearGUIProperty("Torrent.PInSwarm");

			if (!MainWindow.uTorrent.Initialised)
				MainWindow.uTorrent.Init();
			
			ShowPageTorrents();

			// this means we are in search mode
			if (MainWindow.currentDownloadSearch != null)
			{
				ShowPageSearch(false);
				PerformTorrentSearchAsync();
				btnSearchPage.Focus = true;
			}
			
		}

		private void ShowPageTorrents()
		{
			setGUIProperty("Downloads.CurrentView", "Torrent Monitor");
            SelectedItem = m_Facade.SelectedListItemIndex;
			dummyPageTorrents.Visible = true;
			dummyPageTorrentFiles.Visible = false;
			dummyPageSearch.Visible = false;
			dummyPageBrowse.Visible = false;
			m_Facade.Clear();
			LoadUTorrentListAsync();
			

		}

		private void ShowPageTorrentFiles()
		{
			List<TorrentFile> torfiles = new List<TorrentFile>();
			if (MainWindow.uTorrent.GetFileList(curTorrent.Hash, ref torfiles))
			{
				ShowPageTorrentFiles(torfiles);
			}
		}

		private void ShowPageTorrentFiles(List<TorrentFile> torFiles)
		{
            SelectedItem = m_Facade.SelectedListItemIndex;
			setGUIProperty("TorrentFile.Summary", curTorrent.Name);

			dummyPageTorrents.Visible = false;
			dummyPageTorrentFiles.Visible = true;
			dummyPageSearch.Visible = false;
			dummyPageBrowse.Visible = false;
			m_Facade.Clear();
			LoadUTorrentFileList(torFiles);
            m_Facade.SelectedListItemIndex = SelectedItem;
		}

		private void ShowPageSearch(bool showPreviousSearch)
		{
			setGUIProperty("Downloads.CurrentView", "Search");

			clearGUIProperty("TorrentLink.Name");
			clearGUIProperty("TorrentLink.Size");
			clearGUIProperty("TorrentLink.Seeders");
			clearGUIProperty("TorrentLink.Leechers");
			clearGUIProperty("TorrentLink.Source");
			clearGUIProperty("TorrentLink.SourceLong");

			dummyPageTorrents.Visible = false;
			dummyPageTorrentFiles.Visible = false;
			dummyPageSearch.Visible = true;
			dummyPageBrowse.Visible = false;
			m_Facade.Clear();

			try
			{
				if (showPreviousSearch)
				{
					BaseConfig.MyAnimeLog.Write("Search History: {0}", MainWindow.downloadSearchHistory.Count.ToString());
					// show last search
					if (MainWindow.downloadSearchHistory.Count > 0)
					{
						ShowSearchResults(MainWindow.downloadSearchHistory[MainWindow.downloadSearchHistory.Count - 1],
							MainWindow.downloadSearchResultsHistory[MainWindow.downloadSearchHistory.Count - 1]);
					}
					else
					{
						string criteria = "";
						if (Utils.DialogText(ref criteria, GetID))
						{
							MainWindow.currentDownloadSearch = new DownloadSearchCriteria(DownloadSearchType.Manual, criteria);
							PerformTorrentSearchAsync();
						}
					}
				}
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write("Error in ShowPageSearch: {0}", ex.ToString());
				return;
			}
			
		}

		private void ShowPageBrowseTorrents()
		{
			clearGUIProperty("TorrentLink.Name");
			clearGUIProperty("TorrentLink.Size");
			clearGUIProperty("TorrentLink.Seeders");
			clearGUIProperty("TorrentLink.Leechers");
			clearGUIProperty("TorrentLink.Source");
			clearGUIProperty("TorrentLink.SourceLong");

			setGUIProperty("Downloads.CurrentView", "Browse");
			dummyPageTorrents.Visible = false;
			dummyPageSearch.Visible = false;
			dummyPageBrowse.Visible = true;
			dummyPageTorrentFiles.Visible = false;
			m_Facade.Clear();

			try
			{
				PerformTorrentBrowseAsync();
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write("Error in ShowPageSearch: {0}", ex.ToString());
				return;
			}

		}

		private void PerformTorrentBrowseAsync()
		{
			Thread th = new Thread(PerformTorrentBrowse);
			th.Start();
		}

		private void ShowBrowseResults(List<TorrentLink> results)
		{
			if (!dummyPageBrowse.Visible) return;

			m_Facade.Clear();

			setGUIProperty("Browse.Source", DownloadHelper.GetTorrentSourceDescription(curBrowseSource));

			foreach (TorrentLink link in results)
			{
				GUIListItem item = null;
				item = new GUIListItem();
				item.Label = string.Format("{1} ({2})", link.Source, link.TorrentName, link.Size);
				item.TVTag = link;
				m_Facade.Add(item);
			}

			setGUIProperty("Browse.ResultDescription", string.Format("{0} Results", results.Count));

			m_Facade.Focus = true;
		}

		private void PerformTorrentBrowse()
		{
			try
			{
				m_Facade.Clear();

				setGUIProperty("Browse.Source", DownloadHelper.GetTorrentSourceDescription(curBrowseSource));
				setGUIProperty("Browse.ResultDescription", "Searching...");

				List<TorrentLink> links = DownloadHelper.BrowseTorrents(curBrowseSource);
				ShowBrowseResults(links);

			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write("Error in PerformTorrentSearch: {0}", ex.ToString());
				return;
			}
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

			if (control == this.btnTorrentsUIPage)
			{
				this.btnTorrentsUIPage.IsFocused = false;
				ShowPageTorrents();
				m_Facade.Focus = true;
			}

			if (control == this.btnSearchPage)
			{
				this.btnSearchPage.Focus = false;
				ShowPageSearch(true);
				m_Facade.Focus = true;
			}

			if (control == this.btnBrowseTorrentsPage)
			{
				this.btnBrowseTorrentsPage.Focus = false;
				ShowPageBrowseTorrents();
				m_Facade.Focus = true;
			}

			if (control == this.m_Facade)
			{
				// show the files if we are looking at a torrent
				GUIListItem item = m_Facade.SelectedListItem;

				//BaseConfig.MyAnimeLog.Write("Type: {0}", item.TVTag.GetType());

				// torrents
				// show the files if we are looking at a torrent
				if (item.TVTag.GetType() == typeof(Torrent))
				{
					Torrent torItem = item.TVTag as Torrent;
					if (torItem != null)
					{
						curTorrent = torItem;
						List<TorrentFile> torfiles = new List<TorrentFile>();
						if (MainWindow.uTorrent.GetFileList(torItem.Hash, ref torfiles))
						{
							ShowPageTorrentFiles(torfiles);
						}
					}
				}

				if (item.TVTag.GetType() == typeof(TorrentFile))
				{
					if (item.Label == UpFolder)
					{

						ShowPageTorrents();
					}
				}
                
				if (item.TVTag.GetType() == typeof(TorrentLink) && dummyPageSearch.Visible)
				{
					TorrentLink torLink = item.TVTag as TorrentLink;
					if (torLink == null)
						return;

					ShowContextMenuSearch(torLink);
				}

				if (item.TVTag.GetType() == typeof(TorrentLink) && dummyPageBrowse.Visible)
				{
					TorrentLink torLink = item.TVTag as TorrentLink;
					if (torLink == null)
						return;

					ShowContextMenuBrowse(torLink);
				}
			}

			base.OnClicked(controlId, control, actionType);
		}

		private void PerformTorrentSearchAsync()
		{
			Thread th = new Thread(PerformTorrentSearch);
			th.Start();
		}

		private void ShowSearchResults(DownloadSearchCriteria dsc, List<TorrentLink> results)
		{
			if (!dummyPageSearch.Visible) return;

			m_Facade.Clear();

			setGUIProperty("Search.Summary", string.Format("{0}", dsc.ToString()));

			foreach (TorrentLink link in results)
			{
				GUIListItem item = null;
				item = new GUIListItem();
				item.Label = string.Format("({0}) {1} ({2})", link.Source, link.TorrentName, link.Size);
				item.TVTag = link;
				m_Facade.Add(item);
			}

			setGUIProperty("Search.ResultDescription", string.Format("{0} Results", results.Count));
		}

		private void PerformTorrentSearch()
		{
			try
			{
				if (MainWindow.currentDownloadSearch == null)
				{
					return;
				}

				setGUIProperty("Search.Summary", string.Format("{0}", MainWindow.currentDownloadSearch.ToString()));
				setGUIProperty("Search.ResultDescription", "Searching...");

				List<TorrentLink> links = DownloadHelper.SearchTorrents(MainWindow.currentDownloadSearch);
				ShowSearchResults(MainWindow.currentDownloadSearch, links);

				// add history record
				MainWindow.downloadSearchHistory.Add(MainWindow.currentDownloadSearch);
				MainWindow.downloadSearchResultsHistory.Add(links);
				MainWindow.currentDownloadSearch = null;
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write("Error in PerformTorrentSearch: {0}", ex.ToString());
				return;
			}
		}

		

		private void LoadUTorrentListAsync()
		{
			Thread th = new Thread(LoadUTorrentList);
			th.Start();
		}

		private void LoadUTorrentList()
		{
			List<Torrent> torrents = new List<Torrent>();
			bool success = MainWindow.uTorrent.GetTorrentList(ref torrents);
			if (success) ShowUTorrentList(torrents);
		}

		private void LoadUTorrentFileList(List<TorrentFile> torFiles)
		{
			// make sure the user has valid utorrent details
			if (GUIWindowManager.ActiveWindow != Constants.WindowIDs.DOWNLOADS)
			{
				//BaseConfig.MyAnimeLog.Write("Not showing torrent files");
				return;
			}

			try
			{
				GUIListItem item = null;
				int i = 1;

				m_Facade.Clear();

				item = new GUIListItem();
				item.Label = UpFolder;
				item.TVTag = new TorrentFile(); // do this, just so we know we are looking at torrent files
				m_Facade.Add(item);

				foreach (TorrentFile tor in torFiles)
				{
					item = new GUIListItem();
					item.Label = string.Format("{0}/{1} - {2}", i, torFiles.Count, tor.FileName);
					item.TVTag = tor;
					m_Facade.Add(item);
					i++;
				}
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write(ex.ToString());
			}
		}

		private void ShowUTorrentList(List<Torrent> torrents)
		{
			// make sure the user has valid utorrent details
			// do not show if the user is on a different window
			// don't refresh if the user is actually looking at the files for a torrent
			if (GUIWindowManager.ActiveWindow != Constants.WindowIDs.DOWNLOADS)
			{
				//BaseConfig.MyAnimeLog.Write("Not showing torrents");
				return;
			}

			try
			{
				GUIListItem item = null;
				GUIListItem foundItem = null;

				// if the user is not actually looking at this window we will not refresh the list
				// however we may still want to send a notification

				if (dummyPageTorrents.IsVisible)
				{
					// check for any torrents that have been removed
					// if they have it is easier just to clear the list
					bool missingTorrents = false;
					for (int itemIndex = 0; itemIndex < GUIControl.GetItemCount(this.GetID, this.m_Facade.GetID); itemIndex++)
					{
						bool foundTorrent = false;
						item = GUIControl.GetListItem(this.GetID, this.m_Facade.GetID, itemIndex);
						Torrent torItem = item.TVTag as Torrent;

						if (torItem != null)
						{
							foreach (Torrent tor in torrents)
							{
								if (tor.Hash == torItem.Hash)
								{
									foundTorrent = true;
									break;
								}
							}
						}

						if (!foundTorrent)
						{
							missingTorrents = true;
							break;
						}
					}

					if (missingTorrents) m_Facade.Clear();


					long totalSpeed = 0;
					int activeTorrents = 0;
					foreach (Torrent tor in torrents)
					{
						foundItem = null;
						if (tor.IsDownloading)
						{
							activeTorrents++;
							totalSpeed += tor.DownloadSpeed;
						}

						for (int itemIndex = 0; itemIndex < GUIControl.GetItemCount(this.GetID, this.m_Facade.GetID); itemIndex++)
						{
							item = GUIControl.GetListItem(this.GetID, this.m_Facade.GetID, itemIndex);
							Torrent torItem = item.TVTag as Torrent;

							if (curTorrent != null)
							{
								if (curTorrent.Hash == torItem.Hash) curTorrent = torItem;
							}

							if (tor.Hash == torItem.Hash)
								foundItem = item;
						}

						SetTorrentListItem(ref foundItem, tor);
					}

					// refresh the current torrent details if focused
					if (curTorrent != null)
					{
						DisplayTorrentDetails(curTorrent);
					}

					


					setGUIProperty("Torrent.Summary", string.Format("{0} Active Torrents at {1}/sec", activeTorrents, Utils.FormatByteSize((long)totalSpeed)));
				}

				

			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write(ex.ToString());
			}
		}

		private void SetTorrentListItem(ref GUIListItem item, Torrent tor)
		{
			//BaseConfig.MyAnimeLog.Write(tor.ToString());
			if (item == null)
			{
				item = new GUIListItem();
				item.Label = tor.ListDisplay;
				item.TVTag = tor;
				m_Facade.Add(item);
			}
			else
			{
				item.Label = tor.ListDisplay;
				item.TVTag = tor;
			}
		}

		public override bool Init()
		{
			return Load(GUIGraphicsContext.Skin + @"\Anime3_Downloads.xml");
		}

		public static void setGUIProperty(string which, string value)
		{
			MediaPortal.GUI.Library.GUIPropertyManager.SetProperty("#Anime3." + which, value);
		}

		public static void clearGUIProperty(string which)
		{
			setGUIProperty(which, "-"); 
		}

		public override void DeInit()
		{
			base.DeInit();
		}

		private void DisplayTorrentDetails(Torrent tor)
		{
			clearGUIProperty("Torrent.Name");
			clearGUIProperty("Torrent.Size");
			clearGUIProperty("Torrent.Done");
			clearGUIProperty("Torrent.DownloadSpeed");
			clearGUIProperty("Torrent.UploadSpeed");
			clearGUIProperty("Torrent.Downloaded");
			clearGUIProperty("Torrent.Uploaded");
			clearGUIProperty("Torrent.Ratio");
			clearGUIProperty("Torrent.Seeds");
			clearGUIProperty("Torrent.SInSwarm");
			clearGUIProperty("Torrent.Peers");
			clearGUIProperty("Torrent.PInSwarm");

			setGUIProperty("Torrent.Name", tor.Name);
			setGUIProperty("Torrent.Size", tor.SizeFormatted);
			setGUIProperty("Torrent.Done", tor.PercentProgressFormatted);
			setGUIProperty("Torrent.DownloadSpeed", tor.DownloadSpeedFormatted);
			setGUIProperty("Torrent.UploadSpeed", tor.UploadSpeedFormatted);
			setGUIProperty("Torrent.Downloaded", tor.DownloadedFormatted);
			setGUIProperty("Torrent.Uploaded", tor.UploadedFormatted);
			setGUIProperty("Torrent.Ratio", tor.RatioFormatted);
			setGUIProperty("Torrent.Seeds", tor.SeedsConnected.ToString());
			setGUIProperty("Torrent.SInSwarm", tor.SeedsInSwarm.ToString());
			setGUIProperty("Torrent.Peers", tor.PeersConnected.ToString());
			setGUIProperty("Torrent.PInSwarm", tor.PeersInSwarm.ToString());
		}

		private void DisplayTorrentFileDetails(TorrentFile tor)
		{
			clearGUIProperty("TorrentFile.Name");
			clearGUIProperty("TorrentFile.Size");
			clearGUIProperty("TorrentFile.Downloaded");
			clearGUIProperty("TorrentFile.Priority");

			setGUIProperty("TorrentFile.Name", tor.FileName);
			setGUIProperty("TorrentFile.Size", tor.FileSizeFormatted);
			setGUIProperty("TorrentFile.Downloaded", tor.DownloadedFormatted);

			string pri = "";
			switch ((TorrentFilePriority)tor.Priority)
			{
				case TorrentFilePriority.DontDownload: pri = "Don't Download"; break;
				case TorrentFilePriority.High: pri = "High"; break;
				case TorrentFilePriority.Low: pri = "Low"; break;
				case TorrentFilePriority.Medium: pri = "Medium"; break;
			}
			setGUIProperty("TorrentFile.Priority", pri);
		}

		private void DisplayTorrentLinkDetails(TorrentLink tor)
		{
			clearGUIProperty("TorrentLink.Name");
			clearGUIProperty("TorrentLink.Size");
			clearGUIProperty("TorrentLink.Seeders");
			clearGUIProperty("TorrentLink.Leechers");
			clearGUIProperty("TorrentLink.Source");
			clearGUIProperty("TorrentLink.SourceLong");

			setGUIProperty("TorrentLink.Name", tor.TorrentName);
			setGUIProperty("TorrentLink.Size", tor.Size);
			setGUIProperty("TorrentLink.Seeders", tor.Seeders);
			setGUIProperty("TorrentLink.Leechers", tor.Leechers);
			setGUIProperty("TorrentLink.Source", tor.Source);
			setGUIProperty("TorrentLink.SourceLong", tor.SourceLong);
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

							//BaseConfig.MyAnimeLog.Write("Type: {0}", item.TVTag.GetType());

							// torrents
							if (item.TVTag.GetType() == typeof(Torrent))
							{
								Torrent torItem = item.TVTag as Torrent;
								if (torItem != null)
								{
									curTorrent = torItem;
									DisplayTorrentDetails(torItem);
								}
							}

							// torrent file
							if (item.TVTag.GetType() == typeof(TorrentFile))
							{
								TorrentFile torFile = item.TVTag as TorrentFile;
								if (torFile != null)
								{
									//curTorrent = torItem;
									DisplayTorrentFileDetails(torFile);
								}
							}

							// search results
							if (item.TVTag.GetType() == typeof(TorrentLink))
							{
								TorrentLink torLink = item.TVTag as TorrentLink;
								if (torLink != null)
								{
									//curTorrent = torItem;
									DisplayTorrentLinkDetails(torLink);
								}
							}
						}
					}

					return true;
				
				default:
					return base.OnMessage(message);
			}
		}

		private void ShowContextMenuSearch(TorrentLink torLink)
		{
			GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg == null)
				return;

			dlg.Reset();
			dlg.SetHeading("Search");

			dlg.Add("Download via uTorrent");
			dlg.Add("Manual Search");
			dlg.Add("Recent Searches >>>");
			dlg.Add("Clear Search History");
			dlg.DoModal(GUIWindowManager.ActiveWindow);

			switch (dlg.SelectedLabel)
			{
				case 0:
					if (torLink != null)
					{
						MainWindow.uTorrent.AddTorrentFromURL(torLink.TorrentDownloadLink);
						LoadUTorrentListAsync();
					}
					break;

				case 1:
					string criteria = "";
					if (Utils.DialogText(ref criteria, GetID))
					{
						MainWindow.currentDownloadSearch = new DownloadSearchCriteria(DownloadSearchType.Manual, criteria);
						PerformTorrentSearchAsync();
					}
					break;

				case 2:
					ShowRecentSearches();
					break;

				case 3:
					MainWindow.downloadSearchHistory.Clear();
					MainWindow.downloadSearchResultsHistory.Clear();

					setGUIProperty("Search.ResultDescription", "-");
					setGUIProperty("Search.Summary", "-");

					if (dummyPageSearch.Visible) m_Facade.Clear();

					break;
			}
		}

		private void ShowContextMenuBrowse(TorrentLink torLink)
		{
			GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg == null)
				return;

			dlg.Reset();
			dlg.SetHeading("Browse");

			dlg.Add("Download via uTorrent");
			dlg.Add("Select Source");
			dlg.DoModal(GUIWindowManager.ActiveWindow);

			switch (dlg.SelectedLabel)
			{
				case 0:
					if (torLink != null)
					{
						MainWindow.uTorrent.AddTorrentFromURL(torLink.TorrentDownloadLink);
						
						LoadUTorrentListAsync();
					}
					break;

				case 1:
					ShowBrowseSources(torLink);
					break;
			}
		}

		private void ShowBrowseSources(TorrentLink torLink)
		{

			GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg == null)
				return;

			dlg.Reset();
			dlg.SetHeading("Select Source");

			dlg.Add("<<< Browse");
			dlg.Add("Anime Suki");
			dlg.Add("Baka Updates");
			dlg.Add("Nyaa Torrents");
			dlg.Add("Tokyo Toshokan");
			dlg.DoModal(GUIWindowManager.ActiveWindow);

			switch (dlg.SelectedLabel)
			{
				case 0:
					ShowContextMenuBrowse(torLink);
					return;

				case 1: curBrowseSource = TorrentSource.AnimeSuki; break;
				case 2: curBrowseSource = TorrentSource.BakaUpdates; break;
				case 3: curBrowseSource = TorrentSource.Nyaa; break;
				case 4: curBrowseSource = TorrentSource.TokyoToshokan; break;
			}

			PerformTorrentBrowseAsync();
		}

		private void ShowRecentSearches()
		{
			if (MainWindow.downloadSearchHistory.Count == 0)
			{
				Utils.DialogMsg("Error", "No history found");
				return;
			}
			else
			{
				IDialogbox dlg = (IDialogbox)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
				dlg.Reset();
				dlg.SetHeading("Search History");
				GUIListItem pItem = null;

				for (int i = MainWindow.downloadSearchHistory.Count - 1; i >= 0; i--)
				{
					pItem = new GUIListItem(MainWindow.downloadSearchHistory[i].ToString());
					dlg.Add(pItem);
				}

				dlg.DoModal(GUIWindowManager.ActiveWindow);

				if (dlg.SelectedLabel >= 0)
				{
					int idx = MainWindow.downloadSearchHistory.Count - dlg.SelectedLabel - 1;
					ShowSearchResults(MainWindow.downloadSearchHistory[idx], MainWindow.downloadSearchResultsHistory[idx]);
				}
				
			}
		}

		private void ShowContextMenuTorrents(Torrent tor)
		{
			GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg == null)
				return;

			dlg.Reset();
			dlg.SetHeading(tor.Name);

			dlg.Add("Stop Torrent");
			dlg.Add("Start Torrent");
			dlg.Add("Pause Torrent");
			dlg.Add("Remove Torrent");
			dlg.Add("Remove Torrent And Data");
			dlg.DoModal(GUIWindowManager.ActiveWindow);

			switch (dlg.SelectedLabel)
			{
				case 0:
					MainWindow.uTorrent.StopTorrent(tor.Hash);
					LoadUTorrentListAsync();
					break;

				case 1:
					MainWindow.uTorrent.StartTorrent(tor.Hash);
					LoadUTorrentListAsync();
					break;

				case 2:
					MainWindow.uTorrent.PauseTorrent(tor.Hash);
					LoadUTorrentListAsync();
					break;

				case 3:
					MainWindow.uTorrent.RemoveTorrent(tor.Hash);
					LoadUTorrentListAsync();
					break;

				case 4:
					MainWindow.uTorrent.RemoveTorrentAndData(tor.Hash);
					LoadUTorrentListAsync();
					break;

			}
		}

		private void ShowContextMenuTorrentFiles(TorrentFile tor, int idx)
		{
			GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg == null)
				return;

			dlg.Reset();
			dlg.SetHeading(tor.FileName);

			dlg.Add("Priority - High");
			dlg.Add("Priority - Medium");
			dlg.Add("Priority - Low");
			dlg.Add("Priority - Don't Download");
			dlg.DoModal(GUIWindowManager.ActiveWindow);

			switch (dlg.SelectedLabel)
			{
				case 0:
					MainWindow.uTorrent.FileSetPriority(curTorrent.Hash, idx, TorrentFilePriority.High);
					ShowPageTorrentFiles();
					break;

				case 1:
					MainWindow.uTorrent.FileSetPriority(curTorrent.Hash, idx, TorrentFilePriority.Medium);
					ShowPageTorrentFiles();
					break;

				case 2:
					MainWindow.uTorrent.FileSetPriority(curTorrent.Hash, idx, TorrentFilePriority.Low);
					ShowPageTorrentFiles();
					break;

				case 3:
					MainWindow.uTorrent.FileSetPriority(curTorrent.Hash, idx, TorrentFilePriority.DontDownload);
					ShowPageTorrentFiles();
					break;


			}
		}

		protected override void OnShowContextMenu()
		{
			GUIListItem currentitem = this.m_Facade.SelectedListItem;
			
			if (currentitem == null)
			{
				if (dummyPageSearch.Visible)
				{
					ShowContextMenuSearch(null);
				}
				if (dummyPageBrowse.Visible)
				{
					ShowContextMenuBrowse(null);
				}

				return;
			}

			if (currentitem.TVTag == null) return;

			if (currentitem.TVTag.GetType() == typeof(Torrent))
			{
				Torrent tor = currentitem.TVTag as Torrent;
				if (tor == null)
					return;

				ShowContextMenuTorrents(tor);
			}

			if (currentitem.TVTag.GetType() == typeof(TorrentFile))
			{
				TorrentFile tor = currentitem.TVTag as TorrentFile;
				if (tor == null)
					return;

				int idx = -1;
				for (int itemIndex = 0; itemIndex < GUIControl.GetItemCount(this.GetID, this.m_Facade.GetID); itemIndex++)
				{
					GUIListItem item = GUIControl.GetListItem(this.GetID, this.m_Facade.GetID, itemIndex);
					TorrentFile torTemp = item.TVTag as TorrentFile;

					if (torTemp != null)
					{
						if (torTemp.FileName == tor.FileName)
						{
							idx = itemIndex;
							break;
						}
					}
				}

				if (idx >= 0)
				{
					idx = idx - 1;
					ShowContextMenuTorrentFiles(tor, idx);
				}
			}

			if (currentitem.TVTag.GetType() == typeof(TorrentLink) && dummyPageSearch.Visible)
			{
				TorrentLink torLink = currentitem.TVTag as TorrentLink;
				if (torLink == null)
					return;

				ShowContextMenuSearch(torLink);
			}

			if (currentitem.TVTag.GetType() == typeof(TorrentLink) && dummyPageBrowse.Visible)
			{
				TorrentLink torLink = currentitem.TVTag as TorrentLink;
				if (torLink == null)
					return;

				ShowContextMenuBrowse(torLink);
			}
			
		}

	}

	enum TorrentViewMode
	{
		Torrents,
		TorrentFiles
	}

	class FileCopyProgress
	{
		public FileCopyProgress(string _name, long _curFileCount, long _totalFileCount)
		{
			name = _name;
			curFileCount = _curFileCount;
			totalFileCount = _totalFileCount;
		}
		public string name;
		public long curFileCount;
		public long totalFileCount;
	}

	
}
