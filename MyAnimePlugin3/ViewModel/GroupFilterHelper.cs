using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BinaryNorthwest;

namespace MyAnimePlugin3.ViewModel
{
	public class GroupFilterHelper
	{
		public static GroupFilterVM AllGroupsFilter
		{
			get
			{
				GroupFilterVM gf = new GroupFilterVM();
				gf.GroupFilterID = -999;
				gf.FilterConditions = new List<GroupFilterConditionVM>();
				gf.ApplyToSeries = 0;
				gf.BaseCondition = 1;
				gf.GroupFilterName = "All";

				GroupFilterSortingCriteria gfsc = new GroupFilterSortingCriteria();
				gfsc.SortType = GroupFilterSorting.SortName;
				gfsc.SortDirection = GroupFilterSortDirection.Asc;

				gf.SortCriteriaList.Add(gfsc);
				return gf;
			}
		}

		public static List<GroupFilterVM> GetGroupFiltersFromDatabase()
		{
			return JMMServerHelper.GetAllGroupFilters();
		}

		public static List<GroupFilterVM> AllGroupFilters
		{
			get
			{
				List<GroupFilterVM> gfilters = new List<GroupFilterVM>();
				gfilters.Add(AllGroupsFilter);
				gfilters.AddRange(GetGroupFiltersFromDatabase());
				return gfilters;
			}
		}

		public static string GetTextForEnum_Sorting(GroupFilterSorting sort)
		{
			switch (sort)
			{
				case GroupFilterSorting.AniDBRating: return "AniDB Rating";
				case GroupFilterSorting.EpisodeAddedDate: return "Episode Added Date";
				case GroupFilterSorting.EpisodeAirDate: return  "Episode Air Date";
				case GroupFilterSorting.EpisodeWatchedDate: return "Episode Watched Date";
				case GroupFilterSorting.GroupName: return "Group Name";
				case GroupFilterSorting.SortName: return "Sort Name";
				case GroupFilterSorting.MissingEpisodeCount: return "Missing Episode Count";
				case GroupFilterSorting.SeriesAddedDate: return "Series Added Date";
				case GroupFilterSorting.SeriesCount: return "Series Count";
				case GroupFilterSorting.UnwatchedEpisodeCount: return "Unwatched Episode Count";
				case GroupFilterSorting.UserRating: return "User Rating";
				case GroupFilterSorting.Year: return "Year";
				default: return "AniDB Rating";
			}
		}

		public static GroupFilterSorting GetEnumForText_Sorting(string enumDesc)
		{
			if (enumDesc == "AniDB Rating") return GroupFilterSorting.AniDBRating;
			if (enumDesc == "Episode Added Date") return GroupFilterSorting.EpisodeAddedDate;
			if (enumDesc == "Episode Air Date") return GroupFilterSorting.EpisodeAirDate;
			if (enumDesc == "Episode Watched Date") return GroupFilterSorting.EpisodeWatchedDate;
			if (enumDesc == "Group Name") return GroupFilterSorting.GroupName;
			if (enumDesc == "Sort Name") return GroupFilterSorting.SortName;
			if (enumDesc == "Missing Episode Count") return GroupFilterSorting.MissingEpisodeCount;
			if (enumDesc == "Series Added Date") return GroupFilterSorting.SeriesAddedDate;
			if (enumDesc == "Series Count") return GroupFilterSorting.SeriesCount;
			if (enumDesc == "Unwatched Episode Count") return GroupFilterSorting.UnwatchedEpisodeCount;
			if (enumDesc == "User Rating") return GroupFilterSorting.UserRating;
			if (enumDesc == "Year") return GroupFilterSorting.Year;


			return GroupFilterSorting.AniDBRating;
		}

		public static string GetTextForEnum_SortDirection(GroupFilterSortDirection sort)
		{
			switch (sort)
			{
				case GroupFilterSortDirection.Asc: return "Asc";
				case GroupFilterSortDirection.Desc: return "Desc";
				default: return "Asc";
			}
		}

		public static GroupFilterSortDirection GetEnumForText_SortDirection(string enumDesc)
		{
			if (enumDesc == "Asc") return GroupFilterSortDirection.Asc;
			if (enumDesc == "Desc") return GroupFilterSortDirection.Desc;

			return GroupFilterSortDirection.Asc;
		}

