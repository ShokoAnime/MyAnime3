using System;
using System.Collections.Generic;
using System.Linq;
using Shoko.Models.Client;
using Shoko.Models.Enums;
using Shoko.Models.Server;
using Shoko.MyAnime3.ViewModel.Helpers;

namespace Shoko.MyAnime3.ViewModel.Server
{
    public class VM_AnimeSeries_User : CL_AnimeSeries_User, IVM
    {
        public VM_AniDB_Anime Anime => (VM_AniDB_Anime) AniDBAnime.AniDBAnime;


        #region Sorting properties

        // These properties are used when sorting group filters, and must match the names on the AnimeGroupVM

        public decimal AniDBRating
        {
            get
            {
                try
                {
                    return Anime.AniDBRating;
                }
                catch
                {
                    return 0;
                }
            }
        }

        public DateTime? Stat_AirDate_Min
        {
            get
            {
                try
                {
                    return Anime.AirDate;
                }
                catch
                {
                    return null;
                }
            }
        }

        public DateTime? Stat_AirDate_Max
        {
            get
            {
                try
                {
                    return Anime.AirDate;
                }
                catch
                {
                    return null;
                }
            }
        }


        public string SortName => SeriesName;

        public string GroupName => SeriesName;


        public DateTime? Stat_SeriesCreatedDate => DateTimeCreated;

        public decimal? Stat_UserVoteOverall
        {
            get
            {
                AniDB_Vote vote = Anime.UserVote;
                if (vote == null) return 0;

                return vote.VoteValue;
            }
        }

        public int AllSeriesCount => 1;

        #endregion


        public DateTime? AirDate => Anime.AirDate;


        public bool IsComplete
        {
            get
            {
                if (!Anime.EndDate.HasValue) return false; // ongoing

                // all series have finished airing and the user has all the episodes
                if (Anime.EndDate.Value < DateTime.Now && !HasMissingEpisodesAny) return true;

                return false;
            }
        }

        public bool FinishedAiring
        {
            get
            {
                if (!Anime.EndDate.HasValue) return false; // ongoing

                // all series have finished airing
                if (Anime.EndDate.Value < DateTime.Now) return true;

                return false;
            }
        }

        public bool AllFilesWatched => UnwatchedEpisodeCount == 0;

        public bool AnyFilesWatched => WatchedEpisodeCount > 0;


        /*public decimal? Stat_UserVoteOverall
        {
            get
            {
                return AniDB_Anime.Detail.UserRating;
            }
        }*/

        public bool HasMissingEpisodesAny => MissingEpisodeCount > 0 || MissingEpisodeCountGroups > 0;

        public bool HasMissingEpisodesAllDifferentToGroups => MissingEpisodeCount > 0 && MissingEpisodeCount != MissingEpisodeCountGroups;

        public bool HasMissingEpisodesGroups => MissingEpisodeCountGroups > 0;


        public string PosterPath => Anime.DefaultPosterPath;

        public string SeriesName
        {
            get
            {
                if (!string.IsNullOrEmpty(SeriesNameOverride))
                    return SeriesNameOverride;

                if (VM_ShokoServer.Instance.SeriesNameSource == DataSourceType.AniDB)
                    return Anime.FormattedTitle;

                if (TvDB_Series != null && TvDB_Series.Count > 0 && !string.IsNullOrEmpty(TvDB_Series[0].SeriesName) &&
                    !TvDB_Series[0].SeriesName.ToUpper().Contains("**DUPLICATE"))
                    return TvDB_Series[0].SeriesName;
                return Anime.FormattedTitle;
            }
        }

        public string Description
        {
            get
            {
                if (VM_ShokoServer.Instance.SeriesDescriptionSource == DataSourceType.AniDB)
                    return Anime.ParsedDescription;

                if (TvDB_Series != null && TvDB_Series.Count > 0 && !string.IsNullOrEmpty(TvDB_Series[0].Overview))
                    return TvDB_Series[0].Overview;
                return Anime.ParsedDescription;
            }
        }

        private List<VM_AnimeEpisode_User> allEpisodes;

        public List<VM_AnimeEpisode_User> AllEpisodes
        {
            get
            {
                if (allEpisodes == null)
                    RefreshEpisodes();
                return allEpisodes;
            }
        }

        public List<VM_AnimeEpisode_User> GetEpisodesByType(enEpisodeType epType)
        {
            List<VM_AnimeEpisode_User> eps = new List<VM_AnimeEpisode_User>();

            foreach (VM_AnimeEpisode_User ep in AllEpisodes)
                if (ep.EpisodeTypeEnum == epType)
                    eps.Add(ep);

            return eps;
        }

        public List<VM_AnimeEpisode_User> GetEpisodesToDisplay(enEpisodeType epType)
        {
            List<VM_AnimeEpisode_User> eps = new List<VM_AnimeEpisode_User>();

            foreach (VM_AnimeEpisode_User ep in GetEpisodesByType(epType))
            {
                bool useEp = true;
                if (BaseConfig.Settings.ShowOnlyAvailableEpisodes && ep.LocalFileCount == 0)
                    useEp = false;

                if (useEp) eps.Add(ep);
            }

            return eps;
        }

        public void GetWatchedUnwatchedCount(enEpisodeType epType, ref int unwatched, ref int watched)
        {
            unwatched = 0;
            watched = 0;

            foreach (VM_AnimeEpisode_User ep in GetEpisodesByType(epType))
            {
                if (ep.LocalFileCount == 0) continue;

                if (ep.Watched)
                    watched++;
                else
                    unwatched++;
            }
        }

