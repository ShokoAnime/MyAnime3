using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using BinaryNorthwest;

namespace MyAnimePlugin3.ViewModel
{
	public class AniDB_AnimeVM
	{
		public int AnimeID { get; set; }
		public int EpisodeCount { get; set; }
		public DateTime? AirDate { get; set; }
		public DateTime? EndDate { get; set; }
		public string URL { get; set; }
		public string Picname { get; set; }
		public int BeginYear { get; set; }
		public int EndYear { get; set; }
		public int AnimeType { get; set; }
		public string MainTitle { get; set; }
		public string FormattedTitle { get; set; }
		public string AllTitles { get; set; }
		public string AllCategories { get; set; }
		public string AllTags { get; set; }
		public string Description { get; set; }
		public int EpisodeCountNormal { get; set; }
		public int EpisodeCountSpecial { get; set; }
		public int Rating { get; set; }
		public int VoteCount { get; set; }
		public int TempRating { get; set; }
		public int TempVoteCount { get; set; }
		public int AvgReviewRating { get; set; }
		public int ReviewCount { get; set; }
		public DateTime DateTimeUpdated { get; set; }
		public DateTime DateTimeDescUpdated { get; set; }
		public int ImageEnabled { get; set; }
		public string AwardList { get; set; }
		public int Restricted { get; set; }
		public int? AnimePlanetID { get; set; }
		public int? ANNID { get; set; }
		public int? AllCinemaID { get; set; }
		public int? AnimeNfo { get; set; }
		public int? LatestEpisodeNumber { get; set; }

		public AniDB_Anime_DefaultImageVM DefaultPoster { get; set; }
		public AniDB_Anime_DefaultImageVM DefaultFanart { get; set; }
		public AniDB_Anime_DefaultImageVM DefaultWideBanner { get; set; }



		public AniDB_AnimeVM()
		{
		}

		private bool isImageDefault = false;
		public bool IsImageDefault
		{
			get { return isImageDefault; }
			set { isImageDefault = value; }
		}

		public AniDB_AnimeVM(JMMServerBinary.Contract_AniDBAnime contract)
		{
			this.AirDate = contract.AirDate;
			this.AllCategories = contract.AllCategories;
			this.AllCinemaID = contract.AllCinemaID;
			this.AllTags = contract.AllTags;
			this.AllTitles = contract.AllTitles;
			this.AnimeID = contract.AnimeID;
			this.AnimeNfo = contract.AnimeNfo;
			this.AnimePlanetID = contract.AnimePlanetID;
			this.AnimeType = contract.AnimeType;
			this.ANNID = contract.ANNID;
			this.AvgReviewRating = contract.AvgReviewRating;
			this.AwardList = contract.AwardList;
			this.BeginYear = contract.BeginYear;
			this.Description = contract.Description;
			this.DateTimeDescUpdated = contract.DateTimeDescUpdated;
			this.DateTimeUpdated = contract.DateTimeUpdated;
			this.EndDate = contract.EndDate;
			this.EndYear = contract.EndYear;
			this.EpisodeCount = contract.EpisodeCount;
			this.EpisodeCountNormal = contract.EpisodeCountNormal;
			this.EpisodeCountSpecial = contract.EpisodeCountSpecial;
			this.ImageEnabled = contract.ImageEnabled;
			this.LatestEpisodeNumber = contract.LatestEpisodeNumber;
			this.MainTitle = contract.MainTitle;
			this.Picname = contract.Picname;
			this.Rating = contract.Rating;
			this.Restricted = contract.Restricted;
			this.ReviewCount = contract.ReviewCount;
			this.TempRating = contract.TempRating;
			this.TempVoteCount = contract.TempVoteCount;
			this.URL = contract.URL;
			this.VoteCount = contract.VoteCount;
			this.FormattedTitle = contract.FormattedTitle;

			if (contract.DefaultImagePoster != null)
				DefaultPoster = new AniDB_Anime_DefaultImageVM(contract.DefaultImagePoster);
			else
				DefaultPoster = null;

			if (contract.DefaultImageFanart != null)
				DefaultFanart = new AniDB_Anime_DefaultImageVM(contract.DefaultImageFanart);
			else
				DefaultFanart = null;

			if (contract.DefaultImageWideBanner != null)
				DefaultWideBanner = new AniDB_Anime_DefaultImageVM(contract.DefaultImageWideBanner);
			else
				DefaultWideBanner = null;

			bool isDefault = false;
			if (DefaultPoster != null && DefaultPoster.ImageParentType == (int)ImageEntityType.AniDB_Cover)
				isDefault = true;

			IsImageDefault = isDefault;

		}