		/*
		public static string GetTextForEnum_Operator(GroupFilterOperator op)
		{
			switch (op)
			{
				case GroupFilterOperator.Equals: return Properties.Resources.GroupFilterOperator_Equals;
				case GroupFilterOperator.NotEquals: return Properties.Resources.GroupFilterOperator_NotEquals;
				case GroupFilterOperator.Exclude: return Properties.Resources.GroupFilterOperator_Exclude;
				case GroupFilterOperator.Include: return Properties.Resources.GroupFilterOperator_Include;
				case GroupFilterOperator.GreaterThan: return Properties.Resources.GroupFilterOperator_GreaterThan;
				case GroupFilterOperator.LessThan: return Properties.Resources.GroupFilterOperator_LessThan;
				case GroupFilterOperator.In: return Properties.Resources.GroupFilterOperator_In;
				case GroupFilterOperator.NotIn: return Properties.Resources.GroupFilterOperator_NotIn;
				case GroupFilterOperator.InAllEpisodes: return Properties.Resources.GroupFilterOperator_InAllEpisodes;
				case GroupFilterOperator.NotInAllEpisodes: return Properties.Resources.GroupFilterOperator_NotInAllEpisodes;
				case GroupFilterOperator.LastXDays: return Properties.Resources.GroupFilterOperator_LastXDays;

				default: return Properties.Resources.GroupFilterOperator_Equals;
			}
		}

		public static GroupFilterOperator GetEnumForText_Operator(string enumDesc)
		{
			if (enumDesc == Properties.Resources.GroupFilterOperator_Equals) return GroupFilterOperator.Equals;
			if (enumDesc == Properties.Resources.GroupFilterOperator_NotEquals) return GroupFilterOperator.NotEquals;
			if (enumDesc == Properties.Resources.GroupFilterOperator_Exclude) return GroupFilterOperator.Exclude;
			if (enumDesc == Properties.Resources.GroupFilterOperator_Include) return GroupFilterOperator.Include;
			if (enumDesc == Properties.Resources.GroupFilterOperator_GreaterThan) return GroupFilterOperator.GreaterThan;
			if (enumDesc == Properties.Resources.GroupFilterOperator_LessThan) return GroupFilterOperator.LessThan;
			if (enumDesc == Properties.Resources.GroupFilterOperator_In) return GroupFilterOperator.In;
			if (enumDesc == Properties.Resources.GroupFilterOperator_NotIn) return GroupFilterOperator.NotIn;
			if (enumDesc == Properties.Resources.GroupFilterOperator_InAllEpisodes) return GroupFilterOperator.InAllEpisodes;
			if (enumDesc == Properties.Resources.GroupFilterOperator_NotInAllEpisodes) return GroupFilterOperator.NotInAllEpisodes;
			if (enumDesc == Properties.Resources.GroupFilterOperator_LastXDays) return GroupFilterOperator.LastXDays;


			return GroupFilterOperator.Equals;
		}

		public static string GetTextForEnum_ConditionType(GroupFilterConditionType conditionType)
		{
			switch (conditionType)
			{
				case GroupFilterConditionType.AirDate: return Properties.Resources.GroupFilterConditionType_AirDate;
				case GroupFilterConditionType.AllEpisodesWatched: return Properties.Resources.GroupFilterConditionType_AllEpisodesWatched;
				case GroupFilterConditionType.AnimeGroup: return Properties.Resources.GroupFilterConditionType_AnimeGroup;
				case GroupFilterConditionType.AnimeType: return Properties.Resources.GroupFilterConditionType_AnimeType;
				case GroupFilterConditionType.AssignedTvDBInfo: return Properties.Resources.GroupFilterConditionType_AssignedTvDBInfo;
				case GroupFilterConditionType.AssignedMovieDBInfo: return Properties.Resources.GroupFilterConditionType_AssignedMovieDBInfo;
				case GroupFilterConditionType.AssignedTvDBOrMovieDBInfo: return Properties.Resources.GroupFilterConditionType_AssignedTvDBOrMovieDBInfo;
				case GroupFilterConditionType.Category: return Properties.Resources.GroupFilterConditionType_Category;
				case GroupFilterConditionType.CompletedSeries: return Properties.Resources.GroupFilterConditionType_CompletedSeries;
				case GroupFilterConditionType.Favourite: return Properties.Resources.GroupFilterConditionType_Favourite;
				case GroupFilterConditionType.HasUnwatchedEpisodes: return Properties.Resources.GroupFilterConditionType_HasUnwatchedEpisodes;
				case GroupFilterConditionType.MissingEpisodes: return Properties.Resources.GroupFilterConditionType_MissingEpisodes;
				case GroupFilterConditionType.MissingEpisodesCollecting: return Properties.Resources.GroupFilterConditionType_MissingEpisodesCollecting;
				case GroupFilterConditionType.ReleaseGroup: return Properties.Resources.GroupFilterConditionType_ReleaseGroup;
				case GroupFilterConditionType.Studio: return Properties.Resources.GroupFilterConditionType_Studio;
				case GroupFilterConditionType.UserVoted: return Properties.Resources.GroupFilterConditionType_UserVoted;
				case GroupFilterConditionType.VideoQuality: return Properties.Resources.GroupFilterConditionType_VideoQuality;
				case GroupFilterConditionType.AniDBRating: return Properties.Resources.GroupFilterConditionType_AniDBRating;
				case GroupFilterConditionType.UserRating: return Properties.Resources.GroupFilterConditionType_UserRating;
				case GroupFilterConditionType.SeriesCreatedDate: return Properties.Resources.GroupFilterConditionType_SeriesDate;
				case GroupFilterConditionType.EpisodeAddedDate: return Properties.Resources.GroupFilterConditionType_EpisodeAddedDate;
				case GroupFilterConditionType.EpisodeWatchedDate: return Properties.Resources.GroupFilterConditionType_EpisodeWatchedDate;
				case GroupFilterConditionType.FinishedAiring: return Properties.Resources.GroupFilterConditionType_FinishedAiring;
				case GroupFilterConditionType.AudioLanguage: return Properties.Resources.GroupFilterConditionType_AudioLanguage;
				case GroupFilterConditionType.SubtitleLanguage: return Properties.Resources.GroupFilterConditionType_SubtitleLanguage;
				default: return Properties.Resources.GroupFilterConditionType_AirDate;
			}
		}

		public static GroupFilterConditionType GetEnumForText_ConditionType(string enumDesc)
		{
			if (enumDesc == Properties.Resources.GroupFilterConditionType_AirDate) return GroupFilterConditionType.AirDate;
			if (enumDesc == Properties.Resources.GroupFilterConditionType_AllEpisodesWatched) return GroupFilterConditionType.AllEpisodesWatched;
			if (enumDesc == Properties.Resources.GroupFilterConditionType_AnimeGroup) return GroupFilterConditionType.AnimeGroup;
			if (enumDesc == Properties.Resources.GroupFilterConditionType_AnimeType) return GroupFilterConditionType.AnimeType;
			if (enumDesc == Properties.Resources.GroupFilterConditionType_AssignedTvDBInfo) return GroupFilterConditionType.AssignedTvDBInfo;
			if (enumDesc == Properties.Resources.GroupFilterConditionType_AssignedMovieDBInfo) return GroupFilterConditionType.AssignedMovieDBInfo;
			if (enumDesc == Properties.Resources.GroupFilterConditionType_AssignedTvDBOrMovieDBInfo) return GroupFilterConditionType.AssignedTvDBOrMovieDBInfo;
			if (enumDesc == Properties.Resources.GroupFilterConditionType_Category) return GroupFilterConditionType.Category;
			if (enumDesc == Properties.Resources.GroupFilterConditionType_CompletedSeries) return GroupFilterConditionType.CompletedSeries;
			if (enumDesc == Properties.Resources.GroupFilterConditionType_Favourite) return GroupFilterConditionType.Favourite;
			if (enumDesc == Properties.Resources.GroupFilterConditionType_HasUnwatchedEpisodes) return GroupFilterConditionType.HasUnwatchedEpisodes;
			if (enumDesc == Properties.Resources.GroupFilterConditionType_MissingEpisodes) return GroupFilterConditionType.MissingEpisodes;
			if (enumDesc == Properties.Resources.GroupFilterConditionType_MissingEpisodesCollecting) return GroupFilterConditionType.MissingEpisodesCollecting;
			if (enumDesc == Properties.Resources.GroupFilterConditionType_ReleaseGroup) return GroupFilterConditionType.ReleaseGroup;
			if (enumDesc == Properties.Resources.GroupFilterConditionType_Studio) return GroupFilterConditionType.Studio;
			if (enumDesc == Properties.Resources.GroupFilterConditionType_UserVoted) return GroupFilterConditionType.UserVoted;
			if (enumDesc == Properties.Resources.GroupFilterConditionType_VideoQuality) return GroupFilterConditionType.VideoQuality;
			if (enumDesc == Properties.Resources.GroupFilterConditionType_AniDBRating) return GroupFilterConditionType.AniDBRating;
			if (enumDesc == Properties.Resources.GroupFilterConditionType_UserRating) return GroupFilterConditionType.UserRating;
			if (enumDesc == Properties.Resources.GroupFilterConditionType_SeriesDate) return GroupFilterConditionType.SeriesCreatedDate;
			if (enumDesc == Properties.Resources.GroupFilterConditionType_EpisodeAddedDate) return GroupFilterConditionType.EpisodeAddedDate;
			if (enumDesc == Properties.Resources.GroupFilterConditionType_EpisodeWatchedDate) return GroupFilterConditionType.EpisodeWatchedDate;
			if (enumDesc == Properties.Resources.GroupFilterConditionType_FinishedAiring) return GroupFilterConditionType.FinishedAiring;
			if (enumDesc == Properties.Resources.GroupFilterConditionType_AudioLanguage) return GroupFilterConditionType.AudioLanguage;
			if (enumDesc == Properties.Resources.GroupFilterConditionType_SubtitleLanguage) return GroupFilterConditionType.SubtitleLanguage;

			return GroupFilterConditionType.AirDate;
		}
		
		public static List<string> GetAllConditionTypes()
		{
			List<string> cons = new List<string>();

			cons.Add(GetTextForEnum_ConditionType(GroupFilterConditionType.AirDate));
			cons.Add(GetTextForEnum_ConditionType(GroupFilterConditionType.AnimeType));
			cons.Add(GetTextForEnum_ConditionType(GroupFilterConditionType.AnimeGroup));
			cons.Add(GetTextForEnum_ConditionType(GroupFilterConditionType.HasUnwatchedEpisodes));
			cons.Add(GetTextForEnum_ConditionType(GroupFilterConditionType.MissingEpisodes));
			cons.Add(GetTextForEnum_ConditionType(GroupFilterConditionType.MissingEpisodesCollecting));
			cons.Add(GetTextForEnum_ConditionType(GroupFilterConditionType.CompletedSeries));
			cons.Add(GetTextForEnum_ConditionType(GroupFilterConditionType.Favourite));
			cons.Add(GetTextForEnum_ConditionType(GroupFilterConditionType.VideoQuality));
			cons.Add(GetTextForEnum_ConditionType(GroupFilterConditionType.AssignedTvDBInfo));
			cons.Add(GetTextForEnum_ConditionType(GroupFilterConditionType.AssignedMovieDBInfo));
			cons.Add(GetTextForEnum_ConditionType(GroupFilterConditionType.AssignedTvDBOrMovieDBInfo));
			cons.Add(GetTextForEnum_ConditionType(GroupFilterConditionType.Category));
			//cons.Add(GetTextForEnum_ConditionType(GroupFilterConditionType.ReleaseGroup));
			//cons.Add(GetTextForEnum_ConditionType(GroupFilterConditionType.Studio));
			cons.Add(GetTextForEnum_ConditionType(GroupFilterConditionType.UserVoted));
			cons.Add(GetTextForEnum_ConditionType(GroupFilterConditionType.AniDBRating));
			cons.Add(GetTextForEnum_ConditionType(GroupFilterConditionType.UserRating));
			cons.Add(GetTextForEnum_ConditionType(GroupFilterConditionType.SeriesCreatedDate));
			cons.Add(GetTextForEnum_ConditionType(GroupFilterConditionType.EpisodeAddedDate));
			cons.Add(GetTextForEnum_ConditionType(GroupFilterConditionType.EpisodeWatchedDate));
			cons.Add(GetTextForEnum_ConditionType(GroupFilterConditionType.FinishedAiring));
			cons.Add(GetTextForEnum_ConditionType(GroupFilterConditionType.AudioLanguage));
			cons.Add(GetTextForEnum_ConditionType(GroupFilterConditionType.SubtitleLanguage));

			cons.Sort();

			return cons;
		}*/

