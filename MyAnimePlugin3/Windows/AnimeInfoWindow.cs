using System;
using System.Collections.Generic;

using MediaPortal.GUI.Library;

using BinaryNorthwest;

using System.IO;

using MediaPortal.Player;
using MediaPortal.Dialogs;
using MyAnimePlugin3.Downloads;
using Action = MediaPortal.GUI.Library.Action;
using MyAnimePlugin3.ViewModel;

namespace MyAnimePlugin3.Windows
{
    public class AnimeInfoWindow : GUIWindow
    {
        [SkinControlAttribute(901)] protected GUIButtonControl btnNavLeft = null;
        [SkinControlAttribute(902)] protected GUIButtonControl btnNavRight = null;

        [SkinControlAttribute(801)] protected GUIButtonControl btnInfoPage = null;
        [SkinControlAttribute(802)] protected GUIButtonControl btnStatsPage = null;
		[SkinControlAttribute(803)] protected GUIButtonControl btnGroupsPage = null;

        [SkinControlAttribute(1500)] protected GUILabelControl dummyPageInfo = null;
        [SkinControlAttribute(1501)] protected GUILabelControl dummyPageStatistics = null;
        [SkinControlAttribute(1502)] protected GUILabelControl dummyPoster = null;
		[SkinControlAttribute(1503)] protected GUILabelControl dummyPageGroups = null;

        [SkinControlAttribute(100)] protected GUIVideoControl videoControl = null;

		//[SkinControlAttribute(910)] protected GUIButtonControl btnAnimeInfo = null;
		[SkinControlAttribute(911)] protected GUIButtonControl btnAnimeCharacters = null;
		[SkinControlAttribute(912)] protected GUIButtonControl btnAnimeRelations = null;
		[SkinControlAttribute(913)] protected GUIButtonControl btnAnimeFanart = null;
		[SkinControlAttribute(914)] protected GUIButtonControl btnAnimePosters = null;
		[SkinControlAttribute(915)] protected GUIButtonControl btnAnimeWideBanners = null;


        AniDB_AnimeVM MainAnime = null;
        AnimeSeriesVM serMain = null;
        String strEpisodeCount = null;
        int iFileCount = 0;
        int iMissingEpisodeCount = 0;
        String strRunningTime = null;
        String strFileSize = null;
        String strAudioLanguages = "";
        String strSubtitleLanguages = "";
      

		List<string> creditList = new List<string>();

        public AnimeInfoWindow()
        {
            GetID = Constants.WindowIDs.ANIMEINFO;
        }

		//TODO
		/*
		void anidbProcessor_GroupStatusCompleteEvent(GroupStatusCompleteEventArgs ev)
		{
			if (GUIWindowManager.ActiveWindow != Constants.WindowIDs.ANIMEINFO) return;

			int aid = ev.animeID;

			// check if this anime the same one we are looking at
			if (aid != MainAnime.AnimeID) return;

			MainAnime = new AniDB_Anime();
			MainAnime.Load(aid);

			LoadGroups();
			
			setGUIProperty("AnimeInfo.ResultStatus", ev.message);
		}

		private void anidbProcessor_VoteCompleteEvent(VoteCompleteEventArgs ev)
		{
			if (GUIWindowManager.ActiveWindow != Constants.WindowIDs.ANIMEINFO) return;

			int aid = ev.animeID;

			// check if this anime the same one we are looking at
			if (aid != MainAnime.AnimeID) return;
			
			MainAnime = new AniDB_Anime();
			MainAnime.Load(aid);

			LoadInfo();
			SetSkinProperties();
			InfoPage();

			HideControls();
			setGUIProperty("AnimeInfo.ResultStatus", ev.message);
		}

		private void anidbProcessor_GotAnimeInfoEvent(GotAnimeInfoEventArgs ev)
		{
			if (GUIWindowManager.ActiveWindow != Constants.WindowIDs.ANIMEINFO) return;

			int aid = ev.animeID;

			// check if this anime the same one we are looking at
			if (aid != MainAnime.AnimeID) return;

			MainAnime = new AniDB_Anime();
			MainAnime.Load(aid);

			LoadInfo();
            FormatTextPerEpisodeData();
			SetSkinProperties();
			InfoPage();
            StatsPage();

			dummyPageInfo.Visible = true;
		}

		private void anidbProcessor_AniDBStatusEvent(AniDBStatusEventArgs ev)
		{
			if (GUIWindowManager.ActiveWindow != Constants.WindowIDs.ANIMEINFO) return;

			try
			{
				BaseConfig.MyAnimeLog.Write("ev.evType: {0}", ev.evType);

				string cmdDesc = "";
				switch (ev.evType)
				{
					case enHelperActivityType.GettingAnimeInfo:
						cmdDesc = "Getting anime info: " + ev.Status; break;
					case enHelperActivityType.GettingAnimeDesc:
						cmdDesc = "Getting description: " + ev.Status; break;
					case enHelperActivityType.AddingVote:
						cmdDesc = "Adding Vote: " + ev.Status; break;
					case enHelperActivityType.GettingGroupStatus:
						cmdDesc = "Getting group status: " + ev.Status; break;
					case enHelperActivityType.LoggingIn:
						cmdDesc = "Logging in..."; break;
					case enHelperActivityType.LoggingOut:
						cmdDesc = "Logging out..."; break;
					default:
						cmdDesc = ""; break;
				}

				setGUIProperty("AnimeInfo.Status", cmdDesc);
			}
			catch { }
		}*/

