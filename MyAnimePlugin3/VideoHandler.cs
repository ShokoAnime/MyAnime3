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
using System.Linq;
using System.Net;
using MyAnimePlugin3.DataHelpers;
using MyAnimePlugin3.JMMServerBinary;
using MyAnimePlugin3.ViewModel;
using Stream = MyAnimePlugin3.JMMServerBinary.Stream;


namespace MyAnimePlugin3
{
    public class VideoHandler : GUIWindow
    {
        #region GUI Properties

        public enum GuiProperty
        {
            Play_Current_Cast,
            Play_Current_Collections,
            Play_Current_Credits,
            Play_Current_DVDLabel,
            Play_Current_Director,
            Play_Current_File,
            Play_Current_Genre,
            Play_Current_IMDBNumber,
            Play_Current_IsWatched,
            Play_Current_MPAARating,
            Play_Current_PlotKeywords,
            Play_Current_Runtime,
            Play_Current_Studios,
            Play_Current_TagLine,
            Play_Current_Votes,
            Play_Current_Plot,
            Play_Current_PlotOutline,
            Play_Current_Rating,
            Play_Current_Title,
            Play_Current_Year
        }
        #endregion

        #region Vars
        public AnimeEpisodeVM curEpisode = null;
		public AnimeEpisodeVM prevEpisode = null;
        private IVideoInfo current;
        private IVideoInfo previous;
        private string currentUri=string.Empty;
        private string previousUri=string.Empty;

/*        private string curFileName = "";
		private string prevFileName = "";
        private Media prevMedia;
        private Media curMedia;*/
        int timeMovieStopped = 0;
		private BackgroundWorker w = new BackgroundWorker();
		private bool listenToExternalPlayerEvents = false;
        WebClient wc = new WebClient();
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
			g_Player.PlayBackChanged += new g_Player.ChangedHandler(g_Player_PlayBackChanged);
            w.DoWork += new DoWorkEventHandler(w_DoWork);
        }

		
        #endregion

        public static Dictionary<string, int> RecentWatchPositions { get; set; }=new Dictionary<string, int>();

        #region Public Methods
        public void SetGUIProperty(GuiProperty which, string value, bool isInternalMediaportal = false) { this.SetGUIProperty(which.ToString(), value, isInternalMediaportal); }
        public void ClearGUIProperty(GuiProperty which) { this.ClearGUIProperty(which.ToString()); }
        public string GetPropertyName(GuiProperty which) { return this.GetPropertyName(which.ToString()); }

        private static string StaticGetPropertyName(string which)
        {
            return Extensions.BaseProperties + "." + which.Replace("_", ".").Replace("ñ", "_");
        }
        public static void StaticSetGUIProperty(GuiProperty which, string value)
        {
            if (string.IsNullOrEmpty(value))
                value = " ";
            GUIPropertyManager.SetProperty(StaticGetPropertyName(which.ToString()), value);
        }

