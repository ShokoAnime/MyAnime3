using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Shoko.Commons.Extensions;
using Shoko.Models.Client;
using Shoko.Models.Enums;
using Shoko.Models.Server;
using Shoko.MyAnime3.ImageManagement;
using Shoko.MyAnime3.ViewModel.Helpers;
using Shoko.MyAnime3.Windows;


namespace Shoko.MyAnime3.ViewModel.Server
{
    public class VM_AniDB_Anime : CL_AniDB_Anime
    {
        public new VM_AniDB_Anime_DefaultImage DefaultImagePoster => (VM_AniDB_Anime_DefaultImage) base.DefaultImagePoster;
        public new VM_AniDB_Anime_DefaultImage DefaultImageFanart => (VM_AniDB_Anime_DefaultImage) base.DefaultImageFanart;
        public new VM_AniDB_Anime_DefaultImage DefaultImageWideBanner => (VM_AniDB_Anime_DefaultImage) base.DefaultImageWideBanner;


        private bool? isImageDefault;

        public bool IsImageDefault
        {
            get
            {
                if (isImageDefault == null)
                    if (DefaultImagePoster != null && DefaultImagePoster.ImageParentType == (int) ImageEntityType.AniDB_Cover)
                        isImageDefault = true;
                    else
                        isImageDefault = false;
                return isImageDefault.Value;
            }
            set => isImageDefault = value;
        }


        private VM_AniDB_AnimeCrossRefs aniDB_AnimeCrossRefs;

        public VM_AniDB_AnimeCrossRefs AniDB_AnimeCrossRefs
        {
            get
            {
                if (aniDB_AnimeCrossRefs != null) return aniDB_AnimeCrossRefs;
                RefreshAnimeCrossRefs();

                AniDB_AnimeCrossRefs.AllPosters.Insert(0, new PosterContainer(ImageEntityType.AniDB_Cover, this));
                return aniDB_AnimeCrossRefs;
            }
        }
        public string TagsFormatted => string.Join(", ", this.GetAllTags());

        public string TagsFormattedShort => string.Join(", ", this.GetAllTags().ToList().Take(6));


        public void RefreshAnimeCrossRefs()
        {
            aniDB_AnimeCrossRefs = (VM_AniDB_AnimeCrossRefs) VM_ShokoServer.Instance.ShokoServices.GetCrossRefDetails(AnimeID);
            aniDB_AnimeCrossRefs.Fill(this);
        }

        public string PosterPathNoDefaultPlain => Path.Combine(Utils.GetAniDBImagePath(AnimeID), Picname);


        public string PosterPathNoDefault
        {
            get
            {
                if (!File.Exists(PosterPathNoDefaultPlain))
                {
                    ImageDownloadRequest req = new ImageDownloadRequest(ImageEntityType.AniDB_Cover, this, false);
                    MainWindow.imageHelper.DownloadImage(req);
                }

                return PosterPathNoDefaultPlain;
            }
        }

        public string PosterPath
        {
            get
            {
                string fileName = Path.Combine(Utils.GetAniDBImagePath(AnimeID), Picname);

                if (!File.Exists(fileName))
                {
                    ImageDownloadRequest req = new ImageDownloadRequest(ImageEntityType.AniDB_Cover, this, false);
                    MainWindow.imageHelper.DownloadImage(req);
                    if (File.Exists(fileName)) return fileName;

                    return string.Empty;
                }

                return fileName;
            }
        }

        public string FullImagePath => PosterPath;

        public string DefaultPosterPath
        {
            get
            {
                if (DefaultImagePoster == null)
                    return PosterPath;
                ImageEntityType imageType = (ImageEntityType) DefaultImagePoster.ImageParentType;

                switch (imageType)
                {
                    case ImageEntityType.AniDB_Cover:
                        return PosterPath;

                    case ImageEntityType.TvDB_Cover:
                        if (DefaultImagePoster.TVPoster != null)
                            return DefaultImagePoster.TVPoster.FullImagePath;
                        else
                            return PosterPath;
                        /*
                    case ImageEntityType.Trakt_Poster:
                        if (DefaultImagePoster.TraktPoster != null)
                            return DefaultImagePoster.TraktPoster.FullImagePath;
                        else
                            return PosterPath;

                    case ImageEntityType.MovieDB_Poster:
                        if (DefaultImagePoster.MoviePoster != null)
                            return DefaultImagePoster.MoviePoster.FullImagePath;
                        else
                            return PosterPath;*/
                }

                return PosterPath;
            }
        }

        public string DefaultPosterPathOnly
        {
            get
            {
                if (DefaultImagePoster == null)
                    return "";
                ImageEntityType imageType = (ImageEntityType) DefaultImagePoster.ImageParentType;

                switch (imageType)
                {
                    case ImageEntityType.AniDB_Cover:
                        return PosterPath;

                    case ImageEntityType.TvDB_Cover:
                        if (DefaultImagePoster.TVPoster != null)
                            return DefaultImagePoster.TVPoster.FullImagePath;
                        else
                            return string.Empty;
                        /*
                    case ImageEntityType.Trakt_Poster:
                        if (DefaultImagePoster.TraktPoster != null)
                            return DefaultImagePoster.TraktPoster.FullImagePath;
                        else
                            return string.Empty;
                            */
                    case ImageEntityType.MovieDB_Poster:
                        if (DefaultImagePoster.MoviePoster != null)
                            return DefaultImagePoster.MoviePoster.FullImagePath;
                        else
                            return string.Empty;
                }

                return string.Empty;
            }
        }

