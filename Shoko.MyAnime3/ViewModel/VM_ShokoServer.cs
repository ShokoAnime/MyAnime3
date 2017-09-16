using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using Microsoft.Win32;
using Nancy.Rest.Client;
using Shoko.Models.Client;
using Shoko.Models.Enums;
using Shoko.Models.Interfaces;
using Shoko.Models.Server;
using Shoko.MyAnime3.ConfigFiles;
using Shoko.MyAnime3.Events;
using Shoko.MyAnime3.ViewModel.Helpers;
using Shoko.MyAnime3.ViewModel.Server;
using Shoko.MyAnime3.Windows;

namespace Shoko.MyAnime3.ViewModel
{
    public class VM_ShokoServer
    {
        private static VM_ShokoServer _instance;
        private Timer serverStatusTimer;
 

        public object userLock = new object();

        public bool UserAuthenticated { get; set; }
        public JMMUser CurrentUser { get; set; }

        public List<ImportFolder> ImportFolders { get; set; }
        public bool ServerOnline { get; set; }

        public bool IsBanned { get; set; }
        public bool IsAdminUser { get; set; }
        public string BanReason { get; set; }
        public string Username { get; set; }
        public int HasherQueueCount { get; set; }
        public string HasherQueueState { get; set; }
        public int GeneralQueueCount { get; set; }
        public string GeneralQueueState { get; set; }
        public bool HasherQueueRunning { get; set; }
        public bool GeneralQueueRunning { get; set; }
        public bool TraktEnabled { get; set; }
        public string TraktAuthToken { get; set; }

        public DataSourceType EpisodeTitleSource { get; set; }
        public DataSourceType SeriesDescriptionSource { get; set; }
        public DataSourceType SeriesNameSource { get; set; }

        public delegate void ServerStatusEventHandler(ServerStatusEventArgs ev);

        public event ServerStatusEventHandler ServerStatusEvent;

        protected void OnServerStatusEvent(ServerStatusEventArgs ev)
        {
            if (ServerStatusEvent != null)
                ServerStatusEvent(ev);
        }

