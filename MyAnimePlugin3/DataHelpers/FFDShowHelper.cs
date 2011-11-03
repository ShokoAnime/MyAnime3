using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using FFDShowAPI;
using System.Threading;
using MyAnimePlugin3.ViewModel;

namespace MyAnimePlugin3.DataHelpers
{
    public class FFDShowHelper
    {
		//TODO
		/*
        protected static string FFDSHOW_RAW_REGISTRY_KEY = "Software\\GNU\\ffdshow_raw";
        protected static bool isDebug = false;

        protected static List<string> presets = new List<string>();
        public List<string> Presets
		{
			get {
                debug("Get presets called");
                if (presets.Count == 0)
                {
                    // Load ffdshow presets from registry
                    debug("Load ffdshow raw presets from registry");
                    RegistryKey hkcu = Registry.CurrentUser;
                    hkcu = hkcu.OpenSubKey(FFDSHOW_RAW_REGISTRY_KEY);
                    presets = hkcu.GetSubKeyNames().ToList();

                    if (isDebug)
                    {
                        string strPresets="";
                        foreach(string preset in presets){
                            strPresets += preset + ";";
                        }
                        debug("Ffdshow raw presets found from registry: " + strPresets);
                    }
                }
                
                return presets;
            }
		}

        private void debug(string message)
        {
            if (isDebug)
            {
                BaseConfig.MyAnimeLog.Write("MyAnimePlugin2.DataHelpers.FFDShowHelper DEBUG: " + message);
            }
        }


        public void deletePreset(List<AnimeEpisodeVM> episodes)
        {
            // delete all the ffdshow presets in these episodes
			foreach (AnimeEpisodeVM ep in episodes)
            {
                List<VideoDetailedVM> locFiles = ep.FilesForEpisode;
				foreach (VideoDetailedVM fl in locFiles)
                {

                    FileFfdshowPreset preset = fl.FileFfdshowPreset ;
                    if(preset!=null){
                        debug("Deleting preset: " + preset.ValueString);
                        preset.Delete();
                    }
                }
            }
        }
        public void addPreset(List<AnimeEpisode>episodes, string presetString)
        {
            // add all the ffdshow preset in all these episodes
            foreach (AnimeEpisode ep in episodes)
            {
                List<FileLocal> locFiles = ep.FileLocals;
                foreach (FileLocal fl in locFiles)
                {
                    FileFfdshowPreset preset = fl.FileFfdshowPreset;
                    // delete old preset
                    if (preset != null)
                    {
                        debug("Deleting preset: " + preset.ValueString);
                        preset.Delete();
                    }
                    // add new preset
                    preset = new FileFfdshowPreset();
                    preset.ED2KHash = fl.ED2KHash;
                    preset.preset = presetString;
                    debug("Adding preset: " + preset.ValueString);
                    preset.Save();
                }
            }
        }

        public void loadPlayingPreset(AnimeEpisodeVM curEpisode, string fileName)
        {
            debug("Loading preset for episode: " + curEpisode.EpisodeName);
            List<VideoDetailedVM> fileLocals = curEpisode.FilesForEpisode;
            
            FileFfdshowPreset fileFfdshowPreset = null ;
			foreach (VideoDetailedVM fileLocal in fileLocals)
            {
                if (fileLocal.FullPath.ToUpper() == fileName.ToUpper())
                {
                    fileFfdshowPreset = fileLocal.FileFfdshowPreset ;
                    break;
                }
            }

            if (fileFfdshowPreset != null && fileFfdshowPreset.preset != null && fileFfdshowPreset.preset != "")
            {
                debug("Found preset for episode \"" + curEpisode.EpisodeName + "\", preset name is: \"" + fileFfdshowPreset.preset + "\"");
                debug("Episode file name is: " + fileName);
                AnimePluginSettings settings = new AnimePluginSettings();
                if (settings.FfdshowNotificationsLock)
                {
                    Thread.Sleep(settings.FfdshowNotificationsLockTime); //make sure ffdshow has time to load
                }

                
                // Retrieve the ffdshow instance corresponding to the file name
//                FFDShowAPI.FFDShowAPI ffdshowAPI = new FFDShowAPI.FFDShowAPI(fileName, FFDShowAPI.FFDShowAPI.FileNameMode.FileName);  <- currently doesn't work
//                FFDShowAPI.FFDShowAPI ffdshowAPI = new FFDShowAPI.FFDShowAPI();  <- retrieve only the first ffdshow instance
                FFDShowAPI.FFDShowAPI ffdshowAPI = new FFDShowAPI.FFDShowAPI(); 

                List<FFDShowAPI.FFDShowAPI.FFDShowInstance> ffdshowInstances = FFDShowAPI.FFDShowAPI.getFFDShowInstances();
                if (ffdshowInstances == null || ffdshowInstances.Count==0)
                {
                    BaseConfig.MyAnimeLog.Write("FFDShow Error: be sure you have \"ffdshow raw video filter\" added and check in [MediaPortal Configuration -> Videos -> Video Post Processing] and you have in your ffdshow raw configuration: - checked keyboard shortcuts - checked remote API - choosen Custom 32786");
                    return;
                }

                debug("FFDShow number of instance found: " + ffdshowInstances.Count);
                for (int i = 0; i < ffdshowInstances.Count; i++)
                {
                    if (ffdshowInstances[i].fileName == null || ffdshowInstances[i].fileName.Trim() == "") // ffdshow raw doesn't have filename
                    { 
                        debug("FFDShow instance without file name found (raw ffdshow)");
                        ffdshowAPI = new FFDShowAPI.FFDShowAPI(ffdshowInstances[i].handle);
                        if (ffdshowAPI == null)
                        {
                            BaseConfig.MyAnimeLog.Write("FFDShow Error: be sure you have \"ffdshow raw video filter\" added and check in [MediaPortal Configuration -> Videos -> Video Post Processing] and you have in your ffdshow raw configuration: - checked keyboard shortcuts - checked remote API - choosen Custom 32786");
                            return;
                        }
                        break;
                    }

                }

                bool isFFDShowActive = ffdshowAPI.checkFFDShowActive();
                if (isFFDShowActive)
                {
                    debug("FFDShow has been found (" + ffdshowAPI.FFDShowAPIRemote + " handle used)");
                    debug("FFDShow file played: " + ffdshowAPI.getFileName());
                    try
                    {
                        ffdshowAPI.ActivePreset = fileFfdshowPreset.preset;
                        DialogInfo("Set ffdshow raw preset:", "\n\"" + fileFfdshowPreset.preset + "\"");
                        debug("ffdshow raw preset set: " + fileFfdshowPreset.preset);
                    }
                    catch (Exception ex)
                    {
                        debug("Error while setting ffdshow preset : " + ex.Message + "\n" + ex.StackTrace.ToString());
                    }
                }
                else
                {
                    debug("FFDShow has not been found (" + ffdshowAPI.FFDShowAPIRemote + " handle used)");
                    return;
                }




            }
            else
            {
                debug("Preset not found for episode \"" + curEpisode.EpisodeName + "\"");

            }

        }
        public delegate void AsyncloadPlayingPresetCaller(AnimeEpisodeVM curEpisode, string fileName);


        // ASync method to avoid mediaportal video treatment sleep 
        public void loadPlayingPresetASync(AnimeEpisodeVM curEpisode, string fileName)
        {
            // Create an instance of the FFDShowHelper class.
            FFDShowHelper ffdshowHelper = new FFDShowHelper();
            // Create the delegate.
            AsyncloadPlayingPresetCaller caller = new AsyncloadPlayingPresetCaller(loadPlayingPreset);
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
            AnimePluginSettings settings = new AnimePluginSettings();
            if(!settings.FfdshowNotificationsShow)
                return;

            GUIDialogNotify dlgInfo = (GUIDialogNotify)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_NOTIFY);
            if (null == dlgInfo)
                return;

            dlgInfo.Reset();

            //dlgInfo.Focusable = false;

            if (settings.FfdshowNotificationsAutoClose)
            {
                dlgInfo.TimeOut = (settings.FfdshowNotificationsAutoCloseTime/1000);
            }
            dlgInfo.SetHeading(title);
            dlgInfo.SetText(msg);

            dlgInfo.DoModal(GUIWindowManager.ActiveWindow);
        }

        public string findSelectedPresetForMenu(List<AnimeEpisode> episodes)
        {
            bool isShowSelectedPreset = true; // can be set to false to improve preformances, as selectedPreset will not be searched
            string selectedPreset = "";

            foreach (AnimeEpisode ep in episodes)
            {

                // if 1 current preset for selection, set as selected
                if (isShowSelectedPreset)
                {
                    List<FileLocal> fileLocals = ep.FileLocals;
                    foreach (FileLocal fileLocal in fileLocals)
                    {
                        FileFfdshowPreset fileFfdshowPreset = fileLocal.FileFfdshowPreset;
                        if (fileFfdshowPreset != null && fileFfdshowPreset.preset != null && fileFfdshowPreset.preset.Trim() != "")
                        {
                            // get selected preset only if there isn't different ones
                            if (selectedPreset.ToUpper() == fileFfdshowPreset.preset.ToUpper() || selectedPreset == "")
                            {
                                selectedPreset = fileFfdshowPreset.preset;
                            }
                            else
                            {
                                selectedPreset = "";
                                return selectedPreset;
                            }
                        }
                    }
                }
            
            }

            return selectedPreset;
        }


        // default constructor
        public FFDShowHelper()
        {
        }
		*/
    
    }
}
