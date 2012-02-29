using System;
using System.Collections.Generic;
using System.Text;

namespace MyAnimePlugin3
{
	public static class Constants
	{
        public static readonly string WebCacheError = @"<error>No Results</error>";
        public static readonly string AniDBTitlesURL = @"http://anidb.net/api/animetitles.dat.gz";

		#region PlugIn Info constants
		public struct PlugInInfo
		{
			public static readonly int ID = 6101;

			/// <summary>
			/// Name of PlugIn.
			/// </summary>
			public static readonly string NAME = "My Anime 3";

			/// <summary>
			/// Description of PlugIn.
			/// </summary>
			public static readonly string DESCRIPTION = "My Anime 3";

			/// <summary>
			/// Author of PlugIn.
			/// </summary>
			public static readonly string AUTHOR = "Leo Werndly";

			public readonly static string VERSION = "0180";
		}
		#endregion

		#region Window IDs
		public struct WindowIDs
		{
			/// <summary>
			/// The main windows id.
			/// </summary>
			public static readonly int MAIN = 6101;
			public static readonly int ADMIN = 6102;
			public static readonly int FANART = 6103;
			public static readonly int POSTERS = 6104;
			public static readonly int CHARACTERS = 6105;
			public static readonly int WIDEBANNERS = 6106;
			public static readonly int RELATIONS = 6107;
			public static readonly int CALENDAR = 6108;
            public static readonly int ANIMEINFO = 6109;
			public static readonly int DOWNLOADS = 6110;
            public static readonly int COLLECTION = 6111;
            public static readonly int BROWSER = 6112;
            public static readonly int NEWS = 6113;
			public static readonly int WATCHING = 6113;
			public static readonly int RECOMMENDATIONS = 6114;
			public static readonly int SIMILAR = 6115;
			public static readonly int RANDOM = 6116;
			public static readonly int PLAYLISTS = 6117;

			public static readonly int RELATIONS_OLD = 610711;

		}
		#endregion

	

		#region Control IDs
		public struct ControlIDs
		{
			public static readonly int LISTBOX_FILES = 4;
		}
		#endregion

		#region Labels
		public struct Labels
		{
			public static readonly string LASTWATCHED = "Last Watched";
			public static readonly string NEWEPISODES = "New Episodes";
			public static readonly string FAVES = "Favorites";
			public static readonly string FAVESNEW = "New in Favorites";
			public static readonly string MISSING = "Missing Episodes";
			public static readonly string MAINVIEW = "[ Main View ]";
			public static readonly string PREVIOUSFOLDER = "..";

		}

		public struct CharacterType
		{
			public static readonly string MAIN = "main character in";
		}

		public struct SeriesDisplayString
		{
			public static readonly string SeriesName = "<SeriesName>";
			public static readonly string AniDBNameRomaji = "<AniDBNameRomaji>";
			public static readonly string AniDBNameEnglish = "<AniDBNameEnglish>";
			public static readonly string TvDBSeason = "<TvDBSeason>";
			public static readonly string AnimeYear = "<AnimeYear>";
		}

		public struct GroupDisplayString
		{
			public static readonly string GroupName = "<GroupName>";
			public static readonly string AniDBNameRomaji = "<AniDBNameRomaji>";
			public static readonly string AniDBNameEnglish = "<AniDBNameEnglish>";
			public static readonly string AnimeYear = "<AnimeYear>";
		}

        public struct FileSelectionDisplayString
        {
            public static readonly string Group = "<AnGroup>";
            public static readonly string GroupShort = "<AnGroupShort>";
            public static readonly string FileSource = "<FileSource>";
            public static readonly string FileRes = "<FileRes>";
            public static readonly string FileCodec = "<FileCodec>";
            public static readonly string AudioCodec = "<AudioCodec>";
			public static readonly string VideoBitDepth = "<VideoBitDepth>";
        }

		public struct EpisodeDisplayString
		{
			public static readonly string EpisodeNumber = "<EpNo>";
			public static readonly string EpisodeName = "<EpName>";
		}

		public struct URLS
		{
			public static readonly string MAL_Series_Prefix = @"http://myanimelist.net/anime/";

			public static readonly string aniDBFileURLPrefix = @"http://anidb.net/perl-bin/animedb.pl?show=file&fid=";
			public static readonly string aniDBEpisodeURLPrefix = @"http://anidb.net/perl-bin/animedb.pl?show=ep&eid=";
			public static readonly string aniDBSeriesURLPrefix = @"http://anidb.net/perl-bin/animedb.pl?show=anime&aid=";

			public static readonly string tvDBSeriesURLPrefix = @"http://thetvdb.com/?tab=series&id=";
			//public static readonly string tvDBEpisodeURLPrefix = @"http://anidb.net/perl-bin/animedb.pl?show=ep&eid=";

			public static readonly string movieDBSeriesURLPrefix = @"http://www.themoviedb.org/movie/";

			public static readonly string MAL_Series = @"http://myanimelist.net/anime/{0}";

