using System;
using System.Collections.Generic;
using MediaPortal.GUI.Library;
using MyAnimePlugin3.DataHelpers;
using System.IO;
using System.ComponentModel;
using System.Globalization;
using MediaPortal.Dialogs;
using System.Xml;
using BinaryNorthwest;
using Action = MediaPortal.GUI.Library.Action;
using MyAnimePlugin3.ImageManagement;
using MyAnimePlugin3.ViewModel;

namespace MyAnimePlugin3.Windows
{
	public class AdminWindow : GUIWindow
	{
        [SkinControlAttribute(50)]
        protected GUIFacadeControl m_Facade = null;

        [SkinControlAttribute(3511)]
        protected GUILabelControl dummyServerStatus = null;
        [SkinControlAttribute(3512)]
        protected GUILabelControl dummyListUnlinkedFiles = null;

        [SkinControlAttribute(101)]
        protected GUIButtonControl btnServerStatus = null;
        [SkinControlAttribute(102)]
        protected GUIButtonControl btnListUnlinkedFiles = null;
        [SkinControlAttribute(6)]
        protected GUIButtonControl btnRunImport = null;
        [SkinControlAttribute(7)]
        protected GUIButtonControl btnRetryUnlinkedFiles = null;
        [SkinControlAttribute(8)]
        protected GUIButtonControl btnMoreOptions = null;
        [SkinControlAttribute(9)]
        protected GUIButtonControl btnScanDropFolder = null;
        [SkinControlAttribute(10)]
        protected GUIButtonControl btnRemoveRecords = null;
        [SkinControlAttribute(11)]
        protected GUIButtonControl btnSyncVotes = null;
        [SkinControlAttribute(12)]
        protected GUIButtonControl btnSyncMyList = null;

        public enum GuiProperty
        {
            Utilities_Status_HasherQueueCount,
            Utilities_Status_HasherQueueState,
            Utilities_Status_HasherQueueRunning,
            Utilities_Status_GeneralQueueCount,
            Utilities_Status_GeneralQueueState,
            Utilities_Status_GeneralQueueRunning,
            Utilities_Status_ImagesQueueCount,
            Utilities_Status_ImagesQueueState,
            Utilities_Status_ImagesQueueRunning,
            Utilities_UnlinkedFilesCount,
            Utilities_UnlinkedFile_Folder,
            Utilities_UnlinkedFile_FileName,
            Utilities_UnlinkedFile_Size,
            Utilities_UnlinkedFile_Hash,
            Utilities_UnlinkedFile_FileExists,
            Utilities_CurrentView
        }
        public void SetGUIProperty(GuiProperty which, string value) { this.SetGUIProperty(which.ToString(), value); }
        public void ClearGUIProperty(GuiProperty which) { this.ClearGUIProperty(which.ToString()); }

		private BackgroundWorker workerRefreshUnlinkedFiles = new BackgroundWorker();

		private bool FirstLoad = true;


		public AdminWindow()
		{
            // get ID of windowplugin belonging to this setup
            // enter your own unique code
            // ReSharper disable DoNotCallOverridableMethodsInConstructor
            GetID = Constants.WindowIDs.ADMIN;
            // ReSharper restore DoNotCallOverridableMethodsInConstructor


            workerRefreshUnlinkedFiles = new BackgroundWorker();
            workerRefreshUnlinkedFiles.WorkerReportsProgress = true;
            workerRefreshUnlinkedFiles.WorkerSupportsCancellation = true;

            workerRefreshUnlinkedFiles.DoWork += workerRefreshUnlinkedFiles_DoWork;
            workerRefreshUnlinkedFiles.RunWorkerCompleted += workerRefreshUnlinkedFiles_RunWorkerCompleted;
            workerRefreshUnlinkedFiles.ProgressChanged += workerRefreshUnlinkedFiles_ProgressChanged;


        }

