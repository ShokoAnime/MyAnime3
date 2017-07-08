using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MediaPortal.GUI.Library;
using Shoko.Commons.Extensions;
using Shoko.Models.Client;
using Shoko.Models.Enums;
using Shoko.Models.Server;
using Shoko.MyAnime3.Events;
using Shoko.MyAnime3.Extensions;
using Shoko.MyAnime3.ViewModel;
using Shoko.MyAnime3.ViewModel.Helpers;
using Shoko.MyAnime3.ViewModel.Server;
using Action = MediaPortal.GUI.Library.Action;


namespace Shoko.MyAnime3.Windows
{
    public class AnimeInfoWindow : GUIWindow
    {
        [SkinControl(901)] protected GUIButtonControl btnNavLeft = null;
        [SkinControl(902)] protected GUIButtonControl btnNavRight = null;

        [SkinControl(801)] protected GUIButtonControl btnInfoPage = null;
        [SkinControl(802)] protected GUIButtonControl btnStatsPage = null;
        [SkinControl(803)] protected GUIButtonControl btnGroupsPage = null;

        [SkinControl(1500)] protected GUILabelControl dummyPageInfo = null;
        [SkinControl(1501)] protected GUILabelControl dummyPageStatistics = null;
        [SkinControl(1502)] protected GUILabelControl dummyPoster = null;
        [SkinControl(1503)] protected GUILabelControl dummyPageGroups = null;

        [SkinControl(100)] protected GUIVideoControl videoControl = null;

        //[SkinControlAttribute(910)] protected GUIButtonControl btnAnimeInfo = null;
        [SkinControl(911)] protected GUIButtonControl btnAnimeCharacters = null;

        [SkinControl(912)] protected GUIButtonControl btnAnimeRelations = null;
        [SkinControl(913)] protected GUIButtonControl btnAnimeFanart = null;
        [SkinControl(914)] protected GUIButtonControl btnAnimePosters = null;
        [SkinControl(915)] protected GUIButtonControl btnAnimeWideBanners = null;

        [SkinControl(1247)] protected GUILabelControl dummyUserHasVotedSeries = null;


        VM_AniDB_Anime MainAnime;
        VM_AnimeSeries_User serMain;
        readonly string strEpisodeCount = null;
        readonly int iFileCount = 0;
        readonly int iMissingEpisodeCount = 0;
        readonly string strRunningTime = null;
        readonly string strFileSize = null;
        string strAudioLanguages = "";
        string strSubtitleLanguages = "";


        public enum GuiProperty
        {
            AnimeInfo_DownloadStatus,
            AnimeInfo_Title,
            AnimeInfo_PageTitle,
            AnimeInfo_Poster,
            AnimeInfo_Status,
            AnimeInfo_ResultStatus,
            AnimeInfo_Info_AnidbTitle,
            AnimeInfo_Info_EpisodeSpecials,
            AnimeInfo_Info_Year,
            AnimeInfo_Info_Relations,
            AnimeInfo_Info_Rating,
            AnimeInfo_Info_MyRating,
            AnimeInfo_Info_ReviewRating,
            AnimeInfo_Info_ShortTitles,
            AnimeInfo_Info_Type,
            AnimeInfo_Info_OtherTitles,
            AnimeInfo_Info_Genre,
            AnimeInfo_Info_Awards,
            AnimeInfo_Info_CharacterCount,
            AnimeInfo_Info_Restricted,
            AnimeInfo_Info_Description,
            AnimeInfo_Info_Reviews,
            AnimeInfo_Info_GenreShort,
            AnimeInfo_Stats_BlinkedCount,
            AnimeInfo_Stats_StoppedCount,
            AnimeInfo_Stats_PlayedCount,
            AnimeInfo_Stats_WatchedCount,
            AnimeInfo_Stats_FileCount,
            AnimeInfo_Stats_AnimeFileSize,
            AnimeInfo_Stats_MissingEpisodes,
            AnimeInfo_Stats_SubtitleLanguages,
            AnimeInfo_Stats_AudioLanguages,
            AnimeInfo_Stats_TimeSpentWatching,
            AnimeInfo_Stats_RunningTime,
            AnimeInfo_Fanart,
            AnimeInfo_Groups_MyGroupsDescription,
            AnimeInfo_Groups_OtherGroupsDescription
        }

