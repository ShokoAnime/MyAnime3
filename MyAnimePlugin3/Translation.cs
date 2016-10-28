using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

using System.Reflection;

using System.Text.RegularExpressions;
using System.Xml;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;

namespace MyAnimePlugin3
{
    public static class Translation
    {

        #region Private variables

        private static Dictionary<string, string> translations;
        private static Regex translateExpr = new Regex(@"\$\{([^\}]+)\}");
        private static string path = string.Empty;

        #endregion

        #region Constructor

        static Translation()
        {
            string lang;

            try
            {
                lang = GUILocalizeStrings.GetCultureName(GUILocalizeStrings.CurrentLanguage());
            }
            catch (Exception)
            {
                // when running MovingPicturesConfigTester outside of the MediaPortal directory this happens unfortunately
                // so we grab the active culture name from the system            
                lang = CultureInfo.CurrentUICulture.Name;
            }

            BaseConfig.MyAnimeLog.Write("Using language {0}", lang);

            path = Config.GetSubFolder(Config.Dir.Language, "MyAnime3");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            LoadTranslations(lang);
        }

        #endregion


        public static void PopulateLabels()
        {
            foreach (string name in Strings.Keys)
            {
                GUIPropertyManager.SetProperty("#Anime3.Translation." + name + ".Label", Strings[name]);
            }
        }

        #region Public Properties

        /// <summary>
        /// Gets the translated strings collection in the active language
        /// </summary>
        public static Dictionary<string, string> Strings
        {
            get
            {
                if (translations == null)
                {
                    translations = new Dictionary<string, string>();
                    Type transType = typeof (Translation);
                    FieldInfo[] fields = transType.GetFields(BindingFlags.Public | BindingFlags.Static);
                    foreach (FieldInfo field in fields)
                    {
                        translations.Add(field.Name, field.GetValue(transType).ToString());
                    }
                }
                return translations;
            }
        }

        #endregion

        #region Public Methods

        public static int LoadTranslations(string lang)
        {
            XmlDocument doc = new XmlDocument();
            Dictionary<string, string> TranslatedStrings = new Dictionary<string, string>();
            string langPath = "";
            try
            {
                langPath = Path.Combine(path, lang + ".xml");
                doc.Load(langPath);
            }
            catch (Exception e)
            {
                if (lang == "en")
                    return 0; // otherwise we are in an endless loop!

                if (e.GetType() == typeof (FileNotFoundException))
                    BaseConfig.MyAnimeLog.Write("Warning: Cannot find translation file {0}.  Failing back to English",
                                                langPath);
                else
                    BaseConfig.MyAnimeLog.Write(
                        "Error in translation xml file: {0}. Failing back to English, Exception: {1}", lang, e);
                return LoadTranslations("en");
            }
            if (doc.DocumentElement != null)
            {
                foreach (XmlNode stringEntry in doc.DocumentElement.ChildNodes)
                {
                    if (stringEntry.NodeType == XmlNodeType.Element)
                        try
                        {
                            if (stringEntry.Attributes != null)
                                TranslatedStrings.Add(stringEntry.Attributes.GetNamedItem("name").Value,
                                                      Regex.Unescape(stringEntry.InnerText));
                        }
                        catch (Exception ex)
                        {
                            BaseConfig.MyAnimeLog.Write("Error in Translation Engine. Exception: {0}", ex);
                        }
                }
            }
            Type TransType = typeof (Translation);
            FieldInfo[] fieldInfos = TransType.GetFields(BindingFlags.Public | BindingFlags.Static);
            foreach (FieldInfo fi in fieldInfos)
            {
                if (TranslatedStrings.ContainsKey(fi.Name))
                    TransType.InvokeMember(fi.Name, BindingFlags.SetField, null, TransType,
                                           new object[] {TranslatedStrings[fi.Name]});
                else
                    BaseConfig.MyAnimeLog.Write(
                        "Warning: Translation not found for name: {0}.  Using hard-coded English default.", fi.Name);
            }
            return TranslatedStrings.Count;
        }

        public static string GetByName(string name)
        {
            if (!Strings.ContainsKey(name))
                return name;

            return Strings[name];
        }

