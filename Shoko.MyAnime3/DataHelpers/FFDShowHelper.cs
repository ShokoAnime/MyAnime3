using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using Microsoft.Win32;
using Shoko.Models.Server;
using Shoko.MyAnime3.ConfigFiles;
using Shoko.MyAnime3.ViewModel;
using Shoko.MyAnime3.ViewModel.Server;

namespace Shoko.MyAnime3.DataHelpers
{
    public class FFDShowHelper
    {
        protected static string FFDSHOW_RAW_REGISTRY_KEY = "Software\\GNU\\ffdshow_raw";

        //protected static string FFDSHOW_RAW_REGISTRY_KEY = "Software\\GNU\\ffdshow";
        protected static bool isDebug = false;

        protected static List<string> presets = new List<string>();

        public List<string> Presets
        {
            get
            {
                try
                {
                    debug("Get presets called");
                    if (presets.Count == 0)
                    {
                        // Load ffdshow presets from registry
                        debug("Load ffdshow raw presets from registry");
                        RegistryKey hkcu = Registry.CurrentUser;
                        hkcu = hkcu.OpenSubKey(FFDSHOW_RAW_REGISTRY_KEY);
                        if (hkcu == null)
                            return new List<string>();
                        presets = hkcu.GetSubKeyNames().ToList();

                        if (isDebug)
                        {
                            string strPresets = "";
                            foreach (string preset in presets)
                                strPresets += preset + ";";
                            debug("Ffdshow raw presets found from registry: " + strPresets);
                        }
                    }
                }
                catch (Exception ex)
                {
                    BaseConfig.MyAnimeLog.Write(ex.ToString());
                }

                return presets;
            }
        }

        private void debug(string message)
        {
            if (isDebug)
                BaseConfig.MyAnimeLog.Write("MyAnimePlugin3.DataHelpers.FFDShowHelper DEBUG: " + message);
        }


