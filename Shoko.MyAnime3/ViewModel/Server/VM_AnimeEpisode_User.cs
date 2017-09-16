using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Shoko.Commons.Extensions;
using Shoko.Models.Client;
using Shoko.Models.Enums;
using Shoko.Models.Server;
using Shoko.MyAnime3.ConfigFiles;
using Shoko.MyAnime3.Extensions;
using Shoko.MyAnime3.ViewModel.Helpers;

namespace Shoko.MyAnime3.ViewModel.Server
{
    public class VM_AnimeEpisode_User : CL_AnimeEpisode_User, IVM
    {
        public string EpisodeOverview { get; set; }
        public string EpisodeImageLocation { get; set; }


        private VM_AnimeSeries_User animeSeries;

        public VM_AnimeSeries_User AnimeSeries
        {
            get
            {
                if (animeSeries == null)
                    animeSeries = ShokoServerHelper.GetSeries(AnimeSeriesID);

                return animeSeries;
            }
        }

        public EpisodeType EpisodeTypeEnum => (EpisodeType) EpisodeType;

        public string DefaultAudioLanguage
        {
            get
            {
                if (AnimeSeries == null) return string.Empty;
                return AnimeSeries.DefaultAudioLanguage;
            }
        }

        public string DefaultSubtitleLanguage
        {
            get
            {
                if (AnimeSeries == null) return string.Empty;
                return AnimeSeries.DefaultSubtitleLanguage;
            }
        }

        public bool Watched => WatchedDate.HasValue;

        public bool MultipleUnwatchedEpsSeries => UnwatchedEpCountSeries > 1;

        public string RunTime => Utils.FormatSecondsToDisplayTime(AniDB_LengthSeconds);

        private string _epname;

        public string EpisodeName
        {
            get
            {
                if (_epname != null)
                    return _epname;
                if (AniDB_EnglishName.Trim().Length > 0)
                    return AniDB_EnglishName;
                return AniDB_RomajiName;
            }
        }

        public string EpisodeNumberAndName => $"{EpisodeNumber} - {EpisodeName}";

        public string EpisodeNumberAndNameWithType => $"{ShortType}{EpisodeNumber} - {EpisodeName}";

        public string EpisodeTypeAndNumber => $"{ShortType}{EpisodeNumber}";

        public string EpisodeTypeAndNumberAbsolute => $"{ShortType}{EpisodeNumber.ToString().PadLeft(5, '0')}";

        public string ShortType
        {
            get
            {
                string shortType = string.Empty;
                switch (EpisodeTypeEnum)
                {
                    case Shoko.Models.Enums.EpisodeType.Credits:
                        shortType = "C";
                        break;
                    case Shoko.Models.Enums.EpisodeType.Episode:
                        shortType = "E";
                        break;
                    case Shoko.Models.Enums.EpisodeType.Other:
                        shortType = "O";
                        break;
                    case Shoko.Models.Enums.EpisodeType.Parody:
                        shortType = "P";
                        break;
                    case Shoko.Models.Enums.EpisodeType.Special:
                        shortType = "S";
                        break;
                    case Shoko.Models.Enums.EpisodeType.Trailer:
                        shortType = "T";
                        break;
                }
                return shortType;
            }
        }

        public string AirDateAsString
        {
            get
            {
                if (AniDB_AirDate.HasValue)
                    return AniDB_AirDate.Value.ToString("dd MMM yyyy", Globals.Culture);
                return "";
            }
        }

        public string AniDBRatingFormatted => $"{"Rating"}: {AniDB_Rating} ({AniDB_Votes} {"Votes"})";

        public bool FutureDated
        {
            get
            {
                if (!AniDB_AirDate.HasValue) return true;

                return AniDB_AirDate.Value > DateTime.Now;
            }
        }


