using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using System.IO;
using MyAnimePlugin3.Downloads;

namespace MyAnimePlugin3
{
	public class AnimePluginSettings
	{
		// MA3
		public string JMMServer_Address = "";
		public string JMMServer_Port = "";
		public bool HideEpisodeImageWhenUnwatched = false;
		public bool HideEpisodeOverviewWhenUnwatched = false;
		public string ImportFolderMappingsList = "";
		public string CurrentJMMUserID = "";
		public bool DisplayRatingDialogOnCompletion = true;


		public Dictionary<int, string> ImportFolderMappings
		{
			get
			{
				Dictionary<int, string> mappings = new Dictionary<int, string>();
				if (string.IsNullOrEmpty(ImportFolderMappingsList)) return mappings;

				string[] arrmpgs = ImportFolderMappingsList.Split(';');
				foreach (string arrval in arrmpgs)
				{
					if (string.IsNullOrEmpty(arrval)) continue;

					string[] vals = arrval.Split('|');
					mappings[int.Parse(vals[0])] = vals[1];
				}

				return mappings;
			}
		}

		public void SetImportFolderMapping(int folderID, string localPath)
		{
			string output = "";

			// check if we already have this in the existing settings
			bool exists = ImportFolderMappings.ContainsKey(folderID);

			string[] arrmpgs = ImportFolderMappingsList.Split(';');
			foreach (string arrval in arrmpgs)
			{
				if (string.IsNullOrEmpty(arrval)) continue;

				if (output.Length > 0) output += ";";

				string[] vals = arrval.Split('|');

				int thisFolderID = int.Parse(vals[0]);
				if (thisFolderID == folderID)
					output += string.Format("{0}|{1}", thisFolderID, localPath);
				else
					output += string.Format("{0}|{1}", thisFolderID, vals[1]);
			}

			// new entry
			if (!exists)
			{
				if (output.Length > 0) output += ";";
				output += string.Format("{0}|{1}", folderID, localPath);
			}

			ImportFolderMappingsList = output;

			BaseConfig.MyAnimeLog.Write("ImportFolderMappingsList: " + ImportFolderMappingsList);
		}

		public void RemoveImportFolderMapping(int folderID)
		{
			if (ImportFolderMappings.ContainsKey(folderID))
			{
				string output = "";

				string[] arrmpgs = ImportFolderMappingsList.Split(';');
				foreach (string arrval in arrmpgs)
				{
					if (string.IsNullOrEmpty(arrval)) continue;

					string[] vals = arrval.Split('|');

					int thisFolderID = int.Parse(vals[0]);
					if (thisFolderID != folderID)
					{
						if (output.Length > 0) output += ";";
						output += string.Format("{0}|{1}", thisFolderID, vals[1]);
					}
				}

				ImportFolderMappingsList = output;
			}
		}

		// MA3

        public string LastGroupList = "";

		public bool ShowMissing = false;
		public bool ShowMissingMyGroupsOnly = false;
		public bool HideWatchedFiles = false;

		public string DefaultAudioLanguage = "";
		public string DefaultSubtitleLanguage = "";

		public int FindTimeout_s = 3;
		public SearchMode FindMode = SearchMode.t9;
		public bool FindStartWord = true;
		public bool FindFilter = false;

		public int WatchedPercentage = 90;
		public int InfoDelay = 150;

		public RenamingType fileRenameType = RenamingType.Raw;
		public string EpisodeDisplayFormat = "";

		public View LastView = null;
		public ViewClassification LastViewClassification = ViewClassification.Views;
		public string LastStaticViewID = "";

		public GUIFacadeControl.Layout LastGroupViewMode = GUIFacadeControl.Layout.List; // Poster List
		public GUIFacadeControl.Layout LastFanartViewMode = GUIFacadeControl.Layout.List; // fanart window view mode
		public GUIFacadeControl.Layout LastPosterViewMode = GUIFacadeControl.Layout.List; // poster window view mode

