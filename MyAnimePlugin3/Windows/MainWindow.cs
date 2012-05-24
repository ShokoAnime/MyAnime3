using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ConsoleApplication2.com.amazon.webservices;
using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
using MyAnimePlugin3.ConfigFiles;
using MyAnimePlugin3.DataHelpers;
using System.IO;
using Cornerstone.MP;
using System.ComponentModel;
using System.Threading;
using System.Xml;
using BinaryNorthwest;
using System.Runtime.InteropServices;
using MyAnimePlugin3.Downloads;
using Action = MediaPortal.GUI.Library.Action;
using System.Collections;
using MyAnimePlugin3.ViewModel;
using MyAnimePlugin3.ImageManagement;
using MyAnimePlugin3.MultiSortLib;
using MediaPortal.Player;

namespace MyAnimePlugin3
{
	public class MainWindow : GUIWindow, ISetupForm
	{
		#region GUI Controls

		[SkinControlAttribute(2)]
		protected GUIButtonControl btnDisplayOptions = null;
		//[SkinControlAttribute(3)] protected GUIButtonControl btnLayout = null;
		[SkinControlAttribute(4)]
		protected GUIButtonControl btnSettings = null;
		[SkinControlAttribute(11)]
		protected GUIButtonControl btnChangeLayout = null;
		[SkinControlAttribute(12)]
		protected GUIButtonControl btnSwitchUser = null;

		[SkinControlAttribute(920)] protected GUIButtonControl btnWindowContinueWatching = null;
		[SkinControlAttribute(921)] protected GUIButtonControl btnWindowUtilities = null;
		[SkinControlAttribute(922)] protected GUIButtonControl btnWindowCalendar = null;
		[SkinControlAttribute(923)] protected GUIButtonControl btnWindowDownloads = null;
		//[SkinControlAttribute(924)] protected GUIButtonControl btnWindowCollectionStats = null;
		[SkinControlAttribute(925)] protected GUIButtonControl btnWindowRecommendations = null;
		[SkinControlAttribute(926)] protected GUIButtonControl btnWindowRandom = null;
		[SkinControlAttribute(927)] protected GUIButtonControl btnWindowPlaylists = null;

		[SkinControlAttribute(50)]
		protected GUIFacadeControl m_Facade = null;
		//[SkinControlAttribute(51)]
		//protected GUIListControl test = null;

		//[SkinControlAttribute(526)] protected GUIImage loadingImage = null;

		// let the skins react to what we are displaying
		// all these controls are imported from Anime3_Dummy.xml
		[SkinControlAttribute(1232)]
		protected GUILabelControl dummyIsFanartLoaded = null;
		[SkinControlAttribute(1233)]
		protected GUILabelControl dummyIsDarkFanartLoaded = null;
		[SkinControlAttribute(1234)]
		protected GUILabelControl dummyIsLightFanartLoaded = null;
		[SkinControlAttribute(1235)]
		protected GUILabelControl dummyLayoutListMode = null;
		[SkinControlAttribute(1236)]
		protected GUILabelControl dummyLayoutFilmstripMode = null;
		[SkinControlAttribute(1242)]
		protected GUILabelControl dummyLayoutWideBanners = null;

		[SkinControlAttribute(1237)]
		protected GUILabelControl dummyIsSeries = null;
		[SkinControlAttribute(1238)]
		protected GUILabelControl dummyIsGroups = null;
		[SkinControlAttribute(1250)]
		protected GUILabelControl dummyIsGroupFilters = null;
		[SkinControlAttribute(1239)]
		protected GUILabelControl dummyIsEpisodes = null;
		[SkinControlAttribute(1240)]
		protected GUILabelControl dummyIsEpisodeTypes = null;

		[SkinControlAttribute(1241)]
		protected GUILabelControl dummyIsFanartColorAvailable = null;

		[SkinControlAttribute(1243)]
		protected GUILabelControl dummyIsWatched = null;
		[SkinControlAttribute(1244)]
		protected GUILabelControl dummyIsAvailable = null;

		[SkinControlAttribute(1245)]
		protected GUILabelControl dummyFave = null;
		[SkinControlAttribute(1246)]
		protected GUILabelControl dummyMissingEps = null;
		[SkinControlAttribute(1247)]
		protected GUILabelControl dummyUserHasVotedSeries = null;

		[SkinControlAttribute(3401)]
		protected GUILabelControl dummyQueueAniDB = null;
		[SkinControlAttribute(3402)]
		protected GUILabelControl dummyQueueHasher = null;
		[SkinControlAttribute(3403)]
		protected GUILabelControl dummyQueueImages = null;

		[SkinControlAttribute(3463)]
		protected GUIControl dummyFindActive = null;
		[SkinControlAttribute(3464)]
		protected GUIControl dummyFindModeT9 = null;
		[SkinControlAttribute(3465)]
		protected GUIControl dummyFindModeText = null;

		#endregion

		public static Listlevel listLevel = Listlevel.GroupFilter;
		public static object parentLevelObject = null;
		private static Random groupRandom = new Random();

		public static RandomSeriesEpisodeLevel RandomWindow_RandomLevel = RandomSeriesEpisodeLevel.All;
		public static RandomObjectType RandomWindow_RandomType = RandomObjectType.Series;
		public static object RandomWindow_LevelObject = null;
		public static AnimeSeriesVM RandomWindow_CurrentSeries = null;
		public static AnimeEpisodeVM RandomWindow_CurrentEpisode = null;
		public static int RandomWindow_MatchesFound = 0;

		public static bool RandomWindow_SeriesWatched = true;
		public static bool RandomWindow_SeriesUnwatched = true;
		public static bool RandomWindow_SeriesPartiallyWatched = true;
		public static bool RandomWindow_SeriesOnlyComplete = true;
		public static bool RandomWindow_SeriesAllCategories = true;
		public static string RandomWindow_SeriesCategories = "";

		public static bool RandomWindow_EpisodeWatched = true;
		public static bool RandomWindow_EpisodeUnwatched = true;
		public static bool RandomWindow_EpisodeAllCategories = true;
		public static string RandomWindow_EpisodeCategories = "";

		//private bool fanartSet = false;

		private readonly int artworkDelay = 5;
		private System.Timers.Timer displayGrpFilterTimer = null;
		private System.Timers.Timer displayGrpTimer = null;

		public static int? animeSeriesIDToBeRated = null;

		public static AnimePluginSettings settings = null;
		public static JMMServerHelper ServerHelper = new JMMServerHelper();

		public static List<string> completedTorrents = new List<string>();
		public static DownloadSearchCriteria currentDownloadSearch = null;
		public static List<DownloadSearchCriteria> downloadSearchHistory = new List<DownloadSearchCriteria>();
		public static List<List<TorrentLink>> downloadSearchResultsHistory = new List<List<TorrentLink>>();

	
		public static int GlobalSeriesID = -1; // either AnimeSeriesID
		public static int GlobalAnimeID = -1; // AnimeID
		public static int GlobalSeiyuuID = -1; // SeiyuuID

		public static int CurrentCalendarMonth = DateTime.Now.Month;
		public static int CurrentCalendarYear = DateTime.Now.Year;
		public static int CurrentCalendarButton = 4;

		public static VideoHandler vidHandler;

		public static UTorrentHelper uTorrent = new UTorrentHelper();

		public static View currentView = null;
		public static ViewClassification currentViewClassification = ViewClassification.Views;
		public static string currentStaticViewID = ""; // used to stored current year, genre etc in static views

		private GUIFacadeControl.Layout groupViewMode = GUIFacadeControl.Layout.List; // Poster List
		private GUIFacadeControl.Layout seriesViewMode = GUIFacadeControl.Layout.List;
		//private GUIFacadeControl.Layout episodeTypesViewMode = GUIFacadeControl.Layout.List; // List
		private GUIFacadeControl.Layout episodesViewMode = GUIFacadeControl.Layout.List; // List

		private List<GUIListItem> itemsForDelayedImgLoading = null;

		private BackgroundWorker workerFacade = null;
		private BackgroundWorker downloadImagesWorker = new BackgroundWorker();
		public static ImageDownloader imageHelper = null;

		private AsyncImageResource listPoster = null;
		private AsyncImageResource fanartTexture = null;
		//private bool isInitialGroupLoad = true;

		public static GroupFilterVM curGroupFilter = null;
		public static GroupFilterVM curGroupFilterSub = null;
		public static GroupFilterVM curGroupFilterSub2 = null;
		public static AnimeGroupVM curAnimeGroup = null;
		public static AnimeGroupVM curAnimeGroupViewed = null;
		public static AnimeSeriesVM curAnimeSeries = null;
		public static AnimeEpisodeTypeVM curAnimeEpisodeType = null;
		private AnimeEpisodeVM curAnimeEpisode = null;

		Dictionary<int, QuickSort> GroupFilterQuickSorts = null;

		

		private System.Timers.Timer searchTimer = null;
		private System.Timers.Timer autoUpdateTimer = null;
		private SearchCollection search = null;
		private List<GUIListItem> lstFacadeItems = null;
		private string searchSound = "click.wav";

		public delegate void OnToggleWatchedHandler(List<AnimeEpisodeVM> episodes, bool state);
		public event OnToggleWatchedHandler OnToggleWatched;
		protected void ToggleWatchedEvent(List<AnimeEpisodeVM> episodes, bool state)
		{
			if (OnToggleWatched != null)
			{
				OnToggleWatched(episodes, state);
			}
		}

		public delegate void OnRateSeriesHandler(AnimeSeriesVM series, string rateValue);
		public event OnRateSeriesHandler OnRateSeries;
		protected void RateSeriesEvent(AnimeSeriesVM series, string rateValue)
		{
			if (OnRateSeries != null)
			{
				OnRateSeries(series, rateValue);
			}
		}

		public MainWindow()
		{
			// get ID of windowplugin belonging to this setup
			// enter your own unique code
			GetID = Constants.PlugInInfo.ID;

			try
			{
				settings = new AnimePluginSettings();

				imageHelper = new ImageDownloader();
				imageHelper.Init();

				listPoster = new AsyncImageResource();
				listPoster.Property = "#Anime3.GroupSeriesPoster";
				listPoster.Delay = artworkDelay;

				fanartTexture = new AsyncImageResource();
				fanartTexture.Property = "#Anime3.Fanart.1";
				fanartTexture.Delay = artworkDelay;

				GroupFilterQuickSorts = new Dictionary<int, QuickSort>();

				//searching
				searchTimer = new System.Timers.Timer();
				searchTimer.AutoReset = true;
				searchTimer.Interval = settings.FindTimeout_s * 1000;
				searchTimer.Elapsed += new System.Timers.ElapsedEventHandler(searchTimer_Elapsed);

				//set the search key sound to the same sound for the REMOTE_1 key
				Key key = new Key('1', (int)Keys.D1);
				MediaPortal.GUI.Library.Action action = new MediaPortal.GUI.Library.Action();
				ActionTranslator.GetAction(GetID, key, ref action);
				searchSound = action.SoundFileName;

				// timer for automatic updates
				autoUpdateTimer = new System.Timers.Timer();
				autoUpdateTimer.AutoReset = true;
				autoUpdateTimer.Interval = 5 * 60 * 1000; // 5 minutes * 60 seconds
				autoUpdateTimer.Elapsed += new System.Timers.ElapsedEventHandler(autoUpdateTimer_Elapsed);

				downloadImagesWorker.DoWork += new DoWorkEventHandler(downloadImagesWorker_DoWork);

				this.OnToggleWatched += new OnToggleWatchedHandler(MainWindow_OnToggleWatched);

				g_Player.PlayBackEnded += new g_Player.EndedHandler(g_Player_PlayBackEnded);
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write(ex.ToString());
				throw;
			}
		}

		

		void MainWindow_OnToggleWatched(List<AnimeEpisodeVM> episodes, bool state)
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
			List<JMMServerBinary.Contract_AniDBAnime> contracts = JMMServerVM.Instance.clientBinaryHTTP.GetAllAnime();

			int i = 0;
			foreach (JMMServerBinary.Contract_AniDBAnime anime in contracts)
			{
				//Thread.Sleep(5); // don't use too many resources
				imageHelper.DownloadAniDBCover(new AniDB_AnimeVM(anime), false);
				i++;

				//if (i == 80) break;
			}

			// 2. Download posters from TvDB
			List<JMMServerBinary.Contract_TvDB_ImagePoster> posters = JMMServerVM.Instance.clientBinaryHTTP.GetAllTvDBPosters(null);
			foreach (JMMServerBinary.Contract_TvDB_ImagePoster poster in posters)
			{
				//Thread.Sleep(5); // don't use too many resources
				imageHelper.DownloadTvDBPoster(new TvDB_ImagePosterVM(poster), false);
			}

			// 2a. Download posters from MovieDB
			List<JMMServerBinary.Contract_MovieDB_Poster> moviePosters = JMMServerVM.Instance.clientBinaryHTTP.GetAllMovieDBPosters(null);
			foreach (JMMServerBinary.Contract_MovieDB_Poster poster in moviePosters)
			{
				//Thread.Sleep(5); // don't use too many resources
				imageHelper.DownloadMovieDBPoster(new MovieDB_PosterVM(poster), false);
			}

			// 3. Download wide banners from TvDB
			List<JMMServerBinary.Contract_TvDB_ImageWideBanner> banners = JMMServerVM.Instance.clientBinaryHTTP.GetAllTvDBWideBanners(null);
			foreach (JMMServerBinary.Contract_TvDB_ImageWideBanner banner in banners)
			{
				//Thread.Sleep(5); // don't use too many resources
				imageHelper.DownloadTvDBWideBanner(new TvDB_ImageWideBannerVM(banner), false);
			}

			// 4. Download fanart from TvDB
			List<JMMServerBinary.Contract_TvDB_ImageFanart> fanarts = JMMServerVM.Instance.clientBinaryHTTP.GetAllTvDBFanart(null);
			foreach (JMMServerBinary.Contract_TvDB_ImageFanart fanart in fanarts)
			{
				//Thread.Sleep(5); // don't use too many resources
				imageHelper.DownloadTvDBFanart(new TvDB_ImageFanartVM(fanart), false);
			}

			// 4a. Download fanart from MovieDB
			List<JMMServerBinary.Contract_MovieDB_Fanart> movieFanarts = JMMServerVM.Instance.clientBinaryHTTP.GetAllMovieDBFanart(null);
			foreach (JMMServerBinary.Contract_MovieDB_Fanart fanart in movieFanarts)
			{
				//Thread.Sleep(5); // don't use too many resources
				imageHelper.DownloadMovieDBFanart(new MovieDB_FanartVM(fanart), false);
			}

			// 5. Download episode images from TvDB
			List<JMMServerBinary.Contract_TvDB_Episode> eps = JMMServerVM.Instance.clientBinaryHTTP.GetAllTvDBEpisodes(null);
			foreach (JMMServerBinary.Contract_TvDB_Episode episode in eps)
			{
				//Thread.Sleep(5); // don't use too many resources
				imageHelper.DownloadTvDBEpisode(new TvDB_EpisodeVM(episode), false);
			}

			// 6. Download posters from Trakt
			List<JMMServerBinary.Contract_Trakt_ImagePoster> traktPosters = JMMServerVM.Instance.clientBinaryHTTP.GetAllTraktPosters(null);
			foreach (JMMServerBinary.Contract_Trakt_ImagePoster traktposter in traktPosters)
			{
				//Thread.Sleep(5); // don't use too many resources
				if (string.IsNullOrEmpty(traktposter.ImageURL)) continue;
				imageHelper.DownloadTraktPoster(new Trakt_ImagePosterVM(traktposter), false);
			}

			// 7. Download fanart from Trakt
			List<JMMServerBinary.Contract_Trakt_ImageFanart> traktFanarts = JMMServerVM.Instance.clientBinaryHTTP.GetAllTraktFanart(null);
			foreach (JMMServerBinary.Contract_Trakt_ImageFanart traktFanart in traktFanarts)
			{
				//Thread.Sleep(5); // don't use too many resources
				if (string.IsNullOrEmpty(traktFanart.ImageURL)) continue;
				imageHelper.DownloadTraktFanart(new Trakt_ImageFanartVM(traktFanart), false);
			}

			// 8. Download episode images from Trakt
			List<JMMServerBinary.Contract_Trakt_Episode> traktEpisodes = JMMServerVM.Instance.clientBinaryHTTP.GetAllTraktEpisodes(null);
			foreach (JMMServerBinary.Contract_Trakt_Episode traktEp in traktEpisodes)
			{
				//Thread.Sleep(5); // don't use too many resources
				if (string.IsNullOrEmpty(traktEp.EpisodeImage)) continue;

				// special case for trak episodes
				// Trakt will return the fanart image when no episode image exists, but we don't want this
				int pos = traktEp.EpisodeImage.IndexOf(@"episodes/");
				if (pos <= 0) continue;

				imageHelper.DownloadTraktEpisode(new Trakt_EpisodeVM(traktEp), false);
			}

			
		}




		#region External Event Handlers

		#endregion

		public override bool Init()
		{
			try
			{
				BaseConfig.MyAnimeLog.Write("INIT MAIN WINDOW");

				vidHandler = new VideoHandler();
				vidHandler.DefaultAudioLanguage = settings.DefaultAudioLanguage;
				vidHandler.DefaultSubtitleLanguage = settings.DefaultSubtitleLanguage;

			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write("Error on init: {0}", ex.ToString());
			}

			return Load(GUIGraphicsContext.Skin + @"\Anime3_Main.xml");
		}

		void Instance_ServerStatusEvent(Events.ServerStatusEventArgs ev)
		{
			setGUIProperty("HasherQueueCount", ev.HasherQueueCount.ToString());
			setGUIProperty("HasherQueueState", ev.HasherQueueState);
			setGUIProperty("HasherQueueRunning", ev.HasherQueueRunning ? "Running" : "Paused");

			setGUIProperty("GeneralQueueCount", ev.GeneralQueueCount.ToString());
			setGUIProperty("GeneralQueueState", ev.GeneralQueueState);
			setGUIProperty("GeneralQueueRunning", ev.GeneralQueueRunning ? "Running" : "Paused");

			setGUIProperty("ImagesQueueCount", ev.ImagesQueueCount.ToString());
			setGUIProperty("ImagesQueueState", ev.ImagesQueueState);
			setGUIProperty("ImagesQueueRunning", ev.ImagesQueueRunning ? "Running" : "Paused");

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
			AnimePluginSettings sett = new AnimePluginSettings();

			strButtonText = sett.PluginName;
			strButtonImage = String.Empty;
			strButtonImageFocus = String.Empty;
			strPictureImage = "hover_my anime3.jpg";
			return true;
		}



		#endregion

		private void EvaluateVisibility()
		{
			bool fave = false;
			bool missing = false;

			if (curAnimeGroup != null)
			{
				if (curAnimeGroup.IsFave == 1)
					fave = true;

				//BaseConfig.MyAnimeLog.Write("settings.ShowMissing: {0}", settings.ShowMissing);
				bool missingVisible = false;
				if (settings.ShowMissing && listLevel == Listlevel.Series && curAnimeSeries != null)
				{
					missingVisible = curAnimeSeries.HasMissingEpisodesGroups;
				}

				if (settings.ShowMissing && listLevel == Listlevel.Group)
				{
					missingVisible = curAnimeGroup.HasMissingEpisodes;
				}

				if (settings.ShowMissing)
				{
					missing = missingVisible;
				}
				if (dummyFave != null) dummyFave.Visible = fave;
				if (dummyMissingEps != null) dummyMissingEps.Visible = missing;
				//BaseConfig.MyAnimeLog.Write("EvaluateVisibility:: {0} - {1} - {2}", imgListFave != null, curAnimeGroup.IsFave, dummyLayoutListMode.Visible);
			}

			//EvaluateServerStatus();
		}

		protected override void OnPageDestroy(int new_windowId)
		{

			hook.IsEnabled = false;
			hook.UnHook();
			hook = null;
			UnSubClass();

			base.OnPageDestroy(new_windowId);
		}

		#region Detect application focus
		const int GWL_WNDPROC = (-4);
		const int WM_ACTIVATEAPP = 0x1C;