		void Instance_ServerStatusEvent(Events.ServerStatusEventArgs ev)
		{
			string msg = string.Format("JMM Server Status: {0}/{1} -- {2}/{3}", ev.GeneralQueueState, ev.GeneralQueueCount, ev.HasherQueueState, ev.HasherQueueCount);
			BaseConfig.MyAnimeLog.Write(msg);

            SetGUIProperty(GuiProperty.Utilities_Status_HasherQueueCount, ev.HasherQueueCount.ToString(Globals.Culture));
            SetGUIProperty(GuiProperty.Utilities_Status_HasherQueueState, ev.HasherQueueState);
            SetGUIProperty(GuiProperty.Utilities_Status_HasherQueueRunning, ev.HasherQueueRunning ? Translation.Running : Translation.Paused);

            SetGUIProperty(GuiProperty.Utilities_Status_GeneralQueueCount, ev.GeneralQueueCount.ToString(Globals.Culture));
            SetGUIProperty(GuiProperty.Utilities_Status_GeneralQueueState, ev.GeneralQueueState);
            SetGUIProperty(GuiProperty.Utilities_Status_GeneralQueueRunning, ev.GeneralQueueRunning ? Translation.Running : Translation.Paused);

            SetGUIProperty(GuiProperty.Utilities_Status_ImagesQueueCount, ev.ImagesQueueCount.ToString(Globals.Culture));
            SetGUIProperty(GuiProperty.Utilities_Status_ImagesQueueState, ev.ImagesQueueState);
            SetGUIProperty(GuiProperty.Utilities_Status_ImagesQueueRunning, ev.ImagesQueueRunning ? Translation.Running : Translation.Paused);


        }
        private void ShowPageServerStatus()
        {
            SetGUIProperty(GuiProperty.Utilities_CurrentView, Translation.ServerStatus);
            if (dummyServerStatus != null) dummyServerStatus.Visible = true;
            if (dummyListUnlinkedFiles != null) dummyListUnlinkedFiles.Visible = false;
            m_Facade.Focus = true;
        }
        private void DialogProcess()
        {
            Utils.DialogMsg(Translation.Done, Translation.ProcessRunningOnServer);
        }
        private void ScanDropFolder()
        {
            JMMServerVM.Instance.clientBinaryHTTP.ScanDropFolders();
            m_Facade.Focus = true;
            Utils.DialogMsg(Translation.Done, Translation.FilesQueuedForProcessing);
        }
        private void RemoveMissingFiles()
        {

            JMMServerVM.Instance.clientBinaryHTTP.RemoveMissingFiles();
            m_Facade.Focus = true;
            DialogProcess();
        }
        private void SyncVotes()
        {
            JMMServerVM.Instance.clientBinaryHTTP.SyncVotes();
            m_Facade.Focus = true;
            DialogProcess();
        }
        private void SyncMyList()
        {
            JMMServerVM.Instance.clientBinaryHTTP.SyncMyList();
            m_Facade.Focus = true;
            DialogProcess();
        }

        
        protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
        {
            MainMenu menu = new MainMenu();
            menu.Add(btnServerStatus, ShowPageServerStatus);
            menu.Add(btnListUnlinkedFiles, () =>
            {
                SetGUIProperty(GuiProperty.Utilities_CurrentView, Translation.UnlinkedFiles);
                if (dummyServerStatus != null) dummyServerStatus.Visible = false;
                if (dummyListUnlinkedFiles != null) dummyListUnlinkedFiles.Visible = true;
                m_Facade.Focus = true;
                RefreshUnlinkedFiles();
            });
            menu.Add(btnRunImport, () =>
            {
                JMMServerVM.Instance.clientBinaryHTTP.RunImport();
                ShowPageServerStatus();
                DialogProcess();
            });
            menu.Add(btnRetryUnlinkedFiles, () =>
            {
                JMMServerVM.Instance.clientBinaryHTTP.RescanUnlinkedFiles();
                ShowPageServerStatus();
                DialogProcess();
            });
            menu.Add(btnRemoveRecords, RemoveMissingFiles);
            menu.Add(btnScanDropFolder, ScanDropFolder);
            menu.Add(btnSyncVotes, SyncVotes);
            menu.Add(btnSyncMyList, SyncMyList);
            menu.AddContext(btnMoreOptions, ShowMoreOptionsMenu);
            if (menu.Check(control))
                return;

            if (actionType == Action.ActionType.ACTION_SELECT_ITEM)
                OnShowContextMenu();

            base.OnClicked(controlId, control, actionType);
        }
        private ContextMenuAction ShowMoreOptionsMenu()
        {
            ContextMenu cmenu = new ContextMenu(Translation.Options);
            cmenu.AddAction(Translation.ScanDropFolder, ScanDropFolder);
            cmenu.AddAction(Translation.RemoveRecordsWithoutFile, RemoveMissingFiles);
            cmenu.AddAction(Translation.SyncVotes, SyncVotes);
            cmenu.AddAction(Translation.SyncMyList, SyncMyList);
            return cmenu.Show();
        }

      

		public override int GetID
		{
			get { return Constants.WindowIDs.ADMIN; }
			set { base.GetID = value; }
		}
        protected override void OnPageLoad()
        {
            MainWindow.PopulateVersionNumber();
            base.OnPageLoad();

            SetGUIProperty(GuiProperty.Utilities_CurrentView, Translation.UnlinkedFiles);

            if (dummyServerStatus != null) dummyServerStatus.Visible = false;
            if (dummyListUnlinkedFiles != null) dummyListUnlinkedFiles.Visible = true;

            RefreshUnlinkedFiles();

            if (FirstLoad)
            {
                JMMServerVM.Instance.ServerStatusEvent += Instance_ServerStatusEvent;
                FirstLoad = false;
            }
        }



        

		#region TvDB