        public void SetGUIProperty(GuiProperty which, string value)
        {
            this.SetGUIProperty(which.ToString(), value);
        }

        public void ClearGUIProperty(GuiProperty which)
        {
            this.ClearGUIProperty(which.ToString());
        }


        public AnimeInfoWindow()
        {
            GetID = Constants.WindowIDs.ANIMEINFO;

            MainWindow.ServerHelper.GotAnimeEvent += ServerHelper_GotAnimeEvent;
            ClearGUIProperty(GuiProperty.AnimeInfo_DownloadStatus);
        }

        void ServerHelper_GotAnimeEvent(GotAnimeEventArgs ev)
        {
            if (GUIWindowManager.ActiveWindow != Constants.WindowIDs.ANIMEINFO) return;
            ClearGUIProperty(GuiProperty.AnimeInfo_DownloadStatus);
            LoadInfo();
            SetSkinProperties();
            InfoPage();
        }

        public override int GetID
        {
            get { return Constants.WindowIDs.ANIMEINFO; }
            set { base.GetID = value; }
        }

        protected override void OnPageLoad()
        {
            base.OnPageLoad();

            BaseConfig.MyAnimeLog.Write("Anime Info Page loaded");

            //reset varibles
            strAudioLanguages = "";
            strSubtitleLanguages = "";

            HideControls();
            LoadInfo();
            LoadFanart();
            LoadGroups();

            SetSkinProperties();
            InfoPage();
            StatsPage();

            dummyPageInfo.Visible = true;
        }

        #region SkinInfo

        private void LoadInfo()
        {
            if (MainWindow.GlobalSeriesID > 0)
            {
                serMain = ShokoServerHelper.GetSeries(MainWindow.GlobalSeriesID);
                if (serMain != null)
                    MainAnime = serMain.Anime;
            }
        }

        private void SetSkinProperties()
        {
            SetGUIProperty(GuiProperty.AnimeInfo_PageTitle, Translation.Information);

            if (serMain != null)
            {
                SetGUIProperty(GuiProperty.AnimeInfo_Poster, ImageAllocator.GetSeriesImageAsFileName(serMain, GUIFacadeControl.Layout.List));
                SetGUIProperty(GuiProperty.AnimeInfo_Title, serMain.SeriesName);
            }
            else
            {
                ClearGUIProperty(GuiProperty.AnimeInfo_Title);
            }
        }

