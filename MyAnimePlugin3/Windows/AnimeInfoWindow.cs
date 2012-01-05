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

		[SkinControlAttribute(1247)] protected GUILabelControl dummyUserHasVotedSeries = null;


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

			MainWindow.ServerHelper.GotAnimeEvent += new JMMServerHelper.GotAnimeEventHandler(ServerHelper_GotAnimeEvent);

			setGUIProperty("AnimeInfo.DownloadStatus", "-");
        }

		void ServerHelper_GotAnimeEvent(Events.GotAnimeEventArgs ev)
		{
			if (GUIWindowManager.ActiveWindow != Constants.WindowIDs.ANIMEINFO) return;
			setGUIProperty("AnimeInfo.DownloadStatus", "-");
			int aid = ev.AnimeID;
			LoadInfo();
			SetSkinProperties();
			InfoPage();
		}

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

			if (dummyUserHasVotedSeries != null) dummyUserHasVotedSeries.Visible = false;
			// Only AniDB users have votes
			if (JMMServerVM.Instance.CurrentUser.IsAniDBUserBool)
			{
				string myRating = MainAnime.UserVoteFormatted;
				if (string.IsNullOrEmpty(myRating))
					clearGUIProperty("AnimeInfo.Info.MyRating");
				else
				{
					setGUIProperty("AnimeInfo.Info.MyRating", myRating);
					if (dummyUserHasVotedSeries != null) dummyUserHasVotedSeries.Visible = true;
				}
			}

			setGUIProperty("AnimeInfo.Info.AnidbTitle", string.Format("{0} (AID {1})", MainAnime.FormattedTitle, MainAnime.AnimeID));
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
			setGUIProperty("AnimeInfo.Info.Description", serMain.Description);
			
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
				setGUIProperty("AnimeInfo.Stats.MissingEpisodes", iMissingEpisodeCount.ToString());

                setGUIProperty("AnimeInfo.Stats.SubtitleLanguages", strSubtitleLanguages);
                setGUIProperty("AnimeInfo.Stats.AudioLanguages", strAudioLanguages);

				setGUIProperty("AnimeInfo.Stats.RunningTime", strRunningTime);
			}
        }

        #endregion

		protected override void OnShowContextMenu()
		{
			try
			{
				GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
				if (dlg == null)
					return;

				int mnuUpdate = -1;
				int mnuSearch = -1;
				int mnuVotePerm = -1;
				int mnuVoteTemp = -1;
				int mnuVoteRevoke = -1;

				int curMenu = -1;

				AniDB_VoteVM userVote = MainAnime.UserVote;

				dlg.Reset();
				dlg.SetHeading(MainAnime.FormattedTitle);
				dlg.Add("Update Series Info From AniDB");
				curMenu++; mnuUpdate = curMenu;

				dlg.Add("Search for Torrents");
				curMenu++; mnuSearch = curMenu;

				if (userVote == null && JMMServerVM.Instance.CurrentUser.IsAniDBUserBool)
				{
					dlg.Add("Permanent Vote");
					curMenu++; mnuVotePerm = curMenu;

					dlg.Add("Temporary Vote");
					curMenu++; mnuVoteTemp = curMenu;
				}

				if (userVote != null && JMMServerVM.Instance.CurrentUser.IsAniDBUserBool)
				{
					dlg.Add("Revoke Vote");
					curMenu++; mnuVoteRevoke = curMenu;
				}
				
		
				dlg.DoModal(GUIWindowManager.ActiveWindow);

				int selectedLabel = dlg.SelectedLabel;

				if (selectedLabel == mnuUpdate)
				{
					MainWindow.ServerHelper.UpdateAnime(MainAnime.AnimeID);
					setGUIProperty("AnimeInfo.DownloadStatus", "Waiting on server...");
				}

				if (selectedLabel == mnuSearch)
				{
					DownloadHelper.SearchAnime(MainAnime);
				}

				if (selectedLabel == mnuVotePerm)
				{
					decimal rating = Utils.PromptAniDBRating(MainAnime.FormattedTitle);
					if (rating > 0)
					{
						JMMServerVM.Instance.clientBinaryHTTP.VoteAnime(MainAnime.AnimeID, rating, (int)VoteType.AnimePermanent);
						LoadInfo();
						SetSkinProperties();
						InfoPage();
					}
				}

				if (selectedLabel == mnuVoteTemp)
				{
					decimal ratingTemp = Utils.PromptAniDBRating(MainAnime.FormattedTitle);
					if (ratingTemp > 0)
					{
						JMMServerVM.Instance.clientBinaryHTTP.VoteAnime(MainAnime.AnimeID, ratingTemp, (int)VoteType.AnimeTemporary);
						LoadInfo();
						SetSkinProperties();
						InfoPage();
					}
				}

				if (selectedLabel == mnuVoteRevoke)
				{
					JMMServerVM.Instance.clientBinaryHTTP.VoteAnimeRevoke(MainAnime.AnimeID);
					LoadInfo();
					SetSkinProperties();
					InfoPage();
				}
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

		private void LoadGroups()
		{
			string mygroupsData = "";
			string othergroupsData = "";

			List<AniDBReleaseGroupVM> allGrps = JMMServerHelper.GetReleaseGroupsForAnime(MainAnime.AnimeID);

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