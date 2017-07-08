using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using Shoko.Commons;
using Shoko.MyAnime3.Windows;

namespace Shoko.MyAnime3.ConfigFiles
{
    public class AnimePluginSettings
    {
        public static AnimePluginSettings Instance { get; } = new AnimePluginSettings();
      

        // MA3
        public string JMMServer_Address = "";

        public string JMMServer_Port = "";
        public bool AskBeforeStartStreamingPlayback = true;
        public bool HideEpisodeImageWhenUnwatched = false;
        public bool HideEpisodeOverviewWhenUnwatched = false;
        public string ImportFolderMappingsList = "";
        public string CurrentJMMUserID = "";
        public bool DisplayRatingDialogOnCompletion = true;
        public bool LoadLocalThumbnails = true;

        public bool FfdshowNotificationsShow = true;
        public bool FfdshowNotificationsAutoClose = true;
        public bool FfdshowNotificationsLock = true;
        public int FfdshowNotificationsAutoCloseTime = 3000;
        public int FfdshowNotificationsLockTime = 5000;

        public string ModeToggleKey = "]";
        public string StartTextToggleKey = "[";
        private string _subPaths;
        public bool BasicHome = true;
        public bool HomeButtonNavigation = true;


        public Dictionary<int, string> GetMappings()
        {
            Dictionary<int, string> mappings = new Dictionary<int, string>();

            string mpgs = ImportFolderMappingsList;

            if (string.IsNullOrEmpty(mpgs)) return mappings;

            string[] arrmpgs = mpgs.Split(';');
            foreach (string arrval in arrmpgs)
            {
                if (string.IsNullOrEmpty(arrval)) continue;

                string[] vals = arrval.Split('|');
                mappings[int.Parse(vals[0])] = vals[1];
            }

            return mappings;
        }

        public void SetMappings(Dictionary<int, string> mappings)
        {
            string[] maps = mappings.Select(a => a.Key.ToString(CultureInfo.InvariantCulture) + "|" + a.Value).ToArray();
            ImportFolderMappingsList = string.Join(";", maps);
            BaseConfig.MyAnimeLog.Write("ImportFolderMappingsList: " + ImportFolderMappingsList);
        }


        // MA3

        public string LastGroupList = "";

        public bool ShowMissing;
        public bool ShowMissingMyGroupsOnly;
        public bool HideWatchedFiles;

        public string DefaultAudioLanguage = "";
        public string DefaultSubtitleLanguage = "";

        public int FindTimeout_s = 3;
        public SearchMode FindMode = SearchMode.t9;
        public bool FindStartWord = true;
        public bool FindFilter;

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

        public bool HidePlot;

        public View.eLabelStyleGroups LabelStyleGroups = View.eLabelStyleGroups.WatchedUnwatched;
        public View.eLabelStyleEpisodes LabelStyleEpisodes = View.eLabelStyleEpisodes.IconsDate;

        public bool MenuDeleteFiles;

        public string PluginName = "My Anime 3";

        private string thumbsFolder = "";


        public bool UseHashFromCache = true;


        public int PosterSizePct = 50; // percent of poster size
        public int BannerSizePct = 50; // percent of banner size


        public bool HasCustomThumbsFolder
        {
            get
            {
                if (thumbsFolder.Trim().Length > 0)
                    return true;
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

                return Config.GetFolder(Config.Dir.Thumbs); // use default MP thumbs directory 
            }
            set { thumbsFolder = value; }
        }

        public AnimePluginSettings()
        {
            Load();
            FolderMappings.Instance.SetLoadAndSaveCallback(GetMappings, SetMappings);
        }