		public static List<string> GetAllSortTypes()
		{
			List<string> cons = new List<string>();

			cons.Add(GetTextForEnum_Sorting(GroupFilterSorting.AniDBRating));
			cons.Add(GetTextForEnum_Sorting(GroupFilterSorting.EpisodeAddedDate));
			cons.Add(GetTextForEnum_Sorting(GroupFilterSorting.EpisodeAirDate));
			cons.Add(GetTextForEnum_Sorting(GroupFilterSorting.EpisodeWatchedDate));
			cons.Add(GetTextForEnum_Sorting(GroupFilterSorting.GroupName));
			cons.Add(GetTextForEnum_Sorting(GroupFilterSorting.MissingEpisodeCount));
			cons.Add(GetTextForEnum_Sorting(GroupFilterSorting.SeriesAddedDate));
			cons.Add(GetTextForEnum_Sorting(GroupFilterSorting.SeriesCount));
			cons.Add(GetTextForEnum_Sorting(GroupFilterSorting.SortName));
			cons.Add(GetTextForEnum_Sorting(GroupFilterSorting.UnwatchedEpisodeCount));
			cons.Add(GetTextForEnum_Sorting(GroupFilterSorting.UserRating));
			cons.Add(GetTextForEnum_Sorting(GroupFilterSorting.Year));

			cons.Sort();

			return cons;
		}