		public override int GetID
        {
            get { return Constants.WindowIDs.ANIMEINFO; }
            set { base.GetID = value; }
        }

        protected override void OnPageLoad()
        {
            base.OnPageLoad();

            BaseConfig.MyAnimeLog.Write("Anime Info Page loaded");

            //reset varibles
            strAudioLanguages = "";
            strSubtitleLanguages = "";

            HideControls();
            LoadInfo();
            LoadFanart();
			LoadGroups();

            GetLanguageStrings();
            FormatTextPerEpisodeData();
            SetSkinProperties();
            InfoPage();
            StatsPage();

			dummyPageInfo.Visible = true;
        }
        #region SkinInfo
        private void LoadInfo()
        {
			if (MainWindow.GlobalSeriesID > 0)
			{
				serMain = JMMServerHelper.GetSeries(MainWindow.GlobalSeriesID);
				if (serMain != null)
					MainAnime = serMain.AniDB_Anime;
			}

        }

        private void SetSkinProperties()
		{
			setGUIProperty("AnimeInfo.Title", "-");
			setGUIProperty("AnimeInfo.PageTitle", "Information");

			if (serMain != null)
			{
				setGUIProperty("AnimeInfo.Poster", ImageAllocator.GetSeriesImageAsFileName(serMain, GUIFacadeControl.Layout.List));
				setGUIProperty("AnimeInfo.Title", serMain.SeriesName);
			}

		}

        private void InfoPage()
        {
			BaseConfig.MyAnimeLog.Write("Info Page loaded");

			setGUIProperty("AnimeInfo.Status", "-");
			setGUIProperty("AnimeInfo.ResultStatus", "-");
            setGUIProperty("AnimeInfo.Info.AnidbTitle", "-");
            setGUIProperty("AnimeInfo.Info.EpisodeSpecials", "-");
            setGUIProperty("AnimeInfo.Info.Year", "-");
            setGUIProperty("AnimeInfo.Info.Relations", "-");
            setGUIProperty("AnimeInfo.Info.Rating", "-");
			setGUIProperty("AnimeInfo.Info.MyRating", "-");
            setGUIProperty("AnimeInfo.Info.ReviewRating", "-");
            setGUIProperty("AnimeInfo.Info.ShortTitles", "-");
            setGUIProperty("AnimeInfo.Info.Type", "-");
            setGUIProperty("AnimeInfo.Info.OtherTitles", "-");
            setGUIProperty("AnimeInfo.Info.Genre", "-");
            setGUIProperty("AnimeInfo.Info.Awards", "-");
            setGUIProperty("AnimeInfo.Info.CharacterCount", "-");
            setGUIProperty("AnimeInfo.Info.Restricted", "-");
            setGUIProperty("AnimeInfo.Info.Description", "-");

			//TODO
			//if (serMain != null)
			//	setGUIProperty("AnimeInfo.Info.MyRating", MainAnime.us);

            setGUIProperty("AnimeInfo.Info.AnidbTitle", string.Format("{0} (AID {1})", MainAnime.MainTitle, MainAnime.AnimeID));
            setGUIProperty("AnimeInfo.Info.EpisodeSpecials", strEpisodeCount);
            setGUIProperty("AnimeInfo.Info.Year", FormatTextYear());
            setGUIProperty("AnimeInfo.Info.Rating", FormatTextRating());
            setGUIProperty("AnimeInfo.Info.Reviews", FormatTextReviews());
            setGUIProperty("AnimeInfo.Info.Type", MainAnime.AnimeTypeDescription);
            setGUIProperty("AnimeInfo.Info.OtherTitles", FormatTextOtherTitles());
            setGUIProperty("AnimeInfo.Info.Genre", MainAnime.CategoriesFormatted);
			setGUIProperty("AnimeInfo.Info.GenreShort", MainAnime.CategoriesFormattedShort);
            setGUIProperty("AnimeInfo.Info.Awards", FormatTextAwards());

			string eps = MainAnime.EpisodeCountNormal.ToString() + " (" + MainAnime.EpisodeCountSpecial.ToString() + " Specials)";
			setGUIProperty("AnimeInfo.Info.EpisodeSpecials", eps);

            setGUIProperty("AnimeInfo.Info.Restricted", FormatTextRestricted());
     		setGUIProperty("AnimeInfo.Info.Description", MainAnime.ParsedDescription);
			
            dummyPoster.Visible = true;
        }

