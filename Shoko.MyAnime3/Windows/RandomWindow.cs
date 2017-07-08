﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using MediaPortal.GUI.Library;
using Shoko.Commons.Extensions;
using Shoko.Models.Enums;
using Shoko.MyAnime3.ViewModel;
using Shoko.MyAnime3.ViewModel.Helpers;
using Shoko.MyAnime3.ViewModel.Server;
using Action = MediaPortal.GUI.Library.Action;

namespace Shoko.MyAnime3.Windows
{
    public class RandomWindow : GUIWindow
    {
        //public Shoko.Models.Enums.RandomSeriesEpisodeLevel RandomLevel { get; set; }
        //public Shoko.Models.Enums.RandomObjectType RandomType { get; set; }
        //private object LevelObject = null;
        private static readonly Random rndm = new Random();

        [SkinControl(801)] protected GUIButtonControl btnRandom = null;
        [SkinControl(806)] protected GUIButtonControl btnAddTag = null;
        [SkinControl(807)] protected GUIButtonControl btnClearTags = null;
        [SkinControl(808)] protected GUIButtonControl btnAllAnyTags = null;
        [SkinControl(810)] protected GUIButtonControl btnEpisodeList = null;

        [SkinControl(802)] protected GUICheckButton togWatched = null;
        [SkinControl(803)] protected GUICheckButton togUnwatched = null;
        [SkinControl(804)] protected GUICheckButton togPartiallyWatched = null;
        [SkinControl(805)] protected GUICheckButton togCompleteOnly = null;

        [SkinControl(821)] protected GUICheckButton togEpisodeWatched = null;
        [SkinControl(822)] protected GUICheckButton togEpisodeUnwatched = null;
        [SkinControl(823)] protected GUIButtonControl btnEpisodeAddTag = null;
        [SkinControl(824)] protected GUIButtonControl btnEpisodeClearTags = null;
        [SkinControl(825)] protected GUIButtonControl btnPlayEpisode = null;
        [SkinControl(826)] protected GUIButtonControl btnEpisodeAllAnyTags = null;

        [SkinControl(901)] protected GUIButtonControl btnSwitchSeries = null;
        [SkinControl(902)] protected GUIButtonControl btnSwitchEpisode = null;

        [SkinControl(920)] protected GUIButtonControl btnWindowContinueWatching = null;
        [SkinControl(921)] protected GUIButtonControl btnWindowUtilities = null;
        [SkinControl(922)] protected GUIButtonControl btnWindowCalendar = null;

        [SkinControl(925)] protected GUIButtonControl btnWindowRecommendations = null;
        [SkinControl(927)] protected GUIButtonControl btnWindowPlaylists = null;

        [SkinControl(1551)] protected GUILabelControl dummyRandomSeries = null;
        [SkinControl(1552)] protected GUILabelControl dummyRandomEpisode = null;
        [SkinControl(1553)] protected GUILabelControl dummyNoData = null;

        public enum GuiProperty
        {
            Random_Status,
            Random_LevelType,
            Random_LevelName,
            Random_NumberOfMatches,
            Random_CombinedFilterDetails,
            Random_Series_TagType,
            Random_Episode_TagType,
            Random_Series_Tags,
            Random_Episode_Tags,
            Random_Series_Title,
            Random_Series_Description,
            Random_Series_LastWatched,
            Random_Series_EpisodesWatched,
            Random_Series_EpisodesUnwatched,
            Random_Series_Poster,
            Random_Episode_Title,
            Random_Episode_AirDate,
            Random_Episode_RunTime,
            Random_Episode_FileInfo,
            Random_Episode_Overview,
            Random_Episode_Image,
            Random_Episode_Logos
        }

        public void SetGUIProperty(GuiProperty which, string value)
        {
            this.SetGUIProperty(which.ToString(), value);
        }

        public void ClearGUIProperty(GuiProperty which)
        {
            this.ClearGUIProperty(which.ToString());
        }


        private List<string> AllTags = new List<string>();

        private readonly BackgroundWorker getDataWorker = new BackgroundWorker();