		// This static method is required because legacy OSes do not support
		// SetWindowLongPtr
		public static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
		{
			if (IntPtr.Size == 8)
				return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
			else
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
		WindowProc NewWindowProc = null;
		void SubClass()
		{
			IntPtr hWnd = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
			NewWindowProc = new WindowProc(MyWindowProc);
			DefWindowProc = SetWindowLongPtr(hWnd, GWL_WNDPROC, Marshal.GetFunctionPointerForDelegate(NewWindowProc));
		}

		void UnSubClass()
		{
			IntPtr hWnd = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
			DefWindowProc = SetWindowLongPtr(hWnd, GWL_WNDPROC, DefWindowProc);
			DefWindowProc = IntPtr.Zero;
		}

		int MyWindowProc(System.IntPtr hWnd, int msg, int wParam, int lParam)
		{
			if (msg == WM_ACTIVATEAPP)
			{
				//disable keyboard hook if app is inactive
				// wParam=1 when activating, 0 when desactivating
				hook.IsEnabled = (wParam == 1);
			}

			return CallWindowProc(DefWindowProc, hWnd, msg, wParam, lParam);
		}
		#endregion

		protected override void OnPageLoad()
		{
			BaseConfig.MyAnimeLog.Write("Starting page load...");

			SubClass();

			hook = new KeyboardHook();
			hook.KeyDown += new KeyEventHandlerEx(hook_KeyDown);
			hook.KeyUp += new KeyEventHandlerEx(hook_KeyUp);
			hook.IsEnabled = true;

			if (!isFirstInitDone)
				OnFirstStart();

			currentViewClassification = settings.LastViewClassification;
			currentStaticViewID = settings.LastStaticViewID;
			currentView = settings.LastView;



			groupViewMode = settings.LastGroupViewMode;
			m_Facade.CurrentLayout = groupViewMode;
			//backdrop.LoadingImage = loadingImage;

			Console.Write(JMMServerVM.Instance.ServerOnline.ToString());

			LoadFacade();
			m_Facade.Focus = true;

			SkinSettings.Load();

			//MainWindow.anidbProcessor.UpdateVotesHTTP(MainWindow.settings.Username, MainWindow.settings.Password);

			

			autoUpdateTimer.Start();

			BaseConfig.MyAnimeLog.Write("Thumbs Setting Folder: {0}", settings.ThumbsFolder);

			//searching
			setGUIProperty(guiProperty.FindInput, " ");
			setGUIProperty(guiProperty.FindText, " ");
			setGUIProperty(guiProperty.FindMatch, " ");

			

			search = new SearchCollection();
			search.List = m_Facade;
			search.ListItemSearchProperty = "DVDLabel";
			search.Mode = settings.FindMode;
			search.StartWord = settings.FindStartWord;

			UpdateSearchPanel(false);


			DownloadAllImages();
		}


		void autoUpdateTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{

		}
		

		private void AddFacadeItem(GUIListItem item)
		{
			int selectedIndex = m_Facade.SelectedListItemIndex;
			SaveOrRestoreFacadeItems(false);

			m_Facade.Add(item);

			if (searchTimer.Enabled)
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

					workerFacade.DoWork += new DoWorkEventHandler(workerFacade_DoWork);
					workerFacade.RunWorkerCompleted += new RunWorkerCompletedEventHandler(workerFacade_RunWorkerCompleted);
					workerFacade.ProgressChanged += new ProgressChangedEventHandler(workerFacade_ProgressChanged);
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
				GUIControl.ClearControl(this.GetID, m_Facade.GetID);

				SetFacade();

				this.m_Facade.ListLayout.Clear();

				if (this.m_Facade.ThumbnailLayout != null)
					this.m_Facade.ThumbnailLayout.Clear();

				if (this.m_Facade.FilmstripLayout != null)
					this.m_Facade.FilmstripLayout.Clear();

				if (this.m_Facade.CoverFlowLayout != null)
					this.m_Facade.CoverFlowLayout.Clear();


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
									gli.OnItemSelected += new GUIListItem.ItemSelectedHandler(onFacadeItemSelected);


								AddFacadeItem(gli);
								if (arg.Type == BackGroundLoadingArgumentType.ListElementForDelayedImgLoading)
								{
									if (itemsForDelayedImgLoading == null)
										itemsForDelayedImgLoading = new List<GUIListItem>();
									itemsForDelayedImgLoading.Add(gli);
								}
							}
							if (this.m_Facade.SelectedListItemIndex < 1)
							{
								this.m_Facade.Focus = true;
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
						GUIFacadeControl.Layout viewMode = (GUIFacadeControl.Layout)arg.Argument;
						//setFacadeMode(viewMode);
						break;

					case BackGroundLoadingArgumentType.ElementSelection:
						{
							// thread told us which element it'd like to select
							// however the user might have already started moving around
							// if that is the case, we don't select anything
							if (this.m_Facade != null && this.m_Facade.SelectedListItemIndex < 1)
							{
								this.m_Facade.Focus = true;
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

		bool m_bQuickSelect = false;
		void SelectItem(int index)
		{
			//BaseConfig.MyAnimeLog.Write("SelectItem: {0}", index.ToString());

			// Hack for 'set' SelectedListItemIndex not being implemented in Filmstrip View
			// Navigate to selected using OnAction instead 
			if (m_Facade.CurrentLayout == GUIFacadeControl.Layout.Filmstrip || m_Facade.CurrentLayout == GUIFacadeControl.Layout.CoverFlow)
			{

				if (listLevel == Listlevel.Series || listLevel == Listlevel.Group)
				{
					int currentIndex = m_Facade.SelectedListItemIndex;
					if (index >= 0 && index < m_Facade.Count && index != currentIndex)
					{
						m_bQuickSelect = true;
						int increment = (currentIndex < index) ? 1 : -1;
						MediaPortal.GUI.Library.Action.ActionType actionType = (currentIndex < index) ? MediaPortal.GUI.Library.Action.ActionType.ACTION_MOVE_RIGHT : MediaPortal.GUI.Library.Action.ActionType.ACTION_MOVE_LEFT;
						for (int i = currentIndex; i != index; i += increment)
						{
							// Now push fields to skin
							if (i == (index - increment))
								m_bQuickSelect = false;

							m_Facade.OnAction(new MediaPortal.GUI.Library.Action(actionType, 0, 0));
						}
						m_bQuickSelect = false;
					}
					else
					{
						if (listLevel == Listlevel.Group && m_Facade.Count > 0)
						{
							Group_OnItemSelected(m_Facade.SelectedListItem);
						}
					}
				}

			}
			else
				m_Facade.SelectedListItemIndex = index;
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

		void bgLoadFacade()
		{
			try
			{
				GUIListItem item = null;
				int selectedIndex = -1;
				int count = 0;
				bool delayedImageLoading = false;
				List<AnimeGroupVM> groups = null;
				List<GroupFilterVM> groupFilters = null;
				List<GUIListItem> list = new List<GUIListItem>();
				BackGroundLoadingArgumentType type = BackGroundLoadingArgumentType.None;

				switch (listLevel)
				{
					#region Group Filters
					case Listlevel.GroupFilter:
						{
							// List/Poster/Banner

							setGUIProperty("SimpleCurrentView", "Group Filters");

							if (groupViewMode != GUIFacadeControl.Layout.List)
							{
								// reinit the itemsList
								delayedImageLoading = true;
								ReportFacadeLoadingProgress(BackGroundLoadingArgumentType.DelayedImgInit, 0, null);
							}

							// text as usual
							ReportFacadeLoadingProgress(BackGroundLoadingArgumentType.SetFacadeMode, 0, GUIFacadeControl.Layout.List);

							if (workerFacade.CancellationPending)
								return;

							BaseConfig.MyAnimeLog.Write("bgLoadFacde: Group Filters");
							groupFilters = FacadeHelper.GetGroupFilters();
							type = BackGroundLoadingArgumentType.ListFullElement;

							setGUIProperty(guiProperty.GroupCount, groupFilters.Count.ToString());

							foreach (GroupFilterVM grpFilter in groupFilters)
							{
								if (workerFacade.CancellationPending) return;
								try
								{
									item = null;

									SetGroupFilterListItem(ref item, grpFilter);

									if (curGroupFilter != null)
									{
										if (grpFilter.GroupFilterID.Value == curGroupFilter.GroupFilterID.Value)
										{
											selectedIndex = count;
										}
									}
									else
									{
										if (selectedIndex == -1)
											selectedIndex = count;
									}
									

									if (workerFacade.CancellationPending) return;
									else
									{
										list.Add(item);
									}

								}
								catch (Exception ex)
								{
									string msg = string.Format("The 'LoadFacade' function has generated an error displaying list items: {0} - {1}", listLevel, ex.ToString());
									BaseConfig.MyAnimeLog.Write(msg);
								}
								count++;
							}
						}
						break;
					#endregion

					#region Group Filters - Sub
					case Listlevel.GroupFilterSub:
						{
							// List/Poster/Banner

							setGUIProperty("SimpleCurrentView", curGroupFilter.GroupFilterName);

							if (groupViewMode != GUIFacadeControl.Layout.List)
							{
								// reinit the itemsList
								delayedImageLoading = true;
								ReportFacadeLoadingProgress(BackGroundLoadingArgumentType.DelayedImgInit, 0, null);
							}

							// text as usual
							ReportFacadeLoadingProgress(BackGroundLoadingArgumentType.SetFacadeMode, 0, GUIFacadeControl.Layout.List);

							if (workerFacade.CancellationPending)
								return;

							BaseConfig.MyAnimeLog.Write("bgLoadFacde: Group Filters");
							groupFilters = FacadeHelper.GetGroupFilters();
							type = BackGroundLoadingArgumentType.ListFullElement;

							setGUIProperty(guiProperty.GroupCount, "0");

							foreach (GroupFilterVM grpFilter in FacadeHelper.GetTopLevelPredefinedGroupFilters())
							{
								if (workerFacade.CancellationPending) return;
								try
								{
									item = null;

									SetGroupFilterListItem(ref item, grpFilter);

									if (curGroupFilter != null)
									{
										if (grpFilter.GroupFilterID.Value == curGroupFilter.GroupFilterID.Value)
										{
											selectedIndex = count;
										}
									}
									else
									{
										if (selectedIndex == -1)
											selectedIndex = count;
									}


									if (workerFacade.CancellationPending) return;
									else
									{
										list.Add(item);
									}

								}
								catch (Exception ex)
								{
									string msg = string.Format("The 'LoadFacade' function has generated an error displaying list items: {0} - {1}", listLevel, ex.ToString());
									BaseConfig.MyAnimeLog.Write(msg);
								}
								count++;
							}
						}
						break;
					#endregion

					#region Group Filters - Sub2
					case Listlevel.GroupFilterSub2:
						{
							// List/Poster/Banner

							setGUIProperty("SimpleCurrentView", curGroupFilter.GroupFilterName);

							if (groupViewMode != GUIFacadeControl.Layout.List)
							{
								// reinit the itemsList
								delayedImageLoading = true;
								ReportFacadeLoadingProgress(BackGroundLoadingArgumentType.DelayedImgInit, 0, null);
							}

							// text as usual
							ReportFacadeLoadingProgress(BackGroundLoadingArgumentType.SetFacadeMode, 0, GUIFacadeControl.Layout.List);

							if (workerFacade.CancellationPending)
								return;

							BaseConfig.MyAnimeLog.Write("bgLoadFacde: Group Filters");
							groupFilters = FacadeHelper.GetGroupFilters();
							type = BackGroundLoadingArgumentType.ListFullElement;

							setGUIProperty(guiProperty.GroupCount, "0");

							foreach (GroupFilterVM grpFilter in FacadeHelper.GetGroupFiltersForPredefined(curGroupFilterSub))
							{
								if (workerFacade.CancellationPending) return;
								try
								{
									item = null;

									SetGroupFilterListItem(ref item, grpFilter);

									if (curGroupFilter != null)
									{
										if (grpFilter.GroupFilterID.Value == curGroupFilter.GroupFilterID.Value)
										{
											selectedIndex = count;
										}
									}
									else
									{
										if (selectedIndex == -1)
											selectedIndex = count;
									}


									if (workerFacade.CancellationPending) return;
									else
									{
										list.Add(item);
									}

								}
								catch (Exception ex)
								{
									string msg = string.Format("The 'LoadFacade' function has generated an error displaying list items: {0} - {1}", listLevel, ex.ToString());
									BaseConfig.MyAnimeLog.Write(msg);
								}
								count++;
							}
						}
						break;
					#endregion

					#region Groups
					case Listlevel.Group:
						{
							// List/Poster/Banner

							setGUIProperty("SimpleCurrentView", curGroupFilter.GroupFilterName);

							if (groupViewMode != GUIFacadeControl.Layout.List)
							{
								// reinit the itemsList
								delayedImageLoading = true;
								ReportFacadeLoadingProgress(BackGroundLoadingArgumentType.DelayedImgInit, 0, null);
							}

							if (groupViewMode != GUIFacadeControl.Layout.List)
							{
								// graphical
								ReportFacadeLoadingProgress(BackGroundLoadingArgumentType.SetFacadeMode, 0, GUIFacadeControl.Layout.AlbumView);
							}
							else
							{
								// text as usual
								ReportFacadeLoadingProgress(BackGroundLoadingArgumentType.SetFacadeMode, 0, GUIFacadeControl.Layout.List);
							}

							if (workerFacade.CancellationPending)
								return;

							if (curGroupFilterSub2 == null)
								groups = JMMServerHelper.GetAnimeGroupsForFilter(curGroupFilter);
							else
							{
								groups = new List<AnimeGroupVM>();



								List<AnimeGroupVM> tempGroups = JMMServerHelper.GetAnimeGroupsForFilter(GroupFilterHelper.AllGroupsFilter);
								foreach (AnimeGroupVM grp in tempGroups)
								{
									if (curGroupFilterSub2.GroupFilterID.Value == Constants.StaticGF.Predefined_Categories_Child)
									{
										if (grp.Categories.Contains(curGroupFilterSub2.PredefinedCriteria))
											groups.Add(grp);
									}
									if (curGroupFilterSub2.GroupFilterID.Value == Constants.StaticGF.Predefined_Years_Child)
									{
										// find all the groups that qualify by this year
										int startYear = 0;
										if (!grp.Stat_AirDate_Min.HasValue) continue;
										startYear = grp.Stat_AirDate_Min.Value.Year;

										int endYear = int.MaxValue;
										if (grp.Stat_AirDate_Max.HasValue) endYear = grp.Stat_AirDate_Max.Value.Year;

										int critYear = 0;
										if (!int.TryParse(curGroupFilterSub2.PredefinedCriteria, out critYear)) continue;

										if (critYear >= startYear && critYear <= endYear) groups.Add(grp);
										
									}
								}
							}

							// re-sort if user has set a quick sort
							if (GroupFilterQuickSorts.ContainsKey(curGroupFilter.GroupFilterID.Value))
							{
								BaseConfig.MyAnimeLog.Write("APPLYING QUICK SORT");

								GroupFilterSorting sortType = GroupFilterHelper.GetEnumForText_Sorting(GroupFilterQuickSorts[curGroupFilter.GroupFilterID.Value].SortType);
								SortPropOrFieldAndDirection sortProp = GroupFilterHelper.GetSortDescription(sortType, GroupFilterQuickSorts[curGroupFilter.GroupFilterID.Value].SortDirection);
								List<SortPropOrFieldAndDirection> sortCriteria = new List<SortPropOrFieldAndDirection>();
								sortCriteria.Add(sortProp);
								groups = Sorting.MultiSort<AnimeGroupVM>(groups, sortCriteria);
							}


							// Update Series Count Property
							setGUIProperty(guiProperty.GroupCount, groups.Count.ToString());
							type = (groupViewMode != GUIFacadeControl.Layout.List) ? BackGroundLoadingArgumentType.ListElementForDelayedImgLoading : BackGroundLoadingArgumentType.ListFullElement;

							int seriesCount = 0;

							double totalTime = 0;
							DateTime start = DateTime.Now;

							BaseConfig.MyAnimeLog.Write("Building groups: " + curGroupFilter.GroupFilterName);
							foreach (AnimeGroupVM grp in groups)
							{
								if (workerFacade.CancellationPending) return;
								try
								{
									item = null;

									//BaseConfig.MyAnimeLog.Write(string.Format("{0} - {1}", grp.GroupName, grp.AniDBRating));

									SetGroupListItem(ref item, grp);

									if (settings.HideWatchedFiles && grp.UnwatchedEpisodeCount <= 0)
									{
										//watched files should be hidden and entire group is watched
										// -> hide entire group
										continue;
									}

									seriesCount += grp.AllSeriesCount;

									if (curAnimeGroup != null)
									{
										if (grp.AnimeGroupID == curAnimeGroup.AnimeGroupID)
										{
											selectedIndex = count;
										}
									}
									else
									{
										if (selectedIndex == -1)
											selectedIndex = count;
									}


									if (workerFacade.CancellationPending) return;
									else
									{
										list.Add(item);
									}

								}
								catch (Exception ex)
								{
									string msg = string.Format("The 'LoadFacade' function has generated an error displaying list items: {0} - {1}", listLevel, ex.ToString());
									BaseConfig.MyAnimeLog.Write(msg);
								}
								count++;
							}

							TimeSpan ts2 = DateTime.Now - start;
							totalTime += ts2.TotalMilliseconds;

							BaseConfig.MyAnimeLog.Write("Total time for rendering groups: {0}-{1}", groups.Count, totalTime);

							setGUIProperty(guiProperty.SeriesCount, seriesCount.ToString());

							

							
						}
						break;
					#endregion

					#region Series
					case Listlevel.Series:
						{
							// this level includes series as well as sub-groups

							if (seriesViewMode != GUIFacadeControl.Layout.List)
							{
								// reinit the itemsList
								delayedImageLoading = true;
								ReportFacadeLoadingProgress(BackGroundLoadingArgumentType.DelayedImgInit, 0, null);
							}

							if (workerFacade.CancellationPending) return;

							List<SortPropOrFieldAndDirection> sortCriteria = null;

							List<AnimeGroupVM> subGroups = curAnimeGroupViewed.SubGroups;
							if (subGroups.Count > 0)
							{
								sortCriteria = new List<SortPropOrFieldAndDirection>();
								sortCriteria.Add(new SortPropOrFieldAndDirection("SortName", false, SortType.eString));
								subGroups = Sorting.MultiSort<AnimeGroupVM>(subGroups, sortCriteria);
							}

							// get the series for this group
							List<AnimeSeriesVM> seriesList = curAnimeGroupViewed.ChildSeries;
							if (seriesList.Count > 0)
							{
								sortCriteria = new List<SortPropOrFieldAndDirection>();
								sortCriteria.Add(new SortPropOrFieldAndDirection("AirDate", false, SortType.eDateTime));
								seriesList = Sorting.MultiSort<AnimeSeriesVM>(seriesList, sortCriteria);
							}
							//if (seriesList.Count == 0)
							//	bFacadeEmpty = true;

							// Update Series Count Property
							setGUIProperty(guiProperty.SeriesCount, seriesList.Count.ToString());

							// now sort the groups by air date
							
							
							type = BackGroundLoadingArgumentType.ListFullElement;

							foreach (AnimeGroupVM grp in subGroups)
							{
								if (workerFacade.CancellationPending) return;
								try
								{
									item = null;

									SetGroupListItem(ref item, grp);

									if (settings.HideWatchedFiles && grp.UnwatchedEpisodeCount <= 0)
									{
										//watched files should be hidden and entire group is watched
										// -> hide entire group
										continue;
									}

									if (workerFacade.CancellationPending) return;
									else
									{
										list.Add(item);
									}

								}
								catch (Exception ex)
								{
									string msg = string.Format("The 'LoadFacade' function has generated an error displaying list items: {0} - {1}", listLevel, ex.ToString());
									BaseConfig.MyAnimeLog.Write(msg);
								}
							}

							foreach (AnimeSeriesVM ser in seriesList)
							{
								//BaseConfig.MyAnimeLog.Write("LoadFacade-Series:: {0}", ser);
								if (workerFacade.CancellationPending) return;
								try
								{
									item = null;

									SetSeriesListItem(ref item, ser);

									if (settings.HideWatchedFiles && ser.UnwatchedEpisodeCount <= 0)
									{
										//watched files should be hidden and entire series is watched
										// -> hide entire series
										continue;
									}

									if (workerFacade.CancellationPending) return;
									else
									{
										list.Add(item);
									}

								}
								catch (Exception ex)
								{
									string msg = string.Format("The 'LoadFacade' function has generated an error displaying list items: {0} - {1}", listLevel, ex.ToString());
									BaseConfig.MyAnimeLog.Write(msg);
								}
								count++;
							}
						}
						break;
					#endregion

					#region Episode Types
					case Listlevel.EpisodeTypes:
						{

							if (workerFacade.CancellationPending) return;

							//List<AnimeEpisodeType> anEpTypes = AnimeSeries.GetEpisodeTypes(curAnimeSeries.AnimeSeriesID.Value);
							type = BackGroundLoadingArgumentType.ListFullElement;
							foreach (AnimeEpisodeTypeVM anEpType in curAnimeSeries.EpisodeTypesToDisplay)
							{
								item = null;
								SetEpisodeTypeListItem(ref item, anEpType);

								if (workerFacade.CancellationPending) return;
								else
								{
									list.Add(item);
								}
								count++;
							}
						}
						break;
					#endregion

					#region Episodes
					case Listlevel.Episode:
						{

							if (workerFacade.CancellationPending) return;

							if (curAnimeSeries == null) return;

							// get the episodes for this series / episode types
							//BaseConfig.MyAnimeLog.Write("GetEpisodes:: {0}", curAnimeSeries.AnimeSeriesID.Value);

							//List<AnimeEpisode> episodeList = AnimeSeries.GetEpisodes(curAnimeSeries.AnimeSeriesID.Value);
							curAnimeSeries.RefreshEpisodes();
							List<AnimeEpisodeVM> episodeList = curAnimeSeries.GetEpisodesToDisplay(curAnimeEpisodeType.EpisodeType);

							// Update Series Count Property
							//setGUIProperty(guiProperty.SeriesCount, episodeList.Count.ToString());

							bool foundFirstUnwatched = false;
							type = BackGroundLoadingArgumentType.ListFullElement;
							foreach (AnimeEpisodeVM ep in episodeList)
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
									else
									{
										list.Add(item);
									}

								}
								catch (Exception ex)
								{
									string msg = string.Format("The 'LoadFacade' function has generated an error displaying list items: {0} - {1}", listLevel, ex.ToString());
									BaseConfig.MyAnimeLog.Write(msg);
								}
								count++;
							}

							SetFanartForEpisodes();
						}
						setGUIProperty(guiProperty.EpisodeCount, count.ToString());
						break;
					#endregion

				}


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
					List<KeyValuePair<AnimeGroupVM, int>> FacadeGroups = new List<KeyValuePair<AnimeGroupVM, int>>();

					// Fill the list of groups in order of their proximity to the current selection. This makes the groups currently shown load first, and then further out.
					FacadeHelper.ProximityForEach(groups, selectedIndex, delegate(AnimeGroupVM grp, int currIndex)
					{
						FacadeGroups.Add(new KeyValuePair<AnimeGroupVM, int>(grp, currIndex));
					});


					// Create number of threads based on MaxThreads. MaxThreads should be the amount of CPU cores.
					for (int i = 0; i < MaxThreads; i++)
					{
						// Create a new thread. The function it should run is written here using the delegate word.
						Thread thread = new Thread(new ThreadStart(delegate()
						{
							// The number of groups left to load in the facade. Is renewed on each loop of the threads do while loop.
							int FacadeGroupCount = 0;

							do
							{
								// create varible to store the group.
								KeyValuePair<AnimeGroupVM, int> group = new KeyValuePair<AnimeGroupVM, int>();

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
					while (done < MaxThreads)
						Thread.Sleep(500);

					BaseConfig.MyAnimeLog.Write("ImageLoad: Finished");

				}

				
				#endregion

				if (animeSeriesIDToBeRated.HasValue && BaseConfig.Settings.DisplayRatingDialogOnCompletion)
				{
					JMMServerBinary.Contract_AnimeSeries contract = JMMServerVM.Instance.clientBinaryHTTP.GetSeries(animeSeriesIDToBeRated.Value,
						JMMServerVM.Instance.CurrentUser.JMMUserID);
					if (contract != null)
					{
						AnimeSeriesVM ser = new AnimeSeriesVM(contract);
						Utils.PromptToRateSeriesOnCompletion(ser);
					}
				
					animeSeriesIDToBeRated = null;
				}

			}

			catch (Exception e)
			{
				BaseConfig.MyAnimeLog.Write("The 'LoadFacade' function has generated an error: {0}", e.ToString());
			}
		}


		private void SetEpisodeTypeListItem(ref GUIListItem item, AnimeEpisodeTypeVM epType)
		{
			string sIconList = GUIGraphicsContext.Skin + @"\Media\MyAnime3\anime3_ListIcon.png";
			string sUnWatchedFilename = GUIGraphicsContext.Skin + @"\Media\MyAnime3\anime3_UnWatched_left.png";

			try
			{
				item = new GUIListItem(epType.EpisodeTypeDescription);
				item.DVDLabel = epType.EpisodeTypeDescription;
				item.TVTag = epType;
				int unwatched = 0;
				int watched = 0;
				if (curAnimeSeries != null)
					curAnimeSeries.GetWatchedUnwatchedCount(epType.EpisodeType, ref unwatched, ref watched);
				item.IsPlayed = (unwatched == 0);

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

		private bool SetSeriesListItem(ref GUIListItem item, AnimeSeriesVM ser)
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

						item.Label3 = space + watched.ToString().PadLeft(3, '0');
						item.IconImage = sIconList;
						item.Label2 = unwatched.ToString().PadLeft(3, '0');
						break;

					case View.eLabelStyleGroups.Unwatched:

						if (ser.UnwatchedEpisodeCount > 0)
						{
							item.IconImage = sUnWatchedFilename;
							item.Label3 = ser.UnwatchedEpisodeCount.ToString() + " New";
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
						item.Label3 = totalEps.ToString() + " Episodes";
						item.Label2 = "  ";

						break;
				}
			}
			item.DVDLabel = ser.SeriesName;
			item.TVTag = ser;
			item.IsPlayed = (ser.UnwatchedEpisodeCount == 0);

			return true;
		}

		private bool SetGroupFilterListItem(ref GUIListItem item, GroupFilterVM grpFilter)
		{
			item = new GUIListItem(grpFilter.GroupFilterName);
			item.DVDLabel = grpFilter.GroupFilterName;
			item.TVTag = grpFilter;
			item.IsPlayed = false;

			return true;
		}

		private bool SetGroupListItem(ref GUIListItem item, AnimeGroupVM grp)
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
						item.Label3 = space + watched.ToString().PadLeft(3, '0');
						item.IconImage = sIconList;
						item.Label2 = unwatched.ToString().PadLeft(3, '0');
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
			item.IsPlayed = (grp.UnwatchedEpisodeCount == 0);

			return true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="item"></param>
		/// <param name="ep"></param>
		/// <returns>whether this episode has been watched</returns>
		private bool SetEpisodeListItem(ref GUIListItem item, AnimeEpisodeVM ep)
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

				View.eLabelStyleEpisodes style = settings.LabelStyleEpisodes;

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
				bool isWatched = (ep.IsWatched == 1);
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

		private void TestPosters()
		{
			GUIControl.ClearControl(this.GetID, m_Facade.GetID);

			foreach (string fileName in Directory.GetFiles(@"C:\Users\All Users\Team MediaPortal\MediaPortal\thumbs\AnimeThumbs\AniDB", "*.jpg"))
			{
				GUIListItem item = new GUIListItem("");
				item.Label = "Label";
				item.ThumbnailImage = fileName;
				AddFacadeItem(item);
			}
		}

		private void TestBanners()
		{
			GUIControl.ClearControl(this.GetID, m_Facade.GetID);

			setGUIProperty(guiProperty.Title, "Kanokon");

			GUIListItem item = new GUIListItem("");
			//item.Label = "Label";
			//item.Label2 = "xxxxx 2";
			//item.Label3 = "xxxxx 3";
			//item.ThumbnailImage = ImageAllocator.GetSeriesBannerFromFile(@"C:\htpc\banners\amaendayo1.jpg");
			item.IconImage = @"C:\htpc\watched.png";
			AddFacadeItem(item);

			//item.TVTag = null;

			GUIListItem item2 = new GUIListItem("");
			//item2.Label = "Item 2";
			//item2.ThumbnailImage = ImageAllocator.GetSeriesBannerFromFile(@"C:\htpc\banners\clannad2.jpg");
			AddFacadeItem(item2);

			//string img = ImageAllocator.GetSeriesBanner("Asu no Yoichi");
			GUIListItem item3 = new GUIListItem("");
			item3.Label = "";
			//item3.ThumbnailImage = img;
			AddFacadeItem(item3);

			/*
			GUIListItem item3 = new GUIListItem("Item 2");
			//item3.Label = "Item 2";
			item3.ThumbnailImage = @"C:\banner03.jpg";
			AddFacadeItem(item3);

			GUIListItem item4 = new GUIListItem("Item 2");
			//item4.Label = "Item 2";
			//item4.ThumbnailImage = @"C:\banner04.jpg";
			AddFacadeItem(item4);

			GUIListItem item5 = new GUIListItem("Item 2");
			//item5.Label = "Item 2";
			item5.ThumbnailImage = @"C:\banner05.jpg";
			AddFacadeItem(item5);

			GUIListItem item6 = new GUIListItem("Item 2");
			//item6.Label = "Item 2";
			item6.ThumbnailImage = @"C:\banner06.jpg";
			AddFacadeItem(item6);

			GUIListItem item7 = new GUIListItem("Item 2");
			//item7.Label = "Item 2";
			item7.ThumbnailImage = @"C:\banner07.jpg";
			AddFacadeItem(item7);
			*/
		}

		private bool ShowLayoutMenu(string previousMenu)
		{
			GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg == null)
				return true;

			//keep showing the dialog until the user closes it
			int selectedLabel = 0;
			string currentMenu = "Change Layout";
			while (true)
			{
				dlg.Reset();
				dlg.SetHeading(currentMenu);

				if (previousMenu != string.Empty)
					dlg.Add("<<< " + previousMenu);
				dlg.Add("List Posters");
				dlg.Add("Wide Banners");
				dlg.Add("Filmstrip");

				if (!m_Facade.IsNullLayout(GUIFacadeControl.Layout.CoverFlow))
					dlg.Add("Coverflow");

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
						groupViewMode = GUIFacadeControl.Layout.List;
						break;
					case 2:
						groupViewMode = GUIFacadeControl.Layout.LargeIcons;
						break;
					case 3:
						groupViewMode = GUIFacadeControl.Layout.Filmstrip;
						break;
					case 4:
						// Disabled for now due to a bug - enable to see the issue
						groupViewMode = GUIFacadeControl.Layout.CoverFlow;
						break;

						//Utils.DialogMsg("Disabled", "This Layout is temporarily disabled");
						//return false;
					default:
						//close menu
						return false;
				}

				break;
			}

			if (listLevel == Listlevel.Group)
			{
				settings.LastGroupViewMode = groupViewMode;
				settings.Save();
			}

			LoadFacade();
			return false;
		}


		private bool ShowDisplayOptionsMenu(string previousMenu)
		{
			GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg == null)
				return true;

			//keep showing the dialog until the user closes it
			int selectedLabel = 0;
			string currentMenu = "Display Options";
			while (true)
			{
				dlg.Reset();
				dlg.SetHeading(currentMenu);

				if (previousMenu != string.Empty)
					dlg.Add("<<< " + previousMenu);
				dlg.Add("Change Layout >>>");

				dlg.SelectedLabel = selectedLabel;
				dlg.DoModal(GUIWindowManager.ActiveWindow);
				selectedLabel = dlg.SelectedLabel;

				int selection = selectedLabel + ((previousMenu == string.Empty) ? 1 : 0);
				switch (selection)
				{
					case 0:
						//show previous
						LoadFacade();
						return true;
					case 1:
						if (!ShowLayoutMenu(currentMenu))
							return false;
						break;
					default:
						//close menu
						return false;
				}
			}
		}

		#region Options Menus

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


		private bool ShowOptionsDisplayMenu(string previousMenu)
		{
			GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg == null)
				return true;

			//keep showing the dialog until the user closes it
			int selectedLabel = 0;
			string currentMenu = "Display Options";
			while (true)
			{
				dlg.Reset();
				dlg.SetHeading(currentMenu);

				string showEps = string.Format("Only Show Available Episodes ({0})", settings.ShowOnlyAvailableEpisodes ? "On" : "Off");
				string hideWatched = string.Format("Hide Watched Episodes ({0})", settings.HideWatchedFiles ? "On" : "Off");
				string findFilter = string.Format("Find - Only Show Matches ({0})", settings.FindFilter ? "On" : "Off");

				if (previousMenu != string.Empty)
					dlg.Add("<<< " + previousMenu);
				dlg.Add(showEps);
				dlg.Add(hideWatched);
				dlg.Add(findFilter);

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
						settings.ShowOnlyAvailableEpisodes = !settings.ShowOnlyAvailableEpisodes;
						LoadFacade();
						break;
					case 2:
						settings.HideWatchedFiles = !settings.HideWatchedFiles;
						LoadFacade();
						break;
					case 3:
						settings.FindFilter = !settings.FindFilter;
						if (searchTimer.Enabled)
						{
							SaveOrRestoreFacadeItems(false);
							DoSearch(m_Facade.SelectedListItemIndex);
						}
						break;
					default:
						//close menu
						return false;
				}

				settings.Save();
			}
		}

		#endregion

	


		private void ChangeView(View v)
		{
			BaseConfig.MyAnimeLog.Write(string.Format("ChangeView: {0} - {1}", currentViewClassification, v == null ? "" : v.Name));
			currentViewClassification = ViewClassification.Views;
			currentView = new View(v);
			currentStaticViewID = "";

			settings.LastView = currentView;
			settings.LastViewClassification = currentViewClassification;
			settings.LastStaticViewID = currentStaticViewID;
			settings.Save();

			//update skin
			setGUIProperty("SimpleCurrentView", v.DisplayName);
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
			setGUIProperty("SimpleCurrentView", currentStaticViewID);
		}

		private void SetFacade()
		{
			bool filmstrip = false;
			bool widebanners = false;
			bool coverflow = false;
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
			else if (groupViewMode == GUIFacadeControl.Layout.CoverFlow)
				coverflow = true;
			else
				widebanners = true;

			BaseConfig.MyAnimeLog.Write("SetFacade List Mode: {0}", listLevel);

			if (listLevel == Listlevel.GroupFilter || listLevel == Listlevel.GroupFilterSub || listLevel == Listlevel.GroupFilterSub2)
			{
				m_Facade.CurrentLayout = GUIFacadeControl.Layout.List;
				listmode = true;
				widebanners = filmstrip = false;
				groupfilters = true;
				BaseConfig.MyAnimeLog.Write("SetFacade List Mode: {0}", listLevel);
			}

			if (listLevel == Listlevel.Group)
			{
				m_Facade.CurrentLayout = groupViewMode;
				groups = true;
				//BaseConfig.MyAnimeLog.Write("List Mode: {0}, IsGroups: {1}", this.dummyLayoutListMode.Visible, dummyIsGroups.Visible);
			}

			if (listLevel == Listlevel.Series)
			{
				m_Facade.CurrentLayout = GUIFacadeControl.Layout.List;
				listmode = true;
				widebanners = filmstrip = false;
				series = true;
				//BaseConfig.MyAnimeLog.Write("List Mode: {0}, IsSeries: {1}", this.dummyLayoutListMode.Visible, dummyIsSeries.Visible);
			}

			if (listLevel == Listlevel.EpisodeTypes)
			{
				//m_Facade.CurrentLayout = seriesViewMode;
				m_Facade.CurrentLayout = GUIFacadeControl.Layout.List;
				listmode = true;
				widebanners = filmstrip = false;
				episodetypes = true;
				//BaseConfig.MyAnimeLog.Write("List Mode: {0}, IsEpisodeTypes: {1}", this.dummyLayoutListMode.Visible, dummyIsEpisodeTypes.Visible);
			}

			if (listLevel == Listlevel.Episode)
			{
				m_Facade.CurrentLayout = episodesViewMode; // always list
				listmode = true;
				widebanners = filmstrip = false;
				episodes = true;
				//BaseConfig.MyAnimeLog.Write("List Mode: {0}, dummyIsEpisodes: {1}", this.dummyLayoutListMode.Visible, dummyIsEpisodes.Visible);
			}


			//if (this.dummyLayoutFilmstripMode != null) BaseConfig.MyAnimeLog.Write("SetFacade: dummyLayoutFilmstripMode.Visible : {0}", this.dummyLayoutFilmstripMode.Visible);
			//if (this.dummyLayoutWideBanners != null) BaseConfig.MyAnimeLog.Write("SetFacade: dummyLayoutWideBanners.Visible : {0}", this.dummyLayoutWideBanners.Visible);
			//if (this.dummyLayoutListMode != null) BaseConfig.MyAnimeLog.Write("SetFacade: dummyLayoutListMode.Visible : {0}", this.dummyLayoutListMode.Visible);


			EvaluateVisibility();

			if (this.dummyLayoutFilmstripMode != null) dummyLayoutFilmstripMode.Visible = filmstrip;
			if (this.dummyLayoutWideBanners != null) dummyLayoutWideBanners.Visible = widebanners;
			if (this.dummyLayoutListMode != null) this.dummyLayoutListMode.Visible = listmode;
			if (this.dummyIsGroups != null) this.dummyIsGroups.Visible = groups;
			if (this.dummyIsGroupFilters != null) this.dummyIsGroupFilters.Visible = groupfilters;
			if (this.dummyIsSeries != null) this.dummyIsSeries.Visible = series;
			if (this.dummyIsEpisodeTypes != null) this.dummyIsEpisodeTypes.Visible = episodetypes;
			if (this.dummyIsEpisodes != null) this.dummyIsEpisodes.Visible = episodes;

			BaseConfig.MyAnimeLog.Write("SetFacade: Filters: {0} - Groups: {1} - Series: {2} - Episodes: {3}", groupfilters, groups, series, episodes);

			// fix for skin visiblity problem during video playback.
			if (GUIGraphicsContext.IsPlayingVideo)
				GUIWindowManager.Render(0);

			System.Windows.Forms.Application.DoEvents();
		}

		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			if (actionType == MediaPortal.GUI.Library.Action.ActionType.ACTION_MOUSE_DOUBLECLICK)
			{
				OnShowContextMenu();
				return;
			}

			if (actionType == MediaPortal.GUI.Library.Action.ActionType.ACTION_PLAY)
			{
				if (MediaPortal.Player.g_Player.Playing == false)
					BaseConfig.MyAnimeLog.Write("Pressed the play button");
			}

			if (this.btnDisplayOptions != null && control == this.btnDisplayOptions)
			{
				hook.IsEnabled = false;

				ShowDisplayOptionsMenu("");
				btnDisplayOptions.Focus = false;

				Thread.Sleep(100); //make sure key-up's from the context menu aren't cought by the hook
				hook.IsEnabled = true;

				this.btnDisplayOptions.IsFocused = false;

				return;
			}

			if (this.btnWindowUtilities != null && control == this.btnWindowUtilities)
			{
				SetGlobalIDs();
				GUIWindowManager.ActivateWindow(Constants.WindowIDs.ADMIN);

				this.btnWindowUtilities.IsFocused = false;

				return;
			}

			if (this.btnWindowCalendar != null && control == this.btnWindowCalendar)
			{
				SetGlobalIDs();
				GUIWindowManager.ActivateWindow(Constants.WindowIDs.CALENDAR);
				//GUIWindowManager.ActivateWindow(Constants.WindowIDs.BROWSER);

				this.btnWindowCalendar.IsFocused = false;

				return;
			}

			if (this.btnWindowDownloads != null && control == this.btnWindowDownloads)
			{
				SetGlobalIDs();
				GUIWindowManager.ActivateWindow(Constants.WindowIDs.DOWNLOADS);

				this.btnWindowDownloads.IsFocused = false;
				return;
			}

			if (this.btnWindowContinueWatching != null && control == this.btnWindowContinueWatching)
			{
				SetGlobalIDs();
				GUIWindowManager.ActivateWindow(Constants.WindowIDs.WATCHING);

				this.btnWindowContinueWatching.IsFocused = false;
				return;
			}

			if (this.btnWindowRecommendations != null && control == this.btnWindowRecommendations)
			{
				SetGlobalIDs();
				GUIWindowManager.ActivateWindow(Constants.WindowIDs.RECOMMENDATIONS);

				this.btnWindowRecommendations.IsFocused = false;
				return;
			}

			if (this.btnWindowRandom != null && control == this.btnWindowRandom)
			{

				RandomWindow_LevelObject = GroupFilterHelper.AllGroupsFilter;
				RandomWindow_RandomLevel = RandomSeriesEpisodeLevel.GroupFilter;
				RandomWindow_RandomType = RandomObjectType.Series;

				GUIWindowManager.ActivateWindow(Constants.WindowIDs.RANDOM);

				this.btnWindowRandom.IsFocused = false;
				return;
			}

			if (this.btnChangeLayout != null && control == this.btnChangeLayout)
			{
				ShowLayoutMenu("");
				this.btnChangeLayout.IsFocused = false;
				return;
			}

			if (this.btnSwitchUser != null && control == this.btnSwitchUser)
			{
				if (JMMServerVM.Instance.PromptUserLogin())
				{
					listLevel = Listlevel.GroupFilter;
					curAnimeEpisode = null;
					curAnimeGroup = null;
					curAnimeSeries = null;
					curGroupFilter = null;

					// user has logged in, so save to settings so we will log in as the same user next time
					settings.CurrentJMMUserID = JMMServerVM.Instance.CurrentUser.JMMUserID.ToString();
					settings.Save();

					LoadFacade();
				}

				this.btnSwitchUser.IsFocused = false;
				return;
			}

			if (this.btnSettings != null && control == this.btnSettings)
			{
				hook.IsEnabled = false;

				ShowOptionsDisplayMenu("");
				btnDisplayOptions.Focus = false;

				Thread.Sleep(100); //make sure key-up's from the context menu aren't cought by the hook
				hook.IsEnabled = true;

				this.btnSettings.IsFocused = false;

				return;
			}



			try
			{
				if (actionType != MediaPortal.GUI.Library.Action.ActionType.ACTION_SELECT_ITEM) return; // some other events raised onClicked too for some reason?
				if (control == this.m_Facade)
				{
					UpdateSearchPanel(false);

					if (this.m_Facade.SelectedListItem == null || this.m_Facade.SelectedListItem.TVTag == null)
						return;

					switch (listLevel)
					{
						case Listlevel.GroupFilter:
							curGroupFilter = this.m_Facade.SelectedListItem.TVTag as GroupFilterVM;
							if (curGroupFilter == null) return;

							if (curGroupFilter.GroupFilterID.Value == Constants.StaticGF.Predefined)
							{
								listLevel = Listlevel.GroupFilterSub;
								curGroupFilterSub2 = null;
								curGroupFilterSub = null;
							}
							else
							{
								listLevel = Listlevel.Group;
								curGroupFilterSub2 = null;
								curGroupFilterSub = null;
							}

							LoadFacade();
							this.m_Facade.Focus = true;

							break;

						case Listlevel.GroupFilterSub:
							curGroupFilterSub = this.m_Facade.SelectedListItem.TVTag as GroupFilterVM;
							if (curGroupFilterSub == null) return;

							curGroupFilterSub2 = null;
							listLevel = Listlevel.GroupFilterSub2;

							LoadFacade();
							this.m_Facade.Focus = true;

							break;

						case Listlevel.GroupFilterSub2:
							curGroupFilterSub2 = this.m_Facade.SelectedListItem.TVTag as GroupFilterVM;
							if (curGroupFilterSub2 == null) return;

							listLevel = Listlevel.Group;

							LoadFacade();
							this.m_Facade.Focus = true;

							break;

						case Listlevel.Group:
							curAnimeGroup = this.m_Facade.SelectedListItem.TVTag as AnimeGroupVM;
							if (curAnimeGroup == null) return;
							curAnimeGroupViewed = curAnimeGroup;

							// e.g. if there is only one series for the group, show the episode types
							// if there is only for episode type for the series show the episodes
							ShowChildrenLevelForGroup();
							

							LoadFacade();
							this.m_Facade.Focus = true;

							break;

						case Listlevel.Series:

							if (this.m_Facade.SelectedListItem.TVTag == null) return;

							// sub groups
							if (this.m_Facade.SelectedListItem.TVTag.GetType() == typeof(AnimeGroupVM))
							{
								curAnimeGroup = this.m_Facade.SelectedListItem.TVTag as AnimeGroupVM;
								if (curAnimeGroup == null) return;
								curAnimeGroupViewed = curAnimeGroup;

								ShowChildrenLevelForGroup();
							}
							else if (this.m_Facade.SelectedListItem.TVTag.GetType() == typeof(AnimeSeriesVM))
							{
								curAnimeSeries = this.m_Facade.SelectedListItem.TVTag as AnimeSeriesVM;
								if (curAnimeSeries == null) return;

								ShowChildrenLevelForSeries();
							}

							LoadFacade();
							this.m_Facade.Focus = true;

							break;

						case Listlevel.EpisodeTypes:
							curAnimeEpisodeType = this.m_Facade.SelectedListItem.TVTag as AnimeEpisodeTypeVM;
							if (curAnimeEpisodeType == null) return;

							listLevel = Listlevel.Episode;
							SetFanartForEpisodes();
							LoadFacade();

							this.m_Facade.Focus = true;

							break;

						case Listlevel.Episode:
							this.curAnimeEpisode = this.m_Facade.SelectedListItem.TVTag as AnimeEpisodeVM;
							if (curAnimeEpisode == null) return;

							BaseConfig.MyAnimeLog.Write("Selected to play: {0}", curAnimeEpisode.EpisodeNumberAndName);
							vidHandler.ResumeOrPlay(curAnimeEpisode);

							break;
					}
				}
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write("Error in OnClicked: {0} - {1}", ex.Message, ex.ToString());
			}

			base.OnClicked(controlId, control, actionType);
		}

		private void ShowChildrenLevelForSeries()
		{
			List<AnimeEpisodeTypeVM> episodeTypes = curAnimeSeries.EpisodeTypesToDisplay;
			if (episodeTypes.Count > 1)
			{
				listLevel = Listlevel.EpisodeTypes;
			}
			else if (episodeTypes.Count == 1)
			{
				setGUIProperty(guiProperty.SeriesTitle, curAnimeSeries.SeriesName);
				// only one so lets go straight to the episodes
				curAnimeEpisodeType = episodeTypes[0];
				listLevel = Listlevel.Episode;
				SetFanartForEpisodes();

				BaseConfig.MyAnimeLog.Write("Current list level: {0} - {1}", listLevel, curAnimeEpisodeType);
			}
		}

		private void ShowChildrenLevelForGroup()
		{
			List<AnimeGroupVM> subGroups = curAnimeGroupViewed.SubGroups;
			List<AnimeSeriesVM> seriesList = curAnimeGroupViewed.ChildSeries;

			int subLevelCount = seriesList.Count + subGroups.Count;

			if (subLevelCount > 1)
			{
				listLevel = Listlevel.Series;
				LoadFacade();
				return;
			}
			else if (subLevelCount == 1)
			{
				// keep drilling down until we find a series
				// or more than one sub level
				while (subLevelCount == 1 && subGroups.Count > 0)
				{
					curAnimeGroupViewed = subGroups[0];
					curAnimeGroup = subGroups[0];

					subGroups = curAnimeGroup.SubGroups;
					seriesList = curAnimeGroup.ChildSeries;
					subLevelCount = seriesList.Count + subGroups.Count;
				}

				if (subGroups.Count == 0)
				{
					// means we got all the way down to a series
					if (seriesList.Count > 1)
					{
						listLevel = Listlevel.Series;
					}
					else if (seriesList.Count == 1)
					{
						curAnimeSeries = seriesList[0];
						ShowChildrenLevelForSeries();
					}
				}
				else
				{
					// else we have more than one sub level to display
					listLevel = Listlevel.Series;
				}
			}
		}

		public void SetFanartForEpisodes()
		{
			// do this so that after an episode is played and the page is reloaded, we will always show the correct fanart
			if (curAnimeSeries == null) return;

			LoadFanart(curAnimeSeries);
			//LoadFanart(curAnimeSeries);
		}

		public override void DeInit()
		{
			BaseConfig.MyAnimeLog.Write("DeInit");

			base.DeInit();
		}

		public override void OnAction(MediaPortal.GUI.Library.Action action)
		{
			//BaseConfig.MyAnimeLog.Write("Received action: {0}/{1}", action.wID, (char)(action.m_key.KeyChar));

			switch (action.wID)
			{
				case MediaPortal.GUI.Library.Action.ActionType.ACTION_MOVE_DOWN:
				case MediaPortal.GUI.Library.Action.ActionType.ACTION_MOVE_UP:

					//Reset autoclose timer on search
					if (searchTimer.Enabled)
					{
						searchTimer.Stop();
						searchTimer.Start();
					}

					base.OnAction(action);
					break;
				case MediaPortal.GUI.Library.Action.ActionType.ACTION_MOVE_LEFT:
				case MediaPortal.GUI.Library.Action.ActionType.ACTION_MOVE_RIGHT:

					base.OnAction(action);
					break;
				case MediaPortal.GUI.Library.Action.ActionType.ACTION_KEY_PRESSED:
					
					//when the list is selected, search the input
					if (GUIWindowManager.ActiveWindowEx == this.GetID)
					{
						if ((m_Facade.CurrentLayout == GUIFacadeControl.Layout.List && m_Facade.ListLayout.IsFocused)
							|| (m_Facade.CurrentLayout == GUIFacadeControl.Layout.LargeIcons && m_Facade.ThumbnailLayout.IsFocused)
							|| (m_Facade.CurrentLayout == GUIFacadeControl.Layout.Filmstrip && m_Facade.FilmstripLayout.IsFocused)
							|| (m_Facade.CurrentLayout == GUIFacadeControl.Layout.CoverFlow && m_Facade.CoverFlowLayout.IsFocused))
							OnSearchChar((char)(action.m_key.KeyChar));
					}
					break;

				case MediaPortal.GUI.Library.Action.ActionType.ACTION_PARENT_DIR:
				case MediaPortal.GUI.Library.Action.ActionType.ACTION_HOME:
					UpdateSearchPanel(false);
					ImageAllocator.FlushAll();
					GUIWindowManager.ShowPreviousWindow();
					break;

				case MediaPortal.GUI.Library.Action.ActionType.ACTION_PLAY:
					BaseConfig.MyAnimeLog.Write("Received PLAY action");

					try
					{
						if (listLevel == Listlevel.Group)
						{
							if (curAnimeGroup == null) return;
							JMMServerBinary.Contract_AnimeEpisode contract = JMMServerVM.Instance.clientBinaryHTTP.GetNextUnwatchedEpisodeForGroup(curAnimeGroup.AnimeGroupID,
								JMMServerVM.Instance.CurrentUser.JMMUserID);
							if (contract == null) return;
							AnimeEpisodeVM ep = new AnimeEpisodeVM(contract);
							vidHandler.ResumeOrPlay(ep);
						}

						if (listLevel == Listlevel.Series)
						{
							//curAnimeSeries = null;
							if (curAnimeSeries == null) return;
							JMMServerBinary.Contract_AnimeEpisode contract = JMMServerVM.Instance.clientBinaryHTTP.GetNextUnwatchedEpisode(curAnimeSeries.AnimeSeriesID.Value,
								JMMServerVM.Instance.CurrentUser.JMMUserID);
							if (contract == null) return;
							AnimeEpisodeVM ep = new AnimeEpisodeVM(contract);
							vidHandler.ResumeOrPlay(ep);
						}
					}
					catch (Exception ex)
					{
						BaseConfig.MyAnimeLog.Write(ex.ToString());
					}
					break;

				case MediaPortal.GUI.Library.Action.ActionType.ACTION_PREVIOUS_MENU:
					if (searchTimer.Enabled)
					{
						OnSearchAction(SearchAction.EndSearch);
						return;
					}

					// back one level
					if (listLevel == Listlevel.GroupFilter)
					{
						goto case MediaPortal.GUI.Library.Action.ActionType.ACTION_HOME;
					}
					else
					{
						string msg = string.Format("LIST LEVEL:: {0} - GF: {1} - GFSub2: {2}", listLevel, curGroupFilter, curGroupFilterSub2);

						BaseConfig.MyAnimeLog.Write(msg);
						if (listLevel == Listlevel.GroupFilterSub)
						{
							listLevel = Listlevel.GroupFilter;
							curGroupFilterSub = null;

							LoadFacade();
						}
						if (listLevel == Listlevel.GroupFilterSub2)
						{
							// go back to GROUP FILTERS
							listLevel = Listlevel.GroupFilterSub;
							curGroupFilterSub2 = null;

							LoadFacade();
						}
						if (listLevel == Listlevel.Group)
						{
							if (curGroupFilterSub2 == null)
							{
								// go back to GROUP FILTERS
								listLevel = Listlevel.GroupFilter;
							}
							else
							{
								listLevel = Listlevel.GroupFilterSub2;
							}
							LoadFacade();
							curAnimeGroup = null;
						}

						if (listLevel == Listlevel.Series)
						{
							// go back to GROUP
							AnimeGroupVM parentGroup = curAnimeGroupViewed.ParentGroup;
							if (parentGroup == null)
								listLevel = Listlevel.Group;

							ShowParentLevelForGroup(parentGroup);

							LoadFacade();
							curAnimeEpisodeType = null;
							curAnimeSeries = null;
						}

						
						if (listLevel == Listlevel.EpisodeTypes)
						{
							// go back to SERIES
							AnimeSeriesVM parentSeries = curAnimeEpisodeType.AnimeSeries;
							ShowParentLevelForSeries(parentSeries);
							LoadFacade();
							return;
						}
						
						if (listLevel == Listlevel.Episode)
						{
							AnimeSeriesVM parentSeries = curAnimeEpisodeType.AnimeSeries;
							if (parentSeries.EpisodeTypesToDisplay.Count == 1)
								ShowParentLevelForSeries(parentSeries);
							else
							{
								listLevel = Listlevel.EpisodeTypes;
								curAnimeEpisodeType = null;
							}

							LoadFacade();
							return;
						}
					}
					break;

				default:
					base.OnAction(action);
					break;
			}
		}

		private void ShowParentLevelForGroup(AnimeGroupVM grp)
		{
			while (grp != null)
			{
				List<AnimeGroupVM> subGroups = grp.SubGroups;
				List<AnimeSeriesVM> seriesList = grp.ChildSeries;

				if ((seriesList.Count + subGroups.Count) > 1)
				{
					curAnimeGroupViewed = grp;
					curAnimeGroup = grp;

					listLevel = Listlevel.Series;
					return;
				}
				else
				{
					// go up one level
					if (grp.AnimeGroupParentID.HasValue)
						grp = grp.ParentGroup;
					else
					{
						// only one series or subgroup so go all the way back to the group list
						listLevel = Listlevel.Group;
						curAnimeEpisodeType = null;
						curAnimeSeries = null;
						return;
					}
				}
			}
		}

		private void ShowParentLevelForSeries(AnimeSeriesVM ser)
		{
			AnimeGroupVM grp = JMMServerHelper.GetGroup(ser.AnimeGroupID);
			ShowParentLevelForGroup(grp);
		}

		#region Find

		#region Keyboard hook
		KeyboardHook hook = null;
		private const string t9Chars = "0123456789";

		bool IsChar(Keys k, ref char c)
		{
			string str = new KeysConverter().ConvertToString(k);
			if (str.Length != 1)
				return false;
			c = str[0];
			return true;
		}

		bool IsSearchChar(char c)
		{
			if (search == null)
				return false;

			if (search.Mode == SearchMode.t9)
				return (t9Chars.IndexOf(c) > 0) || (c == '*') || (c == '#');

			if (search.Mode == SearchMode.text)
				return char.IsLetterOrDigit(c) || char.IsPunctuation(c) || (c == ' ');

			return false;
		}

		void hook_KeyDown(object sender, KeyEventArgsEx e)
		{
			if (GUIWindowManager.ActiveWindowEx != this.GetID)
				return;


			//mark normal keys as handled, otherwise some other handler might be called
			// (like 'home' for 'h')
			if (!e.Alt && !e.Control && IsSearchChar(e.keyChar) && MediaPortal.Player.g_Player.Playing == false)
				e.Handled = true;
		}

		void hook_KeyUp(object sender, KeyEventArgsEx e)
		{
			if (GUIWindowManager.ActiveWindowEx != this.GetID)
				return;

			try
			{
				//BaseConfig.MyAnimeLog.Write("e.KeyCode: {0} - {1} - {2}", e.KeyCode, e.keyChar, e.KeyValue);
			}
			catch { }

			//when the list is selected, search the input
			if ((m_Facade.CurrentLayout == GUIFacadeControl.Layout.List && m_Facade.ListLayout.IsFocused)
				|| (m_Facade.CurrentLayout == GUIFacadeControl.Layout.LargeIcons && m_Facade.ThumbnailLayout.IsFocused)
				|| (m_Facade.CurrentLayout == GUIFacadeControl.Layout.Filmstrip && m_Facade.FilmstripLayout.IsFocused)
				|| (m_Facade.CurrentLayout == GUIFacadeControl.Layout.CoverFlow && m_Facade.CoverFlowLayout.IsFocused))
			{
				e.Handled = true;

				//catch ctrl+w and ctrl+m for toggling search word and mode
				if (e.Control)
				{
					switch (e.KeyCode)
					{
						case Keys.W:
							OnSearchAction(SearchAction.ToggleStartWord);
							break;
						case Keys.M:
							OnSearchAction(SearchAction.ToggleMode);
							break;
						default:
							e.Handled = false;
							return; //do nothing
					}
				}
				else
				{
					switch (e.KeyCode)
					{
						case Keys.Back:
							OnSearchAction(SearchAction.DeleteChar);
							break;
						case Keys.Tab:
							OnSearchAction(SearchAction.NextMatch);
							break;
						default:
							if (!OnSearchChar(e.keyChar))
							{
								e.Handled = false;
								return; //do nothing
							}
							break;
					}
				}

				if (e.Handled)
				{
					//we handled the keypress: make some noise
					if (!string.IsNullOrEmpty(searchSound) && !MediaPortal.Player.g_Player.Playing)
						MediaPortal.Util.Utils.PlaySound(searchSound, false, true);
				}
			}
		}
		#endregion

		string searchText = string.Empty;
		string searchMatch = string.Empty;

		enum SearchAction { ToggleMode, ToggleStartWord, NextMatch, DeleteChar, EndSearch };
		private void OnSearchAction(SearchAction action)
		{
			//stop timer
			if (searchTimer.Enabled)
				searchTimer.Stop();

			//process action
			switch (action)
			{
				case SearchAction.ToggleMode:
					search.Input = string.Empty;
					searchText = string.Empty;
					searchMatch = string.Empty;
					search.Mode = (search.Mode == SearchMode.text) ? SearchMode.t9 : SearchMode.text;
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
				else if (t9Chars.IndexOf(c) >= 0)
				{
					AddSearchChar(c);
					return true;
				}
			}
			else if (search.Mode == SearchMode.text)
			{
				if (char.IsLetterOrDigit(c) || char.IsPunctuation(c) || (c == ' '))
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
			if (searchTimer.Enabled)
				searchTimer.Stop();

			//add char
			if (search.Mode == SearchMode.t9)
			{
				int n = (int)c - (int)'0';
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
					{
						if (lstMatches.Contains(index))
						{
							lstMatchingItems.Add(m_Facade[index]);
							if (index == match.Index)
								selectedItem = lstMatchingItems.Count - 1; ;
						}
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

		void searchTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			UpdateSearchPanel(false);
		}

		private void UpdateSearchPanel(bool bShow)
		{
			if (searchTimer.Enabled)
				searchTimer.Stop();

			if (dummyFindActive != null)
				dummyFindActive.Visible = bShow;
			if (dummyFindModeT9 != null)
				dummyFindModeT9.Visible = (search.Mode == SearchMode.t9);
			if (dummyFindModeText != null)
				dummyFindModeText.Visible = (search.Mode == SearchMode.text);

			if (!bShow)
			{
				search.Input = string.Empty;
				searchText = string.Empty;
				searchMatch = string.Empty;

				if (settings.FindFilter)
					SaveOrRestoreFacadeItems(false);
			}

			setGUIProperty(guiProperty.FindInput, search.Input);
			setGUIProperty(guiProperty.FindText, searchText);
			setGUIProperty(guiProperty.FindMatch, searchMatch);
			string searchMode = (search.Mode == SearchMode.t9) ? "T9" : "Text";
			string startWord = search.StartWord ? "yes" : "no";
			setGUIProperty(guiProperty.FindMode, searchMode);
			setGUIProperty(guiProperty.FindStartWord, startWord);
			if (search.Mode == SearchMode.t9)
			{
				if (string.IsNullOrEmpty(searchText))
				{
					setGUIProperty(guiProperty.FindSharpMode, "Next Match");
					setGUIProperty(guiProperty.FindAsteriskMode, string.Format("Start Word ({0})", startWord));
				}
				else
				{
					setGUIProperty(guiProperty.FindSharpMode, "Next Match");
					setGUIProperty(guiProperty.FindAsteriskMode, "Clear");
				}
			}

			if (bShow)
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

			if (m_Facade.SelectedListItem.TVTag.GetType() == typeof(GroupFilterVM))
				GroupFilter_OnItemSelected(m_Facade.SelectedListItem);

			if (m_Facade.SelectedListItem.TVTag.GetType() == typeof(AnimeGroupVM))
				Group_OnItemSelected(m_Facade.SelectedListItem);

			if (m_Facade.SelectedListItem.TVTag.GetType() == typeof(AnimeSeriesVM))
				Series_OnItemSelected(m_Facade.SelectedListItem);

			if (m_Facade.SelectedListItem.TVTag.GetType() == typeof(AnimeEpisodeTypeVM))
				EpisodeType_OnItemSelected(m_Facade.SelectedListItem);

			if (m_Facade.SelectedListItem.TVTag.GetType() == typeof(AnimeEpisodeVM))
				Episode_OnItemSelected(m_Facade.SelectedListItem);

			EvaluateVisibility();
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

							if (m_Facade.SelectedListItem != null && m_Facade.SelectedListItem.TVTag != null)
							{
								if (m_Facade.SelectedListItem.TVTag.GetType() == typeof(GroupFilterVM))
									GroupFilter_OnItemSelected(m_Facade.SelectedListItem);

								if (m_Facade.SelectedListItem.TVTag.GetType() == typeof(AnimeGroupVM))
									Group_OnItemSelected(m_Facade.SelectedListItem);

								if (m_Facade.SelectedListItem.TVTag.GetType() == typeof(AnimeSeriesVM))
									Series_OnItemSelected(m_Facade.SelectedListItem);

								if (m_Facade.SelectedListItem.TVTag.GetType() == typeof(AnimeEpisodeTypeVM))
									EpisodeType_OnItemSelected(m_Facade.SelectedListItem);

								if (m_Facade.SelectedListItem.TVTag.GetType() == typeof(AnimeEpisodeVM))
									Episode_OnItemSelected(m_Facade.SelectedListItem);
							}
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

		void g_Player_PlayBackEnded(g_Player.MediaType type, string filename)
		{
			
		}

		static bool isFirstInitDone = false;
		private void OnFirstStart()
		{
			if (isFirstInitDone)
				return;

			if (string.IsNullOrEmpty(MainWindow.settings.JMMServer_Address) || string.IsNullOrEmpty(MainWindow.settings.JMMServer_Port))
			{
				Utils.DialogMsg("Error", "Please exit MP and set your JMM Server location first");
				return;
			}
				

			// check if we can connect to JMM Server
			// and load the default user
			List<JMMUserVM> allUsers = JMMServerHelper.GetAllUsers();
			if (allUsers.Count == 0)
			{
				Utils.DialogMsg("Error", "Error connecting to JMM Server");
				return;
			}
			else
			{
				// check for last jmm user
				if (string.IsNullOrEmpty(MainWindow.settings.CurrentJMMUserID))
				{
					if (!JMMServerVM.Instance.PromptUserLogin())
						return;

					// user has logged in, so save to settings so we will log in as the same user next time
					MainWindow.settings.CurrentJMMUserID = JMMServerVM.Instance.CurrentUser.JMMUserID.ToString();
					settings.Save();
				}
				else
				{
					JMMUserVM selUser = null;
					foreach (JMMUserVM user in allUsers)
					{
						if (user.JMMUserID.ToString().Equals(MainWindow.settings.CurrentJMMUserID))
						{
							selUser = user;
							break;
						}
					}

					if (selUser == null)
					{
						if (!JMMServerVM.Instance.PromptUserLogin())
							return;

						// user has logged in, so save to settings so we will log in as the same user next time
						MainWindow.settings.CurrentJMMUserID = JMMServerVM.Instance.CurrentUser.JMMUserID.ToString();
						settings.Save();
					}
					else
					{
						bool authed = JMMServerVM.Instance.AuthenticateUser(selUser.Username, "");
						string password = "";
						while (!authed)
						{
							// prompt user for a password
							if (Utils.DialogText(ref password, true, GUIWindowManager.ActiveWindow))
							{
								authed = JMMServerVM.Instance.AuthenticateUser(selUser.Username, password);
								if (!authed)
								{
									Utils.DialogMsg("Error", "Incorrect password, please try again");
								}
							}
							else return;
						}

						JMMServerVM.Instance.SetCurrentUser(selUser);

						// user has logged in, so save to settings so we will log in as the same user next time
						settings.CurrentJMMUserID = JMMServerVM.Instance.CurrentUser.JMMUserID.ToString();
						settings.Save();
					}
				}
			}

			JMMServerVM.Instance.ServerStatusEvent += new JMMServerVM.ServerStatusEventHandler(Instance_ServerStatusEvent);

			isFirstInitDone = true;
		}

		private void GroupFilter_OnItemSelected(GUIListItem item)
		{
			GroupFilterVM grpFilter = item.TVTag as GroupFilterVM;
			if (grpFilter == null) return;

			curGroupFilter = grpFilter;

			if (displayGrpFilterTimer != null)
				displayGrpFilterTimer.Stop();

			displayGrpFilterTimer = new System.Timers.Timer();
			displayGrpFilterTimer.AutoReset = false;
			displayGrpFilterTimer.Interval = BaseConfig.Settings.InfoDelay; // 250ms
			displayGrpFilterTimer.Elapsed += new System.Timers.ElapsedEventHandler(displayGrpFilterTimer_Elapsed);
			displayGrpFilterTimer.Enabled = true;
		}

		void displayGrpFilterTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			GroupFilter_OnItemSelectedDisplay();
		}

		private void GroupFilter_OnItemSelectedDisplay()
		{
			if (curGroupFilter == null) return;

			clearGUIProperty("GroupFilter.FilterName");
			clearGUIProperty("GroupFilter.GroupCount");

			setGUIProperty(guiProperty.SeriesTitle, curGroupFilter.GroupFilterName);
			setGUIProperty(guiProperty.Title, curGroupFilter.GroupFilterName);
			setGUIProperty("GroupFilter.FilterName", curGroupFilter.GroupFilterName);

			AnimeGroupVM grp = null;
			List<AnimeGroupVM> groups = JMMServerHelper.GetAnimeGroupsForFilter(curGroupFilter);
			if (groups.Count > 0)
			{
				grp = groups[groupRandom.Next(0, groups.Count - 1)];
			}
			setGUIProperty("GroupFilter.GroupCount", groups.Count.ToString());

			if (grp != null)
			{
				// Delayed Image Loading of Series Banners            
				listPoster.Filename = ImageAllocator.GetGroupImageAsFileName(grp, GUIFacadeControl.Layout.List);

				LoadFanart(grp);
			}

		}

		private void Group_OnItemSelected(GUIListItem item)
		{
			if (item == null || item.TVTag == null || !(item.TVTag is AnimeGroupVM))
				return;

			AnimeGroupVM grp = item.TVTag as AnimeGroupVM;
			if (grp == null) return;

			curAnimeGroup = grp;

			if (displayGrpTimer != null)
				displayGrpTimer.Stop();

			displayGrpTimer = new System.Timers.Timer();
			displayGrpTimer.AutoReset = false;
			displayGrpTimer.Interval = BaseConfig.Settings.InfoDelay; // 250ms
			displayGrpTimer.Elapsed += new System.Timers.ElapsedEventHandler(displayGrpTimer_Elapsed);
			displayGrpTimer.Enabled = true;
		}

		void displayGrpTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			Group_OnItemSelectedDisplay();
		}

		private void Group_OnItemSelectedDisplay()
		{
			// when displaying a group we could be at the group list level or series list level
			// group level = top level groups
			// series level = sub-groups

			clearGUIProperty("SeriesGroup.Year");
			clearGUIProperty("SeriesGroup.Genre");
			clearGUIProperty("SeriesGroup.Episodes");
			clearGUIProperty("SeriesGroup.EpisodesAvailable");
			clearGUIProperty("SeriesGroup.Rating");
			clearGUIProperty("SeriesGroup.EpisodeCountNormal");
			clearGUIProperty("SeriesGroup.EpisodeCountSpecial");
			clearGUIProperty("SeriesGroup.EpisodeCountNormalAvailable");
			clearGUIProperty("SeriesGroup.EpisodeCountSpecialAvailable");
			clearGUIProperty("SeriesGroup.EpisodeCountUnwatched");
			clearGUIProperty("SeriesGroup.EpisodeCountWatched");
			clearGUIProperty("SeriesGroup.RawRating");
			clearGUIProperty("SeriesGroup.RatingVoteCount");
			clearGUIProperty("SeriesGroup.MyRating");
			clearGUIProperty("SeriesGroup.SeriesCount");
			clearGUIProperty(guiProperty.Description);

			if (curAnimeGroup == null) return;

			
			setGUIProperty(guiProperty.Subtitle, "");

			
			if (curAnimeGroup.Stat_UserVoteOverall.HasValue)
				setGUIProperty("SeriesGroup.MyRating", Utils.FormatAniDBRating((double)curAnimeGroup.Stat_UserVoteOverall.Value));

			// set info properties
			// most of these properties actually come from the anidb_anime record
			// we need to find all the series for this group

			setGUIProperty("SeriesGroup.SeriesCount", curAnimeGroup.AllSeriesCount.ToString());
			setGUIProperty("SeriesGroup.Genre", curAnimeGroup.CategoriesFormatted);
			setGUIProperty("SeriesGroup.GenreShort", curAnimeGroup.CategoriesFormattedShort);

			setGUIProperty("SeriesGroup.Year", curAnimeGroup.YearFormatted);


			decimal totalRating = 0;
			int totalVotes = 0;
			int epCountNormal = 0;
			int epCountSpecial = 0;

			List<AnimeSeriesVM> seriesList = curAnimeGroup.ChildSeries;

			if (seriesList.Count == 1)
			{
				setGUIProperty(guiProperty.SeriesTitle, seriesList[0].SeriesName);
				setGUIProperty(guiProperty.Title, seriesList[0].SeriesName);
				setGUIProperty(guiProperty.Description, seriesList[0].Description);
			}
			else
			{
				setGUIProperty(guiProperty.SeriesTitle, curAnimeGroup.GroupName);
				setGUIProperty(guiProperty.Title, curAnimeGroup.GroupName);
				setGUIProperty(guiProperty.Description, curAnimeGroup.ParsedDescription);
			}

			foreach (AnimeSeriesVM ser in seriesList)
			{
				totalRating += ((decimal)ser.AniDB_Anime.Rating * ser.AniDB_Anime.VoteCount);
				totalRating += ((decimal)ser.AniDB_Anime.TempRating * ser.AniDB_Anime.TempVoteCount);

				totalVotes += ser.AniDB_Anime.AniDBTotalVotes;

				epCountNormal += ser.AniDB_Anime.EpisodeCountNormal;
				epCountSpecial += ser.AniDB_Anime.EpisodeCountSpecial;
			}
			decimal AniDBRating = 0;
			if (totalVotes == 0)
				AniDBRating = 0;
			else
				AniDBRating = totalRating / (decimal)totalVotes / (decimal)100;

			if (dummyUserHasVotedSeries != null) dummyUserHasVotedSeries.Visible = false;
			// Only AniDB users have votes
			BaseConfig.MyAnimeLog.Write("IsAniDBUserBool : " + JMMServerVM.Instance.CurrentUser.IsAniDBUserBool.ToString());
			if (JMMServerVM.Instance.CurrentUser.IsAniDBUserBool)
			{
				BaseConfig.MyAnimeLog.Write("seriesList.Count : " + seriesList.Count.ToString());
				if (seriesList.Count == 1)
				{
					AniDB_AnimeVM anAnime = seriesList[0].AniDB_Anime;
					string myRating = anAnime.UserVoteFormatted;
					if (string.IsNullOrEmpty(myRating))
						clearGUIProperty("SeriesGroup.MyRating");
					else
					{
						setGUIProperty("SeriesGroup.MyRating", myRating);
						if (dummyUserHasVotedSeries != null) dummyUserHasVotedSeries.Visible = true;

						BaseConfig.MyAnimeLog.Write("myRating : " + myRating.ToString());
						BaseConfig.MyAnimeLog.Write("dummyUserHasVotedSeries.Visible : " + dummyUserHasVotedSeries.Visible.ToString());
					}
				}
				
			}


			string rating = Utils.FormatAniDBRating((double)AniDBRating) + " (" + totalVotes.ToString() + " votes)";
			setGUIProperty("SeriesGroup.RawRating", Utils.FormatAniDBRating((double)AniDBRating));
			setGUIProperty("SeriesGroup.RatingVoteCount", totalVotes.ToString());
			setGUIProperty("SeriesGroup.Rating", rating);

			// set watched/unavailable flag
			if (dummyIsWatched != null) dummyIsWatched.Visible = (curAnimeGroup.UnwatchedEpisodeCount == 0);



			string eps = epCountNormal.ToString() + " (" + epCountSpecial.ToString() + " Specials)";
			//string epsa = epCountNormalAvailable.ToString() + " (" + epCountSpecialAvailable.ToString() + " Specials)";


			setGUIProperty("SeriesGroup.Episodes", eps);
			//setGUIProperty("SeriesGroup.EpisodesAvailable", epsa);

			setGUIProperty("SeriesGroup.EpisodeCountNormal", epCountNormal.ToString());
			setGUIProperty("SeriesGroup.EpisodeCountSpecial", epCountSpecial.ToString());
			//setGUIProperty("SeriesGroup.EpisodeCountNormalAvailable", epCountNormalAvailable.ToString());
			//setGUIProperty("SeriesGroup.EpisodeCountSpecialAvailable", epCountSpecialAvailable.ToString());
			setGUIProperty("SeriesGroup.EpisodeCountUnwatched", curAnimeGroup.UnwatchedEpisodeCount.ToString());
			setGUIProperty("SeriesGroup.EpisodeCountWatched", curAnimeGroup.WatchedEpisodeCount.ToString());

			// Delayed Image Loading of Series Banners            
			listPoster.Filename = ImageAllocator.GetGroupImageAsFileName(curAnimeGroup, GUIFacadeControl.Layout.List);

			LoadFanart(curAnimeGroup);

		}

		private void EpisodeType_OnItemSelected(GUIListItem item)
		{
			if (m_bQuickSelect)
				return;

			if (dummyIsAvailable != null) dummyIsAvailable.Visible = true;

			clearGUIProperty("SeriesGroup.Year");
			clearGUIProperty("SeriesGroup.Genre");
			clearGUIProperty("SeriesGroup.Episodes");
			clearGUIProperty("SeriesGroup.Rating");
			clearGUIProperty(guiProperty.SeriesTitle);
			clearGUIProperty(guiProperty.Subtitle);
			clearGUIProperty(guiProperty.Description);
			clearGUIProperty("SeriesGroup.EpisodeCountNormal");
			clearGUIProperty("SeriesGroup.EpisodeCountSpecial");
			clearGUIProperty("SeriesGroup.EpisodeCountUnwatched");
			clearGUIProperty("SeriesGroup.EpisodeCountWatched");
			clearGUIProperty("SeriesGroup.RatingVoteCount");
			clearGUIProperty("SeriesGroup.RawRating");

			if (item == null || item.TVTag == null || !(item.TVTag is AnimeEpisodeTypeVM))
				return;


			AnimeEpisodeTypeVM epType = item.TVTag as AnimeEpisodeTypeVM;
			if (epType == null) return;

			curAnimeEpisodeType = epType;

			if (curAnimeSeries == null) return;
			AniDB_AnimeVM anAnime = curAnimeSeries.AniDB_Anime;

			setGUIProperty(guiProperty.SeriesTitle, curAnimeSeries.SeriesName);
			setGUIProperty(guiProperty.Subtitle, "");
			setGUIProperty(guiProperty.Description, curAnimeSeries.Description);

			// set info properties
			// most of these properties actually come from the anidb_anime record
			// we need to find all the series for this group

			setGUIProperty("SeriesGroup.Year", anAnime.Year);
			setGUIProperty("SeriesGroup.Genre", anAnime.CategoriesFormatted);
			setGUIProperty("SeriesGroup.GenreShort", anAnime.CategoriesFormattedShort);

			string eps = anAnime.EpisodeCountNormal.ToString() + " (" + anAnime.EpisodeCountSpecial.ToString() + " Specials)";
			setGUIProperty("SeriesGroup.Episodes", eps);

			setGUIProperty("SeriesGroup.EpisodeCountNormal", anAnime.EpisodeCountNormal.ToString());
			setGUIProperty("SeriesGroup.EpisodeCountSpecial", anAnime.EpisodeCountSpecial.ToString());

			string rating = "";

			rating = Utils.FormatAniDBRating((double)anAnime.AniDBRating) + " (" + anAnime.AniDBTotalVotes.ToString() + " votes)";
			setGUIProperty("SeriesGroup.RawRating", Utils.FormatAniDBRating((double)anAnime.AniDBRating));
			setGUIProperty("SeriesGroup.RatingVoteCount", anAnime.AniDBTotalVotes.ToString());

			setGUIProperty("SeriesGroup.Rating", rating);


			int unwatched = 0;
			int watched = 0;
			if (curAnimeSeries != null)
				curAnimeSeries.GetWatchedUnwatchedCount(epType.EpisodeType, ref unwatched, ref watched);

			// set watched/unavailable flag
			if (dummyIsWatched != null) dummyIsWatched.Visible = (unwatched == 0);

			setGUIProperty("SeriesGroup.EpisodeCountUnwatched", unwatched.ToString());
			setGUIProperty("SeriesGroup.EpisodeCountWatched", watched.ToString());


			// Delayed Image Loading of Series Banners            
			listPoster.Filename = ImageAllocator.GetSeriesImageAsFileName(curAnimeSeries, GUIFacadeControl.Layout.List);
		}

		private void Series_OnItemSelected(GUIListItem item)
		{
			// need to do this here as well because we display series and sub-groups in the same list
			if (displayGrpTimer != null)
				displayGrpTimer.Stop();


			if (m_bQuickSelect)
				return;

			if (dummyIsAvailable != null) dummyIsAvailable.Visible = true;

			clearGUIProperty("SeriesGroup.Year");
			clearGUIProperty("SeriesGroup.Genre");
			clearGUIProperty("SeriesGroup.Episodes");
			clearGUIProperty("SeriesGroup.EpisodesAvailable");
			clearGUIProperty("SeriesGroup.Rating");
			clearGUIProperty(guiProperty.SeriesTitle);
			clearGUIProperty(guiProperty.Subtitle);
			clearGUIProperty(guiProperty.Description);
			clearGUIProperty("SeriesGroup.EpisodeCountNormal");
			clearGUIProperty("SeriesGroup.EpisodeCountSpecial");
			clearGUIProperty("SeriesGroup.EpisodeCountNormalAvailable");
			clearGUIProperty("SeriesGroup.EpisodeCountSpecialAvailable");
			clearGUIProperty("SeriesGroup.EpisodeCountUnwatched");
			clearGUIProperty("SeriesGroup.EpisodeCountWatched");
			clearGUIProperty("SeriesGroup.RatingVoteCount");
			clearGUIProperty("SeriesGroup.RawRating");
			clearGUIProperty("SeriesGroup.MyRating");
			clearGUIProperty(guiProperty.RomanjiTitle);
			clearGUIProperty(guiProperty.EnglishTitle);
			clearGUIProperty(guiProperty.KanjiTitle);
			clearGUIProperty(guiProperty.RotatorTitle);
			if (item == null || item.TVTag == null || !(item.TVTag is AnimeSeriesVM))
				return;


			AnimeSeriesVM ser = item.TVTag as AnimeSeriesVM;
			if (ser == null) return;

			BaseConfig.MyAnimeLog.Write(ser.ToString());

			curAnimeSeries = ser;

			setGUIProperty(guiProperty.SeriesTitle, ser.SeriesName);
			setGUIProperty(guiProperty.Subtitle, "");
			setGUIProperty(guiProperty.Description, ser.Description);


			


			// set info properties
			// most of these properties actually come from the anidb_anime record
			// we need to find all the series for this group

			AniDB_AnimeVM anAnime = ser.AniDB_Anime;

			if (dummyUserHasVotedSeries != null) dummyUserHasVotedSeries.Visible = false;
			// Only AniDB users have votes
			if (JMMServerVM.Instance.CurrentUser.IsAniDBUserBool)
			{
				string myRating = anAnime.UserVoteFormatted;
				if (string.IsNullOrEmpty(myRating))
					clearGUIProperty("SeriesGroup.MyRating");
				else
				{
					setGUIProperty("SeriesGroup.MyRating", myRating);
					if (dummyUserHasVotedSeries != null) dummyUserHasVotedSeries.Visible = true;
				}
			}

			BaseConfig.MyAnimeLog.Write(anAnime.ToString());

			setGUIProperty("SeriesGroup.Year", anAnime.Year);
			setGUIProperty("SeriesGroup.Genre", anAnime.CategoriesFormatted);
			setGUIProperty("SeriesGroup.GenreShort", anAnime.CategoriesFormattedShort);


			string eps = anAnime.EpisodeCountNormal.ToString() + " (" + anAnime.EpisodeCountSpecial.ToString() + " Specials)";
			setGUIProperty("SeriesGroup.Episodes", eps);

			setGUIProperty("SeriesGroup.EpisodeCountNormal", anAnime.EpisodeCountNormal.ToString());
			setGUIProperty("SeriesGroup.EpisodeCountSpecial", anAnime.EpisodeCountSpecial.ToString());

			string rating = "";

			rating = Utils.FormatAniDBRating((double)anAnime.AniDBRating) + " (" + anAnime.AniDBTotalVotes.ToString() + " votes)";
			setGUIProperty("SeriesGroup.RawRating", Utils.FormatAniDBRating((double)anAnime.AniDBRating));
			setGUIProperty("SeriesGroup.RatingVoteCount", anAnime.AniDBTotalVotes.ToString());

			setGUIProperty("SeriesGroup.Rating", rating);

			

			// set watched/unavailable flag
			if (dummyIsWatched != null) dummyIsWatched.Visible = (ser.UnwatchedEpisodeCount == 0);

			setGUIProperty("SeriesGroup.EpisodeCountUnwatched", ser.UnwatchedEpisodeCount.ToString());
			setGUIProperty("SeriesGroup.EpisodeCountWatched", ser.WatchedEpisodeCount.ToString());


			// Delayed Image Loading of Series Banners
			listPoster.Filename = ImageAllocator.GetSeriesImageAsFileName(ser, GUIFacadeControl.Layout.List);


			LoadFanart(ser);
		}

		private string GetSeriesTypeLabel()
		{
			if (curAnimeEpisodeType != null) return curAnimeEpisodeType.EpisodeTypeDescription;

			if (curAnimeSeries != null) return curAnimeSeries.SeriesName;

			if (curAnimeGroup != null) return curAnimeGroup.GroupName;

			return "";

		}

		private void Episode_OnItemSelected(GUIListItem item)
		{
			if (m_bQuickSelect)
				return;

			clearGUIProperty("Episode.MyRating");
			clearGUIProperty("Episode.AirDate");
			clearGUIProperty("Episode.Rating");
			clearGUIProperty("Episode.RawRating");
			clearGUIProperty("Episode.Length");
			clearGUIProperty("Episode.FileInfo");
			clearGUIProperty("Episode.EpisodeName");
			clearGUIProperty("Episode.EpisodeDisplayName");
			clearGUIProperty("Episode.Description");
			clearGUIProperty("Episode.Image");
			clearGUIProperty("Episode.SeriesTypeLabel");
			clearGUIProperty("Episode.EpisodeRomanjiName");
			clearGUIProperty("Episode.EpisodeEnglishName");
			clearGUIProperty("Episode.EpisodeKanjiName");
			clearGUIProperty("Episode.EpisodeRotator");
			if (item == null || item.TVTag == null || !(item.TVTag is AnimeEpisodeVM))
				return;


			AnimeEpisodeVM ep = item.TVTag as AnimeEpisodeVM;
			if (ep == null) return;

			curAnimeEpisode = ep;

			if (curAnimeEpisode.EpisodeImageLocation.Length > 0)
				setGUIProperty("Episode.Image", curAnimeEpisode.EpisodeImageLocation);

			if (!settings.HidePlot)
				setGUIProperty("Episode.Description", curAnimeEpisode.EpisodeOverview);
			else
			{
				if (curAnimeEpisode.EpisodeOverview.Trim().Length > 0 && ep.IsWatched == 0)
					setGUIProperty("Episode.Description", "*** Hidden to prevent spoilers ***");
				else
					setGUIProperty("Episode.Description", curAnimeEpisode.EpisodeOverview);
			}

			setGUIProperty("Episode.EpisodeName", curAnimeEpisode.EpisodeName);
			setGUIProperty("Episode.EpisodeDisplayName", curAnimeEpisode.DisplayName);
			setGUIProperty("Episode.SeriesTypeLabel", GetSeriesTypeLabel());

			setGUIProperty("Episode.AirDate", curAnimeEpisode.AirDateAsString);
			setGUIProperty("Episode.Length", Utils.FormatSecondsToDisplayTime(curAnimeEpisode.AniDB_LengthSeconds));

			setGUIProperty("Episode.Rating", curAnimeEpisode.AniDBRatingFormatted);
			//setGUIProperty("Episode.RawRating", Utils.FormatAniDBRating(curAnimeEpisode.AniDB_Rating));
			setGUIProperty("Episode.RawRating", curAnimeEpisode.AniDB_Rating);

			if (dummyIsWatched != null) dummyIsWatched.Visible = (ep.IsWatched == 1);



			if (dummyIsAvailable != null) dummyIsAvailable.Visible = true;
			// get all the LocalFile rceords for this episode
			List<VideoDetailedVM> fileLocalList = ep.FilesForEpisode;
			bool norepeat = true;
			string finfo = "";
			foreach (VideoDetailedVM vid in fileLocalList)
				finfo = vid.FileSelectionDisplay;
			
			if (fileLocalList.Count == 1)
			{
				setGUIProperty("Episode.FileInfo", finfo);
			}
			else if (fileLocalList.Count > 1)
			{
				setGUIProperty("Episode.FileInfo", fileLocalList.Count.ToString() + " Files Available");
			}
			else if (fileLocalList.Count == 0)
			{
				if (dummyIsAvailable != null) dummyIsAvailable.Visible = false;
			}

			string logos = Logos.buildLogoImage(ep);
			//MyAnimeLog.Write(logos);

			BaseConfig.MyAnimeLog.Write(logos);
			setGUIProperty(guiProperty.Logos, logos);
		}

		#region GUI Properties

		public enum guiProperty
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
			NotificationAction
		}

		private string getGUIProperty(guiProperty which)
		{
			return getGUIProperty(which.ToString());
		}

		public static string getGUIProperty(string which)
		{
			return MediaPortal.GUI.Library.GUIPropertyManager.GetProperty("#Anime3." + which);
		}

		private void setGUIProperty(guiProperty which, string value)
		{
			setGUIProperty(which.ToString(), value);
		}

		public static void setGUIProperty(string which, string value)
		{
			MediaPortal.GUI.Library.GUIPropertyManager.SetProperty("#Anime3." + which, value);
		}

		private void clearGUIProperty(guiProperty which)
		{
			setGUIProperty(which, string.Empty);
		}

		public static void clearGUIProperty(string which)
		{
			setGUIProperty(which, "-"); // String.Empty doesn't work on non-initialized fields, as a result they would display as ugly #TVSeries.bla.bla
		}

		#endregion

		private void LoadFanart(object objectWithFanart)
		{
			//BaseConfig.MyAnimeLog.Write("LOADING FANART FOR:: {0}", objectWithFanart.ToString());

			try
			{
				DateTime start = DateTime.Now;
				string desc = "";

				Fanart fanart = null;

				if (objectWithFanart.GetType() == typeof(AnimeGroupVM))
				{
					AnimeGroupVM grp = objectWithFanart as AnimeGroupVM;
					fanart = new Fanart(grp);
					desc = grp.GroupName;
				}

				if (objectWithFanart.GetType() == typeof(AnimeSeriesVM))
				{
					AnimeSeriesVM ser = objectWithFanart as AnimeSeriesVM;
					fanart = new Fanart(ser);
					desc = ser.SeriesName;
				}

				TimeSpan ts = DateTime.Now - start;
				BaseConfig.MyAnimeLog.Write("GOT FANART details in: {0} ms ({1})", ts.TotalMilliseconds, desc);
				BaseConfig.MyAnimeLog.Write("LOADING FANART: {0} - {1}", desc, fanart.FileName);

				if (String.IsNullOrEmpty(fanart.FileName))
				{
					DisableFanart();
					return;
				}



				fanartTexture.Filename = fanart.FileName;

				if (this.dummyIsFanartLoaded != null)
					this.dummyIsFanartLoaded.Visible = true;

			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write(ex.ToString());
			}
		}


		void DisableFanart()
		{
			//fanartSet = false;
			fanartTexture.Filename = "";

			if (this.dummyIsFanartLoaded != null)
				this.dummyIsFanartLoaded.Visible = false;
		}


		protected override void OnShowContextMenu()
		{
			try
			{
				hook.IsEnabled = false;

				switch (listLevel)
				{
					case Listlevel.GroupFilter:
						ShowContextMenuGroupFilter("");
						break;
					case Listlevel.Group:
						ShowContextMenuGroup("");
						break;
					case Listlevel.Series:
						ShowContextMenuSeries("");
						break;
					case Listlevel.EpisodeTypes:
						break;
					case Listlevel.Episode:
						ShowContextMenuEpisode("");
						break;
				}

				Thread.Sleep(100); //make sure key-up's from the context menu aren't cought by the hook
				if (hook != null) //hook may have fallen out of scope when using contect menu togo to another window.
					hook.IsEnabled = true;
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write("Error in menu: {0}", ex);
			}
		}

		private void AddContextMenuItem()
		{
		}

		private bool SearchTheTvDB(AnimeSeriesVM ser, string searchCriteria, string previousMenu)
		{
			if (searchCriteria.Length == 0)
				return true;

			int aniDBID = ser.AniDB_Anime.AnimeID;

			List<TVDBSeriesSearchResultVM> TVDBSeriesSearchResults = new List<TVDBSeriesSearchResultVM>();
			List<JMMServerBinary.Contract_TVDBSeriesSearchResult> tvResults = JMMServerVM.Instance.clientBinaryHTTP.SearchTheTvDB(searchCriteria.Trim());
			foreach (JMMServerBinary.Contract_TVDBSeriesSearchResult tvResult in tvResults)
				TVDBSeriesSearchResults.Add(new TVDBSeriesSearchResultVM(tvResult));

			BaseConfig.MyAnimeLog.Write("Found {0} tvdb results for {1}", TVDBSeriesSearchResults.Count, searchCriteria);
			if (TVDBSeriesSearchResults.Count > 0)
			{
				GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
				if (dlg == null)
					return true;

				//keep showing the dialog until the user closes it
				int selectedLabel = 0;
				while (true)
				{
					dlg.Reset();
					dlg.SetHeading("TvDB Search Results");

					if (previousMenu != string.Empty)
						dlg.Add("<<< " + previousMenu);


					foreach (TVDBSeriesSearchResultVM res in TVDBSeriesSearchResults)
					{
						string disp = string.Format("{0} ({1}) / {2}", res.SeriesName, res.Language, res.Id);
						dlg.Add(disp);
					}

					dlg.SelectedLabel = selectedLabel;
					dlg.DoModal(GUIWindowManager.ActiveWindow);
					selectedLabel = dlg.SelectedLabel;

					int selection = selectedLabel + ((previousMenu == string.Empty) ? 1 : 0);
					if (selection == 0)
						return true; // previous menu

					if (selection > 0 && selection <= TVDBSeriesSearchResults.Count)
					{
						TVDBSeriesSearchResultVM res = TVDBSeriesSearchResults[selection - 1];

						LinkAniDBToTVDB(ser, aniDBID, res.SeriesID, 1);
						return false;
					}

					return true;
				}
			}
			else
			{
				GUIDialogOK dlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
				if (null != dlgOK)
				{
					dlgOK.SetHeading("Search Results");
					dlgOK.SetLine(1, string.Empty);
					dlgOK.SetLine(2, "No results found");
					dlgOK.DoModal(GUIWindowManager.ActiveWindow);
				}
				return true;
			}
		}

		private bool SearchTrakt(AnimeSeriesVM ser, string searchCriteria, string previousMenu)
		{
			if (searchCriteria.Length == 0)
				return true;

			int aniDBID = ser.AniDB_Anime.AnimeID;

			List<TraktTVShowResponseVM> TraktSeriesSearchResults = new List<TraktTVShowResponseVM>();
			List<JMMServerBinary.Contract_TraktTVShowResponse> traktResults = JMMServerVM.Instance.clientBinaryHTTP.SearchTrakt(searchCriteria);
			foreach (JMMServerBinary.Contract_TraktTVShowResponse traktResult in traktResults)
				TraktSeriesSearchResults.Add(new TraktTVShowResponseVM(traktResult));

			BaseConfig.MyAnimeLog.Write("Found {0} trakt results for {1}", TraktSeriesSearchResults.Count, searchCriteria);
			if (TraktSeriesSearchResults.Count > 0)
			{
				GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
				if (dlg == null)
					return true;

				//keep showing the dialog until the user closes it
				int selectedLabel = 0;
				while (true)
				{
					dlg.Reset();
					dlg.SetHeading("Trakt Search Results");

					if (previousMenu != string.Empty)
						dlg.Add("<<< " + previousMenu);


					foreach (TraktTVShowResponseVM res in TraktSeriesSearchResults)
					{
						string disp = string.Format("{0} ({1})", res.title, res.year);
						dlg.Add(disp);
					}

					dlg.SelectedLabel = selectedLabel;
					dlg.DoModal(GUIWindowManager.ActiveWindow);
					selectedLabel = dlg.SelectedLabel;

					int selection = selectedLabel + ((previousMenu == string.Empty) ? 1 : 0);
					if (selection == 0)
						return true; // previous menu

					if (selection > 0 && selection <= TraktSeriesSearchResults.Count)
					{
						TraktTVShowResponseVM res = TraktSeriesSearchResults[selection - 1];

						LinkAniDBToTrakt(ser, aniDBID, res.TraktID, 1);
						return false;
					}

					return true;
				}
			}
			else
			{
				GUIDialogOK dlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
				if (null != dlgOK)
				{
					dlgOK.SetHeading("Search Results");
					dlgOK.SetLine(1, string.Empty);
					dlgOK.SetLine(2, "No results found");
					dlgOK.DoModal(GUIWindowManager.ActiveWindow);
				}
				return true;
			}
		}

		private bool SearchTheTvDBMenu(AnimeSeriesVM ser, string previousMenu)
		{
			//string searchCriteria = "";
			int aniDBID = ser.AniDB_Anime.AnimeID;

			GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg == null)
				return true;

			//keep showing the dialog until the user closes it
			int selectedLabel = 0;
			string currentMenu = "Search The TvDB";
			while (true)
			{
				dlg.Reset();
				dlg.SetHeading(currentMenu);

				if (previousMenu != string.Empty)
					dlg.Add("<<< " + previousMenu);
				dlg.Add("Search using:   " + ser.AniDB_Anime.FormattedTitle);
				dlg.Add("Manual Search");

				CrossRef_AniDB_TvDBResultVM CrossRef_AniDB_TvDBResult = null;
				JMMServerBinary.Contract_CrossRef_AniDB_TvDBResult xref = JMMServerVM.Instance.clientBinaryHTTP.GetTVDBCrossRefWebCache(aniDBID);
				if (xref != null)
				{
					CrossRef_AniDB_TvDBResult = new CrossRef_AniDB_TvDBResultVM(xref);
					dlg.Add("Community Says:   " + CrossRef_AniDB_TvDBResult.SeriesName);
				}
				

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
						if (!SearchTheTvDB(ser, ser.AniDB_Anime.FormattedTitle, currentMenu))
							return false;
						break;
					case 2:
						{
							if (Utils.DialogText(ref searchText, GetID))
							{
								if (!SearchTheTvDB(ser, searchText, currentMenu))
									return false;
							}
						}
						break;
					case 3:
						LinkAniDBToTVDB(ser, ser.AniDB_Anime.AnimeID, CrossRef_AniDB_TvDBResult.TvDBID, CrossRef_AniDB_TvDBResult.TvDBSeasonNumber);
						return false;
					default:
						//close menu
						return false;
				}
			}
		}

