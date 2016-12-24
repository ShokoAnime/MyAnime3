using System;
using System.Collections.Generic;
using System.ComponentModel;

using System.Drawing;

using System.Windows.Forms;

using System.Diagnostics;
using System.IO;
using MediaPortal.Configuration;
using MyAnimePlugin3.DataHelpers;

using MyAnimePlugin3.Downloads;
using System.Threading;
using Microsoft.Win32;
using MyAnimePlugin3.ViewModel;

namespace MyAnimePlugin3.ConfigFiles
{
    public partial class frmConfig : Form
    {
        #region Attributes
        private List<string> folders = new List<string>();

        // Timer for auto-scrolling
		private System.Windows.Forms.Timer timerDragDrop = new System.Windows.Forms.Timer();

        #endregion

		private static BackgroundWorker workerBatchLinker = new BackgroundWorker();

        #region General
        public frmConfig()
        {
            InitializeComponent();



            this.label17.Text = Translation.DefaultBanner+":";
            this.label22.Text = Translation.DefaultPoster+":";
            this.label23.Text = Translation.DefaultFanart + ":";
            this.button11.Text = Translation.Save;
            this.label24.Text = Translation.Description+":";
            this.linkLabel3.Text = Translation.GotoMAL;
            this.linkLabel4.Text = Translation.GotoTvDB;
            this.label25.Text = Translation.MALID+":";
            this.label26.Text = Translation.TvDBID+":";
            this.label27.Text = Translation.AniDBID + ":";
            this.linkLabel5.Text = Translation.GotoAniDB;
            this.label28.Text = Translation.GroupName+":";
            this.tabPage8.Text = Translation.Groups;
            this.tabPage9.Text = Translation.Series;
            this.tabPage10.Text = Translation.Episodes;
            this.button1.Text = Translation.NewGroup;
            this.tabPage11.Text = Translation.Downloads;
            this.groupBox7.Text = Translation.AnimeBytesCredentials;
            this.btnAnimeBytesTest.Text = Translation.TestLogin;
            this.label9.Text = Translation.Password;
            this.label10.Text = Translation.Username;
            this.groupBox6.Text = Translation.BakaBTCredentials;
            this.btnBakaBTTest.Text = Translation.TestLogin;
            this.label6.Text = Translation.Password;
            this.label7.Text = Translation.Username;
            this.groupBox19.Text = Translation.TorrentSources;
            this.chkTorrentPreferOwnGroups.Text = Translation.PreferTheReleaseGroups;
            this.label59.Text = Translation.Order;
            this.label58.Text = Translation.Enabled;
            this.label57.Text = Translation.Disabled;
            this.btnTorrentDown.Text = Translation.Down;
            this.btnTorrentUp.Text = Translation.Up;
            this.groupBox18.Text = Translation.uTorrent;
            this.btnUTorrentTest.Text = Translation.TestConnection;
            this.label50.Text = Translation.Password;
            this.label51.Text = Translation.Username;
            this.label53.Text = Translation.Port;
            this.label54.Text = Translation.IPAddress;
            this.tabPage3.Text = Translation.Display;
            this.groupBox5.Text = Translation.InfoDelay;
            this.label4.Text = Translation.Milliseconds;
            this.label5.Text = Translation.DisplayGroupInfo;
            this.groupBox17.Text = Translation.Language;
            this.label47.Text = Translation.DefaultSubtitleLanguage;
            this.label48.Text = Translation.DefaultAudioLanguage;
            this.label92.Text = Translation.ImageQualityPercentage;
            this.label91.Text = Translation.ImageQualityPercentage;
            this.label21.Text = Translation.Posters;
            this.label20.Text = Translation.WideBanners;
            this.groupBox13.Text = Translation.Find;
            this.chkFindFilterItems.Text = Translation.OnlyDisplayMatching;
            this.label8.Text = Translation.Seconds;
            this.label45.Text = Translation.CloseFindPanel;
            this.groupBox15.Text = Translation.Other;
            this.chkRateSeries.Text = Translation.PromptToRate;
            this.chkShowMissingGroups.Text = Translation.OnlySubbingGroups;
            this.chkShowMissing.Text = Translation.ShowIndicatorForMissingEps;
            this.chkHideWatchedFiles.Text = Translation.HideFilesWatched;
            this.chkShowAvailableEpsOnly.Text = Translation.OnlyShowEpisodesComputer;
            this.chkHidePlot.Text = Translation.HidePlotUnwatched;
            this.label33.Text = Translation.AnEpisodeWatchedAfter+":";
            this.groupBox4.Text = Translation.FileSelection;
            this.label16.Text = Translation.ThisIsFormattingEps;
            this.groupBox3.Text = Translation.EpisodeDisplay;
            this.tabPage1.Text = Translation.Main;
            this.groupBox2.Text = Translation.ImportFolders;
            this.btnSaveLocalFolderPath.Text = Translation.Save;
            this.label3.Text = Translation.LocalMapping;
            this.groupBox1.Text = Translation.JMMServer;
            this.btnTestJMMServer.Text = Translation.TestConnection;
            this.label2.Text = Translation.ServerPort;
            this.label1.Text = Translation.ServerAddress;
            this.lblVersion.Text = Translation.Version+" X.X.X.X";
            this.ManualLink.Text = "http://jmediamanager.org/myanime3";
            this.label62.Text = Translation.Support;
            this.ForumLink.Text = "https://discordapp.com/channels/96234011612958720/101072543024160768";
            this.label61.Text = Translation.Discord+":";
            this.WebsiteLink.Text = "http://jmediamanager.org";
            this.label60.Text = Translation.Website+":";
            this.label49.Text = Translation.PluginName;
            this.label46.Text = Translation.ImageLocation;
            this.tabPage2.Text = Translation.MoreOptions;
            this.groupBox16.Text = Translation.FFDShowRawPost;
            this.label80.Text = Translation.Milliseconds;
            this.chkFfdshowNotificationsLock.Text = Translation.WaitFFDShow;
            this.label82.Text = Translation.Milliseconds;
            this.chkFfdshowNotificationsAutoClose.Text = Translation.AutoCloseAfter+" ";
            this.chkFfdshowNotificationsShow.Text = Translation.ShowPrsetLoadNotify;
            this.chkLoadlocalThumbnails.Text = Translation.TryToUseLocalThumb;
            this.Text = Translation.Anime3Config;
            this.lblModeToggleKey.Text = Translation.ModeToggle;
            this.lblStarttextToggleKey.Text = Translation.StartTextToggle;
            this.chkAskBeforeStartStreamingPlayback.Text = Translation.AskBeforeStartStreamingPlayback;
            this.chkHomeButtonNavigation.Text = Translation.HomeButtonNavigation;

            btnImagesLocation.Click += new EventHandler(btnImagesLocation_Click);
			btnSelectLocalFolderPath.Click += new EventHandler(btnSelectLocalFolderPath_Click);

			btnUTorrentTest.Click += new EventHandler(btnUTorrentTest_Click);

			btnMoveTorrentIn.Click += new EventHandler(btnMoveTorrentIn_Click);
			btnMoveTorrentOut.Click += new EventHandler(btnMoveTorrentOut_Click);
			btnTorrentUp.Click += new EventHandler(btnTorrentUp_Click);
			btnTorrentDown.Click += new EventHandler(btnTorrentDown_Click);
			btnBakaBTTest.Click += new EventHandler(btnBakaBTTest_Click);
			btnAnimeBytesTest.Click += new EventHandler(btnAnimeBytesTest_Click);

			// File naming
			cboFileFormat.Items.Clear();
			cboFileFormat.Items.Add(Constants.FileSelectionDisplayString.Group);
			cboFileFormat.Items.Add(Constants.FileSelectionDisplayString.GroupShort);
			cboFileFormat.Items.Add(Constants.FileSelectionDisplayString.AudioCodec);
			cboFileFormat.Items.Add(Constants.FileSelectionDisplayString.FileCodec);
			cboFileFormat.Items.Add(Constants.FileSelectionDisplayString.FileRes);
			cboFileFormat.Items.Add(Constants.FileSelectionDisplayString.FileSource);
			cboFileFormat.Items.Add(Constants.FileSelectionDisplayString.VideoBitDepth);

			cboFileFormat.SelectedIndexChanged += new EventHandler(cboFileFormat_SelectedIndexChanged);
			cboFileFormat.SelectedIndex = 0;

			btnAddFileFormat.Click += new EventHandler(btnAddFileFormat_Click);

			// Episode naming
			cboEpisodeFormat.Items.Clear();
			cboEpisodeFormat.Items.Add(Constants.EpisodeDisplayString.EpisodeNumber);
			cboEpisodeFormat.Items.Add(Constants.EpisodeDisplayString.EpisodeName);

			cboEpisodeFormat.SelectedIndexChanged += new EventHandler(cboEpisodeFormat_SelectedIndexChanged);
			cboEpisodeFormat.SelectedIndex = 0;

			btnAddEpisodeFormat.Click += new EventHandler(btnAddEpisodeFormat_Click);


			//get list of languages (sorted by name)
			List<string> lstLanguages = Utils.GetAllAudioSubtitleLanguages();
			

			//add them to the combo boxes
			// audio languages
			cboAudioLanguage.Items.Clear();
			cboAudioLanguage.Items.Add("< "+Translation.UseFileDefault+" >");
			foreach (string lang in lstLanguages)
				cboAudioLanguage.Items.Add(lang);

			cboSubtitleLanguage.Items.Clear();
			cboSubtitleLanguage.Items.Add("< " + Translation.UseFileDefault + " >");
			cboSubtitleLanguage.Items.Add("< " + Translation.NoSubtitles + " >");

			// subtitle languages
			foreach (string lang in lstLanguages)
				cboSubtitleLanguage.Items.Add(lang);

            LoadSettingsIntoForm();

			cboImagesLocation.Items.Clear();
			cboImagesLocation.Items.Add(Translation.Default);
			cboImagesLocation.Items.Add(Translation.Custom);
			cboImagesLocation.SelectedIndexChanged += new EventHandler(cboImagesLocation_SelectedIndexChanged);

			if (BaseConfig.Settings.HasCustomThumbsFolder)
				cboImagesLocation.SelectedIndex = 1;
			else
				cboImagesLocation.SelectedIndex = 0;


			chkShowMissing.Click += new EventHandler(chkShowMissing_Click);
            


			lblDisplayEpsDesc.Visible = true;



            
			ToolTip ToolTip4 = new ToolTip();
			ToolTip4.IsBalloon = true;
			ToolTip4.ToolTipIcon = ToolTipIcon.Info;
			ToolTip4.ToolTipTitle = Translation.PosterQuality;
            ToolTip4.SetToolTip(udPosterQuality, Translation.PosterQualityToolTip);
            
			ToolTip ToolTip5 = new ToolTip();
			ToolTip5.IsBalloon = true;
			ToolTip5.ToolTipIcon = ToolTipIcon.Info;
            ToolTip5.ToolTipTitle = Translation.WideBannerQuality;
			ToolTip5.SetToolTip(udWideBannerQuality, Translation.WideBannerQualityToolTip);

			System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
			if (a != null)
			{
				lblVersion.Text = Translation.Version + " " + Utils.GetApplicationVersion(a);
			}


			lbImportFolders.DisplayMember = Translation.Description;

			btnTestJMMServer.Click += new EventHandler(btnTestJMMServer_Click);
			lbImportFolders.SelectedIndexChanged += new EventHandler(lbImportFolders_SelectedIndexChanged);
			btnSaveLocalFolderPath.Click += new EventHandler(btnSaveLocalFolderPath_Click);

			InitJMMConnection();
        }

		

		