        public static string GetByName(string name, params object[] args)
        {
            return String.Format(GetByName(name), args);
        }

        /// <summary>
        /// Takes an input string and replaces all ${named} variables with the proper translation if available
        /// </summary>
        /// <param name="input">a string containing ${named} variables that represent the translation keys</param>
        /// <returns>translated input string</returns>
        public static string ParseString(string input)
        {
            MatchCollection matches = translateExpr.Matches(input);
            foreach (Match match in matches)
            {
                input = input.Replace(match.Value, GetByName(match.Groups[1].Value));
            }
            return input;
        }

        #endregion

        #region Translations / Strings

        /// <summary>
        /// These will be loaded with the language files content
        /// if the selected lang file is not found, it will first try to load en(us).xml as a backup
        /// if that also fails it will use the hardcoded strings as a last resort.
        /// </summary>

        public static string ActiveTorrentsAt = "{0} Active Torrents at {1}/sec";
        public static string AddToFavorites = "Add to Favorites";
        public static string AdvancedOptions = "Advanced options";

        public static string All = "All";
        public static string AllEpisodesAvailable = "All Episodes Available";
        public static string AlternateSetting = "Alternate Setting";
        public static string AniDBID = "AniDB ID";
        public static string AniDBRating = "AniDB Rating";
        public static string AnEpisodeWatchedAfter = "An episode is watched after";
        public static string Anime = "Anime";
        public static string Anime3Config = "Anime3 Config";
        public static string AnimeBytesCredentials="Anime Byt.es Credentials";
        public static string Any = "Any";
        public static string AreYouSureYouWantDeleteFile = "Are you sure you want to delete this file?";
        public static string AreYouSure = "Are you sure?";
        public static string Asc = "Asc";
        public static string Ascending = "Ascending ";
        public static string AssociateFileEpisode = "Associate File With This Episode";
        public static string AssociateFFDshowPreset = "Associate with a ffdshow raw preset";
        public static string AskBeforeStartStreamingPlayback = "Ask before starting streaming playback";
        public static string AskBeforeStartStreamingPlaybackDialogText = "Ask before streaming  ({0})";
        public static string AutoCloseAfter = "Auto close after";
        public static string AverageRating = "Average Rating";

        
        public static string BackToTorrents = "------ BACK TO TORRENTS ------";
        public static string BakaBTCredentials = "BakaBT Credentials";
        public static string BookmarkCreated = "Bookmark Created";
        public static string BookmarkThisAnime = "Bookmark This Anime";
        public static string Browse = "Browse";

        public static string Calendar = "Calendar";
        public static string CannotEditTvDBLink = "Cannot edit seasons when series has more than one TvDB link, use JMM Desktop instead";
        public static string ChangeLayout = "Change Layout";
        public static string ChangeSortName = "Change Sort Name";
        public static string Clear = "Clear";
        public static string ClearSearchHistory = "Clear Search History";
        public static string CloseFindPanel = "Close find panel after";
        public static string Collecting = "Collecting";
        public static string CommonCharacters = "Common Characters";
        public static string CommunitySays = "Community Says";
        public static string Confirmation = "Confirmation";
        public static string CouldNotFindFirstEpisode = "Could not find the first episode";
        public static string Coverflow = "Coverflow";
        public static string CreateSeriesForAnime = "Create Series for Anime";
        public static string Credits = "Credits";
        public static string CouldNotConnect = "Could not connect to JMM Server";
        public static string ConnectedTorrents = "Connected successfully, {0} torrents in list currently";
        public static string ConnectionFailed = "Connection failed";
        public static string ConnectedSucess = "Connected sucessfully";
        public static string ConnectedFailed = "Connected Failed";
        public static string Current = "Current";
        public static string CurrentUser = "Current User: {0}";
        public static string Custom = "Custom";