        public void RefreshEpisodes()
        {
            allEpisodes = new List<VM_AnimeEpisode_User>();

            try
            {
                TvDBSummary summ = Anime.TvSummary;

                // Normal episodes
                foreach (VM_AnimeEpisode_User ep in ShokoServerHelper.GetEpisodesForSeries(AnimeSeriesID))
                {
                    ep.SetTvDBInfo(summ);
                    allEpisodes.Add(ep);
                }
                allEpisodes = allEpisodes.OrderBy(a => a.EpisodeType).ThenBy(a => a.EpisodeNumber).ToList();
            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write(ex.ToString());
            }
        }

        public List<VM_AnimeEpisodeType> EpisodeTypesToDisplay
        {
            get
            {
                List<VM_AnimeEpisodeType> epTypes = new List<VM_AnimeEpisodeType>();

                try
                {
                    foreach (VM_AnimeEpisode_User ep in AllEpisodes)
                    {
                        if (BaseConfig.Settings.ShowOnlyAvailableEpisodes && ep.LocalFileCount == 0) continue;

                        VM_AnimeEpisodeType epType = new VM_AnimeEpisodeType(this, ep);

                        bool alreadyAdded = false;
                        foreach (VM_AnimeEpisodeType thisEpType in epTypes)
                            if (thisEpType.EpisodeType == epType.EpisodeType)
                            {
                                alreadyAdded = true;
                                break;
                            }
                        if (!alreadyAdded)
                            epTypes.Add(epType);
                    }
                    epTypes = epTypes.OrderBy(a => a.EpisodeType).ToList();
                }
                catch (Exception ex)
                {
                    BaseConfig.MyAnimeLog.Write(ex.ToString());
                }
                return epTypes;
            }
        }

        public List<VM_AnimeEpisodeType> EpisodeTypes
        {
            get
            {
                List<VM_AnimeEpisodeType> epTypes = new List<VM_AnimeEpisodeType>();

                try
                {
                    foreach (VM_AnimeEpisode_User ep in AllEpisodes)
                    {
                        VM_AnimeEpisodeType epType = new VM_AnimeEpisodeType(this, ep);

                        bool alreadyAdded = false;
                        foreach (VM_AnimeEpisodeType thisEpType in epTypes)
                            if (thisEpType.EpisodeType == epType.EpisodeType)
                            {
                                alreadyAdded = true;
                                break;
                            }
                        if (!alreadyAdded)
                            epTypes.Add(epType);
                    }
                    epTypes = epTypes.OrderBy(a => a.EpisodeType).ToList();
                }
                catch (Exception ex)
                {
                    BaseConfig.MyAnimeLog.Write(ex.ToString());
                }
                return epTypes;
            }
        }

        public VM_AnimeSeries_User()
        {
            CrossRefAniDBTvDBV2 = new List<CrossRef_AniDB_TvDBV2>();
            TvDB_Series = new List<TvDB_Series>();
        }

        public override string ToString()
        {
            return $"ANIME SERIES: {AnimeSeriesID} - {AniDB_ID}";
        }

        public void Populate(VM_AnimeSeries_User contract)
        {
            AnimeGroupID = contract.AnimeGroupID;
            AniDB_ID = contract.AniDB_ID;
            DateTimeUpdated = contract.DateTimeUpdated;
            DateTimeCreated = contract.DateTimeCreated;
            DefaultAudioLanguage = contract.DefaultAudioLanguage;
            DefaultSubtitleLanguage = contract.DefaultSubtitleLanguage;
            EpisodeAddedDate = contract.EpisodeAddedDate;
            LatestEpisodeAirDate = contract.LatestEpisodeAirDate;
            LatestLocalEpisodeNumber = contract.LatestLocalEpisodeNumber;
            SeriesNameOverride = contract.SeriesNameOverride;
            DefaultFolder = contract.DefaultFolder;
            MissingEpisodeCount = contract.MissingEpisodeCount;
            MissingEpisodeCountGroups = contract.MissingEpisodeCountGroups;
            AniDBAnime = contract.AniDBAnime;
            CrossRefAniDBTvDBV2 = contract.CrossRefAniDBTvDBV2;
            CrossRefAniDBMovieDB = contract.CrossRefAniDBMovieDB;
            CrossRefAniDBMAL = contract.CrossRefAniDBMAL;
            TvDB_Series = contract.TvDB_Series;
            MovieDB_Movie = contract.MovieDB_Movie;
            TopLevelGroup = contract.TopLevelGroup;
        }


        public bool Save()
        {
            try
            {
                CL_Response<CL_AnimeSeries_User> response = VM_ShokoServer.Instance.ShokoServices.SaveSeries(ToContract(),
                    VM_ShokoServer.Instance.CurrentUser.JMMUserID);
                if (!string.IsNullOrEmpty(response.ErrorMessage))
                    return false;
                Populate((VM_AnimeSeries_User) response.Result);
                return true;
            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write(ex.ToString());
                return false;
            }
        }

        public CL_AnimeSeries_Save_Request ToContract()
        {
            CL_AnimeSeries_Save_Request contract = new CL_AnimeSeries_Save_Request();
            contract.AniDB_ID = AniDB_ID;
            contract.AnimeGroupID = AnimeGroupID;
            contract.AnimeSeriesID = AnimeSeriesID;
            contract.DefaultAudioLanguage = DefaultAudioLanguage;
            contract.DefaultSubtitleLanguage = DefaultSubtitleLanguage;

            return contract;
        }
    }
}