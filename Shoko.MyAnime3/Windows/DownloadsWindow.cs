using System;
using System.Collections.Generic;

using MediaPortal.GUI.Library;
using System.Threading;
using MediaPortal.Dialogs;

using System.ComponentModel;
using System.IO;
using Action = MediaPortal.GUI.Library.Action;
using BinaryNorthwest;
using Shoko.Commons.Downloads;
using Shoko.Commons.Languages;
using Shoko.Models.Enums;
using Shoko.MyAnime3.JMMServerBinary;
using Shoko.MyAnime3.Downloads;
using Shoko.MyAnime3.ViewModel;

namespace Shoko.MyAnime3.Windows
{
	public class DownloadsWindow : GUIWindow
	{
		[SkinControl(50)] protected GUIFacadeControl m_Facade = null;

		[SkinControl(1600)] protected GUILabelControl dummyPageTorrents = null;
		[SkinControl(1601)] protected GUILabelControl dummyPageSearch = null;
		[SkinControl(1602)] protected GUILabelControl dummyPageBrowse = null;
		[SkinControl(1603)] protected GUILabelControl dummyPageTorrentFiles = null;
		[SkinControl(1604)] protected GUILabelControl dummyEpisodeSearch = null;

		[SkinControl(801)] protected GUIButtonControl btnTorrentsUIPage = null;
		[SkinControl(802)] protected GUIButtonControl btnSearchPage = null;
		[SkinControl(803)] protected GUIButtonControl btnBrowseTorrentsPage = null;

		[SkinControl(920)] protected GUIButtonControl btnWindowContinueWatching = null;
		[SkinControl(921)] protected GUIButtonControl btnWindowUtilities = null;
		[SkinControl(922)] protected GUIButtonControl btnWindowCalendar = null;
		//[SkinControlAttribute(923)] protected GUIButtonControl btnWindowDownloads = null;
		//[SkinControlAttribute(924)] protected GUIButtonControl btnWindowCollectionStats = null;
		[SkinControl(925)] protected GUIButtonControl btnWindowRecommendations = null;

        public enum GuiProperty
        {
            Download_Status,
            Downloads_CurrentView,
            Search_ResultDescription,
            Search_Summary,
            TorrentLink_Name,
            TorrentLink_Size,
            TorrentLink_Seeders,
            TorrentLink_Leechers,
            TorrentLink_Source,
            TorrentLink_SourceLong,
            TorrentFile_Summary,
            TorrentFile_Name,
            TorrentFile_Size,
            TorrentFile_Downloaded,
            TorrentFile_Priority,
            Torrent_Summary,
            Torrent_Name,
            Torrent_Size,
            Torrent_Done,
            Torrent_DownloadSpeed,
            Torrent_UploadSpeed,
            Torrent_Downloaded,
            Torrent_Uploaded,
            Torrent_Ratio,
            Torrent_Seeds,
            Torrent_SInSwarm,
            Torrent_Peers,
            Torrent_PInSwarm,
            SubGroup_AnimeName,
            SubGroup_EpisodeName,
            SubGroup_FileDetails,
            Browse_Source,
            Browse_ResultDescription,
        }

        public void SetGUIProperty(GuiProperty which, string value) { this.SetGUIProperty(which.ToString(), value); }
        public void ClearGUIProperty(GuiProperty which) { this.ClearGUIProperty(which.ToString()); }



        private Torrent curTorrent = null;
		private TorrentSourceType curBrowseSource = TorrentSourceType.TokyoToshokanAnime;
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


            ClearGUIProperty(GuiProperty.Download_Status);

            SetGUIProperty(GuiProperty.Torrent_Summary, Translation.Starting + "...");
            SetGUIProperty(GuiProperty.Search_ResultDescription, Translation.NoResults);
            ClearGUIProperty(GuiProperty.Search_Summary);



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
            SetGUIProperty(GuiProperty.Downloads_CurrentView, Translation.TorrentMonitor);

