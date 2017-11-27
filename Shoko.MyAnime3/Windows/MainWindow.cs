using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using Cornerstone.MP;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using Shoko.Commons.Extensions;
using Shoko.Commons.Utils;
using Shoko.Models.Azure;
using Shoko.Models.Client;
using Shoko.Models.Enums;
using Shoko.Models.Server;
using Shoko.Models.TvDB;
using Shoko.MyAnime3.ConfigFiles;
using Shoko.MyAnime3.DataHelpers;
using Shoko.MyAnime3.Events;
using Shoko.MyAnime3.ImageManagement;
using Shoko.MyAnime3.ViewModel;
using Shoko.MyAnime3.ViewModel.Helpers;
using Shoko.MyAnime3.ViewModel.Server;
using Action = MediaPortal.GUI.Library.Action;
using Timer = System.Timers.Timer;
using View = Shoko.MyAnime3.ConfigFiles.View;
// ReSharper disable VirtualMemberCallInConstructor

namespace Shoko.MyAnime3.Windows
{
    public class MainWindow : GUIWindow, ISetupForm
    {
        #region GUI Controls

        [SkinControl(2)] protected GUIButtonControl btnDisplayOptions = null;

        //[SkinControlAttribute(3)] protected GUIButtonControl btnLayout = null;
        [SkinControl(4)] protected GUIButtonControl btnSettings = null;

        [SkinControl(11)] protected GUIButtonControl btnChangeLayout = null;
        [SkinControl(12)] protected GUIButtonControl btnSwitchUser = null;
        [SkinControl(13)] protected GUIButtonControl btnFilters = null;


        [SkinControl(920)] protected GUIButtonControl btnWindowContinueWatching = null;
        [SkinControl(921)] protected GUIButtonControl btnWindowUtilities = null;
        [SkinControl(922)] protected GUIButtonControl btnWindowCalendar = null;

        //[SkinControlAttribute(924)] protected GUIButtonControl btnWindowCollectionStats = null;
        [SkinControl(925)] protected GUIButtonControl btnWindowRecommendations = null;

        [SkinControl(926)] protected GUIButtonControl btnWindowRandom = null;
        [SkinControl(927)] protected GUIButtonControl btnWindowPlaylists = null;

        [SkinControl(50)] protected GUIFacadeControl m_Facade = null;
        //[SkinControlAttribute(51)]
        //protected GUIListControl test = null;

        //[SkinControlAttribute(526)] protected GUIImage loadingImage = null;

        // let the skins react to what we are displaying
        // all these controls are imported from Anime3_Dummy.xml
        [SkinControl(1232)] protected GUILabelControl dummyIsFanartLoaded = null;

        [SkinControl(1233)] protected GUILabelControl dummyIsDarkFanartLoaded = null;
        [SkinControl(1234)] protected GUILabelControl dummyIsLightFanartLoaded = null;
        [SkinControl(1235)] protected GUILabelControl dummyLayoutListMode = null;
        [SkinControl(1236)] protected GUILabelControl dummyLayoutFilmstripMode = null;
        [SkinControl(1242)] protected GUILabelControl dummyLayoutWideBanners = null;

        [SkinControl(1237)] protected GUILabelControl dummyIsSeries = null;
        [SkinControl(1238)] protected GUILabelControl dummyIsGroups = null;
        [SkinControl(1250)] protected GUILabelControl dummyIsGroupFilters = null;
        [SkinControl(1239)] protected GUILabelControl dummyIsEpisodes = null;
        [SkinControl(1240)] protected GUILabelControl dummyIsEpisodeTypes = null;

        [SkinControl(1241)] protected GUILabelControl dummyIsFanartColorAvailable = null;

        [SkinControl(1243)] protected GUILabelControl dummyIsWatched = null;
        [SkinControl(1244)] protected GUILabelControl dummyIsAvailable = null;

        [SkinControl(1245)] protected GUILabelControl dummyFave = null;
        [SkinControl(1246)] protected GUILabelControl dummyMissingEps = null;
        [SkinControl(1247)] protected GUILabelControl dummyUserHasVotedSeries = null;

        [SkinControl(3401)] protected GUILabelControl dummyQueueAniDB = null;
        [SkinControl(3402)] protected GUILabelControl dummyQueueHasher = null;
        [SkinControl(3403)] protected GUILabelControl dummyQueueImages = null;

        [SkinControl(3463)] protected GUIControl dummyFindActive = null;
        [SkinControl(3464)] protected GUIControl dummyFindModeT9 = null;
        [SkinControl(3465)] protected GUIControl dummyFindModeText = null;


        [SkinControl(1300)] protected GUIImage dummyStarOffPlaceholder = null;
        [SkinControl(1301)] protected GUIImage dummyStarOnPlaceholder = null;
        [SkinControl(1302)] protected GUIImage dummyStarCustomPlaceholder = null;

        #endregion

        /*
    public static Listlevel listLevel = Listlevel.GroupFilter;
    public static object parentLevelObject = null;
    
    */




        public static RandomSeriesEpisodeLevel RandomWindow_RandomLevel = RandomSeriesEpisodeLevel.All;
        public static RandomObjectType RandomWindow_RandomType = RandomObjectType.Series;
        public static object RandomWindow_LevelObject;
        public static VM_AnimeSeries_User RandomWindow_CurrentSeries;
        public static VM_AnimeEpisode_User RandomWindow_CurrentEpisode;
        public static VM_AnimeSeries_User ContinueWatching_CurrentSeries = null;
        public static int RandomWindow_MatchesFound = 0;

        public static bool RandomWindow_SeriesWatched = true;
        public static bool RandomWindow_SeriesUnwatched = true;
        public static bool RandomWindow_SeriesPartiallyWatched = true;
        public static bool RandomWindow_SeriesOnlyComplete = true;
        public static bool RandomWindow_SeriesAllTags = true;
        public static List<string> RandomWindow_SeriesTags = new List<string>();

        public static bool RandomWindow_EpisodeWatched = true;
        public static bool RandomWindow_EpisodeUnwatched = true;
        public static bool RandomWindow_EpisodeAllTags = true;
        public static List<string> RandomWindow_EpisodeTags = new List<string>();


        //private bool fanartSet = false;

        private readonly int artworkDelay = 5;
        private Timer displayGrpFilterTimer;
        private Timer displayGrpTimer;

        public static int? animeSeriesIDToBeRated;

        public static AnimePluginSettings settings => AnimePluginSettings.Instance;
        public static ShokoServerHelper ServerHelper = new ShokoServerHelper();


        public static int GlobalSeriesID = -1; // either AnimeSeriesID
        public static int GlobalAnimeID = -1; // AnimeID
        public static int GlobalSeiyuuID = -1; // SeiyuuID

        public static int CurrentCalendarMonth = DateTime.Now.Month;
        public static int CurrentCalendarYear = DateTime.Now.Year;
        public static int CurrentCalendarButton = 4;

        public static VideoHandler vidHandler;


        public static View currentView;
        public static ViewClassification currentViewClassification = ViewClassification.Views;
        public static string currentStaticViewID = ""; // used to stored current year, genre etc in static views

        private GUIFacadeControl.Layout groupViewMode = GUIFacadeControl.Layout.List; // Poster List

        private readonly GUIFacadeControl.Layout seriesViewMode = GUIFacadeControl.Layout.List;

        //private GUIFacadeControl.Layout episodeTypesViewMode = GUIFacadeControl.Layout.List; // List
        private readonly GUIFacadeControl.Layout episodesViewMode = GUIFacadeControl.Layout.List; // List

        private List<GUIListItem> itemsForDelayedImgLoading;

        private BackgroundWorker workerFacade;
        private readonly BackgroundWorker downloadImagesWorker = new BackgroundWorker();
        public static ImageDownloader imageHelper;

        private readonly AsyncImageResource listPoster;
        private readonly AsyncImageResource fanartTexture;

        public static List<History> Breadcrumbs = new List<History> {new History()};

        //private bool isInitialGroupLoad = true;
        /*
    public static GroupFilterVM curGroupFilter = null;
      public static GroupFilterVM selectedGroupFilter = null;
    
    public static AnimeGroupVM curAnimeGroup = null;
    public static AnimeGroupVM curAnimeGroupViewed = null;
    public static AnimeSeriesVM ser = null;
    public static AnimeEpisodeTypeVM curAnimeEpisodeType = null;
    private AnimeEpisodeVM curAnimeEpisode = null;
        */
        readonly Dictionary<int, QuickSort> GroupFilterQuickSorts;

        public static string LastestVersion = string.Empty;
        public static DateTime NextVersionCheck = DateTime.Now;


        private readonly Timer searchTimer;
        private readonly Timer autoUpdateTimer;
        public static Stopwatch keyCommandDelayTimer = new Stopwatch();
        private SearchCollection search;
        private List<GUIListItem> lstFacadeItems;
        private readonly string searchSound;

        #region GUI Properties

        public enum GuiProperty
        {
            Title,
            Subtitle,
            Description,
            CurrentView,
            SimpleCurrentView,
            NextView,
            LastView,
            SeriesBanner,
            SeasonBanner,
            EpisodeImage,
            Logos,
            SeriesCount,
            GroupCount,
            EpisodeCount,
            RomanjiTitle,
            EnglishTitle,
            KanjiTitle,
            RotatorTitle,
            FindText,
            FindMode,
            FindStartWord,
            FindInput,
            FindMatch,
            FindSharpMode,
            FindAsteriskMode,
            SeriesTitle,
            EpisodesTypeTitle,
            NotificationLine1,
            NotificationLine2,
            NotificationIcon,
            NotificationAction,
            VersionNumber,
            LatestVersionNumber,
            LatestVersionText,
            GroupFilter_FilterName,
            GroupFilter_GroupCount,
            SeriesGroup_MyRating,
            SeriesGroup_SeriesCount,
            SeriesGroup_Genre,
            SeriesGroup_GenreShort,
            SeriesGroup_Year,
            SeriesGroup_RawRating,
            SeriesGroup_RatingVoteCount,
            SeriesGroup_Rating,
            SeriesGroup_Episodes,
            SeriesGroup_EpisodeCountNormal,
            SeriesGroup_EpisodeCountSpecial,
            SeriesGroup_EpisodeCountUnwatched,
            SeriesGroup_EpisodeCountWatched,
            HasherQueueCount,
            HasherQueueState,
            HasherQueueRunning,
            GeneralQueueCount,
            GeneralQueueState,
            GeneralQueueRunning,
            ImagesQueueCount,
            ImagesQueueState,
            ImagesQueueRunning,
            SeriesGroup_EpisodesAvailable,
            SeriesGroup_EpisodeCountNormalAvailable,
            SeriesGroup_EpisodeCountSpecialAvailable,
            Episode_Image,
            Episode_Description,
            Episode_EpisodeName,
            Episode_EpisodeDisplayName,
            Episode_SeriesTypeLabel,
            Episode_AirDate,
            Episode_Length,
            Episode_Rating,
            Episode_RawRating,
            Episode_RatingVoteCount,
            Episode_FileInfo,
            GroupSeriesPoster,
            Fanart_1,
            Fanart_2,
            RatingImage,
            CustomRatingImage,
            ModeToggleKey,
            StartTextToggle
        }

        #endregion

        public void SetGUIProperty(GuiProperty which, string value)
        {
            this.SetGUIProperty(which.ToString(), value);
        }

        public void ClearGUIProperty(GuiProperty which)
        {
            this.ClearGUIProperty(which.ToString());
        }

        public string GetPropertyName(GuiProperty which)
        {
            return this.GetPropertyName(which.ToString());
        }


        private static string StaticGetPropertyName(string which)
        {
            return SkinExtensions.BaseProperties + "." + which.Replace("_", ".").Replace("ñ", "_");
        }

        public static void StaticSetGUIProperty(GuiProperty which, string value)
        {
            if (string.IsNullOrEmpty(value))
                value = " ";
            GUIPropertyManager.SetProperty(StaticGetPropertyName(which.ToString()), value);
        }

        public static void PopulateVersionNumber()
        {
            Assembly a = Assembly.GetExecutingAssembly();
            StaticSetGUIProperty(GuiProperty.VersionNumber, Utils.GetApplicationVersion(a));
            Version v = new Version(Utils.GetApplicationVersion(a));
            if (!string.IsNullOrEmpty(LastestVersion))
            {
                StaticSetGUIProperty(GuiProperty.LatestVersionNumber, LastestVersion);
                Version last = new Version(LastestVersion);
                StaticSetGUIProperty(GuiProperty.LatestVersionText, last.CompareTo(v) > 0 ? Translation.NewVersionAvailable : Translation.PluginUpToDate);
            }
            else
            {
                StaticSetGUIProperty(GuiProperty.LatestVersionText, Translation.PluginUpToDate);
            }
        }

        public delegate void OnToggleWatchedHandler(List<VM_AnimeEpisode_User> episodes, bool state);

        public event OnToggleWatchedHandler OnToggleWatched;

        protected void ToggleWatchedEvent(List<VM_AnimeEpisode_User> episodes, bool state)
        {
            if (OnToggleWatched != null)
                OnToggleWatched(episodes, state);
        }

        public delegate void OnRateSeriesHandler(VM_AnimeSeries_User series, string rateValue);

        public event OnRateSeriesHandler OnRateSeries;

        protected void RateSeriesEvent(VM_AnimeSeries_User series, string rateValue)
        {
            OnRateSeries?.Invoke(series, rateValue);
        }

        public MainWindow()
        {
            // get ID of windowplugin belonging to this setup
            // enter your own unique code
            GetID = Constants.PlugInInfo.ID;

            try
            {
                imageHelper = new ImageDownloader();
                imageHelper.Init();

                listPoster = new AsyncImageResource();
                listPoster.Property = GetPropertyName(GuiProperty.GroupSeriesPoster);
                listPoster.Delay = artworkDelay;

                fanartTexture = new AsyncImageResource();
                fanartTexture.Property = GetPropertyName(GuiProperty.Fanart_1);
                fanartTexture.Delay = artworkDelay;

                GroupFilterQuickSorts = new Dictionary<int, QuickSort>();

                //searching
                if (settings.FindTimeout_s > 0)
                {
                    searchTimer = new Timer();
                    searchTimer.AutoReset = true;
                    searchTimer.Interval = settings.FindTimeout_s * 1000;
                    searchTimer.Elapsed += searchTimer_Elapsed;
                }

                //set the search key sound to the same sound for the REMOTE_1 key
                Key key = new Key('1', (int) Keys.D1);
                Action action = new Action();
                ActionTranslator.GetAction(GetID, key, ref action);
                searchSound = action.SoundFileName;

                // timer for automatic updates
                autoUpdateTimer = new Timer();
                autoUpdateTimer.AutoReset = true;
                autoUpdateTimer.Interval = 5 * 60 * 1000; // 5 minutes * 60 seconds
                autoUpdateTimer.Elapsed += autoUpdateTimer_Elapsed;

                downloadImagesWorker.DoWork += downloadImagesWorker_DoWork;

                OnToggleWatched += MainWindow_OnToggleWatched;

                g_Player.PlayBackEnded += g_Player_PlayBackEnded;
            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write(ex.ToString());
                throw;
            }
        }

        void MainWindow_OnToggleWatched(List<VM_AnimeEpisode_User> episodes, bool state)
        {
            string msg = string.Format("OnToggleWatched: {0} / {1}", episodes.Count, state);
            BaseConfig.MyAnimeLog.Write(msg);
        }

        private void DownloadAllImages()
        {
            //if (!downloadImagesWorker.IsBusy)
            //	downloadImagesWorker.RunWorkerAsync();
        }

        void downloadImagesWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // 1. Download posters from AniDB
            List<VM_AniDB_Anime> contracts = VM_ShokoServer.Instance.ShokoServices.GetAllAnime().CastList<VM_AniDB_Anime>();

            foreach (VM_AniDB_Anime anime in contracts)
            {
                //Thread.Sleep(5); // don't use too many resources
                imageHelper.DownloadAniDBCover(anime, false);

                //if (i == 80) break;
            }

            // 2. Download posters from TvDB
            List<VM_TvDB_ImagePoster> posters = VM_ShokoServer.Instance.ShokoServices.GetAllTvDBPosters(null).CastList<VM_TvDB_ImagePoster>();
            foreach (VM_TvDB_ImagePoster poster in posters)
                //Thread.Sleep(5); // don't use too many resources
                imageHelper.DownloadTvDBPoster(poster, false);

            // 2a. Download posters from MovieDB
            List<VM_MovieDB_Poster> moviePosters = VM_ShokoServer.Instance.ShokoServices.GetAllMovieDBPosters(null).CastList<VM_MovieDB_Poster>();
            foreach (VM_MovieDB_Poster poster in moviePosters)
                //Thread.Sleep(5); // don't use too many resources
                imageHelper.DownloadMovieDBPoster(poster, false);

            // 3. Download wide banners from TvDB
            List<VM_TvDB_ImageWideBanner> banners = VM_ShokoServer.Instance.ShokoServices.GetAllTvDBWideBanners(null).CastList<VM_TvDB_ImageWideBanner>();
            foreach (VM_TvDB_ImageWideBanner banner in banners)
                //Thread.Sleep(5); // don't use too many resources
                imageHelper.DownloadTvDBWideBanner(banner, false);

            // 4. Download fanart from TvDB
            List<VM_TvDB_ImageFanart> fanarts = VM_ShokoServer.Instance.ShokoServices.GetAllTvDBFanart(null).CastList<VM_TvDB_ImageFanart>();
            foreach (VM_TvDB_ImageFanart fanart in fanarts)
                //Thread.Sleep(5); // don't use too many resources
                imageHelper.DownloadTvDBFanart(fanart, false);

            // 4a. Download fanart from MovieDB
            List<VM_MovieDB_Fanart> movieFanarts = VM_ShokoServer.Instance.ShokoServices.GetAllMovieDBFanart(null).CastList<VM_MovieDB_Fanart>();
            foreach (VM_MovieDB_Fanart fanart in movieFanarts)
                //Thread.Sleep(5); // don't use too many resources
                imageHelper.DownloadMovieDBFanart(fanart, false);

            // 5. Download episode images from TvDB
            List<TvDB_Episode> eps = VM_ShokoServer.Instance.ShokoServices.GetAllTvDBEpisodes(null).CastList<TvDB_Episode>();
            foreach (TvDB_Episode episode in eps)
                //Thread.Sleep(5); // don't use too many resources
                imageHelper.DownloadTvDBEpisode(episode, false);
            /*
            // 6. Download posters from Trakt
            List<VM_Trakt_ImagePoster> traktPosters = VM_ShokoServer.Instance.ShokoServices.GetAllT.GetAllTraktPosters(null).CastList<VM_Trakt_ImagePoster>();
            foreach (VM_Trakt_ImagePoster traktposter in traktPosters)
            {
                //Thread.Sleep(5); // don't use too many resources
                if (string.IsNullOrEmpty(traktposter.ImageURL)) continue;
                imageHelper.DownloadTraktPoster(traktposter, false);
            }

            // 7. Download fanart from Trakt
            List<VM_Trakt_ImageFanart> traktFanarts = VM_ShokoServer.Instance.ShokoServices.GetAllTraktFanart(null).CastList<VM_Trakt_ImageFanart>();
            foreach (VM_Trakt_ImageFanart traktFanart in traktFanarts)
            {
                //Thread.Sleep(5); // don't use too many resources
                if (string.IsNullOrEmpty(traktFanart.ImageURL)) continue;
                imageHelper.DownloadTraktFanart(traktFanart, false);
            }
            */
            // 8. Download episode images from Trakt
            /*
            List<Trakt_Episode> traktEpisodes = VM_ShokoServer.Instance.ShokoServices.GetAllTraktEpisodes(null);
            foreach (Trakt_Episode traktEp in traktEpisodes)
            {
                //Thread.Sleep(5); // don't use too many resources
                if (string.IsNullOrEmpty(traktEp.EpisodeImage)) continue;

                // special case for trak episodes
                // Trakt will return the fanart image when no episode image exists, but we don't want this

                int pos = traktEp.EpisodeImage.IndexOf(@"episodes/", StringComparison.Ordinal);
                if (pos <= 0) continue;

                imageHelper.DownloadTraktEpisode(traktEp, false);
            }*/
        }


        #region External Event Handlers

        #endregion

        public override bool Init()
        {
            try
            {
                BaseConfig.MyAnimeLog.Write("INIT MAIN WINDOW");
                Translation.PopulateLabels();
                PopulateSearchLabels();
                GUIWindowManager.OnThreadMessageHandler += GUIWindowManager_OnThreadMessageHandler;
                Thread t = new Thread(InitVidHandler);
                t.IsBackground = true;
                t.Start();
            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write("Error on init: {0}", ex.ToString());
            }
            return this.InitSkin<GuiProperty>("Anime3_Main.xml");
        }

        public void InitVidHandler()
        {
            vidHandler = new VideoHandler();
            vidHandler.DefaultAudioLanguage = settings.DefaultAudioLanguage;
            vidHandler.DefaultSubtitleLanguage = settings.DefaultSubtitleLanguage;
        }

        void PopulateSearchLabels()
        {
            SetGUIProperty(GuiProperty.ModeToggleKey, BaseConfig.Settings.ModeToggleKey);
            SetGUIProperty(GuiProperty.StartTextToggle, BaseConfig.Settings.StartTextToggleKey);
        }

        void Instance_ServerStatusEvent(ServerStatusEventArgs ev)
        {
            SetGUIProperty(GuiProperty.HasherQueueCount, ev.HasherQueueCount.ToString(Globals.Culture));
            SetGUIProperty(GuiProperty.HasherQueueState, ev.HasherQueueState);
            SetGUIProperty(GuiProperty.HasherQueueRunning, ev.HasherQueueRunning ? Translation.Running : Translation.Paused);

            SetGUIProperty(GuiProperty.GeneralQueueCount, ev.GeneralQueueCount.ToString(Globals.Culture));
            SetGUIProperty(GuiProperty.GeneralQueueState, ev.GeneralQueueState);
            SetGUIProperty(GuiProperty.GeneralQueueRunning, ev.GeneralQueueRunning ? Translation.Running : Translation.Paused);

            SetGUIProperty(GuiProperty.ImagesQueueCount, ev.ImagesQueueCount.ToString(Globals.Culture));
            SetGUIProperty(GuiProperty.ImagesQueueState, ev.ImagesQueueState);
            SetGUIProperty(GuiProperty.ImagesQueueRunning, ev.ImagesQueueRunning ? Translation.Running : Translation.Paused);


            if (dummyQueueAniDB != null) dummyQueueAniDB.Visible = ev.GeneralQueueCount >= 0;
            if (dummyQueueHasher != null) dummyQueueHasher.Visible = ev.HasherQueueCount >= 0;
            if (dummyQueueImages != null) dummyQueueImages.Visible = ev.ImagesQueueCount >= 0;
        }


        #region ISetupForm Members

        // Returns the name of the plugin which is shown in the plugin menu
        public string PluginName()
        {
            return Constants.PlugInInfo.NAME;
        }

        // Returns the description of the plugin is shown in the plugin menu
        public string Description()
        {
            return Constants.PlugInInfo.DESCRIPTION;
        }

        // Returns the author of the plugin which is shown in the plugin menu
        public string Author()
        {
            return Constants.PlugInInfo.AUTHOR;
        }

        // show the setup dialog
        public void ShowPlugin()
        {
            frmConfig cfg = new frmConfig();
            cfg.ShowDialog();
        }

