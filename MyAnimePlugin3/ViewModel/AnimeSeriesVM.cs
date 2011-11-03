using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BinaryNorthwest;

namespace MyAnimePlugin3.ViewModel
{
	public class AnimeSeriesVM
	{

		public int? AnimeSeriesID { get; set; }
		public int AnimeGroupID { get; set; }
		public int AniDB_ID { get; set; }
		public int UnwatchedEpisodeCount { get; set; }
		public DateTime DateTimeUpdated { get; set; }
		public DateTime DateTimeCreated { get; set; }
		public int WatchedEpisodeCount { get; set; }
		public string DefaultAudioLanguage { get; set; }
		public string DefaultSubtitleLanguage { get; set; }


		public int PlayedCount { get; set; }
		public int WatchedCount { get; set; }
		public int StoppedCount { get; set; }
		public int LatestLocalEpisodeNumber { get; set; }

		public DateTime? Stat_EndDate { get; set; }
		public decimal? Stat_UserVotePermanent { get; set; }
		public decimal? Stat_UserVoteTemporary { get; set; }

		public string Stat_AllCategories { get; set; }
		public string Stat_AllTitles { get; set; }
		public bool Stat_IsComplete { get; set; }
		public bool Stat_HasFinishedAiring { get; set; }
		public string Stat_AllVideoQuality { get; set; }
		public string Stat_AllVideoQualityEpisodes { get; set; }
		public string Stat_AudioLanguages { get; set; }
		public string Stat_SubtitleLanguages { get; set; }
		public bool Stat_HasTvDBLink { get; set; }
		public bool Stat_HasMovieDBLink { get; set; }

		public AniDB_AnimeVM AniDB_Anime { get; set; }
		public CrossRef_AniDB_TvDBVM CrossRef_AniDB_TvDB { get; set; }
		public CrossRef_AniDB_OtherVM CrossRef_AniDB_MovieDB { get; set; }

		public decimal AniDBRating
		{
			get { return AniDB_Anime.AniDBRating; }
		}

		public DateTime? AirDate
		{
			get { return AniDB_Anime.AirDate; }
		}

		public DateTime? EpisodeAddedDate { get; set; }
		public DateTime? WatchedDate { get; set; }

		public string SortName
		{
			get
			{
				return AniDB_Anime.FormattedTitle;
			}
		}


		private int missingEpisodeCount = 0;
		public int MissingEpisodeCount
		{
			get { return missingEpisodeCount; }
			set
			{
				missingEpisodeCount = value;
			}
		}

		/*public decimal? Stat_UserVoteOverall
		{
			get
			{
				return AniDB_Anime.Detail.UserRating;
			}
		}*/

		private bool hasMissingEpisodesAny = false;
		public bool HasMissingEpisodesAny
		{
			get { return hasMissingEpisodesAny; }
			set
			{
				hasMissingEpisodesAny = value;
			}
		}

		private bool hasMissingEpisodesAllDifferentToGroups = false;
		public bool HasMissingEpisodesAllDifferentToGroups
		{
			get { return hasMissingEpisodesAllDifferentToGroups; }
			set
			{
				hasMissingEpisodesAllDifferentToGroups = value;
			}
		}

		private bool hasMissingEpisodesGroups = false;
		public bool HasMissingEpisodesGroups
		{
			get { return hasMissingEpisodesGroups; }
			set
			{
				hasMissingEpisodesGroups = value;
			}
		}

		private int missingEpisodeCountGroups = 0;
		public int MissingEpisodeCountGroups
		{
			get { return missingEpisodeCountGroups; }
			set
			{
				missingEpisodeCountGroups = value;
			}
		}

		private string posterPath = "";
		public string PosterPath
		{
			get { return posterPath; }
			set
			{
				posterPath = value;
			}
		}

		public string SeriesName
		{
			get
			{
				return AniDB_Anime.FormattedTitle;
			}
		}