		private void RefreshUnlinkedFiles()
		{
            if (!workerRefreshUnlinkedFiles.IsBusy)
            {
                if (m_Facade == null) return;
				m_Facade.Clear();

                SetGUIProperty(GuiProperty.Utilities_UnlinkedFilesCount, Translation.Loading + "...");
                ClearGUIProperty(GuiProperty.Utilities_UnlinkedFile_Folder);
                ClearGUIProperty(GuiProperty.Utilities_UnlinkedFile_FileName);
                ClearGUIProperty(GuiProperty.Utilities_UnlinkedFile_Size);
                ClearGUIProperty(GuiProperty.Utilities_UnlinkedFile_Hash);
                ClearGUIProperty(GuiProperty.Utilities_UnlinkedFile_FileExists);

                workerRefreshUnlinkedFiles.RunWorkerAsync();
            }
		}

		void workerRefreshUnlinkedFiles_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{

		}

        void workerRefreshUnlinkedFiles_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            List<GUIListItem> listItems = e.Result as List<GUIListItem>;
            if (listItems != null)
            {
                foreach (GUIListItem itm in listItems)
                {
                    m_Facade.Add(itm);
                }

                SetGUIProperty(GuiProperty.Utilities_UnlinkedFilesCount, listItems.Count.ToString(CultureInfo.CurrentCulture));
                if (listItems.Count > 0)
                {
                    m_Facade.SelectedListItemIndex = 0;
                    m_Facade.Focus = true;
                }
            }
        }

        void workerRefreshUnlinkedFiles_DoWork(object sender, DoWorkEventArgs e)
        {
            List<VideoLocalVM> unlinkedVideos = JMMServerHelper.GetUnlinkedVideos();

            List<GUIListItem> listItems = new List<GUIListItem>();

            foreach (VideoLocalVM locFile in unlinkedVideos)
            {
                string fileNameFull = Path.GetFileName(locFile.FullPath);
                string fileName = fileNameFull;
                if (!string.IsNullOrEmpty(fileNameFull) && File.Exists(fileNameFull))
                    fileName = new FileInfo(fileNameFull).Name;
                GUIListItem itm = new GUIListItem(fileName);
                itm.TVTag = locFile;
                listItems.Add(itm);
            }

            e.Result = listItems;

        }

        public override bool OnMessage(GUIMessage message)
        {
            switch (message.Message)
            {
                case GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS_CHANGED:
                    {
                        int iControl = message.SenderControlId;
                        if (iControl == m_Facade.GetID)
                        {
                            GUIListItem item = m_Facade.SelectedListItem;

                            if (item == null || item.TVTag == null) return true;

                            // unlinked files
                            if (item.TVTag.GetType() == typeof(VideoLocalVM))
                            {
                                VideoLocalVM vid = item.TVTag as VideoLocalVM;
                                if (vid != null)
                                {
                                    SetGUIProperty(GuiProperty.Utilities_UnlinkedFile_Folder, Path.GetDirectoryName(vid.FullPath));
                                    SetGUIProperty(GuiProperty.Utilities_UnlinkedFile_FileName, Path.GetFileName(vid.FullPath));
                                    SetGUIProperty(GuiProperty.Utilities_UnlinkedFile_Size, Utils.FormatFileSize(vid.FileSize));
                                    SetGUIProperty(GuiProperty.Utilities_UnlinkedFile_Hash, vid.Hash);
                                    SetGUIProperty(GuiProperty.Utilities_UnlinkedFile_FileExists, File.Exists(vid.FullPath) ? Translation.Yes : Translation.No);
                                }
                            }

                        }
                    }

                    return true;

                default:
                    return base.OnMessage(message);
            }
        }


        protected override void OnShowContextMenu()
        {
            try
            {

                GUIListItem currentitem = m_Facade.SelectedListItem;
                if (currentitem == null)
                    return;

                VideoLocalVM vid = currentitem.TVTag as VideoLocalVM;
                if (vid == null)
                    return;
                ContextMenu cmenu = new ContextMenu(Translation.FileOptions);
                cmenu.AddAction(Translation.PlayFile, () => MainWindow.vidHandler.ResumeOrPlay(vid));
                cmenu.AddAction(Translation.RehashFile, () =>
                {
                    JMMServerVM.Instance.clientBinaryHTTP.RehashFile(vid.VideoLocalID);
                    DialogProcess();
                });
                cmenu.AddAction(Translation.IgnoreFile, () =>
                {
                    JMMServerVM.Instance.clientBinaryHTTP.SetIgnoreStatusOnFile(vid.VideoLocalID, true);
                    RefreshUnlinkedFiles();
                });
                cmenu.AddAction(Translation.DeleteFileFromDisk, () =>
                {
                    if (!Utils.DialogConfirm(Translation.AreYouSureYouWantDeleteFile)) return;
                    JMMServerVM.Instance.clientBinaryHTTP.DeleteVideoLocalAndFile(vid.VideoLocalID);
                    RefreshUnlinkedFiles();
                });
                cmenu.Show();
            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write("Error in menu: {0}", ex);
            }
        }

        #endregion

        public override bool Init()
        {
            BaseConfig.MyAnimeLog.Write("INIT UTILITIES WINDOW");
            return this.InitSkin<GuiProperty>("Anime3_Admin.xml");
        }

    }
}