		public static List<string> GetQuickSortTypes()
		{
			List<string> cons = new List<string>();

			//cons.Add(GetTextForEnum_Sorting(GroupFilterSorting.AniDBRating)); removed for performance reasons
			cons.Add(GetTextForEnum_Sorting(GroupFilterSorting.EpisodeAddedDate));
			//cons.Add(GetTextForEnum_Sorting(GroupFilterSorting.EpisodeAirDate));
			cons.Add(GetTextForEnum_Sorting(GroupFilterSorting.EpisodeWatchedDate));
			cons.Add(GetTextForEnum_Sorting(GroupFilterSorting.GroupName));
			cons.Add(GetTextForEnum_Sorting(GroupFilterSorting.MissingEpisodeCount));
			cons.Add(GetTextForEnum_Sorting(GroupFilterSorting.SeriesAddedDate));
			cons.Add(GetTextForEnum_Sorting(GroupFilterSorting.SeriesCount));
			cons.Add(GetTextForEnum_Sorting(GroupFilterSorting.SortName));
			cons.Add(GetTextForEnum_Sorting(GroupFilterSorting.UnwatchedEpisodeCount));
			cons.Add(GetTextForEnum_Sorting(GroupFilterSorting.UserRating));
			cons.Add(GetTextForEnum_Sorting(GroupFilterSorting.Year));

			cons.Sort();

			return cons;
		}

