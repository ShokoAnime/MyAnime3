using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using Shoko.Commons.Extensions;
using Shoko.Models.Enums;
using Shoko.MyAnime3.ViewModel;
using Shoko.MyAnime3.ViewModel.Helpers;
using Shoko.MyAnime3.ViewModel.Server;
using Action = MediaPortal.GUI.Library.Action;

namespace Shoko.MyAnime3.Windows
{
    public class ContinueWatchingWindow : GUIWindow
    {
        private List<VM_AnimeEpisode_User> colEpisodes = new List<VM_AnimeEpisode_User>();
        private readonly BackgroundWorker getDataWorker = new BackgroundWorker();

        [SkinControl(50)] protected GUIFacadeControl m_Facade = null;

        [SkinControl(801)] protected GUIButtonControl btnRefresh = null;

        //[SkinControlAttribute(920)] protected GUIButtonControl btnWindowContinueWatching = null;
        [SkinControl(921)] protected GUIButtonControl btnWindowUtilities = null;

        [SkinControl(922)] protected GUIButtonControl btnWindowCalendar = null;

        //[SkinControlAttribute(924)] protected GUIButtonControl btnWindowCollectionStats = null;
        [SkinControl(925)] protected GUIButtonControl btnWindowRecommendations = null;

        [SkinControl(1451)] protected GUILabelControl dummyAnyRecords = null;

        public ContinueWatchingWindow()
        {
            // get ID of windowplugin belonging to this setup
            // enter your own unique code
            GetID = Constants.WindowIDs.WATCHING;

            setGUIProperty("Watching.Status", "-");

            getDataWorker.DoWork += getDataWorker_DoWork;
            getDataWorker.RunWorkerCompleted += getDataWorker_RunWorkerCompleted;
        }

        void getDataWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            colEpisodes = e.Result as List<VM_AnimeEpisode_User>;

            if (colEpisodes == null || colEpisodes.Count == 0)
            {
                if (dummyAnyRecords != null) dummyAnyRecords.Visible = false;
                setGUIProperty("Watching.Status", "No episodes have recently been watched");
                return;
            }

            if (dummyAnyRecords != null) dummyAnyRecords.Visible = true;

            foreach (VM_AnimeEpisode_User ep in colEpisodes)
            {
                GUIListItem item = new GUIListItem("");
                VM_AniDB_Anime anime = ep.AnimeSeries.Anime;

                string imagePath = GUIGraphicsContext.Skin + @"\Media\MyAnime3\anime3_question_poster.png";
                if (anime.DefaultPosterPath.Trim().Length > 0)
                    if (File.Exists(anime.DefaultPosterPath))
                        imagePath = anime.DefaultPosterPath;

                item.IconImage = item.IconImageBig = imagePath;
                item.TVTag = ep;
                item.OnItemSelected += onFacadeItemSelected;
                m_Facade.Add(item);
            }

            if (m_Facade.Count > 0)
            {
                m_Facade.SelectedListItemIndex = 0;

                VM_AnimeEpisode_User ep = m_Facade.SelectedListItem.TVTag as VM_AnimeEpisode_User;
                if (ep != null)
                    SetEpisode(ep);
            }

            if (MainWindow.animeSeriesIDToBeRated.HasValue && BaseConfig.Settings.DisplayRatingDialogOnCompletion)
            {
                VM_AnimeSeries_User ser = (VM_AnimeSeries_User) VM_ShokoServer.Instance.ShokoServices.GetSeries(MainWindow.animeSeriesIDToBeRated.Value,
                    VM_ShokoServer.Instance.CurrentUser.JMMUserID);
                if (ser!=null)
                    Utils.PromptToRateSeriesOnCompletion(ser);
                MainWindow.animeSeriesIDToBeRated = null;
            }
        }

        void getDataWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            List<VM_AnimeEpisode_User> tempEpisodes = new List<VM_AnimeEpisode_User>();
            List<VM_AnimeEpisode_User> epContracts = VM_ShokoServer.Instance.ShokoServices.GetContinueWatchingFilter(VM_ShokoServer.Instance.CurrentUser.JMMUserID, 25).CastList<VM_AnimeEpisode_User>();

            foreach (VM_AnimeEpisode_User ep in epContracts)
            {
                VM_AniDB_Anime anime = ep.AnimeSeries.Anime;
                ep.SetTvDBInfo(anime.TvSummary);

                tempEpisodes.Add(ep);
            }

            // just doing this to preload the series and anime data
            foreach (VM_AnimeEpisode_User ep in colEpisodes)
            {
                VM_AniDB_Anime anime = ep.AnimeSeries.Anime;
            }