        public string AirDateAsString
        {
            get
            {
                if (AirDate.HasValue)
                    return AirDate.Value.ToString("dd MMM yyyy", Globals.Culture);
                return string.Empty;
            }
        }

        public string Year
        {
            get
            {
                string y = BeginYear.ToString();
                if (BeginYear != EndYear)
                    if (EndYear <= 0)
                        y += "-Ongoing";
                    else
                        y += "-" + EndYear;
                return y;
            }
        }

        public string ParsedDescription
        {
            get => Utils.ReparseDescription(Description);
            set => Description = value;
        }

        public bool FinishedAiring
        {
            get
            {
                if (!EndDate.HasValue) return false; // ongoing

                // all series have finished airing and the user has all the episodes
                if (EndDate.Value < DateTime.Now) return true;

                return false;
            }
        }

        public bool FanartExists => AniDB_AnimeCrossRefs?.AllFanarts.Count > 0;

        public bool FanartMissing => !FanartExists;

        public string FanartPath
        {
            get
            {
                // this should be randomised or use the default 
                if (DefaultImageFanart != null)
                    return DefaultImageFanart.FullImagePath;

                if (AniDB_AnimeCrossRefs == null)
                    return string.Empty;

                if (AniDB_AnimeCrossRefs.AllFanarts.Count == 0)
                    return string.Empty;

                if (File.Exists(AniDB_AnimeCrossRefs.AllFanarts[0].FullImagePath))
                    return AniDB_AnimeCrossRefs.AllFanarts[0].FullImagePath;

                return string.Empty;
            }
        }

        public string FanartThumbnailPath
        {
            get
            {
                // this should be randomised or use the default 
                if (DefaultImageFanart != null)
                    return DefaultImageFanart.FullThumbnailPath;

                if (AniDB_AnimeCrossRefs == null)
                    return string.Empty;

                if (AniDB_AnimeCrossRefs.AllFanarts.Count == 0)
                    return string.Empty;

                if (File.Exists(AniDB_AnimeCrossRefs.AllFanarts[0].FullThumbnailPath))
                    return AniDB_AnimeCrossRefs.AllFanarts[0].FullImagePath;

                return string.Empty;
            }
        }

        public string EndDateAsString
        {
            get
            {
                if (EndDate.HasValue)
                    return EndDate.Value.ToString("dd MMM yyyy", Globals.Culture);
                return "Ongoing";
            }
        }

        public string AirDateAndEndDate => $"{AirDateAsString}  to  {EndDateAsString}";

        public string BeginYearAndEndYear
        {
            get
            {
                if (BeginYear == EndYear)
                    return BeginYear.ToString();
                return $"{BeginYear} - {EndYear}";
            }
        }


        public string AnimeID_Friendly => $"AniDB: {AnimeID}";

        public Shoko.Models.Enums.AnimeType AnimeTypeEnum
        {
            get
            {
                if (AnimeType > 5) return Shoko.Models.Enums.AnimeType.Other;
                return (AnimeType) AnimeType;
            }
        }

        public string AnimeTypeDescription
        {
            get
            {
                switch (AnimeTypeEnum)
                {
                    case Shoko.Models.Enums.AnimeType.Movie: return "Movie";
                    case Shoko.Models.Enums.AnimeType.Other: return "Other";
                    case Shoko.Models.Enums.AnimeType.OVA: return "OVA";
                    case Shoko.Models.Enums.AnimeType.TVSeries: return "TV Series";
                    case Shoko.Models.Enums.AnimeType.TVSpecial: return "TV Special";
                    case Shoko.Models.Enums.AnimeType.Web: return "Web";
                    default: return "Other";
                }
            }
        }

        public decimal AniDBTotalRating
        {
            get
            {
                try
                {
                    decimal totalRating = 0;
                    totalRating += (decimal) Rating * VoteCount;
                    totalRating += (decimal) TempRating * TempVoteCount;

                    return totalRating;
                }
                catch
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
                    return TempVoteCount + VoteCount;
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
                    if (AniDBTotalVotes == 0)
                        return 0;
                    return AniDBTotalRating / AniDBTotalVotes / 100;
                }
                catch
                {
                    return 0;
                }
            }
        }

        public string AniDBRatingFormatted => $"{Utils.FormatAniDBRating((double) AniDBRating)} ({AniDBTotalVotes} {Translation.Votes})";

        public AniDB_Vote UserVote => VM_ShokoServer.Instance.ShokoServices.GetUserVote(AnimeID);

        public string UserVoteFormatted
        {
            get
            {
                AniDB_Vote vote = UserVote;
                if (vote == null)
                {
                    BaseConfig.MyAnimeLog.Write("No vote for : " + AnimeID);
                    return string.Empty;
                }
                return $"{Utils.FormatAniDBRating(vote.VoteValue)}";
            }
        }

        public override string ToString()
        {
            return $"ANIME: {AnimeID} - {FormattedTitle}";
        }

        public void ClearTvDBData()
        {
            tvSummary = null;
        }

        private TvDBSummary tvSummary;

        public TvDBSummary TvSummary
        {
            get
            {
                if (tvSummary == null)
                    try
                    {
                        tvSummary = new TvDBSummary();
                        tvSummary.Populate(AnimeID);
                    }
                    catch (Exception ex)
                    {
                        BaseConfig.MyAnimeLog.Write("TvSummary\r\n" + ex);
                    }
                return tvSummary;
            }
        }

        public VM_AnimeSeries_User AnimeSeries => ShokoServerHelper.GetSeriesForAnime(AnimeID);

        public List<CL_AniDB_Anime_Relation> RelatedAnimeLinks => ShokoServerHelper.GetRelatedAnime(AnimeID);

    }
}