		private List<AnimeEpisodeVM> allEpisodes;
		public List<AnimeEpisodeVM> AllEpisodes
		{
			get
			{
				if (allEpisodes == null)
				{
					RefreshEpisodes();
				}
				return allEpisodes;
			}
		}

		public List<AnimeEpisodeVM> GetEpisodesByType(enEpisodeType epType)
		{
			List<AnimeEpisodeVM> eps = new List<AnimeEpisodeVM>();

			foreach (AnimeEpisodeVM ep in AllEpisodes)
			{
				if (ep.EpisodeTypeEnum == epType)
					eps.Add(ep);
			}

			return eps;
		}

		public List<AnimeEpisodeVM> GetEpisodesToDisplay(enEpisodeType epType)
		{
			List<AnimeEpisodeVM> eps = new List<AnimeEpisodeVM>();

			foreach (AnimeEpisodeVM ep in GetEpisodesByType(epType))
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

			foreach (AnimeEpisodeVM ep in GetEpisodesByType(epType))
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
			allEpisodes = new List<AnimeEpisodeVM>();

			try
			{

				Dictionary<int, TvDB_EpisodeVM> dictTvDBEpisodes = this.AniDB_Anime.DictTvDBEpisodes;
				Dictionary<int, int> dictTvDBSeasons = this.AniDB_Anime.DictTvDBSeasons;
				Dictionary<int, int> dictTvDBSeasonsSpecials = this.AniDB_Anime.DictTvDBSeasonsSpecials;
				CrossRef_AniDB_TvDBVM tvDBCrossRef = this.AniDB_Anime.CrossRefTvDB;

				// Normal episodes
				foreach (AnimeEpisodeVM ep in JMMServerHelper.GetEpisodesForSeries(this.AnimeSeriesID.Value))
				{
					ep.SetTvDBImageAndOverview(dictTvDBEpisodes, dictTvDBSeasons, dictTvDBSeasonsSpecials, tvDBCrossRef);
					allEpisodes.Add(ep);
				}

				List<SortPropOrFieldAndDirection> sortCriteria = new List<SortPropOrFieldAndDirection>();
				sortCriteria.Add(new SortPropOrFieldAndDirection("EpisodeType", false, SortType.eInteger));
				sortCriteria.Add(new SortPropOrFieldAndDirection("EpisodeNumber", false, SortType.eInteger));
				allEpisodes = Sorting.MultiSort<AnimeEpisodeVM>(allEpisodes, sortCriteria);
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write(ex.ToString());
			}
		}

		public List<AnimeEpisodeTypeVM> EpisodeTypesToDisplay
		{
			get
			{
				List<AnimeEpisodeTypeVM> epTypes = new List<AnimeEpisodeTypeVM>();

				try
				{
					foreach (AnimeEpisodeVM ep in AllEpisodes)
					{
						if (BaseConfig.Settings.ShowOnlyAvailableEpisodes && ep.LocalFileCount == 0) continue;

						AnimeEpisodeTypeVM epType = new AnimeEpisodeTypeVM(this, ep);

						bool alreadyAdded = false;
						foreach (AnimeEpisodeTypeVM thisEpType in epTypes)
						{
							if (thisEpType.EpisodeType == epType.EpisodeType)
							{
								alreadyAdded = true;
								break;
							}
						}
						if (!alreadyAdded)
							epTypes.Add(epType);
						
					}

					List<SortPropOrFieldAndDirection> sortCriteria = new List<SortPropOrFieldAndDirection>();
					SortPropOrFieldAndDirection sprop = new SortPropOrFieldAndDirection("EpisodeType", false, SortType.eInteger);
					sortCriteria.Add(sprop);
					epTypes = Sorting.MultiSort<AnimeEpisodeTypeVM>(epTypes, sortCriteria);

				}
				catch (Exception ex)
				{
					BaseConfig.MyAnimeLog.Write(ex.ToString());
				}
				return epTypes;
			}
		}