        void StatsPage()
        {
            setGUIProperty("AnimeInfo.Stats.StoppedCount", "-");
            setGUIProperty("AnimeInfo.Stats.PlayedCount", "-");
            setGUIProperty("AnimeInfo.Stats.WatchedCount", "-");
            setGUIProperty("AnimeInfo.Stats.FileCount", "-");
            setGUIProperty("AnimeInfo.Stats.AnimeFileSize", "-");
            setGUIProperty("AnimeInfo.Stats.TimeSpentWatching", "-");
            setGUIProperty("AnimeInfo.Stats.MissingEpisodes", "-");
            setGUIProperty("AnimeInfo.Stats.SubtitleLanguages", "-");
            setGUIProperty("AnimeInfo.Stats.AudioLanguages", "-");
            setGUIProperty("AnimeInfo.Stats.RunningTime", "-");
            setGUIProperty("AnimeInfo.Stats.BlinkedCount", "-");

			if (serMain != null)
			{
				setGUIProperty("AnimeInfo.Stats.StoppedCount", serMain.StoppedCount.ToString());
				setGUIProperty("AnimeInfo.Stats.PlayedCount", serMain.PlayedCount.ToString());
				setGUIProperty("AnimeInfo.Stats.WatchedCount", serMain.WatchedCount.ToString());
				setGUIProperty("AnimeInfo.Stats.FileCount", iFileCount.ToString());
				setGUIProperty("AnimeInfo.Stats.AnimeFileSize", strFileSize);
				setGUIProperty("AnimeInfo.Stats.TimeSpentWatching", FormatTextTimeSpentWatching());
				setGUIProperty("AnimeInfo.Stats.MissingEpisodes", iMissingEpisodeCount.ToString());

                setGUIProperty("AnimeInfo.Stats.SubtitleLanguages", strSubtitleLanguages);
                setGUIProperty("AnimeInfo.Stats.AudioLanguages", strAudioLanguages);

				setGUIProperty("AnimeInfo.Stats.RunningTime", strRunningTime);

				//TODO
				//setGUIProperty("AnimeInfo.Stats.BlinkedCount", ((serMain.TimeSpentWatching / 60) / 4).ToString());
			}
        }

        #endregion

        private void GetLanguageStrings()
        {
			//TODO
			/*
            //Get Subtitle Languages
            List<CrossRef_Subtitles_AnimeSeries> SubtitleLanguages = DBProxy.GetFromQuery<CrossRef_Subtitles_AnimeSeries>("AnimeSeriesID=" + serMain.AnimeSeriesID.Value.ToString());

            foreach (CrossRef_Subtitles_AnimeSeries CrossRefLan in SubtitleLanguages)
            {
                List<Language> Lan = DBProxy.GetFromQuery<Language>("LanguageID=" + CrossRefLan.LanguageID.ToString());

                if (!string.IsNullOrEmpty(strSubtitleLanguages))
                    strSubtitleLanguages += " ";

                strSubtitleLanguages += Lan[0].LanguageName;
            }

            if(String.IsNullOrEmpty(strSubtitleLanguages))
                strSubtitleLanguages = "None Found";


            //Get Audio Languages
            List<CrossRef_Languages_AnimeSeries> AudioLanguages = DBProxy.GetFromQuery<CrossRef_Languages_AnimeSeries>("AnimeSeriesID=" + serMain.AnimeSeriesID.Value.ToString());

            foreach (CrossRef_Languages_AnimeSeries CrossRefLan in AudioLanguages)
            {
                List<Language> Lan = DBProxy.GetFromQuery<Language>("LanguageID=" + CrossRefLan.LanguageID.ToString());

                if (!string.IsNullOrEmpty(strAudioLanguages))
                    strAudioLanguages += " ";

                strAudioLanguages += Lan[0].LanguageName;
            }

            if (String.IsNullOrEmpty(strAudioLanguages))
                strAudioLanguages = "None Found";
			*/
        }