        public static string Databases = "Databases";
        public static string DataMissing = "Data Missing";
        public static string Day = "day";
        public static string Days = "days";
        public static string Default = "Default";
        public static string DefaultBanner = "Default Banner";
        public static string DefaultPoster = "Default Poster";
        public static string DefaultFanart = "Default Fanart";
        public static string DefaultSubtitleLanguage = "Default Subtitle Language";
        public static string DefaultAudioLanguage= "Default Audio Language";
        public static string DeleteFileFromDisk = "Delete file from disk";
        public static string DeleteThisGroupSeriesEpisodes = "Delete This Group/Series/Episodes";
        public static string DeleteThisSeriesEpisodes = "Delete This Series/Episodes";
        public static string Desc = "Desc";
        public static string Descending = "Descending";
        public static string Description = "Description";
        public static string Disable = "Disable";
        public static string Disabled = "Disabled";
        public static string Discord = "Discord";
        public static string Display= "Display";
        public static string DisplayGroupInfo= "Display group info after you stop scrolling for";
        public static string DisplayOptions = "Display Options";
        public static string Done = "Done";
        public static string DontDownload = "Don't Download";
        public static string DontShowThisAnime = "Don't Show This Anime (Ignore)";
        public static string Down = "Down";
        public static string Download = "Download";
        public static string Downloads = "Downloads";
        public static string DownloadThisEpisode = "Download this episode";
        public static string DownloadViaTorrent = "Download via Torrent";

        public static string Edit = "Edit";
        public static string EditGroup = "Edit Group";
        public static string EditSeries = "Edit Series";
        public static string Enable = "Enable";
        public static string Enabled = "Enabled";
        public static string Episode = "Episode";
        public static string EpisodeAddedDate = "Episode Added Date";
        public static string EpisodeAirDate = "Episode Air Date";
        public static string EpisodeRange = "Episode Range";
        public static string Episodes = "Episodes";
        public static string EpisodesShort = "Eps";
        public static string EpisodeWatchedDate = "Episode Watched Date";
        public static string Error = "Error";
        public static string ErrorConnectingJMMServer = "Error connecting to JMM Server";
        
        public static string EpisodeDisplay= "Episode Display";
        public static string EpisodeOverviewNA = "Episode Overview Not Available";
        public static string EpisodeNumberEg = "Episode Number (e.g. 13)";
        public static string EpisodeTitleEg = "Episode Title (e.g Destined Meeting)";


        public static string Fanart = "Fanart";
        public static string FFDShowRawPost = "ffdshow Raw Post-processing";
        public static string FileNotFoundLocally = "File not found on local share";
        public static string Find = "Find";
        public static string FindOnlyShowMatches = "Find - Only Show Matches ({0})";
        public static string File = "File";
        public static string Files = "Files";
        public static string FileOptions = "File Options";
        public static string FileSelection= "File Selection";
        public static string FileSelectionGroup = "Group (e.g. Datte Bayo)";
        public static string FileSelectionGroupShort = "Group Short (e.g DB)";
        public static string FileSelectionAudioCodec = "Audio Codec (e.g OGG Vorbis)";
        public static string FileSelectionFileCodec = "File Codec (e.g XVid)";
        public static string FileSelectionFileRes = "File Res (e.g 1280x720)";
        public static string FileSelectionFileSource = "File Source (e.g DVD)";
        public static string FileL = "file";
        

        public static string FilesAvailable = "Files Available";
        public static string FilesQueuedForProcessing = "Files have been queued for processing";
        public static string Filmstrip="Filmstrip";
        public static string FilterName = "Filter Name";
        public static string FilterOptions = "Filters";
        public static string FilterOptionAllEpisodes = "Watched episodes";
        public static string FilterOptionUnwatchedEpisodes = "Unwatched episodes";

        public static string FullStory = "Full Story";


        public static string GotoEpisodeList = "Go To Episode List";
        public static string GotoAniDB = "Go to AniDB";
        public static string GotoMAL = "Go to MyAnimeList";
        public static string GotoTvDB = "Go to TheTvDB";
        
        public static string Group = "Group";
        public static string GroupFilter = "Group Filter";
        public static string GroupFilters = "Group Filters";
        public static string GroupName = "Group Name";
        public static string GroupMenu = "Group Menu";
        public static string GroupNotFound = "Group not found";
        public static string Groups = "Grpups";

