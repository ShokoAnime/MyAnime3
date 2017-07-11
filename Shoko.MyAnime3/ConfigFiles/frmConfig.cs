using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;
using Shoko.Commons;
using Shoko.Commons.Extensions;
using Shoko.Models.Server;
using Shoko.MyAnime3.Extensions;
using Shoko.MyAnime3.ViewModel;

namespace Shoko.MyAnime3.ConfigFiles
{
    public partial class frmConfig : Form
    {
        private List<ImportFolder> _importFolders = new List<ImportFolder>();

        #region General

        public frmConfig()
        {
            InitializeComponent();


            label17.Text = Translation.DefaultBanner + ":";
            label22.Text = Translation.DefaultPoster + ":";
            label23.Text = Translation.DefaultFanart + ":";
            button11.Text = Translation.Save;
            label24.Text = Translation.Description + ":";
            linkLabel3.Text = Translation.GotoMAL;
            linkLabel4.Text = Translation.GotoTvDB;
            label25.Text = Translation.MALID + ":";
            label26.Text = Translation.TvDBID + ":";
            label27.Text = Translation.AniDBID + ":";
            linkLabel5.Text = Translation.GotoAniDB;
            label28.Text = Translation.GroupName + ":";
            tabPage8.Text = Translation.Groups;
            tabPage9.Text = Translation.Series;
            tabPage10.Text = Translation.Episodes;
            button1.Text = Translation.NewGroup;
            tabPage3.Text = Translation.Display;
            groupBox5.Text = Translation.InfoDelay;
            label4.Text = Translation.Milliseconds;
            label5.Text = Translation.DisplayGroupInfo;
            groupBox17.Text = Translation.Language;
            label47.Text = Translation.DefaultSubtitleLanguage;
            label48.Text = Translation.DefaultAudioLanguage;
            label92.Text = Translation.ImageQualityPercentage;
            label91.Text = Translation.ImageQualityPercentage;
            label21.Text = Translation.Posters;
            label20.Text = Translation.WideBanners;
            groupBox13.Text = Translation.Find;
            chkFindFilterItems.Text = Translation.OnlyDisplayMatching;
            label8.Text = Translation.Seconds;
            label45.Text = Translation.CloseFindPanel;
            groupBox15.Text = Translation.Other;
            chkRateSeries.Text = Translation.PromptToRate;
            chkShowMissingGroups.Text = Translation.OnlySubbingGroups;
            chkShowMissing.Text = Translation.ShowIndicatorForMissingEps;
            chkHideWatchedFiles.Text = Translation.HideFilesWatched;
            chkShowAvailableEpsOnly.Text = Translation.OnlyShowEpisodesComputer;
            chkHidePlot.Text = Translation.HidePlotUnwatched;
            label33.Text = Translation.AnEpisodeWatchedAfter + ":";
            groupBox4.Text = Translation.FileSelection;
            label16.Text = Translation.ThisIsFormattingEps;
            groupBox3.Text = Translation.EpisodeDisplay;
            tabPage1.Text = Translation.Main;
            groupBox2.Text = Translation.ImportFolders;
            btnSaveLocalFolderPath.Text = Translation.Save;
            label3.Text = Translation.LocalMapping;
            groupBox1.Text = Translation.JMMServer;
            btnTestJMMServer.Text = Translation.TestConnection;
            label2.Text = Translation.ServerPort;
            label1.Text = Translation.ServerAddress;
            lblVersion.Text = Translation.Version + " X.X.X.X";
            ManualLink.Text = "http://shokoanime.com/mediaportal/";
            label62.Text = Translation.Support;
            ForumLink.Text = "https://discordapp.com/channels/96234011612958720/101072543024160768";
            label61.Text = Translation.Discord + ":";
            WebsiteLink.Text = "http://shokoanime.com";
            label60.Text = Translation.Website + ":";
            label49.Text = Translation.PluginName;
            label46.Text = Translation.ImageLocation;
            tabPage2.Text = Translation.MoreOptions;
            groupBox16.Text = Translation.FFDShowRawPost;
            label80.Text = Translation.Milliseconds;
            chkFfdshowNotificationsLock.Text = Translation.WaitFFDShow;
            label82.Text = Translation.Milliseconds;
            chkFfdshowNotificationsAutoClose.Text = Translation.AutoCloseAfter + " ";
            chkFfdshowNotificationsShow.Text = Translation.ShowPrsetLoadNotify;
            chkLoadlocalThumbnails.Text = Translation.TryToUseLocalThumb;
            Text = Translation.Anime3Config;
            lblModeToggleKey.Text = Translation.ModeToggle;
            lblStarttextToggleKey.Text = Translation.StartTextToggle;
            chkAskBeforeStartStreamingPlayback.Text = Translation.AskBeforeStartStreamingPlayback;
            chkHomeButtonNavigation.Text = Translation.HomeButtonNavigation;

            btnImagesLocation.Click += btnImagesLocation_Click;
            btnSelectLocalFolderPath.Click += btnSelectLocalFolderPath_Click;
            // File naming
            cboFileFormat.Items.Clear();
            cboFileFormat.Items.Add(Constants.FileSelectionDisplayString.Group);
            cboFileFormat.Items.Add(Constants.FileSelectionDisplayString.GroupShort);
            cboFileFormat.Items.Add(Constants.FileSelectionDisplayString.AudioCodec);
            cboFileFormat.Items.Add(Constants.FileSelectionDisplayString.FileCodec);
            cboFileFormat.Items.Add(Constants.FileSelectionDisplayString.FileRes);
            cboFileFormat.Items.Add(Constants.FileSelectionDisplayString.FileSource);
            cboFileFormat.Items.Add(Constants.FileSelectionDisplayString.VideoBitDepth);

            cboFileFormat.SelectedIndexChanged += cboFileFormat_SelectedIndexChanged;
            cboFileFormat.SelectedIndex = 0;

            btnAddFileFormat.Click += btnAddFileFormat_Click;

            // Episode naming
            cboEpisodeFormat.Items.Clear();
            cboEpisodeFormat.Items.Add(Constants.EpisodeDisplayString.EpisodeNumber);
            cboEpisodeFormat.Items.Add(Constants.EpisodeDisplayString.EpisodeName);

            cboEpisodeFormat.SelectedIndexChanged += cboEpisodeFormat_SelectedIndexChanged;
            cboEpisodeFormat.SelectedIndex = 0;

            btnAddEpisodeFormat.Click += btnAddEpisodeFormat_Click;


            //get list of languages (sorted by name)
            List<string> lstLanguages = Utils.GetAllAudioSubtitleLanguages();


            //add them to the combo boxes
            // audio languages
            cboAudioLanguage.Items.Clear();
            cboAudioLanguage.Items.Add("< " + Translation.UseFileDefault + " >");
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
            cboImagesLocation.SelectedIndexChanged += cboImagesLocation_SelectedIndexChanged;

            if (BaseConfig.Settings.HasCustomThumbsFolder)
                cboImagesLocation.SelectedIndex = 1;
            else
                cboImagesLocation.SelectedIndex = 0;


            chkShowMissing.Click += chkShowMissing_Click;


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

            Assembly a = Assembly.GetExecutingAssembly();
            lblVersion.Text = Translation.Version + " " + Utils.GetApplicationVersion(a);


            lbImportFolders.DisplayMember = Translation.Description;

            btnTestJMMServer.Click += btnTestJMMServer_Click;
            lbImportFolders.SelectedIndexChanged += lbImportFolders_SelectedIndexChanged;
            btnSaveLocalFolderPath.Click += btnSaveLocalFolderPath_Click;

            InitJMMConnection();
        }