        public void deletePreset(List<VM_AnimeEpisode_User> episodes)
        {
            try
            {
                // delete all the ffdshow presets in these episodes
                foreach (VM_AnimeEpisode_User ep in episodes)
                {
                    List<VM_VideoDetailed> locFiles = ep.FilesForEpisode;
                    foreach (VM_VideoDetailed vid in locFiles)
                    {
                        FileFfdshowPreset preset = vid.FileFfdshowPreset;
                        if (preset != null)
                        {
                            debug("Deleting preset: " + preset);
                            VM_ShokoServer.Instance.ShokoServices.DeleteFFDPreset(vid.VideoLocalID);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write("deletePreset: " + ex);
            }
        }

        public void addPreset(List<VM_AnimeEpisode_User> episodes, string presetString)
        {
            // add all the ffdshow preset in all these episodes
            foreach (VM_AnimeEpisode_User ep in episodes)
            {
                List<VM_VideoDetailed> locFiles = ep.FilesForEpisode;
                foreach (VM_VideoDetailed fl in locFiles)
                {
                    FileFfdshowPreset preset = fl.FileFfdshowPreset;
                    if (preset == null) preset = new FileFfdshowPreset();

                    preset.Hash = fl.VideoLocal_Hash;
                    preset.FileSize = fl.VideoLocal_FileSize;
                    preset.Preset = presetString;
                    VM_ShokoServer.Instance.ShokoServices.SaveFFDPreset(preset);
                }
            }
        }

        public void loadPlayingPreset(VM_AnimeEpisode_User curEpisode, string fileName)
        {
            debug("Loading preset for episode: " + curEpisode.EpisodeName);
            List<VM_VideoDetailed> fileLocals = curEpisode.FilesForEpisode;

            FileFfdshowPreset fileFfdshowPreset = null;
            foreach (VM_VideoDetailed fileLocal in fileLocals)
                if (fileLocal.Uri.ToUpper() == fileName.ToUpper())
                {
                    fileFfdshowPreset = fileLocal.FileFfdshowPreset;
                    break;
                }

            if (fileFfdshowPreset != null && fileFfdshowPreset.Preset != null && fileFfdshowPreset.Preset != "")
            {
                debug("Found preset for episode \"" + curEpisode.EpisodeName + "\", preset name is: \"" + fileFfdshowPreset.Preset + "\"");
                debug("Episode file name is: " + fileName);
                AnimePluginSettings settings = AnimePluginSettings.Instance;
                if (settings.FfdshowNotificationsLock)
                    Thread.Sleep(settings.FfdshowNotificationsLockTime); //make sure ffdshow has time to load


                // Retrieve the ffdshow instance corresponding to the file name
                //                FFDShowAPI.FFDShowAPI ffdshowAPI = new FFDShowAPI.FFDShowAPI(fileName, FFDShowAPI.FFDShowAPI.FileNameMode.FileName);  <- currently doesn't work
                //                FFDShowAPI.FFDShowAPI ffdshowAPI = new FFDShowAPI.FFDShowAPI();  <- retrieve only the first ffdshow instance
                FFDShowAPI.FFDShowAPI ffdshowAPI = new FFDShowAPI.FFDShowAPI();

                List<FFDShowAPI.FFDShowAPI.FFDShowInstance> ffdshowInstances = FFDShowAPI.FFDShowAPI.getFFDShowInstances();
                if (ffdshowInstances == null || ffdshowInstances.Count == 0)
                {
                    BaseConfig.MyAnimeLog.Write("FFDShow Error: be sure you have \"ffdshow raw video filter\" added and check in [MediaPortal Configuration -> Videos -> Video Post Processing] and you have in your ffdshow raw configuration: - checked keyboard shortcuts - checked remote API - choosen Custom 32786");
                    return;
                }

                debug("FFDShow number of instance found: " + ffdshowInstances.Count);
                for (int i = 0; i < ffdshowInstances.Count; i++)
                    if (ffdshowInstances[i].fileName == null || ffdshowInstances[i].fileName.Trim() == "") // ffdshow raw doesn't have filename
                    {
                        debug("FFDShow instance without file name found (raw ffdshow)");
                        ffdshowAPI = new FFDShowAPI.FFDShowAPI(ffdshowInstances[i].handle);
                        break;
                    }

                bool isFFDShowActive = ffdshowAPI.checkFFDShowActive();
                if (isFFDShowActive)
                {
                    debug("FFDShow has been found (" + ffdshowAPI.FFDShowAPIRemote + " handle used)");
                    debug("FFDShow file played: " + ffdshowAPI.getFileName());
                    try
                    {
                        ffdshowAPI.ActivePreset = fileFfdshowPreset.Preset;
                        DialogInfo("Set ffdshow raw preset:", "\n\"" + fileFfdshowPreset.Preset + "\"");
                        debug("ffdshow raw preset set: " + fileFfdshowPreset.Preset);
                    }
                    catch (Exception ex)
                    {
                        debug("Error while setting ffdshow preset : " + ex.Message + "\n" + ex.StackTrace);
                    }
                }
                else
                {
                    debug("FFDShow has not been found (" + ffdshowAPI.FFDShowAPIRemote + " handle used)");
                }
            }
            else
            {
                debug("Preset not found for episode \"" + curEpisode.EpisodeName + "\"");
            }
        }

        public delegate void AsyncloadPlayingPresetCaller(VM_AnimeEpisode_User curEpisode, string fileName);


        // ASync method to avoid mediaportal video treatment sleep 
        public void loadPlayingPresetASync(VM_AnimeEpisode_User curEpisode, string fileName)
        {
            // Create an instance of the FFDShowHelper class.
            FFDShowHelper ffdshowHelper = new FFDShowHelper();
            // Create the delegate.
            AsyncloadPlayingPresetCaller caller = loadPlayingPreset;
            // Initiate the asychronous call.
            IAsyncResult result = caller.BeginInvoke(curEpisode, fileName, null, null);
            Thread.Sleep(0);
            debug("Main thread " + Thread.CurrentThread.ManagedThreadId + " does some work.");
            // Call EndInvoke to wait for the asynchronous call to complete,
            // and to retrieve the results.
            //            string returnValue = caller.EndInvoke(result);
            //            caller.EndInvoke(result);
        }


        public void DialogInfo(string title, string msg)
        {
            AnimePluginSettings settings = AnimePluginSettings.Instance;
            if (!settings.FfdshowNotificationsShow)
                return;

            GUIDialogNotify dlgInfo = (GUIDialogNotify) GUIWindowManager.GetWindow((int) GUIWindow.Window.WINDOW_DIALOG_NOTIFY);
            if (null == dlgInfo)
                return;

            dlgInfo.Reset();

            //dlgInfo.Focusable = false;

            if (settings.FfdshowNotificationsAutoClose)
                dlgInfo.TimeOut = settings.FfdshowNotificationsAutoCloseTime / 1000;
            dlgInfo.SetHeading(title);
            dlgInfo.SetText(msg);

            dlgInfo.DoModal(GUIWindowManager.ActiveWindow);
        }

        public string findSelectedPresetForMenu(List<VM_AnimeEpisode_User> episodes)
        {
            bool isShowSelectedPreset = true; // can be set to false to improve preformances, as selectedPreset will not be searched
            string selectedPreset = "";

            foreach (VM_AnimeEpisode_User ep in episodes)
                // if 1 current preset for selection, set as selected
                if (isShowSelectedPreset)
                {
                    List<VM_VideoDetailed> fileLocals = ep.FilesForEpisode;
                    foreach (VM_VideoDetailed fileLocal in fileLocals)
                    {
                        FileFfdshowPreset fileFfdshowPreset = fileLocal.FileFfdshowPreset;
                        if (fileFfdshowPreset != null && fileFfdshowPreset.Preset != null && fileFfdshowPreset.Preset.Trim() != "")
                            if (selectedPreset.ToUpper() == fileFfdshowPreset.Preset.ToUpper() || selectedPreset == "")
                            {
                                selectedPreset = fileFfdshowPreset.Preset;
                            }
                            else
                            {
                                selectedPreset = "";
                                return selectedPreset;
                            }
                    }
                }

            return selectedPreset;
        }


        // default constructor
    }
}