        // Indicates whether plugin can be enabled/disabled
        public bool CanEnable()
        {
            return true;
        }

        // Enter the id number here again
        public int GetWindowId()
        {
            return Constants.PlugInInfo.ID;
        }

        // Indicates if plugin is enabled by default;
        public bool DefaultEnabled()
        {
            return true;
        }

        // indicates if a plugin has it's own setup screen
        public bool HasSetup()
        {
            return true;
        }


        /// <summary>
        /// If the plugin should have it's own button on the main menu of MediaPortal then it
        /// should return true to this method, otherwise if it should not be on home
        /// it should return false
        /// </summary>
        /// <param name="strButtonText">text the button should have</param>
        /// <param name="strButtonImage">image for the button, or empty for default</param>
        /// <param name="strButtonImageFocus">image for the button, or empty for default</param>
        /// <param name="strPictureImage">subpicture for the button or empty for none</param>
        /// <returns>true : plugin needs it's own button on home
        /// false : plugin does not need it's own button on home</returns>
        public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
        {
            AnimePluginSettings sett = AnimePluginSettings.Instance;

            strButtonText = sett.PluginName;
            strButtonImage = String.Empty;
            strButtonImageFocus = String.Empty;
            strPictureImage = "hover_my anime3.jpg";
            return true;
        }

        #endregion

        public VM_AnimeGroup_User GetTopGroup()
        {
            for (int x = 0; x < Breadcrumbs.Count; x++)
                if (Breadcrumbs[x].Listing is VM_AnimeGroup_User)
                    return (VM_AnimeGroup_User) Breadcrumbs[x].Listing;
            return null;
        }

        public VM_AnimeSeries_User GetTopSerie()
        {
            for (int x = 0; x < Breadcrumbs.Count; x++)
                if (Breadcrumbs[x].Listing is VM_AnimeSeries_User)
                    return (VM_AnimeSeries_User) Breadcrumbs[x].Listing;
            return null;
        }

        public VM_AnimeEpisodeType GetTopEpType()
        {
            for (int x = 0; x < Breadcrumbs.Count; x++)
                if (Breadcrumbs[x].Listing is VM_AnimeEpisodeType)
                    return (VM_AnimeEpisodeType) Breadcrumbs[x].Listing;
            return null;
        }

        public History GetCurrent()
        {
            return Breadcrumbs[Breadcrumbs.Count - 1];
        }

        private void EvaluateVisibility()
        {
            bool fave = false;
            bool missing = false;
            VM_AnimeGroup_User grp = GetTopGroup();

            if (grp != null)
            {
                if (grp.IsFave == 1)
                    fave = true;

                //BaseConfig.MyAnimeLog.Write("settings.ShowMissing: {0}", settings.ShowMissing);
                bool missingVisible = false;
                if (settings.ShowMissing && GetCurrent().Selected is VM_AnimeSeries_User)
                    missingVisible = ((VM_AnimeSeries_User) GetCurrent().Selected).HasMissingEpisodesGroups;

                if (settings.ShowMissing && GetCurrent().Selected is VM_AnimeGroup_User)
                    missingVisible = ((VM_AnimeGroup_User) GetCurrent().Selected).HasMissingEpisodes;

                if (settings.ShowMissing)
                    missing = missingVisible;
                if (dummyFave != null) dummyFave.Visible = fave;
                if (dummyMissingEps != null) dummyMissingEps.Visible = missing;
                //BaseConfig.MyAnimeLog.Write("EvaluateVisibility:: {0} - {1} - {2}", imgListFave != null, curAnimeGroup.IsFave, dummyLayoutListMode.Visible);
            }

            //EvaluateServerStatus();
        }

        protected override void OnPageDestroy(int new_windowId)
        {
            //Removed will generate problems with MediaPortal
            //UnSubClass();
     

            base.OnPageDestroy(new_windowId);
        }
        /*
#region Detect application focus

const int GWL_WNDPROC = -4;
const int WM_ACTIVATEAPP = 0x1C;

// This static method is required because legacy OSes do not support
// SetWindowLongPtr
public static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
{
    if (IntPtr.Size == 8)
        return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
    return new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));
}

[DllImport("user32.dll", EntryPoint = "SetWindowLong")]
private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

[DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

[DllImport("user32.dll", EntryPoint = "CallWindowProc")]
private static extern int CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hwnd, int msg, int wParam, int lParam);

private delegate int WindowProc(IntPtr hwnd, int msg, int wParam, int lParam);

IntPtr DefWindowProc = IntPtr.Zero;
WindowProc NewWindowProc;

void SubClass()
{
    IntPtr hWnd = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
    NewWindowProc = MyWindowProc;
    DefWindowProc = SetWindowLongPtr(hWnd, GWL_WNDPROC, Marshal.GetFunctionPointerForDelegate(NewWindowProc));
}

void UnSubClass()
{
    IntPtr hWnd = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
    DefWindowProc = SetWindowLongPtr(hWnd, GWL_WNDPROC, DefWindowProc);
    DefWindowProc = IntPtr.Zero;
}

        int MyWindowProc(IntPtr hWnd, int msg, int wParam, int lParam)
        {
            if (msg == WM_ACTIVATEAPP)
            {
                // Legacy keyboard hook controller, no longer needed
            }

            return CallWindowProc(DefWindowProc, hWnd, msg, wParam, lParam);
        }

        #endregion
        */
        protected override void OnPageLoad()
        {
            BaseConfig.MyAnimeLog.Write("Starting page load...");
      

            Utils.GetLatestVersionAsync();

            //Removed will generate problems with MediaPortal
            //SubClass();

            if (!isFirstInitDone)
                OnFirstStart();

            currentViewClassification = settings.LastViewClassification;
            currentStaticViewID = settings.LastStaticViewID;
            currentView = settings.LastView;


            groupViewMode = settings.LastGroupViewMode;
            m_Facade.CurrentLayout = groupViewMode;
            //backdrop.LoadingImage = loadingImage;

            Console.Write(VM_ShokoServer.Instance.ServerOnline.ToString());

            LoadFacade();
            m_Facade.Focus = true;

            SkinSettings.Load();

            //MainWindow.anidbProcessor.UpdateVotesHTTP(MainWindow.settings.Username, MainWindow.settings.Password);


            autoUpdateTimer.Start();

            BaseConfig.MyAnimeLog.Write("Thumbs Setting Folder: {0}", settings.ThumbsFolder);

            //searching
            ClearGUIProperty(GuiProperty.FindInput);
            ClearGUIProperty(GuiProperty.FindText);
            ClearGUIProperty(GuiProperty.FindMatch);


            search = new SearchCollection();
            search.List = m_Facade;
            search.ListItemSearchProperty = "DVDLabel";
            search.Mode = settings.FindMode;
            search.StartWord = settings.FindStartWord;

            UpdateSearchPanel(false);


            DownloadAllImages();
        }