        public void SetTvDBInfo(TvDBSummary tvSummary)
        {
            EpisodeOverview = Translation.EpisodeOverviewNA;
            EpisodeImageLocation = "";

            #region episode override

            // check if this episode has a direct tvdb over-ride
            if (tvSummary.DictTvDBCrossRefEpisodes.ContainsKey(AniDB_EpisodeID))
                foreach (TvDB_Episode tvep in tvSummary.DictTvDBEpisodes.Values)
                    if (tvSummary.DictTvDBCrossRefEpisodes[AniDB_EpisodeID] == tvep.Id)
                    {
                        if (string.IsNullOrEmpty(tvep.Overview))
                            EpisodeOverview = Translation.EpisodeOverviewNA;
                        else
                            EpisodeOverview = tvep.Overview;

                        if (string.IsNullOrEmpty(tvep.GetFullImagePath()) || !File.Exists(tvep.GetFullImagePath()))
                            EpisodeImageLocation = string.IsNullOrEmpty(tvep.GetOnlineImagePath()) ? @"/Images/EpisodeThumb_NotFound.png" : tvep.GetOnlineImagePath();
                        else
                            EpisodeImageLocation = tvep.GetFullImagePath();

                        if (VM_ShokoServer.Instance.EpisodeTitleSource == DataSourceType.TheTvDB && !string.IsNullOrEmpty(tvep.EpisodeName))
                            _epname = tvep.EpisodeName;

                        return;
                    }

            #endregion

            #region normal episodes

            // now do stuff to improve performance
            if (EpisodeTypeEnum == Shoko.Models.Enums.EpisodeType.Episode)
                if (tvSummary.CrossRefTvDBV2 != null && tvSummary.CrossRefTvDBV2.Count > 0)
                {
                    // find the xref that is right
                    // relies on the xref's being sorted by season number and then episode number (desc)
                    List<CrossRef_AniDB_TvDBV2> tvDBCrossRef = tvSummary.CrossRefTvDBV2.OrderByDescending(a => a.AniDBStartEpisodeNumber).ToList();
                    bool foundStartingPoint = false;
                    CrossRef_AniDB_TvDBV2 xrefBase = null;
                    foreach (CrossRef_AniDB_TvDBV2 xrefTV in tvDBCrossRef)
                    {
                        if (xrefTV.AniDBStartEpisodeType != (int)Shoko.Models.Enums.EpisodeType.Episode) continue;
                        if (EpisodeNumber >= xrefTV.AniDBStartEpisodeNumber)
                        {
                            foundStartingPoint = true;
                            xrefBase = xrefTV;
                            break;
                        }
                    }

                    // we have found the starting epiosde numbder from AniDB
                    // now let's check that the TvDB Season and Episode Number exist
                    if (foundStartingPoint)
                    {
                        Dictionary<int, int> dictTvDBSeasons = null;
                        Dictionary<int, TvDB_Episode> dictTvDBEpisodes = null;
                        foreach (TvDBDetails det in tvSummary.TvDetails.Values)
                            if (det.TvDBID == xrefBase.TvDBID)
                            {
                                dictTvDBSeasons = det.DictTvDBSeasons;
                                dictTvDBEpisodes = det.DictTvDBEpisodes;
                                break;
                            }
                        if (dictTvDBEpisodes != null && dictTvDBSeasons != null)
                        {
                            if (dictTvDBSeasons.ContainsKey(xrefBase.TvDBSeasonNumber))
                            {
                                int episodeNumber = dictTvDBSeasons[xrefBase.TvDBSeasonNumber] + (EpisodeNumber + xrefBase.TvDBStartEpisodeNumber - 2) - (xrefBase.AniDBStartEpisodeNumber - 1);
                                if (dictTvDBEpisodes.ContainsKey(episodeNumber))
                                {
                                    TvDB_Episode tvep = dictTvDBEpisodes[episodeNumber];
                                    if (string.IsNullOrEmpty(tvep.Overview))
                                        EpisodeOverview = "Episode Overview Not Available";
                                    else
                                        EpisodeOverview = tvep.Overview;

                                    if (string.IsNullOrEmpty(tvep.GetFullImagePath()) || !File.Exists(tvep.GetFullImagePath()))
                                        if (string.IsNullOrEmpty(tvep.GetOnlineImagePath()))
                                            EpisodeImageLocation = @"/Images/EpisodeThumb_NotFound.png";
                                        else
                                            EpisodeImageLocation = tvep.GetOnlineImagePath();
                                    else
                                        EpisodeImageLocation = tvep.GetFullImagePath();

                                    if (VM_ShokoServer.Instance.EpisodeTitleSource == DataSourceType.TheTvDB && !string.IsNullOrEmpty(tvep.EpisodeName))
                                        _epname = tvep.EpisodeName;
                                }
                            }
                        }
                    }
                }

            #endregion


            #region special episodes

            if ((EpisodeTypeEnum == Shoko.Models.Enums.EpisodeType.Special) && tvSummary.CrossRefTvDBV2!=null)
            {
                // find the xref that is right
                // relies on the xref's being sorted by season number and then episode number (desc)
               
                List<CrossRef_AniDB_TvDBV2> tvDBCrossRef = tvSummary.CrossRefTvDBV2.OrderByDescending(a => a.AniDBStartEpisodeNumber).ToList();

                bool foundStartingPoint = false;
                CrossRef_AniDB_TvDBV2 xrefBase = null;
                foreach (CrossRef_AniDB_TvDBV2 xrefTV in tvDBCrossRef)
                {
                    if (xrefTV.AniDBStartEpisodeType != (int)Shoko.Models.Enums.EpisodeType.Special) continue;
                    if (EpisodeNumber >= xrefTV.AniDBStartEpisodeNumber)
                    {
                        foundStartingPoint = true;
                        xrefBase = xrefTV;
                        break;
                    }
                }

                if (tvSummary.CrossRefTvDBV2 != null && tvSummary.CrossRefTvDBV2.Count > 0)
                    if (foundStartingPoint)
                    {
                        Dictionary<int, int> dictTvDBSeasons = null;
                        Dictionary<int, TvDB_Episode> dictTvDBEpisodes = null;
                        foreach (TvDBDetails det in tvSummary.TvDetails.Values)
                            if (det.TvDBID == xrefBase.TvDBID)
                            {
                                dictTvDBSeasons = det.DictTvDBSeasons;
                                dictTvDBEpisodes = det.DictTvDBEpisodes;
                                break;
                            }
                        if (dictTvDBEpisodes != null && dictTvDBSeasons != null)
                        {
                            if (dictTvDBSeasons.ContainsKey(xrefBase.TvDBSeasonNumber))
                            {
                                int episodeNumber = dictTvDBSeasons[xrefBase.TvDBSeasonNumber] + (EpisodeNumber + xrefBase.TvDBStartEpisodeNumber - 2) - (xrefBase.AniDBStartEpisodeNumber - 1);
                                if (dictTvDBEpisodes.ContainsKey(episodeNumber))
                                {
                                    TvDB_Episode tvep = dictTvDBEpisodes[episodeNumber];
                                    EpisodeOverview = tvep.Overview;

                                    if (string.IsNullOrEmpty(tvep.GetFullImagePath()) || !File.Exists(tvep.GetFullImagePath()))
                                        if (string.IsNullOrEmpty(tvep.GetOnlineImagePath()))
                                            EpisodeImageLocation = @"/Images/EpisodeThumb_NotFound.png";
                                        else
                                            EpisodeImageLocation = tvep.GetOnlineImagePath();
                                    else
                                        EpisodeImageLocation = tvep.GetFullImagePath();

                                    if (VM_ShokoServer.Instance.EpisodeTitleSource == DataSourceType.TheTvDB && !string.IsNullOrEmpty(tvep.EpisodeName))
                                        _epname = tvep.EpisodeName;
                                }
                            }
                        }
                    }
            }

            #endregion
        }

