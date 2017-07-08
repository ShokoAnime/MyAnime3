using System;
using System.Collections.Generic;
using System.Linq;
using Shoko.Models.Client;

using Shoko.MyAnime3.ViewModel.Helpers;

namespace Shoko.MyAnime3.ViewModel.Server
{
    public class VM_AnimeGroup_User : CL_AnimeGroup_User, IComparable<VM_AnimeGroup_User>, IVM
    {
        public int CompareTo(VM_AnimeGroup_User obj)
        {
            return String.Compare(SortName, obj.SortName, StringComparison.Ordinal);
        }

        public override string ToString()
        {
            return $"{AnimeGroupID} - {GroupName}";
        }


        public decimal AniDBTotalRating
        {
            get
            {
                try
                {
                    decimal totalRating = 0;
                    foreach (VM_AnimeSeries_User series in AllSeries)
                    {
                        totalRating += (decimal) series.Anime.Rating * series.Anime.VoteCount;
                        totalRating += (decimal) series.Anime.TempRating * series.Anime.TempVoteCount;
                    }

                    return totalRating;
                }
                catch
                {
                    return 0;
                }
            }
        }
        public string TagsFormatted => string.Join(", ", Stat_AllTags);

        public string TagsFormattedShort => string.Join(", ", Stat_AllTags.Take(6));


        public int AniDBTotalVotes
        {
            get
            {
                try
                {
                    int cnt = 0;
                    foreach (VM_AnimeSeries_User series in AllSeries)
                        cnt += series.Anime.AniDBTotalVotes;

                    return cnt;
                }
                catch
                {
                    return 0;
                }
            }
        }

        public decimal AniDBRating
        {
            get
            {
                try
                {
                    /*if (AniDBTotalVotes == 0)
                        return 0;
                    else
                        return AniDBTotalRating / (decimal)AniDBTotalVotes / (decimal)100;*/

                    return Stat_AniDBRating / 100;
                }
                catch
                {
                    return 0;
                }
            }
        }

        public VM_AnimeSeries_User DefaultSeries
        {
            get
            {
                if (!DefaultAnimeSeriesID.HasValue) return null;
                return ShokoServerHelper.GetSeries(DefaultAnimeSeriesID.Value);
            }
        }

        public List<VM_AnimeGroup_User> SubGroups => ShokoServerHelper.GetSubGroupsForGroup(this);

        public VM_AnimeGroup_User ParentGroup
        {
            get
            {
                if (!AnimeGroupParentID.HasValue) return null;

                return ShokoServerHelper.GetGroup(AnimeGroupParentID.Value);
            }
        }

        public List<VM_AnimeSeries_User> ChildSeries => ShokoServerHelper.GetAnimeSeriesForGroup(this);

        public List<VM_AnimeSeries_User> AllSeries => ShokoServerHelper.GetAnimeSeriesForGroupRecursive(this);

        public string YearFormatted
        {
            get
            {
                if (!Stat_AirDate_Min.HasValue) return "";


                int beginYear = Stat_AirDate_Min.Value.Year;
                int endYear = Stat_EndDate?.Year ?? 0;

                string ret = beginYear.ToString();

                if (beginYear != endYear)
                    if (endYear <= 0)
                        ret += "-Ongoing";
                    else
                        ret += "-" + endYear;

                return ret;
            }
        }

        public int AllSeriesCount => Stat_SeriesCount;

        public string ParsedDescription
        {
            get
            {
                string desc = Description;
                if (DefaultAnimeSeriesID.HasValue)
                {
                    VM_AnimeSeries_User ser = DefaultSeries;
                    if (ser != null)
                    {
                        VM_AniDB_Anime anime = ser.Anime;
                        desc = anime.Description;
                    }
                }

                return Utils.ReparseDescription(desc);
            }
            set => Description = value;
        }

