using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyAnimePlugin3.JMMServerBinary;

namespace MyAnimePlugin3.ViewModel
{
	public class AnimeGroupVM : IComparable<AnimeGroupVM>
	{
		// Data from AnimeGroup
		public int AnimeGroupID { get; set; }
		public int? AnimeGroupParentID { get; set; }
		public int? DefaultAnimeSeriesID { get; set; }
		public string GroupName { get; set; }
		public string Description { get; set; }
		public int IsFave { get; set; }
		public int IsManuallyNamed { get; set; }
		public int UnwatchedEpisodeCount { get; set; }
		public DateTime DateTimeUpdated { get; set; }
		public int WatchedEpisodeCount { get; set; }
		public string SortName { get; set; }
		public DateTime? WatchedDate { get; set; }
		public DateTime? EpisodeAddedDate { get; set; }
		public int PlayedCount { get; set; }
		public int WatchedCount { get; set; }
		public int StoppedCount { get; set; }
		public int OverrideDescription { get; set; }

		public int MissingEpisodeCount { get; set; }
		public int MissingEpisodeCountGroups { get; set; }

		public DateTime? Stat_AirDate_Min { get; set; }
		public DateTime? Stat_AirDate_Max { get; set; }
		public DateTime? Stat_EndDate { get; set; }
		public DateTime? Stat_SeriesCreatedDate { get; set; }
		public decimal? Stat_UserVotePermanent { get; set; }
		public decimal? Stat_UserVoteTemporary { get; set; }
		public decimal? Stat_UserVoteOverall { get; set; }
		public string Stat_AllCategories { get; set; }
		public string Stat_AllTitles { get; set; }
		public bool Stat_IsComplete { get; set; }
		public bool Stat_HasFinishedAiring { get; set; }
		public bool Stat_HasTvDBLink { get; set; }
		public bool Stat_HasMovieDBLink { get; set; }
		public bool Stat_HasMovieDBOrTvDBLink { get; set; }
		public string Stat_AllVideoQuality { get; set; }
		public string Stat_AllVideoQuality_Episodes { get; set; }
		public string Stat_AudioLanguages { get; set; }
		public string Stat_SubtitleLanguages { get; set; }
		public int Stat_SeriesCount { get; set; }
		public int Stat_EpisodeCount { get; set; }
		public decimal Stat_AniDBRating { get; set; }

		public int CompareTo(AnimeGroupVM obj)
		{
			return SortName.CompareTo(obj.SortName);
		}

		public override string ToString()
		{
			return string.Format("{0} - {1}", AnimeGroupID, GroupName);
		}