        public void RefreshFilesForEpisode()
        {
            try
            {
                filesForEpisode = VM_ShokoServer.Instance.ShokoServices.GetFilesForEpisode(AnimeEpisodeID,
                    VM_ShokoServer.Instance.CurrentUser.JMMUserID).CastList<VM_VideoDetailed>();
            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write(ex.ToString());
            }
        }

        private List<VM_VideoDetailed> filesForEpisode;

        public List<VM_VideoDetailed> FilesForEpisode
        {
            get
            {
                if (filesForEpisode == null)
                    RefreshFilesForEpisode();
                return filesForEpisode;
            }
        }

        public string DisplayName
        {
            get
            {
                AnimePluginSettings settings = AnimePluginSettings.Instance;
                string newName = settings.EpisodeDisplayFormat;

                if (newName.Contains(Constants.EpisodeDisplayString.EpisodeNumber))
                    newName = newName.Replace(Constants.EpisodeDisplayString.EpisodeNumber, EpisodeNumber.ToString());

                if (newName.Contains(Constants.EpisodeDisplayString.EpisodeName))
                    newName = newName.Replace(Constants.EpisodeDisplayString.EpisodeName, EpisodeName);


                return newName;
            }
        }

        public void ToggleWatchedStatus(bool watched)
        {
            ToggleWatchedStatus(watched, true);
        }

        public void ToggleWatchedStatus(bool watched, bool promptForRating)
        {
            bool currentStatus = Watched;
            if (currentStatus == watched) return;

            CL_Response<CL_AnimeEpisode_User> response = VM_ShokoServer.Instance.ShokoServices.ToggleWatchedStatusOnEpisode(AnimeEpisodeID, watched,
                VM_ShokoServer.Instance.CurrentUser.JMMUserID);
            if (!string.IsNullOrEmpty(response.ErrorMessage))
            {
                BaseConfig.MyAnimeLog.Write("Error in ToggleWatchedStatus: " + response.ErrorMessage);
                return;
            }

            if (promptForRating && BaseConfig.Settings.DisplayRatingDialogOnCompletion)
            {
                VM_AnimeSeries_User ser = (VM_AnimeSeries_User) VM_ShokoServer.Instance.ShokoServices.GetSeries(response.Result.AnimeSeriesID,
                    VM_ShokoServer.Instance.CurrentUser.JMMUserID);
                if (ser != null)
                    Utils.PromptToRateSeriesOnCompletion(ser);
            }
        }

        public void IncrementEpisodeStats(StatCountType statCountType)
        {
            VM_ShokoServer.Instance.ShokoServices.IncrementEpisodeStats(AnimeEpisodeID, VM_ShokoServer.Instance.CurrentUser.JMMUserID,
                (int) statCountType);
        }
    }
}