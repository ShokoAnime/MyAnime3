using System;
using System.Collections.Generic;
using System.Text;

using BinaryNorthwest;
using MyAnimePlugin3.ViewModel;


namespace MyAnimePlugin3
{
	public class FacadeHelper
	{

		public delegate void ForEachOperation<T>(T element, int currIndex);

		/// <summary>
		/// Performs an operation for each element in the list, by starting with a specific index and working its way around it (eg: n, n+1, n-1, n+2, n-2, ...)
		/// </summary>
		/// <typeparam name="T">The Type of elements in the IList</typeparam>
		/// <param name="elements">All elements, this value cannot be null</param>
		/// <param name="startElement">The starting point for the operation (0 operates like a traditional foreach loop)</param>
		/// <param name="operation">The operation to perform on each element</param>
		public static void ProximityForEach<T>(IList<T> elements, int startElement, ForEachOperation<T> operation)
		{
			if (elements == null)
				throw new ArgumentNullException("elements");
			if ((startElement >= elements.Count && elements.Count > 0) || startElement < 0)
				throw new ArgumentOutOfRangeException("startElement", startElement, "startElement must be > 0 and <= elements.Count (" + elements.Count + ")");
			if (elements.Count > 0)                                      // if empty list, nothing to do, but legal, so not an exception
			{
				T item;
				for (int lower = startElement, upper = startElement + 1; // start with the selected, and then go down before going up
					 upper < elements.Count || lower >= 0;               // only exit once both ends have been reached
					 lower--, upper++)
				{
					if (lower >= 0)                                      // are lower elems left?
					{
						item = elements[lower];
						operation(item, lower);
						elements[lower] = item;
					}
					if (upper < elements.Count)                          // are higher elems left?
					{
						item = elements[upper];
						operation(item, upper);
						elements[upper] = item;
					}
				}
			}
		}

        


		public static List<GroupFilterVM> GetGroupFilters()
		{
			return GroupFilterHelper.AllGroupFilters;
		}

		public static List<GroupFilterVM> GetTopLevelPredefinedGroupFilters()
		{
			List<GroupFilterVM> gfs = new List<GroupFilterVM>();

			GroupFilterVM gf = new GroupFilterVM();
			gf.GroupFilterID = Constants.StaticGF.Predefined_Years;
			gf.FilterConditions = new List<GroupFilterConditionVM>();
			gf.ApplyToSeries = 0;
			gf.BaseCondition = 1;
			gf.PredefinedCriteria = "";
			gf.GroupFilterName = "By Year";

			gfs.Add(gf);

			GroupFilterVM gfGenres = new GroupFilterVM();
			gfGenres.GroupFilterID = Constants.StaticGF.Predefined_Categories;
			gfGenres.FilterConditions = new List<GroupFilterConditionVM>();
			gfGenres.ApplyToSeries = 0;
			gfGenres.BaseCondition = 1;
			gfGenres.PredefinedCriteria = "";
			gfGenres.GroupFilterName = "By Category";

			gfs.Add(gfGenres);

			return gfs;
		}

		public static List<GroupFilterVM> GetGroupFiltersForPredefined(GroupFilterVM pre)
		{
			List<GroupFilterVM> gfs = new List<GroupFilterVM>();

			if (pre.GroupFilterID.Value == Constants.StaticGF.Predefined_Years)
			{
				List<int> years = new List<int>();

				List<JMMServerBinary.Contract_AnimeSeries> contracts = JMMServerVM.Instance.clientBinaryHTTP.GetAllSeries(JMMServerVM.Instance.CurrentUser.JMMUserID);
				foreach (JMMServerBinary.Contract_AnimeSeries serContract in contracts)
				{
					AnimeSeriesVM ser = new AnimeSeriesVM(serContract);

					int startYear = 0;
					if (!ser.Stat_AirDate_Min.HasValue) continue;
					startYear = ser.Stat_AirDate_Min.Value.Year;

					int endYear = DateTime.Now.AddYears(1).Year;
					if (ser.Stat_AirDate_Max.HasValue)
						endYear = ser.Stat_AirDate_Max.Value.Year;

					for (int i = startYear; i <= endYear; i++)
					{
						if (!years.Contains(i)) years.Add(i);
					}
				}

				years.Sort();

				foreach (int yr in years)
				{
					GroupFilterVM gf = new GroupFilterVM();
					gf.GroupFilterID = Constants.StaticGF.Predefined_Years_Child;
					gf.FilterConditions = new List<GroupFilterConditionVM>();
					gf.ApplyToSeries = 0;
					gf.BaseCondition = 1;
					gf.GroupFilterName = yr.ToString();
					gf.PredefinedCriteria = yr.ToString();

					gfs.Add(gf);
				}
			}
			else if (pre.GroupFilterID.Value == Constants.StaticGF.Predefined_Categories)
			{
				List<string> categories = new List<string>();

				List<AnimeGroupVM> grps = JMMServerHelper.GetAnimeGroupsForFilter(GroupFilterHelper.AllGroupsFilter);
				foreach (AnimeGroupVM grp in grps)
				{
					foreach (string cat in grp.Categories)
					{
						if (!categories.Contains(cat) && !string.IsNullOrEmpty(cat))
							categories.Add(cat);
					}
				}

				categories.Sort();

				foreach (string cat in categories)
				{
					GroupFilterVM gf = new GroupFilterVM();
					gf.GroupFilterID = Constants.StaticGF.Predefined_Categories_Child;
					gf.FilterConditions = new List<GroupFilterConditionVM>();
					gf.ApplyToSeries = 0;
					gf.BaseCondition = 1;
					gf.GroupFilterName = cat;
					gf.PredefinedCriteria = cat;

					gfs.Add(gf);
				}
			}

			return gfs;
		}
	}
}