            SelectedItem = m_Facade.SelectedListItemIndex;
			dummyPageTorrents.Visible = true;
			dummyPageTorrentFiles.Visible = false;
			dummyPageSearch.Visible = false;
			dummyPageBrowse.Visible = false;
			dummyEpisodeSearch.Visible = false;
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
            SetGUIProperty(GuiProperty.TorrentFile_Summary, curTorrent.Name);


			dummyPageTorrents.Visible = false;
			dummyPageTorrentFiles.Visible = true;
			dummyPageSearch.Visible = false;
			dummyPageBrowse.Visible = false;
			dummyEpisodeSearch.Visible = false;
			m_Facade.Clear();
			LoadUTorrentFileList(torFiles);
            m_Facade.SelectedListItemIndex = SelectedItem;
		}

		private void ShowEpisodeDetails(AnimeEpisodeVM ep)
		{
			dummyEpisodeSearch.Visible = false;
			AnimeSeriesVM series = JMMServerHelper.GetSeries(ep.AnimeSeriesID);
			if (series != null && series.AniDB_Anime != null)
			{

                SetGUIProperty(GuiProperty.SubGroup_AnimeName, series.AniDB_Anime.FormattedTitle);
                SetGUIProperty(GuiProperty.SubGroup_EpisodeName, ep.EpisodeNumberAndName);

                List<GroupVideoQualityVM> videoQualityRecords = new List<GroupVideoQualityVM>();
				List<JMMServerBinary.Contract_GroupVideoQuality> summ = new List<Contract_GroupVideoQuality>(JMMServerVM.Instance.clientBinaryHTTP.GetGroupVideoQualitySummary(series.AniDB_Anime.AnimeID));
				foreach (JMMServerBinary.Contract_GroupVideoQuality contract in summ)
				{
					GroupVideoQualityVM vidQual = new GroupVideoQualityVM(contract);
					videoQualityRecords.Add(vidQual);
				}

				// apply sorting
				if (videoQualityRecords.Count > 0)
				{
					List<SortPropOrFieldAndDirection> sortlist = new List<SortPropOrFieldAndDirection>();
					sortlist.Add(new SortPropOrFieldAndDirection("FileCountNormal", true, SortType.eInteger));
					videoQualityRecords = Sorting.MultiSort<GroupVideoQualityVM>(videoQualityRecords, sortlist);

					string fileDetails = "";
					foreach (GroupVideoQualityVM gvq in videoQualityRecords)
						fileDetails += string.Format("{0}({1}/{2}/{3}bit) - {4} Files ({5})", gvq.GroupNameShort, gvq.Resolution, gvq.VideoSource, gvq.VideoBitDepth, gvq.FileCountNormal, gvq.NormalEpisodeNumberSummary)
							+ Environment.NewLine;


                    SetGUIProperty(GuiProperty.SubGroup_FileDetails, fileDetails);
                }
                else
                {
                    ClearGUIProperty(GuiProperty.SubGroup_FileDetails);
                }

                dummyEpisodeSearch.Visible = true;
			}
		}

		private void ShowPageSearch(bool showPreviousSearch)
		{
            SetGUIProperty(GuiProperty.Downloads_CurrentView, Translation.Search);

            ClearGUIProperty(GuiProperty.TorrentLink_Name);
            ClearGUIProperty(GuiProperty.TorrentLink_Size);
            ClearGUIProperty(GuiProperty.TorrentLink_Seeders);
            ClearGUIProperty(GuiProperty.TorrentLink_Leechers);
            ClearGUIProperty(GuiProperty.TorrentLink_Source);
            ClearGUIProperty(GuiProperty.TorrentLink_SourceLong);

            dummyPageTorrents.Visible = false;
			dummyPageTorrentFiles.Visible = false;
			dummyPageSearch.Visible = true;
			dummyPageBrowse.Visible = false;
			dummyEpisodeSearch.Visible = false;
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
            SetGUIProperty(GuiProperty.Downloads_CurrentView, Translation.Browse);

            ClearGUIProperty(GuiProperty.TorrentLink_Name);
            ClearGUIProperty(GuiProperty.TorrentLink_Size);
            ClearGUIProperty(GuiProperty.TorrentLink_Seeders);
            ClearGUIProperty(GuiProperty.TorrentLink_Leechers);
            ClearGUIProperty(GuiProperty.TorrentLink_Source);
            ClearGUIProperty(GuiProperty.TorrentLink_SourceLong);

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

            SetGUIProperty(GuiProperty.Browse_Source, EnumTranslator.TorrentSourceTranslated(curBrowseSource)));