		protected override void OnShowContextMenu()
		{
			try
			{
				GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
				if (dlg == null)
					return;

				dlg.Reset();
				dlg.SetHeading(MainAnime.MainTitle);
				dlg.Add("Update Series Info From AniDB");
				dlg.Add("Update Group Info From AniDB");
				dlg.Add("Search for Torrents");
				dlg.Add("Permanent Vote");
				dlg.Add("Temporary Vote");
				dlg.Add("Revoke Vote");
				
		
				dlg.DoModal(GUIWindowManager.ActiveWindow);

				//TODO
				/*
				switch (dlg.SelectedId)
				{
					case 1:

						MainWindow.anidbProcessor.UpdateAnimeInfoHTTP(MainAnime.AnimeID, true, false);

						// we do this to get the extra infor like creator list
						MainWindow.anidbProcessor.UpdateAnimeInfo(MainAnime.AnimeID, true, false);
						break;

					case 2:

						MainWindow.anidbProcessor.UpdateGroupStatus(MainAnime.AnimeID, true);

						// we do this to get the extra infor like creator list
						MainWindow.anidbProcessor.UpdateAnimeInfo(MainAnime.AnimeID, true, false);
						break;

					case 3:

						DownloadHelper.SearchAnime(MainAnime);
						break;

					case 4:
						decimal rating = Utils.PromptAniDBRating();
						if (rating > 0)
						{
							MainWindow.anidbProcessor.VoteAnime(MainAnime.AnimeID, rating);
						}
						break;

					case 5:
						decimal ratingTemp = Utils.PromptAniDBRating();
						if (ratingTemp > 0)
						{
							MainWindow.anidbProcessor.VoteAnimeTemp(MainAnime.AnimeID, ratingTemp);
						}
						break;

					case 6:

						if (serMain != null)
						{
							AniDB_Vote vote = serMain.GetUserRatingRecord();
							if (vote != null)
							{
								if (vote.VoteType == (int)enAniDBVoteType.Anime)
									MainWindow.anidbProcessor.VoteAnimeRevoke(MainAnime.AnimeID);
								else
									MainWindow.anidbProcessor.VoteAnimeTempRevoke(MainAnime.AnimeID);
							}
						}
						break;

					
					
				}*/
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write("Error in menu: {0}", ex);
			}
		}

        private void HideControls()
        {
            dummyPageInfo.Visible = false;
            dummyPageStatistics.Visible = false;
			dummyPageGroups.Visible = false;
        }

        protected override void OnPageDestroy(int new_windowId)
        {
            base.OnPageDestroy(new_windowId);
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
			if (this.btnNavLeft != null && control == this.btnNavLeft)
            {
                GUIWindowManager.CloseCurrentWindow();
                GUIWindowManager.ActivateWindow(Constants.WindowIDs.CHARACTERS, false);

                return;
            }

			if (this.btnNavRight != null && control == this.btnNavRight)
            {
                GUIWindowManager.CloseCurrentWindow();
                GUIWindowManager.ActivateWindow(Constants.WindowIDs.RELATIONS, false);

                return;
            }

			if (this.btnInfoPage != null && control == this.btnInfoPage)
            {
                HideControls();
                dummyPageInfo.Visible = true;
            }

			if (this.btnStatsPage != null && control == this.btnStatsPage)
            {
                HideControls();
                dummyPageStatistics.Visible = true;
            }

			if (this.btnGroupsPage != null && control == this.btnGroupsPage)
			{
				HideControls();
				dummyPageGroups.Visible = true;
			}

			if (MA3WindowManager.HandleWindowChangeButton(control))
				return;

            base.OnClicked(controlId, control, actionType);
        }

        private void LoadFanart()
        {
            clearGUIProperty("AnimeInfo.Fanart");
			Fanart fanart = new Fanart(MainAnime); ;

            if (fanart == null) return;

            if (fanart.FileName.Length > 0)
            {
                setGUIProperty("AnimeInfo.Fanart", fanart.FileName);
            }
        }

        public override bool OnMessage(GUIMessage message)
        {
            return base.OnMessage(message);
        }

