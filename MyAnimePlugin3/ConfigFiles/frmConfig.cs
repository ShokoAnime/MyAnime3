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

			btnImagesLocation.Click += new EventHandler(btnImagesLocation_Click);
			btnSelectLocalFolderPath.Click += new EventHandler(btnSelectLocalFolderPath_Click);

			btnUTorrentTest.Click += new EventHandler(btnUTorrentTest_Click);

			btnMoveTorrentIn.Click += new EventHandler(btnMoveTorrentIn_Click);
			btnMoveTorrentOut.Click += new EventHandler(btnMoveTorrentOut_Click);
			btnTorrentUp.Click += new EventHandler(btnTorrentUp_Click);
			btnTorrentDown.Click += new EventHandler(btnTorrentDown_Click);
			btnBakaBTTest.Click += new EventHandler(btnBakaBTTest_Click);

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
			List<string> lstLanguages = Utils.GetAllAudioSubtitleLanaguages();
			

			//add them to the combo boxes
			// audio languages
			cboAudioLanguage.Items.Clear();
			cboAudioLanguage.Items.Add("< Use File Default >");
			foreach (string lang in lstLanguages)
				cboAudioLanguage.Items.Add(lang);

			cboSubtitleLanguage.Items.Clear();
			cboSubtitleLanguage.Items.Add("< Use File Default >");
			cboSubtitleLanguage.Items.Add("< No Subtitles >");

			// subtitle languages
			foreach (string lang in lstLanguages)
				cboSubtitleLanguage.Items.Add(lang);

            LoadSettingsIntoForm();

			cboImagesLocation.Items.Clear();
			cboImagesLocation.Items.Add("Default");
			cboImagesLocation.Items.Add("Custom");
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
			ToolTip4.ToolTipTitle = "Poster Quality";
			ToolTip4.SetToolTip(udPosterQuality, "Used to adjust the quality of images shown in the coverflow and filmstrip layouts. \nSelecting a lower percentage will result in lower memory and CPU usage. Resolution at 100% is 1000 x 680");

			ToolTip ToolTip5 = new ToolTip();
			ToolTip5.IsBalloon = true;
			ToolTip5.ToolTipIcon = ToolTipIcon.Info;
			ToolTip5.ToolTipTitle = "Wide Banner Quality";
			ToolTip5.SetToolTip(udWideBannerQuality, "Used to adjust the quality of images shown in the Wide Banner layouts. \nSelecting a lower percentage will result in lower memory and CPU usage. Resolution at 100% is 758 x 140");

			ToolTip ToolTip6 = new ToolTip();
			ToolTip6.IsBalloon = true;
			ToolTip6.ToolTipIcon = ToolTipIcon.Info;
			ToolTip6.ToolTipTitle = "Singles Series Display";
			ToolTip6.SetToolTip(chkSingleSeries, "When a group only has one series, the series name will be displayed instead of the group name. This could have a performance impact in large collections");


			System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
			if (a != null)
			{
				lblVersion.Text = "Version " + Utils.GetApplicationVersion(a);
			}


			lbImportFolders.DisplayMember = "Description";

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
			dlg.Description = "Select a folder";
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
				MessageBox.Show("Success!", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}


		private bool InitJMMConnection()
		{
			//lbImportFolders.DataSource = null;
			lbImportFolders.Items.Clear();

			if (!JMMServerVM.Instance.SetupBinaryClient())
			{
				MessageBox.Show("Could not connect to JMM Server", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
				MessageBox.Show(string.Format("Connected successfully, {0} torrents in list currently", torrents.Count));
			}
			else
			{
				MessageBox.Show("Connection failed");
			}
		}

		void btnBakaBTTest_Click(object sender, EventArgs e)
		{
			try
			{
				SaveSettings();


				if (string.IsNullOrEmpty(BaseConfig.Settings.BakaBTUsername))
				{
					MessageBox.Show("Please enter a username first");
					txtBakaBTUsername.Focus();
					return;
				}

				if (string.IsNullOrEmpty(BaseConfig.Settings.BakaBTPassword))
				{
					MessageBox.Show("Please enter a password first");
					txtBakaBTPassword.Focus();
					return;
				}

				this.Cursor = Cursors.WaitCursor;

				TorrentsBakaBT bakaBT = new TorrentsBakaBT();
				BaseConfig.Settings.BakaBTCookieHeader = bakaBT.Login(BaseConfig.Settings.BakaBTUsername, BaseConfig.Settings.BakaBTPassword);

				

				if (!string.IsNullOrEmpty(BaseConfig.Settings.BakaBTCookieHeader))
				{
					this.Cursor = Cursors.Arrow;
					MessageBox.Show("Connected sucessfully", "Sucess", MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
				else
				{
					this.Cursor = Cursors.Arrow;
					MessageBox.Show("Connected FAILED", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
					lblDisplayEpsDesc.Text = "Episode Number (e.g. 13)"; break;
				case 1:
					lblDisplayEpsDesc.Text = "Episode Title (e.g Destined Meeting)"; break;
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
					lblFileSelectionVars.Text = "Group (e.g. Datte Bayo)"; break;
				case 1:
					lblFileSelectionVars.Text = "Group Short (e.g DB)"; break;
				case 2:
					lblFileSelectionVars.Text = "Audio Codec (e.g OGG Vorbis)"; break;
				case 3:
					lblFileSelectionVars.Text = "File Codec (e.g XVid)"; break;
				case 4:
					lblFileSelectionVars.Text = "File Res (e.g 1280x720)"; break;
				case 5:
					lblFileSelectionVars.Text = "File Source (e.g DVD)"; break;
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

			if (cboImagesLocation.SelectedIndex == 0) // default
				BaseConfig.Settings.ThumbsFolder = "";
			else // custom
				BaseConfig.Settings.ThumbsFolder = txtImagesLocation.Text.Trim();


			BaseConfig.Settings.WatchedPercentage = int.Parse(udWatched.Value.ToString());

			BaseConfig.Settings.ShowMissing = chkShowMissing.Checked;
			BaseConfig.Settings.ShowMissingMyGroupsOnly = chkShowMissingGroups.Checked;
			BaseConfig.Settings.HideWatchedFiles = chkHideWatchedFiles.Checked;
			BaseConfig.Settings.DisplayRatingDialogOnCompletion = chkRateSeries.Checked;
			BaseConfig.Settings.SingleSeriesGroups = chkSingleSeries.Checked;

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

			chkTorrentPreferOwnGroups.Checked = BaseConfig.Settings.TorrentPreferOwnGroups;

			udWatched.Value = BaseConfig.Settings.WatchedPercentage;

			chkShowMissing.Checked = BaseConfig.Settings.ShowMissing;
			chkShowMissingGroups.Checked = BaseConfig.Settings.ShowMissingMyGroupsOnly;
			chkRateSeries.Checked = BaseConfig.Settings.DisplayRatingDialogOnCompletion;
			chkSingleSeries.Checked = BaseConfig.Settings.SingleSeriesGroups;
			

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

        }
        #endregion

        #region Tab 'Main'

		void btnImagesLocation_Click(object sender, EventArgs e)
		{
			FolderBrowserDialog dlg = new FolderBrowserDialog();
			dlg.Description = "Select a folder";
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				txtImagesLocation.Text = dlg.SelectedPath;
			}
		}

		void cboImagesLocation_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (cboImagesLocation.Text == "Default")
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
            Process.Start("http://www.otakumm.com");
        }

        private void ForumLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://www.otakumm.com/forum");
        }

        private void ManualLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("www.otakumm.com/Anime2Wiki/Manual");
        }

    }
}