		public List<string> Categories
		{
			get
			{
				string[] cats = Stat_AllCategories.Split('|');

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

		public decimal AniDBTotalRating
		{
			get
			{
				try
				{
					decimal totalRating = 0;
					foreach (AnimeSeriesVM series in AllSeries)
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
					foreach (AnimeSeriesVM series in AllSeries)
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
					/*if (AniDBTotalVotes == 0)
						return 0;
					else
						return AniDBTotalRating / (decimal)AniDBTotalVotes / (decimal)100;*/

					return Stat_AniDBRating / (decimal)100;

				}
				catch (Exception ex)
				{
					return 0;
				}
			}
		}

		public AnimeSeriesVM DefaultSeries
		{
			get
			{
				if (!this.DefaultAnimeSeriesID.HasValue) return null;
				return JMMServerHelper.GetSeries(this.DefaultAnimeSeriesID.Value);
			}
		}

		public List<AnimeGroupVM> SubGroups
		{
			get
			{
				return JMMServerHelper.GetSubGroupsForGroup(this);
			}
		}

		public AnimeGroupVM ParentGroup
		{
			get
			{
				if (!this.AnimeGroupParentID.HasValue) return null;

				return JMMServerHelper.GetGroup(this.AnimeGroupParentID.Value);
			}
		}

		public List<AnimeSeriesVM> ChildSeries
		{
			get
			{
				return JMMServerHelper.GetAnimeSeriesForGroup(this);
			}
		}

		public List<AnimeSeriesVM> AllSeries
		{
			get
			{
				return JMMServerHelper.GetAnimeSeriesForGroupRecursive(this);
			}
		}

		public string YearFormatted
		{
			get
			{
				if (!Stat_AirDate_Min.HasValue) return "";

				
				int beginYear = Stat_AirDate_Min.Value.Year;
				int endYear = Stat_EndDate.HasValue ? Stat_EndDate.Value.Year : 0;

				string ret = beginYear.ToString();

				if (beginYear != endYear)
				{
					if (endYear <= 0)
						ret += "-Ongoing";
					else
						ret += "-" + endYear.ToString();
				}

				return ret;
			}
		}

		public int AllSeriesCount
		{
			get
			{
				return Stat_SeriesCount;
			}
		}

		public string ParsedDescription
		{
			get
			{
				string desc = Description;
				if (DefaultAnimeSeriesID.HasValue)
				{
					AnimeSeriesVM ser = DefaultSeries;
					if (ser != null)
					{
						AniDB_AnimeVM anime = ser.AniDB_Anime;
						desc = anime.Description;
					}
				}

				return Utils.ReparseDescription(desc);

			}
			set
			{
				Description = value;
			}
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

		public bool HasUnwatchedFiles
		{
			get
			{
				return UnwatchedEpisodeCount > 0;
			}
		}

		public bool AllFilesWatched
		{
			get
			{
				return UnwatchedEpisodeCount == 0;
			}
		}

		public bool AnyFilesWatched
		{
			get
			{
				return WatchedEpisodeCount > 0;
			}
		}

		public bool HasMissingEpisodesAny
		{
			get
			{
				return (MissingEpisodeCount > 0 || MissingEpisodeCountGroups > 0);
			}
		}

		public bool HasMissingEpisodesAllDifferentToGroups
		{
			get
			{
				return (MissingEpisodeCount > 0 && MissingEpisodeCount != MissingEpisodeCountGroups);
			}
		}

		public bool HasMissingEpisodesGroups
		{
			get
			{
				return MissingEpisodeCountGroups > 0;
			}
		}

		public bool HasMissingEpisodes
		{
			get
			{
				return MissingEpisodeCountGroups > 0;
			}
		}

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

		public AnimeGroupVM()
		{
		}

		public void Populate(JMMServerBinary.Contract_AnimeGroup contract)
		{
			// readonly members
			this.AnimeGroupID = contract.AnimeGroupID;
			this.AnimeGroupParentID = contract.AnimeGroupParentID;
			this.DefaultAnimeSeriesID = contract.DefaultAnimeSeriesID;
			this.DateTimeUpdated = contract.DateTimeUpdated;
			this.MissingEpisodeCount = contract.MissingEpisodeCount;
			this.MissingEpisodeCountGroups = contract.MissingEpisodeCountGroups;
			this.PlayedCount = contract.PlayedCount;
			this.StoppedCount = contract.StoppedCount;
			this.UnwatchedEpisodeCount = contract.UnwatchedEpisodeCount;
			this.WatchedCount = contract.WatchedCount;
			this.EpisodeAddedDate = contract.EpisodeAddedDate;
			this.WatchedDate = contract.WatchedDate;
			this.WatchedEpisodeCount = contract.WatchedEpisodeCount;



			this.Stat_AirDate_Min = contract.Stat_AirDate_Min;
			this.Stat_AirDate_Max = contract.Stat_AirDate_Max;
			this.Stat_EndDate = contract.Stat_EndDate;
			this.Stat_SeriesCreatedDate = contract.Stat_SeriesCreatedDate;
			this.Stat_UserVoteOverall = contract.Stat_UserVoteOverall;
			this.Stat_UserVotePermanent = contract.Stat_UserVotePermanent;
			this.Stat_UserVoteTemporary = contract.Stat_UserVoteTemporary;
			this.Stat_AllCategories = contract.Stat_AllCategories;
			this.Stat_AllTitles = contract.Stat_AllTitles;
			this.Stat_IsComplete = contract.Stat_IsComplete;
			this.Stat_HasFinishedAiring = contract.Stat_HasFinishedAiring;
			this.Stat_AllVideoQuality = contract.Stat_AllVideoQuality;
			this.Stat_AllVideoQuality_Episodes = contract.Stat_AllVideoQuality_Episodes;
			this.Stat_AudioLanguages = contract.Stat_AudioLanguages;
			this.Stat_SubtitleLanguages = contract.Stat_SubtitleLanguages;
			this.Stat_HasTvDBLink = contract.Stat_HasTvDBLink;
			this.Stat_HasMovieDBLink = contract.Stat_HasMovieDBLink;
			this.Stat_HasMovieDBOrTvDBLink = contract.Stat_HasMovieDBOrTvDBLink;
			this.Stat_SeriesCount = contract.Stat_SeriesCount;
			this.Stat_EpisodeCount = contract.Stat_EpisodeCount;
			this.Stat_AniDBRating = contract.Stat_AniDBRating;

			// editable members
			this.GroupName = contract.GroupName;
			this.IsFave = contract.IsFave;
			this.SortName = contract.SortName;
			this.Description = contract.Description;
			//this.UserHasVoted = this.Stat_UserVotePermanent.HasValue;
		}

		public AnimeGroupVM(JMMServerBinary.Contract_AnimeGroup contract)
		{
			Populate(contract);
		}

		public bool Save()
		{
			try
			{
				JMMServerBinary.Contract_AnimeGroup_SaveResponse response = JMMServerVM.Instance.clientBinaryHTTP.SaveGroup(this.ToContract(),
					JMMServerVM.Instance.CurrentUser.JMMUserID);
				if (!string.IsNullOrEmpty(response.ErrorMessage))
				{
					return false;
				}
				else
				{
					this.Populate(response.AnimeGroup);
					return true;
				}
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write(ex.ToString());
				return false;
			}
		}

		public JMMServerBinary.Contract_AnimeGroup_Save ToContract()
		{
			JMMServerBinary.Contract_AnimeGroup_Save contract = new JMMServerBinary.Contract_AnimeGroup_Save();
			contract.AnimeGroupID = this.AnimeGroupID;
			contract.AnimeGroupParentID = this.AnimeGroupParentID;

			// editable members
			contract.GroupName = this.GroupName;
			contract.IsFave = this.IsFave;
			contract.SortName = this.SortName;
			contract.Description = this.Description;

			return contract;
		}
	}
}
