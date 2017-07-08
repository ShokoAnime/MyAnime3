using System.Collections.Generic;
using System.Linq;
using Shoko.Models.Enums;
using Shoko.MyAnime3.ViewModel.Server;

namespace Shoko.MyAnime3.ViewModel.Helpers
{
	public class GroupFilterHelper
	{

	    public static VM_GroupFilter AllGroupsFilter
	    {
	        get { return GetTopLevelGroupFilters().FirstOrDefault(a => a.FilterType == (int) GroupFilterType.All); }
	    }

       
		public static List<VM_GroupFilter> GetTopLevelGroupFilters()
		{
			return ShokoServerHelper.GetTopLevelGroupFilters();
		}
        public static List<VM_GroupFilter> GetChildGroupFilters(VM_GroupFilter gf)
        {
            return ShokoServerHelper.GetChildGroupFilters(gf);
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

		
	
	}
}
