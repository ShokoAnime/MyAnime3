using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.ServiceModel.Channels;
using System.ServiceModel;
using System.ServiceModel.Description;
using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
using Microsoft.Win32;
using MyAnimePlugin3.Events;
using MyAnimePlugin3.JMMServerBinary;

namespace MyAnimePlugin3.ViewModel
{
	public class JMMServerVM
	{
		private static JMMServerVM _instance;
		private System.Timers.Timer serverStatusTimer = null;
		private System.Timers.Timer saveTimer = null;

		public object userLock = new object();

		public bool UserAuthenticated { get; set; }
		public JMMUserVM CurrentUser { get; set; }

		public List<ImportFolderVM> ImportFolders { get; set; }
		public bool ServerOnline { get; set; }

		public bool IsBanned { get; set; }
		public bool IsAdminUser { get; set; }
		public string BanReason  { get; set; }
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
			{
				ServerStatusEvent(ev);
			}
		}

		public static JMMServerVM Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new JMMServerVM();
					_instance.Init();
				}
				return _instance;
			}
		}

		private JMMServerVM()
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
			ImportFolders = new List<ImportFolderVM>();
			//UnselectedLanguages = new ObservableCollection<NamingLanguage>();
			//SelectedLanguages = new ObservableCollection<NamingLanguage>();
			//AllUsers = new ObservableCollection<JMMUserVM>();

			try
			{
				//SetupClient();
				//SetupTCPClient();
				SetupBinaryClient();
			}
			catch { }

			// timer for server status
			serverStatusTimer = new System.Timers.Timer();
			serverStatusTimer.AutoReset = false;
			serverStatusTimer.Interval = 4 * 1000; // 4 seconds
			serverStatusTimer.Elapsed += new System.Timers.ElapsedEventHandler(serverStatusTimer_Elapsed);
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
                    string org = (string)k.GetValue("subSearchDir", null);
                    if (string.IsNullOrEmpty(org))
                        org = path;
                    else if (!org.Contains(path))
                        org += ";" + path;
                    k.SetValue("subSearchDir", org);
                }
                k = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\GNU\ffdshow64", true);
                if (k != null)
                {
                    string org = (string)k.GetValue("subSearchDir", null);
                    if (string.IsNullOrEmpty(org))
                        org = path;
                    else if (!org.Contains(path))
                        org += ";" + path;
                    k.SetValue("subSearchDir", org);
                }
                k = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Gabest\VSFilter\DefTextPathes", true);
                if (k != null)
                {
                    for (int x = 0; x < 10; x++)
                    {
                        string val = (string)k.GetValue("Path" + x, null);
                        if (val != null && val == path)
                            break;
                        if (val == null)
                        {
                            k.SetValue("Path" + x, path);
                            break;
                        }
                    }
                }
                k = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\MPC-HC\MPC-HC\Settings", true);
                if (k != null)
                {
                    string org = (string)k.GetValue("SubtitlePaths", null);
                    if (string.IsNullOrEmpty(org))
                        org = path;
                    else if (!org.Contains(path))
                        org += ";" + path;
                    k.SetValue("SubtitlePaths", org);
                }
                k = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Daum\PotPlayerMini\CaptionFolderList", true);
                if (k != null)
                {
                    for (int x = 0; x < 10; x++)
                    {
                        string val = (string)k.GetValue(x.ToString(), null);
                        if (val != null && val == path)
                            break;
                        if (val == null)
                        {
                            k.SetValue(x.ToString(), path);
                            break;
                        }
                    }
                }
                string vlcrcpath = Path.Combine(Environment.GetFolderPath((Environment.SpecialFolder.ApplicationData)), "vlc", "vlcrc");
                try
                {
                    if (File.Exists(vlcrcpath))
                    {
                        string[] lines = File.ReadAllLines(vlcrcpath);
                        for (int x = 0; x < lines.Length; x++)
                        {
                            string s = lines[x];
                            if (s.StartsWith("#sub-autodetect-path=") || s.StartsWith("sub-autodetect-path="))
                            {
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

                }
                catch (Exception)
                {

                }
            }
            catch (Exception e)
            {
                int a = 1;
            }


        }
        private JMMServerBinary.IJMMServer _clientBinaryHTTP = null;
		public JMMServerBinary.IJMMServer clientBinaryHTTP
		{
			get
			{
				if (_clientBinaryHTTP == null)
				{
					try
					{
						SetupBinaryClient();
					}
					catch { }
				}
				return _clientBinaryHTTP;
			}
		}

		private JMMImageServer.JMMServerImageClient _imageClient = null;
		public JMMImageServer.JMMServerImageClient imageClient
		{
			get
			{
				if (_imageClient == null)
				{
					try
					{
						SetupImageClient();
					}
					catch { }
				}
				return _imageClient;
			}
		}

		void serverStatusTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			try
			{
				if (!ServerOnline)
				{
					serverStatusTimer.Start();
					return;
				}

				JMMServerBinary.Contract_ServerStatus status = JMMServerVM.Instance.clientBinaryHTTP.GetServerStatus();

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

				string msg = string.Format("JMM Server Status: {0}/{1} -- {2}/{3}", GeneralQueueState, GeneralQueueCount, HasherQueueState, HasherQueueCount);
				//BaseConfig.MyAnimeLog.Write(msg);
			}

			catch { }

			serverStatusTimer.Start();
		}

		public static bool SettingsAreValid()
		{
			AnimePluginSettings settings = new AnimePluginSettings();
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
				AnimePluginSettings settings = new AnimePluginSettings();
				string url = string.Format(@"http://{0}:{1}/JMMServerImage", settings.JMMServer_Address, settings.JMMServer_Port);
				BasicHttpBinding binding = new BasicHttpBinding();
				binding.MessageEncoding = WSMessageEncoding.Mtom;
				binding.MaxReceivedMessageSize = 2147483647;
				binding.ReaderQuotas.MaxArrayLength = 2147483647;
				EndpointAddress endpoint = new EndpointAddress(new Uri(url));
				_imageClient = new JMMImageServer.JMMServerImageClient(binding, endpoint);
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write(ex.ToString());
			}
		}

		public bool SetupBinaryClient()
		{
			ServerOnline = false;
			_clientBinaryHTTP = null;
			ImportFolders.Clear();

			if (!SettingsAreValid()) return false;

			try
			{
				AnimePluginSettings settings = new AnimePluginSettings();
				string url = string.Format(@"http://{0}:{1}/JMMServerBinary", settings.JMMServer_Address, settings.JMMServer_Port);
				BaseConfig.MyAnimeLog.Write("JMM Server URL: " + url);

				BinaryMessageEncodingBindingElement encoding = new BinaryMessageEncodingBindingElement();
				encoding.ReaderQuotas.MaxArrayLength = int.MaxValue;
				encoding.ReaderQuotas.MaxBytesPerRead = int.MaxValue;
				encoding.ReaderQuotas.MaxDepth = int.MaxValue;
				encoding.ReaderQuotas.MaxNameTableCharCount = int.MaxValue;
				encoding.ReaderQuotas.MaxStringContentLength = int.MaxValue;

				HttpTransportBindingElement transport = new HttpTransportBindingElement();
				transport.MaxReceivedMessageSize = int.MaxValue;
				transport.MaxBufferPoolSize = int.MaxValue;
				transport.MaxBufferSize = int.MaxValue;
				transport.MaxReceivedMessageSize = int.MaxValue;


				Binding binding = new CustomBinding(encoding, transport);

				binding.SendTimeout = new TimeSpan(30, 0, 30);
				binding.ReceiveTimeout = new TimeSpan(30, 0, 30);
				binding.OpenTimeout = new TimeSpan(30, 0, 30);
				binding.CloseTimeout = new TimeSpan(30, 0, 30);

				EndpointAddress endpoint = new EndpointAddress(new Uri(url));

				var factory = new ChannelFactory<JMMServerBinary.IJMMServerChannel>(binding, endpoint);
				foreach (OperationDescription op in factory.Endpoint.Contract.Operations)
				{
					var dataContractBehavior = op.Behaviors.Find<DataContractSerializerOperationBehavior>();
					if (dataContractBehavior != null)
					{
						dataContractBehavior.MaxItemsInObjectGraph = int.MaxValue;
					}
				}

				_clientBinaryHTTP = factory.CreateChannel();

				// try connecting to see if the server is responding
				JMMServerBinary.Contract_ServerStatus status = JMMServerVM.Instance.clientBinaryHTTP.GetServerStatus();
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
			JMMServerBinary.Contract_ServerSettings contract = _clientBinaryHTTP.GetServerSettings();

			// Language
			this.EpisodeTitleSource = (DataSourceType)contract.EpisodeTitleSource;
			this.SeriesDescriptionSource = (DataSourceType)contract.SeriesDescriptionSource;
			this.SeriesNameSource = (DataSourceType)contract.SeriesNameSource;

            // Trakt
            this.TraktEnabled = contract.Trakt_IsEnabled;
		    this.TraktAuthToken = contract.Trakt_AuthToken;
		}

		public void RefreshImportFolders()
		{
			ImportFolders.Clear();

			if (!ServerOnline) return;
			try
			{
				List<JMMServerBinary.Contract_ImportFolder> importFolders = new List<Contract_ImportFolder>(Instance.clientBinaryHTTP.GetImportFolders());

				foreach (JMMServerBinary.Contract_ImportFolder ifolder in importFolders)
				{
					ImportFolderVM grpNew = new ImportFolderVM(ifolder);
					ImportFolders.Add(grpNew);
				}
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write("RefreshImportFolders: " + ex.ToString());
			}

		}

		public bool AuthenticateUser(string username, string password)
		{
			JMMServerBinary.Contract_JMMUser retUser = JMMServerVM.Instance.clientBinaryHTTP.AuthenticateUser(username, password);
			if (retUser != null)
				return true;
			else
				return false;
		}

		public bool PromptUserLogin()
		{
			GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg == null)
				return true;

			//keep showing the dialog until the user closes it
			int selectedLabel = 0;
		
			dlg.Reset();
			dlg.SetHeading(Translation.SelectUser);

			if (CurrentUser != null)
			{
				string msgCurUser = string.Format(Translation.CurrentUser, CurrentUser.Username);

				dlg.Add(msgCurUser);
				dlg.Add("--------");
			}

			List<JMMUserVM> allUsers = JMMServerHelper.GetAllUsers();
			foreach (JMMUserVM user in allUsers)
				dlg.Add(user.Username);

			dlg.DoModal(GUIWindowManager.ActiveWindow);
			selectedLabel = dlg.SelectedLabel;

			if (selectedLabel < 0) return false;
			if (CurrentUser != null) selectedLabel = selectedLabel  - 2;
			JMMUserVM selUser = allUsers[selectedLabel];;

			BaseConfig.MyAnimeLog.Write("selected user label: " + selectedLabel.ToString());

			// try and auth user with a blank password

			bool authed = AuthenticateUser(selUser.Username, "");
			string password = "";
			while (!authed)
			{
				// prompt user for a password
				if (Utils.DialogText(ref password, true, GUIWindowManager.ActiveWindow))
				{
					authed = AuthenticateUser(selUser.Username, password);
					if (!authed)
					{
						Utils.DialogMsg(Translation.Error, Translation.IncorrectPasswordTryAgain);	
					}
				}
				else return false;
			}

			SetCurrentUser(selUser);

			return true;
			
		}

		public void SetCurrentUser(JMMUserVM user)
		{
			CurrentUser = user;
			Username = CurrentUser.Username;
			IsAdminUser = CurrentUser.IsAdmin == 1;
			UserAuthenticated = true;
		}
	}
}