        public bool ResumeOrPlay(VideoLocalVM fileToPlay)
		{
			try
			{
				curEpisode = null;


                if (fileToPlay.IsLocalOrStreaming()==null)
				{
					Utils.DialogMsg("Error", "File could not be found!");
					return false;
				}
			    current = fileToPlay;

                BaseConfig.MyAnimeLog.Write("Getting time stopped for : {0}", fileToPlay.FileName);
				BaseConfig.MyAnimeLog.Write("Time stopped for : {0} - {1}", fileToPlay.FileName, fileToPlay.ResumePosition/1000);


                #region Ask user to Resume
                timeMovieStopped = (int)(fileToPlay.ResumePosition / 1000);
                if (timeMovieStopped > 0)
				{
					GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);

					if (null != dlgYesNo)
					{
						dlgYesNo.SetHeading(GUILocalizeStrings.Get(900)); //resume movie?
						dlgYesNo.SetLine(1, fileToPlay.FileName);
						dlgYesNo.SetLine(2, GUILocalizeStrings.Get(936) + " " + MediaPortal.Util.Utils.SecondsToHMSString(TimeSpan.FromMilliseconds(fileToPlay.ResumePosition)));
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
                previous = current;
                previousUri = currentUri;
                current = fileToPlay;
				BaseConfig.MyAnimeLog.Write("Filetoplay: {0}", fileToPlay.FileName);


                if (!fileToPlay.IsLocalOrStreaming()==null)
                {
					Utils.DialogMsg("Error", "File could not be found!");
                    return false;
                }
				BaseConfig.MyAnimeLog.Write("Getting time stopped for : {0}", fileToPlay.FileName);
                timeMovieStopped = (int)(fileToPlay.VideoLocal_ResumePosition/1000);
				BaseConfig.MyAnimeLog.Write("Time stopped for : {0} - {1}", fileToPlay.FileName, timeMovieStopped);

                prevEpisode = curEpisode;
				curEpisode = episode;

 

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

			string displayName = curEpisode.EpisodeNumberAndName;
            string rating = Utils.FormatAniDBRating(Convert.ToDouble(curEpisode.AniDB_Rating)) + " (" + curEpisode.AniDB_Votes + " " + Translation.Votes + ")";
            string formattedEpNameAndAirdate = $"{displayName} [{curEpisode.AirDateAsString}]";

            SetGUIProperty(GuiProperty.Play_Current_Title, clear ? "" : curEpisode.AnimeSeries.SeriesName, true);
            SetGUIProperty(GuiProperty.Play_Current_Year, clear ? "" : formattedEpNameAndAirdate, true);
            SetGUIProperty(GuiProperty.Play_Current_Plot, clear ? "" : curEpisode.EpisodeOverview, true);
            SetGUIProperty(GuiProperty.Play_Current_PlotOutline, clear ? "" : curEpisode.Description, true);
            SetGUIProperty(GuiProperty.Play_Current_Rating, clear ? "" : rating, true);

            // Optional labels
            /*
            SetGUIProperty(GuiProperty.Play_Current_TagLine, string.Empty, true);
            SetGUIProperty(GuiProperty.Play_Current_IsWatched, string.Empty", true);
            SetGUIProperty(GuiProperty.Play_Current_Runtime, string.Empty, true);
            SetGUIProperty(GuiProperty.Play_Current_Cast, string.Empty, true);
            SetGUIProperty(GuiProperty.Play_Current_Votes, string.Empty);
            SetGUIProperty(GuiProperty.Play_Current_PlotKeywords, string.Empty, true);
            SetGUIProperty(GuiProperty.Play_Current_Cast, string.Empty, true);
            SetGUIProperty(GuiProperty.Play_Current_File, string.Empty, true);
            SetGUIProperty(GuiProperty.Play_Current_DVDLabel, string.Empty, true);
            SetGUIProperty(GuiProperty.Play_Current_IMDBNumber, string.Empty, true);
            SetGUIProperty(GuiProperty.Play_Current_Runtime, string.Empty, true);
            SetGUIProperty(GuiProperty.Play_Current_MPAARating, string.Empty, true);
            SetGUIProperty(GuiProperty.Play_Current_IsWatched, string.Empty, true);
            SetGUIProperty(GuiProperty.Play_Current_TagLine, string.Empty, true);
            SetGUIProperty(GuiProperty.Play_Current_Director, string.Empty, true);
            SetGUIProperty(GuiProperty.Play_Current_Genre, string.Empty, true);
            SetGUIProperty(GuiProperty.Play_Current_Credits, string.Empty, true);
            SetGUIProperty(GuiProperty.Play_Current_Studios, string.Empty, true);
            SetGUIProperty(GuiProperty.Play_Current_Collections, string.Empty, true);
            */

            try
            {
                string imgNameSeries = curEpisode.AnimeSeries.PosterPath;
                MediaPortal.GUI.Library.GUIPropertyManager.SetProperty("#Play.Current.Thumb", clear ? "" : imgNameSeries);
            }
            catch { }
				
        }

        void MarkEpisodeAsWatched(AnimeEpisodeVM episode)
        {
			episode.ToggleWatchedStatus(true, false);
			MainWindow.animeSeriesIDToBeRated = episode.AnimeSeriesID;
        }

        void CreateSubsOnTempIfNecesary(Media m)
        {
            Part p = m.Parts[0];
            string fullname = p.Key.Replace("\\", "/").Replace("//", "/").Replace(":", string.Empty);
            string fname = Path.GetFileNameWithoutExtension(fullname);
            if (p.Streams != null)
            {
                foreach (Stream s in p.Streams.Where(a => a.File != null && a.StreamType == "3"))
                {
                    string extension = Path.GetExtension(s.File);
                    string filePath = Path.Combine(Path.GetTempPath(), Path.GetDirectoryName(fullname));
                    try
                    {
                        string subtitle = wc.DownloadString(s.Key);
                        /*   try
                           {
                               Directory.CreateDirectory(filePath);
                               string fullpath = Path.Combine(filePath, fname + extension);
                               File.WriteAllText(filePath, subtitle);
                           }
                           catch (Exception)
                           {
                           }*/
                        try
                        {
                            filePath = Path.Combine(Path.GetTempPath(), fname + extension);
                            File.WriteAllText(filePath, subtitle);
                        }
                        catch (Exception)
                        {
                        }

                    }
                    catch (Exception e)
                    {
                    }
                }
            }
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

                if (current.IsLocalOrStreaming() == true)
                {
                    string filename = current.Media.Parts[0].Key.Split('/').Last();

                    if (BaseConfig.Settings.AskBeforeStartStreamingPlayback)
                    {
                        GUIDialogYesNo dlgYesNo =
                            (GUIDialogYesNo) GUIWindowManager.GetWindow((int) GUIWindow.Window.WINDOW_DIALOG_YES_NO);

                        if (null != dlgYesNo)
                        {

                            dlgYesNo.SetHeading(Translation.UseStreaming);
                            dlgYesNo.SetLine(1, Translation.FileNotFoundLocally);
                            dlgYesNo.SetLine(2, filename);
                            dlgYesNo.SetDefaultToYes(true);
                            dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);

                            if (!dlgYesNo.IsConfirmed)
                            {
                                return false;
                            }
                        }
                    }

                    GUIGraphicsContext.IsFullScreenVideo = true;
                    GUIWindowManager.ActivateWindow((int) GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);

                    // Start Listening to any External Player Events
                    listenToExternalPlayerEvents = true;
                    CreateSubsOnTempIfNecesary(current.Media);

                    IPlayerFactory prevfactory = g_Player.Factory;
                    g_Player.Factory = PlayerFactory.Instance;

                    timeMovieStopped = 0;
                    string title = filename;
                    BaseConfig.MyAnimeLog.Write("Streaming: " + title);
                    BaseConfig.MyAnimeLog.Write("Url: " + current.Media.Parts[0].Key);
                    if (g_Player.Player != null)
                        BaseConfig.MyAnimeLog.Write("Before Player " +
                                                    (g_Player.Player is Player ? "Anime" : "Mediaportal"));
                    BaseConfig.MyAnimeLog.Write("Factory " +
                                                (g_Player.Factory is PlayerFactory ? "Anime" : "Mediaportal"));

                    //FIX MEDIAPORTAL 1 Bug checking for mediainfo.
                    g_Player._mediaInfo = new MediaInfoWrapper("donoexists");
                    //************************//
                    g_Player.Play(current.Media.Parts[0].Key, g_Player.MediaType.Video);

                    currentUri = current.Media.Parts[0].Key;
                    if (g_Player.Player != null)
                        BaseConfig.MyAnimeLog.Write("Player " + (g_Player.Player is Player ? "Anime" : "Mediaportal"));

                    g_Player.Factory = prevfactory;
                }
                else
                {
                    // Double check for local file existence
                    if (!File.Exists(current.LocalFileSystemFullPath))
                    {
                        GUIDialogNotify dlgFileNotFound =
                            (GUIDialogNotify) GUIWindowManager.GetWindow((int) GUIWindow.Window.WINDOW_DIALOG_NOTIFY);

                        if (null != dlgFileNotFound)
                        {
                            string filename = Path.GetFileName(current.LocalFileSystemFullPath);

                            dlgFileNotFound.SetHeading(Translation.FileNotFoundLocally);
                            dlgFileNotFound.SetText(filename);
                            dlgFileNotFound.DoModal(GUIWindowManager.ActiveWindow);

                            if (dlgFileNotFound.SelectedLabel > 0)
                            {
                                // Not handled
                            }
                        }

                        return false;
                    }

                    GUIGraphicsContext.IsFullScreenVideo = true;
                    GUIWindowManager.ActivateWindow((int) GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);

                    // Start Listening to any External Player Events
                    listenToExternalPlayerEvents = true;
                    CreateSubsOnTempIfNecesary(current.Media);

                    g_Player.Play(current.LocalFileSystemFullPath, g_Player.MediaType.Video);
                    currentUri = current.Media.Parts[0].Key = current.LocalFileSystemFullPath;
                }
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
                            if (MediaPortal.Util.Utils.TranslateLanguageString(lang)
                              .Equals(requestedLanguage, StringComparison.OrdinalIgnoreCase))
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
                            if (MediaPortal.Util.Utils.TranslateLanguageString(lang)
                              .Equals(requestedLanguage, StringComparison.OrdinalIgnoreCase))
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

        /*
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
        */
        #region Playback Event Handlers
        void OnPlayBackStopped(MediaPortal.Player.g_Player.MediaType type, int timeMovieStopped, string filename)
        {
            if (PlayBackOpIsOfConcern(type, filename))
            {
                LogPlayBackOp("stopped", filename);
                try
				{
                    JMMServerVM.Instance.clientBinaryHTTP.SetResumePosition(current.VideoLocalID, JMMServerVM.Instance.CurrentUser.JMMUserID,timeMovieStopped*1000);
					BaseConfig.MyAnimeLog.Write("Checking for set watched");
					#region Set Watched
                    double watchedAfter = BaseConfig.Settings.WatchedPercentage;

					if (!g_Player.IsExternalPlayer)
					{
						if ((timeMovieStopped / g_Player.Duration) > watchedAfter / 100)
							PlaybackOperationEnded(true, curEpisode);
						else
							PlaybackOperationEnded(false, curEpisode);
					}
					else
					{
						// if this is an external player always set watched to true
						PlaybackOperationEnded(true, curEpisode);
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
            if (PlayBackOpIsOfConcern(type, filename))
            {
                LogPlayBackOp("ended", filename);

                try
                {
                    PlaybackOperationEnded(true, curEpisode);
                }
                catch (Exception e)
                {
                    BaseConfig.MyAnimeLog.Write("Error in VideoHandler.OnPlayBackEnded: {0}", e.ToString());
                }
            }
        }

		void g_Player_PlayBackChanged(g_Player.MediaType type, int stoptime, string filename)
		{
			if (PlayBackOpWasOfConcern(g_Player.IsVideo ? g_Player.MediaType.Video : g_Player.MediaType.Unknown, g_Player.CurrentFile))
			{
			    LogPlayBackOp("changed", filename);

                try
				{
					BaseConfig.MyAnimeLog.Write("Checking for set watched");
					#region Set Watched
					double watchedAfter = BaseConfig.Settings.WatchedPercentage;
                    JMMServerVM.Instance.clientBinaryHTTP.SetResumePosition(previous.VideoLocalID, JMMServerVM.Instance.CurrentUser.JMMUserID, stoptime * 1000);


                    if (!g_Player.IsExternalPlayer)
					{
						if ((stoptime / g_Player.Duration) > watchedAfter / 100)
							PlaybackOperationEnded(true, prevEpisode);
						else
							PlaybackOperationEnded(false, prevEpisode);
					}
					else
					{
						// if this is an external player always set watched to true
						PlaybackOperationEnded(true, prevEpisode);
					}

					#endregion

				}
				catch (Exception e)
				{
					BaseConfig.MyAnimeLog.Write("AnimePlugin.VideoHandler.OnPlayBackStopped()\r\n" + e.ToString());
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

				// ffdshow preset auto loading
				FFDShowHelper ffdshowHelper = new FFDShowHelper();
				// ASync call to avoid mediaportal video treatment sleep 
				ffdshowHelper.loadPlayingPresetASync(curEpisode, filename);
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
			PlaybackOperationEnded(true, curEpisode);
		}
		#endregion

        #region Helpers

      bool PlayBackOpIsOfConcern(MediaPortal.Player.g_Player.MediaType type, string filename)
      {
          bool IsOfConcern = curEpisode != null && type == g_Player.MediaType.Video && currentUri == filename;
        if (IsOfConcern)
        {
          BaseConfig.MyAnimeLog.Write("PlayBackOpIsOfConcern: {0} - {1} - {2}", filename, type, curEpisode);
        }

        return IsOfConcern;
      }

      bool PlayBackOpWasOfConcern(MediaPortal.Player.g_Player.MediaType type, string filename)
      {
          bool WasOfConcern = prevEpisode != null && type == g_Player.MediaType.Video && previousUri == filename;
        if (WasOfConcern)
        {
          BaseConfig.MyAnimeLog.Write("PlayBackOpWasOfConcern: {0} - {1} - {2}", filename, type, prevEpisode);
        }

        return WasOfConcern;
      }

      void PlaybackOperationEnded(bool countAsWatched, AnimeEpisodeVM ep)
        {
			try
			{
                if (ep == null)
					return;

				if (ep != null)
					ep.IncrementEpisodeStats(StatCountType.Stopped);

				//save watched status
				if (countAsWatched || ep.IsWatched == 1)
				{
					//MPTVSeriesLog.Write("This episode counts as watched");
                    if (countAsWatched)
                    {
						BaseConfig.MyAnimeLog.Write("Marking episode as watched: " + ep.EpisodeNumberAndNameWithType);
						MarkEpisodeAsWatched(ep);

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