        public string fileSelectionDisplayFormat = "";

		public string XMLWebCacheIP = "anime.hobbydb.net.leaf.arvixe.com";
		//public string XMLWebCacheIP = "anime.hobbydb.net"; 
		//public string XMLWebCacheIP = "localhost:8080";

		public bool AniDBAutoEpisodesSubbed = true;
		public bool ShowOnlyAvailableEpisodes = true;

		public bool HidePlot = false;

		public View.eLabelStyleGroups LabelStyleGroups = View.eLabelStyleGroups.WatchedUnwatched;
		public View.eLabelStyleEpisodes LabelStyleEpisodes = View.eLabelStyleEpisodes.IconsDate;

		public bool MenuDeleteFiles = false;

		public string PluginName = "My Anime 3";

		private string thumbsFolder = "";

		public string UTorrentAddress = "";
		public string UTorrentUsername = "";
		public string UTorrentPassword = "";
		public string UTorrentPort = "";
        public bool UseHashFromCache = true;


		public static string TorrentSourcesAll = MyAnimePlugin3.Constants.TorrentSourceNames.Nyaa + ";" +
			MyAnimePlugin3.Constants.TorrentSourceNames.TT + ";" +
			MyAnimePlugin3.Constants.TorrentSourceNames.AnimeSuki + ";" +
			MyAnimePlugin3.Constants.TorrentSourceNames.BakaUpdates + ";";

		public string TorrentSourcesRaw = "";
		public List<string> TorrentSources = new List<string>();

		public bool TorrentPreferOwnGroups = true;

		public int PosterSizePct = 50; // percent of poster size
		public int BannerSizePct = 50; // percent of banner size


		public bool HasCustomThumbsFolder
		{
			get
			{
				if (thumbsFolder.Trim().Length > 0)
					return true;
				else
					return false;
			}
		}

		public string ThumbsFolder
		{
			get
			{
				//BaseConfig.MyAnimeLog.Write("Thumbs Folder: {0}", Config.GetFolder(Config.Dir.Thumbs));

				if (thumbsFolder.Trim().Length > 0)
				{
					// strip any trailing "\"'s
					if (Directory.Exists(thumbsFolder))
						return thumbsFolder.Trim().TrimEnd('\\');

					try
					{
						Directory.CreateDirectory(thumbsFolder);
					}
					catch (Exception ex)
					{
						BaseConfig.MyAnimeLog.Write("Could not create Thumbs Folder: {0} - {1}", thumbsFolder, ex.Message);
						return Config.GetFolder(Config.Dir.Thumbs);
					}

					return thumbsFolder.Trim().TrimEnd('\\');
				}

				return Config.GetFolder(Config.Dir.Thumbs);  // use default MP thumbs directory 
			}
			set
			{
				thumbsFolder = value;
			}
		}

		public AnimePluginSettings()
		{
			Load();
		}