        public static string HiddenToPreventSpoiles = "Hidden to prevent spoilers";
        public static string HideWatchedEpisodes = "Hide Watched Episodes ({0})";
        public static string HideFilesWatched = "Hide files that have already been watched";
        public static string HidePlotUnwatched = "Hide plot for unwatched episodes";
        public static string High = "High";
        public static string Hour = "hour";
        public static string Hours = "hours";

        public static string Images = "Images";
        public static string ImageLocation = "Images Location";
        public static string ImageQualityPercentage = "Image quality (Percentage)";
        public static string ImportFolders = "Import Folders";
        public static string IgnoreFile = "Ignore file";
        public static string IncorrectPasswordTryAgain = "Incorrect password, please try again";
        public static string Information = "Information";
        public static string InfoDelay= "Info Delay";
        public static string IPAddress= "IP Address";

        public static string JMMServer = "JMM Server";

        public static string Language= "Language";
        public static string ListPosters = "List Posters";
        public static string Loading = "Loading";
        public static string LoadingData = "Loading Data";
        public static string LocalFiles = "Local Files";
        public static string LocalMapping= "Local Mapping";
        public static string Low = "Low";

        public static string Main = "Main";
        public static string MAL = "MAL";
        public static string MALID = "MAL ID";
        public static string Manual = "Manual";
        public static string ManualSearch = "Manual Search";
        public static string MarkAllAsUnwatched = "Mark ALL as Unwatched";
        public static string MarkAllAsWatched = "Mark ALL as Watched";
        public static string MarkAllPreviousAsUnwatched = "Mark ALL PREVIOUS as Watched";
        public static string MarkAllPreviousAsWatched = "Mark ALL PREVIOUS as Unwatched";
        public static string MarkAsUnwatched = "Mark as Unwatched";
        public static string MarkAsWatched = "Mark as Watched";
        public static string Match = "Match";
        public static string Matches = "Matches";
        public static string Medium = "Medium";
        public static string Milliseconds= "milliseconds";
        public static string Minute = "minute";
        public static string Minutes = "minutes";

        public static string MissingEpisodeCount = "Missing Episode Count";
        public static string ModeToggle = "Mode toggle key:";

        public static string MoreOptions = "More Options";
        public static string MultipleLinks = "Multiple Links";

        public static string NextMatch = "Next Match";
        public static string New = "New";
        public static string NewGroup = "New Group";
        public static string NewVersionAvailable = "New Version Available";
        public static string No = "No";
        public static string NoAwards = "No Awards";
        public static string NoEpisodesRecentlyWatched = "No episodes have recently been watched";
        public static string NoHistoryFound = "No History Found";
        public static string NoMatchesFound = "No Matches Found";
        public static string None = "None";
        public static string NoneL = "none";
        public static string NoRecommendationsAvailable = "No recommendations available";
        public static string NoResults = "No Results";
        public static string NoResultsFound = "No results found";
        public static string NoSeasonsFound = "No seasons found";
        public static string NoSubtitles = "No Subtitles";
        public static string NotInMyCollection = "Not In My Collection";
        public static string NoUnlinkedFilesToSelect = "No unlinked files to select";

        public static string Off = "Off";
        public static string On = "On";
        public static string Options = "Options";
        public static string OldPresetRemove = "Old preset successfully removed";
        public static string Other = "Other";
        public static string OnlyDisplayMatching = "Only display matching items";
        public static string OnlyShowAvailableEpisodes = "Only Show Available Episodes ({0})";
        public static string OnlyShowEpisodesComputer= "Only show episodes that are available on the computer";
        public static string OnlySubbingGroups = "Only for the subbing groups I am collecting";

        public static string Order = "Order";
        public static string OverviewNotAvailable = "Overview not available";