			foreach (TorrentLink link in results)
			{
                GUIListItem item = new GUIListItem();
                item.Label = string.Format("{0} ({1})", link.TorrentName, link.Size);
                item.TVTag = link;
				m_Facade.Add(item);
			}

            SetGUIProperty(GuiProperty.Browse_ResultDescription, string.Format("{0} {1}", results.Count, results.Count == 1 ? Translation.Result : Translation.Results));


			m_Facade.Focus = true;
		}

		private void PerformTorrentBrowse()
		{
			try
			{
				m_Facade.Clear();

                SetGUIProperty(GuiProperty.Browse_Source, EnumTranslator.TorrentSourceTranslated(curBrowseSource));
                SetGUIProperty(GuiProperty.Browse_ResultDescription, Translation.Searching + "...");
                List<TorrentLink> links = DownloadHelper.BrowseTorrents(curBrowseSource);
				ShowBrowseResults(links);

			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write("Error in PerformTorrentSearch: {0}", ex.ToString());
				return;
			}
		}

        protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
        {
            MainMenu menu = new MainMenu();
            menu.Add(btnTorrentsUIPage, () =>
            {
                ShowPageTorrents();
                m_Facade.Focus = true;
            });
            menu.Add(btnSearchPage, () =>
            {
                ShowPageSearch(true);
                m_Facade.Focus = true;
            });
            menu.Add(btnBrowseTorrentsPage, () =>
            {
                ShowPageBrowseTorrents();
                m_Facade.Focus = true;
            });
            if (menu.Check(control))
                return;

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
					if (item.Label == Translation.BackToTorrents)
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

			if (dsc.SearchType == DownloadSearchType.Episode)
			{
				AnimeEpisodeVM ep = dsc.SearchParameter as AnimeEpisodeVM;
				ShowEpisodeDetails(ep);
			}
			else
				dummyEpisodeSearch.Visible = false;

            SetGUIProperty(GuiProperty.Search_Summary, string.Format("{0}", dsc));

			foreach (TorrentLink link in results)
			{
				GUIListItem item = null;
				item = new GUIListItem();

				string tname = link.TorrentName;
				if (tname.Length > 50) tname = tname.Substring(0, 50) + "...";

				item.Label = string.Format("({0}) {1} ({2})", link.Source, tname, link.Size);
				item.TVTag = link;
				m_Facade.Add(item);

				BaseConfig.MyAnimeLog.Write("TORRENT: " + item.Label);
			}
            SetGUIProperty(GuiProperty.Search_ResultDescription, string.Format("{0} {1}", results.Count, results.Count == 1 ? Translation.Result : Translation.Results));
		}

		private void PerformTorrentSearch()
		{
			try
			{
				if (MainWindow.currentDownloadSearch == null)
				{
					return;
				}


                SetGUIProperty(GuiProperty.Search_Summary, string.Format("{0}", MainWindow.currentDownloadSearch));
                SetGUIProperty(GuiProperty.Search_ResultDescription, Translation.Searching + "...");


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
				item.Label = Translation.BackToTorrents;
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
						    if (torItem != null)
						    {
						        if (curTorrent != null)
						        {
						            if (curTorrent.Hash == torItem.Hash) curTorrent = torItem;
						        }

						        if (tor.Hash == torItem.Hash)
						            foundItem = item;
						    }
						}

						SetTorrentListItem(ref foundItem, tor);
					}

					// refresh the current torrent details if focused
					if (curTorrent != null)
					{
						DisplayTorrentDetails(curTorrent);
					}



                    SetGUIProperty(GuiProperty.Torrent_Summary, string.Format(Translation.ActiveTorrentsAt, activeTorrents, Utils.FormatByteSize(totalSpeed)));
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
            return this.InitSkin<GuiProperty>("Anime3_Downloads.xml");
        }



        private void DisplayTorrentDetails(Torrent tor)
        {
            SetGUIProperty(GuiProperty.Torrent_Name, tor.Name);
            SetGUIProperty(GuiProperty.Torrent_Size, tor.SizeFormatted);
            SetGUIProperty(GuiProperty.Torrent_Done, tor.PercentProgressFormatted);
            SetGUIProperty(GuiProperty.Torrent_DownloadSpeed, tor.DownloadSpeedFormatted);
            SetGUIProperty(GuiProperty.Torrent_UploadSpeed, tor.UploadSpeedFormatted);
            SetGUIProperty(GuiProperty.Torrent_Downloaded, tor.DownloadedFormatted);
            SetGUIProperty(GuiProperty.Torrent_Uploaded, tor.UploadedFormatted);
            SetGUIProperty(GuiProperty.Torrent_Ratio, tor.RatioFormatted);
            SetGUIProperty(GuiProperty.Torrent_Seeds, tor.SeedsConnected.ToString(Globals.Culture));
            SetGUIProperty(GuiProperty.Torrent_SInSwarm, tor.SeedsInSwarm.ToString(Globals.Culture));
            SetGUIProperty(GuiProperty.Torrent_Peers, tor.PeersConnected.ToString(Globals.Culture));
            SetGUIProperty(GuiProperty.Torrent_PInSwarm, tor.PeersInSwarm.ToString(Globals.Culture));
        }

        private void DisplayTorrentFileDetails(TorrentFile tor)
        {
            SetGUIProperty(GuiProperty.TorrentFile_Name, tor.FileName);
            SetGUIProperty(GuiProperty.TorrentFile_Size, tor.FileSizeFormatted);
            SetGUIProperty(GuiProperty.TorrentFile_Downloaded, tor.DownloadedFormatted);

            string pri = "";
            switch ((TorrentFilePriority)tor.Priority)
            {
                case TorrentFilePriority.DontDownload: pri = Translation.DontDownload; break;
                case TorrentFilePriority.High: pri = Translation.High; break;
                case TorrentFilePriority.Low: pri = Translation.Low; break;
                case TorrentFilePriority.Medium: pri = Translation.Medium; break;
            }
            SetGUIProperty(GuiProperty.TorrentFile_Priority, pri);
        }

        private void DisplayTorrentLinkDetails(TorrentLink tor)
        {
            SetGUIProperty(GuiProperty.TorrentLink_Name, tor.TorrentName);
            SetGUIProperty(GuiProperty.TorrentLink_Size, tor.Size);
            SetGUIProperty(GuiProperty.TorrentLink_Seeders, tor.Seeders);
            SetGUIProperty(GuiProperty.TorrentLink_Leechers, tor.Leechers);
            SetGUIProperty(GuiProperty.TorrentLink_Source, tor.Source);
            SetGUIProperty(GuiProperty.TorrentLink_SourceLong, tor.SourceLong);
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
            ContextMenu cmenu = new ContextMenu(Translation.Search);
            cmenu.AddAction(Translation.DownloadViaTorrent, () =>
            {
                if (torLink != null)
                {
                    MainWindow.uTorrent.AddTorrentFromURL(torLink.TorrentDownloadLink);
                    LoadUTorrentListAsync();
                }
            });
            cmenu.AddAction(Translation.ManualSearch, () =>
            {
                string criteria = "";
                if (Utils.DialogText(ref criteria, GetID))
                {
                    MainWindow.currentDownloadSearch = new DownloadSearchCriteria(DownloadSearchType.Manual, criteria);
                    PerformTorrentSearchAsync();
                }
            });
            cmenu.Add(Translation.RecentSearches + " >>>", ShowRecentSearches);
            cmenu.AddAction(Translation.ClearSearchHistory, () =>
            {
                MainWindow.downloadSearchHistory.Clear();
                MainWindow.downloadSearchResultsHistory.Clear();
                ClearGUIProperty(GuiProperty.Search_ResultDescription);
                ClearGUIProperty(GuiProperty.Search_Summary);
                if (dummyPageSearch.Visible) m_Facade.Clear();
            });
            cmenu.Show();
        }

        private ContextMenuAction ShowContextMenuBrowse(TorrentLink torLink)
        {
            ContextMenu cmenu = new ContextMenu(Translation.Browse);
            cmenu.AddAction(Translation.DownloadViaTorrent, () =>
            {
                MainWindow.uTorrent.AddTorrentFromURL(torLink.TorrentDownloadLink);
                LoadUTorrentListAsync();
            });
            cmenu.Add(Translation.SelectSource, () => ShowBrowseSources(torLink));
            return cmenu.Show();
        }

        private ContextMenuAction ShowBrowseSources(TorrentLink torLink)
        {
            ContextMenu cmenu = new ContextMenu(Translation.SelectSource);
            cmenu.Add("<<< " + Translation.Browse, () => ShowContextMenuBrowse(torLink));
            foreach (TorrentSource src in Enum.GetValues(typeof(TorrentSource)))
            {
                TorrentSource local = src;
                cmenu.AddAction(DownloadHelper.GetTorrentSourceDescription(src), () =>
                {
                    curBrowseSource = local;
                    PerformTorrentBrowseAsync();
                });
            }
            return cmenu.Show();
        }

        private ContextMenuAction ShowRecentSearches()
        {
            if (MainWindow.downloadSearchHistory.Count == 0)
            {
                Utils.DialogMsg(Translation.Error, Translation.NoHistoryFound);
                return ContextMenuAction.Exit;
            }
            ContextMenu cmenu = new ContextMenu(Translation.SearchHistory);

            for (int i = MainWindow.downloadSearchHistory.Count - 1; i >= 0; i--)
            {
                int local = i;
                cmenu.AddAction(MainWindow.downloadSearchHistory[i].ToString(), () => ShowSearchResults(MainWindow.downloadSearchHistory[local], MainWindow.downloadSearchResultsHistory[local]));
            }
            return cmenu.Show();
        }

        private void ShowContextMenuTorrents(Torrent tor)
        {
            ContextMenu cmenu = new ContextMenu(tor.Name);
            cmenu.AddAction(Translation.StopTorrent, () => MainWindow.uTorrent.StopTorrent(tor.Hash));
            cmenu.AddAction(Translation.StartTorrent, () => MainWindow.uTorrent.StartTorrent(tor.Hash));
            cmenu.AddAction(Translation.PauseTorrent, () => MainWindow.uTorrent.PauseTorrent(tor.Hash));
            cmenu.AddAction(Translation.RemoveTorrent, () => MainWindow.uTorrent.RemoveTorrent(tor.Hash));
            cmenu.AddAction(Translation.RemoveTorrentAndData, () => MainWindow.uTorrent.RemoveTorrentAndData(tor.Hash));
            if (cmenu.Show() == ContextMenuAction.Exit)
                LoadUTorrentListAsync();
        }

        private void ShowContextMenuTorrentFiles(TorrentFile tor, int idx)
        {
            ContextMenu cmenu = new ContextMenu(tor.FileName);
            cmenu.AddAction(Translation.Priority + " - " + Translation.High, () => MainWindow.uTorrent.FileSetPriority(curTorrent.Hash, idx, TorrentFilePriority.High));
            cmenu.AddAction(Translation.Priority + " - " + Translation.Medium, () => MainWindow.uTorrent.FileSetPriority(curTorrent.Hash, idx, TorrentFilePriority.Medium));
            cmenu.AddAction(Translation.Priority + " - " + Translation.Low, () => MainWindow.uTorrent.FileSetPriority(curTorrent.Hash, idx, TorrentFilePriority.Low));
            cmenu.AddAction(Translation.Priority + " - " + Translation.DontDownload, () => MainWindow.uTorrent.FileSetPriority(curTorrent.Hash, idx, TorrentFilePriority.DontDownload));
            if (cmenu.Show() == ContextMenuAction.Exit)
                ShowPageTorrentFiles();
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
