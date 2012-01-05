#region GNU license
// MP-TVSeries - Plugin for Mediaportal
// http://www.team-mediaportal.com
// Copyright (C) 2006-2007
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//
//+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
//+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
#endregion

using System;
using System.Collections.Generic;

using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Dialogs;

using System.ComponentModel;


using MediaPortal.Video.Database;
using System.IO;
using System.Diagnostics;
using MyAnimePlugin3.DataHelpers;
using MyAnimePlugin3.ViewModel;


namespace MyAnimePlugin3
{
    public class VideoHandler
    {
        #region Vars
		public AnimeEpisodeVM curEpisode;
        private string curFileName = "";
        int timeMovieStopped = 0;
		private BackgroundWorker w = new BackgroundWorker();
		private bool listenToExternalPlayerEvents = false;

		public string DefaultAudioLanguage = "<file>";
		public string DefaultSubtitleLanguage = "<file>";
        #endregion

        #region Constructor
        public VideoHandler()
        {
			MediaPortal.Util.Utils.OnStartExternal += new MediaPortal.Util.Utils.UtilEventHandler(onStartExternal);
			MediaPortal.Util.Utils.OnStopExternal += new MediaPortal.Util.Utils.UtilEventHandler(onStopExternal);

            g_Player.PlayBackStopped += new g_Player.StoppedHandler(OnPlayBackStopped);
            g_Player.PlayBackEnded += new g_Player.EndedHandler(OnPlayBackEnded);
            g_Player.PlayBackStarted += new g_Player.StartedHandler(OnPlayBackStarted);
            w.DoWork += new DoWorkEventHandler(w_DoWork);
        }
        #endregion

        private int GetTimeStopped(string fileName)
        {

            IMDBMovie movieDetails = new IMDBMovie();
            VideoDatabase.GetMovieInfo(fileName, ref movieDetails);
            int idFile = VideoDatabase.GetFileId(fileName);
            int idMovie = VideoDatabase.GetMovieId(fileName);

            byte[] resumeData = null;

            if ((idMovie >= 0) && (idFile >= 0))
            {
                return VideoDatabase.GetMovieStopTimeAndResumeData(idFile, out resumeData); 
            }

            return 0;
        }

        #region Public Methods

		public bool ResumeOrPlay(VideoLocalVM fileToPlay)
		{
			try
			{
				curEpisode = null;

				int timeMovieStopped = 0;
				if (!File.Exists(fileToPlay.FullPath))
				{
					Utils.DialogMsg("Error", "File could not be found!");
					return false;
				}

				BaseConfig.MyAnimeLog.Write("Getting time stopped for : {0}", fileToPlay.FullPath);
				timeMovieStopped = GetTimeStopped(fileToPlay.FullPath);
				BaseConfig.MyAnimeLog.Write("Time stopped for : {0} - {1}", fileToPlay.FullPath, timeMovieStopped);

				curFileName = fileToPlay.FullPath;

				#region Ask user to Resume
				if (timeMovieStopped > 0)
				{
					GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);

					if (null != dlgYesNo)
					{
						dlgYesNo.SetHeading(GUILocalizeStrings.Get(900)); //resume movie?
						dlgYesNo.SetLine(1, fileToPlay.FileName);
						dlgYesNo.SetLine(2, GUILocalizeStrings.Get(936) + " " + MediaPortal.Util.Utils.SecondsToHMSString(timeMovieStopped));
						dlgYesNo.SetDefaultToYes(true);
						dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);
						if (!dlgYesNo.IsConfirmed) // reset resume data in DB
						{
							timeMovieStopped = 0;
						}
					}
				}
				#endregion