        public static string ParentStory="Parent Story";
        public static string Parody = "Parody";
        public static string Password= "Password";
        public static string Paused = "Paused";
        public static string PauseTorrent = "Pause Torrent";
        public static string PermanentVote = "Permanent Vote";
        public static string PlayFile = "Play file";
        public static string PlayPreviousEpisode = "Play Previous Episode";
        public static string PleaseExitMPFirst = "Please exit MP and set your JMM Server location first";
        public static string PleaseUsernameFirst = "Please enter a username first";
        public static string PleasePasswordFirst = "Please enter a password first";
        public static string PluginName= "Plugin Name";
        public static string PluginUpToDate = "Plugin up to date";
        public static string Port = "Port";
        public static string Poster = "Poster";
        public static string PosterQuality = "Poster Quality";
        public static string PosterQualityToolTip = "Used to adjust the quality of images shown in the coverflow and filmstrip layouts. \nSelecting a lower percentage will result in lower memory and CPU usage. Resolution at 100% is 1000 x 680";
        public static string Posters = "Posters";
        public static string PostProcessing = "Post-processing";
        public static string PredefinedFilters = "Predefined Filters";
        public static string PreferTheReleaseGroups = "Prefer the release groups I am collecting when downloading an episode";
        public static string Prequels = "Prequels";
        public static string PresetAdded = "Preset \"{0}\" successfully added";
        public static string PreviousEpisodeNotFound = "Previous episode not found";
        public static string Priority = "Priority";
        public static string ProcessRunningOnServer = "Process is running on the server";
        public static string PromptToRate = "Prompt to rate series when all episodes are watched";

        public static string QuickSort = "Quick Sort";


        public static string RandomSeries = "Random Series";
        public static string RandomEpisode = "Random Episode";
        public static string RateOne = "Abysmal";
        public static string RateTwo = "Terrible";
        public static string RateThree = "Bad";
        public static string RateFour = "Poor";
        public static string RateFive = "Mediocre";
        public static string RateSix = "Fair";
        public static string RateSeven = "Good";
        public static string RateEight = "Great";
        public static string RateNine = "Superb";
        public static string RateTen = "Perfect";

        public static string RecentSearches = "Recent Searches";
        public static string RefreshingView = "Refreshing view";
        public static string RehashFile = "Rehash file";
        public static string Relations = "Relations";
        public static string RemoveAsDefault = "Remove as Default";
        public static string RemoveDefaultSeries = "Remove Default Series";
        public static string RenameGroup = "Rename Group";
        public static string RemoveFromFavorites = "Remove from Favorites";
        public static string RemoveFileFromEpisode = "Remove File From This Episode";
        public static string RemoveMALAssociation = "Remove MAL Association";
        public static string RemoveMovieDBAssociation = "Remove MovieDB Association";
        public static string RemoveOldPresetAssociation = "Remove old preset association";
        public static string RemoveRecordsWithoutFile = "Remove records without physical file";
        public static string RemoveQuickSort = "Remove Quick Sort";
        public static string RemoveTorrent = "Remove Torrent";
        public static string RemoveTorrentAndData = "Remove Torrent And Data";
        public static string RemoveTraktAssociation = "Remove Trakt Association";
        public static string RemoveTVDBAssociation = "Remove TVDB Association";
        public static string RequestSendToServerPleaseRefresh = "Request sent to server, please refresh view";
        public static string Result = "Result";
        public static string Results = "Results";
        public static string RevokeVote = "Revoke Vote";
        public static string Running = "Running";




        public static string SameSetting="Same Setting";
        public static string Save = "Save";
        public static string ScanDropFolder = "Scan Drop Folder(s)";
        public static string Search = "Search";
        public static string SearchForTorrents = "Search for Torrents";
        public static string SearchHistory = "Search History";
        public static string Searching = "Searching";
        public static string SearchMAL = "Search MAL";
        public static string SearchTheMovieDB = "Search The MovieDB";
        public static string SearchTheTvDB = "Search The TvDB";
        public static string SearchTrakt = "Search Trakt.Tv";
        public static string SearchResults = "Search Results";
        public static string SearchUsing = "Search using";
        public static string Season = "Season";
        public static string SeasonResults = "Season Results";
        public static string SelectFile = "Select File";
        public static string SelectSeason = "Select Season";
        public static string SelectSeries = "Select Series";
        public static string SelectSource = "Select Source";
        public static string SelectAFolder = "Select a folder";
        public static string SelectUser = "Select User";
        public static string Second = "second";
        public static string Seconds = "seconds";
        public static string Sequels = "Sequels";
        public static string SelectTag = "Select Tag";
        public static string SelectDefaultLanguage = "Select Default Language";
        public static string Series = "Series";
        public static string SeriesAddedDate = "Series Added Date";
        public static string SeriesInformation = "Series Information";
        public static string SeriesCount = "Series Count";
        public static string SeriesCreated = "Series Created";
        public static string ServerAddress= "Server Address";
        public static string ServerPort = "Server Port";
        