        public RandomWindow()
        {
            // get ID of windowplugin belonging to this setup
            // enter your own unique code
            GetID = Constants.WindowIDs.RANDOM;

            getDataWorker.DoWork += getDataWorker_DoWork;
            getDataWorker.RunWorkerCompleted += getDataWorker_RunWorkerCompleted;
        }

        public void SetDetails()
        {
            if (MainWindow.RandomWindow_RandomType == RandomObjectType.Series)
            {
                dummyRandomSeries.Visible = true;
                dummyRandomEpisode.Visible = false;

                if (btnRandom != null) btnRandom.NavigateDown = 802;
                if (btnRandom != null) btnRandom.NavigateUp = 810;
                if (btnRandom != null) btnRandom.NavigateRight = 805;
            }
            else
            {
                dummyRandomSeries.Visible = false;
                dummyRandomEpisode.Visible = true;

                if (btnRandom != null) btnRandom.NavigateDown = 821;
                if (btnRandom != null) btnRandom.NavigateUp = 825;
                if (btnRandom != null) btnRandom.NavigateRight = 823;
            }

            SetDisplayDetails();
        }

        private void SetDisplayDetails()
        {
            string combinedDetails = string.Empty;
            if (MainWindow.RandomWindow_RandomLevel == RandomSeriesEpisodeLevel.GroupFilter)
            {
                SetGUIProperty(GuiProperty.Random_LevelType, Translation.GroupFilter);
                VM_GroupFilter gf = MainWindow.RandomWindow_LevelObject as VM_GroupFilter;
                if (gf != null)
                {
                    SetGUIProperty(GuiProperty.Random_LevelName, gf.GroupFilterName);

                    combinedDetails += Translation.GroupFilter + ": " + gf.GroupFilterName + " ";
                }
                else
                {
                    ClearGUIProperty(GuiProperty.Random_LevelName);
                }
            }
            else if (MainWindow.RandomWindow_RandomLevel == RandomSeriesEpisodeLevel.Group)
            {
                SetGUIProperty(GuiProperty.Random_LevelType, Translation.Group);
                VM_AnimeGroup_User grp = MainWindow.RandomWindow_LevelObject as VM_AnimeGroup_User;
                if (grp != null)
                {
                    SetGUIProperty(GuiProperty.Random_LevelName, grp.GroupName);
                    combinedDetails += Translation.Group + ": " + grp.GroupName + " ";
                }
                else
                {
                    ClearGUIProperty(GuiProperty.Random_LevelName);
                }
            }
            else if (MainWindow.RandomWindow_RandomLevel == RandomSeriesEpisodeLevel.Series)
            {
                SetGUIProperty(GuiProperty.Random_LevelType, Translation.Series);
                VM_AnimeSeries_User ser = MainWindow.RandomWindow_LevelObject as VM_AnimeSeries_User;
                if (ser != null)
                {
                    SetGUIProperty(GuiProperty.Random_LevelName, ser.SeriesName);

                    combinedDetails += Translation.Series + ": " + ser.SeriesName + " ";
                }
                else
                {
                    ClearGUIProperty(GuiProperty.Random_LevelName);
                }
            }
            else
            {
                ClearGUIProperty(GuiProperty.Random_LevelType);
                ClearGUIProperty(GuiProperty.Random_LevelName);
            }
            if (MainWindow.RandomWindow_RandomType == RandomObjectType.Series)
                combinedDetails += string.Format(" ({0} " + (MainWindow.RandomWindow_MatchesFound == 1 ? Translation.Match : Translation.Matches) + ")", MainWindow.RandomWindow_MatchesFound);

            SetGUIProperty(GuiProperty.Random_CombinedFilterDetails, combinedDetails);

            SetGUIProperty(GuiProperty.Random_NumberOfMatches, MainWindow.RandomWindow_MatchesFound.ToString(Globals.Culture));

            SetGUIProperty(GuiProperty.Random_Series_Tags, string.Join(", ", MainWindow.RandomWindow_SeriesTags));
            SetGUIProperty(GuiProperty.Random_Episode_Tags, string.Join(", ", MainWindow.RandomWindow_EpisodeTags));

            SetGUIProperty(GuiProperty.Random_Series_TagType, MainWindow.RandomWindow_SeriesAllTags ? Translation.All : Translation.Any);
            SetGUIProperty(GuiProperty.Random_Episode_TagType, MainWindow.RandomWindow_EpisodeAllTags ? Translation.All : Translation.Any);
        }