				Play(timeMovieStopped, fileToPlay.DefaultAudioLanguage, fileToPlay.DefaultSubtitleLanguage);
				return true;
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write("Error inResumeOrPlay : {0}", ex.ToString());
			}
			return false;
		}

		
        public bool ResumeOrPlay(AnimeEpisodeVM episode)
        {
            try
            {
                // get the list if FileLocal records for this AnimeEpisode
                List<VideoDetailedVM> fileLocalList = episode.FilesForEpisode;
				
                if (fileLocalList.Count == 0) return false;
				VideoDetailedVM fileToPlay = null;
                if (fileLocalList.Count == 1)
                    fileToPlay = fileLocalList[0];
                else
                {
                    // ask the user which file they want to play
                    IDialogbox dlg = (IDialogbox)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
                    dlg.Reset();
                    dlg.SetHeading("Select File");
                    GUIListItem pItem = null;

					foreach (VideoDetailedVM fl in fileLocalList)
                    {
                        pItem = new GUIListItem(fl.FileSelectionDisplay);
                        dlg.Add(pItem);
                    }

                    dlg.DoModal(GUIWindowManager.ActiveWindow);

                    if (dlg.SelectedId > 0)
                    {
                        fileToPlay = fileLocalList[dlg.SelectedId - 1];
                    }
					
                }

                

                if (fileToPlay == null) return false;

				BaseConfig.MyAnimeLog.Write("Filetoplay: {0}", fileToPlay.FullPath);


                    if (!File.Exists(fileToPlay.FullPath))
                    {
						Utils.DialogMsg("Error", "File could not be found!");
                        return false;
                    }
					BaseConfig.MyAnimeLog.Write("Getting time stopped for : {0}", fileToPlay.FullPath);
					timeMovieStopped = GetTimeStopped(fileToPlay.FullPath);
					BaseConfig.MyAnimeLog.Write("Time stopped for : {0} - {1}", fileToPlay.FullPath, timeMovieStopped);
               

                curEpisode = episode;
				curFileName = fileToPlay.FullPath;

 

                #region Ask user to Resume
                if (timeMovieStopped > 0)
                {
                    //MPTVSeriesLog.Write("Asking user to resume episode from: " + Utils.SecondsToHMSString(timeMovieStopped));
                    GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);

                    if (null != dlgYesNo)
                    {
                        dlgYesNo.SetHeading(GUILocalizeStrings.Get(900)); //resume movie?
                        dlgYesNo.SetLine(1, episode.EpisodeName);
                        dlgYesNo.SetLine(2, GUILocalizeStrings.Get(936) + " " + MediaPortal.Util.Utils.SecondsToHMSString(timeMovieStopped));
                        dlgYesNo.SetDefaultToYes(true);
                        dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);
                        if (!dlgYesNo.IsConfirmed) // reset resume data in DB
                        {
                            timeMovieStopped = 0;
                            //MPTVSeriesLog.Write("User selected to start episode from beginning", MPTVSeriesLog.LogLevel.Debug);
                        }
                    }
                }
                #endregion

			

                Play(timeMovieStopped, curEpisode.DefaultAudioLanguage, curEpisode.DefaultSubtitleLanguage);
                return true;
            }
            catch (Exception e)
            {
                BaseConfig.MyAnimeLog.Write("ResumeOrPlay: {0}", e.ToString());
                return false;
            }
        }
        #endregion

        /// <summary>        
        /// Updates the movie metadata on the playback screen (for when the user clicks info). 
        /// The delay is neccesary because Player tries to use metadata from the MyVideos database.
        /// We want to update this after that happens so the correct info is there.
        /// Clears properties if (EventArgs.Argument == true)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void w_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            bool clear = (bool)e.Argument;
            if (!clear)
                System.Threading.Thread.Sleep(2000);

            SetGUIProperties((bool)e.Argument);
        }

        /// <summary>
        /// Sets the following Properties:
        /// "#Play.Current.Title"
        /// "#Play.Current.Plot"
        /// "#Play.Current.Thumb"
        /// "#Play.Current.Year"
        /// </summary>
        /// <param name="clear">Clears the properties instead of filling them if True</param>
        void SetGUIProperties(bool clear)
        {
			if (curEpisode == null) return;
			
			string imgNameSeries = "";
			string displayName = curEpisode.EpisodeNumberAndName;

			MediaPortal.GUI.Library.GUIPropertyManager.SetProperty("#Play.Current.Title", clear ? "" : displayName);
			MediaPortal.GUI.Library.GUIPropertyManager.SetProperty("#Play.Current.Plot", clear ? "" : curEpisode.Description);
			MediaPortal.GUI.Library.GUIPropertyManager.SetProperty("#Play.Current.Thumb", clear ? "" : curEpisode.EpisodeImageLocation);

			if (curEpisode.EpisodeImageLocation.Trim().Length == 0)
				MediaPortal.GUI.Library.GUIPropertyManager.SetProperty("#Play.Current.Thumb", clear ? "" : imgNameSeries);
        }

        void MarkEpisodeAsWatched(AnimeEpisodeVM episode)
        {
			episode.ToggleWatchedStatus(true, false);
			MainWindow.animeSeriesIDToBeRated = episode.AnimeSeriesID;
        }

        /// <summary>
        /// Initiates Playback of m_currentEpisode[DBEpisode.cFilename] and calls Fullscreen Window
        /// </summary>
		/// <param name="timeMovieStopped">Resumepoint of Movie, 0 or negative for Start from Beginning</param>
		/// <param name="audioLanguage">Audio language to be used, use null for system default</param>
		/// <param name="subLanguage">Subtitle language to be used, use null for system default or an empty string for no subs</param>
		/// 
        bool Play(int timeMovieStopped, String audioLanguage, String subLanguage)
        {
            bool result = false;
            try
            {
                // sometimes it takes up to 30+ secs to go to fullscreen even though the video is already playing
                // lets force fullscreen here
                // note: MP might still be unresponsive during this time, but at least we are in fullscreen and can see video should this happen
                // I haven't actually found out why it happens, but I strongly believe it has something to do with the video database and the player doing something in the background
                // (why does it do anything with the video database.....i just want it to play a file and do NOTHING else!)
                GUIGraphicsContext.IsFullScreenVideo = true;
                GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);

				// Start Listening to any External Player Events
				listenToExternalPlayerEvents = true;

                result = g_Player.Play(curFileName, g_Player.MediaType.Video) || g_Player.IsExternalPlayer;

				// Stop Listening to any External Player Events
				listenToExternalPlayerEvents = false;

				//set properties
				if (g_Player.Playing)
				{
					g_Player.Pause();

					//set audio language
					if (string.IsNullOrEmpty(audioLanguage))
						audioLanguage = DefaultAudioLanguage;
					if (audioLanguage != "<file>")
					{
						string requestedLanguage = MediaPortal.Util.Utils.TranslateLanguageString(audioLanguage);
						for (int index = 0; index < g_Player.AudioStreams; index++)
						{
							string lang = g_Player.AudioLanguage(index);
							if (MediaPortal.Util.Utils.TranslateLanguageString(lang).Equals(requestedLanguage, StringComparison.OrdinalIgnoreCase))
							{
								g_Player.Player.CurrentAudioStream = index;
								break;
							}
						}
					}

					//set sub language
					g_Player.Player.EnableSubtitle = true;
					if (string.IsNullOrEmpty(subLanguage))
						subLanguage = DefaultSubtitleLanguage;
					if (subLanguage == "<none>")
					{
						//no subs
						g_Player.Player.EnableSubtitle = false;
					}
					else if (subLanguage != "<file>")
					{
						//selected sub
						string requestedLanguage = MediaPortal.Util.Utils.TranslateLanguageString(subLanguage);
						for (int index = 0; index < g_Player.SubtitleStreams; index++)
						{
							string lang = g_Player.SubtitleLanguage(index);
							if (MediaPortal.Util.Utils.TranslateLanguageString(lang).Equals(requestedLanguage, StringComparison.OrdinalIgnoreCase))
							{
								g_Player.Player.CurrentSubtitleStream = index;
								break;
							}
						}
					}

					// tell player where to resume
					if (timeMovieStopped > 0)
						g_Player.SeekAbsolute(timeMovieStopped);

					if (curEpisode != null)
						curEpisode.IncrementEpisodeStats(StatCountType.Played);

					g_Player.Pause();

				}
            }
            catch (Exception e)
            {
                BaseConfig.MyAnimeLog.Write("Error in VideoHandler.Play: {0}", e);
                result = false;
            }
            return result;
        }

		public bool PlayPreview(string fileName)
		{
			bool result = false;
			try
			{
				GUIGraphicsContext.IsFullScreenVideo = false;
				result = g_Player.Play(fileName, g_Player.MediaType.Video);

			}
			catch (Exception e)
			{
				BaseConfig.MyAnimeLog.Write("Error in VideoHandler.Play: {0}", e);
				result = false;
			}
			return result;
		}

        #region Playback Event Handlers
        void OnPlayBackStopped(MediaPortal.Player.g_Player.MediaType type, int timeMovieStopped, string filename)
        {
			BaseConfig.MyAnimeLog.Write("OnPlayBackStopped: {0} - {1} - {2}", filename, timeMovieStopped, type);
            if (PlayBackOpIsOfConcern(type, filename))
            {
                LogPlayBackOp("stopped", filename);
                try
				{
					BaseConfig.MyAnimeLog.Write("Checking for set watched");
					#region Set Watched
                    double watchedAfter = BaseConfig.Settings.WatchedPercentage;

					if (!g_Player.IsExternalPlayer)
					{
						if ((timeMovieStopped / g_Player.Duration) > watchedAfter / 100)
							PlaybackOperationEnded(true);
						else
							PlaybackOperationEnded(false);
					}
					else
					{
						// if this is an external player always set watched to true
						PlaybackOperationEnded(true);
					}

					#endregion

				}
                catch (Exception e)
                {
                    BaseConfig.MyAnimeLog.Write("AnimePlugin.VideoHandler.OnPlayBackStopped()\r\n" + e.ToString());
                }
            }

			
        }

        void OnPlayBackEnded(MediaPortal.Player.g_Player.MediaType type, string filename)
        {
			BaseConfig.MyAnimeLog.Write("OnPlayBackEnded: {0} - {1}", filename, type);
            if (PlayBackOpIsOfConcern(type, filename))
            {
                LogPlayBackOp("ended", filename);
                try
                {
                    PlaybackOperationEnded(true);
                }
                catch (Exception e)
                {
                    BaseConfig.MyAnimeLog.Write("Error in VideoHandler.OnPlayBackEnded: {0}", e.ToString());
                }
            }
        }

        void OnPlayBackStarted(MediaPortal.Player.g_Player.MediaType type, string filename)
        {
            if (PlayBackOpIsOfConcern(type, filename))
            {
                LogPlayBackOp("started", filename);

                // really stupid, you have to wait until the player itself sets the properties (a few seconds) and after that set them
                w.RunWorkerAsync(false);
            }
        }
        #endregion

		#region External Player Event Handlers
		private void onStartExternal(Process proc, bool waitForExit)
		{
			// If we were listening for external player events
			if (listenToExternalPlayerEvents)
			{
				BaseConfig.MyAnimeLog.Write("Playback Started in External Player");
			}
		}

		private void onStopExternal(Process proc, bool waitForExit)
		{
			if (!listenToExternalPlayerEvents)
				return;

			BaseConfig.MyAnimeLog.Write("Playback Stopped in External Player");

			// Exit fullscreen Video so we can see main facade again			
			if (GUIGraphicsContext.IsFullScreenVideo)
			{
				GUIGraphicsContext.IsFullScreenVideo = false;
			}
			// Mark Episode as watched regardless and prompt for rating
			PlaybackOperationEnded(true);
		}
		#endregion

        #region Helpers
        bool PlayBackOpIsOfConcern(MediaPortal.Player.g_Player.MediaType type, string filename)
        {
			BaseConfig.MyAnimeLog.Write("PlayBackOpIsOfConcern: {0} - {1} - {2}", filename, type, curEpisode);
            return (curEpisode != null &&
                    type == g_Player.MediaType.Video &&
                    curFileName == filename);
        }

        void PlaybackOperationEnded(bool countAsWatched)
        {
			try
			{
				if (curEpisode == null)
					return;

				if (curEpisode != null)
					curEpisode.IncrementEpisodeStats(StatCountType.Stopped);

				//save watched status
				if (countAsWatched || curEpisode.IsWatched == 1)
				{
					//MPTVSeriesLog.Write("This episode counts as watched");
                    if (countAsWatched)
                    {
						BaseConfig.MyAnimeLog.Write("Marking episode as watched...");
                        MarkEpisodeAsWatched(curEpisode);

                    }
				}
				
                
                SetGUIProperties(true); // clear GUI Properties     
			}
			catch (Exception e)
			{
				BaseConfig.MyAnimeLog.Write("Error in VideoHandler.PlaybackOperationEnded: {0}", e.ToString());
			}
        }

        void LogPlayBackOp(string OperationType, string filename)
        {
            BaseConfig.MyAnimeLog.Write(string.Format("Playback {0} for: {1}", OperationType, filename));
        }
        #endregion
    }
}