        void btnSaveLocalFolderPath_Click(object sender, EventArgs e)
        {
            if (lbImportFolders.SelectedItem != null)
            {
                ImportFolder fldr = _importFolders[lbImportFolders.SelectedIndex];
                if (fldr == null) return;
                FolderMappings.Instance.MapFolder(fldr.ImportFolderID, txtFolderLocalPath.Text.Trim());
                BaseConfig.Settings.Save();
            }
        }

        void btnSelectLocalFolderPath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            dlg.Description = Translation.SelectAFolder;
            if (dlg.ShowDialog() == DialogResult.OK)
                txtFolderLocalPath.Text = dlg.SelectedPath;
        }

        void lbImportFolders_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtFolderLocalPath.Text = "";
            ImportFolder fldr = _importFolders[lbImportFolders.SelectedIndex];
            if (fldr == null) return;

            txtFolderLocalPath.Text = fldr.GetLocalFileSystemFullPath();
        }

        void btnTestJMMServer_Click(object sender, EventArgs e)
        {
            SaveSettings();
            if (InitJMMConnection())
                MessageBox.Show(Translation.Sucess + "!", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }


        private bool InitJMMConnection()
        {
            //lbImportFolders.DataSource = null;
            lbImportFolders.Items.Clear();
            _importFolders.Clear();

            if (!VM_ShokoServer.Instance.SetupClient())
            {
                MessageBox.Show(Translation.CouldNotConnect, Translation.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // refresh import folders
            foreach (ImportFolder fldr in VM_ShokoServer.Instance.ImportFolders)
            {
                lbImportFolders.Items.Add(fldr.ImportFolderLocation);
                _importFolders.Add(fldr);
            }

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
            {
                chkShowMissingGroups.Enabled = true;
            }
        }

        void cboEpisodeFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (cboEpisodeFormat.SelectedIndex)
            {
                case 0:
                    lblDisplayEpsDesc.Text = Translation.EpisodeNumberEg;
                    break;
                case 1:
                    lblDisplayEpsDesc.Text = Translation.EpisodeTitleEg;
                    break;
            }
        }

        void btnAddEpisodeFormat_Click(object sender, EventArgs e)
        {
            txtFormatEp.Text += cboEpisodeFormat.SelectedItem.ToString();
        }

        void btnAddFileFormat_Click(object sender, EventArgs e)
        {
            txtFileSelection.Text += cboFileFormat.SelectedItem.ToString();
        }

        void cboFileFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (cboFileFormat.SelectedIndex)
            {
                case 0:
                    lblFileSelectionVars.Text = Translation.FileSelectionGroup;
                    break;
                case 1:
                    lblFileSelectionVars.Text = Translation.FileSelectionGroupShort;
                    break;
                case 2:
                    lblFileSelectionVars.Text = Translation.FileSelectionAudioCodec;
                    break;
                case 3:
                    lblFileSelectionVars.Text = Translation.FileSelectionFileCodec;
                    break;
                case 4:
                    lblFileSelectionVars.Text = Translation.FileSelectionFileRes;
                    break;
                case 5:
                    lblFileSelectionVars.Text = Translation.FileSelectionFileSource;
                    break;
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


            if (cboImagesLocation.SelectedIndex == 0) // default
                BaseConfig.Settings.ThumbsFolder = "";
            else // custom
                BaseConfig.Settings.ThumbsFolder = txtImagesLocation.Text.Trim();


            BaseConfig.Settings.WatchedPercentage = Convert.ToInt32(udWatched.Value);

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

            BaseConfig.Settings.FindTimeout_s = (int) nudFindTimeout.Value;
            BaseConfig.Settings.FindFilter = chkFindFilterItems.Checked;


            BaseConfig.Settings.EpisodeDisplayFormat = txtFormatEp.Text.Trim();
            BaseConfig.Settings.fileSelectionDisplayFormat = txtFileSelection.Text.Trim();

            BaseConfig.Settings.HidePlot = chkHidePlot.Checked;
            BaseConfig.Settings.ShowOnlyAvailableEpisodes = chkShowAvailableEpsOnly.Checked;


            BaseConfig.Settings.PosterSizePct = (int) udPosterQuality.Value;
            BaseConfig.Settings.BannerSizePct = (int) udWideBannerQuality.Value;
            BaseConfig.Settings.LoadLocalThumbnails = chkLoadlocalThumbnails.Checked;


            BaseConfig.Settings.InfoDelay = (int) udInfoDelay.Value;

            BaseConfig.Settings.FfdshowNotificationsShow = chkFfdshowNotificationsShow.Checked;
            BaseConfig.Settings.FfdshowNotificationsAutoClose = chkFfdshowNotificationsAutoClose.Checked;
            BaseConfig.Settings.FfdshowNotificationsLock = chkFfdshowNotificationsLock.Checked;

            int iClose;
            int.TryParse(txtFfdshowNotificationsAutoCloseTime.Text, out iClose);
            BaseConfig.Settings.FfdshowNotificationsAutoCloseTime = iClose;

            int iLock;
            int.TryParse(txtFfdshowNotificationsAutoCloseTime.Text, out iLock);
            BaseConfig.Settings.FfdshowNotificationsLockTime = iLock;
            if (tbModeToggleKey.Text.Length == 1 && tbModeToggleKey.Text != tbStarttextToggleKey.Text)
                BaseConfig.Settings.ModeToggleKey = tbModeToggleKey.Text.ToLower();
            else
                BaseConfig.Settings.ModeToggleKey = "]";

            if (tbStarttextToggleKey.Text.Length == 1 && tbStarttextToggleKey.Text != tbModeToggleKey.Text)
                BaseConfig.Settings.StartTextToggleKey = tbStarttextToggleKey.Text.ToLower();
            else
                BaseConfig.Settings.StartTextToggleKey = "[";

            BaseConfig.Settings.AskBeforeStartStreamingPlayback = chkAskBeforeStartStreamingPlayback.Checked;
            BaseConfig.Settings.HomeButtonNavigation = chkHomeButtonNavigation.Checked;
            BaseConfig.Settings.Save();
        }

        private void LoadSettingsIntoForm()
        {
            txtPluginName.Text = BaseConfig.Settings.PluginName;

            txtJMMServerAddress.Text = BaseConfig.Settings.JMMServer_Address;
            txtJMMServerPort.Text = BaseConfig.Settings.JMMServer_Port;


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

            nudFindTimeout.Value = BaseConfig.Settings.FindTimeout_s;
            chkFindFilterItems.Checked = BaseConfig.Settings.FindFilter;
            udInfoDelay.Value = BaseConfig.Settings.InfoDelay;

            txtFormatEp.Text = BaseConfig.Settings.EpisodeDisplayFormat;
            txtFileSelection.Text = BaseConfig.Settings.fileSelectionDisplayFormat;

            chkShowAvailableEpsOnly.Checked = BaseConfig.Settings.ShowOnlyAvailableEpisodes;
            chkHidePlot.Checked = BaseConfig.Settings.HidePlot;

            udPosterQuality.Value = BaseConfig.Settings.PosterSizePct;
            udWideBannerQuality.Value = BaseConfig.Settings.BannerSizePct;
            chkLoadlocalThumbnails.Checked = BaseConfig.Settings.LoadLocalThumbnails;


            chkFfdshowNotificationsShow.Checked = BaseConfig.Settings.FfdshowNotificationsShow;
            chkFfdshowNotificationsAutoClose.Checked = BaseConfig.Settings.FfdshowNotificationsAutoClose;
            chkFfdshowNotificationsLock.Checked = BaseConfig.Settings.FfdshowNotificationsLock;
            txtFfdshowNotificationsAutoCloseTime.Text = BaseConfig.Settings.FfdshowNotificationsAutoCloseTime.ToString();
            txtFfdshowNotificationsLockTime.Text = BaseConfig.Settings.FfdshowNotificationsLockTime.ToString();


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
                txtImagesLocation.Text = dlg.SelectedPath;
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
            Process.Start(" http://shokoanime.com");
        }

        private void ForumLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://discordapp.com/channels/96234011612958720/101072543024160768");
        }

        private void ManualLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://shokoanime.com/mediaportal/");
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
                tbModeToggleKey.Text = TruncateText(tbModeToggleKey.Text, 1);
        }

        private void tbStarttextToggleKey_Validating(object sender, CancelEventArgs e)
        {
            if (tbStarttextToggleKey.Text.Length > 1)
                tbStarttextToggleKey.Text = TruncateText(tbStarttextToggleKey.Text, 1);
        }

        private void tbModeToggleKey_TextChanged(object sender, EventArgs e)
        {
            if (tbModeToggleKey.Text.Length > 1)
                tbModeToggleKey.Text = TruncateText(tbModeToggleKey.Text, 1);
        }

        private void tbStarttextToggleKey_TextChanged(object sender, EventArgs e)
        {
            if (tbStarttextToggleKey.Text.Length > 1)
                tbStarttextToggleKey.Text = TruncateText(tbStarttextToggleKey.Text, 1);
        }
    }
}