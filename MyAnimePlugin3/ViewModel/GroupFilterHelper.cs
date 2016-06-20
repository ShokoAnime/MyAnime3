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
	        get { return GetTopLevelGroupFilters().FirstOrDefault(a => a.FilterType == (int) GroupFilterType.All); }
	    }

       
		public static List<GroupFilterVM> GetTopLevelGroupFilters()
		{
			return JMMServerHelper.GetTopLevelGroupFilters();
		}
        public static List<GroupFilterVM> GetChildGroupFilters(GroupFilterVM gf)
        {
            return JMMServerHelper.GetChildGroupFilters(gf);
        }
        public static string GetTextForEnum_Sorting(GroupFilterSorting sort)
		{
			switch (sort)
			{
                case GroupFilterSorting.AniDBRating: return Translation.AniDBRating;
                case GroupFilterSorting.EpisodeAddedDate: return Translation.EpisodeAddedDate;
                case GroupFilterSorting.EpisodeAirDate: return Translation.EpisodeAirDate;
                case GroupFilterSorting.EpisodeWatchedDate: return Translation.EpisodeWatchedDate;
                case GroupFilterSorting.GroupName: return Translation.GroupName;
                case GroupFilterSorting.GroupFilterName: return Translation.FilterName;
                case GroupFilterSorting.SortName: return Translation.SortName;
                case GroupFilterSorting.MissingEpisodeCount: return Translation.MissingEpisodeCount;
                case GroupFilterSorting.SeriesAddedDate: return Translation.SeriesAddedDate;
                case GroupFilterSorting.SeriesCount: return Translation.SeriesCount;
                case GroupFilterSorting.UnwatchedEpisodeCount: return Translation.UnwatchedEpisodeCount;
                case GroupFilterSorting.UserRating: return Translation.UserRating;
                case GroupFilterSorting.Year: return Translation.Year;
                default: return Translation.AniDBRating;

            }
		}

		public static GroupFilterSorting GetEnumForText_Sorting(string enumDesc)
		{
            if (enumDesc == Translation.AniDBRating) return GroupFilterSorting.AniDBRating;
            if (enumDesc == Translation.EpisodeAddedDate) return GroupFilterSorting.EpisodeAddedDate;
            if (enumDesc == Translation.EpisodeAirDate) return GroupFilterSorting.EpisodeAirDate;
            if (enumDesc == Translation.EpisodeWatchedDate) return GroupFilterSorting.EpisodeWatchedDate;
            if (enumDesc == Translation.GroupName) return GroupFilterSorting.GroupName;
		    if (enumDesc == Translation.FilterName) return GroupFilterSorting.GroupFilterName;            
            if (enumDesc == Translation.SortName) return GroupFilterSorting.SortName;
            if (enumDesc == Translation.MissingEpisodeCount) return GroupFilterSorting.MissingEpisodeCount;
            if (enumDesc == Translation.SeriesAddedDate) return GroupFilterSorting.SeriesAddedDate;
            if (enumDesc == Translation.SeriesCount) return GroupFilterSorting.SeriesCount;
            if (enumDesc == Translation.UnwatchedEpisodeCount) return GroupFilterSorting.UnwatchedEpisodeCount;
            if (enumDesc == Translation.UserRating) return GroupFilterSorting.UserRating;
            if (enumDesc == Translation.Year) return GroupFilterSorting.Year;


            return GroupFilterSorting.AniDBRating;
		}

		public static string GetTextForEnum_SortDirection(GroupFilterSortDirection sort)
		{
			switch (sort)
			{
				case GroupFilterSortDirection.Asc: return Translation.Asc;
				case GroupFilterSortDirection.Desc: return Translation.Desc;
				default: return "Asc";
			}
		}

		public static GroupFilterSortDirection GetEnumForText_SortDirection(string enumDesc)
		{
			if (enumDesc == Translation.Asc) return GroupFilterSortDirection.Asc;
			if (enumDesc == Translation.Desc) return GroupFilterSortDirection.Desc;

			return GroupFilterSortDirection.Asc;
		}

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
            cons.Add(GetTextForEnum_Sorting(GroupFilterSorting.GroupFilterName));

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
            cons.Add(GetTextForEnum_Sorting(GroupFilterSorting.GroupFilterName));

            cons.Sort();

			return cons;
		}

		
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
                case GroupFilterSorting.GroupFilterName:
                    sortColumn = "GroupFilterName"; sortFieldType = SortType.eString; break;
                default:
					sortColumn = "GroupName"; sortFieldType = SortType.eString; break;
			}

			

			return new SortPropOrFieldAndDirection(sortColumn, sortDescending, sortFieldType);
		}
	}
}