		private AniDB_AnimeCrossRefsVM aniDB_AnimeCrossRefs = null;
		public AniDB_AnimeCrossRefsVM AniDB_AnimeCrossRefs
		{
			get
			{
				if (aniDB_AnimeCrossRefs != null) return aniDB_AnimeCrossRefs;
				RefreshAnimeCrossRefs();
				AniDB_AnimeCrossRefs.AllPosters.Insert(0, new PosterContainer(ImageEntityType.AniDB_Cover, this));
				return aniDB_AnimeCrossRefs;
			}
		}

		public List<string> Categories
		{
			get
			{
				string[] cats = AllCategories.Split('|');

				if (cats.Length == 0) return new List<string>();
				return new List<string>(cats);
			}
		}

		public string CategoriesFormatted
		{
			get
			{
				string ret = "";
				foreach (string cat in Categories)
				{
					if (ret.Length > 0) ret += ", ";
					ret += cat;
				}
				return ret;
			}
		}

		public string CategoriesFormattedShort
		{
			get
			{
				string ret = "";
				int i = 0;
				foreach (string cat in Categories)
				{
					if (ret.Length > 0) ret += ", ";
					ret += cat;

					if (i == 6) break;
				}
				return ret;
			}
		}

		public List<string> Titles
		{
			get
			{
				string[] titles = AllTitles.Split('|');

				if (titles.Length == 0) return new List<string>();
				return new List<string>(titles);
			}
		}

		public void RefreshAnimeCrossRefs()
		{
			JMMServerBinary.Contract_AniDB_AnimeCrossRefs xrefDetails = JMMServerVM.Instance.clientBinaryHTTP.GetCrossRefDetails(this.AnimeID);
			if (xrefDetails == null) return;

			aniDB_AnimeCrossRefs = new AniDB_AnimeCrossRefsVM();
			aniDB_AnimeCrossRefs.Populate(xrefDetails);
		}

		public string PosterPathNoDefault
		{
			get
			{
				string fileName = Path.Combine(Utils.GetAniDBImagePath(AnimeID), Picname);
				return fileName;
			}
		}

		public string PosterPath
		{
			get
			{
				string fileName = Path.Combine(Utils.GetAniDBImagePath(AnimeID), Picname);

				if (!File.Exists(fileName))
					return "";
				
				return fileName;
			}
		}

		public string FullImagePath
		{
			get
			{
				return PosterPath;
			}
		}

		public string DefaultPosterPath
		{
			get
			{
				if (DefaultPoster == null)
					return PosterPath;
				else
				{
					ImageEntityType imageType = (ImageEntityType)DefaultPoster.ImageParentType;

					switch (imageType)
					{
						case ImageEntityType.AniDB_Cover:
							return this.PosterPath;

						case ImageEntityType.TvDB_Cover:
							if (DefaultPoster.TVPoster != null)
								return DefaultPoster.TVPoster.FullImagePath;
							else
								return this.PosterPath;

						case ImageEntityType.Trakt_Poster:
							if (DefaultPoster.TraktPoster != null)
								return DefaultPoster.TraktPoster.FullImagePath;
							else
								return this.PosterPath;

						case ImageEntityType.MovieDB_Poster:
							if (DefaultPoster.MoviePoster != null)
								return DefaultPoster.MoviePoster.FullImagePath;
							else
								return this.PosterPath;
					}
				}

				return PosterPath;
			}
		}