			public static readonly string AniDB_File = @"http://anidb.net/perl-bin/animedb.pl?show=file&fid={0}";
			public static readonly string AniDB_Episode = @"http://anidb.net/perl-bin/animedb.pl?show=ep&eid={0}";
			public static readonly string AniDB_Series = @"http://anidb.net/perl-bin/animedb.pl?show=anime&aid={0}";
			public static readonly string AniDB_ReleaseGroup = @"http://anidb.net/perl-bin/animedb.pl?show=group&gid={0}";
			public static readonly string AniDB_Images = @"http://img7.anidb.net/pics/anime/{0}";

			public static readonly string TvDB_Series = @"http://thetvdb.com/?tab=series&id={0}";
			public static readonly string TvDB_Images = @"http://thetvdb.com/banners/{0}";

			public static readonly string MovieDB_Series = @"http://www.themoviedb.org/movie/{0}";
			public static readonly string Trakt_Series = @"http://trakt.tv/show/{0}";
		}

		public struct GroupLabelStyle
		{
			public static readonly string EpCount = "Total Episode Count";
			public static readonly string Unwatched = "Only Unwatched Episode Count";
			public static readonly string WatchedUnwatched = "Watched and Unwatched Episode Counts";
		}

		public struct EpisodeLabelStyle
		{
			public static readonly string IconsDate = "Icons and Date";
			public static readonly string IconsOnly = "Icons Only";
		}

		#endregion

		public struct WebURLStrings
		{
			
		}

		public struct TorrentSourceNames
		{
			public static readonly string TT = "Tokyo Toshokan";
			public static readonly string AnimeSuki = "Anime Suki";
			public static readonly string BakaBT = "Baka BT";
			public static readonly string BakaUpdates = "BakaUpdates";
			public static readonly string Nyaa = "Nyaa Torrents";
		}

		public struct EpisodeTypeStrings
		{
			public static readonly string Normal = "Episodes";
			public static readonly string Credits = "Credits";
			public static readonly string Specials = "Specials";
			public static readonly string Trailer = "Trailer";
			public static readonly string Parody = "Parody";
			public static readonly string Other = "Other";
		}

		public struct TvDBURLs
		{
			public static readonly string apiKey = "B178B8940CAF4A2C";
			public static readonly string prefixSeriesSearch = @"http://www.thetvdb.com/api/GetSeries.php?seriesname=";
			public static readonly string urlFullSeriesData = @"{0}/api/{1}/series/{2}/all/en.zip"; // mirrirURL, apiKey, seriesID
			public static readonly string urlBannersXML = @"{0}/api/{1}/series/{2}/banners.xml"; // mirrirURL, apiKey, seriesID
			public static readonly string urlSeriesBaseXML = @"{0}/api/{1}/series/{2}/en.xml"; // mirrirURL, apiKey, seriesID
			public static readonly string urlEpisodeXML = @"{0}/api/{1}/episodes/{2}/en.xml"; // mirrirURL, apiKey, episodeID
			public static readonly string urlUpdatesList = @"{0}/api/Updates.php?type=all&time={1}"; // mirrirURL, server time
		}

		public struct Folders
		{
			public static readonly string thumbsSubFolder = "AnimeThumbs";
			public static readonly string thumbsTvDB = @"TvDB";
			public static readonly string thumbsAniDB = @"AniDB";
			public static readonly string thumbsAniDB_Chars = @"AniDB\Characters";
			public static readonly string thumbsAniDB_Creators = @"AniDB\Creators";
			public static readonly string thumbsMAL = @"MAL";
			public static readonly string thumbsMovieDB = @"MovieDB";
		}
		
	}

	public static class Globals
	{
		public static string CurrentFolderPath = "";
		public static int CurrentFolderListIndex = 0;
		//public static AniDBLib aniDBHelper = new AniDBLib();
		//public static MyAnimeListParser myAnimeListHelper = new MyAnimeListParser();
		public static bool firstStart = true;
		public static string CurrentCharID = "";
		public static MediaPortal.Localisation.LocalisationProvider Localisation = new MediaPortal.Localisation.LocalisationProvider(MediaPortal.Configuration.Config.GetFolder(MediaPortal.Configuration.Config.Dir.Language), MediaPortal.GUI.Library.GUILocalizeStrings.GetCultureName(MediaPortal.GUI.Library.GUILocalizeStrings.CurrentLanguage()));
		public static System.Globalization.CultureInfo Culture = Localisation.CurrentLanguage.IsNeutralCulture ? System.Globalization.CultureInfo.CreateSpecificCulture(Localisation.CurrentLanguage.Name) : Localisation.CurrentLanguage;

		private static List<string> folderHistory = new List<string>();

		public static void AddToFolderHistory(string folder)
		{
			folderHistory.Add(folder);
		}

		public static string LastFolder
		{
			get
			{
				if (folderHistory.Count < 1) return "";

				return folderHistory[folderHistory.Count - 1];
			}
		}

	}
}