		/*
		public static List<string> GetAllowedOperators(GroupFilterConditionType conditionType)
		{
			List<string> ops = new List<string>();

			switch (conditionType)
			{
				case GroupFilterConditionType.AirDate:
					ops.Add(GetTextForEnum_Operator(GroupFilterOperator.GreaterThan));
					ops.Add(GetTextForEnum_Operator(GroupFilterOperator.LessThan));
					ops.Add(GetTextForEnum_Operator(GroupFilterOperator.LastXDays));
					break;
				case GroupFilterConditionType.SeriesCreatedDate:
					ops.Add(GetTextForEnum_Operator(GroupFilterOperator.GreaterThan));
					ops.Add(GetTextForEnum_Operator(GroupFilterOperator.LessThan));
					ops.Add(GetTextForEnum_Operator(GroupFilterOperator.LastXDays));
					break;
				case GroupFilterConditionType.EpisodeWatchedDate:
					ops.Add(GetTextForEnum_Operator(GroupFilterOperator.GreaterThan));
					ops.Add(GetTextForEnum_Operator(GroupFilterOperator.LessThan));
					ops.Add(GetTextForEnum_Operator(GroupFilterOperator.LastXDays));
					break;
				case GroupFilterConditionType.EpisodeAddedDate:
					ops.Add(GetTextForEnum_Operator(GroupFilterOperator.GreaterThan));
					ops.Add(GetTextForEnum_Operator(GroupFilterOperator.LessThan));
					ops.Add(GetTextForEnum_Operator(GroupFilterOperator.LastXDays));
					break;
				case GroupFilterConditionType.AllEpisodesWatched:
					ops.Add(GetTextForEnum_Operator(GroupFilterOperator.Include));
					ops.Add(GetTextForEnum_Operator(GroupFilterOperator.Exclude));
					break;
				case GroupFilterConditionType.AnimeGroup:
					ops.Add(GetTextForEnum_Operator(GroupFilterOperator.Equals));
					ops.Add(GetTextForEnum_Operator(GroupFilterOperator.NotEquals));
					break;
				case GroupFilterConditionType.AnimeType:
					ops.Add(GetTextForEnum_Operator(GroupFilterOperator.In));
					ops.Add(GetTextForEnum_Operator(GroupFilterOperator.NotIn));
					break;
				case GroupFilterConditionType.AssignedTvDBInfo:
					ops.Add(GetTextForEnum_Operator(GroupFilterOperator.Include));
					ops.Add(GetTextForEnum_Operator(GroupFilterOperator.Exclude));
					break;
				case GroupFilterConditionType.AssignedMovieDBInfo:
					ops.Add(GetTextForEnum_Operator(GroupFilterOperator.Include));
					ops.Add(GetTextForEnum_Operator(GroupFilterOperator.Exclude));
					break;
				case GroupFilterConditionType.AssignedTvDBOrMovieDBInfo:
					ops.Add(GetTextForEnum_Operator(GroupFilterOperator.Include));
					ops.Add(GetTextForEnum_Operator(GroupFilterOperator.Exclude));
					break;
				case GroupFilterConditionType.Category:
					ops.Add(GetTextForEnum_Operator(GroupFilterOperator.In));
					ops.Add(GetTextForEnum_Operator(GroupFilterOperator.NotIn));
					break;
				case GroupFilterConditionType.AudioLanguage:
					ops.Add(GetTextForEnum_Operator(GroupFilterOperator.In));
					ops.Add(GetTextForEnum_Operator(GroupFilterOperator.NotIn));
					break;
				case GroupFilterConditionType.SubtitleLanguage:
					ops.Add(GetTextForEnum_Operator(GroupFilterOperator.In));
					ops.Add(GetTextForEnum_Operator(GroupFilterOperator.NotIn));
					break;
				case GroupFilterConditionType.CompletedSeries:
					ops.Add(GetTextForEnum_Operator(GroupFilterOperator.Include));
					ops.Add(GetTextForEnum_Operator(GroupFilterOperator.Exclude));
					break;
				case GroupFilterConditionType.FinishedAiring:
					ops.Add(GetTextForEnum_Operator(GroupFilterOperator.Include));
					ops.Add(GetTextForEnum_Operator(GroupFilterOperator.Exclude));
					break;
				case GroupFilterConditionType.Favourite:
					ops.Add(GetTextForEnum_Operator(GroupFilterOperator.Include));
					ops.Add(GetTextForEnum_Operator(GroupFilterOperator.Exclude));
					break;
				case GroupFilterConditionType.HasUnwatchedEpisodes:
					ops.Add(GetTextForEnum_Operator(GroupFilterOperator.Include));
					ops.Add(GetTextForEnum_Operator(GroupFilterOperator.Exclude));
					break;
				case GroupFilterConditionType.MissingEpisodes:
					ops.Add(GetTextForEnum_Operator(GroupFilterOperator.Include));
					ops.Add(GetTextForEnum_Operator(GroupFilterOperator.Exclude));
					break;
				case GroupFilterConditionType.MissingEpisodesCollecting:
					ops.Add(GetTextForEnum_Operator(GroupFilterOperator.Include));
					ops.Add(GetTextForEnum_Operator(GroupFilterOperator.Exclude));
					break;
				case GroupFilterConditionType.ReleaseGroup:
					ops.Add(GetTextForEnum_Operator(GroupFilterOperator.In));
					ops.Add(GetTextForEnum_Operator(GroupFilterOperator.NotIn));
					break;
				case GroupFilterConditionType.Studio:
					ops.Add(GetTextForEnum_Operator(GroupFilterOperator.In));
					ops.Add(GetTextForEnum_Operator(GroupFilterOperator.NotIn));
					break;
				case GroupFilterConditionType.UserVoted:
					ops.Add(GetTextForEnum_Operator(GroupFilterOperator.Include));
					ops.Add(GetTextForEnum_Operator(GroupFilterOperator.Exclude));
					break;
				case GroupFilterConditionType.VideoQuality:
					ops.Add(GetTextForEnum_Operator(GroupFilterOperator.In));
					ops.Add(GetTextForEnum_Operator(GroupFilterOperator.NotIn));
					ops.Add(GetTextForEnum_Operator(GroupFilterOperator.InAllEpisodes));
					ops.Add(GetTextForEnum_Operator(GroupFilterOperator.NotInAllEpisodes));
					break;
				case GroupFilterConditionType.AniDBRating:
					ops.Add(GetTextForEnum_Operator(GroupFilterOperator.GreaterThan));
					ops.Add(GetTextForEnum_Operator(GroupFilterOperator.LessThan));
					break;
				case GroupFilterConditionType.UserRating:
					ops.Add(GetTextForEnum_Operator(GroupFilterOperator.GreaterThan));
					ops.Add(GetTextForEnum_Operator(GroupFilterOperator.LessThan));
					break;
			}


			return ops;
		}
		*/
		public static string GetDateAsString(DateTime aDate)
		{
			return aDate.Year.ToString().PadLeft(4, '0') +
						aDate.Month.ToString().PadLeft(2, '0') +
						aDate.Day.ToString().PadLeft(2, '0');
		}