		public List<AnimeEpisodeTypeVM> EpisodeTypes
		{
			get
			{
				List<AnimeEpisodeTypeVM> epTypes = new List<AnimeEpisodeTypeVM>();

				try
				{
					foreach (AnimeEpisodeVM ep in AllEpisodes)
					{
						AnimeEpisodeTypeVM epType = new AnimeEpisodeTypeVM(this, ep);

						bool alreadyAdded = false;
						foreach (AnimeEpisodeTypeVM thisEpType in epTypes)
						{
							if (thisEpType.EpisodeType == epType.EpisodeType)
							{
								alreadyAdded = true;
								break;
							}
						}
						if (!alreadyAdded)
							epTypes.Add(epType);
					}

					List<SortPropOrFieldAndDirection> sortCriteria = new List<SortPropOrFieldAndDirection>();
					SortPropOrFieldAndDirection sprop = new SortPropOrFieldAndDirection("EpisodeType", false, SortType.eInteger);
					sortCriteria.Add(sprop);
					epTypes = Sorting.MultiSort<AnimeEpisodeTypeVM>(epTypes, sortCriteria);

				}
				catch (Exception ex)
				{
					BaseConfig.MyAnimeLog.Write(ex.ToString());
				}
				return epTypes;
			}
		}

		public AnimeSeriesVM()
		{
		}

		public override string ToString()
		{
			return string.Format("ANIME SERIES: {0} - {1}", AnimeSeriesID, AniDB_ID);
		}

		public void Populate(JMMServerBinary.Contract_AnimeSeries contract)
		{
			AniDB_Anime = new AniDB_AnimeVM(contract.AniDBAnime);

			if (contract.CrossRefAniDBTvDB != null)
				CrossRef_AniDB_TvDB = new CrossRef_AniDB_TvDBVM(contract.CrossRefAniDBTvDB);
			else
				CrossRef_AniDB_TvDB = null;

			if (contract.CrossRefAniDBMovieDB != null)
				CrossRef_AniDB_MovieDB = new CrossRef_AniDB_OtherVM(contract.CrossRefAniDBMovieDB);
			else
				CrossRef_AniDB_MovieDB = null;

			// read only members
			this.AniDB_ID = contract.AniDB_ID;
			this.AnimeGroupID = contract.AnimeGroupID;
			this.AnimeSeriesID = contract.AnimeSeriesID;
			this.DateTimeUpdated = contract.DateTimeUpdated;
			this.DateTimeCreated = contract.DateTimeCreated;
			this.DefaultAudioLanguage = contract.DefaultAudioLanguage;
			this.DefaultSubtitleLanguage = contract.DefaultSubtitleLanguage;

			this.LatestLocalEpisodeNumber = contract.LatestLocalEpisodeNumber;
			this.PlayedCount = contract.PlayedCount;
			this.StoppedCount = contract.StoppedCount;
			this.UnwatchedEpisodeCount = contract.UnwatchedEpisodeCount;
			this.WatchedCount = contract.WatchedCount;
			this.WatchedDate = contract.WatchedDate;
			this.EpisodeAddedDate = contract.EpisodeAddedDate;
			this.WatchedEpisodeCount = contract.WatchedEpisodeCount;

			this.MissingEpisodeCount = contract.MissingEpisodeCount;
			this.MissingEpisodeCountGroups = contract.MissingEpisodeCountGroups;

			HasMissingEpisodesAny = (MissingEpisodeCount > 0 || MissingEpisodeCountGroups > 0);
			HasMissingEpisodesAllDifferentToGroups = (MissingEpisodeCount > 0 && MissingEpisodeCount != MissingEpisodeCountGroups);
			HasMissingEpisodesGroups = MissingEpisodeCountGroups > 0;

			PosterPath = AniDB_Anime.DefaultPosterPath;
		}

		public AnimeSeriesVM(JMMServerBinary.Contract_AnimeSeries contract)
		{
			Populate(contract);
		}
	}
}