        void getDataWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (btnRandom != null) btnRandom.IsFocused = true;
        }

        void getDataWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (MainWindow.RandomWindow_RandomType == RandomObjectType.Series) SetRandomSeries();
            if (MainWindow.RandomWindow_RandomType == RandomObjectType.Episode) SetRandomEpisode();
        }

        public override bool Init()
        {
            return Load(GUIGraphicsContext.Skin + @"\Anime3_Random.xml");
        }

        public override int GetID
        {
            get { return Constants.WindowIDs.RANDOM; }
            set { base.GetID = value; }
        }

        protected override void OnPageLoad()
        {
            base.OnPageLoad();
            BaseConfig.MyAnimeLog.Write("OnPageLoad: RandomWindow");


            if (togWatched != null) togWatched.Selected = MainWindow.RandomWindow_SeriesWatched;
            if (togUnwatched != null) togUnwatched.Selected = MainWindow.RandomWindow_SeriesUnwatched;
            if (togPartiallyWatched != null) togPartiallyWatched.Selected = MainWindow.RandomWindow_SeriesPartiallyWatched;
            if (togCompleteOnly != null) togCompleteOnly.Selected = MainWindow.RandomWindow_SeriesOnlyComplete;

            if (togEpisodeUnwatched != null) togEpisodeUnwatched.Selected = MainWindow.RandomWindow_EpisodeUnwatched;
            if (togEpisodeWatched != null) togEpisodeWatched.Selected = MainWindow.RandomWindow_EpisodeWatched;

            LoadData();

            //m_Facade.Focus = true;
        }


        private void LoadData()
        {
            if (getDataWorker.IsBusy) return;

            SetGUIProperty(GuiProperty.Random_Status, Translation.Loading + "...");

            dummyNoData.Visible = true;

            if (MainWindow.RandomWindow_RandomLevel == RandomSeriesEpisodeLevel.All)
            {
                MainWindow.RandomWindow_LevelObject = GroupFilterHelper.AllGroupsFilter;
                MainWindow.RandomWindow_RandomLevel = RandomSeriesEpisodeLevel.GroupFilter;
            }

            SetDetails();

            if (MainWindow.RandomWindow_RandomType == RandomObjectType.Series && MainWindow.RandomWindow_CurrentSeries != null)
                DisplaySeries();
            else if (MainWindow.RandomWindow_RandomType == RandomObjectType.Episode && MainWindow.RandomWindow_CurrentEpisode != null)
                DisplayEpisode();
            else
                getDataWorker.RunWorkerAsync();
        }

        private List<VM_AnimeSeries_User> GetSeriesForGroupFilter()
        {
            List<VM_AnimeSeries_User> serList = new List<VM_AnimeSeries_User>();
            try
            {
                BaseConfig.MyAnimeLog.Write("Getting list of candidate random series");

                if (MainWindow.RandomWindow_RandomLevel != RandomSeriesEpisodeLevel.GroupFilter) return serList;
                VM_GroupFilter gf = MainWindow.RandomWindow_LevelObject as VM_GroupFilter;
                if (gf?.GroupFilterID == null) return serList;

                BaseConfig.MyAnimeLog.Write("Getting list of candidate random series for: " + gf.GroupFilterName);

                bool allTags = MainWindow.RandomWindow_SeriesAllTags;
                if (MainWindow.RandomWindow_RandomType == RandomObjectType.Episode)
                    allTags = MainWindow.RandomWindow_EpisodeAllTags;

                bool completeSeries = true;
                bool allWatched = true;
                bool unwatched = true;
                bool partiallyWatched = true;

                if (togWatched != null) allWatched = togWatched.Selected;
                if (togUnwatched != null) unwatched = togUnwatched.Selected;
                if (togPartiallyWatched != null) partiallyWatched = togPartiallyWatched.Selected;
                if (togCompleteOnly != null) completeSeries = togCompleteOnly.Selected;

                List<VM_AnimeGroup_User> contracts = VM_ShokoServer.Instance.ShokoServices.GetAnimeGroupsForFilter(
                    gf.GroupFilterID, VM_ShokoServer.Instance.CurrentUser.JMMUserID,
                    false).CastList<VM_AnimeGroup_User>();

                BaseConfig.MyAnimeLog.Write("Total groups for filter = " + contracts.Count);

                HashSet<string> selectedTags = new HashSet<string>(MainWindow.RandomWindow_SeriesTags, StringComparer.InvariantCultureIgnoreCase);
                if (MainWindow.RandomWindow_RandomType == RandomObjectType.Episode)
                    selectedTags = new HashSet<string>(MainWindow.RandomWindow_EpisodeTags, StringComparer.InvariantCultureIgnoreCase);

                foreach (VM_AnimeGroup_User grp in contracts)
                {
                    // ignore sub groups
                    if (grp.AnimeGroupParentID.HasValue) continue;

                    foreach (VM_AnimeSeries_User ser in grp.AllSeries)
                    {
                        // tags
                        if (selectedTags.Count > 0)
                        {
                            bool foundTag = false;
                            if (allTags) foundTag = true; // all
                            if (ser.Anime.GetAllTags().Overlaps(selectedTags))
                                foundTag = true;
                            if (!foundTag) continue;
                        }

                        if (!ser.IsComplete && completeSeries) continue;

                        if (allWatched && ser.AllFilesWatched)
                        {
                            serList.Add(ser);
                            continue;
                        }

                        if (unwatched && !ser.AnyFilesWatched)
                        {
                            serList.Add(ser);
                            continue;
                        }


                        if (partiallyWatched && ser.AnyFilesWatched && !ser.AllFilesWatched)
                            serList.Add(ser);
                    }
                }
            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write(ex.ToString());
            }

            return serList;
        }

        private List<VM_AnimeSeries_User> GetSeriesForGroup()
        {
            List<VM_AnimeSeries_User> serList = new List<VM_AnimeSeries_User>();
            try
            {
                BaseConfig.MyAnimeLog.Write("Getting list of candidate random series for group");

                if (MainWindow.RandomWindow_RandomLevel != RandomSeriesEpisodeLevel.Group) return serList;
                VM_AnimeGroup_User grp = MainWindow.RandomWindow_LevelObject as VM_AnimeGroup_User;
                if (grp == null) return serList;

                bool allTags = MainWindow.RandomWindow_SeriesAllTags;
                if (MainWindow.RandomWindow_RandomType == RandomObjectType.Episode)
                    allTags = MainWindow.RandomWindow_EpisodeAllTags;

                BaseConfig.MyAnimeLog.Write("Getting list of candidate random series for: " + grp.GroupName);

                HashSet<string> selectedTags = new HashSet<string>(MainWindow.RandomWindow_SeriesTags, StringComparer.InvariantCultureIgnoreCase);
                if (MainWindow.RandomWindow_RandomType == RandomObjectType.Episode)
                    selectedTags = new HashSet<string>(MainWindow.RandomWindow_EpisodeTags, StringComparer.InvariantCultureIgnoreCase);


                foreach (VM_AnimeSeries_User ser in grp.AllSeries)
                {
                    // tags
                    if (selectedTags.Count > 0)
                    {
                        bool foundTag = false;
                        if (allTags) foundTag = true; // all
                        if (ser.Anime.GetAllTags().Overlaps(selectedTags))
                            foundTag = true;
                        if (!foundTag) continue;
                    }

                    serList.Add(ser);
                }
            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write(ex.ToString());
            }

            return serList;
        }

        private void SetRandomSeries()
        {
            try
            {
                List<VM_AnimeSeries_User> serList = new List<VM_AnimeSeries_User>();
                if (MainWindow.RandomWindow_RandomLevel == RandomSeriesEpisodeLevel.GroupFilter)
                    serList = GetSeriesForGroupFilter();

                if (MainWindow.RandomWindow_RandomLevel == RandomSeriesEpisodeLevel.Group)
                    serList = GetSeriesForGroup();

                if (MainWindow.RandomWindow_RandomLevel == RandomSeriesEpisodeLevel.Series)
                {
                    VM_AnimeSeries_User ser = MainWindow.RandomWindow_LevelObject as VM_AnimeSeries_User;
                    if (ser != null) serList.Add(ser);
                }

                BaseConfig.MyAnimeLog.Write("Found " + serList.Count + " series");

                if (serList != null && serList.Count > 0)
                {
                    MainWindow.RandomWindow_MatchesFound = serList.Count;

                    VM_AnimeSeries_User ser = serList[rndm.Next(0, serList.Count)];
                    MainWindow.RandomWindow_CurrentSeries = ser;
                    DisplaySeries();
                }
                else
                {
                    DisplaySeries();
                }


                SetDisplayDetails();
            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write(ex.ToString());
            }
        }

        private void SetRandomEpisode()
        {
            try
            {
                List<VM_AnimeSeries_User> serList = new List<VM_AnimeSeries_User>();
                if (MainWindow.RandomWindow_RandomLevel == RandomSeriesEpisodeLevel.GroupFilter)
                    serList = GetSeriesForGroupFilter();

                if (MainWindow.RandomWindow_RandomLevel == RandomSeriesEpisodeLevel.Group)
                    serList = GetSeriesForGroup();

                if (MainWindow.RandomWindow_RandomLevel == RandomSeriesEpisodeLevel.Series)
                {
                    VM_AnimeSeries_User ser = MainWindow.RandomWindow_LevelObject as VM_AnimeSeries_User;
                    if (ser != null) serList.Add(ser);
                }

                BaseConfig.MyAnimeLog.Write("Found " + serList.Count + " series for episodes");

                bool watched = true;
                bool unwatched = true;

                if (togEpisodeWatched != null) watched = togEpisodeWatched.Selected;
                if (togEpisodeUnwatched != null) unwatched = togEpisodeUnwatched.Selected;

                MainWindow.RandomWindow_CurrentEpisode = null;

                if (serList != null && serList.Count > 0)
                {
                    Dictionary<int, VM_AnimeSeries_User> dictSeries = new Dictionary<int, VM_AnimeSeries_User>();
                    foreach (VM_AnimeSeries_User ser in serList)
                        dictSeries[ser.AnimeSeriesID] = ser;

                    bool needEpisode = true;
                    while (needEpisode)
                    {
                        if (dictSeries.Values.Count == 0)
                            break;

                        List<VM_AnimeSeries_User> tempSerList = new List<VM_AnimeSeries_User>(dictSeries.Values);
                        MainWindow.RandomWindow_CurrentSeries = tempSerList[rndm.Next(0, tempSerList.Count)];

                        // get all the episodes
                        List<VM_AnimeEpisode_User> epList = new List<VM_AnimeEpisode_User>();
                        foreach (VM_AnimeEpisode_User ep in MainWindow.RandomWindow_CurrentSeries.AllEpisodes)
                        {
                            bool useEp = false;
                            if (watched && ep.Watched)
                                useEp = true;

                            if (unwatched && !ep.Watched)
                                useEp = true;

                            if (ep.LocalFileCount == 0)
                                useEp = false;

                            if (ep.EpisodeTypeEnum != enEpisodeType.Episode && ep.EpisodeTypeEnum != enEpisodeType.Special)
                                useEp = false;

                            if (useEp) epList.Add(ep);
                        }

                        if (epList.Count > 0)
                        {
                            MainWindow.RandomWindow_CurrentEpisode = epList[rndm.Next(0, epList.Count)];
                            needEpisode = false;
                        }
                        else if (MainWindow.RandomWindow_CurrentSeries.AnimeSeriesID != 0)
                        {
                            dictSeries.Remove(MainWindow.RandomWindow_CurrentSeries.AnimeSeriesID);
                        }
                    }
                }

                DisplayEpisode();

                SetDisplayDetails();
            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write(ex.ToString());
            }
        }

        private void DisplayEpisode()
        {
            if (MainWindow.RandomWindow_CurrentEpisode == null)
            {
                SetGUIProperty(GuiProperty.Random_Status, Translation.NoMatchesFound);
                MainWindow.RandomWindow_MatchesFound = 0;
                MainWindow.RandomWindow_CurrentSeries = null;
                MainWindow.RandomWindow_CurrentEpisode = null;
            }
            else
            {
                SetGUIProperty(GuiProperty.Random_Series_Description, MainWindow.RandomWindow_CurrentSeries.Description);
                SetGUIProperty(GuiProperty.Random_Series_Title, MainWindow.RandomWindow_CurrentSeries.SeriesName);
                SetGUIProperty(GuiProperty.Random_Series_LastWatched, MainWindow.RandomWindow_CurrentSeries.WatchedDate.HasValue ? MainWindow.RandomWindow_CurrentSeries.WatchedDate.Value.ToString("dd MMM yy", Globals.Culture) : "-");
                SetGUIProperty(GuiProperty.Random_Series_EpisodesWatched, MainWindow.RandomWindow_CurrentSeries.WatchedEpisodeCount.ToString(Globals.Culture));
                SetGUIProperty(GuiProperty.Random_Series_EpisodesUnwatched, MainWindow.RandomWindow_CurrentSeries.UnwatchedEpisodeCount.ToString(Globals.Culture));
                SetGUIProperty(GuiProperty.Random_Series_Poster, ImageAllocator.GetSeriesImageAsFileName(MainWindow.RandomWindow_CurrentSeries, GUIFacadeControl.Layout.List));

                SetGUIProperty(GuiProperty.Random_Episode_Title, MainWindow.RandomWindow_CurrentEpisode.EpisodeNumberAndNameWithType);
                SetGUIProperty(GuiProperty.Random_Episode_AirDate, MainWindow.RandomWindow_CurrentEpisode.AirDateAsString);
                SetGUIProperty(GuiProperty.Random_Episode_RunTime, Utils.FormatSecondsToDisplayTime(MainWindow.RandomWindow_CurrentEpisode.AniDB_LengthSeconds));


                if (MainWindow.RandomWindow_CurrentEpisode.EpisodeImageLocation.Length > 0)
                    SetGUIProperty(GuiProperty.Random_Episode_Image, MainWindow.RandomWindow_CurrentEpisode.EpisodeImageLocation);
                else
                    ClearGUIProperty(GuiProperty.Random_Episode_Image);

                // Overview
                string overview = MainWindow.RandomWindow_CurrentEpisode.EpisodeOverview;
                if (BaseConfig.Settings.HidePlot)
                {
                    if (MainWindow.RandomWindow_CurrentEpisode.EpisodeOverview.Trim().Length > 0 &&
                        !MainWindow.RandomWindow_CurrentEpisode.IsWatched())
                        overview = "*** " + Translation.HiddenToPreventSpoiles + " ***";
                    SetGUIProperty(GuiProperty.Random_Episode_Overview, overview);
                }

                // File Info
                List<VM_VideoDetailed> filesForEpisode =
                    VM_ShokoServer.Instance.ShokoServices.GetFilesForEpisode(
                        MainWindow.RandomWindow_CurrentEpisode.AnimeEpisodeID,
                        VM_ShokoServer.Instance.CurrentUser.JMMUserID).CastList<VM_VideoDetailed>();


                string finfo = "";
                foreach (VM_VideoDetailed vid in filesForEpisode)
                    finfo = vid.FileSelectionDisplay;

                if (filesForEpisode.Count > 1)
                    finfo = filesForEpisode.Count.ToString(Globals.Culture) + " " + Translation.FilesAvailable;

                SetGUIProperty(GuiProperty.Random_Episode_FileInfo, finfo);
                // Logos
                string logos = Logos.buildLogoImage(MainWindow.RandomWindow_CurrentEpisode);

                BaseConfig.MyAnimeLog.Write(logos);
                SetGUIProperty(GuiProperty.Random_Episode_Logos, logos);

                dummyNoData.Visible = false;
            }
        }

        private void DisplaySeries()
        {
            if (MainWindow.RandomWindow_CurrentSeries == null)
            {
                SetGUIProperty(GuiProperty.Random_Status, Translation.NoMatchesFound);
                ClearGUIProperty(GuiProperty.Random_Series_Title);
                ClearGUIProperty(GuiProperty.Random_Series_Description);
                ClearGUIProperty(GuiProperty.Random_Series_LastWatched);
                ClearGUIProperty(GuiProperty.Random_Series_EpisodesWatched);
                ClearGUIProperty(GuiProperty.Random_Series_EpisodesUnwatched);
                ClearGUIProperty(GuiProperty.Random_Series_Poster);
                ClearGUIProperty(GuiProperty.Random_NumberOfMatches);
                MainWindow.RandomWindow_MatchesFound = 0;
                MainWindow.RandomWindow_CurrentSeries = null;
            }
            else
            {
                SetGUIProperty(GuiProperty.Random_Series_Title, MainWindow.RandomWindow_CurrentSeries.SeriesName);
                SetGUIProperty(GuiProperty.Random_Series_Description, MainWindow.RandomWindow_CurrentSeries.Description);
                SetGUIProperty(GuiProperty.Random_Series_LastWatched, MainWindow.RandomWindow_CurrentSeries.WatchedDate.HasValue ? MainWindow.RandomWindow_CurrentSeries.WatchedDate.Value.ToString("dd MMM yy", Globals.Culture) : "-");
                SetGUIProperty(GuiProperty.Random_Series_EpisodesWatched, MainWindow.RandomWindow_CurrentSeries.WatchedEpisodeCount.ToString(Globals.Culture));
                SetGUIProperty(GuiProperty.Random_Series_EpisodesUnwatched, MainWindow.RandomWindow_CurrentSeries.UnwatchedEpisodeCount.ToString(Globals.Culture));
                SetGUIProperty(GuiProperty.Random_Series_Poster, ImageAllocator.GetSeriesImageAsFileName(MainWindow.RandomWindow_CurrentSeries, GUIFacadeControl.Layout.List));
                dummyNoData.Visible = false;
            }
        }

        protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
        {
            MainMenu menu = new MainMenu();
            menu.Add(btnAddTag, () =>
            {
                string tag = Utils.PromptSelectTag("");
                if (!MainWindow.RandomWindow_SeriesTags.Contains(tag, StringComparer.InvariantCultureIgnoreCase))
                {
                    MainWindow.RandomWindow_SeriesTags.Add(tag);
                    SetDisplayDetails();
                }
            });
            menu.Add(btnEpisodeAddTag, () =>
            {
                string tag = Utils.PromptSelectTag("");
                if (!MainWindow.RandomWindow_EpisodeTags.Contains(tag, StringComparer.InvariantCultureIgnoreCase))
                {
                    MainWindow.RandomWindow_EpisodeTags.Add(tag);
                    SetDisplayDetails();
                }
            });
            menu.Add(btnClearTags, () =>
            {
                MainWindow.RandomWindow_SeriesTags.Clear();
                SetDisplayDetails();
            });
            menu.Add(btnEpisodeClearTags, () =>
            {
                MainWindow.RandomWindow_EpisodeTags.Clear();
                SetDisplayDetails();
            });
            menu.Add(btnAllAnyTags, () =>
            {
                MainWindow.RandomWindow_SeriesAllTags = !MainWindow.RandomWindow_SeriesAllTags;
                SetDisplayDetails();
            });
            menu.Add(btnEpisodeAllAnyTags, () =>
            {
                MainWindow.RandomWindow_EpisodeAllTags = !MainWindow.RandomWindow_EpisodeAllTags;
                SetDisplayDetails();
            });
            menu.Add(btnRandom, () =>
            {
                if (togWatched != null) MainWindow.RandomWindow_SeriesWatched = togWatched.Selected;
                if (togUnwatched != null)
                    MainWindow.RandomWindow_SeriesUnwatched = togUnwatched.Selected;
                if (togPartiallyWatched != null)
                    MainWindow.RandomWindow_SeriesPartiallyWatched = togPartiallyWatched.Selected;
                if (togCompleteOnly != null)
                    MainWindow.RandomWindow_SeriesOnlyComplete = togCompleteOnly.Selected;

                if (togEpisodeUnwatched != null)
                    MainWindow.RandomWindow_EpisodeUnwatched = togEpisodeUnwatched.Selected;
                if (togEpisodeWatched != null)
                    MainWindow.RandomWindow_EpisodeWatched = togEpisodeWatched.Selected;

                MainWindow.RandomWindow_CurrentEpisode = null;
                MainWindow.RandomWindow_CurrentSeries = null;
                btnRandom.IsFocused = true;
                LoadData();
            });
            menu.Add(btnSwitchSeries, () =>
            {
                btnSwitchSeries.IsFocused = false;
                btnRandom.IsFocused = true;
                MainWindow.RandomWindow_RandomType = RandomObjectType.Series;
                LoadData();
            });
            menu.Add(btnSwitchEpisode, () =>
            {
                btnSwitchEpisode.IsFocused = false;
                btnRandom.IsFocused = true;
                MainWindow.RandomWindow_RandomType = RandomObjectType.Episode;
                LoadData();
            });
            menu.AddFunc(btnPlayEpisode, () =>
            {
                if (MainWindow.RandomWindow_CurrentEpisode == null) return false;
                MainWindow.vidHandler.ResumeOrPlay(MainWindow.RandomWindow_CurrentEpisode);
                return true;
            });
            menu.AddFunc(btnEpisodeList, () =>
            {
                if (MainWindow.RandomWindow_CurrentSeries == null) return false;
                MainWindow.Breadcrumbs = new List<History>
                {
                    new History {Selected = GroupFilterHelper.AllGroupsFilter}
                };

                // find the group for this series
                VM_AnimeGroup_User grp = ShokoServerHelper.GetGroup(MainWindow.RandomWindow_CurrentSeries.AnimeGroupID);
                if (grp == null)
                {
                    BaseConfig.MyAnimeLog.Write("Group not found");
                    return false;
                }
                MainWindow.Breadcrumbs.Add(new History {Listing = GroupFilterHelper.AllGroupsFilter, Selected = grp});
                MainWindow.Breadcrumbs.Add(new History {Listing = grp, Selected = MainWindow.RandomWindow_CurrentSeries});
                bool foundEpType = false;
                if (MainWindow.RandomWindow_CurrentSeries.EpisodeTypesToDisplay.Count == 1)
                {
                    MainWindow.Breadcrumbs.Add(new History {Listing = MainWindow.RandomWindow_CurrentSeries, Selected = null});
                }
                else
                {
                    foreach (VM_AnimeEpisodeType anEpType in MainWindow.RandomWindow_CurrentSeries.EpisodeTypesToDisplay)
                        if (anEpType.EpisodeType == enEpisodeType.Episode)
                        {
                            MainWindow.Breadcrumbs.Add(new History {Listing = MainWindow.RandomWindow_CurrentSeries, Selected = anEpType});
                            MainWindow.Breadcrumbs.Add(new History {Listing = anEpType, Selected = null});
                            foundEpType = true;
                            break;
                        }

                    if (!foundEpType) return false;
                }
                GUIWindowManager.CloseCurrentWindow();
                GUIWindowManager.ActivateWindow(Constants.WindowIDs.MAIN, false);
                return true;
            });
            if (menu.Check(control))
                return;
            base.OnClicked(controlId, control, actionType);
        }
    }
}