        private void InfoPage()
        {
            BaseConfig.MyAnimeLog.Write("Info Page loaded");

            ClearGUIProperty(GuiProperty.AnimeInfo_Status);
            ClearGUIProperty(GuiProperty.AnimeInfo_ResultStatus);
            ClearGUIProperty(GuiProperty.AnimeInfo_Info_ShortTitles);


            if (dummyUserHasVotedSeries != null) dummyUserHasVotedSeries.Visible = false;
            // Only AniDB users have votes
            if (VM_ShokoServer.Instance.CurrentUser.IsAniDBUserBool())
            {
                string myRating = MainAnime.UserVoteFormatted;
                if (string.IsNullOrEmpty(myRating))
                {
                    ClearGUIProperty(GuiProperty.AnimeInfo_Info_MyRating);
                }
                else
                {
                    SetGUIProperty(GuiProperty.AnimeInfo_Info_MyRating, myRating);
                    if (dummyUserHasVotedSeries != null) dummyUserHasVotedSeries.Visible = true;
                }
            }
            else
            {
                ClearGUIProperty(GuiProperty.AnimeInfo_Info_MyRating);
            }
            SetGUIProperty(GuiProperty.AnimeInfo_Info_AnidbTitle, string.Format("{0} (AID {1})", MainAnime.FormattedTitle, MainAnime.AnimeID));
            SetGUIProperty(GuiProperty.AnimeInfo_Info_EpisodeSpecials, strEpisodeCount);
            SetGUIProperty(GuiProperty.AnimeInfo_Info_Year, FormatTextYear());
            SetGUIProperty(GuiProperty.AnimeInfo_Info_Rating, FormatTextRating());
            SetGUIProperty(GuiProperty.AnimeInfo_Info_Reviews, FormatTextReviews());
            SetGUIProperty(GuiProperty.AnimeInfo_Info_Relations, FormatTextRelations());
            SetGUIProperty(GuiProperty.AnimeInfo_Info_Type, MainAnime.AnimeTypeDescription);
            SetGUIProperty(GuiProperty.AnimeInfo_Info_OtherTitles, FormatTextOtherTitles());
            SetGUIProperty(GuiProperty.AnimeInfo_Info_Genre, MainAnime.TagsFormatted);
            SetGUIProperty(GuiProperty.AnimeInfo_Info_GenreShort, MainAnime.TagsFormattedShort);
            SetGUIProperty(GuiProperty.AnimeInfo_Info_Awards, FormatTextAwards());
            SetGUIProperty(GuiProperty.AnimeInfo_Info_ReviewRating, MainAnime.AvgReviewRating.ToString(Globals.Culture));
            SetGUIProperty(GuiProperty.AnimeInfo_Info_CharacterCount, MainAnime.Characters.Count.ToString(Globals.Culture));

            string eps = MainAnime.EpisodeCountNormal.ToString(Globals.Culture) + " (" + MainAnime.EpisodeCountSpecial.ToString(Globals.Culture) + " " + Translation.Specials + ")";
            SetGUIProperty(GuiProperty.AnimeInfo_Info_EpisodeSpecials, eps);

            SetGUIProperty(GuiProperty.AnimeInfo_Info_Restricted, FormatTextRestricted());
            SetGUIProperty(GuiProperty.AnimeInfo_Info_Description, serMain.Description);

            dummyPoster.Visible = true;
        }


        void StatsPage()
        {
            ClearGUIProperty(GuiProperty.AnimeInfo_Stats_BlinkedCount);

            if (serMain != null)
            {
                SetGUIProperty(GuiProperty.AnimeInfo_Stats_StoppedCount, serMain.StoppedCount.ToString(Globals.Culture));
                SetGUIProperty(GuiProperty.AnimeInfo_Stats_PlayedCount, serMain.PlayedCount.ToString(Globals.Culture));
                SetGUIProperty(GuiProperty.AnimeInfo_Stats_WatchedCount, serMain.WatchedCount.ToString(Globals.Culture));
                SetGUIProperty(GuiProperty.AnimeInfo_Stats_FileCount, iFileCount.ToString(Globals.Culture));
                SetGUIProperty(GuiProperty.AnimeInfo_Stats_AnimeFileSize, strFileSize);
                SetGUIProperty(GuiProperty.AnimeInfo_Stats_MissingEpisodes, iMissingEpisodeCount.ToString(Globals.Culture));
                SetGUIProperty(GuiProperty.AnimeInfo_Stats_SubtitleLanguages, strSubtitleLanguages);
                SetGUIProperty(GuiProperty.AnimeInfo_Stats_AudioLanguages, strAudioLanguages);
                SetGUIProperty(GuiProperty.AnimeInfo_Stats_TimeSpentWatching, FormatTotalWatchTime());
                SetGUIProperty(GuiProperty.AnimeInfo_Stats_RunningTime, strRunningTime);
            }
            else
            {
                ClearGUIProperty(GuiProperty.AnimeInfo_Stats_StoppedCount);
                ClearGUIProperty(GuiProperty.AnimeInfo_Stats_PlayedCount);
                ClearGUIProperty(GuiProperty.AnimeInfo_Stats_WatchedCount);
                ClearGUIProperty(GuiProperty.AnimeInfo_Stats_FileCount);
                ClearGUIProperty(GuiProperty.AnimeInfo_Stats_AnimeFileSize);
                ClearGUIProperty(GuiProperty.AnimeInfo_Stats_MissingEpisodes);
                ClearGUIProperty(GuiProperty.AnimeInfo_Stats_SubtitleLanguages);
                ClearGUIProperty(GuiProperty.AnimeInfo_Stats_AudioLanguages);
                ClearGUIProperty(GuiProperty.AnimeInfo_Stats_RunningTime);
                ClearGUIProperty(GuiProperty.AnimeInfo_Stats_TimeSpentWatching);
            }
        }