        public override bool Init()
        {
            return Load(GUIGraphicsContext.Skin + @"\Anime3_AnimeInfo.xml");
        }

        public static void setGUIProperty(string which, string value)
        {
            MediaPortal.GUI.Library.GUIPropertyManager.SetProperty("#Anime3." + which, value);
        }

        public static void clearGUIProperty(string which)
        {
            setGUIProperty(which, "-"); // String.Empty doesn't work on non-initialized fields, as a result they would display as ugly #TVSeries.bla.bla
        }

        private void FormatTextPerEpisodeData()
        {
			//TODO
			/*
            iFileCount = 0;
            iMissingEpisodeCount = 0;
            
            strEpisodeCount = "";

            AnimeEpisodeList seriesEps = null;

            if (serMain != null)
                seriesEps = serMain.Episodes;

            int specials = 0;
            int normal = 0;
            int other = 0;

            int HasNormal = 0;
            int HasSpecials = 0;
            int HasOther = 0;
            long iFilesize = 0;
            int iRunningTime = 0;
            int iEpisodeRunningTime = 0;

            if (serMain != null)
            {
                foreach (AnimeEpisode Episode in seriesEps)
                {
                    // get types
                    switch (Episode.EpisodeType)
                    {
                        case "2":
                            specials++;
                            if (Episode.FileLocals.Count != 0)
                                HasSpecials++;
                            break;
                        case "1":
                            normal++;
                            if (Episode.FileLocals.Count != 0)
                            {
                                HasNormal++;
                            }
                            break;
                        default:
                            BaseConfig.MyAnimeLog.Write(Episode.EpisodeType);
                            other++;
                            if (Episode.FileLocals.Count != 0)
                                HasOther++;
                            break;
                    }

                    List<FileLocal> files = Episode.FileLocals;
                    iFileCount += files.Count;

                    // add each file FileSize to total file FileSize for anime
                    // also get each files audio and subtitle languages and add them to the shows lamgauge lists.
                    // Note on logic:: Instead of counting how many episodes and how many files, then checking if
                    // this is the last file, just to see if we should add another forward slash, this just adds
                    // one everytime, then after to loop we are guaranteed to have a left over slash, and we simply
                    // remove it.
                    bool FoundFile = false;
                    foreach (FileLocal file in files)
                    {
                        iFilesize += file.FileSize;
                        if (file.FileHash==null)
                            continue;
                      
                        // if a normal episode, add file duration to shows duration.
                        if (!FoundFile)
                        {
                            iRunningTime += file.FileHash.Duration;
                            FoundFile = true;

                            if (Episode.EpisodeType == "2")
                                iEpisodeRunningTime += file.FileHash.Duration;
                        }
                    }

                }
            }
            else
            {
                normal = MainAnime.EpisodeCountNormal;
                specials = MainAnime.EpisodeCountSpecial;
                other = 0;
            }


      
            // turn into MegaBytes
            iFilesize = (iFilesize / 1000) / 1000;

            strFileSize = iFilesize.ToString();
            
            // add comma if it is a GB or bigger
            if (strFileSize.Length > 3)
            {
                strFileSize = strFileSize.Insert((iFilesize.ToString().Length - 3), ",");
            }

            strFileSize += " MB's";

            // create number of episodes string
            strEpisodeCount = normal.ToString() + "/" + specials.ToString() + "/" + other.ToString() + " | You have: " + HasNormal.ToString() + "/" + HasSpecials.ToString() + "/" + HasOther.ToString();

            // Create missing episodes string
            iMissingEpisodeCount = normal - HasNormal;

            // Create Running time string
            int Hours, Minutes, Seconds = 0;
            
            Hours = (int)Math.Floor((double)iRunningTime / 3600000);
            iRunningTime = iRunningTime - (3600000 * Hours);
            Minutes = (int)Math.Floor((double)iRunningTime / 60000);
            iRunningTime = iRunningTime - (60000 * Minutes);
            Seconds = (int)Math.Floor((double)iRunningTime / 1000);

            strRunningTime = Hours.ToString() + " Hours " + Minutes.ToString() + " Minutes " + Seconds.ToString() + " Seconds";*/
        }

        private string FormatTextYear()
        {
			return MainAnime.AirDateAsString;
        }

        private string FormatTextRating()
        {   
			string rat = string.Format("{0} (Votes {1}) / {2} (Votes {3})",
				MainAnime.Rating.ToString().Insert(1, "."), MainAnime.VoteCount,
				MainAnime.TempRating.ToString().Insert(1, "."), MainAnime.TempVoteCount);
			return rat;
        }