		public string DefaultPosterPathOnly
		{
			get
			{
				if (DefaultPoster == null)
					return "";
				else
				{
					ImageEntityType imageType = (ImageEntityType)DefaultPoster.ImageParentType;

					switch (imageType)
					{
						case ImageEntityType.AniDB_Cover:
							return this.PosterPath;

						case ImageEntityType.TvDB_Cover:
							if (DefaultPoster.TVPoster != null)
								return DefaultPoster.TVPoster.FullImagePath;
							else
								return "";

						case ImageEntityType.Trakt_Poster:
							if (DefaultPoster.TraktPoster != null)
								return DefaultPoster.TraktPoster.FullImagePath;
							else
								return "";

						case ImageEntityType.MovieDB_Poster:
							if (DefaultPoster.MoviePoster != null)
								return DefaultPoster.MoviePoster.FullImagePath;
							else
								return "";
					}
				}

				return "";
			}
		}

		public string AirDateAsString
		{
			get
			{
				if (AirDate.HasValue)
					return AirDate.Value.ToString("dd MMM yyyy", Globals.Culture);
				else
					return "";
			}
		}

		public string Year
		{
			get
			{
				string y = BeginYear.ToString();
				if (BeginYear != EndYear)
				{
					if (EndDate.HasValue)
						y += "-Ongoing";
					else
						y += "-" + EndYear.ToString();
				}
				return y;
			}
		}