        public static VM_ShokoServer Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new VM_ShokoServer();
                    _instance.Init();
                }
                return _instance;
            }
        }

        private VM_ShokoServer()
        {
        }

        private void Init()
        {
            IsBanned = false;
            IsAdminUser = false;
            BanReason = "";
            Username = "";
            HasherQueueCount = 0;
            HasherQueueState = "";
            GeneralQueueCount = 0;
            GeneralQueueState = "";
            HasherQueueRunning = false;
            GeneralQueueRunning = false;

            UserAuthenticated = false;
            ImportFolders = new List<ImportFolder>();
            //UnselectedLanguages = new ObservableCollection<NamingLanguage>();
            //SelectedLanguages = new ObservableCollection<NamingLanguage>();
            //AllUsers = new ObservableCollection<JMMUser>();

            try
            {
                //SetupClient();
                //SetupTCPClient();
                SetupClient();
            }
            catch
            {
                // ignored
            }

            // timer for server status
            serverStatusTimer = new Timer();
            serverStatusTimer.AutoReset = false;
            serverStatusTimer.Interval = 4 * 1000; // 4 seconds
            serverStatusTimer.Elapsed += serverStatusTimer_Elapsed;
            serverStatusTimer.Enabled = true;
            AddTempPathToSubtilePaths();
        }

        private void AddTempPathToSubtilePaths()
        {
            string path = Path.GetTempPath();
            //FFDSHow
            try
            {
                RegistryKey k = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\GNU\ffdshow", true);
                if (k != null)
                {
                    string org = (string) k.GetValue("subSearchDir", null);
                    if (string.IsNullOrEmpty(org))
                        org = path;
                    else if (!org.Contains(path))
                        org += ";" + path;
                    k.SetValue("subSearchDir", org);
                }
                k = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\GNU\ffdshow64", true);
                if (k != null)
                {
                    string org = (string) k.GetValue("subSearchDir", null);
                    if (string.IsNullOrEmpty(org))
                        org = path;
                    else if (!org.Contains(path))
                        org += ";" + path;
                    k.SetValue("subSearchDir", org);
                }
                k = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Gabest\VSFilter\DefTextPathes", true);
                if (k != null)
                    for (int x = 0; x < 10; x++)
                    {
                        string val = (string) k.GetValue("Path" + x, null);
                        if (val != null && val == path)
                            break;
                        if (val == null)
                        {
                            k.SetValue("Path" + x, path);
                            break;
                        }
                    }
                k = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\MPC-HC\MPC-HC\Settings", true);
                if (k != null)
                {
                    string org = (string) k.GetValue("SubtitlePaths", null);
                    if (string.IsNullOrEmpty(org))
                        org = path;
                    else if (!org.Contains(path))
                        org += ";" + path;
                    k.SetValue("SubtitlePaths", org);
                }
                k = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Daum\PotPlayerMini\CaptionFolderList", true);
                if (k != null)
                    for (int x = 0; x < 10; x++)
                    {
                        string val = (string) k.GetValue(x.ToString(), null);
                        if (val != null && val == path)
                            break;
                        if (val == null)
                        {
                            k.SetValue(x.ToString(), path);
                            break;
                        }
                    }
                string vlcrcpath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "vlc", "vlcrc");
                try
                {
                    if (File.Exists(vlcrcpath))
                    {
                        string[] lines = File.ReadAllLines(vlcrcpath);
                        for (int x = 0; x < lines.Length; x++)
                        {
                            string s = lines[x];
                            if (s.StartsWith("#sub-autodetect-path=") || s.StartsWith("sub-autodetect-path="))
                                if (!s.Contains(path))
                                {
                                    s += ", " + path;
                                    if (s.StartsWith("#"))
                                        s = s.Substring(1);
                                    lines[x] = s;
                                    File.WriteAllLines(vlcrcpath, lines);
                                    break;
                                }
                        }
                    }
                }
                catch
                {
                    // ignored
                }
            }
            catch
            {
                // ignored
            }
        }

        private IShokoServer _shokoservices;

        public IShokoServer ShokoServices
        {
            get
            {
                if (_shokoservices == null)
                    try
                    {
                        SetupClient();
                    }
                    catch
                    {
                        // ignored
                    }
                return _shokoservices;
            }
        }

        private IShokoServerImage _imageClient;

        public IShokoServerImage ShokoImages
        {
            get
            {
                if (_imageClient == null)
                    try
                    {
                        SetupImageClient();
                    }
                    catch
                    {
                        // ignored
                    }
                return _imageClient;
            }
        }


        void serverStatusTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (!ServerOnline)
                {
                    serverStatusTimer.Start();
                    return;
                }

                CL_ServerStatus status = Instance.ShokoServices.GetServerStatus();

                HasherQueueCount = status.HashQueueCount;
                GeneralQueueCount = status.GeneralQueueCount;

                HasherQueueState = status.HashQueueState;
                GeneralQueueState = status.GeneralQueueState;

                IsBanned = status.IsBanned;
                BanReason = status.BanReason;

                HasherQueueRunning = !HasherQueueState.ToLower().Contains("pause");
                GeneralQueueRunning = !GeneralQueueState.ToLower().Contains("pause");

                ServerStatusEventArgs evt = new ServerStatusEventArgs();
                evt.BanReason = BanReason;
                evt.GeneralQueueCount = GeneralQueueCount;
                evt.GeneralQueueRunning = GeneralQueueRunning;
                evt.GeneralQueueState = GeneralQueueState;
                evt.HasherQueueCount = HasherQueueCount;
                evt.HasherQueueRunning = HasherQueueRunning;
                evt.HasherQueueState = HasherQueueState;

                evt.ImagesQueueCount = status.ImagesQueueCount;
                evt.ImagesQueueRunning = !status.ImagesQueueState.ToLower().Contains("pause");
                evt.ImagesQueueState = status.ImagesQueueState;

                evt.IsBanned = IsBanned;

                OnServerStatusEvent(evt);

                //string msg = string.Format("JMM Server Status: {0}/{1} -- {2}/{3}", GeneralQueueState, GeneralQueueCount, HasherQueueState, HasherQueueCount);
                //BaseConfig.MyAnimeLog.Write(msg);
            }

            catch
            {
                // ignored
            }

            serverStatusTimer.Start();
        }

        public static bool SettingsAreValid()
        {
            AnimePluginSettings settings = AnimePluginSettings.Instance;
            if (string.IsNullOrEmpty(settings.JMMServer_Address) || string.IsNullOrEmpty(settings.JMMServer_Port))
                return false;


            return true;
        }

        public void SetupImageClient()
        {
            //ServerOnline = false;
            _imageClient = null;

            if (!SettingsAreValid()) return;

            try
            {
                AnimePluginSettings settings = AnimePluginSettings.Instance;
                _imageClient = ClientFactory.Create<IShokoServerImage>($"http://{settings.JMMServer_Address}:{settings.JMMServer_Port}/");
            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write(ex.ToString());
            }
        }


        public bool SetupClient()
        {
            ServerOnline = false;
            _shokoservices = null;
            ImportFolders.Clear();

            if (!SettingsAreValid()) return false;

            try
            {
                AnimePluginSettings settings = AnimePluginSettings.Instance;
                string url = string.Format(@"http://{0}:{1}/", settings.JMMServer_Address, settings.JMMServer_Port);
                BaseConfig.MyAnimeLog.Write("JMM Server URL: " + url);


                Dictionary<Type, Type> mappings = new Dictionary<Type, Type>();

                //Mappings area.
                mappings.Add(typeof(CL_AniDB_Anime), typeof(VM_AniDB_Anime));
                mappings.Add(typeof(CL_AniDB_Anime_DefaultImage), typeof(VM_AniDB_Anime_DefaultImage));
                mappings.Add(typeof(CL_AniDB_AnimeCrossRefs), typeof(VM_AniDB_AnimeCrossRefs));
                mappings.Add(typeof(CL_AnimeEpisode_User), typeof(VM_AnimeEpisode_User));
                mappings.Add(typeof(CL_AnimeGroup_User), typeof(VM_AnimeGroup_User));
                mappings.Add(typeof(CL_AnimeSeries_User), typeof(VM_AnimeSeries_User));
                mappings.Add(typeof(CL_BookmarkedAnime), typeof(VM_BookmarkedAnime));
                mappings.Add(typeof(CL_GroupFilter), typeof(VM_GroupFilter));
                mappings.Add(typeof(MovieDB_Fanart), typeof(VM_MovieDB_Fanart));
                mappings.Add(typeof(MovieDB_Poster), typeof(VM_MovieDB_Poster));
                mappings.Add(typeof(CL_Recommendation), typeof(VM_Recommendation));
               // mappings.Add(typeof(Trakt_ImageFanart), typeof(VM_Trakt_ImageFanart));
               // mappings.Add(typeof(Trakt_ImagePoster), typeof(VM_Trakt_ImagePoster));
                mappings.Add(typeof(TvDB_ImageFanart), typeof(VM_TvDB_ImageFanart));
                mappings.Add(typeof(TvDB_ImagePoster), typeof(VM_TvDB_ImagePoster));
                mappings.Add(typeof(TvDB_ImageWideBanner), typeof(VM_TvDB_ImageWideBanner));
                mappings.Add(typeof(CL_VideoDetailed), typeof(VM_VideoDetailed));
                mappings.Add(typeof(CL_VideoLocal), typeof(VM_VideoLocal));


                _shokoservices =
                    ClientFactory.Create<IShokoServer>(
                        $"http://{settings.JMMServer_Address}:{settings.JMMServer_Port}/", mappings);
                // try connecting to see if the server is responding
                CL_ServerStatus status = Instance.ShokoServices.GetServerStatus();
                ServerOnline = true;

                GetServerSettings();
                RefreshImportFolders();

                BaseConfig.MyAnimeLog.Write("JMM Server Status: " + status.GeneralQueueState);

                return true;
            }
            catch (Exception ex)
            {
                //Utils.ShowErrorMessage(ex);
                BaseConfig.MyAnimeLog.Write(ex.ToString());
                return false;
            }
        }

        private void GetServerSettings()
        {
            CL_ServerSettings contract = _shokoservices.GetServerSettings();

            // Language
            EpisodeTitleSource = (DataSourceType) contract.EpisodeTitleSource;
            SeriesDescriptionSource = (DataSourceType) contract.SeriesDescriptionSource;
            SeriesNameSource = (DataSourceType) contract.SeriesNameSource;

            // Trakt
            TraktEnabled = contract.Trakt_IsEnabled;
            TraktAuthToken = contract.Trakt_AuthToken;
        }

        public void RefreshImportFolders()
        {
            ImportFolders.Clear();

            if (!ServerOnline) return;
            try
            {
                ImportFolders = Instance.ShokoServices.GetImportFolders();
            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write("RefreshImportFolders: " + ex);
            }
        }

        public bool AuthenticateUser(string username, string password)
        {
            JMMUser retUser = Instance.ShokoServices.AuthenticateUser(username, password);
            if (retUser != null)
                return true;
            return false;
        }

        public bool PromptUserLogin()
        {
            GUIDialogMenu dlg = (GUIDialogMenu) GUIWindowManager.GetWindow((int) GUIWindow.Window.WINDOW_DIALOG_MENU);
            if (dlg == null)
                return true;

            //keep showing the dialog until the user closes it
            int selectedLabel;

            dlg.Reset();
            dlg.SetHeading(Translation.SelectUser);

            if (CurrentUser != null)
            {
                string msgCurUser = string.Format(Translation.CurrentUser, CurrentUser.Username);

                dlg.Add(msgCurUser);
                dlg.Add("--------");
                dlg.Add(">> " + Translation.LogOut);
            }

            List<JMMUser> allUsers = ShokoServerHelper.GetAllUsers();
            foreach (JMMUser user in allUsers)
                dlg.Add(user.Username);

            dlg.DoModal(GUIWindowManager.ActiveWindow);
            selectedLabel = dlg.SelectedLabel;

            if (selectedLabel < 0) return false;
            if (CurrentUser != null) selectedLabel = selectedLabel - 3;

            if (dlg.SelectedLabelText == ">> " + Translation.LogOut)
            {
                LogOut(true);
                return false;
            }
            JMMUser selUser = allUsers[selectedLabel];
            

            BaseConfig.MyAnimeLog.Write("Selected user label: " + selectedLabel);

            // try and auth user with a blank password

            bool authed = AuthenticateUser(selUser.Username, "");
            string password = "";
            while (!authed)
                // prompt user for a password
                if (Utils.DialogText(ref password, true, GUIWindowManager.ActiveWindow))
                {
                    authed = AuthenticateUser(selUser.Username, password);
                    if (!authed)
                        Utils.DialogMsg(Translation.Error, Translation.IncorrectPasswordTryAgain);
                }
                else
                {
                    return false;
                }

            SetCurrentUser(selUser);

            return true;
        }

        public void LogOut(bool returnToHome)
        {
            CurrentUser = null;
            Username = "";
            IsAdminUser = false;
            UserAuthenticated = false;
            MainWindow.isFirstInitDone = false;

            if (returnToHome)
                MainWindow.ReturnToMPHome();
        }

        public void SetCurrentUser(JMMUser user)
        {
            CurrentUser = user;
            Username = CurrentUser.Username;
            IsAdminUser = CurrentUser.IsAdmin == 1;
            UserAuthenticated = true;
        }
    }
}