        /*public List<string> AnimeTypesList
        {
            get
            {
                List<string> atypeList = new List<string>();
                foreach (AnimeSeriesVM series in AllAnimeSeries)
                {
                    string atype = series.AniDB_Anime.AnimeTypeDescription;
                    if (!atypeList.Contains(atype)) atypeList.Add(atype);
                }
                return atypeList;
            }
        }

        public string AnimeTypesString
        {
            get
            {
                string atypesString = "";
                foreach (string atype in AnimeTypesList)
                {
                    if (!string.IsNullOrEmpty(atypesString))
                        atypesString += ", ";
                    atypesString += atype;
                }
                return atypesString;
            }
        }*/

        public bool HasUnwatchedFiles => UnwatchedEpisodeCount > 0;

        public bool AllFilesWatched => UnwatchedEpisodeCount == 0;

        public bool AnyFilesWatched => WatchedEpisodeCount > 0;

        public bool HasMissingEpisodesAny => MissingEpisodeCount > 0 || MissingEpisodeCountGroups > 0;

        public bool HasMissingEpisodesAllDifferentToGroups => MissingEpisodeCount > 0 && MissingEpisodeCount != MissingEpisodeCountGroups;

        public bool HasMissingEpisodesGroups => MissingEpisodeCountGroups > 0;

        public bool HasMissingEpisodes => MissingEpisodeCountGroups > 0;

        /*
        public decimal AniDBTotalRating
        {
            get
            {
                try
                {
                    decimal totalRating = 0;
                    foreach (AnimeSeriesVM series in AllAnimeSeries)
                    {
                        totalRating += ((decimal)series.AniDB_Anime.Rating * series.AniDB_Anime.VoteCount);
                        totalRating += ((decimal)series.AniDB_Anime.TempRating * series.AniDB_Anime.TempVoteCount);
                    }

                    return totalRating;
                }
                catch (Exception ex)
                {
                    return 0;
                }
            }
        }

        public int AniDBTotalVotes
        {
            get
            {
                try
                {
                    int cnt = 0;
                    foreach (AnimeSeriesVM series in AllAnimeSeries)
                    {
                        cnt += series.AniDB_Anime.AniDBTotalVotes;
                    }

                    return cnt;
                }
                catch (Exception ex)
                {
                    return 0;
                }
            }
        }

        public decimal AniDBRating
        {
            get
            {
                try
                {
                    if (AniDBTotalVotes == 0)
                        return 0;
                    else
                        return AniDBTotalRating / (decimal)AniDBTotalVotes / (decimal)100;

                }
                catch (Exception ex)
                {
                    return 0;
                }
            }
        }

        public string AniDBRatingFormatted
        {
            get
            {
                return string.Format("{0} ({1} {2})", Utils.FormatAniDBRating((double)AniDBRating),
                    AniDBTotalVotes, JMMClient.Properties.Resources.Votes);
            }
        }


        public string EpisodeCountFormatted
        {
            get
            {
                int epCountNormal = 0;
                int epCountSpecial = 0;
                foreach (AnimeSeriesVM series in AllAnimeSeries)
                {
                    epCountNormal += series.AniDB_Anime.EpisodeCountNormal;
                    epCountSpecial += series.AniDB_Anime.EpisodeCountSpecial;
                }

                return string.Format("{0} {1} ({2} {3})", epCountNormal, JMMClient.Properties.Resources.Episodes,
                    epCountSpecial, JMMClient.Properties.Resources.Specials);
            }
        }*/