        #endregion

        protected override void OnShowContextMenu()
        {
            try
            {
                AniDB_Vote userVote = MainAnime.UserVote;

                ContextMenu cmenu = new ContextMenu(MainAnime.FormattedTitle);

                cmenu.AddAction(Translation.UpdateSeriesInfo, () =>
                {
                    MainWindow.ServerHelper.UpdateAnime(MainAnime.AnimeID);
                    SetGUIProperty(GuiProperty.AnimeInfo_DownloadStatus, Translation.WaitingOnServer + "...");
                });


                if (userVote == null && VM_ShokoServer.Instance.CurrentUser.IsAniDBUserBool())
                {
                    cmenu.AddAction(Translation.PermanentVote, () =>
                    {
                        decimal rating = Utils.PromptAniDBRating(MainAnime.FormattedTitle);
                        if (rating > 0)
                        {
                            VM_ShokoServer.Instance.ShokoServices.VoteAnime(MainAnime.AnimeID, rating, (int) AniDBVoteType.Anime);
                            LoadInfo();
                            SetSkinProperties();
                            InfoPage();
                        }
                    });
                    cmenu.AddAction(Translation.TemporaryVote, () =>
                    {
                        decimal rating = Utils.PromptAniDBRating(MainAnime.FormattedTitle);
                        if (rating > 0)
                        {
                            VM_ShokoServer.Instance.ShokoServices.VoteAnime(MainAnime.AnimeID, rating, (int) AniDBVoteType.AnimeTemp);
                            LoadInfo();
                            SetSkinProperties();
                            InfoPage();
                        }
                    });
                }

                if (userVote != null && Commons.Extensions.Models.IsAniDBUserBool(VM_ShokoServer.Instance.CurrentUser))
                    cmenu.AddAction(Translation.RevokeVote, () =>
                    {
                        VM_ShokoServer.Instance.ShokoServices.VoteAnimeRevoke(MainAnime.AnimeID);
                        LoadInfo();
                        SetSkinProperties();
                        InfoPage();
                    });
                cmenu.Show();
            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write("Error in menu: {0}", ex);
            }
        }

        private void HideControls()
        {
            dummyPageInfo.Visible = false;
            dummyPageStatistics.Visible = false;
            dummyPageGroups.Visible = false;
        }

        protected override void OnPageDestroy(int new_windowId)
        {
            base.OnPageDestroy(new_windowId);
        }


        protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
        {
            MainMenu menu = new MainMenu();
            menu.AddFunc(btnNavLeft, () =>
            {
                GUIWindowManager.CloseCurrentWindow();
                if (serMain.AnimeSeriesID != 0) MainWindow.GlobalSeriesID = serMain.AnimeSeriesID;

                GUIWindowManager.ActivateWindow(Constants.WindowIDs.CHARACTERS, false);
                return true;
            });
            menu.AddFunc(btnNavRight, () =>
            {
                if (serMain.AnimeSeriesID != 0) MainWindow.GlobalSeriesID = serMain.AnimeSeriesID;

                GUIWindowManager.CloseCurrentWindow();
                GUIWindowManager.ActivateWindow(Constants.WindowIDs.RELATIONS, false);
                return true;
            });
            menu.AddFunc(btnInfoPage, () =>
            {
                HideControls();
                dummyPageInfo.Visible = true;
                return true;
            });
            menu.AddFunc(btnStatsPage, () =>
            {
                HideControls();
                dummyPageStatistics.Visible = true;
                return true;
            });
            menu.AddFunc(btnGroupsPage, () =>
            {
                HideControls();
                dummyPageGroups.Visible = true;
                return true;
            });
            if (menu.Check(control))
                return;
            base.OnClicked(controlId, control, actionType);
        }