		void btnSaveLocalFolderPath_Click(object sender, EventArgs e)
		{
			ImportFolderVM fldr = lbImportFolders.SelectedItem as ImportFolderVM;
			if (fldr == null) return;

			BaseConfig.Settings.SetImportFolderMapping(fldr.ImportFolderID.Value, txtFolderLocalPath.Text.Trim());
			BaseConfig.Settings.Save();
		}

		void btnSelectLocalFolderPath_Click(object sender, EventArgs e)
		{
			FolderBrowserDialog dlg = new FolderBrowserDialog();
		    dlg.Description = Translation.SelectAFolder;
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				txtFolderLocalPath.Text = dlg.SelectedPath;
			}
		}

		void lbImportFolders_SelectedIndexChanged(object sender, EventArgs e)
		{
			txtFolderLocalPath.Text = "";

			ImportFolderVM fldr = lbImportFolders.SelectedItem as ImportFolderVM;
			if (fldr == null) return;

			txtFolderLocalPath.Text = fldr.LocalPath;
		}

		void btnTestJMMServer_Click(object sender, EventArgs e)
		{
			SaveSettings();
			if (InitJMMConnection())
				MessageBox.Show(Translation.Sucess+ "!", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}


		private bool InitJMMConnection()
		{
			//lbImportFolders.DataSource = null;
			lbImportFolders.Items.Clear();

			if (!JMMServerVM.Instance.SetupBinaryClient())
			{
				MessageBox.Show(Translation.CouldNotConnect, Translation.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}

			// refresh import folders
			foreach (ImportFolderVM fldr in JMMServerVM.Instance.ImportFolders)
				lbImportFolders.Items.Add(fldr);

			return true;
		}
		

		


		void chkShowMissing_Click(object sender, EventArgs e)
		{
			if (!chkShowMissing.Checked)
			{
				chkShowMissingGroups.Checked = false;
				chkShowMissingGroups.Enabled = false;
			}
			else
				chkShowMissingGroups.Enabled = true;
		}

		void btnTorrentDown_Click(object sender, EventArgs e)
		{
			if (lstTorrentIn.SelectedItem == null) return;
			if (lstTorrentIn.SelectedIndex == lstTorrentIn.Items.Count - 1) return; // already at bottom

			string src = lstTorrentIn.SelectedItem.ToString();
			int newPos = lstTorrentIn.SelectedIndex + 1;
			lstTorrentIn.Items.RemoveAt(lstTorrentIn.SelectedIndex);
			lstTorrentIn.Items.Insert(newPos, src);
			lstTorrentIn.SelectedIndex = newPos;
		}

		void btnTorrentUp_Click(object sender, EventArgs e)
		{
			if (lstTorrentIn.SelectedItem == null) return;
			if (lstTorrentIn.SelectedIndex == 0) return; // already at top

			string src = lstTorrentIn.SelectedItem.ToString();
			int newPos = lstTorrentIn.SelectedIndex - 1;
			lstTorrentIn.Items.RemoveAt(lstTorrentIn.SelectedIndex);
			lstTorrentIn.Items.Insert(newPos, src);
			lstTorrentIn.SelectedIndex = newPos;
		}

		void btnMoveTorrentOut_Click(object sender, EventArgs e)
		{
			if (lstTorrentIn.SelectedItem == null) return;

			string src = lstTorrentIn.SelectedItem.ToString();
			lstTorrentOut.Items.Add(src);
			lstTorrentIn.Items.RemoveAt(lstTorrentIn.SelectedIndex);
		}

		void btnMoveTorrentIn_Click(object sender, EventArgs e)
		{
			if (lstTorrentOut.SelectedItem == null) return;

			string src = lstTorrentOut.SelectedItem.ToString();
			lstTorrentIn.Items.Add(src);
			lstTorrentOut.Items.RemoveAt(lstTorrentOut.SelectedIndex);
		}

		void btnUTorrentTest_Click(object sender, EventArgs e)
		{
			SaveSettings();

			List<Torrent> torrents = new List<Torrent>();
			UTorrentHelper uTorrent = new UTorrentHelper();
			uTorrent.Init();
			if (uTorrent.GetTorrentList(ref torrents))
			{
				MessageBox.Show(string.Format(Translation.ConnectedTorrents, torrents.Count));
			}
			else
			{
				MessageBox.Show(Translation.ConnectionFailed);
			}
		}

		void btnAnimeBytesTest_Click(object sender, EventArgs e)
		{
			try
			{
				SaveSettings();


				if (string.IsNullOrEmpty(BaseConfig.Settings.AnimeBytesUsername))
				{
					MessageBox.Show(Translation.PleaseUsernameFirst);
					txtAnimeBytesUsername.Focus();
					return;
				}

				if (string.IsNullOrEmpty(BaseConfig.Settings.AnimeBytesPassword))
				{
					MessageBox.Show(Translation.PleasePasswordFirst);
					txtAnimeBytesPassword.Focus();
					return;
				}

				this.Cursor = Cursors.WaitCursor;

				TorrentsAnimeBytes AnimeBytes = new TorrentsAnimeBytes();
				BaseConfig.Settings.AnimeBytesCookieHeader = AnimeBytes.Login(BaseConfig.Settings.AnimeBytesUsername, BaseConfig.Settings.AnimeBytesPassword);



				if (!string.IsNullOrEmpty(BaseConfig.Settings.AnimeBytesCookieHeader))
				{
					this.Cursor = Cursors.Arrow;
					MessageBox.Show(Translation.ConnectedSucess, Translation.Sucess, MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
				else
				{
					this.Cursor = Cursors.Arrow;
					MessageBox.Show(Translation.ConnectedFailed, Translation.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
					txtAnimeBytesUsername.Focus();
					return;
				}

			}
			catch (Exception ex)
			{
				this.Cursor = Cursors.Arrow;
				MessageBox.Show(ex.Message);
			}
		}

		void btnBakaBTTest_Click(object sender, EventArgs e)
		{
			try
			{
				SaveSettings();


				if (string.IsNullOrEmpty(BaseConfig.Settings.BakaBTUsername))
				{
					MessageBox.Show(Translation.PleaseUsernameFirst);
					txtBakaBTUsername.Focus();
					return;
				}

				if (string.IsNullOrEmpty(BaseConfig.Settings.BakaBTPassword))
				{
					MessageBox.Show(Translation.PleasePasswordFirst);
					txtBakaBTPassword.Focus();
					return;
				}

				this.Cursor = Cursors.WaitCursor;

				TorrentsBakaBT bakaBT = new TorrentsBakaBT();
				BaseConfig.Settings.BakaBTCookieHeader = bakaBT.Login(BaseConfig.Settings.BakaBTUsername, BaseConfig.Settings.BakaBTPassword);

				

				if (!string.IsNullOrEmpty(BaseConfig.Settings.BakaBTCookieHeader))
				{
					this.Cursor = Cursors.Arrow;
					MessageBox.Show(Translation.ConnectedSucess, Translation.Sucess, MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
				else
				{
					this.Cursor = Cursors.Arrow;
					MessageBox.Show(Translation.ConnectedFailed, Translation.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
					txtBakaBTUsername.Focus();
					return;
				}

			}
			catch (Exception ex)
			{
				this.Cursor = Cursors.Arrow;
				MessageBox.Show(ex.Message);
			}
		}

		void cboEpisodeFormat_SelectedIndexChanged(object sender, EventArgs e)
		{
			switch (cboEpisodeFormat.SelectedIndex)
			{
				case 0:
					lblDisplayEpsDesc.Text = Translation.EpisodeNumberEg; break;
				case 1:
			        lblDisplayEpsDesc.Text = Translation.EpisodeTitleEg; break;
			}
		}

		void btnAddEpisodeFormat_Click(object sender, EventArgs e)
		{
			txtFormatEp.Text += cboEpisodeFormat.SelectedItem.ToString();
		}

		void btnAddFileFormat_Click(object sender, EventArgs e)
		{
			txtFileSelection.Text += cboFileFormat.SelectedItem.ToString(); ;
		}

		void cboFileFormat_SelectedIndexChanged(object sender, EventArgs e)
		{
			switch (cboFileFormat.SelectedIndex)
			{
				case 0:
					lblFileSelectionVars.Text = Translation.FileSelectionGroup; break;
				case 1:
					lblFileSelectionVars.Text = Translation.FileSelectionGroupShort; break;
				case 2:
					lblFileSelectionVars.Text = Translation.FileSelectionAudioCodec; break;
				case 3:
			        lblFileSelectionVars.Text = Translation.FileSelectionFileCodec; break;
				case 4:
					lblFileSelectionVars.Text = Translation.FileSelectionFileRes; break;
				case 5:
					lblFileSelectionVars.Text = Translation.FileSelectionFileSource; break;
			}
		}

        protected override void OnClosing(CancelEventArgs e)
        {
            SaveSettings();

            base.OnClosing(e);
        }

        private void SaveSettings()
        {
			BaseConfig.Settings.PluginName = txtPluginName.Text.Trim();

			BaseConfig.Settings.JMMServer_Address = txtJMMServerAddress.Text.Trim();
			BaseConfig.Settings.JMMServer_Port = txtJMMServerPort.Text.Trim();

			BaseConfig.Settings.UTorrentAddress = txtUTorrentAddress.Text.Trim();
			BaseConfig.Settings.UTorrentPassword = txtUTorrentPassword.Text.Trim();
			BaseConfig.Settings.UTorrentPort = txtUTorrentPort.Text.Trim();
			BaseConfig.Settings.UTorrentUsername = txtUTorrentUsername.Text.Trim();

			BaseConfig.Settings.TorrentPreferOwnGroups = chkTorrentPreferOwnGroups.Checked;

			
			BaseConfig.Settings.BakaBTUsername = txtBakaBTUsername.Text.Trim();
			BaseConfig.Settings.BakaBTPassword = txtBakaBTPassword.Text.Trim();

			BaseConfig.Settings.AnimeBytesUsername = txtAnimeBytesUsername.Text.Trim();
			BaseConfig.Settings.AnimeBytesPassword = txtAnimeBytesPassword.Text.Trim(); 

			if (cboImagesLocation.SelectedIndex == 0) // default
				BaseConfig.Settings.ThumbsFolder = "";
			else // custom
				BaseConfig.Settings.ThumbsFolder = txtImagesLocation.Text.Trim();


			BaseConfig.Settings.WatchedPercentage = int.Parse(udWatched.Value.ToString());

			BaseConfig.Settings.ShowMissing = chkShowMissing.Checked;
			BaseConfig.Settings.ShowMissingMyGroupsOnly = chkShowMissingGroups.Checked;
			BaseConfig.Settings.HideWatchedFiles = chkHideWatchedFiles.Checked;
			BaseConfig.Settings.DisplayRatingDialogOnCompletion = chkRateSeries.Checked;

			if (cboAudioLanguage.SelectedIndex == 0)
				BaseConfig.Settings.DefaultAudioLanguage = "<file>";
			else
				BaseConfig.Settings.DefaultAudioLanguage = cboAudioLanguage.SelectedItem.ToString();

			if (cboSubtitleLanguage.SelectedIndex == 0)
				BaseConfig.Settings.DefaultSubtitleLanguage = "<file>";
			else if (cboSubtitleLanguage.SelectedIndex == 1)
				BaseConfig.Settings.DefaultSubtitleLanguage = "<none>";
			else
				BaseConfig.Settings.DefaultSubtitleLanguage = cboSubtitleLanguage.SelectedItem.ToString();

			BaseConfig.Settings.FindTimeout_s = (int)nudFindTimeout.Value;
			BaseConfig.Settings.FindFilter = chkFindFilterItems.Checked;


			BaseConfig.Settings.EpisodeDisplayFormat = txtFormatEp.Text.Trim();
			BaseConfig.Settings.fileSelectionDisplayFormat = txtFileSelection.Text.Trim();

			BaseConfig.Settings.HidePlot = chkHidePlot.Checked;
			BaseConfig.Settings.ShowOnlyAvailableEpisodes = chkShowAvailableEpsOnly.Checked;


			BaseConfig.Settings.PosterSizePct = (int)udPosterQuality.Value;
			BaseConfig.Settings.BannerSizePct = (int)udWideBannerQuality.Value;
			BaseConfig.Settings.LoadLocalThumbnails = chkLoadlocalThumbnails.Checked;


			BaseConfig.Settings.InfoDelay = (int)udInfoDelay.Value;

			BaseConfig.Settings.FfdshowNotificationsShow = chkFfdshowNotificationsShow.Checked;
			BaseConfig.Settings.FfdshowNotificationsAutoClose = chkFfdshowNotificationsAutoClose.Checked;
			BaseConfig.Settings.FfdshowNotificationsLock = chkFfdshowNotificationsLock.Checked;

			int iClose = 0;
			int.TryParse(txtFfdshowNotificationsAutoCloseTime.Text, out iClose);
			BaseConfig.Settings.FfdshowNotificationsAutoCloseTime = iClose;

			int iLock = 0;
			int.TryParse(txtFfdshowNotificationsAutoCloseTime.Text, out iLock);
			BaseConfig.Settings.FfdshowNotificationsLockTime = iLock;

			BaseConfig.Settings.TorrentSources.Clear();
			foreach (object srco in lstTorrentIn.Items)
			{
				string src = srco as string;
				BaseConfig.Settings.TorrentSources.Add(src);
			}


            if (tbModeToggleKey.Text.Length == 1 && tbModeToggleKey.Text != tbStarttextToggleKey.Text)
            {
                BaseConfig.Settings.ModeToggleKey = tbModeToggleKey.Text.ToLower();
            }
            else
            {
                BaseConfig.Settings.ModeToggleKey = "]";
            }

            if (tbStarttextToggleKey.Text.Length == 1 && tbStarttextToggleKey.Text != tbModeToggleKey.Text)
            {
                BaseConfig.Settings.StartTextToggleKey = tbStarttextToggleKey.Text.ToLower();
            }
            else
            {
                BaseConfig.Settings.StartTextToggleKey = "[";
            }

            BaseConfig.Settings.AskBeforeStartStreamingPlayback = chkAskBeforeStartStreamingPlayback.Checked;
            BaseConfig.Settings.HomeButtonNavigation = chkHomeButtonNavigation.Checked;
            BaseConfig.Settings.Save();


        }
     
        private void LoadSettingsIntoForm()
        {

			txtPluginName.Text = BaseConfig.Settings.PluginName;

			txtJMMServerAddress.Text = BaseConfig.Settings.JMMServer_Address;
			txtJMMServerPort.Text = BaseConfig.Settings.JMMServer_Port;

			txtUTorrentAddress.Text = BaseConfig.Settings.UTorrentAddress;
			txtUTorrentPassword.Text = BaseConfig.Settings.UTorrentPassword;
			txtUTorrentPort.Text = BaseConfig.Settings.UTorrentPort;
			txtUTorrentUsername.Text = BaseConfig.Settings.UTorrentUsername;

			txtBakaBTUsername.Text = BaseConfig.Settings.BakaBTUsername;
			txtBakaBTPassword.Text = BaseConfig.Settings.BakaBTPassword;

			txtAnimeBytesUsername.Text = BaseConfig.Settings.AnimeBytesUsername;
			txtAnimeBytesPassword.Text = BaseConfig.Settings.AnimeBytesPassword; 

			chkTorrentPreferOwnGroups.Checked = BaseConfig.Settings.TorrentPreferOwnGroups;

			udWatched.Value = BaseConfig.Settings.WatchedPercentage;

			chkShowMissing.Checked = BaseConfig.Settings.ShowMissing;
			chkShowMissingGroups.Checked = BaseConfig.Settings.ShowMissingMyGroupsOnly;
			chkRateSeries.Checked = BaseConfig.Settings.DisplayRatingDialogOnCompletion;			

			if (!BaseConfig.Settings.ShowMissing) chkShowMissingGroups.Enabled = false;

			chkHideWatchedFiles.Checked = BaseConfig.Settings.HideWatchedFiles;

			int index = cboAudioLanguage.FindStringExact(BaseConfig.Settings.DefaultAudioLanguage);
			if (index > 0)
				cboAudioLanguage.SelectedIndex = index;
			else
				cboAudioLanguage.SelectedIndex = 0;
			index = cboSubtitleLanguage.FindStringExact(BaseConfig.Settings.DefaultSubtitleLanguage);
			if (index > 1)
				cboSubtitleLanguage.SelectedIndex = index;
			else if (BaseConfig.Settings.DefaultSubtitleLanguage == "<none>")
				cboSubtitleLanguage.SelectedIndex = 1;
			else
				cboSubtitleLanguage.SelectedIndex = 0;

			nudFindTimeout.Value = (decimal)BaseConfig.Settings.FindTimeout_s;
			chkFindFilterItems.Checked = BaseConfig.Settings.FindFilter;
			udInfoDelay.Value = (decimal)BaseConfig.Settings.InfoDelay;

			txtFormatEp.Text = BaseConfig.Settings.EpisodeDisplayFormat;
			txtFileSelection.Text = BaseConfig.Settings.fileSelectionDisplayFormat;

			chkShowAvailableEpsOnly.Checked = BaseConfig.Settings.ShowOnlyAvailableEpisodes;
			chkHidePlot.Checked = BaseConfig.Settings.HidePlot;

			udPosterQuality.Value = (decimal)BaseConfig.Settings.PosterSizePct;
			udWideBannerQuality.Value = (decimal)BaseConfig.Settings.BannerSizePct;
			chkLoadlocalThumbnails.Checked = BaseConfig.Settings.LoadLocalThumbnails;


			chkFfdshowNotificationsShow.Checked = BaseConfig.Settings.FfdshowNotificationsShow;
			chkFfdshowNotificationsAutoClose.Checked = BaseConfig.Settings.FfdshowNotificationsAutoClose;
			chkFfdshowNotificationsLock.Checked = BaseConfig.Settings.FfdshowNotificationsLock;
			txtFfdshowNotificationsAutoCloseTime.Text = BaseConfig.Settings.FfdshowNotificationsAutoCloseTime.ToString();
			txtFfdshowNotificationsLockTime.Text = BaseConfig.Settings.FfdshowNotificationsLockTime.ToString();


			// get a full list of torrent sources
			string[] allSources = AnimePluginSettings.TorrentSourcesAll.Split(';');
			foreach (string src in BaseConfig.Settings.TorrentSources)
			{
				lstTorrentIn.Items.Add(src);
			}
			foreach (string src in allSources)
			{
				if (!BaseConfig.Settings.TorrentSources.Contains(src))
					lstTorrentOut.Items.Add(src);
			}


            tbModeToggleKey.Text = BaseConfig.Settings.ModeToggleKey;
            tbStarttextToggleKey.Text = BaseConfig.Settings.StartTextToggleKey;
            chkAskBeforeStartStreamingPlayback.Checked = BaseConfig.Settings.AskBeforeStartStreamingPlayback;
            chkHomeButtonNavigation.Checked = BaseConfig.Settings.HomeButtonNavigation;
        }
        #endregion

        #region Tab 'Main'

        void btnImagesLocation_Click(object sender, EventArgs e)
		{
			FolderBrowserDialog dlg = new FolderBrowserDialog();
		    dlg.Description = Translation.SelectAFolder;
            if (dlg.ShowDialog() == DialogResult.OK)
			{
				txtImagesLocation.Text = dlg.SelectedPath;
			}
		}

		void cboImagesLocation_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (cboImagesLocation.Text == Translation.Default)
			{
				btnImagesLocation.Enabled = false;
				txtImagesLocation.Enabled = false;
				BaseConfig.Settings.ThumbsFolder = "";
			}
			else
			{
				btnImagesLocation.Enabled = true;
				txtImagesLocation.Enabled = true;
				//settings.ThumbsFolder = txtImagesLocation.Text.Trim();
			}
			txtImagesLocation.Text = BaseConfig.Settings.ThumbsFolder;
		}

        #endregion


 

        private void WebsiteLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(" http://jmediamanager.org");
        }

        private void ForumLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://discordapp.com/channels/96234011612958720/101072543024160768");
        }

        private void ManualLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://jmediamanager.org/myanime3/");
        }


        public static string TruncateText(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        private void btnResetModeText_Click(object sender, EventArgs e)
        {
            tbModeToggleKey.Text = @"]";
        }

        private void btnClearStartText_Click(object sender, EventArgs e)
        {
            tbStarttextToggleKey.Text = @"[";
        }

        private void tbModeToggleKey_Validating(object sender, CancelEventArgs e)
        {
            if (tbModeToggleKey.Text.Length > 1)
            {
                tbModeToggleKey.Text =  TruncateText(tbModeToggleKey.Text, 1);
            }
        }

        private void tbStarttextToggleKey_Validating(object sender, CancelEventArgs e)
        {
            if (tbStarttextToggleKey.Text.Length > 1)
            {
                tbStarttextToggleKey.Text =  TruncateText(tbStarttextToggleKey.Text, 1);
            }
        }

        private void tbModeToggleKey_TextChanged(object sender, EventArgs e)
        {
            if (tbModeToggleKey.Text.Length > 1)
            {
                tbModeToggleKey.Text = TruncateText(tbModeToggleKey.Text, 1);
            }
        }

        private void tbStarttextToggleKey_TextChanged(object sender, EventArgs e)
        {
            if (tbStarttextToggleKey.Text.Length > 1)
            {
                tbStarttextToggleKey.Text = TruncateText(tbStarttextToggleKey.Text, 1);
            }
        }


    }
}