        public void Populate(VM_AnimeGroup_User contract)
        {
            AnimeGroupParentID = contract.AnimeGroupParentID;
            DefaultAnimeSeriesID = contract.DefaultAnimeSeriesID;
            base.GroupName = contract.GroupName;
            Description = contract.Description;
            IsManuallyNamed = contract.IsManuallyNamed;
            DateTimeUpdated = contract.DateTimeUpdated;
            base.SortName = contract.SortName;
            EpisodeAddedDate = contract.EpisodeAddedDate;
            LatestEpisodeAirDate = contract.LatestEpisodeAirDate;
            OverrideDescription = contract.OverrideDescription;
            MissingEpisodeCount = contract.MissingEpisodeCount;
            MissingEpisodeCountGroups = contract.MissingEpisodeCountGroups;
            Stat_AirDate_Min = contract.Stat_AirDate_Min;
            Stat_AirDate_Max = contract.Stat_AirDate_Max;
            Stat_EndDate = contract.Stat_EndDate;
            Stat_SeriesCreatedDate = contract.Stat_SeriesCreatedDate;
            Stat_UserVotePermanent = contract.Stat_UserVotePermanent;
            Stat_UserVoteTemporary = contract.Stat_UserVoteTemporary;
            Stat_UserVoteOverall = contract.Stat_UserVoteOverall;
            Stat_AllTags = contract.Stat_AllTags;
            Stat_AllYears = contract.Stat_AllYears;
            Stat_AllCustomTags = contract.Stat_AllCustomTags;
            Stat_AllTitles = contract.Stat_AllTitles;
            Stat_AnimeTypes = contract.Stat_AnimeTypes;
            Stat_IsComplete = contract.Stat_IsComplete;
            Stat_HasFinishedAiring = contract.Stat_HasFinishedAiring;
            Stat_IsCurrentlyAiring = contract.Stat_IsCurrentlyAiring;
            Stat_HasTvDBLink = contract.Stat_HasTvDBLink;
            Stat_HasMALLink = contract.Stat_HasMALLink;
            Stat_HasMovieDBLink = contract.Stat_HasMovieDBLink;
            Stat_HasMovieDBOrTvDBLink = contract.Stat_HasMovieDBOrTvDBLink;
            Stat_AllVideoQuality = contract.Stat_AllVideoQuality;
            Stat_AllVideoQuality_Episodes = contract.Stat_AllVideoQuality_Episodes;
            Stat_AudioLanguages = contract.Stat_AudioLanguages;
            Stat_SubtitleLanguages = contract.Stat_SubtitleLanguages;
            Stat_SeriesCount = contract.Stat_SeriesCount;
            Stat_EpisodeCount = contract.Stat_EpisodeCount;
            Stat_AniDBRating = contract.Stat_AniDBRating;
            ServerPosterPath = contract.ServerPosterPath;
            SeriesForNameOverride = contract.SeriesForNameOverride;
        }
        public new VM_AnimeSeries_User SeriesForNameOverride { get; set; }

        private string _groupName;
        public new string GroupName
        {
            get
            {
                if (_groupName != null)
                    return _groupName;
                if (SeriesForNameOverride != null)
                    return SeriesForNameOverride.SeriesName;
                return base.GroupName;
            }
            set { _groupName = value; }
        }

        private string _sortName;
        public new string SortName
        {
            get
            {
                if (_sortName != null)
                    return _sortName;
                if (SeriesForNameOverride != null)
                    return SeriesForNameOverride.SeriesName;
                return base.SortName;
            }
            set { _sortName = value; }
        }


    
        public bool Save()
        {
            try
            {
                CL_Response<CL_AnimeGroup_User> response=VM_ShokoServer.Instance.ShokoServices.SaveGroup(ToContract(),
                    VM_ShokoServer.Instance.CurrentUser.JMMUserID);
                if (!string.IsNullOrEmpty(response.ErrorMessage))
                {
                    return false;
                }
                Populate((VM_AnimeGroup_User)response.Result);
                return true;
            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write(ex.ToString());
                return false;
            }
        }

        public CL_AnimeGroup_Save_Request ToContract()
        {
            CL_AnimeGroup_Save_Request contract = new CL_AnimeGroup_Save_Request();
            contract.AnimeGroupID = AnimeGroupID;
            contract.AnimeGroupParentID = AnimeGroupParentID;

            // editable members
            contract.GroupName = GroupName;
            contract.IsFave = IsFave;
            contract.SortName = SortName;
            contract.Description = Description;

            return contract;
        }
    }
}