        private void LoadFanart()
        {
            Fanart fanart = new Fanart(MainAnime);
            if (fanart.FileName.Length > 0)
                SetGUIProperty(GuiProperty.AnimeInfo_Fanart, fanart.FileName);
            else
                ClearGUIProperty(GuiProperty.AnimeInfo_Fanart);
        }


        public override bool Init()
        {
            return this.InitSkin<GuiProperty>("Anime3_AnimeInfo.xml");
        }


        private string FormatTextYear()
        {
            return MainAnime.AirDateAsString;
        }

        private string FormatTextRating()
        {
            string rat = string.Format("{0} (" + Translation.Votes + " {1}) / {2} (" + Translation.Votes + " {3})",
                MainAnime.Rating.ToString(Globals.Culture).Insert(1, "."), MainAnime.VoteCount,
                MainAnime.TempRating.ToString(Globals.Culture).Insert(1, "."), MainAnime.TempVoteCount);
            return rat;
        }

        private string FormatTextReviews()
        {
            return MainAnime.ReviewCount.ToString(Globals.Culture) + " (" + Translation.AverageRating + ": " + MainAnime.AvgReviewRating.ToString(Globals.Culture).Insert(1, ".") + ")";
        }

        private string FormatTextOtherTitles()
        {
            return string.Join(" **", MainAnime.GetAllTitles().ToList().Take(4));
        }

        private void AddTimePart(StringBuilder bld, int value, string singlecaption, string pluralcaption)
        {
            if (value > 0)
            {
                bld.Append(value.ToString(Globals.Culture));
                bld.Append(" ");
                bld.Append(value == 1 ? singlecaption : pluralcaption);
                bld.Append(", ");
            }
        }

        public string FormatTotalWatchTime()
        {
            long secs = 0;
            foreach (VM_AnimeEpisode_User ep in serMain.AllEpisodes)
                if (ep.Watched)
                    secs += ep.AniDB_LengthSeconds;
            int days = (int) (secs / 86400);
            int hours = (int) (secs / 3600 % 24);
            int minutes = (int) (secs / 60 % 60);
            int seconds = (int) (secs % 60);
            StringBuilder bld = new StringBuilder();
            AddTimePart(bld, days, Translation.Day, Translation.Days);
            AddTimePart(bld, hours, Translation.Hour, Translation.Hours);
            AddTimePart(bld, minutes, Translation.Minute, Translation.Minutes);
            AddTimePart(bld, seconds, Translation.Second, Translation.Seconds);
            if (bld.Length > 1)
                bld.Remove(bld.Length - 2, 2);
            return bld.ToString();
        }

        private void AddRelation(StringBuilder bld, string title, int value)
        {
            if (value > 0)
            {
                bld.Append(title);
                bld.Append(": ");
                bld.Append(value.ToString(Globals.Culture));
                bld.Append(" ");
            }
        }