		public string MediaFolder
		{
			get { return Config.GetFolder(Config.Dir.Plugins) + @"\Windows\AnimePlugin\"; }
		}

		public bool TorrentsTT
		{
			get
			{
				return TorrentSources.Contains(MyAnimePlugin3.Constants.TorrentSourceNames.TT);
			}
		}

		public bool TorrentsAnimeSuki
		{
			get
			{
				return TorrentSources.Contains(MyAnimePlugin3.Constants.TorrentSourceNames.AnimeSuki);
			}
		}

		public bool TorrentsBakaBT
		{
			get
			{
				return TorrentSources.Contains(MyAnimePlugin3.Constants.TorrentSourceNames.BakaBT);
			}
		}

		public bool TorrentsBakaUpdates
		{
			get
			{
				return TorrentSources.Contains(MyAnimePlugin3.Constants.TorrentSourceNames.BakaUpdates);
			}
		}

		public bool TorrentsNyaa
		{
			get
			{
				return TorrentSources.Contains(MyAnimePlugin3.Constants.TorrentSourceNames.Nyaa);
			}
		}

		public TorrentSource DefaultTorrentSource
		{
			get
			{
				if (TorrentSources.Count == 0) return TorrentSource.TokyoToshokan;

				if (TorrentSources[0] == MyAnimePlugin3.Constants.TorrentSourceNames.AnimeSuki) return TorrentSource.AnimeSuki;
				if (TorrentSources[0] == MyAnimePlugin3.Constants.TorrentSourceNames.BakaBT) return TorrentSource.BakaBT;
				if (TorrentSources[0] == MyAnimePlugin3.Constants.TorrentSourceNames.BakaUpdates) return TorrentSource.BakaUpdates;
				if (TorrentSources[0] == MyAnimePlugin3.Constants.TorrentSourceNames.Nyaa) return TorrentSource.Nyaa;
				if (TorrentSources[0] == MyAnimePlugin3.Constants.TorrentSourceNames.TT) return TorrentSource.TokyoToshokan;

				return TorrentSource.TokyoToshokan;
			}
		}

		private bool GetBooleanSetting(ref MediaPortal.Profile.Settings xmlreader, string settingName, bool defaultValue)
		{
			string val = "";
			val = xmlreader.GetValueAsString("Anime3", settingName, defaultValue ? "1" : "0");
			return val == "0" ? false : true;
		}

		public void Load()
		{
			StringDictionary settings = new StringDictionary();
			MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml");


			JMMServer_Address = xmlreader.GetValueAsString("Anime3", "JMMServer_Address", "127.0.0.1");
			JMMServer_Port = xmlreader.GetValueAsString("Anime3", "JMMServer_Port", "8111");
			ImportFolderMappingsList = xmlreader.GetValueAsString("Anime3", "ImportFolderMappingsList", "");
			CurrentJMMUserID = xmlreader.GetValueAsString("Anime3", "CurrentJMMUserID", "");

			thumbsFolder = xmlreader.GetValueAsString("Anime3", "ThumbsFolder", "");
			PluginName = xmlreader.GetValueAsString("Anime3", "PluginName", "My Anime 3");

				
			//XMLWebServiceIP = xmlreader.GetValueAsString("Anime2", "XMLWebServiceIP", "anime.hobbydb.net");
            LastGroupList = xmlreader.GetValueAsString("Anime2", "LastGroupList", "");

			UTorrentAddress = xmlreader.GetValueAsString("Anime3", "UTorrentAddress", "");
			UTorrentPassword = xmlreader.GetValueAsString("Anime3", "UTorrentPassword", "");
			UTorrentPort = xmlreader.GetValueAsString("Anime3", "UTorrentPort", "");
			UTorrentUsername = xmlreader.GetValueAsString("Anime3", "UTorrentUsername", "");

			TorrentSourcesRaw = xmlreader.GetValueAsString("Anime3", "TorrentSources", AnimePluginSettings.TorrentSourcesAll);

			TorrentPreferOwnGroups = GetBooleanSetting(ref xmlreader, "TorrentPreferOwnGroups", true);

			WatchedPercentage = int.Parse(xmlreader.GetValueAsString("Anime3", "WatchedPercentage", "90"));

			EpisodeDisplayFormat = xmlreader.GetValueAsString("Anime3", "EpisodeDisplayFormat", @"<EpNo>: <EpName>");
			fileSelectionDisplayFormat = xmlreader.GetValueAsString("Anime3", "FileSelectionDisplayFormat", @"<AnGroupShort> - <FileRes> / <FileSource> / <VideoBitDepth>bit");

			ShowMissing = GetBooleanSetting(ref xmlreader, "ShowMissing", true);
			ShowMissingMyGroupsOnly = GetBooleanSetting(ref xmlreader, "ShowMissingMyGroupsOnly", false);
			DisplayRatingDialogOnCompletion = GetBooleanSetting(ref xmlreader, "DisplayRatingDialogOnCompletion", true);

			string viewMode = "";
			viewMode = xmlreader.GetValueAsString("Anime3", "LastGroupViewMode", "0");
			LastGroupViewMode = (GUIFacadeControl.Layout)int.Parse(viewMode);

			string viewModeFan = "";
			viewModeFan = xmlreader.GetValueAsString("Anime3", "LastFanartViewMode", "1");
			LastFanartViewMode = (GUIFacadeControl.Layout)int.Parse(viewModeFan);

			string viewModePoster = "";
			viewModePoster = xmlreader.GetValueAsString("Anime3", "LastPosterViewMode", "2");
			LastPosterViewMode = (GUIFacadeControl.Layout)int.Parse(viewModePoster);

			HideWatchedFiles = GetBooleanSetting(ref xmlreader, "HideWatchedFiles", false);

			DefaultAudioLanguage = xmlreader.GetValueAsString("Anime3", "DefaultAudioLanguage", @"<file>");
			DefaultSubtitleLanguage = xmlreader.GetValueAsString("Anime3", "DefaultSubtitleLanguage", @"<file>");
				
			string findtimeout = "";
			findtimeout = xmlreader.GetValueAsString("Anime3", "FindTimeout", "3");
			FindTimeout_s = int.Parse(findtimeout);

			string findmode = "";
			findmode = xmlreader.GetValueAsString("Anime3", "FindMode", "0");
			FindMode = (SearchMode)int.Parse(findmode);

			FindStartWord = GetBooleanSetting(ref xmlreader, "FindStartWord", true);
			FindFilter = GetBooleanSetting(ref xmlreader, "FindFilter", true);
			AniDBAutoEpisodesSubbed = GetBooleanSetting(ref xmlreader, "AniDBAutoEpisodesSubbed", true);
			ShowOnlyAvailableEpisodes = GetBooleanSetting(ref xmlreader, "ShowOnlyAvailableEpisodes", true);
			HidePlot = GetBooleanSetting(ref xmlreader, "HidePlot", true);
			MenuDeleteFiles = GetBooleanSetting(ref xmlreader, "MenuDeleteFiles", false);

			string infodel = "";
			infodel = xmlreader.GetValueAsString("Anime3", "InfoDelay", "150");
			InfoDelay = int.Parse(infodel);

			string postpct = "";
			postpct = xmlreader.GetValueAsString("Anime3", "PosterSizePct", "50");
			int tmpPost = 0;
			int.TryParse(postpct, out tmpPost);

			if (tmpPost > 0 && tmpPost <= 100)
				PosterSizePct = tmpPost;
			else
				PosterSizePct = 50;

			string banpct = "";
			banpct = xmlreader.GetValueAsString("Anime3", "BannerSizePct", "50");
			int tmpBanner = 0;
			int.TryParse(banpct, out tmpBanner);

			if (tmpBanner > 0 && tmpBanner <= 100)
				BannerSizePct = tmpBanner;
			else
				BannerSizePct = 50;


			xmlreader.Dispose();



			// parse the list of torrent sources
			if (TorrentSourcesRaw.Length > 0)
			{
				string[] fitems = TorrentSourcesRaw.Split(';');
				foreach (string s in fitems)
				{
					TorrentSources.Add(s);
				}
			}
		}

		public void Save()
		{
			using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings("MediaPortal.xml"))
			{
				xmlwriter.SetValue("Anime3", "JMMServer_Address", JMMServer_Address.Trim());
				xmlwriter.SetValue("Anime3", "JMMServer_Port", JMMServer_Port.Trim());
				xmlwriter.SetValue("Anime3", "ImportFolderMappingsList", ImportFolderMappingsList.Trim());
				xmlwriter.SetValue("Anime3", "CurrentJMMUserID", CurrentJMMUserID);

				BaseConfig.MyAnimeLog.Write("ImportFolderMappingsList.Save: " + ImportFolderMappingsList);

				xmlwriter.SetValue("Anime3", "ThumbsFolder", thumbsFolder); // use the raw value
				xmlwriter.SetValue("Anime3", "PluginName", PluginName.Trim());

				xmlwriter.SetValue("Anime3", "UTorrentAddress", UTorrentAddress.Trim());
				xmlwriter.SetValue("Anime3", "UTorrentPassword", UTorrentPassword.Trim());
				xmlwriter.SetValue("Anime3", "UTorrentPort", UTorrentPort.Trim());
				xmlwriter.SetValue("Anime3", "UTorrentUsername", UTorrentUsername.Trim());

				// save the list of torrent sources
				string torList = "";
				foreach (string src in TorrentSources)
				{
					if (torList.Length > 0)
						torList += ";";

					torList += src;
				}
				xmlwriter.SetValue("Anime3", "TorrentSources", torList);

				xmlwriter.SetValue("Anime3", "TorrentPreferOwnGroups", TorrentPreferOwnGroups ? "1" : "0");

				xmlwriter.SetValue("Anime3", "WatchedPercentage", WatchedPercentage.ToString());

                xmlwriter.SetValue("Anime3", "ShowMissing", ShowMissing ? "1" : "0");
				xmlwriter.SetValue("Anime3", "ShowMissingMyGroupsOnly", ShowMissingMyGroupsOnly ? "1" : "0");
				xmlwriter.SetValue("Anime3", "DisplayRatingDialogOnCompletion", DisplayRatingDialogOnCompletion ? "1" : "0");

                xmlwriter.SetValue("Anime3", "HideWatchedFiles", HideWatchedFiles ? "1" : "0");

				xmlwriter.SetValue("Anime3", "DefaultAudioLanguage", DefaultAudioLanguage);
				xmlwriter.SetValue("Anime3", "DefaultSubtitleLanguage", DefaultSubtitleLanguage);

				xmlwriter.SetValue("Anime3", "FindTimeout", FindTimeout_s);
				xmlwriter.SetValue("Anime3", "FindMode", (int)FindMode);
				xmlwriter.SetValue("Anime3", "FindStartWord", FindStartWord ? "1" : "0");
				xmlwriter.SetValue("Anime3", "FindFilter", FindFilter ? "1" : "0");

				xmlwriter.SetValue("Anime3", "InfoDelay", InfoDelay.ToString());

				xmlwriter.SetValue("Anime3", "LastGroupViewMode", ((int)LastGroupViewMode).ToString());
				xmlwriter.SetValue("Anime3", "LastFanartViewMode", ((int)LastFanartViewMode).ToString());
				xmlwriter.SetValue("Anime3", "LastPosterViewMode", ((int)LastPosterViewMode).ToString());

				xmlwriter.SetValue("Anime3", "AniDBAutoEpisodesSubbed", AniDBAutoEpisodesSubbed ? "1" : "0");
				xmlwriter.SetValue("Anime3", "ShowOnlyAvailableEpisodes", ShowOnlyAvailableEpisodes ? "1" : "0");
				xmlwriter.SetValue("Anime3", "HidePlot", HidePlot ? "1" : "0");

				xmlwriter.SetValue("Anime3", "MenuDeleteFiles", MenuDeleteFiles ? "1" : "0");

				xmlwriter.SetValue("Anime3", "PosterSizePct", PosterSizePct.ToString());
				xmlwriter.SetValue("Anime3", "BannerSizePct", BannerSizePct.ToString());

				xmlwriter.SetValue("Anime3", "EpisodeDisplayFormat", EpisodeDisplayFormat);
				xmlwriter.SetValue("Anime3", "FileSelectionDisplayFormat", fileSelectionDisplayFormat);
				

				
             
			}
		}
	}

	public enum RenamingType
	{
		Raw = 1,
		MetaData = 2
	}

	public enum enFanartSize
	{
		All = 1,
		HD = 2,
		FullHD = 3
	}

	public enum RenamingLanguage
	{
		Romaji = 1,
		English = 2
	}

	public enum Storage
	{
		Unknown = 1,
		HDD = 2,
		CD = 3
	}
}