		public string ParsedDescription
		{
			get
			{
				return Utils.ReparseDescription(Description);

			}
			set
			{
				Description = value;
			}
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

		public bool FanartExists
		{
			get
			{
				if (AniDB_AnimeCrossRefs == null) return false;

				if (AniDB_AnimeCrossRefs.AllFanarts.Count > 0)
					return true;
				else
					return false;

			}
		}

		public bool FanartMissing
		{
			get
			{
				return !FanartExists;
			}
		}

		public string FanartPath
		{
			get
			{
				// this should be randomised or use the default 
				if (DefaultFanart != null)
					return DefaultFanart.FullImagePath;

				if (AniDB_AnimeCrossRefs == null)
					return "";

				if (AniDB_AnimeCrossRefs.AllFanarts.Count == 0)
					return "";

				if (File.Exists(AniDB_AnimeCrossRefs.AllFanarts[0].FullImagePath))
					return AniDB_AnimeCrossRefs.AllFanarts[0].FullImagePath;

				return "";
			}
		}

		public string FanartThumbnailPath
		{
			get
			{
				// this should be randomised or use the default 
				if (DefaultFanart != null)
					return DefaultFanart.FullThumbnailPath;

				if (AniDB_AnimeCrossRefs == null)
					return "";

				if (AniDB_AnimeCrossRefs.AllFanarts.Count == 0)
					return "";

				if (File.Exists(AniDB_AnimeCrossRefs.AllFanarts[0].FullThumbnailPath))
					return AniDB_AnimeCrossRefs.AllFanarts[0].FullImagePath;

				return "";
			}
		}

		public string EndDateAsString
		{
			get
			{
				if (EndDate.HasValue)
					return EndDate.Value.ToString("dd MMM yyyy", Globals.Culture);
				else
					return "Ongoing";
			}
		}

		public string AirDateAndEndDate
		{
			get
			{
				return string.Format("{0}  to  {1}", AirDateAsString, EndDateAsString);
			}
		}

		public string BeginYearAndEndYear
		{
			get
			{
				if (BeginYear == EndYear) return BeginYear.ToString();
				else
					return string.Format("{0} - {1}", BeginYear, EndYear);
			}
		}

		public string AniDB_SiteURL
		{
			get
			{
				return string.Format(Constants.URLS.AniDB_Series, AnimeID);

			}
		}

		public string AnimeID_Friendly
		{
			get
			{
				return string.Format("AniDB: {0}", AnimeID);
			}
		}

		public enAnimeType AnimeTypeEnum
		{
			get
			{
				if (AnimeType > 5) return enAnimeType.Other;
				return (enAnimeType)AnimeType;
			}
		}

		public string AnimeTypeDescription
		{
			get
			{
				switch (AnimeTypeEnum)
				{
					case enAnimeType.Movie: return "Movie";
					case enAnimeType.Other: return "Other";
					case enAnimeType.OVA: return "OVA";
					case enAnimeType.TVSeries: return "TV Series";
					case enAnimeType.TVSpecial: return "TV Special";
					case enAnimeType.Web: return "Web";
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
					totalRating += ((decimal)Rating * VoteCount);
					totalRating += ((decimal)TempRating * TempVoteCount);

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
					return TempVoteCount + VoteCount;
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
					AniDBTotalVotes, "Votes");
			}
		}

		public AniDB_VoteVM UserVote
		{
			get
			{
				JMMServerBinary.Contract_AniDBVote contract = JMMServerVM.Instance.clientBinaryHTTP.GetUserVote(this.AnimeID);
				if (contract == null) return null;

				return new AniDB_VoteVM(contract);
			}
		}

		public string UserVoteFormatted
		{
			get
			{
				AniDB_VoteVM vote = this.UserVote;
				if (vote == null)
				{
					BaseConfig.MyAnimeLog.Write("No vote for : " + this.AnimeID);
					return "";
				}
				else
				{
					return string.Format("{0}", Utils.FormatAniDBRating((double)vote.VoteValue));
				}
			}
		}

		public override string ToString()
		{
			return string.Format("ANIME: {0} - {1}", AnimeID, FormattedTitle);
		}

		private CrossRef_AniDB_TvDBVM crossRefTvDB = null;
		public CrossRef_AniDB_TvDBVM CrossRefTvDB
		{
			get
			{
				if (crossRefTvDB == null)
				{
					try
					{
						JMMServerBinary.Contract_CrossRef_AniDB_TvDB contract = JMMServerVM.Instance.clientBinaryHTTP.GetTVDBCrossRef(this.AnimeID);
						if (contract != null)
							crossRefTvDB = new CrossRef_AniDB_TvDBVM(contract);
					}
					catch (Exception ex)
					{
						BaseConfig.MyAnimeLog.Write(ex.ToString());
					}
				}
				return crossRefTvDB;
			}
		}

		private List<TvDB_EpisodeVM> tvDBEpisodes = null;
		public List<TvDB_EpisodeVM> TvDBEpisodes
		{
			get
			{
				if (tvDBEpisodes == null)
				{
					try
					{
						if (CrossRefTvDB != null)
						{
							List<JMMServerBinary.Contract_TvDB_Episode> eps = JMMServerVM.Instance.clientBinaryHTTP.GetAllTvDBEpisodes(CrossRefTvDB.TvDBID);
							tvDBEpisodes = new List<TvDB_EpisodeVM>();
							foreach (JMMServerBinary.Contract_TvDB_Episode episode in eps)
								tvDBEpisodes.Add(new TvDB_EpisodeVM(episode));

							List<SortPropOrFieldAndDirection> sortCriteria = new List<SortPropOrFieldAndDirection>();
							sortCriteria.Add(new SortPropOrFieldAndDirection("SeasonNumber", false, SortType.eInteger));
							sortCriteria.Add(new SortPropOrFieldAndDirection("EpisodeNumber", false, SortType.eInteger));
							tvDBEpisodes = Sorting.MultiSort<TvDB_EpisodeVM>(tvDBEpisodes, sortCriteria);
						}
					}
					catch (Exception ex)
					{
						BaseConfig.MyAnimeLog.Write(ex.ToString());
					}
				}
				return tvDBEpisodes;
			}
		}

		private Dictionary<int, TvDB_EpisodeVM> dictTvDBEpisodes = null;
		public Dictionary<int, TvDB_EpisodeVM> DictTvDBEpisodes
		{
			get
			{
				if (dictTvDBEpisodes == null)
				{
					try
					{
						if (TvDBEpisodes != null)
						{

							dictTvDBEpisodes = new Dictionary<int, TvDB_EpisodeVM>();
							// create a dictionary of absolute episode numbers for tvdb episodes
							// sort by season and episode number
							// ignore season 0, which is used for specials
							List<TvDB_EpisodeVM> eps = TvDBEpisodes;

							int i = 1;
							foreach (TvDB_EpisodeVM ep in eps)
							{
								dictTvDBEpisodes[i] = ep;
								i++;
							}
							
						}
					}
					catch (Exception ex)
					{
						BaseConfig.MyAnimeLog.Write(ex.ToString());
					}
				}
				return dictTvDBEpisodes;
			}
		}

		private Dictionary<int, int> dictTvDBSeasons = null;
		public Dictionary<int, int> DictTvDBSeasons
		{
			get
			{
				if (dictTvDBSeasons == null)
				{
					try
					{
						if (TvDBEpisodes != null)
						{
							dictTvDBSeasons = new Dictionary<int, int>();
							// create a dictionary of season numbers and the first episode for that season

							List<TvDB_EpisodeVM> eps = TvDBEpisodes;
							int i = 1;
							int lastSeason = -999;
							foreach (TvDB_EpisodeVM ep in eps)
							{
								if (ep.SeasonNumber != lastSeason)
									dictTvDBSeasons[ep.SeasonNumber] = i;

								lastSeason = ep.SeasonNumber;
								i++;

							}
						}
					}
					catch (Exception ex)
					{
						BaseConfig.MyAnimeLog.Write(ex.ToString());
					}
				}
				return dictTvDBSeasons;
			}
		}

		private Dictionary<int, int> dictTvDBSeasonsSpecials = null;
		public Dictionary<int, int> DictTvDBSeasonsSpecials
		{
			get
			{
				if (dictTvDBSeasonsSpecials == null)
				{
					try
					{
						if (TvDBEpisodes != null)
						{
							dictTvDBSeasonsSpecials = new Dictionary<int, int>();
							// create a dictionary of season numbers and the first episode for that season

							List<TvDB_EpisodeVM> eps = TvDBEpisodes;
							int i = 1;
							int lastSeason = -999;
							foreach (TvDB_EpisodeVM ep in eps)
							{
								if (ep.SeasonNumber > 0) continue;

								int thisSeason = 0;
								if (ep.AirsBeforeSeason.HasValue) thisSeason = ep.AirsBeforeSeason.Value;
								if (ep.AirsAfterSeason.HasValue) thisSeason = ep.AirsAfterSeason.Value;

								if (thisSeason != lastSeason)
									dictTvDBSeasonsSpecials[thisSeason] = i;

								lastSeason = thisSeason;
								i++;

							}
						}
					}
					catch (Exception ex)
					{
						BaseConfig.MyAnimeLog.Write(ex.ToString());
					}
				}
				return dictTvDBSeasonsSpecials;
			}
		}

		public AnimeSeriesVM AnimeSeries
		{
			get
			{
				return JMMServerHelper.GetSeriesForAnime(this.AnimeID);
			}
		}

		public List<AniDB_Anime_RelationVM> RelatedAnimeLinks
		{
			get
			{
				return JMMServerHelper.GetRelatedAnime(this.AnimeID);
			}
		}

		public List<AniDB_CharacterVM> Characters
		{
			get
			{
				return JMMServerHelper.GetCharactersForAnime(this.AnimeID);
			}
		}
	}
}