		private bool SearchTraktMenu(AnimeSeriesVM ser, string previousMenu)
		{
			//string searchCriteria = "";
			int aniDBID = ser.AniDB_Anime.AnimeID;

			GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg == null)
				return true;

			//keep showing the dialog until the user closes it
			int selectedLabel = 0;
			string currentMenu = "Search Trakt";
			while (true)
			{
				dlg.Reset();
				dlg.SetHeading(currentMenu);

				if (previousMenu != string.Empty)
					dlg.Add("<<< " + previousMenu);
				dlg.Add("Search using:   " + ser.AniDB_Anime.FormattedTitle);
				dlg.Add("Manual Search");

				CrossRef_AniDB_TraktResultVM webCacheResult = null;
				JMMServerBinary.Contract_CrossRef_AniDB_TraktResult xref = JMMServerVM.Instance.clientBinaryHTTP.GetTraktCrossRefWebCache(aniDBID);
				if (xref != null)
				{
					webCacheResult = new CrossRef_AniDB_TraktResultVM(xref);
					dlg.Add("Community Says:   " + webCacheResult.ShowName);
				}


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
						if (!SearchTrakt(ser, ser.AniDB_Anime.FormattedTitle, currentMenu))
							return false;
						break;
					case 2:
						{
							if (Utils.DialogText(ref searchText, GetID))
							{
								if (!SearchTrakt(ser, searchText, currentMenu))
									return false;
							}
						}
						break;
					case 3:
						LinkAniDBToTrakt(ser, ser.AniDB_Anime.AnimeID, webCacheResult.TraktID, webCacheResult.TraktSeasonNumber);
						return false;
					default:
						//close menu
						return false;
				}
			}
		}

		private void LinkAniDBToTrakt(AnimeSeriesVM ser, int animeID, string traktID, int season)
		{
			string res = JMMServerVM.Instance.clientBinaryHTTP.LinkAniDBTrakt(animeID, traktID, season);
			if (res.Length > 0)
				Utils.DialogMsg("Error", res);

			ser = JMMServerHelper.GetSeries(ser.AnimeSeriesID.Value);
		}

		private void LinkAniDBToTVDB(AnimeSeriesVM ser, int animeID, int tvDBID, int season)
		{
			string res = JMMServerVM.Instance.clientBinaryHTTP.LinkAniDBTvDB(animeID, tvDBID, season);
			if (res.Length > 0)
				Utils.DialogMsg("Error", res);

			ser = JMMServerHelper.GetSeries(ser.AnimeSeriesID.Value);
		}

		private void LinkAniDBToMovieDB(AnimeSeriesVM ser, int animeID, int movieID)
		{
			string res = JMMServerVM.Instance.clientBinaryHTTP.LinkAniDBOther(animeID, movieID, (int)CrossRefType.MovieDB);
			if (res.Length > 0)
				Utils.DialogMsg("Error", res);

			ser = JMMServerHelper.GetSeries(ser.AnimeSeriesID.Value);
		}

		private void LinkAniDBToMAL(AnimeSeriesVM ser, int animeID, int malID, string malTitle)
		{
			if (ser.CrossRef_AniDB_MAL != null)
			{
				foreach (CrossRef_AniDB_MALVM xref in ser.CrossRef_AniDB_MAL)
					JMMServerVM.Instance.clientBinaryHTTP.RemoveLinkAniDBMAL(xref.AnimeID, xref.StartEpisodeType, xref.StartEpisodeNumber);
			}

			string res = JMMServerVM.Instance.clientBinaryHTTP.LinkAniDBMAL(animeID, malID, malTitle, 1, 1);
			if (res.Length > 0)
				Utils.DialogMsg("Error", res);

			ser = JMMServerHelper.GetSeries(ser.AnimeSeriesID.Value);
		}

		private bool SearchTheMovieDBMenu(AnimeSeriesVM ser, string previousMenu)
		{
			int aniDBID = ser.AniDB_Anime.AnimeID;

			GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg == null)
				return true;

			//keep showing the dialog until the user closes it
			int selectedLabel = 0;
			string currentMenu = "Search The MovieDB";
			while (true)
			{
				dlg.Reset();
				dlg.SetHeading(currentMenu);

				if (previousMenu != string.Empty)
					dlg.Add("<<< " + previousMenu);
				dlg.Add("Search using:   " + ser.AniDB_Anime.FormattedTitle);
				dlg.Add("Manual Search");

				CrossRef_AniDB_OtherResultVM CrossRef_AniDB_OtherResult = null;
				JMMServerBinary.Contract_CrossRef_AniDB_OtherResult xref = JMMServerVM.Instance.clientBinaryHTTP.GetOtherAnimeCrossRefWebCache(aniDBID, (int)CrossRefType.MovieDB);
				if (xref != null)
				{
					CrossRef_AniDB_OtherResult = new CrossRef_AniDB_OtherResultVM(xref);
					dlg.Add("Community Says:   " + CrossRef_AniDB_OtherResult.CrossRefID.ToString());
				}

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
						if (!SearchTheMovieDB(ser, ser.AniDB_Anime.FormattedTitle, currentMenu))
							return false;
						break;
					case 2:
						{
							string searchText = ser.AniDB_Anime.FormattedTitle;
							if (Utils.DialogText(ref searchText, GetID))
							{
								if (!SearchTheMovieDB(ser, searchText, currentMenu))
									return false;
							}
						}
						break;
					case 3:
						LinkAniDBToMovieDB(ser, CrossRef_AniDB_OtherResult.AnimeID, int.Parse(CrossRef_AniDB_OtherResult.CrossRefID));
						return false;
					default:
						//close menu
						return false;
				}
			}
		}

		

		private bool SearchTheMovieDB(AnimeSeriesVM ser, string searchCriteria, string previousMenu)
		{
			if (searchCriteria.Length == 0)
				return true;

			int aniDBID = ser.AniDB_Anime.AnimeID;

			List<MovieDBMovieSearchResultVM> MovieDBSeriesSearchResults = new List<MovieDBMovieSearchResultVM>();
			List<JMMServerBinary.Contract_MovieDBMovieSearchResult> movieResults = JMMServerVM.Instance.clientBinaryHTTP.SearchTheMovieDB(searchCriteria.Trim());
			foreach (JMMServerBinary.Contract_MovieDBMovieSearchResult movieResult in movieResults)
				MovieDBSeriesSearchResults.Add(new MovieDBMovieSearchResultVM(movieResult));

			BaseConfig.MyAnimeLog.Write("Found {0} moviedb results for {1}", MovieDBSeriesSearchResults.Count, searchCriteria);

			if (MovieDBSeriesSearchResults.Count > 0)
			{
				GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
				if (dlg == null)
					return true;

				//keep showing the dialog until the user closes it
				int selectedLabel = 0;
				while (true)
				{
					dlg.Reset();
					dlg.SetHeading("Search Results");

					if (previousMenu != string.Empty)
						dlg.Add("<<< " + previousMenu);
					foreach (MovieDBMovieSearchResultVM res in MovieDBSeriesSearchResults)
						dlg.Add(res.MovieName);

					dlg.SelectedLabel = selectedLabel;
					dlg.DoModal(GUIWindowManager.ActiveWindow);
					selectedLabel = dlg.SelectedLabel;

					int selection = selectedLabel + ((previousMenu == string.Empty) ? 1 : 0);
					if (selection == 0)
						return true; //previous menu

					if (selection > 0 && selection <= MovieDBSeriesSearchResults.Count)
					{
						MovieDBMovieSearchResultVM res = MovieDBSeriesSearchResults[selection - 1];

						LinkAniDBToMovieDB(ser, aniDBID, res.MovieID);
						return false;
					}

					return true;
				}
			}
			else
			{
				GUIDialogOK dlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
				if (null != dlgOK)
				{
					dlgOK.SetHeading("Search Results");
					dlgOK.SetLine(1, string.Empty);
					dlgOK.SetLine(2, "No results found");
					dlgOK.DoModal(GUIWindowManager.ActiveWindow);
				}
				return true;
			}
		}

		private bool SearchMALMenu(AnimeSeriesVM ser, string previousMenu)
		{
			int aniDBID = ser.AniDB_Anime.AnimeID;

			GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg == null)
				return true;

			//keep showing the dialog until the user closes it
			int selectedLabel = 0;
			string currentMenu = "Search MAL";
			while (true)
			{
				dlg.Reset();
				dlg.SetHeading(currentMenu);

				if (previousMenu != string.Empty)
					dlg.Add("<<< " + previousMenu);
				dlg.Add("Search using:   " + ser.AniDB_Anime.FormattedTitle);
				dlg.Add("Manual Search");

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
						if (!SearchMAL(ser, ser.AniDB_Anime.FormattedTitle, currentMenu))
							return false;
						break;
					case 2:
						{
							string searchText = ser.AniDB_Anime.FormattedTitle;
							if (Utils.DialogText(ref searchText, GetID))
							{
								if (!SearchMAL(ser, searchText, currentMenu))
									return false;
							}
						}
						break;
					default:
						//close menu
						return false;
				}
			}
		}

		private bool SearchMAL(AnimeSeriesVM ser, string searchCriteria, string previousMenu)
		{
			if (searchCriteria.Length == 0)
				return true;

			int aniDBID = ser.AniDB_Anime.AnimeID;

			List<MALSearchResultVM> MALSearchResults = new List<MALSearchResultVM>();
			List<JMMServerBinary.Contract_MALAnimeResponse> malResults = JMMServerVM.Instance.clientBinaryHTTP.SearchMAL(searchCriteria.Trim());
			foreach (JMMServerBinary.Contract_MALAnimeResponse malResult in malResults)
				MALSearchResults.Add(new MALSearchResultVM(malResult));

			BaseConfig.MyAnimeLog.Write("Found {0} MAL results for {1}", MALSearchResults.Count, searchCriteria);

			if (MALSearchResults.Count > 0)
			{
				GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
				if (dlg == null)
					return true;

				//keep showing the dialog until the user closes it
				int selectedLabel = 0;
				while (true)
				{
					dlg.Reset();
					dlg.SetHeading("Search Results");

					if (previousMenu != string.Empty)
						dlg.Add("<<< " + previousMenu);
					foreach (MALSearchResultVM res in MALSearchResults)
						dlg.Add(string.Format("{0} ({1} Eps)", res.title, res.episodes));

					dlg.SelectedLabel = selectedLabel;
					dlg.DoModal(GUIWindowManager.ActiveWindow);
					selectedLabel = dlg.SelectedLabel;

					int selection = selectedLabel + ((previousMenu == string.Empty) ? 1 : 0);
					if (selection == 0)
						return true; //previous menu

					if (selection > 0 && selection <= MALSearchResults.Count)
					{
						MALSearchResultVM res = MALSearchResults[selection - 1];

						LinkAniDBToMAL(ser, aniDBID, res.id, res.title);
						return false;
					}

					return true;
				}
			}
			else
			{
				GUIDialogOK dlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
				if (null != dlgOK)
				{
					dlgOK.SetHeading("Search Results");
					dlgOK.SetLine(1, string.Empty);
					dlgOK.SetLine(2, "No results found");
					dlgOK.DoModal(GUIWindowManager.ActiveWindow);
				}
				return true;
			}
		}

		private bool ShowContextMenuEpisode(string previousMenu)
		{
			GUIListItem currentitem = this.m_Facade.SelectedListItem;
			if (currentitem == null)
				return true;

			GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg == null)
				return true;

			AnimeEpisodeVM episode = currentitem.TVTag as AnimeEpisodeVM;
			if (episode == null)
				return true;

			//keep showing the dialog until the user closes it
			int selectedLabel = 0;
			string currentMenu = episode.EpisodeNumberAndName;
			while (true)
			{
				dlg.Reset();
				dlg.SetHeading(currentMenu);

				if (previousMenu != string.Empty)
					dlg.Add("<<< " + previousMenu);
				bool isWatched = (episode.IsWatched == 1);
				if (isWatched)
					dlg.Add("Mark as Unwatched");
				else
					dlg.Add("Mark as Watched");
				dlg.Add("Mark ALL as Watched");
				dlg.Add("Mark ALL as Unwatched");
				dlg.Add("Mark ALL PREVIOUS as Watched");
				dlg.Add("Mark ALL PREVIOUS as Unwatched");
				dlg.Add("Associate File With This Episode");
				dlg.Add("Remove File From This Episode");
				dlg.Add("Download this epsiode");
				dlg.Add("Post-processing >>>");

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
						{   // Mark as Watched/Unwatched
							BaseConfig.MyAnimeLog.Write("Toggle watched status: {0} - {1}", isWatched, episode);
							episode.ToggleWatchedStatus(!isWatched);
							LoadFacade();
					
							return false;
						}
					case 2: // Mark ALL as Watched
						{
							JMMServerVM.Instance.clientBinaryHTTP.SetWatchedStatusOnSeries(curAnimeSeries.AnimeSeriesID.Value,
								true, int.MaxValue, (int)curAnimeEpisodeType.EpisodeType, JMMServerVM.Instance.CurrentUser.JMMUserID);

							if (BaseConfig.Settings.DisplayRatingDialogOnCompletion)
							{
								JMMServerBinary.Contract_AnimeSeries contract = JMMServerVM.Instance.clientBinaryHTTP.GetSeries(curAnimeSeries.AnimeSeriesID.Value,
									JMMServerVM.Instance.CurrentUser.JMMUserID);
								if (contract != null)
								{
									AnimeSeriesVM ser = new AnimeSeriesVM(contract);
									Utils.PromptToRateSeriesOnCompletion(ser);
								}
							}

							LoadFacade();
							return false;
						}

					case 3: // Mark ALL as Unwatched
						{
							JMMServerVM.Instance.clientBinaryHTTP.SetWatchedStatusOnSeries(curAnimeSeries.AnimeSeriesID.Value,
								false, int.MaxValue, (int)curAnimeEpisodeType.EpisodeType, JMMServerVM.Instance.CurrentUser.JMMUserID);

							LoadFacade();
							return false;
						}

					case 4: // Mark ALL PREVIOUS as Watched
						{

							JMMServerVM.Instance.clientBinaryHTTP.SetWatchedStatusOnSeries(curAnimeSeries.AnimeSeriesID.Value,
								true, episode.EpisodeNumber, (int)curAnimeEpisodeType.EpisodeType, JMMServerVM.Instance.CurrentUser.JMMUserID);

							JMMServerBinary.Contract_AnimeSeries contract = JMMServerVM.Instance.clientBinaryHTTP.GetSeries(curAnimeSeries.AnimeSeriesID.Value,
								JMMServerVM.Instance.CurrentUser.JMMUserID);
							if (contract != null)
							{
								AnimeSeriesVM ser = new AnimeSeriesVM(contract);
								Utils.PromptToRateSeriesOnCompletion(ser);
							}

							LoadFacade();
							return false;
						}

					case 5: // Mark ALL PREVIOUS as Unwatched
						{

							JMMServerVM.Instance.clientBinaryHTTP.SetWatchedStatusOnSeries(curAnimeSeries.AnimeSeriesID.Value,
								false, episode.EpisodeNumber, (int)curAnimeEpisodeType.EpisodeType, JMMServerVM.Instance.CurrentUser.JMMUserID);

							LoadFacade();
							return false;
						}

					case 6: // associate file with this episode
						{
							List<VideoLocalVM> unlinkedVideos = JMMServerHelper.GetUnlinkedVideos();
							if (unlinkedVideos.Count == 0)
							{
								GUIDialogOK dlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
								if (null != dlgOK)
								{
									dlgOK.SetHeading("Error");
									dlgOK.SetLine(1, string.Empty);
									dlgOK.SetLine(2, "No unlinked files to select");
									dlgOK.DoModal(GUIWindowManager.ActiveWindow);
								}
								break;
							}

							// ask the user which file they want to associate
							IDialogbox dlg2 = (IDialogbox)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
							dlg2.Reset();
							dlg2.SetHeading("Select File");

							foreach (VideoLocalVM fl in unlinkedVideos)
								dlg2.Add(Path.GetFileName(fl.FullPath) + " - " + Path.GetDirectoryName(fl.FullPath));

							dlg2.DoModal(GUIWindowManager.ActiveWindow);

							if (dlg2.SelectedId > 0)
							{
								VideoLocalVM selectedFile = unlinkedVideos[dlg2.SelectedId - 1];
								JMMServerHelper.LinkedFileToEpisode(selectedFile.VideoLocalID, episode.AnimeEpisodeID);
								LoadFacade();
								return false;
							}
							break;
						}
					case 7: // remove associated file
						{
							List<VideoDetailedVM> vidList = episode.FilesForEpisode;
							if (vidList.Count == 0)
							{
								GUIDialogOK dlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
								if (null != dlgOK)
								{
									dlgOK.SetHeading("Error");
									dlgOK.SetLine(1, string.Empty);
									dlgOK.SetLine(2, "This episode has no associated files");
									dlgOK.DoModal(GUIWindowManager.ActiveWindow);
								}
								break;
							}

							// ask the user which file they want to un-associate
							IDialogbox dlg2 = (IDialogbox)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
							dlg2.Reset();
							dlg2.SetHeading("Select File");

							foreach (VideoDetailedVM fl in vidList)
								dlg2.Add(Path.GetFileName(fl.FileName));

							dlg2.DoModal(GUIWindowManager.ActiveWindow);

							if (dlg2.SelectedId > 0)
							{
								VideoDetailedVM selectedFile = vidList[dlg2.SelectedId - 1];
								string res = JMMServerVM.Instance.clientBinaryHTTP.RemoveAssociationOnFile(selectedFile.VideoLocalID, episode.AniDB_EpisodeID);
								if (!string.IsNullOrEmpty(res))
									Utils.DialogMsg("Error", res);

								LoadFacade();
								return false;
							}
							break;
						}
					case 8:
						DownloadHelper.SearchEpisode(curAnimeEpisode);
						return false;

					case 9:
						if (!ShowContextMenuPostProcessing(currentMenu))
							return false;
						break;

					default:
						//close menu
						return false;
				}
			}
		}


		private bool ShowContextMenuDatabases(AnimeSeriesVM ser, string previousMenu)
		{
			GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg == null)
				return true;

			int mnuPrevious = -1;
			int mnuTvDB = -1;
			int mnuMovieDB = -1;
			int mnuTvDBSub = -1;
			int mnuMovieDBSub = -1;
			int mnuTrakt = -1;
			int mnuTraktSub = -1;
			int mnuMAL = -1;
			int mnuMALSub = -1;

			int curMenu = -1;

			//keep showing the dialog until the user closes it
			int selectedLabel = 0;
			string currentMenu = ser.SeriesName + " Databases";

			bool hasTvDBLink = ser.CrossRef_AniDB_TvDB != null && ser.AniDB_Anime.AniDB_AnimeCrossRefs != null && ser.AniDB_Anime.AniDB_AnimeCrossRefs.TvDBSeries != null;
			bool hasMovieDBLink = ser.CrossRef_AniDB_MovieDB != null && ser.AniDB_Anime.AniDB_AnimeCrossRefs != null && ser.AniDB_Anime.AniDB_AnimeCrossRefs.MovieDB_Movie != null;
			bool hasTraktLink = ser.AniDB_Anime.AniDB_AnimeCrossRefs != null && ser.AniDB_Anime.AniDB_AnimeCrossRefs.TraktShow != null;
			bool hasMALLink = ser.CrossRef_AniDB_MAL != null && ser.CrossRef_AniDB_MAL.Count > 0;

			while (true)
			{
				dlg.Reset();
				dlg.SetHeading(currentMenu);

				string tvdbText = "Search The TvDB";
				string moviedbText = "Search The MovieDB";
				string traktText = "Search Trakt.tv";
				string malText = "Search MAL";

				if (ser != null)
				{
					if (hasTvDBLink)
						tvdbText += "    (Current: " + ser.AniDB_Anime.AniDB_AnimeCrossRefs.TvDBSeries.SeriesName + ")";

					if (hasMovieDBLink)
						moviedbText += "    (Current: " + ser.AniDB_Anime.AniDB_AnimeCrossRefs.MovieDB_Movie.MovieName + ")";

					if (hasTraktLink)
						traktText += "    (Current: " + ser.AniDB_Anime.AniDB_AnimeCrossRefs.TraktShow.Title + ")";

					if (hasMALLink)
					{
						if (ser.CrossRef_AniDB_MAL.Count == 1)
							malText += "    (Current: " + ser.CrossRef_AniDB_MAL[0].MALTitle + ")";
						else
							malText += "    (Current: Multiple Links)";
					}
				}

				if (previousMenu != string.Empty)
				{
					dlg.Add("<<< " + previousMenu);
					curMenu++; mnuPrevious = curMenu;
				}


				if (ser != null && !hasMovieDBLink)
				{
					dlg.Add(tvdbText);
					curMenu++; mnuTvDB = curMenu;

					dlg.Add(traktText);
					curMenu++; mnuTrakt = curMenu;

					dlg.Add(malText);
					curMenu++; mnuMAL = curMenu;
				}

				if (ser != null && !hasTvDBLink && !hasTraktLink)
				{
					dlg.Add(moviedbText);
					curMenu++; mnuMovieDB = curMenu;
				}

				if (ser != null && hasTvDBLink)
				{
					dlg.Add("The Tv DB >>>");
					curMenu++; mnuTvDBSub = curMenu;
				}

				if (ser != null && hasTraktLink)
				{
					dlg.Add("Trakt.tv >>>");
					curMenu++; mnuTraktSub = curMenu;
				}

				if (ser != null && hasMALLink)
				{
					dlg.Add("MAL >>>");
					curMenu++; mnuMALSub = curMenu;
				}

				if (ser != null && hasMovieDBLink)
				{
					dlg.Add("The Movie DB >>>");
					curMenu++; mnuMovieDBSub = curMenu;
				}

				dlg.SelectedLabel = selectedLabel;
				dlg.DoModal(GUIWindowManager.ActiveWindow);
				selectedLabel = dlg.SelectedLabel;

				if (selectedLabel == mnuPrevious) return true;

				if (selectedLabel == mnuTvDB)
				{
					if (!SearchTheTvDBMenu(ser, currentMenu))
						return false;
				}

				if (selectedLabel == mnuTrakt)
				{
					if (!SearchTraktMenu(ser, currentMenu))
						return false;
				}

				if (selectedLabel == mnuMAL)
				{
					if (!SearchMALMenu(ser, currentMenu))
						return false;
				}

				if (selectedLabel == mnuMovieDB)
				{
					if (!SearchTheMovieDBMenu(ser, currentMenu))
						return false;
				}

				if (selectedLabel == mnuTvDBSub)
				{
					if (!ShowContextMenuTVDB(ser, currentMenu))
						return false;
				}

				if (selectedLabel == mnuTraktSub)
				{
					if (!ShowContextMenuTrakt(ser, currentMenu))
						return false;
				}

				if (selectedLabel == mnuMALSub)
				{
					if (!ShowContextMenuMAL(ser, currentMenu))
						return false;
				}

				if (selectedLabel == mnuMovieDBSub)
				{
					if (!ShowContextMenuMovieDB(ser, currentMenu))
						return false;
				}

				return false;
			}
		}

		private bool ShowContextMenuGroupEdit(string previousMenu)
		{
			GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg == null)
				return true;

			GUIListItem currentitem = this.m_Facade.SelectedListItem;
			if (currentitem == null)
				return true;

			AnimeGroupVM grp = currentitem.TVTag as AnimeGroupVM;
			if (grp == null)
				return true;

			List<AnimeSeriesVM> allSeries = grp.AllSeries;
			AnimeSeriesVM equalSeries = null;
			if (allSeries.Count == 1)
				equalSeries = allSeries[0];

			int mnuPrevious = -1;
			int mnuChangeSortName = -1;
			int mnuAudioLanguage = -1;
			int mnuSubLanguage = -1;
			int mnuDelete = -1;
			int mnuRename = -1;
			int mnuRemDefault = -1;
			int mnuAddDefault = -1;

			int curMenu = -1;

			//keep showing the dialog until the user closes it
			int selectedLabel = 0;
			string currentMenu = grp.GroupName + " - Edit";
			while (true)
			{
				dlg.Reset();
				dlg.SetHeading(currentMenu);

				dlg.Add("<<< " + previousMenu);
				curMenu++; mnuPrevious = curMenu;

				dlg.Add("Rename Group");
				curMenu++; mnuRename = curMenu;

				dlg.Add("Change Sort Name");
				curMenu++; mnuChangeSortName = curMenu;

				if (equalSeries != null)
				{
					dlg.Add("Set Default Audio Language");
					curMenu++; mnuAudioLanguage = curMenu;

					dlg.Add("Set Default Subtitle language");
					curMenu++; mnuSubLanguage = curMenu;
				}

				if (grp.DefaultAnimeSeriesID.HasValue)
				{
					dlg.Add("Remove Default Series");
					curMenu++; mnuRemDefault = curMenu;
				}

				if (allSeries.Count > 1)
				{
					dlg.Add("Set Default Series");
					curMenu++; mnuAddDefault = curMenu;
				}

				dlg.Add("Delete This Group/Series/Episodes");
				curMenu++; mnuDelete = curMenu;			

				dlg.SelectedLabel = selectedLabel;
				dlg.DoModal(GUIWindowManager.ActiveWindow);
				selectedLabel = dlg.SelectedLabel;

				if (selectedLabel == mnuPrevious)
					return true;

				if (selectedLabel == mnuRename)
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
						return false;
					}
				}

				if (selectedLabel == mnuChangeSortName)
				{
					string sortName = grp.SortName;
					if (Utils.DialogText(ref sortName, GetID))
					{
						grp.SortName = sortName;
						grp.Save();
						LoadFacade();
						return false;
					}
				}

				if (selectedLabel == mnuAudioLanguage)
				{
					String language = equalSeries.DefaultAudioLanguage;
					if (Utils.DialogLanguage(ref language, false))
					{
						equalSeries.DefaultAudioLanguage = language;
						equalSeries.Save();
						return false;
					}
				}

				if (selectedLabel == mnuRemDefault)
				{
					JMMServerVM.Instance.clientBinaryHTTP.RemoveDefaultSeriesForGroup(grp.AnimeGroupID);
					grp.DefaultAnimeSeriesID = null;
				}

				if (selectedLabel == mnuAddDefault)
				{
					AnimeSeriesVM ser = null;
					if (Utils.DialogSelectSeries(ref ser, allSeries))
					{
						grp.DefaultAnimeSeriesID = ser.AnimeSeriesID;
						JMMServerVM.Instance.clientBinaryHTTP.SetDefaultSeriesForGroup(grp.AnimeGroupID, ser.AnimeSeriesID.Value);
					}
				}

				if (selectedLabel == mnuSubLanguage)
				{
					String language = equalSeries.DefaultSubtitleLanguage;
					if (Utils.DialogLanguage(ref language, true))
					{
						equalSeries.DefaultSubtitleLanguage = language;
						equalSeries.Save();
						return false;
					}
				}

				if (selectedLabel == mnuDelete)
				{
					if (Utils.DialogConfirm("Are you sure?"))
					{
						JMMServerVM.Instance.clientBinaryHTTP.DeleteAnimeGroup(grp.AnimeGroupID, false);
					}

					LoadFacade();
					return false;
				}

				

				return false;
			}
		}

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

		private bool ShowContextMenuGroupFilter(string previousMenu)
		{
			GUIListItem currentitem = this.m_Facade.SelectedListItem;
			if (currentitem == null)
				return true;

			GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg == null)
				return true;

			GroupFilterVM gf = currentitem.TVTag as GroupFilterVM;
			if (gf == null)
				return true;

			int mnuPrev = -1;
			int mnuRandomSeries = -1;
			int mnuRandomEpisode = -1;

			//keep showing the dialog until the user closes it
			int selectedLabel = 0;
			string currentMenu = gf.GroupFilterName;
			while (true)
			{
				int curMenu = -1;

				dlg.Reset();
				dlg.SetHeading(currentMenu);


				if (previousMenu != string.Empty)
				{
					dlg.Add("<<< " + previousMenu);
					curMenu++; mnuPrev = curMenu;
				}
				dlg.Add("Random Series");
				curMenu++; mnuRandomSeries = curMenu;

				dlg.Add("Random Episode");
				curMenu++; mnuRandomEpisode = curMenu;

				dlg.SelectedLabel = selectedLabel;
				dlg.DoModal(GUIWindowManager.ActiveWindow);
				selectedLabel = dlg.SelectedLabel;

				if (selectedLabel == mnuPrev) return true;
				if (selectedLabel == mnuRandomSeries)
				{
					RandomWindow_CurrentEpisode = null;
					RandomWindow_CurrentSeries = null;

					RandomWindow_LevelObject = gf;
					RandomWindow_RandomLevel = RandomSeriesEpisodeLevel.GroupFilter;
					RandomWindow_RandomType = RandomObjectType.Series;

					GUIWindowManager.ActivateWindow(Constants.WindowIDs.RANDOM);

					return false;
				}

				if (selectedLabel == mnuRandomEpisode)
				{
					RandomWindow_CurrentEpisode = null;
					RandomWindow_CurrentSeries = null;

					RandomWindow_LevelObject = gf;
					RandomWindow_RandomLevel = RandomSeriesEpisodeLevel.GroupFilter;
					RandomWindow_RandomType = RandomObjectType.Episode;

					GUIWindowManager.ActivateWindow(Constants.WindowIDs.RANDOM);

					return false;
				}

			}
		}

		private bool ShowContextMenuGroup(string previousMenu)
		{
			GUIListItem currentitem = this.m_Facade.SelectedListItem;
			if (currentitem == null)
				return true;

			GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg == null)
				return true;

			AnimeGroupVM grp = currentitem.TVTag as AnimeGroupVM;
			if (grp == null)
				return true;

			int mnuPrev = -1;
			int mnuFave = -1;
			int mnuWatched = -1;
			int mnuUnwatched = -1;
			int mnuEdit = -1;
			int mnuQuickSort = -1;
			int mnuRemoveQuickSort = -1;
			int mnuDatabases = -1;
			int mnuImages = -1;
			int mnuSeries = -1;
			int mnuRandomSeries = -1;
			int mnuRandomEpisode = -1;
			int mnuPostProcessing = -1;

			//keep showing the dialog until the user closes it
			int selectedLabel = 0;
			string currentMenu = grp.GroupName;
			while (true)
			{
				int curMenu = -1;

				dlg.Reset();
				dlg.SetHeading(currentMenu);

				string faveText = "Add to Favorites";
				if (grp.IsFave == 1)
					faveText = "Remove from Favorites";

				if (previousMenu != string.Empty)
				{
					dlg.Add("<<< " + previousMenu);
					curMenu++; mnuPrev = curMenu;
				}
				dlg.Add(faveText);
				curMenu++; mnuFave = curMenu;

				dlg.Add("Mark ALL as Watched");
				curMenu++; mnuWatched = curMenu;

				dlg.Add("Mark ALL as Unwatched");
				curMenu++; mnuUnwatched = curMenu;

				dlg.Add("Edit Group >>>");
				curMenu++; mnuEdit = curMenu;

				dlg.Add("Quick Sort >>>");
				curMenu++; mnuQuickSort = curMenu;

				if (GroupFilterQuickSorts.ContainsKey(curGroupFilter.GroupFilterID.Value))
				{
					dlg.Add("Remove Quick Sort");
					curMenu++; mnuRemoveQuickSort = curMenu;
				}

				if (grp.AllSeries.Count == 1)
				{
					dlg.Add("Databases >>>");
					curMenu++; mnuDatabases = curMenu;
					dlg.Add("Images >>>");
					curMenu++; mnuImages = curMenu;
					dlg.Add("Series Information");
					curMenu++; mnuSeries = curMenu;
				}

				dlg.Add("Random Series");
				curMenu++; mnuRandomSeries = curMenu;

				dlg.Add("Random Episode");
				curMenu++; mnuRandomEpisode = curMenu;

				dlg.Add("Post-processing >>>");
				curMenu++; mnuPostProcessing = curMenu;

				dlg.SelectedLabel = selectedLabel;
				dlg.DoModal(GUIWindowManager.ActiveWindow);
				selectedLabel = dlg.SelectedLabel;

				if (selectedLabel == mnuPrev) return true;
				if (selectedLabel == mnuFave)
				{
					grp.IsFave = grp.IsFave == 1 ? 0 : 1;
					grp.Save();

					EvaluateVisibility();
					return false;
				}

				if (selectedLabel == mnuWatched)
				{
					foreach (AnimeSeriesVM ser in grp.AllSeries)
						JMMServerHelper.SetWatchedStatusOnSeries(true, int.MaxValue, ser.AnimeSeriesID.Value);

					

					LoadFacade();
					return false;
				}

				if (selectedLabel == mnuUnwatched)
				{
					foreach (AnimeSeriesVM ser in grp.AllSeries)
						JMMServerHelper.SetWatchedStatusOnSeries(false, int.MaxValue, ser.AnimeSeriesID.Value);

					LoadFacade();
					return false;
				}

				if (selectedLabel == mnuEdit)
				{
					if (!ShowContextMenuGroupEdit(currentMenu))
						return false;
				}

				if (selectedLabel == mnuQuickSort)
				{
					string sortType = "";
					GroupFilterSortDirection sortDirection = GroupFilterSortDirection.Asc;
					if (GroupFilterQuickSorts.ContainsKey(curGroupFilter.GroupFilterID.Value))
						sortDirection = GroupFilterQuickSorts[curGroupFilter.GroupFilterID.Value].SortDirection;

					if (!Utils.DialogSelectGFQuickSort(ref sortType, ref sortDirection, curGroupFilter.GroupFilterName))
					{
						if (!GroupFilterQuickSorts.ContainsKey(curGroupFilter.GroupFilterID.Value))
							GroupFilterQuickSorts[curGroupFilter.GroupFilterID.Value] = new QuickSort();
						GroupFilterQuickSorts[curGroupFilter.GroupFilterID.Value].SortType = sortType;
						GroupFilterQuickSorts[curGroupFilter.GroupFilterID.Value].SortDirection = sortDirection;
						LoadFacade();
						return false;
					}
				}

				if (selectedLabel == mnuRemoveQuickSort)
				{
					GroupFilterQuickSorts.Remove(curGroupFilter.GroupFilterID.Value);
					LoadFacade();
					return false;
				}

				if (selectedLabel == mnuDatabases)
				{
					if (!ShowContextMenuDatabases(grp.AllSeries[0], "Group Menu"))
						return false;
				}

				if (selectedLabel == mnuImages)
				{
					if (!ShowContextMenuImages(currentMenu))
						return false;
				}

				if (selectedLabel == mnuSeries)
				{
					ShowAnimeInfoWindow();
					return false;
				}

				if (selectedLabel == mnuRandomSeries)
				{
					RandomWindow_CurrentEpisode = null;
					RandomWindow_CurrentSeries = null;

					RandomWindow_LevelObject = grp;
					RandomWindow_RandomLevel = RandomSeriesEpisodeLevel.Group;
					RandomWindow_RandomType = RandomObjectType.Series;

					GUIWindowManager.ActivateWindow(Constants.WindowIDs.RANDOM);

					return false;
				}

				if (selectedLabel == mnuRandomEpisode)
				{
					RandomWindow_CurrentEpisode = null;
					RandomWindow_CurrentSeries = null;

					RandomWindow_LevelObject = grp;
					RandomWindow_RandomLevel = RandomSeriesEpisodeLevel.Group;
					RandomWindow_RandomType = RandomObjectType.Episode;

					GUIWindowManager.ActivateWindow(Constants.WindowIDs.RANDOM);

					return false;
				}

				if (selectedLabel == mnuPostProcessing)
				{
					ShowContextMenuPostProcessing(currentMenu);
					return false;
				}
				
			}
		}

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

		private void ShowAnimeInfoWindow()
		{
			SetGlobalIDs();
			GUIWindowManager.ActivateWindow(Constants.WindowIDs.ANIMEINFO, false);
		}

		private bool ShowContextMenuImages(string previousMenu)
		{
			GUIListItem currentitem = this.m_Facade.SelectedListItem;
			if (currentitem == null)
				return true;

			GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg == null)
				return true;
			string displayName = "";

			AnimeSeriesVM ser = null;
			if (listLevel == Listlevel.Group)
			{
				displayName = curAnimeGroup.GroupName;

				List<AnimeSeriesVM> allSeries = curAnimeGroup.AllSeries;
				if (allSeries.Count == 1) ser = allSeries[0];
			}
			else
			{
				displayName = curAnimeSeries.SeriesName;
				ser = curAnimeSeries;
			}

			//keep showing the dialog until the user closes it
			int selectedLabel = 0;
			string currentMenu = displayName;
			while (true)
			{
				dlg.Reset();
				dlg.SetHeading(currentMenu);

				if (previousMenu != string.Empty)
					dlg.Add("<<< " + previousMenu);
				dlg.Add("Fanart");
				dlg.Add("Posters");
				dlg.Add("Wide Banners");

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
						ShowFanartWindow();
						return false;
					case 2:
						ShowPostersWindow();
						return false;
					case 3:
						ShowWideBannersWindow();
						return false;
					default:
						//close menu
						return false;
				}
			}
		}


		private bool ShowContextMenuTVDB(AnimeSeriesVM ser, string previousMenu)
		{
			GUIListItem currentitem = this.m_Facade.SelectedListItem;
			if (currentitem == null)
				return true;

			GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg == null)
				return true;

			int tvdbid = -1;
			int season = -1;
			string displayName = "";

			if (ser.CrossRef_AniDB_TvDB != null && ser.AniDB_Anime.AniDB_AnimeCrossRefs != null && ser.AniDB_Anime.AniDB_AnimeCrossRefs.TvDBSeries != null)
			{
				displayName = ser.AniDB_Anime.AniDB_AnimeCrossRefs.TvDBSeries.SeriesName;
				tvdbid = ser.AniDB_Anime.AniDB_AnimeCrossRefs.CrossRef_AniDB_TvDB.TvDBID;
				season = ser.AniDB_Anime.AniDB_AnimeCrossRefs.CrossRef_AniDB_TvDB.TvDBSeasonNumber;
			}
			else
				return false;

	
			//keep showing the dialog until the user closes it
			int selectedLabel = 0;
			string currentMenu = displayName;
			while (true)
			{
				dlg.Reset();
				dlg.SetHeading(currentMenu);

				if (previousMenu != string.Empty)
					dlg.Add("<<< " + previousMenu);
				dlg.Add("Remove TVDB Association");
				dlg.Add("Switch Season (Current is " + season.ToString() + ")");


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

						JMMServerVM.Instance.clientBinaryHTTP.RemoveLinkAniDBTvDB(ser.AniDB_Anime.AnimeID);
						break;
					case 2:
						if (!ShowSeasonSelectionMenuTvDB(ser, ser.AniDB_Anime.AnimeID, tvdbid, currentMenu))
							return false;
						break;
					
					default:
						//close menu
						return false;
				}
			}
		}

		private bool ShowContextMenuTrakt(AnimeSeriesVM ser, string previousMenu)
		{
			GUIListItem currentitem = this.m_Facade.SelectedListItem;
			if (currentitem == null)
				return true;

			GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg == null)
				return true;

			string traktID = "";
			int season = -1;
			string displayName = "";

			if (ser.AniDB_Anime.AniDB_AnimeCrossRefs != null && ser.AniDB_Anime.AniDB_AnimeCrossRefs.CrossRef_AniDB_Trakt != null 
				&& ser.AniDB_Anime.AniDB_AnimeCrossRefs.TraktShow != null)
			{
				displayName = ser.AniDB_Anime.AniDB_AnimeCrossRefs.TraktShow.Title;
				traktID = ser.AniDB_Anime.AniDB_AnimeCrossRefs.CrossRef_AniDB_Trakt.TraktID;
				season = ser.AniDB_Anime.AniDB_AnimeCrossRefs.CrossRef_AniDB_Trakt.TraktSeasonNumber;
			}
			else
				return false;


			//keep showing the dialog until the user closes it
			int selectedLabel = 0;
			string currentMenu = displayName;
			while (true)
			{
				dlg.Reset();
				dlg.SetHeading(currentMenu);

				if (previousMenu != string.Empty)
					dlg.Add("<<< " + previousMenu);
				dlg.Add("Remove Trakt Association");
				dlg.Add("Switch Season (Current is " + season.ToString() + ")");


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

						JMMServerVM.Instance.clientBinaryHTTP.RemoveLinkAniDBTrakt(ser.AniDB_Anime.AnimeID);
						break;
					case 2:
						if (!ShowSeasonSelectionMenuTrakt(ser, ser.AniDB_Anime.AnimeID, traktID, currentMenu))
							return false;
						break;

					default:
						//close menu
						return false;
				}
			}
		}

		private bool ShowContextMenuMovieDB(AnimeSeriesVM ser, string previousMenu)
		{
			GUIListItem currentitem = this.m_Facade.SelectedListItem;
			if (currentitem == null)
				return true;

			GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg == null)
				return true;

			//int moviedbid = -1;
			string displayName = "";

			if (ser.CrossRef_AniDB_MovieDB != null && ser.AniDB_Anime.AniDB_AnimeCrossRefs != null && ser.AniDB_Anime.AniDB_AnimeCrossRefs.MovieDB_Movie != null)
			{
				displayName = ser.AniDB_Anime.AniDB_AnimeCrossRefs.MovieDB_Movie.MovieName;
			}
			else
				return false;

			//keep showing the dialog until the user closes it
			int selectedLabel = 0;
			string currentMenu = displayName;
			while (true)
			{
				dlg.Reset();
				dlg.SetHeading(currentMenu);

				if (previousMenu != string.Empty)
					dlg.Add("<<< " + previousMenu);
				dlg.Add("Remove MovieDB Association");

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
						JMMServerVM.Instance.clientBinaryHTTP.RemoveLinkAniDBOther(ser.AniDB_Anime.AnimeID, (int)CrossRefType.MovieDB);
						return false;
					default:
						//close menu
						return false;
				}
			}
		}

		private bool ShowContextMenuMAL(AnimeSeriesVM ser, string previousMenu)
		{
			GUIListItem currentitem = this.m_Facade.SelectedListItem;
			if (currentitem == null)
				return true;

			GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg == null)
				return true;

			//int moviedbid = -1;
			string displayName = "";

			if (ser.CrossRef_AniDB_MAL != null && ser.CrossRef_AniDB_MAL.Count > 0)
			{
				if (ser.CrossRef_AniDB_MAL.Count == 1)
					displayName = ser.CrossRef_AniDB_MAL[0].MALTitle;
				else
					displayName = "Multiple Links!";
			}
			else
				return false;

			//keep showing the dialog until the user closes it
			int selectedLabel = 0;
			string currentMenu = displayName;
			while (true)
			{
				dlg.Reset();
				dlg.SetHeading(currentMenu);

				if (previousMenu != string.Empty)
					dlg.Add("<<< " + previousMenu);
				dlg.Add("Remove MAL Association");

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
						foreach (CrossRef_AniDB_MALVM xref in ser.CrossRef_AniDB_MAL)
						{
							JMMServerVM.Instance.clientBinaryHTTP.RemoveLinkAniDBMAL(xref.AnimeID, xref.StartEpisodeType, xref.StartEpisodeNumber);
						}
						return false;
					default:
						//close menu
						return false;
				}
			}
		}

		private bool ShowSeasonSelectionMenuTvDB(AnimeSeriesVM ser, int animeID, int tvdbid, string previousMenu)
		{
			try
			{
				List<int> seasons = JMMServerVM.Instance.clientBinaryHTTP.GetSeasonNumbersForSeries(tvdbid);
				if (seasons.Count == 0)
				{
					GUIDialogOK dlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
					if (null != dlgOK)
					{
						dlgOK.SetHeading("Season Results");
						dlgOK.SetLine(1, string.Empty);
						dlgOK.SetLine(2, "No seasons found");
						dlgOK.DoModal(GUIWindowManager.ActiveWindow);
					}

					return true;
				}

				GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
				if (dlg == null)
					return true;

				//keep showing the dialog until the user closes it
				int selectedLabel = 0;
				string currentMenu = "Select Season";
				while (true)
				{
					dlg.Reset();
					dlg.SetHeading(currentMenu);

					if (previousMenu != string.Empty)
						dlg.Add("<<< " + previousMenu);
					foreach (int season in seasons)
						dlg.Add("Season " + season.ToString());

					dlg.SelectedLabel = selectedLabel;
					dlg.DoModal(GUIWindowManager.ActiveWindow);
					selectedLabel = dlg.SelectedLabel;

					int selection = selectedLabel + ((previousMenu == string.Empty) ? 1 : 0);
					if (selection == 0)
						return true; //previous menu

					if (selection > 0 && selection <= seasons.Count)
					{
						int selectedSeason = seasons[selection - 1];
						LinkAniDBToTVDB(ser, animeID, tvdbid, selectedSeason);
					}

					return false;
				}
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write("Error in ShowSeasonSelectionMenu:: {0}", ex);
			}

			return true;
		}

		private bool ShowSeasonSelectionMenuTrakt(AnimeSeriesVM ser, int animeID, string traktID, string previousMenu)
		{
			try
			{
				List<int> seasons = JMMServerVM.Instance.clientBinaryHTTP.GetSeasonNumbersForTrakt(traktID);
				if (seasons.Count == 0)
				{
					GUIDialogOK dlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
					if (null != dlgOK)
					{
						dlgOK.SetHeading("Season Results");
						dlgOK.SetLine(1, string.Empty);
						dlgOK.SetLine(2, "No seasons found");
						dlgOK.DoModal(GUIWindowManager.ActiveWindow);
					}

					return true;
				}

				GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
				if (dlg == null)
					return true;

				//keep showing the dialog until the user closes it
				int selectedLabel = 0;
				string currentMenu = "Select Season";
				while (true)
				{
					dlg.Reset();
					dlg.SetHeading(currentMenu);

					if (previousMenu != string.Empty)
						dlg.Add("<<< " + previousMenu);
					foreach (int season in seasons)
						dlg.Add("Season " + season.ToString());

					dlg.SelectedLabel = selectedLabel;
					dlg.DoModal(GUIWindowManager.ActiveWindow);
					selectedLabel = dlg.SelectedLabel;

					int selection = selectedLabel + ((previousMenu == string.Empty) ? 1 : 0);
					if (selection == 0)
						return true; //previous menu

					if (selection > 0 && selection <= seasons.Count)
					{
						int selectedSeason = seasons[selection - 1];
						LinkAniDBToTrakt(ser, animeID, traktID, selectedSeason);
					}

					return false;
				}
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write("Error in ShowSeasonSelectionMenu:: {0}", ex);
			}

			return true;
		}

		private bool ShowContextMenuSeriesEdit(string previousMenu)
		{
			GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg == null)
				return true;

			GUIListItem currentitem = this.m_Facade.SelectedListItem;
			if (currentitem == null)
				return true;

			AnimeSeriesVM ser = currentitem.TVTag as AnimeSeriesVM;
			if (ser == null)
				return true;

			//keep showing the dialog until the user closes it
			int selectedLabel = 0;
			string currentMenu = ser.SeriesName + " - Edit";
			while (true)
			{
				dlg.Reset();
				dlg.SetHeading(currentMenu);

				if (previousMenu != string.Empty)
					dlg.Add("<<< " + previousMenu);
				dlg.Add("Set Default Audio Language");
				dlg.Add("Set Default Subtitle language");
				dlg.Add("Delete This Series/Episodes");

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
						{
							String language = ser.DefaultAudioLanguage;
							if (Utils.DialogLanguage(ref language, false))
							{
								ser.DefaultAudioLanguage = language;
								ser.Save();
								return false;
							}
						}
						break;
					case 2:
						{
							String language = ser.DefaultSubtitleLanguage;
							if (Utils.DialogLanguage(ref language, true))
							{
								ser.DefaultSubtitleLanguage = language;
								ser.Save();
								return false;
							}
						}
						break;
					case 3:
						if (Utils.DialogConfirm("Are you sure?"))
						{
							JMMServerVM.Instance.clientBinaryHTTP.DeleteAnimeSeries(ser.AnimeSeriesID.Value, false, false);
							LoadFacade();

							return false;
						}
						break;
					default:
						//close menu
						return false;
				}
			}
		}

		private bool ShowContextMenuSeries(string previousMenu)
		{
			GUIListItem currentitem = this.m_Facade.SelectedListItem;
			if (currentitem == null)
				return true;

			GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg == null)
				return true;

			AnimeSeriesVM ser = currentitem.TVTag as AnimeSeriesVM;
			if (ser == null)
				return true;

			//keep showing the dialog until the user closes it
			int selectedLabel = 0;
			string currentMenu = ser.SeriesName;
			while (true)
			{
				dlg.Reset();
				dlg.SetHeading(currentMenu);

				if (previousMenu != string.Empty)
					dlg.Add("<<< " + previousMenu);
				dlg.Add("Series Information");
				dlg.Add("Mark all as watched");
				dlg.Add("Mark all as unwatched");
				dlg.Add("Databases >>>");
				dlg.Add("Images >>>");
				dlg.Add("Edit Series >>>");
				dlg.Add("Random Episode");
				dlg.Add("Post-processing >>>");

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
						ShowAnimeInfoWindow();
						return false;
					case 2: // Mark ALL as Watched
						{
							JMMServerHelper.SetWatchedStatusOnSeries(true, int.MaxValue, ser.AnimeSeriesID.Value);

							JMMServerBinary.Contract_AnimeSeries contract = JMMServerVM.Instance.clientBinaryHTTP.GetSeries(ser.AnimeSeriesID.Value,
								JMMServerVM.Instance.CurrentUser.JMMUserID);
							if (contract != null)
							{
								AnimeSeriesVM serTemp = new AnimeSeriesVM(contract);
								Utils.PromptToRateSeriesOnCompletion(serTemp);
							}

							LoadFacade();
							return false;
						}

					case 3: // Mark ALL as Unwatched
						{
							JMMServerHelper.SetWatchedStatusOnSeries(false, int.MaxValue, ser.AnimeSeriesID.Value);
							LoadFacade();
							return false;
						}
					case 4:
						if (!ShowContextMenuDatabases(ser, currentMenu))
							return false;
						break;
					case 5:
						if (!ShowContextMenuImages(currentMenu))
							return false;
						break;
					case 6:
						if (!ShowContextMenuSeriesEdit(currentMenu))
							return false;
						break;
					case 7:
						RandomWindow_CurrentEpisode = null;
						RandomWindow_CurrentSeries = null;

						RandomWindow_LevelObject = ser;
						RandomWindow_RandomLevel = RandomSeriesEpisodeLevel.Series;
						RandomWindow_RandomType = RandomObjectType.Episode;

						GUIWindowManager.ActivateWindow(Constants.WindowIDs.RANDOM);
						return false;

					case 8:
						if (!ShowContextMenuPostProcessing(currentMenu))
							return false;
						break;

					default:
						//close menu
						return false;
				}
			}
		}

		private bool ShowContextMenuPostProcessing(string previousMenu)
		{

			GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg == null)
				return true;

			GUIListItem currentitem = this.m_Facade.SelectedListItem;
			if (currentitem == null)
				return true;

			AnimeGroupVM grp = currentitem.TVTag as AnimeGroupVM;
			List<AnimeEpisodeVM> episodes = new List<AnimeEpisodeVM>();
			if (grp == null)
			{
				AnimeSeriesVM ser = currentitem.TVTag as AnimeSeriesVM;
				if (ser == null)
				{
					AnimeEpisodeVM ep = currentitem.TVTag as AnimeEpisodeVM;
					episodes.Add(ep);
				}
				else
				{
					foreach (AnimeEpisodeVM ep in ser.AllEpisodes)
					{
						episodes.Add(ep);
					}
				}
			}
			else
			{
				List<AnimeSeriesVM> seriesList = grp.AllSeries;
				foreach (AnimeSeriesVM ser in seriesList)
				{
					foreach (AnimeEpisodeVM ep in ser.AllEpisodes)
					{
						episodes.Add(ep);
					}
				}

			}

			if (episodes == null)
				return true;


			//keep showing the dialog until the user closes it
			string currentMenu = "Associate with a ffdshow raw preset";
			int selectedLabel = 0;
			int intLabel = 0;

			FFDShowHelper ffdshowHelper = new FFDShowHelper();
			List<string> presets = ffdshowHelper.Presets;

			string selectedPreset = ffdshowHelper.findSelectedPresetForMenu(episodes);

			while (true)
			{
				dlg.Reset();
				dlg.SetHeading(currentMenu);

				if (previousMenu != string.Empty)
				{
					dlg.Add("<<< " + previousMenu);
					intLabel++;
				}


				dlg.Add("Remove old preset association");
				intLabel++;
				foreach (string preset in presets)
				{
					dlg.Add("Set preset: " + preset);
					// preset selected
					if (selectedPreset == preset)
						selectedLabel = intLabel;

					intLabel++;
				}

				dlg.SelectedLabel = selectedLabel;
				dlg.DoModal(GUIWindowManager.ActiveWindow);
				selectedLabel = dlg.SelectedLabel;

				int selection = selectedLabel + ((previousMenu == string.Empty) ? 1 : 0);

				if (selection == 0)
				{
					//show previous
					return true;
				}
				else if (selection == -1)
				{
					//close menu
					return false;
				}
				else
				{
					string message = "";
					if (selection == 1)
					{
						//DB remove preset
						ffdshowHelper.deletePreset(episodes);
						message = "Old preset successfully removed.";
					}
					else
					{
						//DB associate serie/group with preset
						string choosenPreset = presets.ToArray()[selection - 2];
						ffdshowHelper.addPreset(episodes, choosenPreset);
						message = "Preset \"" + choosenPreset + "\" successfully added.";
					}
					Utils.DialogMsg("Confirmation", message);
					return false;
				}
			}
		}

		private void SetGlobalIDs()
		{
			GlobalSeriesID = -1;

			if (curAnimeGroup == null)
				return;

			AnimeSeriesVM ser = null;
			List<AnimeSeriesVM> allSeries = curAnimeGroup.AllSeries;
			if (allSeries.Count == 1)
				ser = allSeries[0];
			else
				ser = curAnimeSeries;

			if (ser == null) return;

			GlobalSeriesID = ser.AnimeSeriesID.Value;

		}

		private void ShowFanartWindow()
		{
			SetGlobalIDs();
			GUIWindowManager.ActivateWindow(Constants.WindowIDs.FANART, false);
		}

		private void ShowPostersWindow()
		{
			SetGlobalIDs();
			GUIWindowManager.ActivateWindow(Constants.WindowIDs.POSTERS, false);
		}

		private void ShowWideBannersWindow()
		{
			SetGlobalIDs();
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

	public enum Listlevel
	{
		Episode = 0, // The actual episodes
		EpisodeTypes = 1, // Normal, Credits, Specials
		Series = 2, // Da capo, Da Capo S2, Da Capo II etc
		Group = 3, // Da Capo
		GroupFilter = 4, // Favouritess
		GroupFilterSub = 5, // Predefined - Categories
		GroupFilterSub2 = 6 // Predefined - Categories - Action
	}

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

		public object Argument = null;
		public int IndexArgument = 0;
	}
}