        private string FormatTextRelations()
        {
            int sequel = 0;
            int prequel = 0;
            int samesetting = 0;
            int alternatesetting = 0;
            int commoncharacters = 0;
            int sidestory = 0;
            int parentstory = 0;
            int summary = 0;
            int fullstory = 0;
            int other = 0;

            StringBuilder bld = new StringBuilder();
            try
            {
                List<CL_AniDB_Anime_Relation> relatedAnimes = MainAnime.RelatedAnimeLinks;
                if (relatedAnimes.Count > 0)
                {
                    foreach (CL_AniDB_Anime_Relation relAnime in relatedAnimes)
                        switch (relAnime.AniDB_Anime_RelationID)
                        {
                            case 1:
                                sequel++;
                                break;
                            case 2:
                                prequel++;
                                break;
                            case 11:
                            case 12:
                                samesetting++;
                                break;
                            case 21:
                            case 22:
                            case 31:
                            case 32:
                                alternatesetting++;
                                break;
                            case 41:
                            case 42:
                                commoncharacters++;
                                break;
                            case 51:
                                sidestory++;
                                break;
                            case 52:
                                parentstory++;
                                break;
                            case 61:
                                summary++;
                                break;
                            case 62:
                                fullstory++;
                                break;
                            case 100:
                                other++;
                                break;
                        }
                    AddRelation(bld, Translation.Sequels, sequel);
                    AddRelation(bld, Translation.Prequels, prequel);
                    AddRelation(bld, Translation.SameSetting, samesetting);
                    AddRelation(bld, Translation.AlternateSetting, alternatesetting);
                    AddRelation(bld, Translation.CommonCharacters, commoncharacters);
                    AddRelation(bld, Translation.SideStory, sidestory);
                    AddRelation(bld, Translation.ParentStory, parentstory);
                    AddRelation(bld, Translation.Summary, summary);
                    AddRelation(bld, Translation.FullStory, fullstory);
                    AddRelation(bld, Translation.Other, other);
                    if (bld.Length > 0)
                        bld.Remove(bld.Length - 1, 1);
                    return bld.ToString();
                }
                return Translation.None;
            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write(ex.ToString());
                return Translation.Error;
            }
        }

        private string FormatTextAwards()
        {
            string formatText = MainAnime.AwardList;
            if (formatText == "")
                formatText = Translation.NoAwards;

            return formatText;
        }

        private string FormatTextRestricted()
        {
            return MainAnime.Restricted == 0 ? Translation.No : Translation.Yes;
        }


        private void LoadGroups()
        {
            string mygroupsData = "";
            string othergroupsData = "";

            List<CL_AniDB_GroupStatus> allGrps = ShokoServerHelper.GetReleaseGroupsForAnime(MainAnime.AnimeID);

            List<CL_AniDB_GroupStatus> mygrps = new List<CL_AniDB_GroupStatus>();
            List<CL_AniDB_GroupStatus> othergrps = new List<CL_AniDB_GroupStatus>();

            foreach (CL_AniDB_GroupStatus grp in allGrps)
                if (grp.UserCollecting)
                    mygrps.Add(grp);
                else
                    othergrps.Add(grp);

            if (mygrps.Count > 0)
            {
                mygrps = mygrps.OrderByDescending(a => a.FileCount).ToList();

                foreach (CL_AniDB_GroupStatus grp in mygrps)
                {
                    mygroupsData += string.Format("{0} - ({1} {2})", grp.GroupName, grp.FileCount, Translation.LocalFiles);
                    mygroupsData += Environment.NewLine;
                }
            }
            else
            {
                mygroupsData = "-";
            }

            SetGUIProperty(GuiProperty.AnimeInfo_Groups_MyGroupsDescription, mygroupsData);


            if (othergrps.Count > 0)
            {
                // now sort the groups by name
                othergrps = othergrps.OrderBy(a => a.GroupName).ToList();

                foreach (CL_AniDB_GroupStatus grp in othergrps)
                {
                    othergroupsData += string.Format("{0} - {2}: {1}", grp.GroupName, grp.EpisodeRange, Translation.EpisodeRange);
                    othergroupsData += Environment.NewLine;
                }
            }
            else
            {
                othergroupsData = "-";
            }

            SetGUIProperty(GuiProperty.AnimeInfo_Groups_OtherGroupsDescription, othergroupsData);
        }
    }
}