        public static string ServerStatus = "Server Status";
        public static string SetAsDefault = "Set as Default";
        public static string SetDefaultAudioLanguage = "Set Default Audio Language";
        public static string SetDefaultSubtitleLanguage = "Set Default Subtitle language";
        public static string SetDefaultSeries = "Set Default Series";
        public static string SetPreset = "Set preset";
        public static string ShowIndicatorForMissingEps = "Show indicator for missing episodes";
        public static string ShowPrsetLoadNotify = "Show preset load notifications";
        public static string SideStory = "Side Story";
        public static string SinglesSeriesDisplay = "Singles Series Display";
        public static string SinglesSeriesDisplayToolTip = "When a group only has one series, the series name will be displayed instead of the group name.This could have a performance impact in large collections";
        public static string Source = "Source";
        public static string SortDirection = "Sort Direction";
        public static string SortName = "Sort Name";
        public static string Specials = "Specials";
        public static string StartWord = "Start Word ({0})";
        public static string Starting = "Starting";
        public static string StartTextToggle = "Start text toggle key:";
        public static string StartTorrent = "Start Torrent";
        public static string StopTorrent = "Stop Torrent";
        public static string StreamingNotSupported = "File not local and streaming not yet supported";

        public static string Sucess = "Sucess";
        public static string Summary = "Summary";
        public static string Support = "Support";
        public static string SyncVotes = "Sync Votes from AniDB";
        public static string SyncMyList = "Sync MyList from AniDB";
        public static string SwitchSeason = "Switch Season (Current is {0})";
 
        public static string T9 = "T9";
        public static string Text = "Text";
        public static string TemporaryVote = "Temporary Vote";
        public static string ThisIsFormattingEps= "This is the formatting used when showing multiple files for one episode";
        public static string TorrentMonitor = "Torrent Monitor";
        public static string TorrentSources = "Torrent Sources";
        public static string TestConnection = "Test Connection";
        public static string TestLogin = "Test Login";
        public static string TheMovieDB = "The Movie DB";
        public static string TheTVDB = "The TV DB";
        
        public static string ThisEpisodeHasNoAssociatedFiles = "This episode has no associated files";
        public static string Trailers = "Trailers";
        public static string TraktSearchResults = "Trakt Search Results";
        public static string Trakt = "Trakt.Tv";
        public static string TryToUseLocalThumb= "Try to use local episode thumbnails if none exist remotely";
        public static string TvDBID = "The TV DB ID:";
        public static string TVDBSearchResults = "TvDB Search Results";

        public static string UnlinkedFiles = "Unlinked Files";
        public static string UnwatchedEpisodeCount = "Unwatched Episode Count";
        public static string Up= "Up";
        public static string UpdateSeriesInfo = "Update Series Info From AniDB";
        public static string UseFileDefault = "Use File Default";
        public static string UseSeriesName = "Use series name for single series groups";
        public static string UseStreaming = "Use Streaming";
        public static string UserRating = "User Rating";
        public static string Username= "Username";
        public static string UseSystemDefault = "Use System Default";
        public static string uTorrent="uTorrent";
        public static string Version = "Version";
        public static string ViewSeriesInfo = "View Series Info";
        public static string Votes = "Votes";


        public static string Year = "Year";
        public static string Yes = "Yes";
        public static string YouDontHaveTheSeries = "You do not have this series in your collection";

        public static string WaitingOnServer = "Waiting on server";
        public static string WaitFFDShow = "Wait ffdshow load for";
        public static string Watch = "Watch";
        public static string Website = "WebSite";
        public static string WideBanner = "Wide Banner";
        public static string WideBanners = "Wide Banners";
        public static string WideBannerQuality = "Wide Banner Quality";
        public static string WideBannerQualityToolTip = "Used to adjust the quality of images shown in the Wide Banner layouts. \nSelecting a lower percentage will result in lower memory and CPU usage. Resolution at 100% is 758 x 140";

        #endregion

    }
}