		public static DateTime GetDateFromString(string sDate)
		{
			try
			{
				int year = int.Parse(sDate.Substring(0, 4));
				int month = int.Parse(sDate.Substring(4, 2));
				int day = int.Parse(sDate.Substring(6, 2));

				return new DateTime(year, month, day);
			}
			catch (Exception ex)
			{
				return DateTime.Today;
			}
		}

		public static string GetDateAsFriendlyString(DateTime aDate)
		{
			return aDate.ToString("dd MMM yyyy", Globals.Culture);
		}

		public static List<SortPropOrFieldAndDirection> GetSortDescriptions(GroupFilterVM gf)
		{
			List<SortPropOrFieldAndDirection> sortlist = new List<SortPropOrFieldAndDirection>();
			foreach (GroupFilterSortingCriteria gfsc in gf.SortCriteriaList)
			{
				sortlist.Add(GetSortDescription(gfsc.SortType, gfsc.SortDirection));
			}
			return sortlist;
		}

		public static SortPropOrFieldAndDirection GetSortDescription(GroupFilterSorting sortType, GroupFilterSortDirection sortDirection)
		{
			string sortColumn = "";
			bool sortDescending = sortDirection == GroupFilterSortDirection.Desc;
			SortType sortFieldType = SortType.eString;

			switch (sortType)
			{
				case GroupFilterSorting.AniDBRating:
					sortColumn = "AniDBRating"; sortFieldType = SortType.eDoubleOrFloat; break;
				case GroupFilterSorting.EpisodeAddedDate:
					sortColumn = "EpisodeAddedDate"; sortFieldType = SortType.eDateTime; break;
				case GroupFilterSorting.EpisodeAirDate:
					sortColumn = "AirDate"; sortFieldType = SortType.eDateTime; break;
				case GroupFilterSorting.EpisodeWatchedDate:
					sortColumn = "WatchedDate"; sortFieldType = SortType.eDateTime; break;
				case GroupFilterSorting.GroupName:
					sortColumn = "GroupName"; sortFieldType = SortType.eString; break;
				case GroupFilterSorting.SortName:
					sortColumn = "SortName"; sortFieldType = SortType.eString; break;
				case GroupFilterSorting.MissingEpisodeCount:
					sortColumn = "MissingEpisodeCount"; sortFieldType = SortType.eInteger; break;
				case GroupFilterSorting.SeriesAddedDate:
					sortColumn = "Stat_SeriesCreatedDate"; sortFieldType = SortType.eDateTime; break;
				case GroupFilterSorting.SeriesCount:
					sortColumn = "AllSeriesCount"; sortFieldType = SortType.eInteger; break;
				case GroupFilterSorting.UnwatchedEpisodeCount:
					sortColumn = "UnwatchedEpisodeCount"; sortFieldType = SortType.eInteger; break;
				case GroupFilterSorting.UserRating:
					sortColumn = "Stat_UserVoteOverall"; sortFieldType = SortType.eDoubleOrFloat; break;
				case GroupFilterSorting.Year:
					if (sortDirection == GroupFilterSortDirection.Asc)
						sortColumn = "Stat_AirDate_Min";
					else
						sortColumn = "Stat_AirDate_Max";

					sortFieldType = SortType.eDateTime;
					break;
				default:
					sortColumn = "GroupName"; sortFieldType = SortType.eString; break;
			}

			return new SortPropOrFieldAndDirection(sortColumn, sortDescending, sortFieldType);
		}
	}
}