        void autoUpdateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
        }


        private void AddFacadeItem(GUIListItem item)
        {
            int selectedIndex = m_Facade.SelectedListItemIndex;
            SaveOrRestoreFacadeItems(false);

            m_Facade.Add(item);

            if (searchTimer != null && searchTimer.Enabled)
                DoSearch(selectedIndex);
        }

        private void LoadFacade()
        {
            try
            {
                if (workerFacade == null)
                {
                    workerFacade = new BackgroundWorker();
                    workerFacade.WorkerReportsProgress = true;
                    workerFacade.WorkerSupportsCancellation = true;

                    workerFacade.DoWork += workerFacade_DoWork;
                    workerFacade.RunWorkerCompleted += workerFacade_RunWorkerCompleted;
                    workerFacade.ProgressChanged += workerFacade_ProgressChanged;
                }

                lock (workerFacade)
                {
                    if (workerFacade.IsBusy) // we have to wait - complete method will call LoadFacade again
                    {
                        if (!workerFacade.CancellationPending)
                            workerFacade.CancelAsync();
                        return;
                    }
                    prepareLoadFacade();
                    workerFacade.RunWorkerAsync();
                }
            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write("LoadFacade ERROR:: {0}", ex);
            }
        }

        void prepareLoadFacade()
        {
            try
            {
                GUIControl.ClearControl(GetID, m_Facade.GetID);

                SetFacade();

                m_Facade.ListLayout.Clear();

                if (m_Facade.ThumbnailLayout != null)
                    m_Facade.ThumbnailLayout.Clear();

                if (m_Facade.FilmstripLayout != null)
                    m_Facade.FilmstripLayout.Clear();

                if (m_Facade.CoverFlowLayout != null)
                    m_Facade.CoverFlowLayout.Clear();


                if (m_Facade != null) m_Facade.Focus = true;
            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write(ex.ToString());
            }
        }

        void workerFacade_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            try
            {
                BackgroundFacadeLoadingArgument arg = e.UserState as BackgroundFacadeLoadingArgument;

                if (workerFacade.CancellationPending)
                {
                    BaseConfig.MyAnimeLog.Write("bg_ProgressChanged cancelled");
                    return;
                }

                if (arg == null || arg.Type == BackGroundLoadingArgumentType.None) return;

                switch (arg.Type)
                {
                    case BackGroundLoadingArgumentType.ListFullElement:
                    case BackGroundLoadingArgumentType.ListElementForDelayedImgLoading:
                        List<GUIListItem> ls = arg.Argument as List<GUIListItem>;
                        if (m_Facade != null && ls != null && ls.Count > 0)
                        {
                            foreach (GUIListItem gli in ls)
                            {
                                //BaseConfig.MyAnimeLog.Write("workerFacade_ProgressChanged - ListElementForDelayedImgLoading");
                                // Messages are not recieved in OnMessage for Filmstrip, instead subscribe to OnItemSelected
                                if (m_Facade.CurrentLayout == GUIFacadeControl.Layout.Filmstrip || m_Facade.CurrentLayout == GUIFacadeControl.Layout.CoverFlow)
                                    gli.OnItemSelected += onFacadeItemSelected;


                                AddFacadeItem(gli);
                                if (arg.Type == BackGroundLoadingArgumentType.ListElementForDelayedImgLoading)
                                {
                                    if (itemsForDelayedImgLoading == null)
                                        itemsForDelayedImgLoading = new List<GUIListItem>();
                                    itemsForDelayedImgLoading.Add(gli);
                                }
                            }
                            if (m_Facade.SelectedListItemIndex < 1)
                            {
                                m_Facade.Focus = true;
                                SelectItem(arg.IndexArgument);
                            }
                        }
                        break;
                    case BackGroundLoadingArgumentType.DelayedImgLoading:
                    {
                        if (itemsForDelayedImgLoading != null && itemsForDelayedImgLoading.Count > arg.IndexArgument)
                        {
                            string image = arg.Argument as string;
                            itemsForDelayedImgLoading[arg.IndexArgument].IconImageBig = image;
                        }
                    }
                        break;

                    case BackGroundLoadingArgumentType.DelayedImgInit:
                        itemsForDelayedImgLoading = null;
                        break;
                    case BackGroundLoadingArgumentType.SetFacadeMode:
                        //GUIFacadeControl.Layout viewMode = (GUIFacadeControl.Layout) arg.Argument;
                        //setFacadeMode(viewMode);
                        break;

                    case BackGroundLoadingArgumentType.ElementSelection:
                    {
                        // thread told us which element it'd like to select
                        // however the user might have already started moving around
                        // if that is the case, we don't select anything
                        if (m_Facade != null && m_Facade.SelectedListItemIndex < 1)
                        {
                            m_Facade.Focus = true;
                            SelectItem(arg.IndexArgument);
                        }
                    }
                        break;
                }
            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write(string.Format("Error in bg_ProgressChanged: {0}: {1}", ex.Message, ex.InnerException));
            }
        }

        bool m_bQuickSelect;

        void SelectItem(int index)
        {
            // BaseConfig.MyAnimeLog.Write("SelectItem: {0}", index.ToString());

            // Hack for 'set' SelectedListItemIndex not being implemented in Filmstrip View
            // Navigate to selected using OnAction instead 
            if (m_Facade.CurrentLayout == GUIFacadeControl.Layout.Filmstrip || m_Facade.CurrentLayout == GUIFacadeControl.Layout.CoverFlow)
            {
                if (GetCurrent().Selected is VM_AnimeGroup_User || GetCurrent().Selected is VM_AnimeSeries_User)
                {
                    int currentIndex = m_Facade.SelectedListItemIndex;
                    if (index >= 0 && index < m_Facade.Count && index != currentIndex)
                    {
                        m_bQuickSelect = true;
                        int increment = currentIndex < index ? 1 : -1;
                        Action.ActionType actionType = currentIndex < index ? Action.ActionType.ACTION_MOVE_RIGHT : Action.ActionType.ACTION_MOVE_LEFT;
                        for (int i = currentIndex; i != index; i += increment)
                        {
                            // Now push fields to skin
                            if (i == index - increment)
                                m_bQuickSelect = false;

                            m_Facade.OnAction(new Action(actionType, 0, 0));
                        }
                        m_bQuickSelect = false;
                    }
                    else
                    {
                        if (GetCurrent().Selected is VM_AnimeGroup_User && m_Facade.Count > 0)
                            Group_OnItemSelected(m_Facade.SelectedListItem);
                    }
                }
            }
            else
            {
                m_Facade.SelectedListItemIndex = index;
            }
        }

        void workerFacade_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // ZF - seems to be crashing because of facade being null sometimes, before getting inside the plugin
            if (m_Facade == null)
                return;

            if (e.Cancelled)
            {
                LoadFacade(); // we only cancel if the user clicked something while we were still loading
                // whatever was selected we will enter (this is because m_selected whatever will not get updated
                // even if the user selects somethign else while we wait for cancellation due to it being a different listlevel)                                
                return;
            }

            if (m_Facade == null)
                return;

            m_Facade.Focus = true;
        }

        void workerFacade_DoWork(object sender, DoWorkEventArgs e)
        {
            bgLoadFacade();
            if (workerFacade.CancellationPending)
                e.Cancel = true;
        }

        void ReportFacadeLoadingProgress(BackGroundLoadingArgumentType type, int indexArgument, object state)
        {
            if (!workerFacade.CancellationPending)
            {
                BackgroundFacadeLoadingArgument Arg = new BackgroundFacadeLoadingArgument();
                Arg.Type = type;
                Arg.IndexArgument = indexArgument;
                Arg.Argument = state;

                workerFacade.ReportProgress(0, Arg);
            }
        }

        /*
      private Listlevel LevelFromItem(IVM ivm)
      {
          Listlevel listlevel;
          if (ivm == null || ivm is GroupFilterVM)
          {
              GroupFilterVM gfvm = ivm as GroupFilterVM;
              if (gfvm != null && gfvm.Childs.Count == 0)
                  listlevel = Listlevel.Group;
              else
                  listlevel = Listlevel.GroupFilter;
          }
          else if (ivm is AnimeGroupVM)
          {
              listlevel = Listlevel.GroupAndSeries;
          }
          else if (ivm is AnimeSeriesVM)
          {
              AnimeSeriesVM asvm = ivm as AnimeSeriesVM;
              if (asvm.EpisodeTypesToDisplay.Count == 1)
                  listlevel = Listlevel.Episode;
              else
                  listlevel = Listlevel.EpisodeTypes;
          }
          else
          {
              listlevel = Listlevel.Episode;
          }
            return listlevel;
      }
        */
        void bgLoadFacade()
        {
            try
            {
                GUIListItem item ;
                int selectedIndex = -1;
                int count = 0;
                bool delayedImageLoading = false;
                List<VM_AnimeGroup_User> groups = null;
                List<VM_GroupFilter> groupFilters ;
                List<GUIListItem> list = new List<GUIListItem>();
                BackGroundLoadingArgumentType type = BackGroundLoadingArgumentType.None;
                History level = GetCurrent();

                #region Group Filters

                if (level.Listing == null || level.Listing is VM_GroupFilter && ((VM_GroupFilter) level.Listing).Childs.Count > 0)
                {
                    // List/Poster/Banner
                    VM_GroupFilter selected = (VM_GroupFilter) level.Selected;

                    SetGUIProperty(GuiProperty.SimpleCurrentView, Translation.GroupFilters);

                    if (groupViewMode != GUIFacadeControl.Layout.List)
                    {
                        // reinit the itemsList
                        delayedImageLoading = true;
                        ReportFacadeLoadingProgress(BackGroundLoadingArgumentType.DelayedImgInit, 0, null);
                    }

                    // text as usual
                    ReportFacadeLoadingProgress(BackGroundLoadingArgumentType.SetFacadeMode, 0,
                        GUIFacadeControl.Layout.List);

                    if (workerFacade.CancellationPending)
                        return;

                    groupFilters = level.Listing == null
                        ? FacadeHelper.GetTopLevelGroupFilters()
                        : FacadeHelper.GetChildFilters((VM_GroupFilter) level.Listing);
                    type = BackGroundLoadingArgumentType.ListFullElement;

                    SetGUIProperty(GuiProperty.GroupCount, groupFilters.Count.ToString());

                    foreach (VM_GroupFilter grpFilter in groupFilters)
                    {
                        if (workerFacade.CancellationPending) return;
                        try
                        {
                            item = null;

                            SetGroupFilterListItem(ref item, grpFilter);
                            if (workerFacade.CancellationPending) return;
                            if (selected != null && grpFilter.GroupFilterID != 0 && selected.GroupFilterID != 0 &&
                                grpFilter.GroupFilterID == selected.GroupFilterID)
                                selectedIndex = count;
                            list.Add(item);
                        }
                        catch (Exception ex)
                        {
                            string msg = string.Format("The 'LoadFacade' function has generated an error displaying list items GF: {0} - {1}", level.Listing == null ? "MAIN" : ((VM_GroupFilter) level.Listing).GroupFilterName, ex);
                            BaseConfig.MyAnimeLog.Write(msg);
                        }
                        count++;
                    }
                }

                #endregion

                #region Groups

                else if (level.Listing is VM_GroupFilter)
                {
                    // List/Poster/Banner
                    VM_GroupFilter gf = (VM_GroupFilter) level.Listing;
                    VM_AnimeGroup_User selected = (VM_AnimeGroup_User) level.Selected;
                    SetGUIProperty(GuiProperty.SimpleCurrentView, gf.GroupFilterName);

                    if (groupViewMode != GUIFacadeControl.Layout.List)
                    {
                        // reinit the itemsList
                        delayedImageLoading = true;
                        ReportFacadeLoadingProgress(BackGroundLoadingArgumentType.DelayedImgInit, 0, null);
                    }

                    if (groupViewMode != GUIFacadeControl.Layout.List)
                        ReportFacadeLoadingProgress(BackGroundLoadingArgumentType.SetFacadeMode, 0, GUIFacadeControl.Layout.AlbumView);
                    else
                        ReportFacadeLoadingProgress(BackGroundLoadingArgumentType.SetFacadeMode, 0, GUIFacadeControl.Layout.List);

                    if (workerFacade.CancellationPending)
                        return;

                    groups = ShokoServerHelper.GetAnimeGroupsForFilter(gf);


                    // re-sort if user has set a quick sort
                    if (GroupFilterQuickSorts.ContainsKey(gf.GroupFilterID))
                    {
                        BaseConfig.MyAnimeLog.Write("APPLYING QUICK SORT");
                        QuickSort srt = GroupFilterQuickSorts[gf.GroupFilterID];
                        groups = groups.AsQueryable().GeneratePredicate(GroupFilterHelper.GetEnumForText_Sorting(srt.SortType), srt.SortDirection).ToList();
                    }


                    // Update Series Count Property
                    SetGUIProperty(GuiProperty.GroupCount, groups.Count.ToString(Globals.Culture));
                    type = groupViewMode != GUIFacadeControl.Layout.List ? BackGroundLoadingArgumentType.ListElementForDelayedImgLoading : BackGroundLoadingArgumentType.ListFullElement;

                    int seriesCount = 0;

                    double totalTime = 0;
                    DateTime start = DateTime.Now;

                    BaseConfig.MyAnimeLog.Write("Building groups: " + gf.GroupFilterName);
                    foreach (VM_AnimeGroup_User grp in groups)
                    {
                        if (workerFacade.CancellationPending) return;
                        try
                        {
                            item = null;

                            //BaseConfig.MyAnimeLog.Write(string.Format("{0} - {1}", grp.GroupName, grp.AniDBRating));

                            SetGroupListItem(ref item, grp);

                            if (settings.HideWatchedFiles && grp.UnwatchedEpisodeCount <= 0)
                                continue;

                            seriesCount += grp.AllSeriesCount;

                            if (selected != null && selected.AnimeGroupID == grp.AnimeGroupID)
                                selectedIndex = count;

                            if (workerFacade.CancellationPending) return;
                            list.Add(item);
                            count++;
                        }
                        catch (Exception ex)
                        {
                            string msg = string.Format("The 'LoadFacade' function has generated an error displaying list items GF: {0} - {1}", gf.GroupFilterName, ex);
                            BaseConfig.MyAnimeLog.Write(msg);
                        }
                    }

                    TimeSpan ts2 = DateTime.Now - start;
                    totalTime += ts2.TotalMilliseconds;

                    BaseConfig.MyAnimeLog.Write("Total time for rendering groups: {0}-{1}", groups.Count, totalTime);

                    SetGUIProperty(GuiProperty.SeriesCount, seriesCount.ToString());
                }

                #endregion

                #region Series

                else if (level.Listing is VM_AnimeGroup_User)
                {
                    // this level includes series as well as sub-groups

                    VM_AnimeGroup_User ag = (VM_AnimeGroup_User) level.Listing;
                    IVM selected = level.Selected;


                    if (seriesViewMode != GUIFacadeControl.Layout.List)
                    {
                        // reinit the itemsList
                        delayedImageLoading = true;
                        ReportFacadeLoadingProgress(BackGroundLoadingArgumentType.DelayedImgInit, 0, null);
                    }

                    if (workerFacade.CancellationPending) return;


                    List<VM_AnimeGroup_User> subGroups = ag.SubGroups;
                    if (subGroups.Count > 0)
                        subGroups = subGroups.OrderBy(a => a.SortName).ToList();

                    // get the series for this group
                    List<VM_AnimeSeries_User> seriesList = ag.ChildSeries;
                    if (seriesList.Count > 0)
                        seriesList = seriesList.OrderBy(a => a.AirDate).ToList();
                    //if (seriesList.Count == 0)
                    //	bFacadeEmpty = true;

                    // Update Series Count Property
                    SetGUIProperty(GuiProperty.SeriesCount, seriesList.Count.ToString());

                    // now sort the groups by air date


                    type = BackGroundLoadingArgumentType.ListFullElement;

                    foreach (VM_AnimeGroup_User grp in subGroups)
                    {
                        if (workerFacade.CancellationPending) return;
                        try
                        {
                            item = null;

                            SetGroupListItem(ref item, grp);

                            if (settings.HideWatchedFiles && grp.UnwatchedEpisodeCount <= 0)
                                continue;
                            if (selected is VM_AnimeGroup_User && ((VM_AnimeGroup_User) selected).AnimeGroupID == grp.AnimeGroupID)
                                selectedIndex = count;
                            if (workerFacade.CancellationPending) return;
                            list.Add(item);
                            count++;
                        }
                        catch (Exception ex)
                        {
                            string msg = string.Format("The 'LoadFacade' function has generated an error displaying list items  Group: {0} - {1}", ag.GroupName, ex);
                            BaseConfig.MyAnimeLog.Write(msg);
                        }
                    }

                    foreach (VM_AnimeSeries_User ser in seriesList)
                    {
                        //BaseConfig.MyAnimeLog.Write("LoadFacade-Series:: {0}", ser);
                        if (workerFacade.CancellationPending) return;
                        try
                        {
                            item = null;

                            SetSeriesListItem(ref item, ser);

                            if (settings.HideWatchedFiles && ser.UnwatchedEpisodeCount <= 0)
                                continue;
                            if (selected is VM_AnimeSeries_User && ((VM_AnimeSeries_User) selected).AnimeSeriesID == ser.AnimeSeriesID)
                                selectedIndex = count;
                            if (workerFacade.CancellationPending) return;
                            list.Add(item);
                            count++;
                        }
                        catch (Exception ex)
                        {
                            string msg = string.Format("The 'LoadFacade' function has generated an error displaying list items Group: {0} - {1}", ag.GroupName, ex);
                            BaseConfig.MyAnimeLog.Write(msg);
                        }
                    }
                }

                #endregion

                #region Episode Types

                else if (level.Listing is VM_AnimeSeries_User && ((VM_AnimeSeries_User) level.Listing).EpisodeTypes.Count > 1)
                {
                    if (workerFacade.CancellationPending) return;

                    VM_AnimeSeries_User asvm = (VM_AnimeSeries_User) level.Listing;
                    VM_AnimeEpisodeType selected = (VM_AnimeEpisodeType) level.Selected;

                    //List<AnimeEpisodeType> anEpTypes = AnimeSeries.GetEpisodeTypes(ser.AnimeSeriesID.Value);
                    type = BackGroundLoadingArgumentType.ListFullElement;
                    foreach (VM_AnimeEpisodeType anEpType in asvm.EpisodeTypesToDisplay)
                    {
                        if (workerFacade.CancellationPending) return;
                        item = null;
                        SetEpisodeTypeListItem(ref item, anEpType);
                        if (selected != null && selected.EpisodeType == anEpType.EpisodeType)
                            selectedIndex = count;
                        list.Add(item);
                        count++;
                    }
                }

                #endregion

                #region Episodes

                else if (level.Listing is VM_AnimeSeries_User || level.Listing is VM_AnimeEpisodeType)
                {
                    if (workerFacade.CancellationPending) return;

                    VM_AnimeEpisodeType vm = level.Listing as VM_AnimeEpisodeType;
                    if (vm == null)
                    {
                        VM_AnimeSeries_User asvm = level.Listing as VM_AnimeSeries_User;
                        vm = asvm.EpisodeTypes[0];
                    }

                    EpisodeType eptype = vm.EpisodeType;

                    // get the episodes for this series / episode types
                    //BaseConfig.MyAnimeLog.Write("GetEpisodes:: {0}", ser.AnimeSeriesID.Value);

                    //List<AnimeEpisode> episodeList = AnimeSeries.GetEpisodes(ser.AnimeSeriesID.Value);
                    vm.AnimeSeries.RefreshEpisodes();
                    List<VM_AnimeEpisode_User> episodeList = vm.AnimeSeries.GetEpisodesToDisplay(eptype);

                    // Update Series Count Property
                    //setGUIProperty(guiProperty.SeriesCount, episodeList.Count.ToString());

                    bool foundFirstUnwatched = false;
                    type = BackGroundLoadingArgumentType.ListFullElement;
                    foreach (VM_AnimeEpisode_User ep in episodeList)
                    {
                        //BaseConfig.MyAnimeLog.Write("LoadFacade-Episodes:: {0}", ep);
                        if (workerFacade.CancellationPending) return;
                        try
                        {
                            item = null;
                            bool isWatched = SetEpisodeListItem(ref item, ep);

                            if (isWatched && settings.HideWatchedFiles)
                                continue;

                            if (!foundFirstUnwatched && !isWatched && ep.LocalFileCount > 0)
                            {
                                selectedIndex = count;
                                foundFirstUnwatched = true;
                            }

                            if (workerFacade.CancellationPending) return;
                            list.Add(item);
                        }
                        catch (Exception ex)
                        {
                            string msg = string.Format("The 'LoadFacade' function has generated an error displaying list items: Serie: {0} - {1}", vm.AnimeSeries.SeriesName, ex);
                            BaseConfig.MyAnimeLog.Write(msg);
                        }
                        count++;
                    }

                    SetFanartForEpisodes();
                    SetGUIProperty(GuiProperty.EpisodeCount, count.ToString());
                }

                #endregion


                #region Report ItemToAutoSelect

                if (selectedIndex == -1)
                    selectedIndex = 0;

                BaseConfig.MyAnimeLog.Write("Report ItemToAutoSelect: {0}", selectedIndex.ToString());

                #endregion

                ReportFacadeLoadingProgress(type, selectedIndex, list);

                SetFacade();

                #region DelayedImageLoading

                // we only use delayed image loading for the main groups view
                // since the other views will not have enough items to be concerned about


                if (delayedImageLoading && groups != null)
                {
                    BaseConfig.MyAnimeLog.Write("delayedImageLoading: Started");
                    // This is a perfect oportunity to use all cores on the machine
                    // we queue each image up to be loaded, resize and put them into memory in parallel


                    // Set the amount of threads to the amount of CPU cores in the machine.
                    int MaxThreads = Environment.ProcessorCount;
                    // This keeps track of how many of the threads have terminated
                    int done = 0;
                    // Pool of threads.
                    List<Thread> ImageLoadThreadPool = new List<Thread>();
                    // List of Groups in the facade. This is checked by the threads to get groups to load fanart for.
                    List<KeyValuePair<VM_AnimeGroup_User, int>> FacadeGroups = new List<KeyValuePair<VM_AnimeGroup_User, int>>();

                    // Fill the list of groups in order of their proximity to the current selection. This makes the groups currently shown load first, and then further out.
                    FacadeHelper.ProximityForEach(groups, selectedIndex, delegate(VM_AnimeGroup_User grp, int currIndex) { FacadeGroups.Add(new KeyValuePair<VM_AnimeGroup_User, int>(grp, currIndex)); });


                    // Create number of threads based on MaxThreads. MaxThreads should be the amount of CPU cores.
                    for (int i = 0; i < MaxThreads; i++)
                    {
                        // Create a new thread. The function it should run is written here using the delegate word.
                        Thread thread = new Thread(new ThreadStart(delegate
                        {
                            // The number of groups left to load in the facade. Is renewed on each loop of the threads do while loop.
                            int FacadeGroupCount ;

                            do
                            {
                                // create varible to store the group.
                                KeyValuePair<VM_AnimeGroup_User, int> group;

                                // The FacadeGroups list is accessed by all threads, therefor it is in a locked section to make it thread safe.
                                lock (FacadeGroups)
                                {
                                    // Dtore into the facadeGroupCount varible which is in the threads scope.
                                    FacadeGroupCount = FacadeGroups.Count;

                                    // If there are groups left to load, and the facade is not stopping, load a group and remove it from the list.
                                    if (FacadeGroupCount > 0 && !workerFacade.CancellationPending)
                                    {
                                        group = FacadeGroups[0];
                                        FacadeGroups.RemoveAt(0);
                                    }
                                    // Ether their are no more groups or the facade is stopping, so we should exit while marking us as finished.
                                    else
                                    {
                                        Interlocked.Increment(ref done);
                                        return;
                                    }
                                }

                                // If a group was loaded, get it's image, then report that the image is loaded to the facadeworker.
                                // the facade worker (which is another thread itself) will handle putting the image into the facade for us.
                                if (group.Key != null)
                                {
                                    string img = ImageAllocator.GetGroupImage(group.Key, groupViewMode);
                                    ReportFacadeLoadingProgress(BackGroundLoadingArgumentType.DelayedImgLoading, group.Value, img);
                                }
                            }
                            // while there are still groups left to load, repeat loop.
                            while (FacadeGroupCount > 0);
                        }));

                        // Make the thread a lower priority. Everything else should have a higher priority then this background image loading.
                        thread.Priority = ThreadPriority.BelowNormal;

                        // add this thread to the thread pool.
                        ImageLoadThreadPool.Add(thread);
                    }

                    // for each thread in the thread pool, start it.
                    foreach (Thread thread in ImageLoadThreadPool)
                        thread.Start();

                    // Do not continue untill all the image loading threads are finished. Currently we are in the facade background worker thread. The image loading threads call
                    // this thread's ProgressChanged function, which then adds the images that were loaded in to the facade. If we go beyond this point before the image loading
                    // threads finish, then that ProgressChanged function might not exisit any more since this facade background worker thread could have finished already.

                    //while (done < MaxThreads)
                    //    Thread.Sleep(500);

                    BaseConfig.MyAnimeLog.Write("ImageLoad: Finished");
                }

                #endregion

                if (animeSeriesIDToBeRated.HasValue && BaseConfig.Settings.DisplayRatingDialogOnCompletion)
                {
                    VM_AnimeSeries_User ser = (VM_AnimeSeries_User) VM_ShokoServer.Instance.ShokoServices.GetSeries(animeSeriesIDToBeRated.Value,
                        VM_ShokoServer.Instance.CurrentUser.JMMUserID);
                    if (ser != null)
                        Utils.PromptToRateSeriesOnCompletion(ser);

                    animeSeriesIDToBeRated = null;
                }
            }

            catch (Exception e)
            {
                BaseConfig.MyAnimeLog.Write("The 'LoadFacade' function has generated an error: {0}", e.ToString());
            }
        }


        private void SetEpisodeTypeListItem(ref GUIListItem item, VM_AnimeEpisodeType epType)
        {
            string sIconList = GUIGraphicsContext.Skin + @"\Media\MyAnime3\anime3_ListIcon.png";
            //string sUnWatchedFilename = GUIGraphicsContext.Skin + @"\Media\MyAnime3\anime3_UnWatched_left.png";

            try
            {
                item = new GUIListItem(epType.EpisodeTypeDescription);
                item.DVDLabel = epType.EpisodeTypeDescription;
                item.TVTag = epType;
                int unwatched = 0;
                int watched = 0;
                VM_AnimeSeries_User ser = GetTopSerie();
                if (ser != null)
                    ser.GetWatchedUnwatchedCount(epType.EpisodeType, ref unwatched, ref watched);
                item.IsPlayed = unwatched == 0;

                View.eLabelStyleGroups style = View.eLabelStyleGroups.WatchedUnwatched;

                switch (style)
                {
                    case View.eLabelStyleGroups.WatchedUnwatched:

                        string space = " ";

                        item.Label3 = space + watched.ToString().PadLeft(3, '0');
                        item.IconImage = sIconList;
                        item.Label2 = unwatched.ToString().PadLeft(3, '0');
                        break;

                    /*case View.eLabelStyleGroups.Unwatched:
        
                      if (unwatched > 0)
                      {
                        item.IconImage = sUnWatchedFilename;
                        item.Label3 = unwatched.ToString() + " New";
                        item.Label2 = "  ";
                      }
                      else
                      {
                        item.Label2 = "  ";
                        item.Label3 = "  ";
                      }
                      break;
        
                    case View.eLabelStyleGroups.TotalEpisodes:
        
                      int totalEps = unwatched + watched;
        
                      item.IconImage = sUnWatchedFilename;
                      item.Label3 = totalEps.ToString() + " Episodes";
                      item.Label2 = "  ";
        
                      break;*/
                }
            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write("Failed to create episode type item: {0}", ex);
            }
        }

        private void SetSeriesListItem(ref GUIListItem item, VM_AnimeSeries_User ser)
        {
            string sIconList = GUIGraphicsContext.Skin + @"\Media\MyAnime3\anime3_ListIcon.png";
            string sUnWatchedFilename = GUIGraphicsContext.Skin + @"\Media\MyAnime3\anime3_UnWatched_left.png";

            if (seriesViewMode != GUIFacadeControl.Layout.List)
            {
                // Graphical Mode
                item = new GUIListItem();
                //item.IconImage = item.IconImageBig = ImageAllocator.GetSeriesBanner(ser, seriesViewMode);
            }
            else
            {
                item = new GUIListItem(ser.SeriesName);

                View.eLabelStyleGroups style = settings.LabelStyleGroups;

                switch (style)
                {
                    case View.eLabelStyleGroups.WatchedUnwatched:

                        string unwatched = ser.UnwatchedEpisodeCount.ToString();
                        string watched = ser.WatchedEpisodeCount.ToString();
                        string space = " ";

                        item.Label3 = space + watched.PadLeft(3, '0');
                        item.IconImage = sIconList;
                        item.Label2 = unwatched.PadLeft(3, '0');
                        break;

                    case View.eLabelStyleGroups.Unwatched:

                        if (ser.UnwatchedEpisodeCount > 0)
                        {
                            item.IconImage = sUnWatchedFilename;
                            item.Label3 = ser.UnwatchedEpisodeCount + " " + Translation.New;
                            item.Label2 = "  ";
                        }
                        else
                        {
                            item.Label2 = "  ";
                            item.Label3 = "  ";
                        }
                        break;

                    case View.eLabelStyleGroups.TotalEpisodes:

                        int totalEps = ser.UnwatchedEpisodeCount + ser.WatchedEpisodeCount;

                        item.IconImage = sUnWatchedFilename;
                        item.Label3 = totalEps + " " + Translation.Episodes;
                        item.Label2 = "  ";

                        break;
                }
            }
            item.DVDLabel = ser.SeriesName;
            item.TVTag = ser;
            item.IsPlayed = ser.UnwatchedEpisodeCount == 0;

        }

        private void SetGroupFilterListItem(ref GUIListItem item, VM_GroupFilter grpFilter)
        {
            item = new GUIListItem(grpFilter.GroupFilterName);
            item.DVDLabel = grpFilter.GroupFilterName;
            item.TVTag = grpFilter;
            item.IsPlayed = false;

        }

        private void SetGroupListItem(ref GUIListItem item, VM_AnimeGroup_User grp)
        {
            if (groupViewMode != GUIFacadeControl.Layout.List)
            {
                // Graphical Mode
                item = new GUIListItem();
            }
            else
            {
                string sIconList = GUIGraphicsContext.Skin + @"\Media\MyAnime3\anime3_ListIcon.png"; // MyAnime3\anime3_ListIcon
                //string sUnWatchedFilename = GUIGraphicsContext.Skin + @"\Media\MyAnime3\anime3_UnWatched_left.png";
                item = new GUIListItem(grp.GroupName);

                View.eLabelStyleGroups style = View.eLabelStyleGroups.WatchedUnwatched;

                switch (style)
                {
                    case View.eLabelStyleGroups.WatchedUnwatched:

                        // Available (Files are Local) Images
                        string unwatched = grp.UnwatchedEpisodeCount.ToString();
                        string watched = grp.WatchedEpisodeCount.ToString();
                        string space = " ";

                        //item.Label3 = space + watched.ToString().PadLeft(3, '0');
                        item.Label3 = space + watched.PadLeft(3, '0');
                        item.IconImage = sIconList;
                        item.Label2 = unwatched.PadLeft(3, '0');
                        break;


                    /*case View.eLabelStyleGroups.Unwatched:
        
                      if (grp.UnwatchedEpisodeCount > 0)
                      {
                        item.IconImage = sUnWatchedFilename;
                        item.Label3 = grp.UnwatchedEpisodeCount.ToString() + " New";
                        item.Label2 = "  ";
                      }
                      else
                      {
                        item.Label2 = "  ";
                        item.Label3 = "  ";
                      }
                      break;
        
                    case View.eLabelStyleGroups.TotalEpisodes:
        
                      int totalEps = grp.UnwatchedEpisodeCount + grp.WatchedEpisodeCount;
        
                      item.IconImage = sUnWatchedFilename;
                      item.Label3 = totalEps.ToString() + " Eps";
                      item.Label2 = "  ";
        
                      break;*/
                }
            }
            item.DVDLabel = grp.GroupName;
            item.TVTag = grp;
            item.IsPlayed = grp.UnwatchedEpisodeCount == 0;

 
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="ep"></param>
        /// <returns>whether this episode has been watched</returns>
        private bool SetEpisodeListItem(ref GUIListItem item, VM_AnimeEpisode_User ep)
        {
            try
            {
                if (item == null)
                {
                    item = new GUIListItem();
                    item.Label = ep.EpisodeNumberAndName;
                    item.Label2 = " ";
                    item.DVDLabel = ep.EpisodeNumberAndName;
                    item.TVTag = ep;
                }


                // for each anime episode we may actually have one or more files
                // if one of the files has been watched we consider the episode watched

                //View.eLabelStyleEpisodes style = settings.LabelStyleEpisodes;

                // get the AniDB_Episode info for this AnimeEpisode
                /*if (ep.AniDB_EpisodeID.HasValue)
                        {
                            AniDB_Episode aniEp = new AniDB_Episode();
                            if (aniEp.Load(ep.AniDB_EpisodeID.Value))
                            {
                                string space = "";
                                if (style == View.eLabelStyleEpisodes.IconsDate)
                                    item.Label3 = space + Utils.GetAniDBDateWithShortYear(aniEp.AirDate);
                            }
                        }*/


                // get all the LocalFile records for this episode
                bool isWatched = ep.IsWatched();
                item.IsRemote = ep.LocalFileCount == 0;
                item.IsPlayed = isWatched;
                return isWatched;
            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write("Failed to create episode item: {0}", ex);
                return false;
            }
        }
        
        private ContextMenuAction ShowLayoutMenu(string previousMenu)
        {
            ContextMenu cmenu = new ContextMenu(Translation.ChangeLayout, previousMenu);
            cmenu.AddAction(Translation.ListPosters, () => groupViewMode = GUIFacadeControl.Layout.List);
            cmenu.AddAction(Translation.WideBanners, () => groupViewMode = GUIFacadeControl.Layout.LargeIcons);
            cmenu.AddAction(Translation.Filmstrip, () => groupViewMode = GUIFacadeControl.Layout.Filmstrip);
            if (!m_Facade.IsNullLayout(GUIFacadeControl.Layout.CoverFlow))
                cmenu.AddAction(Translation.Coverflow, () => groupViewMode = GUIFacadeControl.Layout.CoverFlow);
            ContextMenuAction context = cmenu.Show();
            if (context == ContextMenuAction.Exit)
            {
                History level = GetCurrent();
                if (level.Listing is VM_GroupFilter && ((VM_GroupFilter) level.Listing).Childs.Count == 0)
                {
                    settings.LastGroupViewMode = groupViewMode;
                    settings.Save();
                }
                LoadFacade();
            }
            return context;
        }


        private void ShowDisplayOptionsMenu(string previousMenu)
        {
            ContextMenu cmenu = new ContextMenu(Translation.DisplayOptions, previousMenu);
            cmenu.Add(Translation.ChangeLayout, () => ShowLayoutMenu(Translation.DisplayOptions));
            cmenu.Show();
        }

        private void ShowFilterOptions(string previousMenu)
        {
            ContextMenu cmenu = new ContextMenu(Translation.FilterOptions, previousMenu);
            string showEps = String.Format(Translation.OnlyShowAvailableEpisodes,
                settings.ShowOnlyAvailableEpisodes ? Translation.On : Translation.Off);
            string hideWatched = String.Format(Translation.HideWatchedEpisodes,
                settings.HideWatchedFiles ? Translation.On : Translation.Off);

            cmenu.AddAction(showEps, () =>
            {
                settings.ShowOnlyAvailableEpisodes = !settings.ShowOnlyAvailableEpisodes;
                LoadFacade();
                settings.Save();
            });
            cmenu.AddAction(hideWatched, () =>
            {
                settings.HideWatchedFiles = !settings.HideWatchedFiles;
                settings.Save();
                LoadFacade();
            });

            cmenu.Show();
        }

        #region Options Menus

        /*
    private bool ShowOptionsMenu(string previousMenu)
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
        return true;
    
      //keep showing the dialog until the user closes it
      int selectedLabel = 0;
      string currentMenu = "Options";
      while (true)
      {
        dlg.Reset();
        dlg.SetHeading(currentMenu);
    
        if (previousMenu != string.Empty)
          dlg.Add("<<< " + previousMenu);
        dlg.Add("AniDB >>>");
        dlg.Add("Display >>>");
    
        dlg.SelectedLabel = selectedLabel;
        dlg.DoModal(GUIWindowManager.ActiveWindow);
        selectedLabel = dlg.SelectedLabel;
    
        int selection = selectedLabel + ((previousMenu == string.Empty) ? 1 : 0);
        switch (selection)
        {
          case 0:
            //show previous
            return true;
          case 1:
            //if (!ShowOptionsAniDBMenu(currentMenu))
            //	return false;
            break;
          case 2:
            if (!ShowOptionsDisplayMenu(currentMenu))
              return false;
            break;
          default:
            //close menu
            return false;
        }
      }
    }
        */

        private void  ShowOptionsDisplayMenu(string previousMenu)
        {
            string findFilter = String.Format(Translation.FindOnlyShowMatches,
                settings.FindFilter ? Translation.On : Translation.Off);
            ContextMenu cmenu = new ContextMenu(Translation.DisplayOptions, previousmenu: previousMenu);
            string askBeforeStreaming = String.Format(Translation.AskBeforeStartStreamingPlaybackDialogText,
                settings.AskBeforeStartStreamingPlayback ? Translation.On : Translation.Off);

            cmenu.AddAction(findFilter, () =>
            {
                settings.FindFilter = !settings.FindFilter;
                if (searchTimer.Enabled)
                {
                    SaveOrRestoreFacadeItems(false);
                    DoSearch(m_Facade.SelectedListItemIndex);
                }
                settings.Save();
            });

            cmenu.AddAction(askBeforeStreaming, () =>
            {
                settings.AskBeforeStartStreamingPlayback = !settings.AskBeforeStartStreamingPlayback;
                settings.Save();
            });

            cmenu.Show();
        }

        #endregion

        /*
        private void ChangeView(View v)
        {
            BaseConfig.MyAnimeLog.Write(string.Format("ChangeView: {0} - {1}", currentViewClassification,
                v == null ? "" : v.Name));
            currentViewClassification = ViewClassification.Views;
            currentView = new View(v);
            currentStaticViewID = "";

            settings.LastView = currentView;
            settings.LastViewClassification = currentViewClassification;
            settings.LastStaticViewID = currentStaticViewID;
            settings.Save();

            //update skin
            SetGUIProperty(GuiProperty.SimpleCurrentView, v.DisplayName);
        }

        private void ChangeStaticView(ViewClassification vClass, string id)
        {
            BaseConfig.MyAnimeLog.Write(string.Format("ChangeStaticView: {0} - {1}", vClass, id));
            currentViewClassification = vClass;
            currentStaticViewID = id;

            settings.LastViewClassification = currentViewClassification;
            settings.LastStaticViewID = currentStaticViewID;
            settings.Save();

            //update skin
            SetGUIProperty(GuiProperty.SimpleCurrentView, currentStaticViewID);
        }
        */
        private void SetFacade()
        {
            bool filmstrip = false;
            bool widebanners = false;
       
            bool listmode = false;
            bool groups = false;
            bool series = false;
            bool episodes = false;
            bool episodetypes = false;
            bool groupfilters = false;


            if (groupViewMode == GUIFacadeControl.Layout.List)
                listmode = true;
            else if (groupViewMode == GUIFacadeControl.Layout.Filmstrip)
                filmstrip = true;
           /* else if (groupViewMode == GUIFacadeControl.Layout.CoverFlow)
                coverflow = true;*/
            else
                widebanners = true;

            History level = GetCurrent();
            if (level.Listing == null || level.Listing is VM_GroupFilter && ((VM_GroupFilter) level.Listing).Childs.Count > 0)
            {
                m_Facade.CurrentLayout = GUIFacadeControl.Layout.List;
                listmode = true;
                widebanners = filmstrip = false;
                groupfilters = true;
                BaseConfig.MyAnimeLog.Write("SetFacade List Mode: GroupFilter}");
            }
            else if (level.Listing is VM_GroupFilter)
            {
                m_Facade.CurrentLayout = groupViewMode;
                groups = true;
                BaseConfig.MyAnimeLog.Write("SetFacade List Mode: Group}");
                //BaseConfig.MyAnimeLog.Write("List Mode: {0}, IsGroups: {1}", this.dummyLayoutListMode.Visible, dummyIsGroups.Visible);
            }
            else if (level.Listing is VM_AnimeGroup_User)
            {
                m_Facade.CurrentLayout = GUIFacadeControl.Layout.List;
                listmode = true;
                widebanners = filmstrip = false;
                series = true;
                BaseConfig.MyAnimeLog.Write("SetFacade List Mode: Series}");

                //BaseConfig.MyAnimeLog.Write("List Mode: {0}, IsSeries: {1}", this.dummyLayoutListMode.Visible, dummyIsSeries.Visible);
            }

            else if (level.Listing is VM_AnimeSeries_User && ((VM_AnimeSeries_User) level.Listing).EpisodeTypes.Count > 1)
            {
                //m_Facade.CurrentLayout = seriesViewMode;
                m_Facade.CurrentLayout = GUIFacadeControl.Layout.List;
                listmode = true;
                widebanners = filmstrip = false;
                episodetypes = true;
                BaseConfig.MyAnimeLog.Write("SetFacade List Mode: Episodes Types}");

                //BaseConfig.MyAnimeLog.Write("List Mode: {0}, IsEpisodeTypes: {1}", this.dummyLayoutListMode.Visible, dummyIsEpisodeTypes.Visible);
            }

            else if (level.Listing is VM_AnimeSeries_User || level.Listing is VM_AnimeEpisodeType)
            {
                m_Facade.CurrentLayout = episodesViewMode; // always list
                listmode = true;
                widebanners = filmstrip = false;
                episodes = true;
                BaseConfig.MyAnimeLog.Write("SetFacade List Mode: Episodes}");

                //BaseConfig.MyAnimeLog.Write("List Mode: {0}, dummyIsEpisodes: {1}", this.dummyLayoutListMode.Visible, dummyIsEpisodes.Visible);
            }


            //if (this.dummyLayoutFilmstripMode != null) BaseConfig.MyAnimeLog.Write("SetFacade: dummyLayoutFilmstripMode.Visible : {0}", this.dummyLayoutFilmstripMode.Visible);
            //if (this.dummyLayoutWideBanners != null) BaseConfig.MyAnimeLog.Write("SetFacade: dummyLayoutWideBanners.Visible : {0}", this.dummyLayoutWideBanners.Visible);
            //if (this.dummyLayoutListMode != null) BaseConfig.MyAnimeLog.Write("SetFacade: dummyLayoutListMode.Visible : {0}", this.dummyLayoutListMode.Visible);


            EvaluateVisibility();

            if (dummyLayoutFilmstripMode != null) dummyLayoutFilmstripMode.Visible = filmstrip;
            if (dummyLayoutWideBanners != null) dummyLayoutWideBanners.Visible = widebanners;
            if (dummyLayoutListMode != null) dummyLayoutListMode.Visible = listmode;
            if (dummyIsGroups != null) dummyIsGroups.Visible = groups;
            if (dummyIsGroupFilters != null) dummyIsGroupFilters.Visible = groupfilters;
            if (dummyIsSeries != null) dummyIsSeries.Visible = series;
            if (dummyIsEpisodeTypes != null) dummyIsEpisodeTypes.Visible = episodetypes;
            if (dummyIsEpisodes != null) dummyIsEpisodes.Visible = episodes;

            BaseConfig.MyAnimeLog.Write("SetFacade: Filters: {0} - Groups: {1} - Series: {2} - Episodes: {3}", groupfilters, groups, series, episodes);

            // fix for skin visiblity problem during video playback.
            if (GUIGraphicsContext.IsPlayingVideo)
                GUIWindowManager.Render(0);

            Application.DoEvents();
        }

        protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
        {
            if (actionType == Action.ActionType.ACTION_MOUSE_DOUBLECLICK)
            {
                OnShowContextMenu();
                return;
            }

            if (actionType == Action.ActionType.ACTION_PLAY)
                if (g_Player.Playing == false)
                    BaseConfig.MyAnimeLog.Write("Pressed the play button");
            MainMenu menu = new MainMenu();
            menu.Add(btnDisplayOptions, () =>
            {
                m_Facade.Focus = true;
                ShowDisplayOptionsMenu(string.Empty);
            });
            menu.Add(btnWindowUtilities, () =>
            {
                SetGlobalIDs();
                GUIWindowManager.ActivateWindow(Constants.WindowIDs.ADMIN);
            });
            menu.Add(btnWindowCalendar, () =>
            {
                SetGlobalIDs();
                GUIWindowManager.ActivateWindow(Constants.WindowIDs.CALENDAR);
            });
            menu.Add(btnWindowContinueWatching, () =>
            {
                SetGlobalIDs();
                GUIWindowManager.ActivateWindow(Constants.WindowIDs.WATCHING);
            });
            menu.Add(btnWindowRecommendations, () =>
            {
                SetGlobalIDs();
                GUIWindowManager.ActivateWindow(Constants.WindowIDs.RECOMMENDATIONS);
            });
            menu.Add(btnWindowRandom, () =>
            {
                /*
                GroupFilterVM grpFilter = new GroupFilterVM();
                grpFilter.GroupFilterName = "All";
                grpFilter.GroupFilterID = 16;
                grpFilter.FilterType = 4;
                RandomWindow_LevelObject = grpFilter;
                RandomWindow_RandomLevel = RandomSeriesEpisodeLevel.GroupFilter;
                RandomWindow_RandomType = RandomObjectType.Series;
                GUIWindowManager.ActivateWindow(Constants.WindowIDs.RANDOM);*/
                ShowContextMenuGroupFilter("");
            });
            menu.Add(btnChangeLayout, () =>
            {
                m_Facade.Focus = true;
                ShowLayoutMenu(string.Empty);
            });
            menu.Add(btnFilters, () =>
            {
                m_Facade.Focus = true;
                ShowFilterOptions(string.Empty);
            });
            menu.Add(btnSwitchUser, () =>
            {
                if (VM_ShokoServer.Instance.PromptUserLogin())
                {
                    Breadcrumbs = new List<History> {new History()};

                    // user has logged in, so save to settings so we will log in as the same user next time
                    settings.CurrentJMMUserID =
                        VM_ShokoServer.Instance.CurrentUser.JMMUserID.ToString(CultureInfo.InvariantCulture);
                    settings.Save();

                    LoadFacade();
                }
            });
            menu.Add(btnSettings, () =>
            {
                m_Facade.Focus = true;
                ShowOptionsDisplayMenu(string.Empty);
            });
            if (menu.Check(control))
                return;

            try
            {
                if (actionType != Action.ActionType.ACTION_SELECT_ITEM) return; // some other events raised onClicked too for some reason?
                if (control == m_Facade)
                {
                    UpdateSearchPanel(false);

                    if (m_Facade.SelectedListItem == null || m_Facade.SelectedListItem.TVTag == null)
                        return;
                    IVM cur = m_Facade.SelectedListItem.TVTag as IVM;
                    if (cur == null)
                        return;
                    if (cur is VM_AnimeGroup_User)
                        cur = GetChildrenLevelForGroup((VM_AnimeGroup_User) cur);
                    if (cur is VM_AnimeSeries_User)
                        cur = GetChildrenLevelForSeries((VM_AnimeSeries_User) cur);
                    if (cur is VM_AnimeEpisode_User)
                    {
                        VM_AnimeEpisode_User ep = (VM_AnimeEpisode_User) cur;
                        BaseConfig.MyAnimeLog.Write("Selected to play: {0}", ep.EpisodeNumberAndName);
                        vidHandler.ResumeOrPlay(ep);
                    }
                    else
                    {
                        BaseConfig.MyAnimeLog.Write("Clicked to " + cur.GetType());
                        Breadcrumbs.Add(new History {Listing = cur});
                        LoadFacade();
                        m_Facade.Focus = true;
                    }
                }
            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write("Error in OnClicked: {0} - {1}", ex.Message, ex.ToString());
            }

            base.OnClicked(controlId, control, actionType);
        }


        private IVM GetChildrenLevelForSeries(VM_AnimeSeries_User vm)
        {
            IVM v;
            List<VM_AnimeEpisodeType> episodeTypes = vm.EpisodeTypesToDisplay;
            if (episodeTypes.Count == 1)
            {
                SetGUIProperty(GuiProperty.SeriesTitle, vm.SeriesName);
                // only one so lets go straight to the episodes
                v = episodeTypes[0];
                SetFanartForEpisodes();
                return v;
            }
            return vm;
        }

        private IVM GetChildrenLevelForGroup(VM_AnimeGroup_User vm)
        {
            List<VM_AnimeGroup_User> subGroups = vm.SubGroups;
            List<VM_AnimeSeries_User> seriesList = vm.ChildSeries;

            int subLevelCount = seriesList.Count + subGroups.Count;

            if (subLevelCount > 1 || subLevelCount == 0)
                return vm;
            // keep drilling down until we find a series
            // or more than one sub level
            while (subLevelCount == 1 && subGroups.Count > 0)
            {
                vm = subGroups[0];
                subGroups = vm.SubGroups;
                seriesList = vm.ChildSeries;
                subLevelCount = seriesList.Count + subGroups.Count;
            }

            if (subGroups.Count == 0 && seriesList.Count == 1)
                return GetChildrenLevelForSeries(seriesList[0]);
            return vm;
        }

        public void SetFanartForEpisodes()
        {
            // do this so that after an episode is played and the page is reloaded, we will always show the correct fanart
            VM_AnimeSeries_User aser = GetTopSerie();
            if (aser == null) return;

            LoadFanart(aser);
        }

        public override void DeInit()
        {
            BaseConfig.MyAnimeLog.Write("DeInit");

            base.DeInit();
        }

        public static void ReturnToMPHome()
        {
            // Set home window message for use later
            var msgHome = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GOTO_WINDOW, 0, 0, 0,
                (int) Window.WINDOW_HOME, 00432100, null);

            if (BaseConfig.Settings.BasicHome)
                msgHome = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GOTO_WINDOW, 0, 0, 0,
                    (int) Window.WINDOW_SECOND_HOME, 00432100, null);

            GUIWindowManager.SendThreadMessage(msgHome);
        }

        public override void OnAction(Action action)
        {
            try
            {
                //if (action != null && action.m_key != null)
                //{
                    //BaseConfig.MyAnimeLog.Write("Received action: {0} | keychar: {1}", action.wID, (char) (action.m_key.KeyChar));
                //}
                //else if (action != null)
                //{
                    //BaseConfig.MyAnimeLog.Write("Received action: {0}", action.wID);
                //}

                if (GUIWindowManager.ActiveWindowEx != GetID)
                    return;
                if (action == null)
                    return;
                switch (action.wID)
                {
                    case Action.ActionType.ACTION_SWITCH_HOME:
                        string keyChar = "";
                        if (action.m_key != null)
                            keyChar = KeycodeToString(action.m_key.KeyChar);

                        if (keyChar.ToLower() != "h" && !IsFilterActive())
                            ReturnToMPHome();
                        break;
                    case Action.ActionType.ACTION_MOVE_DOWN:
                    case Action.ActionType.ACTION_MOVE_UP:

                        //Reset autoclose timer on search
                        if (searchTimer != null && searchTimer.Enabled)
                        {
                            searchTimer.Stop();
                            searchTimer.Start();
                        }

                        base.OnAction(action);
                        break;
                    case Action.ActionType.ACTION_MOVE_LEFT:
                    case Action.ActionType.ACTION_MOVE_RIGHT:

                        base.OnAction(action);
                        break;
                    case Action.ActionType.ACTION_REMOTE_RED_BUTTON:
                        OnSearchAction(SearchAction.ToggleMode);
                        return;

                    case Action.ActionType.ACTION_REMOTE_BLUE_BUTTON:
                        OnSearchAction(SearchAction.ToggleStartWord);
                        return;

                    case Action.ActionType.ACTION_KEY_PRESSED:
                        KeyCommandHandler(action.m_key.KeyChar);
                        return;

                    case Action.ActionType.ACTION_PARENT_DIR:
                        return;

                    case Action.ActionType.ACTION_HOME:
                        // BaseConfig.MyAnimeLog.Write("Basic home navgiation = " + BaseConfig.Settings.HomeButtonNavigation);

                        if (BaseConfig.Settings.HomeButtonNavigation)
                        {
                            UpdateSearchPanel(false);
                            ImageAllocator.FlushAll();
                            GUIWindowManager.ShowPreviousWindow();
                        }
                        else
                        {
                            ReturnToMPHome();
                        }
                        break;

                    case Action.ActionType.ACTION_PLAY:
                        BaseConfig.MyAnimeLog.Write("Received PLAY action");

                        try
                        {
                            IVM ivm = GetCurrent().Selected;
                            if (ivm is VM_AnimeGroup_User)
                            {
                                VM_AnimeGroup_User grp = (VM_AnimeGroup_User) ivm;
                                VM_AnimeEpisode_User ep = (VM_AnimeEpisode_User) VM_ShokoServer.Instance.ShokoServices.GetNextUnwatchedEpisodeForGroup(grp.AnimeGroupID,
                                    VM_ShokoServer.Instance.CurrentUser.JMMUserID);
                                if (ep == null) return;
                                vidHandler.ResumeOrPlay(ep);
                            }

                            if (ivm is VM_AnimeSeries_User)
                            {
                                VM_AnimeSeries_User ser = (VM_AnimeSeries_User) ivm;
                                //ser = null;
                                if (ser.AnimeSeriesID == 0) return;
                                VM_AnimeEpisode_User ep = (VM_AnimeEpisode_User) VM_ShokoServer.Instance.ShokoServices.GetNextUnwatchedEpisode(ser.AnimeSeriesID, VM_ShokoServer.Instance.CurrentUser.JMMUserID);
                                if (ep == null) return;
                                vidHandler.ResumeOrPlay(ep);
                            }
                        }
                        catch (Exception ex)
                        {
                            BaseConfig.MyAnimeLog.Write(ex.ToString());
                        }
                        break;

                    case Action.ActionType.ACTION_PREVIOUS_MENU:
                        if (searchTimer != null && searchTimer.Enabled)
                        {
                            OnSearchAction(SearchAction.EndSearch);
                            return;
                        }

                        // back one level

                        //string msg = string.Format("LIST LEVEL:: {0} - GF: {1} ", listLevel, selectedGroupFilter?.GroupFilterName ?? "None");

                        //					BaseConfig.MyAnimeLog.Write(msg);

                        if (Breadcrumbs.Count == 1)
                        {
                            BaseConfig.MyAnimeLog.Write("Going HOME");
                            goto case Action.ActionType.ACTION_HOME;
                        }
                        Breadcrumbs.Remove(Breadcrumbs[Breadcrumbs.Count - 1]);
                        LoadFacade();
                        break;

                    default:
                        base.OnAction(action);
                        break;
                }
            }
            catch (Exception ex)
            {
                // On error we just let the action passthru
                BaseConfig.MyAnimeLog.Write("Error occured in OnAction(): " + ex);
                base.OnAction(action);
            }
        }

        bool IsFilterActive()
        {
            bool isFilterActive = false;
            if (dummyFindActive != null)
                if (dummyFindActive.Visible)
                    isFilterActive = true;

            return isFilterActive;
        }

        void GUIWindowManager_OnThreadMessageHandler(object sender, GUIMessage message)
        {
            if (GUIWindowManager.ActiveWindowEx != GetID)
                return;
            /*
          BaseConfig.MyAnimeLog.Write("Message = " + message.Message.ToString());
          BaseConfig.MyAnimeLog.Write("Message param1 = " + message.Param1.ToString());
          BaseConfig.MyAnimeLog.Write("Message param2 = " + message.Param2.ToString());
          BaseConfig.MyAnimeLog.Write("Message param3 = " + message.Param3.ToString());
          BaseConfig.MyAnimeLog.Write("Message param4 = " + message.Param4.ToString());
          BaseConfig.MyAnimeLog.Write("SendToTargetWindow = " + message.SendToTargetWindow.ToString());
          */
            // Check for custom param 2 and let message thru if found
            if (message.Param2 == 00432100 && !IsFilterActive())
                return;

            // Prevent certain messages from beeing sent to MP core
            if (message.Message == GUIMessage.MessageType.GUI_MSG_GOTO_WINDOW &&
                message.TargetWindowId == 0 && message.TargetControlId == 0 && message.SenderControlId == 0 &&
                message.SendToTargetWindow == false && message.Object == null && message.Object2 == null &&
                message.Param2 == 0 && message.Param3 == 0 && message.Param4 == 0 &&
                (message.Param1 == (int) Window.WINDOW_HOME || message.Param1 == (int) Window.WINDOW_SECOND_HOME))
            {
                message.SendToTargetWindow = true;
                message.TargetWindowId = GetID;
                message.Param1 = GetID;
                message.Message = GUIMessage.MessageType.GUI_MSG_HIDE_MESSAGE;
            }
        }

        #region Find

        #region Keyboard event handling

        private
            const string t9Chars = "0123456789";
        /*
        bool IsChar(Keys k, ref char c)
        {
            string str = new KeysConverter().ConvertToString(k);
            if (string.IsNullOrEmpty(str))
                return false;
            if (str.Length != 1)
                return false;
            c = str[0];
            return true;
        }
        */
        bool IsSearchChar(char c)
        {
            if (search == null)
                return false;

            if (search.Mode == SearchMode.t9)
                return t9Chars.IndexOf(c) > 0 || c == '*' || c == '#';

            if (search.Mode == SearchMode.text)
                return char.IsLetterOrDigit(c) || char.IsPunctuation(c) || c == ' ';

            return false;
        }

        public string KeycodeToString(int keyCode)
        {
            return ((char) keyCode).ToString();
        }

        private void KeyCommandHandler(int keycodeInput)
        {
            // For some reason keycode [ and ] aren't lining up to their WinForm keycode counterpart so we have this workaround first
            char keycode = (char) keycodeInput;
            string keycodeString = KeycodeToString(keycodeInput).ToLower();
            //BaseConfig.MyAnimeLog.Write("KeyCommandHandler | keycode string: " + keycodeString);
            //BaseConfig.MyAnimeLog.Write("KeyCommandHandler | Mode toggle key: " + BaseConfig.Settings.ModeToggleKey);

            // Skip key command processing if video window is fullscreen
            if (g_Player.FullScreen)
                return;

            // Delay stopwatch for certain events
            if (keyCommandDelayTimer.IsRunning && keyCommandDelayTimer.ElapsedMilliseconds < 5000)
                return;

            keyCommandDelayTimer.Stop();
            //when the list is selected, search the input
            if (m_Facade.CurrentLayout == GUIFacadeControl.Layout.List && m_Facade.ListLayout.IsFocused
                || m_Facade.CurrentLayout == GUIFacadeControl.Layout.LargeIcons && m_Facade.ThumbnailLayout.IsFocused
                || m_Facade.CurrentLayout == GUIFacadeControl.Layout.Filmstrip && m_Facade.FilmstripLayout.IsFocused
                || m_Facade.CurrentLayout == GUIFacadeControl.Layout.CoverFlow && m_Facade.CoverFlowLayout.IsFocused)
            {
                if (keycodeString == BaseConfig.Settings.ModeToggleKey)
                {
                    OnSearchAction(SearchAction.ToggleMode);
                    return;
                }
                if (keycodeString == BaseConfig.Settings.StartTextToggleKey)
                {
                    OnSearchAction(SearchAction.ToggleStartWord);
                    return;
                }
                if (keycodeString == "x")
                    if (g_Player.Playing)
                        return;

                // Normal keycode matching for everything else
                switch (keycode)
                {
                    case (char) Keys.Back:
                        OnSearchAction(SearchAction.DeleteChar);
                        break;
                    case (char) Keys.Tab:
                        OnSearchAction(SearchAction.NextMatch);
                        break;
                    default:
                        if (!OnSearchChar(keycode))
                            return;
                        break;
                }
            }
            if (!string.IsNullOrEmpty(searchSound) && !g_Player.Playing)
                MediaPortal.Util.Utils.PlaySound(searchSound, false, true);
        }

        #endregion

        string searchText = string.Empty;
        string searchMatch = string.Empty;

        enum SearchAction
        {
            ToggleMode,
            ToggleStartWord,
            NextMatch,
            DeleteChar,
            EndSearch
        }

        private void OnSearchAction(SearchAction action)
        {
            //stop timer
            if (searchTimer != null && searchTimer.Enabled)
                searchTimer.Stop();

            //process action
            switch (action)
            {
                case SearchAction.ToggleMode:
                    search.Input = string.Empty;
                    searchText = string.Empty;
                    searchMatch = string.Empty;
                    search.Mode = search.Mode == SearchMode.text ? SearchMode.t9 : SearchMode.text;
                    settings.FindMode = search.Mode;
                    settings.Save();
                    break;
                case SearchAction.ToggleStartWord:
                    search.StartWord = !search.StartWord;
                    settings.FindStartWord = search.StartWord;
                    settings.Save();
                    break;
                case SearchAction.NextMatch:
                    DoSearch(m_Facade.SelectedListItemIndex + 1);
                    break;
                case SearchAction.DeleteChar:
                    if (search.Input.Length > 0)
                    {
                        if (settings.FindFilter)
                            SaveOrRestoreFacadeItems(false);

                        search.Input = search.Input.Remove(search.Input.Length - 1);
                        DoSearch(m_Facade.SelectedListItemIndex);
                    }
                    break;
                case SearchAction.EndSearch:
                    UpdateSearchPanel(false);
                    return;
            }

            //update screen
            UpdateSearchPanel(true);
        }

        private void SaveOrRestoreFacadeItems(bool save)
        {
            if (save)
            {
                //save
                lstFacadeItems = new List<GUIListItem>(m_Facade.Count);
                for (int item = 0; item < m_Facade.Count; item++)
                    lstFacadeItems.Add(m_Facade[item]);
            }
            else if (lstFacadeItems != null)
            {
                //restore
                GUIListItem selectedItem = m_Facade.SelectedListItem;
                m_Facade.Clear();
                for (int item = 0; item < lstFacadeItems.Count; item++)
                {
                    m_Facade.Add(lstFacadeItems[item]);
                    if (lstFacadeItems[item] == selectedItem)
                        m_Facade.SelectedListItemIndex = m_Facade.Count - 1;
                }
                lstFacadeItems = null;
            }
        }

        private bool OnSearchChar(char c)
        {
            if (!IsSearchChar(c))
                return false;

            if (search.Mode == SearchMode.t9)
            {
                if (c == '*' || c == '#')
                {
                    if (string.IsNullOrEmpty(search.Input))
                    {
                        //if panel visible, process the special char
                        // otherwise just show the find panel
                        if (dummyFindActive == null || dummyFindActive.Visible)
                        {
                            if (c == '*')
                                OnSearchAction(SearchAction.ToggleStartWord);
                            //If we switched to text mode by accident we'd have to 
                            // get a full keyboard to get back to T9 mode ...
                            // So switching mode with # is disabled
                            //else if (c == '#')
                            //    OnSearchAction(SearchAction.ToggleMode);
                        }
                        else
                        {
                            UpdateSearchPanel(true);
                        }
                    }
                    else
                    {
                        if (c == '*')
                            OnSearchAction(SearchAction.DeleteChar);
                        else if (c == '#')
                            OnSearchAction(SearchAction.NextMatch);
                    }
                    return true;
                }
                if (t9Chars.IndexOf(c) >= 0)
                {
                    AddSearchChar(c);
                    return true;
                }
            }
            else if (search.Mode == SearchMode.text)
            {
                if (char.IsLetterOrDigit(c) || char.IsPunctuation(c) || c == ' ')
                {
                    AddSearchChar(c);
                    return true;
                }
            }

            return false;
        }

        private void AddSearchChar(char c)
        {
            //stop timer
            if (searchTimer != null && searchTimer.Enabled)
                searchTimer.Stop();

            //add char
            if (search.Mode == SearchMode.t9)
            {
                int n = c - '0';
                if (n >= 0 && n <= 9)
                {
                    //add number to sequence
                    search.Input = search.Input + c;
                    searchText = searchText + "?";
                }
            }
            else if (search.Mode == SearchMode.text)
            {
                //add char to sequence
                search.Input = search.Input + c;
                searchText = searchText + "?";
            }

            //search
            DoSearch(m_Facade.SelectedListItemIndex);

            //update screen
            UpdateSearchPanel(true);
        }

        private void DoSearch(int start)
        {
            SearchCollection.SearchMatch match = null;

            //search
            if (settings.FindFilter)
            {
                List<int> lstMatches = new List<int>();
                if (search.GetMatches(start, lstMatches, ref match))
                {
                    //copy current list
                    if (lstFacadeItems == null)
                        SaveOrRestoreFacadeItems(true);

                    //find all matching items
                    List<GUIListItem> lstMatchingItems = new List<GUIListItem>(lstMatches.Count);
                    int selectedItem = -1;
                    for (int index = 0; index < m_Facade.Count; index++)
                        if (lstMatches.Contains(index))
                        {
                            lstMatchingItems.Add(m_Facade[index]);
                            if (index == match.Index)
                                selectedItem = lstMatchingItems.Count - 1;
                        }

                    //update the list
                    m_Facade.Clear();
                    for (int index = 0; index < lstMatchingItems.Count; index++)
                        m_Facade.Add(lstMatchingItems[index]);
                    m_Facade.SelectedListItemIndex = selectedItem;
                }
            }
            else
            {
                match = search.GetMatch(start);

                //select match
                SelectItem(match != null ? match.Index : -1);
            }

            //update sceen
            if (match != null)
            {
                //match found
                searchText = match.Text;
                searchMatch = m_Facade.SelectedListItem.Label;
            }
            else
            {
                //no match found
                searchText = new string('*', search.Input.Length);
                searchMatch = string.Empty;
            }
        }

        void searchTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            UpdateSearchPanel(false);
        }

        private void UpdateSearchPanel(bool bShow)
        {
            if (searchTimer == null) return;

            if (searchTimer.Enabled)
                searchTimer.Stop();

            if (dummyFindActive != null)
                dummyFindActive.Visible = bShow;
            if (dummyFindModeT9 != null)
                dummyFindModeT9.Visible = search.Mode == SearchMode.t9;
            if (dummyFindModeText != null)
                dummyFindModeText.Visible = search.Mode == SearchMode.text;

            if (!bShow)
            {
                search.Input = string.Empty;
                searchText = string.Empty;
                searchMatch = string.Empty;

                if (settings.FindFilter)
                    SaveOrRestoreFacadeItems(false);
            }

            SetGUIProperty(GuiProperty.FindInput, search.Input);
            SetGUIProperty(GuiProperty.FindText, searchText);
            SetGUIProperty(GuiProperty.FindMatch, searchMatch);
            string searchMode = search.Mode == SearchMode.t9 ? Translation.T9 : Translation.Text;
            string startWord = search.StartWord ? Translation.Yes : Translation.No;
            SetGUIProperty(GuiProperty.ModeToggleKey, BaseConfig.Settings.ModeToggleKey);
            SetGUIProperty(GuiProperty.StartTextToggle, BaseConfig.Settings.StartTextToggleKey);


            SetGUIProperty(GuiProperty.FindMode, searchMode);
            SetGUIProperty(GuiProperty.FindStartWord, startWord);
            if (search.Mode == SearchMode.t9)
                if (string.IsNullOrEmpty(searchText))
                {
                    SetGUIProperty(GuiProperty.FindSharpMode, Translation.NextMatch);
                    SetGUIProperty(GuiProperty.FindAsteriskMode, String.Format(Translation.StartWord, startWord));
                }
                else
                {
                    SetGUIProperty(GuiProperty.FindSharpMode, Translation.NextMatch);
                    SetGUIProperty(GuiProperty.FindAsteriskMode, Translation.Clear);
                }

            if (searchTimer != null && bShow)
                searchTimer.Start();
        }

        #endregion

        // triggered when a selection change was made on the facade
        private void onFacadeItemSelected(GUIListItem item, GUIControl parent)
        {
            // if this is not a message from the facade, exit
            if (parent != m_Facade && parent != m_Facade.FilmstripLayout && parent != m_Facade.CoverFlowLayout &&
                parent != m_Facade.ThumbnailLayout && parent != m_Facade.ListLayout)
                return;
            if (m_Facade.SelectedListItem.TVTag == null) return;

            if (m_Facade.SelectedListItem.TVTag.GetType() == typeof(VM_GroupFilter))
                GroupFilter_OnItemSelected(m_Facade.SelectedListItem);

            if (m_Facade.SelectedListItem.TVTag.GetType() == typeof(VM_AnimeGroup_User))
                Group_OnItemSelected(m_Facade.SelectedListItem);

            if (m_Facade.SelectedListItem.TVTag.GetType() == typeof(VM_AnimeSeries_User))
                Series_OnItemSelected(m_Facade.SelectedListItem);

            if (m_Facade.SelectedListItem.TVTag.GetType() == typeof(VM_AnimeEpisodeType))
                EpisodeType_OnItemSelected(m_Facade.SelectedListItem);

            if (m_Facade.SelectedListItem.TVTag.GetType() == typeof(VM_AnimeEpisode_User))
                Episode_OnItemSelected(m_Facade.SelectedListItem);

            EvaluateVisibility();
        }

        public override bool OnMessage(GUIMessage message)
        {
            try
            {
                switch (message.Message)
                {
                    case GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS_CHANGED:
                    {
                        int iControl = message.SenderControlId;
                        if (iControl == m_Facade.GetID)
                            if (m_Facade.SelectedListItem != null && m_Facade.SelectedListItem.TVTag != null)
                            {
                                if (m_Facade.SelectedListItem.TVTag.GetType() == typeof(VM_GroupFilter))
                                    GroupFilter_OnItemSelected(m_Facade.SelectedListItem);

                                if (m_Facade.SelectedListItem.TVTag.GetType() == typeof(VM_AnimeGroup_User))
                                    Group_OnItemSelected(m_Facade.SelectedListItem);

                                if (m_Facade.SelectedListItem.TVTag.GetType() == typeof(VM_AnimeSeries_User))
                                    Series_OnItemSelected(m_Facade.SelectedListItem);

                                if (m_Facade.SelectedListItem.TVTag.GetType() == typeof(VM_AnimeEpisodeType))
                                    EpisodeType_OnItemSelected(m_Facade.SelectedListItem);

                                if (m_Facade.SelectedListItem.TVTag.GetType() == typeof(VM_AnimeEpisode_User))
                                    Episode_OnItemSelected(m_Facade.SelectedListItem);
                            }
                    }

                        EvaluateVisibility();
                        return true;

                    case GUIMessage.MessageType.GUI_MSG_PLAYBACK_ENDED:
                    case GUIMessage.MessageType.GUI_MSG_PLAYBACK_STOPPED:
                    {
                        //-- Need to reload the GUI to display changes 
                        //-- if episode is classified as watched
                        LoadFacade();
                    }
                        return true;

                    default:
                        return base.OnMessage(message);
                }
            }
            catch (Exception ex)
            {
                // On error we just let the message passthru
                BaseConfig.MyAnimeLog.Write("Error occured in OnMessage(): " + ex.Message);
                return base.OnMessage(message);
            }
        }

        void g_Player_PlayBackEnded(g_Player.MediaType type, string filename)
        {
            // Not used at this time
        }

        public static bool isFirstInitDone;

        private void OnFirstStart()
        {
            if (isFirstInitDone)
                return;
            fanartTexture.Filename = GUIGraphicsContext.Skin + @"\Media\hover_my anime3.jpg";
            Breadcrumbs = new List<History> {new History()};
            if (string.IsNullOrEmpty(settings.JMMServer_Address) ||
                string.IsNullOrEmpty(settings.JMMServer_Port))
            {
                Utils.DialogMsg(Translation.Error, Translation.PleaseExitMPFirst);
                return;
            }


            // check if we can connect to Shoko Server
            // and load the default user
            List<JMMUser> allUsers = ShokoServerHelper.GetAllUsers();
            if (allUsers.Count == 0)
            {
                Utils.DialogMsg(Translation.Error, Translation.ErrorConnectingJMMServer);
                return;
            }
            // check for last jmm user
            if (string.IsNullOrEmpty(settings.CurrentJMMUserID))
            {
                if (!VM_ShokoServer.Instance.PromptUserLogin())
                    return;

                // user has logged in, so save to settings so we will log in as the same user next time
                settings.CurrentJMMUserID = VM_ShokoServer.Instance.CurrentUser.JMMUserID.ToString();
                settings.Save();
            }
            else
            {
                JMMUser selUser = null;
                foreach (JMMUser user in allUsers)
                    if (user.JMMUserID.ToString().Equals(settings.CurrentJMMUserID))
                    {
                        selUser = user;
                        break;
                    }

                if (selUser == null)
                {
                    if (!VM_ShokoServer.Instance.PromptUserLogin())
                        return;

                    // user has logged in, so save to settings so we will log in as the same user next time
                    settings.CurrentJMMUserID = VM_ShokoServer.Instance.CurrentUser.JMMUserID.ToString();
                    settings.Save();
                }
                else
                {
                    bool authed = VM_ShokoServer.Instance.AuthenticateUser(selUser.Username, "");
                    string password = "";
                    while (!authed)
                        // prompt user for a password
                        if (Utils.DialogText(ref password, true, GUIWindowManager.ActiveWindow))
                        {
                            authed = VM_ShokoServer.Instance.AuthenticateUser(selUser.Username, password);
                            if (!authed)
                                Utils.DialogMsg(Translation.Error, Translation.IncorrectPasswordTryAgain);
                        }
                        else
                        {
                            return;
                        }

                    VM_ShokoServer.Instance.SetCurrentUser(selUser);

                    // user has logged in, so save to settings so we will log in as the same user next time
                    settings.CurrentJMMUserID = VM_ShokoServer.Instance.CurrentUser.JMMUserID.ToString();
                    settings.Save();
                }
            }

            VM_ShokoServer.Instance.ServerStatusEvent += Instance_ServerStatusEvent;

            isFirstInitDone = true;
        }


        private void GroupFilter_OnItemSelected(GUIListItem item)
        {
            VM_GroupFilter grpFilter = item.TVTag as VM_GroupFilter;
            if (grpFilter == null) return;
            History h = GetCurrent();
            if (h.Selected == grpFilter)
                return;
            h.Selected = grpFilter;

            if (displayGrpFilterTimer != null)
                displayGrpFilterTimer.Stop();

            displayGrpFilterTimer = new Timer();
            displayGrpFilterTimer.AutoReset = false;
            displayGrpFilterTimer.Interval = BaseConfig.Settings.InfoDelay; // 250ms
            displayGrpFilterTimer.Elapsed += displayGrpFilterTimer_Elapsed;
            displayGrpFilterTimer.Enabled = true;
        }


        void displayGrpFilterTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            GroupFilter_OnItemSelectedDisplay();
        }

        private Tuple<Fanart, string> FindFanartForGroupFilter(VM_GroupFilter gf)
        {
            if (gf.Childs.Count > 0)
                foreach (int v in gf.Childs.Randomize(gf.Childs.Count))
                {
                    VM_GroupFilter gfm = (VM_GroupFilter) VM_ShokoServer.Instance.ShokoServices.GetGroupFilter(v);
                    if (gfm != null)
                    {
                        Tuple<Fanart, string> n = FindFanartForGroupFilter(gfm);
                        if (n != null)
                            return n;
                    }
                }
            else if (gf.Groups.ContainsKey(VM_ShokoServer.Instance.CurrentUser.JMMUserID) && gf.Groups[VM_ShokoServer.Instance.CurrentUser.JMMUserID].Count > 0)
                foreach (int v in gf.Groups[VM_ShokoServer.Instance.CurrentUser.JMMUserID].Randomize(gf.Groups.Count))
                {
                    VM_AnimeGroup_User gvm = (VM_AnimeGroup_User) VM_ShokoServer.Instance.ShokoServices.GetGroup(v, VM_ShokoServer.Instance.CurrentUser.JMMUserID);
                    if (gvm != null)
                    {
                        Fanart fanart = new Fanart(gvm);
                        if (!string.IsNullOrEmpty(fanart.FileName))
                        {
                            string f = ImageAllocator.GetGroupImageAsFileName(gvm, GUIFacadeControl.Layout.List);
                            if (f != null)
                                return new Tuple<Fanart, string>(fanart, f);
                        }
                    }
                }
            return null;
        }


        private void GroupFilter_OnItemSelectedDisplay()
        {
            VM_GroupFilter gf = GetCurrent().Selected as VM_GroupFilter;
            if (gf == null) return;

            SetGUIProperty(GuiProperty.SeriesTitle, gf.GroupFilterName);
            SetGUIProperty(GuiProperty.Title, gf.GroupFilterName);
            SetGUIProperty(GuiProperty.GroupFilter_FilterName, gf.GroupFilterName);
            SetGUIProperty(GuiProperty.GroupFilter_GroupCount, gf.Childs.Count > 0 ? gf.Childs.Count.ToString() : gf.Groups[VM_ShokoServer.Instance.CurrentUser.JMMUserID].Count.ToString());

            Tuple<Fanart, string> ff = FindFanartForGroupFilter(gf);

            if (ff != null)
            {
                // Delayed Image Loading of Series Banners     
                listPoster.Filename = ff.Item2;
                LoadFanart(ff.Item1);
            }
        }


        private void Group_OnItemSelected(GUIListItem item)
        {
            VM_AnimeGroup_User grp = item?.TVTag as VM_AnimeGroup_User;
            if (grp == null) return;
            History h = GetCurrent();
            if (h.Selected == grp)
                return;
            h.Selected = grp;

            if (displayGrpTimer != null)
                displayGrpTimer.Stop();

            displayGrpTimer = new Timer();
            displayGrpTimer.AutoReset = false;
            displayGrpTimer.Interval = BaseConfig.Settings.InfoDelay; // 250ms
            displayGrpTimer.Elapsed += displayGrpTimer_Elapsed;
            displayGrpTimer.Enabled = true;
        }

        void displayGrpTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Group_OnItemSelectedDisplay();
        }

        private void Group_OnItemSelectedDisplay()
        {
            // when displaying a group we could be at the group list level or series list level
            // group level = top level groups
            // series level = sub-groups

            VM_AnimeGroup_User grp = GetCurrent().Selected as VM_AnimeGroup_User;

            if (grp == null) return;

            ClearGUIProperty(GuiProperty.Subtitle);

            if (grp.Stat_UserVoteOverall.HasValue)
            {
                SetGUIProperty(GuiProperty.SeriesGroup_MyRating, Utils.FormatAniDBRating((double) grp.Stat_UserVoteOverall.Value));
                // Image Rankings
                if (dummyStarCustomPlaceholder != null && dummyStarOffPlaceholder != null)
                {
                    string im = Logos.buildRating((double) grp.Stat_UserVoteOverall.Value, dummyStarOffPlaceholder.FileName,
                        dummyStarCustomPlaceholder.FileName,
                        dummyStarOnPlaceholder.Width, dummyStarOnPlaceholder.Height);
                    SetGUIProperty(GuiProperty.CustomRatingImage, im);
                }
            }
            else
            {
                ClearGUIProperty(GuiProperty.SeriesGroup_MyRating);
            }

            // set info properties
            // most of these properties actually come from the anidb_anime record
            // we need to find all the series for this group

            SetGUIProperty(GuiProperty.SeriesGroup_SeriesCount, grp.AllSeriesCount.ToString(Globals.Culture));
            SetGUIProperty(GuiProperty.SeriesGroup_Genre, grp.TagsFormatted);
            SetGUIProperty(GuiProperty.SeriesGroup_GenreShort, grp.TagsFormattedShort);
            SetGUIProperty(GuiProperty.SeriesGroup_Year, grp.YearFormatted);


            decimal totalRating = 0;
            int totalVotes = 0;
            int epCountNormal = 0;
            int epCountSpecial = 0;

            List<VM_AnimeSeries_User> seriesList = grp.ChildSeries;

            /*
            if (seriesList.Count == 1)
            {
              SetGUIProperty(GuiProperty.SeriesTitle, seriesList[0].SeriesName);
              SetGUIProperty(GuiProperty.Title, seriesList[0].SeriesName);
              SetGUIProperty(GuiProperty.Description, seriesList[0].Description);
            }
            else
            {
              SetGUIProperty(GuiProperty.SeriesTitle, grp.GroupName);
              SetGUIProperty(GuiProperty.Title, grp.GroupName);
              SetGUIProperty(GuiProperty.Description, grp.ParsedDescription);
            }*/
            SetGUIProperty(GuiProperty.SeriesTitle, grp.GroupName);
            SetGUIProperty(GuiProperty.Title, grp.GroupName);
            SetGUIProperty(GuiProperty.Description, grp.ParsedDescription);

            foreach (VM_AnimeSeries_User ser in seriesList)
            {
                totalRating += (decimal) ser.Anime.Rating * ser.Anime.VoteCount;
                totalRating += (decimal) ser.Anime.TempRating * ser.Anime.TempVoteCount;

                totalVotes += ser.Anime.AniDBTotalVotes;

                epCountNormal += ser.Anime.EpisodeCountNormal;
                epCountSpecial += ser.Anime.EpisodeCountSpecial;
            }
            decimal AniDBRating ;
            if (totalVotes == 0)
                AniDBRating = 0;
            else
                AniDBRating = totalRating / totalVotes / 100;

            if (dummyUserHasVotedSeries != null) dummyUserHasVotedSeries.Visible = false;
            // Only AniDB users have votes
            BaseConfig.MyAnimeLog.Write("IsAniDBUserBool : " + VM_ShokoServer.Instance.CurrentUser.IsAniDBUserBool());
            if (VM_ShokoServer.Instance.CurrentUser.IsAniDBUserBool())
            {
                BaseConfig.MyAnimeLog.Write("seriesList.Count : " + seriesList.Count);
                if (seriesList.Count == 1)
                {
                    VM_AniDB_Anime anAnime = seriesList[0].Anime;
                    string myRating = anAnime.UserVoteFormatted;
                    if (string.IsNullOrEmpty(myRating))
                    {
                        ClearGUIProperty(GuiProperty.SeriesGroup_MyRating);
                    }
                    else
                    {
                        SetGUIProperty(GuiProperty.SeriesGroup_MyRating, myRating);
                        // Image Rankings
                        if (dummyStarCustomPlaceholder != null && dummyStarOffPlaceholder != null)
                        {
                            string im = Logos.buildRating(Convert.ToDouble(myRating),
                                dummyStarOffPlaceholder.FileName, dummyStarCustomPlaceholder.FileName,
                                dummyStarOnPlaceholder.Width, dummyStarOnPlaceholder.Height);
                            SetGUIProperty(GuiProperty.CustomRatingImage, im);
                        }

                        if (dummyUserHasVotedSeries != null)
                        {
                            dummyUserHasVotedSeries.Visible = true;
                            BaseConfig.MyAnimeLog.Write("myRating : " + myRating);
                            BaseConfig.MyAnimeLog.Write("dummyUserHasVotedSeries.Visible : " +
                                                        dummyUserHasVotedSeries.Visible);
                        }


                    }
                }
            }


            string rating = Utils.FormatAniDBRating((double) AniDBRating) + " (" + totalVotes + " " + Translation.Votes + ")";
            SetGUIProperty(GuiProperty.SeriesGroup_RawRating, Utils.FormatAniDBRating((double) AniDBRating));
            SetGUIProperty(GuiProperty.SeriesGroup_RatingVoteCount, totalVotes.ToString(Globals.Culture));
            SetGUIProperty(GuiProperty.SeriesGroup_Rating, rating);

            // set watched/unavailable flag
            if (dummyIsWatched != null) dummyIsWatched.Visible = grp.UnwatchedEpisodeCount == 0;


            string eps = epCountNormal + " (" + epCountSpecial + " " + Translation.Specials + ")";
            //string epsa = epCountNormalAvailable.ToString() + " (" + epCountSpecialAvailable.ToString() + " Specials)";


            SetGUIProperty(GuiProperty.SeriesGroup_Episodes, eps);
            //setGUIProperty("SeriesGroup.EpisodesAvailable", epsa);

            SetGUIProperty(GuiProperty.SeriesGroup_EpisodeCountNormal, epCountNormal.ToString(Globals.Culture));
            SetGUIProperty(GuiProperty.SeriesGroup_EpisodeCountSpecial, epCountSpecial.ToString(Globals.Culture));
            //setGUIProperty("SeriesGroup.EpisodeCountNormalAvailable", epCountNormalAvailable.ToString());
            //setGUIProperty("SeriesGroup.EpisodeCountSpecialAvailable", epCountSpecialAvailable.ToString());
            SetGUIProperty(GuiProperty.SeriesGroup_EpisodeCountUnwatched, grp.UnwatchedEpisodeCount.ToString(Globals.Culture));
            SetGUIProperty(GuiProperty.SeriesGroup_EpisodeCountWatched, grp.WatchedEpisodeCount.ToString(Globals.Culture));


            // Delayed Image Loading of Series Banners            
            listPoster.Filename = ImageAllocator.GetGroupImageAsFileName(grp, GUIFacadeControl.Layout.List);

            LoadFanart(grp);
        }

        private void EpisodeType_OnItemSelected(GUIListItem item)
        {
            if (m_bQuickSelect)
                return;

            if (dummyIsAvailable != null) dummyIsAvailable.Visible = true;

            VM_AnimeEpisodeType ept = item?.TVTag as VM_AnimeEpisodeType;
            if (ept == null) return;
            History h = GetCurrent();
            if (h.Selected == ept)
                return;
            h.Selected = ept;
            VM_AnimeSeries_User ser = GetTopSerie();

            if (ser == null) return;
            VM_AniDB_Anime anAnime = ser.Anime;

            SetGUIProperty(GuiProperty.SeriesTitle, ser.SeriesName);
            ClearGUIProperty(GuiProperty.Subtitle);
            SetGUIProperty(GuiProperty.Description, ser.Description);

            // set info properties
            // most of these properties actually come from the anidb_anime record
            // we need to find all the series for this group

            SetGUIProperty(GuiProperty.SeriesGroup_Year, anAnime.Year);
            SetGUIProperty(GuiProperty.SeriesGroup_Genre, anAnime.TagsFormatted);
            SetGUIProperty(GuiProperty.SeriesGroup_GenreShort, anAnime.TagsFormattedShort);

            string eps = anAnime.EpisodeCountNormal + " (" + anAnime.EpisodeCountSpecial + " " + Translation.Specials + ")";
            SetGUIProperty(GuiProperty.SeriesGroup_Episodes, eps);

            SetGUIProperty(GuiProperty.SeriesGroup_EpisodeCountNormal, anAnime.EpisodeCountNormal.ToString(Globals.Culture));
            SetGUIProperty(GuiProperty.SeriesGroup_EpisodeCountSpecial, anAnime.EpisodeCountSpecial.ToString(Globals.Culture));


            string rating = "";

            rating = Utils.FormatAniDBRating((double) anAnime.AniDBRating) + " (" + anAnime.AniDBTotalVotes + " " + Translation.Votes + ")";
            SetGUIProperty(GuiProperty.SeriesGroup_RawRating, Utils.FormatAniDBRating((double) anAnime.AniDBRating));
            SetGUIProperty(GuiProperty.SeriesGroup_RatingVoteCount, anAnime.AniDBTotalVotes.ToString(Globals.Culture));
            SetGUIProperty(GuiProperty.SeriesGroup_Rating, rating);
            // Image Rankings
            if (dummyStarOnPlaceholder != null && dummyStarOffPlaceholder != null)
            {
                string im = Logos.buildRating((double) anAnime.AniDBRating, dummyStarOffPlaceholder.FileName, dummyStarOnPlaceholder.FileName,
                    dummyStarOnPlaceholder.Width, dummyStarOnPlaceholder.Height);
                SetGUIProperty(GuiProperty.RatingImage, im);
            }


            int unwatched = 0;
            int watched = 0;
            if (ser != null)
                ser.GetWatchedUnwatchedCount(ept.EpisodeType, ref unwatched, ref watched);

            // set watched/unavailable flag
            if (dummyIsWatched != null) dummyIsWatched.Visible = unwatched == 0;

            SetGUIProperty(GuiProperty.SeriesGroup_EpisodeCountUnwatched, unwatched.ToString(Globals.Culture));
            SetGUIProperty(GuiProperty.SeriesGroup_EpisodeCountWatched, watched.ToString(Globals.Culture));


            // Delayed Image Loading of Series Banners            
            listPoster.Filename = ImageAllocator.GetSeriesImageAsFileName(ser, GUIFacadeControl.Layout.List);
        }


        private void Series_OnItemSelected(GUIListItem item)
        {
            VM_AnimeSeries_User ser = item?.TVTag as VM_AnimeSeries_User;
            if (ser == null) return;
            History h = GetCurrent();
            if (h.Selected == ser)
                return;
            h.Selected = ser;

            // need to do this here as well because we display series and sub-groups in the same list
            if (displayGrpTimer != null)
                displayGrpTimer.Stop();


            if (m_bQuickSelect)
                return;

            if (dummyIsAvailable != null) dummyIsAvailable.Visible = true;

            ClearGUIProperty(GuiProperty.RomanjiTitle);
            ClearGUIProperty(GuiProperty.EnglishTitle);
            ClearGUIProperty(GuiProperty.KanjiTitle);
            ClearGUIProperty(GuiProperty.RotatorTitle);
            if (item?.TVTag == null || !(item.TVTag is VM_AnimeSeries_User))
                return;


            BaseConfig.MyAnimeLog.Write(ser.ToString());

            SetGUIProperty(GuiProperty.SeriesTitle, ser.SeriesName);
            ClearGUIProperty(GuiProperty.Subtitle);
            SetGUIProperty(GuiProperty.Description, ser.Description);

            // set info properties
            // most of these properties actually come from the anidb_anime record
            // we need to find all the series for this group

            VM_AniDB_Anime anAnime = ser.Anime;

            if (dummyUserHasVotedSeries != null) dummyUserHasVotedSeries.Visible = false;
            // Only AniDB users have votes
            if (VM_ShokoServer.Instance.CurrentUser.IsAniDBUserBool())
            {
                string myRating = anAnime.UserVoteFormatted;
                if (string.IsNullOrEmpty(myRating))
                {
                    ClearGUIProperty(GuiProperty.SeriesGroup_MyRating);
                }
                else
                {
                    SetGUIProperty(GuiProperty.SeriesGroup_MyRating, myRating);
                    if (dummyUserHasVotedSeries != null) dummyUserHasVotedSeries.Visible = true;
                    // Image Rankings
                    if (dummyStarCustomPlaceholder != null && dummyStarOffPlaceholder != null)
                    {
                        string im = Logos.buildRating(Convert.ToDouble(myRating), dummyStarOffPlaceholder.FileName, dummyStarCustomPlaceholder.FileName,
                            dummyStarOnPlaceholder.Width, dummyStarOnPlaceholder.Height);
                        SetGUIProperty(GuiProperty.CustomRatingImage, im);
                    }
                }
            }

            BaseConfig.MyAnimeLog.Write(anAnime.ToString());

            SetGUIProperty(GuiProperty.SeriesGroup_Year, anAnime.Year);
            SetGUIProperty(GuiProperty.SeriesGroup_Genre, anAnime.TagsFormatted);
            SetGUIProperty(GuiProperty.SeriesGroup_GenreShort, anAnime.TagsFormattedShort);


            string eps = anAnime.EpisodeCountNormal + " (" + anAnime.EpisodeCountSpecial + " " + Translation.Specials + ")";

            SetGUIProperty(GuiProperty.SeriesGroup_Episodes, eps);

            SetGUIProperty(GuiProperty.SeriesGroup_EpisodeCountNormal, anAnime.EpisodeCountNormal.ToString(Globals.Culture));
            SetGUIProperty(GuiProperty.SeriesGroup_EpisodeCountSpecial, anAnime.EpisodeCountSpecial.ToString(Globals.Culture));

            string rating ;

            rating = Utils.FormatAniDBRating((double) anAnime.AniDBRating) + " (" + anAnime.AniDBTotalVotes + " votes)";
            SetGUIProperty(GuiProperty.SeriesGroup_RawRating, Utils.FormatAniDBRating((double) anAnime.AniDBRating));
            SetGUIProperty(GuiProperty.SeriesGroup_RatingVoteCount, anAnime.AniDBTotalVotes.ToString(Globals.Culture));

            SetGUIProperty(GuiProperty.SeriesGroup_Rating, rating);

            // Image Rankings
            if (dummyStarOnPlaceholder != null && dummyStarOffPlaceholder != null)
            {
                string im = Logos.buildRating((double) anAnime.AniDBRating, dummyStarOffPlaceholder.FileName, dummyStarOnPlaceholder.FileName,
                    dummyStarOnPlaceholder.Width, dummyStarOnPlaceholder.Height);
                SetGUIProperty(GuiProperty.RatingImage, im);
            }


            // set watched/unavailable flag
            if (dummyIsWatched != null) dummyIsWatched.Visible = ser.UnwatchedEpisodeCount == 0;

            SetGUIProperty(GuiProperty.SeriesGroup_EpisodeCountUnwatched, ser.UnwatchedEpisodeCount.ToString(Globals.Culture));
            SetGUIProperty(GuiProperty.SeriesGroup_EpisodeCountWatched, ser.WatchedEpisodeCount.ToString(Globals.Culture));


            // Delayed Image Loading of Series Banners
            listPoster.Filename = ImageAllocator.GetSeriesImageAsFileName(ser, GUIFacadeControl.Layout.List);


            LoadFanart(ser);
        }

        private string GetSeriesTypeLabel()
        {
            VM_AnimeEpisodeType ept = GetTopEpType();
            if (ept != null) return ept.EpisodeTypeDescription;
            VM_AnimeSeries_User ser = GetTopSerie();
            if (ser != null) return ser.SeriesName;
            VM_AnimeGroup_User grp = GetTopGroup();
            if (grp != null)
                return grp.GroupName;
            return string.Empty;
        }


        private void Episode_OnItemSelected(GUIListItem item)
        {
            if (m_bQuickSelect)
                return;

            VM_AnimeEpisode_User ep = item?.TVTag as VM_AnimeEpisode_User;
            if (ep == null) return;
            History h = GetCurrent();
            if (h.Selected == ep)
                return;
            h.Selected = ep;


            if (!string.IsNullOrEmpty(ep.EpisodeImageLocation))
            {
                BaseConfig.MyAnimeLog.Write("We have a local thumbnail: " + ep.EpisodeImageLocation);
                SetGUIProperty(GuiProperty.Episode_Image, ep.EpisodeImageLocation);

                /*
                //Try to find local thumbnail and use that instead of fanart if it has none
                if (settings.LoadLocalThumbnails)
                {
                  string localThumbnail = LoadLocalThumbnail(ep.AnimeEpisodeID);
                  if (!string.IsNullOrEmpty(localThumbnail) && string.IsNullOrEmpty(fanartTexture.Filename))
                  {
                    fanartTexture.Filename = localThumbnail;
        
                    if (this.dummyIsFanartLoaded != null)
                      this.dummyIsFanartLoaded.Visible = true;
                  }
                }*/
            }
            else if (settings.LoadLocalThumbnails)
            {
                BaseConfig.MyAnimeLog.Write("We do not have a local thumbnail for: " + ep.AnimeSeries.SeriesName);
                string localThumbnail = LoadLocalThumbnail(ep.AnimeEpisodeID);

                // Try to find local thumbnail
                if (string.IsNullOrEmpty(localThumbnail))
                {
                    // Fallback to default thumbnail if none found
                    SetGUIProperty(GuiProperty.Episode_Image, ep.EpisodeImageLocation);
                    fanartTexture.Filename = GUIGraphicsContext.Skin + @"\Media\hover_my anime3.jpg";
                }
                else
                {
                    // Set thumbnail to local and replace fanart image with it as well
                    SetGUIProperty(GuiProperty.Episode_Image, localThumbnail);

                    fanartTexture.Filename = localThumbnail;

                    if (dummyIsFanartLoaded != null)
                        dummyIsFanartLoaded.Visible = true;
                }
            }
            else
            {
                ClearGUIProperty(GuiProperty.Episode_Image);
            }

            if (!settings.HidePlot)
            {
                SetGUIProperty(GuiProperty.Episode_Description, ep.EpisodeOverview);
            }
            else
            {
                if (ep.EpisodeOverview.Trim().Length > 0 && ep.IsWatched())
                    SetGUIProperty(GuiProperty.Episode_Description, "*** " + Translation.HiddenToPreventSpoiles + " ***");
                else if (!string.IsNullOrEmpty(ep.EpisodeOverview))
                    SetGUIProperty(GuiProperty.Episode_Description, ep.EpisodeOverview);
                else
                    ClearGUIProperty(GuiProperty.Episode_Description);
            }
            // Make sure to set title again, needed for direct series list navigations (continue watching / something random)
            if (GetPropertyName(GuiProperty.Title) != ep.AnimeSeries.SeriesName)
                SetGUIProperty(GuiProperty.Title, ep.AnimeSeries.SeriesName);

            SetGUIProperty(GuiProperty.Episode_EpisodeName, ep.EpisodeName);
            SetGUIProperty(GuiProperty.Episode_EpisodeDisplayName, ep.DisplayName);
            SetGUIProperty(GuiProperty.Episode_SeriesTypeLabel, GetSeriesTypeLabel());

            SetGUIProperty(GuiProperty.Episode_AirDate, ep.AirDateAsString);
            SetGUIProperty(GuiProperty.Episode_Length, Utils.FormatSecondsToDisplayTime(ep.AniDB_LengthSeconds));
            string rating = Utils.FormatAniDBRating(Convert.ToDouble(ep.AniDB_Rating)) + " (" + ep.AniDB_Votes + " " + Translation.Votes + ")";

            SetGUIProperty(GuiProperty.Episode_Rating, rating);
            //setGUIProperty("Episode.RawRating", Utils.FormatAniDBRating(ep.AniDB_Rating));
            SetGUIProperty(GuiProperty.Episode_RawRating, Utils.FormatAniDBRating(Convert.ToDouble(ep.AniDB_Rating)));
            SetGUIProperty(GuiProperty.Episode_RatingVoteCount, ep.AniDB_Votes);


            if (dummyIsWatched != null) dummyIsWatched.Visible = ep.IsWatched();

            // Image Rankings
            if (dummyStarOnPlaceholder != null && dummyStarOffPlaceholder != null)
            {
                string im = Logos.buildRating(Convert.ToDouble(ep.AniDB_Rating), dummyStarOffPlaceholder.FileName, dummyStarOnPlaceholder.FileName,
                    dummyStarOnPlaceholder.Width, dummyStarOnPlaceholder.Height);
                SetGUIProperty(GuiProperty.RatingImage, im);
            }

            if (dummyIsAvailable != null) dummyIsAvailable.Visible = true;
            // get all the LocalFile rceords for this episode
            List<VM_VideoDetailed> fileLocalList = ep.FilesForEpisode;

            string finfo = "";
            foreach (VM_VideoDetailed vid in fileLocalList)
                finfo = vid.FileSelectionDisplay;

            if (fileLocalList.Count == 1)
            {
                SetGUIProperty(GuiProperty.Episode_FileInfo, finfo);
            }
            else if (fileLocalList.Count > 1)
            {
                SetGUIProperty(GuiProperty.Episode_FileInfo, fileLocalList.Count.ToString(Globals.Culture) + " " + Translation.FilesAvailable);
            }
            else if (fileLocalList.Count == 0)
            {
                ClearGUIProperty(GuiProperty.Episode_FileInfo);
                if (dummyIsAvailable != null) dummyIsAvailable.Visible = false;
            }

            string logos = Logos.buildLogoImage(ep);
            //MyAnimeLog.Write(logos);

            BaseConfig.MyAnimeLog.Write(logos);
            SetGUIProperty(GuiProperty.Logos, logos);
        }


        private void LoadFanart(object objectWithFanart)
        {
            BaseConfig.MyAnimeLog.Write("LOADING FANART FOR:: {0}", objectWithFanart.ToString());

            try
            {
                DateTime start = DateTime.Now;
                string desc = string.Empty;
                Fanart fanart = null;
                if (objectWithFanart is Fanart)
                {
                    fanart = (Fanart) objectWithFanart;
                }
                else if (objectWithFanart.GetType() == typeof(VM_AnimeGroup_User))
                {
                    VM_AnimeGroup_User grp = (VM_AnimeGroup_User) objectWithFanart;
                    fanart = new Fanart(grp);
                    desc = grp.GroupName;
                }
                else if (objectWithFanart.GetType() == typeof(VM_AnimeSeries_User))
                {
                    VM_AnimeSeries_User ser = (VM_AnimeSeries_User) objectWithFanart;
                    fanart = new Fanart(ser);
                    desc = ser.SeriesName;
                }

                TimeSpan ts = DateTime.Now - start;
                BaseConfig.MyAnimeLog.Write("GOT FANART details in: {0} ms ({1})", ts.TotalMilliseconds, desc);
                BaseConfig.MyAnimeLog.Write("LOADING FANART: {0} - {1}", desc, fanart?.FileName ?? "NONE");

                if (string.IsNullOrEmpty(fanart?.FileName))
                    fanartTexture.Filename = GUIGraphicsContext.Skin + @"\Media\hover_my anime3.jpg";
                else
                    fanartTexture.Filename = fanart.FileName;

                if (dummyIsFanartLoaded != null)
                    dummyIsFanartLoaded.Visible = true;
            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write(ex.ToString());
            }
        }

        public string LoadLocalThumbnail(int episodeID)
        {
            string thumbnail = "";

            try
            {
                List<VM_VideoDetailed> epContracts = VM_ShokoServer.Instance.ShokoServices.GetFilesForEpisode(episodeID,
                    VM_ShokoServer.Instance.CurrentUser.JMMUserID).CastList<VM_VideoDetailed>();

                if (epContracts.Any())
                    foreach (VM_VideoDetailed epcontract in epContracts)
                    {
                        string episodeFilePathWithExtension = epcontract.GetLocalFileSystemFullPath();

                        if (string.IsNullOrEmpty(episodeFilePathWithExtension))
                            continue;


                        // Full episode file path without file extension
                        string episodeFileNameWithoutExtension =
                            Path.GetFileNameWithoutExtension(episodeFilePathWithExtension);
                        string episodeFilePathWithoutExtension = Path.Combine(
                            Path.GetDirectoryName(episodeFilePathWithExtension), episodeFileNameWithoutExtension);

                        //BaseConfig.MyAnimeLog.Write("Episode file path without extension: " + episodeFilePathWithoutExtension);

                        // Thumbnail format: <episode_full_path_without_extension>.jpg <--- the Mediaportal default
                        if (File.Exists(episodeFilePathWithoutExtension + ".jpg"))
                        {
                            thumbnail = episodeFilePathWithoutExtension + ".jpg";
                            return thumbnail;
                        }

                        //BaseConfig.MyAnimeLog.Write("Episode file path with extensions: " + episodeFilePathWithExtension);

                        // Thumbnail format: <episode_full_path_with_extension>.jpg <--- the standard in programs like Video Thumbnails Maker
                        if (File.Exists(episodeFilePathWithExtension + ".jpg"))
                        {
                            thumbnail = episodeFilePathWithExtension + ".jpg";
                            return thumbnail;
                        }
                    }
                BaseConfig.MyAnimeLog.Write("We don't have episode contracts for: {0}", episodeID);
            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write("Error in LoadLocalThumbnail: {0}", ex);
            }

            return thumbnail;
        }
        /*
        void DisableFanart()
        {
            //fanartSet = false;
            fanartTexture.Filename = "";


            if (dummyIsFanartLoaded != null)
                dummyIsFanartLoaded.Visible = false;
        }
        */

        protected override void OnShowContextMenu()
        {
            try
            {
                if (GetCurrent().Selected == null)
                    return;
                IVM vm = GetCurrent().Selected;
                if (vm == null)
                    return;
                if (vm is VM_GroupFilter)
                    ShowContextMenuGroupFilter("");
                else if (vm is VM_AnimeGroup_User)
                    ShowContextMenuGroup("");
                else if (vm is VM_AnimeSeries_User)
                    ShowContextMenuSeries("");
                else if (vm is VM_AnimeEpisode_User)
                    ShowContextMenuEpisode("");
            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write("Error in menu: {0}", ex);
            }
        }


        private ContextMenuAction SearchTheTvDB(VM_AnimeSeries_User ser, string searchCriteria, string previousMenu)
        {
            if (searchCriteria.Length == 0)
                return ContextMenuAction.Exit;

            int aniDBID = ser.Anime.AnimeID;

            List<TVDB_Series_Search_Response> TVDBSeriesSearchResults = VM_ShokoServer.Instance.ShokoServices.SearchTheTvDB(searchCriteria.Trim());


            BaseConfig.MyAnimeLog.Write("Found {0} tvdb results for {1}", TVDBSeriesSearchResults.Count, searchCriteria);
            if (TVDBSeriesSearchResults.Count > 0)
            {
                ContextMenu cmenu = new ContextMenu(Translation.TVDBSearchResults, previousmenu: previousMenu);
                foreach (TVDB_Series_Search_Response res in TVDBSeriesSearchResults)
                {
                    TVDB_Series_Search_Response local = res;
                    string disp = String.Format("{0} ({1}) / {2}", res.SeriesName, res.Language, res.Id);
                    cmenu.AddAction(disp, () => LinkAniDBToTVDB(ser, aniDBID, EpisodeType.Episode, 1, local.SeriesID, 1, 1));
                }
                return cmenu.Show();
            }
            this.Alert(Translation.SearchResults, string.Empty, Translation.NoResultsFound);
            return ContextMenuAction.Exit;
        }


        private ContextMenuAction SearchTheTvDBMenu(VM_AnimeSeries_User ser, string previousMenu)
        {
            //string searchCriteria = "";
            int aniDBID = ser.Anime.AnimeID;
            ContextMenu cmenu = new ContextMenu(Translation.SearchTheTvDB, previousMenu);
            cmenu.Add(Translation.SearchUsing + ": " + ser.Anime.FormattedTitle, () => SearchTheTvDB(ser, ser.Anime.FormattedTitle, Translation.SearchTheTvDB));
            cmenu.Add(Translation.ManualSearch, () =>
            {
                if (Utils.DialogText(ref searchText, GetID))
                    return SearchTheTvDB(ser, searchText, Translation.SearchTheTvDB);
                return ContextMenuAction.Continue;
            });


            List<Azure_CrossRef_AniDB_TvDB> CrossRef_AniDB_TvDBResult = VM_ShokoServer.Instance.ShokoServices.GetTVDBCrossRefWebCache(aniDBID, false);
            if (CrossRef_AniDB_TvDBResult != null && CrossRef_AniDB_TvDBResult.Count > 0)
            {
                string xrefSummary = string.Empty;
                foreach (Azure_CrossRef_AniDB_TvDB xref in CrossRef_AniDB_TvDBResult)
                {
                    xrefSummary += Environment.NewLine;
                    xrefSummary += string.Format("AniDB {0}:{1} -- TvDB {2}: {3}:{4}",
                        xref.AniDBStartEpisodeType, xref.AniDBStartEpisodeNumber, xref.TvDBTitle, xref.TvDBSeasonNumber, xref.TvDBStartEpisodeNumber);
                }

                cmenu.AddAction(Translation.CommunitySays + ": " + xrefSummary, () =>
                {
                    string res = VM_ShokoServer.Instance.ShokoServices.RemoveLinkAniDBTvDBForAnime(aniDBID);
                    if (res.Length > 0)
                        Utils.DialogMsg(Translation.Error, res);
                    else
                        foreach (CrossRef_AniDB_TvDBV2 xref in CrossRef_AniDB_TvDBResult)
                            LinkAniDBToTVDB(ser, xref.AnimeID, (EpisodeType) xref.AniDBStartEpisodeType,
                                xref.AniDBStartEpisodeNumber, xref.TvDBID,
                                xref.TvDBSeasonNumber, xref.TvDBStartEpisodeNumber);
                });
            }
            return cmenu.Show();
        }

        private void LinkAniDBToTVDB(VM_AnimeSeries_User ser, int animeID, EpisodeType anidbEpType, int anidbEpNumber, int tvDBID, int tvSeason, int tvEpNumber)
        {
            string res = VM_ShokoServer.Instance.ShokoServices.LinkAniDBTvDB(animeID, (int) anidbEpType, anidbEpNumber,
                tvDBID, tvSeason, tvEpNumber, null);

            if (res.Length > 0)
                Utils.DialogMsg("Error", res);

            ShokoServerHelper.GetSeries(ser.AnimeSeriesID);
        }

        private void LinkAniDBToMovieDB(VM_AnimeSeries_User ser, int animeID, int movieID)
        {
            string res = VM_ShokoServer.Instance.ShokoServices.LinkAniDBOther(animeID, movieID, (int) CrossRefType.MovieDB);
            if (res.Length > 0)
                Utils.DialogMsg(Translation.Error, res);

            ShokoServerHelper.GetSeries(ser.AnimeSeriesID);
        }

        private void LinkAniDBToMAL(VM_AnimeSeries_User ser, int animeID, int malID, string malTitle)
        {
            if (ser.CrossRefAniDBMAL != null)
                foreach (CrossRef_AniDB_MAL xref in ser.CrossRefAniDBMAL)
                    VM_ShokoServer.Instance.ShokoServices.RemoveLinkAniDBMAL(xref.AnimeID, xref.StartEpisodeType, xref.StartEpisodeNumber);

            string res = VM_ShokoServer.Instance.ShokoServices.LinkAniDBMAL(animeID, malID, malTitle, 1, 1);
            if (res.Length > 0)
                Utils.DialogMsg(Translation.Error, res);

            ShokoServerHelper.GetSeries(ser.AnimeSeriesID);
        }

        private ContextMenuAction SearchTheMovieDBMenu(VM_AnimeSeries_User ser, string previousMenu)
        {
            int aniDBID = ser.Anime.AnimeID;
            ContextMenu cmenu = new ContextMenu(Translation.SearchTheMovieDB, previousMenu);
            cmenu.Add(Translation.SearchUsing + ": " + ser.Anime.FormattedTitle, () => SearchTheMovieDB(ser, ser.Anime.FormattedTitle, Translation.SearchTheMovieDB));
            cmenu.Add(Translation.ManualSearch, () =>
            {
                searchText = ser.Anime.FormattedTitle;
                if (Utils.DialogText(ref searchText, GetID))
                    return SearchTheMovieDB(ser, ser.Anime.FormattedTitle, Translation.SearchTheMovieDB);
                return ContextMenuAction.Continue;
            });
            CL_CrossRef_AniDB_Other_Response crossrossRefAniDbOtherResult = VM_ShokoServer.Instance.ShokoServices.GetOtherAnimeCrossRefWebCache(aniDBID, (int) CrossRefType.MovieDB);
            if (crossrossRefAniDbOtherResult != null)
                cmenu.AddAction(Translation.CommunitySays + ": " + crossrossRefAniDbOtherResult.CrossRefID, () => LinkAniDBToMovieDB(ser, crossrossRefAniDbOtherResult.AnimeID, Int32.Parse(crossrossRefAniDbOtherResult.CrossRefID)));
            return cmenu.Show();
        }


        private ContextMenuAction SearchTheMovieDB(VM_AnimeSeries_User ser, string searchCriteria, string previousMenu)
        {
            if (searchCriteria.Length == 0)
                return ContextMenuAction.Exit;
            int aniDbid = ser.Anime.AnimeID;

            List<CL_MovieDBMovieSearch_Response> movieDbSeriesSearchResults = VM_ShokoServer.Instance.ShokoServices.SearchTheMovieDB(searchCriteria.Trim());


            BaseConfig.MyAnimeLog.Write("Found {0} moviedb results for {1}", movieDbSeriesSearchResults.Count, searchCriteria);

            if (movieDbSeriesSearchResults.Count > 0)
            {
                ContextMenu cmenu = new ContextMenu(Translation.SearchResults, previousmenu: previousMenu);
                foreach (CL_MovieDBMovieSearch_Response res in movieDbSeriesSearchResults)
                {
                    CL_MovieDBMovieSearch_Response local = res;
                    cmenu.AddAction(res.MovieName, () => LinkAniDBToMovieDB(ser, aniDbid, local.MovieID));
                }
                return cmenu.Show();
            }
            this.Alert(Translation.SearchResults, string.Empty, Translation.NoResultsFound);
            return ContextMenuAction.Exit;
        }

        private ContextMenuAction SearchMALMenu(VM_AnimeSeries_User ser, string previousMenu)
        {
     

            ContextMenu cmenu = new ContextMenu(Translation.SearchMAL, previousmenu: previousMenu);
            cmenu.Add(Translation.SearchUsing + ": " + ser.Anime.FormattedTitle, () => SearchMAL(ser, ser.Anime.FormattedTitle, Translation.SearchMAL));
            cmenu.Add(Translation.ManualSearch, () =>
            {
                searchText = ser.Anime.FormattedTitle;
                if (Utils.DialogText(ref searchText, GetID))
                    return SearchMAL(ser, searchText, Translation.SearchMAL);
                return ContextMenuAction.Continue;
            });
            return cmenu.Show();
        }

        private ContextMenuAction SearchMAL(VM_AnimeSeries_User ser, string searchCriteria, string previousMenu)
        {
            if (searchCriteria.Length == 0)
                return ContextMenuAction.Exit;

            int aniDbid = ser.Anime.AnimeID;

            List<CL_MALAnime_Response> malSearchResults = VM_ShokoServer.Instance.ShokoServices.SearchMAL(searchCriteria.Trim());


            BaseConfig.MyAnimeLog.Write("Found {0} MAL results for {1}", malSearchResults.Count, searchCriteria);

            if (malSearchResults.Count > 0)
            {
                ContextMenu cmenu = new ContextMenu(Translation.SearchResults, previousmenu: previousMenu);
                foreach (CL_MALAnime_Response res in malSearchResults)
                {
                    CL_MALAnime_Response local = res;
                    cmenu.AddAction(String.Format("{0} ({1} {2})", res.title, res.episodes, Translation.EpisodesShort), () => LinkAniDBToMAL(ser, aniDbid, local.id, local.title));
                }
                return cmenu.Show();
            }
            this.Alert(Translation.SearchResults, string.Empty, Translation.NoResultsFound);
            return ContextMenuAction.Exit;
        }

        private void ReplaceSerie(int id, VM_AnimeSeries_User ser)
        {
            for (int x = 0; x < Breadcrumbs.Count; x++)
            {
                VM_AnimeSeries_User sel = Breadcrumbs[x].Selected as VM_AnimeSeries_User;
                VM_AnimeSeries_User lis = Breadcrumbs[x].Listing as VM_AnimeSeries_User;
                if (sel != null && sel.AnimeSeriesID == id)
                    Breadcrumbs[x].Selected = ser;
                if (lis != null && lis.AnimeSeriesID == id)
                    Breadcrumbs[x].Listing = ser;
            }
        }

        private ContextMenuAction ShowContextMenuEpisode(string previousMenu)
        {
            GUIListItem currentitem = m_Facade.SelectedListItem;
            if (currentitem == null)
                return ContextMenuAction.Exit;

            GUIDialogMenu dlg = (GUIDialogMenu) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_MENU);
            if (dlg == null)
                return ContextMenuAction.Exit;

            VM_AnimeEpisode_User episode = currentitem.TVTag as VM_AnimeEpisode_User;
            if (episode == null)
                return ContextMenuAction.Exit;
            bool isWatched = episode.IsWatched();

            ContextMenu cmenu = new ContextMenu(episode.EpisodeNumberAndName, previousmenu: previousMenu);
            cmenu.AddAction(isWatched ? Translation.MarkAsUnwatched : Translation.MarkAsWatched, () =>
            {
                BaseConfig.MyAnimeLog.Write("Toggle watched status: {0} - {1}", isWatched, episode);
                episode.ToggleWatchedStatus(!isWatched);
                LoadFacade();
            });
            VM_AnimeSeries_User ser = GetTopSerie();
            VM_AnimeEpisodeType e = GetTopEpType();
            EpisodeType ept = EpisodeType.Episode;
            if (e != null)
                ept = e.EpisodeType;
            cmenu.AddAction(Translation.MarkAllAsWatched, () =>
            {
                if (ser == null)
                {
                    // If we can't get a top series we fallback on what the episode tells us
                    BaseConfig.MyAnimeLog.Write("Error during watch state change - series was null!");
                    ser = episode.AnimeSeries;
                }

                if (ser?.AnimeSeriesID != null)
                {
                    BaseConfig.MyAnimeLog.Write("Marking series as watched: ID -> {0} - Name -> {1}", ser.AnimeSeriesID,
                        ser.SeriesName);

                    VM_ShokoServer.Instance.ShokoServices.SetWatchedStatusOnSeries(ser.AnimeSeriesID, true,
                        Int32.MaxValue, (int) ept, VM_ShokoServer.Instance.CurrentUser.JMMUserID);

                    if (BaseConfig.Settings.DisplayRatingDialogOnCompletion)
                    {
                        ser =
                            (VM_AnimeSeries_User) VM_ShokoServer.Instance.ShokoServices.GetSeries(ser.AnimeSeriesID,
                                VM_ShokoServer.Instance.CurrentUser.JMMUserID);
                        if (ser != null)
                        {
                            if (ser.AnimeSeriesID != 0)
                                ReplaceSerie(ser.AnimeSeriesID, ser);
                            Utils.PromptToRateSeriesOnCompletion(ser);
                        }
                    }
                    LoadFacade();
                }
            });
            cmenu.AddAction(Translation.MarkAllAsUnwatched, () =>
            {
                if (ser == null)
                {
                    // If we can't get a top series we fallback on what the episode tells us
                    BaseConfig.MyAnimeLog.Write("Error during watch state change - series was null!");
                    ser = episode.AnimeSeries;
                }
                else
                {
                    BaseConfig.MyAnimeLog.Write("Marking series as un-watched: ID -> {0} - Name -> {1}", ser.AnimeSeriesID,
                        ser.SeriesName);

                    VM_ShokoServer.Instance.ShokoServices.SetWatchedStatusOnSeries(ser.AnimeSeriesID, false,
                        Int32.MaxValue, (int) ept, VM_ShokoServer.Instance.CurrentUser.JMMUserID);
                    LoadFacade();
                }
            });
            cmenu.AddAction(Translation.MarkAllPreviousAsWatched, () =>
            {
                if (ser == null)
                {
                    // If we can't get a top series we fallback on what the episode tells us
                    BaseConfig.MyAnimeLog.Write("Error during watch state change - series was null!");
                    ser = episode.AnimeSeries;
                }
                else                
                {
                    VM_ShokoServer.Instance.ShokoServices.SetWatchedStatusOnSeries(ser.AnimeSeriesID, false,
                        episode.EpisodeNumber - 1, (int) ept, VM_ShokoServer.Instance.CurrentUser.JMMUserID);

                    ser = (VM_AnimeSeries_User)
                        VM_ShokoServer.Instance.ShokoServices.GetSeries(ser.AnimeSeriesID,
                            VM_ShokoServer.Instance.CurrentUser.JMMUserID);
                    if (ser != null)
                    {
                        if (ser.AnimeSeriesID != 0)
                            ReplaceSerie(ser.AnimeSeriesID, ser);
                        Utils.PromptToRateSeriesOnCompletion(ser);
                    }
                    LoadFacade();
                }
            });
            cmenu.AddAction(Translation.MarkAllPreviousAsUnwatched, () =>
            {
                if (ser == null)
                {
                    // If we can't get a top series we fallback on what the episode tells us
                    BaseConfig.MyAnimeLog.Write("Error during watch state change - series was null!");
                    ser = episode.AnimeSeries;
                }

                if (ser?.AnimeSeriesID != null)
                {
                    VM_ShokoServer.Instance.ShokoServices.SetWatchedStatusOnSeries(ser.AnimeSeriesID, true,
                        episode.EpisodeNumber - 1, (int) ept, VM_ShokoServer.Instance.CurrentUser.JMMUserID);
                    LoadFacade();
                }
            });
            cmenu.AddAction(Translation.RateSeries, () =>
            {
                if (ser == null)
                {
                    // If we can't get a top series we fallback on what the episode tells us
                    BaseConfig.MyAnimeLog.Write("Error during rating = series was null!");
                    ser = episode.AnimeSeries;
                }

                if (ser?.AnimeSeriesID != null)
                    Utils.PromptToRateSeriesMaually(ser);
            });
            cmenu.Add(Translation.AssociateFileEpisode, () =>
            {
                List<VM_VideoLocal> unlinkedVideos = ShokoServerHelper.GetUnlinkedVideos();
                if (unlinkedVideos.Count == 0)
                {
                    this.Alert(Translation.Error, string.Empty, Translation.NoUnlinkedFilesToSelect);
                    return ContextMenuAction.Continue;
                }
                ContextMenu cfmenu = new ContextMenu(Translation.SelectFile);
                foreach (VM_VideoLocal fl in unlinkedVideos)
                {
                    VM_VideoLocal local = fl;
                    cfmenu.AddAction(fl.FileName + " - " + (fl.Places.FirstOrDefault()?.ImportFolder?.ImportFolderLocation ?? ""), () =>
                    {
                        ShokoServerHelper.LinkedFileToEpisode(local.VideoLocalID, episode.AnimeEpisodeID);
                        LoadFacade();
                    });
                }
                cfmenu.Show();
                return ContextMenuAction.Exit;
            });
            cmenu.Add(Translation.RemoveFileFromEpisode, () =>
            {
                List<VM_VideoDetailed> vidList = episode.FilesForEpisode;
                if (vidList.Count == 0)
                {
                    this.Alert(Translation.Error, string.Empty, Translation.ThisEpisodeHasNoAssociatedFiles);
                    return ContextMenuAction.Continue;
                }
                ContextMenu cfmenu = new ContextMenu(Translation.SelectFile);
                foreach (VM_VideoDetailed fl in vidList)
                {
                    VM_VideoDetailed local = fl;
                    cfmenu.AddAction(Path.GetFileName(fl.FileName), () =>
                    {
                        string res = VM_ShokoServer.Instance.ShokoServices.RemoveAssociationOnFile(local.VideoLocalID, episode.AniDB_EpisodeID);
                        if (!String.IsNullOrEmpty(res))
                            Utils.DialogMsg(Translation.Error, res);
                        LoadFacade();
                    });
                }
                cfmenu.Show();
                return ContextMenuAction.Exit;
            });
            cmenu.Add(Translation.PostProcessing + " ...", () => ShowContextMenuPostProcessing(episode.EpisodeNumberAndName));

            return cmenu.Show();
        }


        private ContextMenuAction ShowContextMenuDatabases(VM_AnimeSeries_User ser, string previousMenu)
        {
            string currentMenu = ser.SeriesName + " " + Translation.Databases;

            bool hasTvDbLink = ser.CrossRefAniDBTvDBV2.Count > 0 && ser.Anime.AniDB_AnimeCrossRefs != null && ser.Anime.AniDB_AnimeCrossRefs.TvDBCrossRefExists;
            bool hasMovieDbLink = ser.CrossRefAniDBMovieDB != null && ser.Anime.AniDB_AnimeCrossRefs != null && ser.Anime.AniDB_AnimeCrossRefs.MovieDBMovie != null;
            bool hasMalLink = ser.CrossRefAniDBMAL != null && ser.CrossRefAniDBMAL.Count > 0;

            ContextMenu cmenu = new ContextMenu(currentMenu, previousmenu: previousMenu);

            string tvdbText = Translation.SearchTheTvDB;
            string moviedbText = Translation.SearchTheMovieDB;
            string malText = Translation.SearchMAL;

            if (hasTvDbLink && ser.Anime.AniDB_AnimeCrossRefs != null)
                tvdbText += string.Format(" ({0}: {1})", Translation.Current, ser.Anime.AniDB_AnimeCrossRefs.TvDBSeries[0].SeriesName);

            if (hasMovieDbLink && ser.Anime.AniDB_AnimeCrossRefs != null)
                moviedbText += string.Format(" ({0}: {1})", Translation.Current, ser.Anime.AniDB_AnimeCrossRefs.MovieDBMovie.MovieName);

            if (hasMalLink)
                malText += string.Format(" ({0}: {1})", Translation.Current, ser.CrossRefAniDBMAL.Count == 1 ? ser.CrossRefAniDBMAL[0].MALTitle : Translation.MultipleLinks);

            if (!hasMovieDbLink)
            {
                cmenu.Add(tvdbText, () => SearchTheTvDBMenu(ser, currentMenu));
                cmenu.Add(malText, () => SearchMALMenu(ser, currentMenu));
            }

            if (!hasTvDbLink)
                cmenu.Add(moviedbText, () => SearchTheMovieDBMenu(ser, currentMenu));
            if (hasTvDbLink)
                cmenu.Add(Translation.TheTVDB + " ...", () => ShowContextMenuTVDB(ser, currentMenu));
            if (hasMalLink)
                cmenu.Add(Translation.MAL + " ...", () => ShowContextMenuMAL(ser, currentMenu));
            if (hasMovieDbLink)
                cmenu.Add(Translation.TheMovieDB + " ...", () => ShowContextMenuMovieDB(ser, currentMenu));
            return cmenu.Show();
        }

        private ContextMenuAction ShowContextMenuGroupEdit(string previousMenu)
        {
            GUIListItem currentitem = m_Facade.SelectedListItem;

            VM_AnimeGroup_User grp = currentitem?.TVTag as VM_AnimeGroup_User;
            if (grp == null)
                return ContextMenuAction.Exit;


            string currentMenu = grp.GroupName + " - " + Translation.Edit;
            ContextMenu cmenu = new ContextMenu(currentMenu, previousmenu: previousMenu);
            cmenu.Add(Translation.RenameGroup, () =>
            {
                string name = grp.GroupName;
                if (Utils.DialogText(ref name, GetID) && name != string.Empty)
                {
                    if (name != grp.GroupName)
                    {
                        grp.GroupName = name;
                        grp.SortName = Utils.GetSortName(name);
                        grp.Save();
                        LoadFacade();
                    }
                    return ContextMenuAction.Exit;
                }
                return ContextMenuAction.Continue;
            });
            cmenu.Add(Translation.ChangeSortName, () =>
            {
                string sortName = grp.SortName;
                if (Utils.DialogText(ref sortName, GetID))
                {
                    grp.SortName = sortName;
                    grp.Save();
                    LoadFacade();
                    return ContextMenuAction.Exit;
                }
                return ContextMenuAction.Continue;
            });
            List<VM_AnimeSeries_User> allSeries = grp.AllSeries;
            VM_AnimeSeries_User equalSeries = null;
            if (allSeries.Count == 1)
                equalSeries = allSeries[0];
            if (equalSeries != null)
            {
                cmenu.Add(Translation.SetDefaultAudioLanguage, () =>
                {
                    String language = equalSeries.DefaultAudioLanguage;
                    if (Utils.DialogLanguage(ref language, false))
                    {
                        equalSeries.DefaultAudioLanguage = language;
                        equalSeries.Save();
                        return ContextMenuAction.Exit;
                    }
                    return ContextMenuAction.Continue;
                });
                cmenu.Add(Translation.SetDefaultSubtitleLanguage, () =>
                {
                    String language = equalSeries.DefaultSubtitleLanguage;
                    if (Utils.DialogLanguage(ref language, true))
                    {
                        equalSeries.DefaultSubtitleLanguage = language;
                        equalSeries.Save();
                        return ContextMenuAction.Exit;
                    }
                    return ContextMenuAction.Continue;
                });
            }
            if (grp.DefaultAnimeSeriesID.HasValue)
                cmenu.Add(Translation.RemoveDefaultSeries, () =>
                    // ReSharper restore ImplicitlyCapturedClosure
                {
                    VM_ShokoServer.Instance.ShokoServices.RemoveDefaultSeriesForGroup(grp.AnimeGroupID);
                    grp.DefaultAnimeSeriesID = null;
                    return ContextMenuAction.Continue;
                });
            if (allSeries.Count > 1)
                cmenu.Add(Translation.SetDefaultSeries, () =>
                    // ReSharper restore ImplicitlyCapturedClosure
                {
                    VM_AnimeSeries_User ser = null;
                    if (Utils.DialogSelectSeries(ref ser, allSeries))
                        if (ser.AnimeSeriesID != 0)
                        {
                            grp.DefaultAnimeSeriesID = ser.AnimeSeriesID;
                            VM_ShokoServer.Instance.ShokoServices.SetDefaultSeriesForGroup(grp.AnimeGroupID, ser.AnimeSeriesID);
                        }
                    return ContextMenuAction.Continue;
                });
            // ReSharper disable ImplicitlyCapturedClosure
            cmenu.Add(Translation.DeleteThisGroupSeriesEpisodes, () =>
                // ReSharper restore ImplicitlyCapturedClosure
            {
                if (Utils.DialogConfirm(Translation.AreYouSure))
                {
                    VM_ShokoServer.Instance.ShokoServices.DeleteAnimeGroup(grp.AnimeGroupID, false);
                    LoadFacade();
                    return ContextMenuAction.Exit;
                }
                return ContextMenuAction.Continue;
            });
            return cmenu.Show();
        }
        /*
        private void CheckForEmptyList()
        {
            // If we have no items in list anymore navigate back so we don't end up with empty list view
            // Mostly for mark as watched actions
            BaseConfig.MyAnimeLog.Write("Facade count: " + m_Facade.Count);
            GUIListItem currentitem = m_Facade.SelectedListItem;

            if (m_Facade.Count == 0)
            {
                if (searchTimer != null && searchTimer.Enabled)
                {
                    OnSearchAction(SearchAction.EndSearch);
                    return;
                }

                try
                {
                    Breadcrumbs.Remove(Breadcrumbs[Breadcrumbs.Count - 1]);
                }
                catch (Exception)
                {
                }
                LoadFacade();
            }
        }
        */
        /*
        private bool ShowContextMenuSeriesInfo(string previousMenu)
        {
          GUIListItem currentitem = this.m_Facade.SelectedListItem;
          if (currentitem == null)
            return true;
        
          GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
          if (dlg == null)
            return true;
        
          //keep showing the dialog until the user closes it
          int selectedLabel = 0;
          string currentMenu = "Series Information";
          while (true)
          {
            dlg.Reset();
            dlg.SetHeading(currentMenu);
        
            if (previousMenu != string.Empty)
              dlg.Add("<<< " + previousMenu);
            dlg.Add("Characters/Actors");
            dlg.Add("Related Series");
        
            dlg.SelectedLabel = selectedLabel;
            dlg.DoModal(GUIWindowManager.ActiveWindow);
            selectedLabel = dlg.SelectedLabel;
        
            int selection = selectedLabel + ((previousMenu == string.Empty) ? 1 : 0);
            switch (selection)
            {
              case 0:
                //show previous
                return true;
              case 1:
                ShowCharacterWindow();
                return false;
              case 2:
                ShowRelationsWindow();
                return false;
              default:
                //close menu
                return false;
            }
          }
        }
            */
        private ContextMenuAction ShowContextMenuGroupFilter(string previousMenu)
        {
            GUIListItem currentitem = m_Facade.SelectedListItem;
            if (currentitem == null)
                return ContextMenuAction.Exit;

            VM_GroupFilter gf = currentitem.TVTag as VM_GroupFilter;
            if (gf == null)
                return ContextMenuAction.Exit;

            ContextMenu cmenu = new ContextMenu(gf.GroupFilterName, previousmenu: previousMenu);
            cmenu.AddAction(Translation.RandomSeries, () =>
            {
                RandomWindow_CurrentEpisode = null;
                RandomWindow_CurrentSeries = null;
                RandomWindow_LevelObject = gf;
                RandomWindow_RandomLevel = RandomSeriesEpisodeLevel.GroupFilter;
                RandomWindow_RandomType = RandomObjectType.Series;
                GUIWindowManager.ActivateWindow(Constants.WindowIDs.RANDOM);
            });
            cmenu.AddAction(Translation.RandomEpisode, () =>
            {
                RandomWindow_CurrentEpisode = null;
                RandomWindow_CurrentSeries = null;
                RandomWindow_LevelObject = gf;
                RandomWindow_RandomLevel = RandomSeriesEpisodeLevel.GroupFilter;
                RandomWindow_RandomType = RandomObjectType.Episode;
                GUIWindowManager.ActivateWindow(Constants.WindowIDs.RANDOM);
            });
            return cmenu.Show();
        }

        private ContextMenuAction ShowContextMenuGroup(string previousMenu)
        {
            GUIListItem currentitem = m_Facade.SelectedListItem;
            if (currentitem == null)
                return ContextMenuAction.Exit;
            VM_AnimeGroup_User grp = currentitem.TVTag as VM_AnimeGroup_User;
            if (grp == null)
                return ContextMenuAction.Exit;

            ContextMenu cmenu = new ContextMenu(grp.GroupName, previousmenu: previousMenu);

            if (grp.SubGroups.Count == 0)
                cmenu.AddAction(grp.IsFave == 1 ? Translation.RemoveFromFavorites : Translation.AddToFavorites, () =>
                {
                    grp.IsFave = grp.IsFave == 1 ? 0 : 1;
                    grp.Save();
                    EvaluateVisibility();
                });

            cmenu.AddAction(Translation.MarkAllAsWatched, () =>
            {
                foreach (VM_AnimeSeries_User ser in grp.AllSeries)
                    if (ser.AnimeSeriesID != 0)
                        ShokoServerHelper.SetWatchedStatusOnSeries(true, Int32.MaxValue, ser.AnimeSeriesID);
                LoadFacade();
            });
            cmenu.AddAction(Translation.MarkAllAsUnwatched, () =>
            {
                foreach (VM_AnimeSeries_User ser in grp.AllSeries)
                    if (ser.AnimeSeriesID != 0)
                        ShokoServerHelper.SetWatchedStatusOnSeries(false, Int32.MaxValue, ser.AnimeSeriesID);
                LoadFacade();
            });
            if (grp.AllSeries.Count == 1)
                cmenu.AddAction(Translation.SeriesInformation, () => ShowAnimeInfoWindow(grp.AllSeries[0]));
            History h = GetCurrent();
            if (h.Listing is VM_GroupFilter)
            {
                VM_GroupFilter gf = (VM_GroupFilter) h.Listing;
                cmenu.Add(Translation.QuickSort + " ...", () =>
                {
                    string sortType = "";
                    GroupFilterSortDirection sortDirection = GroupFilterSortDirection.Asc;
                    if (gf.GroupFilterID != 0)
                    {
                        if (GroupFilterQuickSorts.ContainsKey(gf.GroupFilterID))
                            sortDirection = GroupFilterQuickSorts[gf.GroupFilterID].SortDirection;
                        if (!Utils.DialogSelectGFQuickSort(ref sortType, ref sortDirection, gf.GroupFilterName))
                        {
                            if (!GroupFilterQuickSorts.ContainsKey(gf.GroupFilterID))
                                GroupFilterQuickSorts[gf.GroupFilterID] = new QuickSort();
                            GroupFilterQuickSorts[gf.GroupFilterID].SortType = sortType;
                            GroupFilterQuickSorts[gf.GroupFilterID].SortDirection = sortDirection;
                            LoadFacade();
                            return ContextMenuAction.Exit;
                        }
                    }
                    return ContextMenuAction.Continue;
                });
                if (gf.GroupFilterID != 0 && GroupFilterQuickSorts.ContainsKey(gf.GroupFilterID))
                    cmenu.AddAction(Translation.RemoveQuickSort, () =>
                    {
                        GroupFilterQuickSorts.Remove(gf.GroupFilterID);
                        LoadFacade();
                    });
            }

            // ReSharper disable ImplicitlyCapturedClosure
            cmenu.AddAction(Translation.RandomSeries, () =>
                // ReSharper restore ImplicitlyCapturedClosure
            {
                RandomWindow_CurrentEpisode = null;
                RandomWindow_CurrentSeries = null;
                RandomWindow_LevelObject = grp;
                RandomWindow_RandomLevel = RandomSeriesEpisodeLevel.Group;
                RandomWindow_RandomType = RandomObjectType.Series;
                GUIWindowManager.ActivateWindow(Constants.WindowIDs.RANDOM);
            });
            // ReSharper disable ImplicitlyCapturedClosure
            cmenu.AddAction(Translation.RandomEpisode, () =>
                // ReSharper restore ImplicitlyCapturedClosure
            {
                RandomWindow_CurrentEpisode = null;
                RandomWindow_CurrentSeries = null;
                RandomWindow_LevelObject = grp;
                RandomWindow_RandomLevel = RandomSeriesEpisodeLevel.Group;
                RandomWindow_RandomType = RandomObjectType.Episode;
                GUIWindowManager.ActivateWindow(Constants.WindowIDs.RANDOM);
            });

            cmenu.Add(Translation.AdvancedOptions + " ...", () => ShowContextMenuAdvancedOptionsGroup(grp.GroupName));

            return cmenu.Show();
        }

        private ContextMenuAction ShowContextMenuAdvancedOptionsGroup(string previousMenu)
        {
            GUIListItem currentitem = m_Facade.SelectedListItem;
            if (currentitem == null)
                return ContextMenuAction.Exit;
            VM_AnimeGroup_User grp = currentitem.TVTag as VM_AnimeGroup_User;
            if (grp == null)
                return ContextMenuAction.Exit;

            ContextMenu cmenu = new ContextMenu(Translation.AdvancedOptions, previousmenu: previousMenu);
            cmenu.Add(Translation.EditGroup + " ...", () => ShowContextMenuGroupEdit(grp.GroupName));
            if (grp.AllSeries.Count == 1)
            {
                cmenu.Add(Translation.Databases + " ...",
                    () => ShowContextMenuDatabases(grp.AllSeries[0], Translation.GroupMenu));
                cmenu.Add(Translation.Images + " ...", () => ShowContextMenuImages(grp.GroupName));
            }
            cmenu.Add(Translation.PostProcessing + " ...", () => ShowContextMenuPostProcessing(grp.GroupName));

            return cmenu.Show();
        }
        /*
        private void ShowRelationsWindow()
        {
            SetGlobalIDs();
            GUIWindowManager.ActivateWindow(Constants.WindowIDs.RELATIONS, false);
        }

        private void ShowCharacterWindow()
        {
            SetGlobalIDs();
            GUIWindowManager.ActivateWindow(Constants.WindowIDs.CHARACTERS, false);
        }
        */
        private void ShowAnimeInfoWindow(VM_AnimeSeries_User ser)
        {
            if (ser != null)
                GlobalSeriesID = ser.AnimeSeriesID;

            GUIWindowManager.ActivateWindow(Constants.WindowIDs.ANIMEINFO, false);
        }

        private ContextMenuAction ShowContextMenuImages(string previousMenu)
        {
            GUIListItem currentitem = m_Facade.SelectedListItem;
            if (currentitem == null)
                return ContextMenuAction.Exit;

            VM_AnimeSeries_User ser = currentitem.TVTag as VM_AnimeSeries_User;


            string displayName;
            if (ser == null)
            {
                VM_AnimeGroup_User grp = currentitem.TVTag as VM_AnimeGroup_User;
                if (grp == null)
                    return ContextMenuAction.Exit;

                displayName = grp.GroupName;
                GlobalSeriesID = grp.AllSeries[0].AnimeSeriesID;
            }
            else
            {
                displayName = ser.SeriesName;
                GlobalSeriesID = ser.AnimeSeriesID;
            }

            ContextMenu cmenu = new ContextMenu(displayName, previousmenu: previousMenu);
            cmenu.AddAction(Translation.Fanart, ShowFanartWindow);
            cmenu.AddAction(Translation.Posters, ShowPostersWindow);
            cmenu.AddAction(Translation.WideBanners, ShowWideBannersWindow);
            return cmenu.Show();
        }


        private ContextMenuAction ShowContextMenuTVDB(VM_AnimeSeries_User ser, string previousMenu)
        {
            GUIListItem currentitem = m_Facade.SelectedListItem;
            if (currentitem == null)
                return ContextMenuAction.Exit;

            int tvdbid;
            int season;
            string displayName;

            if (ser.CrossRefAniDBTvDBV2.Count > 0 && ser.TvDB_Series.Count > 0 && ser.Anime.AniDB_AnimeCrossRefs != null && ser.Anime.AniDB_AnimeCrossRefs.TvDBCrossRefExists)
            {
                displayName = ser.Anime.AniDB_AnimeCrossRefs.TvDBSeries[0].SeriesName;
                tvdbid = ser.CrossRefAniDBTvDBV2[0].TvDBID;
                season = ser.CrossRefAniDBTvDBV2[0].TvDBSeasonNumber;
            }
            else
            {
                return ContextMenuAction.PreviousMenu;
            }
            ContextMenu cmenu = new ContextMenu(displayName, previousmenu: previousMenu);
            // ReSharper disable ImplicitlyCapturedClosure
            cmenu.AddAction(Translation.RemoveTVDBAssociation, () => VM_ShokoServer.Instance.ShokoServices.RemoveLinkAniDBTvDBForAnime(ser.Anime.AnimeID));
            // ReSharper restore ImplicitlyCapturedClosure
            cmenu.Add(string.Format(Translation.SwitchSeason, season.ToString(Globals.Culture)), () =>
            {
                if (ser.CrossRefAniDBTvDBV2.Count < 2)
                    return ShowSeasonSelectionMenuTvDB(ser, ser.Anime.AnimeID, tvdbid, displayName);
                Utils.DialogMsg(Translation.Error, Translation.CannotEditTvDBLink);
                return ContextMenuAction.Continue;
            });
            return cmenu.Show();
        }

        /*
        private ContextMenuAction ShowContextMenuTrakt(AnimeSeriesVM ser, string previousMenu)
        {
            GUIListItem currentitem = m_Facade.SelectedListItem;
            if (currentitem == null)
                return ContextMenuAction.Exit;
    
    
    
            string traktId;
            int season;
            string displayName;
    
            if (ser.AniDB_Anime.AniDB_AnimeCrossRefs != null && ser.AniDB_Anime.AniDB_AnimeCrossRefs.CrossRef_AniDB_Trakt != null
                && ser.AniDB_Anime.AniDB_AnimeCrossRefs.TraktShow != null)
            {
                displayName = ser.AniDB_Anime.AniDB_AnimeCrossRefs.TraktShow.Title;
                traktId = ser.AniDB_Anime.AniDB_AnimeCrossRefs.CrossRef_AniDB_Trakt.TraktID;
                season = ser.AniDB_Anime.AniDB_AnimeCrossRefs.CrossRef_AniDB_Trakt.TraktSeasonNumber;
            }
            else
                return ContextMenuAction.PreviousMenu;
    
            ContextMenu cmenu = new ContextMenu(displayName, previousmenu: previousMenu);
            // ReSharper disable ImplicitlyCapturedClosure
            cmenu.AddAction(Translation.RemoveTraktAssociation, () => JMMServerVM.Instance.clientBinaryHTTP.RemoveLinkAniDBTrakt(ser.AniDB_Anime.AnimeID));
            // ReSharper restore ImplicitlyCapturedClosure
            cmenu.Add(string.Format(Translation.SwitchSeason, season.ToString(Globals.Culture)), () => ShowSeasonSelectionMenuTrakt(ser, ser.AniDB_Anime.AnimeID, traktId, displayName));
            return cmenu.Show();
        }
        */
        private ContextMenuAction ShowContextMenuMovieDB(VM_AnimeSeries_User ser, string previousMenu)
        {
            GUIListItem currentitem = m_Facade.SelectedListItem;
            if (currentitem == null)
                return ContextMenuAction.Exit;

            string displayName;

            if (ser.CrossRefAniDBMovieDB != null && ser.Anime.AniDB_AnimeCrossRefs != null &&
                ser.Anime.AniDB_AnimeCrossRefs.MovieDBMovie != null)
                displayName = ser.Anime.AniDB_AnimeCrossRefs.MovieDBMovie.MovieName;
            else
                return ContextMenuAction.PreviousMenu;
            ContextMenu cmenu = new ContextMenu(displayName, previousMenu);
            cmenu.AddAction(Translation.RemoveMovieDBAssociation, () => VM_ShokoServer.Instance.ShokoServices.RemoveLinkAniDBOther(ser.Anime.AnimeID, (int) CrossRefType.MovieDB));
            return cmenu.Show();
        }

        private ContextMenuAction ShowContextMenuMAL(VM_AnimeSeries_User ser, string previousMenu)
        {
            GUIListItem currentitem = m_Facade.SelectedListItem;
            if (currentitem == null)
                return ContextMenuAction.Exit;
            string displayName;

            if (ser.CrossRefAniDBMAL != null && ser.CrossRefAniDBMAL.Count > 0)
                displayName = ser.CrossRefAniDBMAL.Count == 1
                    ? ser.CrossRefAniDBMAL[0].MALTitle
                    : Translation.MultipleLinks;
            else
                return ContextMenuAction.PreviousMenu;
            ContextMenu cmenu = new ContextMenu(displayName, previousmenu: previousMenu);
            cmenu.AddAction(Translation.RemoveMALAssociation, () =>
            {
                foreach (CrossRef_AniDB_MAL xref in ser.CrossRefAniDBMAL)
                    VM_ShokoServer.Instance.ShokoServices.RemoveLinkAniDBMAL(xref.AnimeID, xref.StartEpisodeType, xref.StartEpisodeNumber);
            });
            return cmenu.Show();
        }

        private ContextMenuAction ShowSeasonSelectionMenuTvDB(VM_AnimeSeries_User ser, int animeID, int tvdbid, string previousMenu)
        {
            try
            {
                List<int> seasons = new List<int>(VM_ShokoServer.Instance.ShokoServices.GetSeasonNumbersForSeries(tvdbid));
                if (seasons.Count == 0)
                {
                    this.Alert(Translation.SeasonResults, string.Empty, Translation.NoSeasonsFound);
                    return ContextMenuAction.Exit;
                }
                ContextMenu cmenu = new ContextMenu(Translation.SelectSeason, previousmenu: previousMenu);
                foreach (int season in seasons)
                {
                    int local = season;
                    cmenu.AddAction(Translation.Season + " " + season.ToString(Globals.Culture), () =>
                    {
                        VM_ShokoServer.Instance.ShokoServices.RemoveLinkAniDBTvDBForAnime(animeID);
                        LinkAniDBToTVDB(ser, animeID, EpisodeType.Episode, 1, tvdbid, local, 1);
                    });
                }
                return cmenu.Show();
            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write("Error in ShowSeasonSelectionMenu:: {0}", ex);
            }

            return ContextMenuAction.Exit;
        }

        private ContextMenuAction ShowContextMenuSeriesEdit(string previousMenu)
        {
            GUIListItem currentitem = m_Facade.SelectedListItem;
            if (currentitem == null)
                return ContextMenuAction.Exit;

            VM_AnimeSeries_User ser = currentitem.TVTag as VM_AnimeSeries_User;
            if (ser == null)
                return ContextMenuAction.Exit;

            string currentMenu = ser.SeriesName + " - " + Translation.Edit;
            ContextMenu cmenu = new ContextMenu(currentMenu, previousmenu: previousMenu);
            // ReSharper disable ImplicitlyCapturedClosure
            cmenu.Add(Translation.SetDefaultAudioLanguage, () =>
                // ReSharper restore ImplicitlyCapturedClosure
            {
                string language = ser.DefaultAudioLanguage;
                if (Utils.DialogLanguage(ref language, false))
                {
                    ser.DefaultAudioLanguage = language;
                    ser.Save();
                    return ContextMenuAction.Exit;
                }
                return ContextMenuAction.Continue;
            });
            // ReSharper disable ImplicitlyCapturedClosure
            cmenu.Add(Translation.SetDefaultSubtitleLanguage, () =>
                // ReSharper restore ImplicitlyCapturedClosure
            {
                string language = ser.DefaultSubtitleLanguage;
                if (Utils.DialogLanguage(ref language, true))
                {
                    ser.DefaultSubtitleLanguage = language;
                    ser.Save();
                    return ContextMenuAction.Exit;
                }
                return ContextMenuAction.Continue;
            });
            cmenu.Add(Translation.DeleteThisSeriesEpisodes, () =>
            {
                if (ser.AnimeSeriesID != 0)
                    if (Utils.DialogConfirm(Translation.AreYouSure))
                    {
                        VM_ShokoServer.Instance.ShokoServices.DeleteAnimeSeries(ser.AnimeSeriesID, false, false);
                        LoadFacade();
                        return ContextMenuAction.Exit;
                    }
                return ContextMenuAction.Continue;
            });
            return cmenu.Show();
        }

        private ContextMenuAction ShowContextMenuSeries(string previousMenu)
        {
            GUIListItem currentitem = m_Facade.SelectedListItem;
            if (currentitem == null)
                return ContextMenuAction.Exit;

            VM_AnimeSeries_User ser = currentitem.TVTag as VM_AnimeSeries_User;
            if (ser == null)
                return ContextMenuAction.Exit;

            ContextMenu cmenu = new ContextMenu(ser.SeriesName, previousmenu: previousMenu);
            cmenu.AddAction(Translation.SeriesInformation, () => ShowAnimeInfoWindow(ser));
            cmenu.AddAction(Translation.MarkAllAsWatched, () =>
            {
                if (ser.AnimeSeriesID != 0)
                {
                    ShokoServerHelper.SetWatchedStatusOnSeries(true, Int32.MaxValue, ser.AnimeSeriesID);
                    VM_AnimeSeries_User serTemp = (VM_AnimeSeries_User) VM_ShokoServer.Instance.ShokoServices.GetSeries(ser.AnimeSeriesID, VM_ShokoServer.Instance.CurrentUser.JMMUserID);
                    if (serTemp != null)
                        Utils.PromptToRateSeriesOnCompletion(serTemp);
                    LoadFacade();
                }
            });
            cmenu.AddAction(Translation.MarkAllAsUnwatched, () =>
            {
                if (ser.AnimeSeriesID != 0)
                {
                    ShokoServerHelper.SetWatchedStatusOnSeries(false, Int32.MaxValue, ser.AnimeSeriesID);
                    LoadFacade();
                }
            });
            cmenu.AddAction(Translation.RateSeries, () =>
            {

                if (ser.AnimeSeriesID != 0)
                    Utils.PromptToRateSeriesMaually(ser);
            });
            cmenu.Add(Translation.Databases + " ...", () => ShowContextMenuDatabases(ser, ser.SeriesName));
            cmenu.Add(Translation.Images + " ...", () => ShowContextMenuImages(ser.SeriesName));
            cmenu.Add(Translation.EditSeries + " ...", () => ShowContextMenuSeriesEdit(ser.SeriesName));
            // ReSharper disable ImplicitlyCapturedClosure
            cmenu.AddAction(Translation.RandomEpisode, () =>
                // ReSharper restore ImplicitlyCapturedClosure
            {
                RandomWindow_CurrentEpisode = null;
                RandomWindow_CurrentSeries = null;
                RandomWindow_LevelObject = ser;
                RandomWindow_RandomLevel = RandomSeriesEpisodeLevel.Series;
                RandomWindow_RandomType = RandomObjectType.Episode;
                GUIWindowManager.ActivateWindow(Constants.WindowIDs.RANDOM);
            });
            cmenu.Add(Translation.PostProcessing + " ...", () => ShowContextMenuPostProcessing(ser.SeriesName));
            return cmenu.Show();
        }

        private ContextMenuAction ShowContextMenuPostProcessing(string previousMenu)
        {
            GUIListItem currentitem = m_Facade.SelectedListItem;
            if (currentitem == null)
                return ContextMenuAction.Exit;

            VM_AnimeGroup_User grp = currentitem.TVTag as VM_AnimeGroup_User;
            List<VM_AnimeEpisode_User> episodes = new List<VM_AnimeEpisode_User>();
            if (grp == null)
            {
                VM_AnimeSeries_User ser = currentitem.TVTag as VM_AnimeSeries_User;
                if (ser == null)
                {
                    VM_AnimeEpisode_User ep = currentitem.TVTag as VM_AnimeEpisode_User;
                    episodes.Add(ep);
                }
                else
                {
                    foreach (VM_AnimeEpisode_User ep in ser.AllEpisodes)
                        episodes.Add(ep);
                }
            }
            else
            {
                List<VM_AnimeSeries_User> seriesList = grp.AllSeries;
                foreach (VM_AnimeSeries_User ser in seriesList)
                foreach (VM_AnimeEpisode_User ep in ser.AllEpisodes)
                    episodes.Add(ep);
            }

            //keep showing the dialog until the user closes it
            /*string currentMenu = "Associate with a ffdshow raw preset";
            int selectedLabel = 0;
            int intLabel = 0;
            */
            FFDShowHelper ffdshowHelper = new FFDShowHelper();
            List<string> presets = ffdshowHelper.Presets;

            string selectedPreset = ffdshowHelper.findSelectedPresetForMenu(episodes);


            ContextMenu cmenu = new ContextMenu(Translation.AssociateFFDshowPreset, previousmenu: previousMenu);
            cmenu.AddAction(Translation.RemoveOldPresetAssociation, () =>
            {
                ffdshowHelper.deletePreset(episodes);
                Utils.DialogMsg(Translation.Confirmation, Translation.OldPresetRemove);
            });
            foreach (string preset in presets)
            {
                string local = preset;
                cmenu.AddAction(Translation.SetPreset + ": " + preset, () =>
                {
                    ffdshowHelper.addPreset(episodes, local);
                    Utils.DialogMsg(Translation.Confirmation, string.Format(Translation.PresetAdded, local));
                }, local == selectedPreset);
            }
            return cmenu.Show();
        }

        private void SetGlobalIDs()
        {
            GlobalSeriesID = -1;
            VM_AnimeGroup_User grp = GetTopGroup();

            if (grp == null)
                return;

            VM_AnimeSeries_User ser = null;
            List<VM_AnimeSeries_User> allSeries = grp.AllSeries;
            if (allSeries.Count == 1)
                ser = allSeries[0];
            if (ser == null) return;

            GlobalSeriesID = ser.AnimeSeriesID;
        }

        private void ShowFanartWindow()
        {
            GUIWindowManager.ActivateWindow(Constants.WindowIDs.FANART, false);
        }

        private void ShowPostersWindow()
        {
            GUIWindowManager.ActivateWindow(Constants.WindowIDs.POSTERS, false);
        }

        private void ShowWideBannersWindow()
        {
            GUIWindowManager.ActivateWindow(Constants.WindowIDs.WIDEBANNERS, false);
        }
    }


    enum conMenuActionGroup
    {
        ToggleAsFave = 1,
        TvDBSearch = 2,
        TvDB_Main = 3
    }

    public enum groupSort
    {
        Name = 1,
        AniDBRating = 2,
        UserRating = 3,
        AirDate = 4,
        WatchedDate = 5
    }

    enum conMenuActionTvDB
    {
        TvDB_Fanart = 1,
        TvDB_DownloadWideBanners = 2,
        TvDB_DownloadPosters = 3,
        TvDB_SwitchSeason = 4,
        TvDB_EpisodeInfo = 5
    }

    /*
  public enum Listlevel
  {
    Episode = 0, // The actual episodes
    EpisodeTypes = 1, // Normal, Credits, Specials
    GroupAndSeries = 2, // Da capo, Da Capo S2, Da Capo II etc
    Group = 3, // Da Capo
    GroupFilter = 4, // Favouritess
  }
    */
    public enum ViewType
    {
        All = 0,
        Faves = 1,
        FavesUnwatched = 2,
        Genre = 3,
        Year = 4,
        CompleteSeries = 5,
        NewSeason = 6
    }

    public enum ViewClassification
    {
        Views = 0,
        StaticYears = 1,
        StaticGenres = 2
    }

    enum BackGroundLoadingArgumentType
    {
        None,
        DelayedImgLoading,
        DelayedImgInit,
        ElementSelection,
        SetFacadeMode,
        ListFullElement,
        ListElementForDelayedImgLoading
    }

    public enum AniDBMyListSyncMode
    {
        AniDB_All = 1, // Take watched/unwatched status from AniDB
        AniDB_WatchedOnly = 2 // Only take watched status from AniDB, update AniDB if local episode is watched 
    }

    class BackgroundFacadeLoadingArgument
    {
        public BackGroundLoadingArgumentType Type = BackGroundLoadingArgumentType.None;

        public object Argument;
        public int IndexArgument;
    }
}