        private string FormatTextReviews()
        {
            return MainAnime.ReviewCount.ToString() + " (Average rating: " + MainAnime.AvgReviewRating.ToString().Insert(1, ".") + ")";
        }

        private string FormatTextOtherTitles()
        {
			int i = 0;
			string ret = "";

			foreach (string title in MainAnime.Titles)
			{
				if (i == 4) break;
				if (!string.IsNullOrEmpty(ret)) ret += " **";
				ret += title;
				i++;
			}

			return ret;
        }

        private string FormatTextAwards()
        {
            string FormatText = MainAnime.AwardList;
            if (FormatText == "")
                FormatText = "No Awards";

            return FormatText;
        }

        private string FormatTextRestricted()
        {
            if (MainAnime.Restricted == 0)
                return "No";
            else
                return "Yes";
        }

        private string FormatTextTimeSpentWatching()
        {
			//TODO
			return "";
			/*
			if (serMain != null)
			{
				int iTimeSpent = serMain.TimeSpentWatching;

				// Create Time Spent Watching string

				int Hours, Minutes, Seconds = 0;

				Hours = (int)Math.Floor((decimal)iTimeSpent / 3600);
				iTimeSpent = iTimeSpent - (3600 * Hours);
				Minutes = (int)Math.Floor((decimal)iTimeSpent / 60);
				iTimeSpent = iTimeSpent - (60 * Minutes);
				Seconds = iTimeSpent;

				return Hours.ToString() + " Hours " + Minutes.ToString() + " Minutes " + Seconds.ToString() + " Seconds";
			}
			else return "";*/
        }

		private void LoadGroups()
		{
			string mygroupsData = "";
			string othergroupsData = "";

			List<AniDBReleaseGroupVM> allGrps = JMMServerHelper.GetReleaseGroupsForAnime(MainAnime.AnimeID);

			/*
			List<AniDB_GroupStatus> allgrps = MainAnime.GroupsStatus;

			// if no groups are found, try updating the info
			if (allgrps.Count == 0)
			{
				MainWindow.anidbProcessor.UpdateGroupStatus(MainAnime.AnimeID, true);
				MainWindow.anidbProcessor.UpdateAnimeInfo(MainAnime.AnimeID, true, false);

				setGUIProperty("AnimeInfo.Groups.MyGroupsDescription", "Downloading Information...");
				setGUIProperty("AnimeInfo.Groups.OtherGroupsDescription", "Downloading Information...");

				return;
			}*/

			List<AniDBReleaseGroupVM> mygrps = new List<AniDBReleaseGroupVM>();
			List<AniDBReleaseGroupVM> othergrps = new List<AniDBReleaseGroupVM>();

			foreach (AniDBReleaseGroupVM grp in allGrps)
			{
				if (grp.UserCollecting)
					mygrps.Add(grp);
				else
					othergrps.Add(grp);
			}

			if (mygrps.Count > 0)
			{
				// now sort the groups by file count
				List<SortPropOrFieldAndDirection> sortCriteria = new List<SortPropOrFieldAndDirection>();
				sortCriteria.Add(new SortPropOrFieldAndDirection("FileCount", true, SortType.eInteger));
				mygrps = Sorting.MultiSort<AniDBReleaseGroupVM>(mygrps, sortCriteria);

				foreach (AniDBReleaseGroupVM grp in mygrps)
				{
					mygroupsData += string.Format("{0} - ({1} Local Files)", grp.GroupName, grp.FileCount);
					mygroupsData += Environment.NewLine;
				}
			}
			else
				mygroupsData = "-";

			setGUIProperty("AnimeInfo.Groups.MyGroupsDescription", mygroupsData);



			if (othergrps.Count > 0)
			{
				// now sort the groups by name
				List<SortPropOrFieldAndDirection> sortCriteria = new List<SortPropOrFieldAndDirection>();
				sortCriteria.Add(new SortPropOrFieldAndDirection("GroupName", false, SortType.eString));
				othergrps = Sorting.MultiSort<AniDBReleaseGroupVM>(othergrps, sortCriteria);

				foreach (AniDBReleaseGroupVM grp in othergrps)
				{
					othergroupsData += string.Format("{0} - Episode Range: {1}", grp.GroupName, grp.EpisodeRange);
					othergroupsData += Environment.NewLine;
				}
			}
			else
				othergroupsData = "-";

			setGUIProperty("AnimeInfo.Groups.OtherGroupsDescription", othergroupsData);
		}
    }
}