        public string MediaFolder
        {
            get { return Config.GetFolder(Config.Dir.Plugins) + @"\Windows\AnimePlugin\"; }
        }

   
        private PropertyInfo ResolveProperty(Expression<Func<object>> expr)
        {
            var member = expr.Body as MemberExpression;
            if (member == null)
            {
                var ue = expr.Body as UnaryExpression;
                if (ue != null)
                    member = ue.Operand as MemberExpression;
            }
            if (member != null)
                return GetType().GetProperty(member.Member.Name);
            return null;
        }


        private bool GetBooleanSetting(ref Settings xmlreader, string settingName, bool defaultValue)
        {
            string val = xmlreader.GetValueAsString("Anime3", settingName, defaultValue ? "1" : "0");
            return val != "0";
        }

        public void Load()
        {
            Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml"));
            //MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml");


            JMMServer_Address = xmlreader.GetValueAsString("Anime3", "JMMServer_Address", "127.0.0.1");
            JMMServer_Port = xmlreader.GetValueAsString("Anime3", "JMMServer_Port", "8111");
            ImportFolderMappingsList = xmlreader.GetValueAsString("Anime3", "ImportFolderMappingsList", "");
            CurrentJMMUserID = xmlreader.GetValueAsString("Anime3", "CurrentJMMUserID", "");

            thumbsFolder = xmlreader.GetValueAsString("Anime3", "ThumbsFolder", "");
            PluginName = xmlreader.GetValueAsString("Anime3", "PluginName", "My Anime 3");


            //XMLWebServiceIP = xmlreader.GetValueAsString("Anime2", "XMLWebServiceIP", "anime.hobbydb.net");
            LastGroupList = xmlreader.GetValueAsString("Anime3", "LastGroupList", "");


            WatchedPercentage = int.Parse(xmlreader.GetValueAsString("Anime3", "WatchedPercentage", "90"));

            EpisodeDisplayFormat = xmlreader.GetValueAsString("Anime3", "EpisodeDisplayFormat", @"<EpNo>: <EpName>");
            fileSelectionDisplayFormat = xmlreader.GetValueAsString("Anime3", "FileSelectionDisplayFormat", @"<AnGroupShort> - <FileRes> / <FileSource> / <VideoBitDepth>bit");

            ShowMissing = GetBooleanSetting(ref xmlreader, "ShowMissing", true);
            ShowMissingMyGroupsOnly = GetBooleanSetting(ref xmlreader, "ShowMissingMyGroupsOnly", false);
            DisplayRatingDialogOnCompletion = GetBooleanSetting(ref xmlreader, "DisplayRatingDialogOnCompletion", true);

            string viewMode= xmlreader.GetValueAsString("Anime3", "LastGroupViewMode", "0");
            LastGroupViewMode = (GUIFacadeControl.Layout) int.Parse(viewMode);

            string viewModeFan = xmlreader.GetValueAsString("Anime3", "LastFanartViewMode", "1");
            LastFanartViewMode = (GUIFacadeControl.Layout) int.Parse(viewModeFan);

            string viewModePoster = xmlreader.GetValueAsString("Anime3", "LastPosterViewMode", "2");
            LastPosterViewMode = (GUIFacadeControl.Layout) int.Parse(viewModePoster);

            HideWatchedFiles = GetBooleanSetting(ref xmlreader, "HideWatchedFiles", false);

            DefaultAudioLanguage = xmlreader.GetValueAsString("Anime3", "DefaultAudioLanguage", @"<file>");
            DefaultSubtitleLanguage = xmlreader.GetValueAsString("Anime3", "DefaultSubtitleLanguage", @"<file>");

            string findtimeout = xmlreader.GetValueAsString("Anime3", "FindTimeout", "3");
            FindTimeout_s = int.Parse(findtimeout);

            string findmode = xmlreader.GetValueAsString("Anime3", "FindMode", "0");
            FindMode = (SearchMode) int.Parse(findmode);

            FindStartWord = GetBooleanSetting(ref xmlreader, "FindStartWord", true);
            FindFilter = GetBooleanSetting(ref xmlreader, "FindFilter", true);
            AniDBAutoEpisodesSubbed = GetBooleanSetting(ref xmlreader, "AniDBAutoEpisodesSubbed", true);
            ShowOnlyAvailableEpisodes = GetBooleanSetting(ref xmlreader, "ShowOnlyAvailableEpisodes", true);
            HidePlot = GetBooleanSetting(ref xmlreader, "HidePlot", true);
            MenuDeleteFiles = GetBooleanSetting(ref xmlreader, "MenuDeleteFiles", false);

            string infodel = xmlreader.GetValueAsString("Anime3", "InfoDelay", "150");
            InfoDelay = int.Parse(infodel);

            string postpct = xmlreader.GetValueAsString("Anime3", "PosterSizePct", "50");
            int tmpPost;
            int.TryParse(postpct, out tmpPost);

            if (tmpPost > 0 && tmpPost <= 100)
                PosterSizePct = tmpPost;
            else
                PosterSizePct = 50;

            LoadLocalThumbnails = GetBooleanSetting(ref xmlreader, "LoadLocalThumbnails", true);

            string banpct = xmlreader.GetValueAsString("Anime3", "BannerSizePct", "50");
            int tmpBanner;
            int.TryParse(banpct, out tmpBanner);

            if (tmpBanner > 0 && tmpBanner <= 100)
                BannerSizePct = tmpBanner;
            else
                BannerSizePct = 50;


            string ffdshowNotificationsShow = xmlreader.GetValueAsString("Anime3", "FfdshowNotificationsShow", "1");
            FfdshowNotificationsShow = ffdshowNotificationsShow != "0";

            string ffdshowNotificationsAutoClose = xmlreader.GetValueAsString("Anime3", "FfdshowNotificationsAutoClose", "1");
            FfdshowNotificationsAutoClose = ffdshowNotificationsAutoClose != "0";

            string ffdshowNotificationsLock = xmlreader.GetValueAsString("Anime3", "FfdshowNotificationsLock", "1");
            FfdshowNotificationsLock = ffdshowNotificationsLock != "0";

            FfdshowNotificationsAutoCloseTime = int.Parse(xmlreader.GetValueAsString("Anime3", "FfdshowNotificationsAutoCloseTime", "3000"));
            FfdshowNotificationsLockTime = int.Parse(xmlreader.GetValueAsString("Anime3", "FfdshowNotificationsLockTime", "5000"));

            ModeToggleKey = xmlreader.GetValueAsString("Anime3", "ModeToggleKey", "]");
            StartTextToggleKey = xmlreader.GetValueAsString("Anime3", "StartTextToggleKey", "[");
            AskBeforeStartStreamingPlayback = GetBooleanSetting(ref xmlreader, "AskBeforeStartStreamingPlayback", true);
            HomeButtonNavigation = GetBooleanSetting(ref xmlreader, "HomeButtonNavigation", true);
            BasicHome = xmlreader.GetValueAsBool("gui", "startbasichome", false);

            _subPaths = xmlreader.GetValueAsString("subtitles", "paths", @".\");
            xmlreader.Dispose();
        }


        public void Save()
        {
            using (Settings xmlwriter = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
            {
                xmlwriter.SetValue("Anime3", "JMMServer_Address", JMMServer_Address.Trim());
                xmlwriter.SetValue("Anime3", "JMMServer_Port", JMMServer_Port.Trim());
                xmlwriter.SetValue("Anime3", "ImportFolderMappingsList", ImportFolderMappingsList.Trim());
                xmlwriter.SetValue("Anime3", "CurrentJMMUserID", CurrentJMMUserID);

                BaseConfig.MyAnimeLog.Write("ImportFolderMappingsList.Save: " + ImportFolderMappingsList);

                xmlwriter.SetValue("Anime3", "ThumbsFolder", thumbsFolder); // use the raw value
                xmlwriter.SetValue("Anime3", "PluginName", PluginName.Trim());


                xmlwriter.SetValue("Anime3", "WatchedPercentage", WatchedPercentage.ToString());

                xmlwriter.SetValue("Anime3", "ShowMissing", ShowMissing ? "1" : "0");
                xmlwriter.SetValue("Anime3", "ShowMissingMyGroupsOnly", ShowMissingMyGroupsOnly ? "1" : "0");
                xmlwriter.SetValue("Anime3", "DisplayRatingDialogOnCompletion", DisplayRatingDialogOnCompletion ? "1" : "0");

                xmlwriter.SetValue("Anime3", "HideWatchedFiles", HideWatchedFiles ? "1" : "0");

                xmlwriter.SetValue("Anime3", "DefaultAudioLanguage", DefaultAudioLanguage);
                xmlwriter.SetValue("Anime3", "DefaultSubtitleLanguage", DefaultSubtitleLanguage);

                xmlwriter.SetValue("Anime3", "FindTimeout", FindTimeout_s);
                xmlwriter.SetValue("Anime3", "FindMode", (int) FindMode);
                xmlwriter.SetValue("Anime3", "FindStartWord", FindStartWord ? "1" : "0");
                xmlwriter.SetValue("Anime3", "FindFilter", FindFilter ? "1" : "0");

                xmlwriter.SetValue("Anime3", "InfoDelay", InfoDelay.ToString());

                xmlwriter.SetValue("Anime3", "LastGroupViewMode", ((int) LastGroupViewMode).ToString());
                xmlwriter.SetValue("Anime3", "LastFanartViewMode", ((int) LastFanartViewMode).ToString());
                xmlwriter.SetValue("Anime3", "LastPosterViewMode", ((int) LastPosterViewMode).ToString());
                xmlwriter.SetValue("Anime3", "LoadLocalThumbnails", LoadLocalThumbnails ? "1" : "0");

                xmlwriter.SetValue("Anime3", "AniDBAutoEpisodesSubbed", AniDBAutoEpisodesSubbed ? "1" : "0");
                xmlwriter.SetValue("Anime3", "ShowOnlyAvailableEpisodes", ShowOnlyAvailableEpisodes ? "1" : "0");
                xmlwriter.SetValue("Anime3", "HidePlot", HidePlot ? "1" : "0");

                xmlwriter.SetValue("Anime3", "MenuDeleteFiles", MenuDeleteFiles ? "1" : "0");

                xmlwriter.SetValue("Anime3", "PosterSizePct", PosterSizePct.ToString());
                xmlwriter.SetValue("Anime3", "BannerSizePct", BannerSizePct.ToString());

                xmlwriter.SetValue("Anime3", "EpisodeDisplayFormat", EpisodeDisplayFormat);
                xmlwriter.SetValue("Anime3", "FileSelectionDisplayFormat", fileSelectionDisplayFormat);

                xmlwriter.SetValue("Anime3", "FfdshowNotificationsShow", FfdshowNotificationsShow ? "1" : "0");
                xmlwriter.SetValue("Anime3", "FfdshowNotificationsAutoClose", FfdshowNotificationsAutoClose ? "1" : "0");
                xmlwriter.SetValue("Anime3", "FfdshowNotificationsLock", FfdshowNotificationsLock ? "1" : "0");
                xmlwriter.SetValue("Anime3", "FfdshowNotificationsAutoCloseTime", FfdshowNotificationsAutoCloseTime.ToString());
                xmlwriter.SetValue("Anime3", "FfdshowNotificationsLockTime", FfdshowNotificationsLockTime.ToString());
                xmlwriter.SetValue("Anime3", "ModeToggleKey", ModeToggleKey);
                xmlwriter.SetValue("Anime3", "StartTextToggleKey", StartTextToggleKey);
                xmlwriter.SetValue("Anime3", "AskBeforeStartStreamingPlayback", AskBeforeStartStreamingPlayback ? "1" : "0");
                xmlwriter.SetValue("Anime3", "HomeButtonNavigation", HomeButtonNavigation ? "1" : "0");

                string pth = Path.GetTempPath();
                if (!_subPaths.Contains(pth))
                {
                    _subPaths += "," + pth;
                    xmlwriter.SetValue("subtitles", "paths", _subPaths);
                }
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