            e.Result = tempEpisodes;
        }

        public override bool Init()
        {
            return Load(GUIGraphicsContext.Skin + @"\Anime3_Watching.xml");
        }

        public override int GetID
        {
            get { return Constants.WindowIDs.WATCHING; }
            set { base.GetID = value; }
        }

        protected override void OnPageLoad()
        {
            base.OnPageLoad();
            m_Facade.CurrentLayout = GUIFacadeControl.Layout.Filmstrip;
            BaseConfig.MyAnimeLog.Write("OnPageLoad: ContinueWatchingWindow");

            LoadData();
            m_Facade.Focus = true;
        }

        public static void setGUIProperty(string which, string value)
        {
            GUIPropertyManager.SetProperty("#Anime3." + which, value);
        }

        public static void clearGUIProperty(string which)
        {
            setGUIProperty(which, "-"); // String.Empty doesn't work on non-initialized fields, as a result they would display as ugly #TVSeries.bla.bla
        }

        private void LoadData()
        {
            colEpisodes.Clear();
            GUIControl.ClearControl(GetID, m_Facade.GetID);
            setGUIProperty("Watching.Status", "Loading Data...");
            if (dummyAnyRecords != null) dummyAnyRecords.Visible = false;

            getDataWorker.RunWorkerAsync();
        }

        private void onFacadeItemSelected(GUIListItem item, GUIControl parent)
        {
            //BaseConfig.MyAnimeLog.Write("Facade Item Selected");
            // if this is not a message from the facade, exit
            if (parent != m_Facade && parent != m_Facade.FilmstripLayout)
                return;

            VM_AnimeEpisode_User ep = m_Facade.SelectedListItem.TVTag as VM_AnimeEpisode_User;
            SetEpisode(ep);
        }

        private void SetEpisode(VM_AnimeEpisode_User ep)
        {
            if (ep == null) return;

            /*AniDB_AnimeVM anime = ep.AnimeSeries.AniDB_Anime;
      
                  Dictionary<int, TvDB_Episode> dictTvDBEpisodes = anime.DictTvDBEpisodes;
                  Dictionary<int, int> dictTvDBSeasons = anime.DictTvDBSeasons;
                  Dictionary<int, int> dictTvDBSeasonsSpecials = anime.DictTvDBSeasonsSpecials;
                  CrossRef_AniDB_TvDBVM tvDBCrossRef = anime.CrossRefTvDB;
                  ep.SetTvDBInfo(dictTvDBEpisodes, dictTvDBSeasons, dictTvDBSeasonsSpecials, tvDBCrossRef);*/


            clearGUIProperty("Watching.Series.Title");
            clearGUIProperty("Watching.Series.Description");
            clearGUIProperty("Watching.Series.LastWatched");
            clearGUIProperty("Watching.Series.EpisodesAvailable");
            clearGUIProperty("Watching.Episode.Title");
            clearGUIProperty("Watching.Episode.AirDate");
            clearGUIProperty("Watching.Episode.RunTime");
            clearGUIProperty("Watching.Episode.FileInfo");
            clearGUIProperty("Watching.Episode.Overview");
            clearGUIProperty("Watching.Episode.Image");
            clearGUIProperty("Watching.Episode.Logos");


            setGUIProperty("Watching.Series.Title", ep.AnimeSeries.SeriesName);
            setGUIProperty("Watching.Series.Description", ep.AnimeSeries.Description);
            setGUIProperty("Watching.Series.LastWatched", ep.AnimeSeries.WatchedDate.HasValue ? ep.AnimeSeries.WatchedDate.Value.ToString("dd MMM yy", Globals.Culture) : "-");
            setGUIProperty("Watching.Series.EpisodesAvailable", ep.AnimeSeries.UnwatchedEpisodeCount.ToString());

            setGUIProperty("Watching.Episode.Title", ep.EpisodeNumberAndNameWithType);
            setGUIProperty("Watching.Episode.AirDate", ep.AirDateAsString);
            setGUIProperty("Watching.Episode.RunTime", Utils.FormatSecondsToDisplayTime(ep.AniDB_LengthSeconds));


            setGUIProperty("Watching.Series.Poster", ImageAllocator.GetSeriesImageAsFileName(ep.AnimeSeries, GUIFacadeControl.Layout.List));

            if (ep.EpisodeImageLocation.Length > 0)
                setGUIProperty("Watching.Episode.Image", ep.EpisodeImageLocation);

            try
            {
                Fanart fanart = new Fanart(ep.AnimeSeries);
                if (!string.IsNullOrEmpty(fanart.FileName))
                    setGUIProperty("Watching.Series.Fanart", fanart.FileName);
                else
                    setGUIProperty("Watching.Series.Fanart", "-");
            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write(ep.AnimeSeries.SeriesName + " - " + ex);
            }

            // Overview
            string overview = ep.EpisodeOverview;
            if (BaseConfig.Settings.HidePlot)
                if (ep.EpisodeOverview.Trim().Length > 0 && ep.IsWatched())
                    overview = "*** Hidden to prevent spoilers ***";
            setGUIProperty("Watching.Episode.Overview", overview);

            // File Info
            List<VM_VideoDetailed> filesForEpisode = VM_ShokoServer.Instance.ShokoServices.GetFilesForEpisode(ep.AnimeEpisodeID,
                VM_ShokoServer.Instance.CurrentUser.JMMUserID).CastList<VM_VideoDetailed>();


            string finfo = "";
            foreach (VM_VideoDetailed vid in filesForEpisode)
                finfo = vid.FileSelectionDisplay;

            if (filesForEpisode.Count > 1)
                finfo = filesForEpisode.Count + " Files Available";

            setGUIProperty("Watching.Episode.FileInfo", finfo);

            // Logos
            string logos = Logos.buildLogoImage(ep);

            BaseConfig.MyAnimeLog.Write(logos);
            setGUIProperty("Watching.Episode.Logos", logos);
        }

        protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
        {
            if (MA3WindowManager.HandleWindowChangeButton(control))
                return;

            if (btnRefresh != null && control == btnRefresh)
            {
                btnRefresh.IsFocused = false;
                m_Facade.Focus = true;
                LoadData();
            }

            if (control == m_Facade)
            {
                // show the files if we are looking at a torrent
                GUIListItem item = m_Facade.SelectedListItem;
                if (item == null || item.TVTag == null) return;
                if (item.TVTag.GetType() == typeof(VM_AnimeEpisode_User))
                {
                    VM_AnimeEpisode_User ep = item.TVTag as VM_AnimeEpisode_User;
                    if (ep != null)
                        MainWindow.vidHandler.ResumeOrPlay(ep);
                }
            }

            base.OnClicked(controlId, control, actionType);
        }

        protected override void OnShowContextMenu()
        {
            GUIListItem currentitem = m_Facade.SelectedListItem;
            if (currentitem == null) return;

            if (currentitem.TVTag.GetType() == typeof(VM_AnimeEpisode_User))
            {
                VM_AnimeEpisode_User ep = currentitem.TVTag as VM_AnimeEpisode_User;
                if (ep != null)
                {
                    GUIDialogMenu dlg = (GUIDialogMenu) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_MENU);
                    if (dlg == null)
                        return;

                    dlg.Reset();
                    dlg.SetHeading(ep.EpisodeNumberAndName);
                    dlg.Add("Mark as Watched");
                    dlg.Add("Play Previous Episode");
                    dlg.Add("Go To Episode List");
                    dlg.Add("View Series Info");

                    dlg.DoModal(GUIWindowManager.ActiveWindow);

                    switch (dlg.SelectedLabelText)
                    {
                        case "Mark as Watched":
                            ep.ToggleWatchedStatus(true);
                            LoadData();
                            break;

                        case "Play Previous Episode":
                            if (ep.AnimeSeries == null) return;
                            VM_AnimeEpisode_User epPrev = (VM_AnimeEpisode_User) VM_ShokoServer.Instance.ShokoServices.GetPreviousEpisodeForUnwatched(ep.AnimeSeries.AnimeSeriesID,
                                VM_ShokoServer.Instance.CurrentUser.JMMUserID);

                            if (epPrev == null)
                            {
                                Utils.DialogMsg("Error", "Previous episode not found");
                                return;
                            }
                            MainWindow.vidHandler.ResumeOrPlay(epPrev);
                            break;

                        case "Go To Episode List":
                            if (ep.AnimeSeries == null) return;

                            MainWindow.Breadcrumbs = new List<History>
                            {
                                new History {Selected = GroupFilterHelper.AllGroupsFilter}
                            };

                            // find the group for this series
                            VM_AnimeGroup_User grp = ShokoServerHelper.GetGroup(ep.AnimeSeries.AnimeGroupID);
                            if (grp == null)
                            {
                                BaseConfig.MyAnimeLog.Write("Group not found");
                                return;
                            }
                            MainWindow.ContinueWatching_CurrentSeries = ep.AnimeSeries;

                            MainWindow.Breadcrumbs.Add(new History {Listing = GroupFilterHelper.AllGroupsFilter, Selected = grp});
                            MainWindow.Breadcrumbs.Add(new History {Listing = grp, Selected = MainWindow.ContinueWatching_CurrentSeries});
                            bool foundEpType = false;
                            if (MainWindow.ContinueWatching_CurrentSeries.EpisodeTypesToDisplay.Count == 1)
                            {
                                MainWindow.Breadcrumbs.Add(new History {Listing = MainWindow.ContinueWatching_CurrentSeries, Selected = null});
                            }
                            else
                            {
                                foreach (VM_AnimeEpisodeType anEpType in MainWindow.ContinueWatching_CurrentSeries.EpisodeTypesToDisplay)
                                    if (anEpType.EpisodeType == enEpisodeType.Episode)
                                    {
                                        MainWindow.Breadcrumbs.Add(new History {Listing = MainWindow.ContinueWatching_CurrentSeries, Selected = anEpType});
                                        MainWindow.Breadcrumbs.Add(new History {Listing = anEpType, Selected = null});
                                        foundEpType = true;
                                        break;
                                    }

                                if (!foundEpType) return;
                            }
                            GUIWindowManager.CloseCurrentWindow();
                            GUIWindowManager.ActivateWindow(Constants.WindowIDs.MAIN, false);
                            return;

                        case "View Series Info":

                            if (ep.AnimeSeries == null) return;
                            MainWindow.GlobalSeriesID = ep.AnimeSeries.AnimeSeriesID;
                            GUIWindowManager.ActivateWindow(Constants.WindowIDs.ANIMEINFO, false);

                            break;
                    }
                }
            }
        }
    }
}