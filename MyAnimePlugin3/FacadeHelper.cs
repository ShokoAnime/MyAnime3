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

		//TODO
		/*
		public static List<AnimeGroup> GetGroups(View view)
		{
			List<GroupFilterVM> filters = GetGroupFilters();

			double totalTime = 0;
			double totalTime3 = 0;
			double totalTime4 = 0;
			double totalTime5 = 0;
			double totalTime6 = 0;
			DateTime start = DateTime.Now;

			if (view == null)
                return AnimeGroup.GetAll();
            List<string> queries = new List<string>();
            List<object> objs=new List<object>();
		    //favorite
            if (view.ShowTypeFavorite != View.eShowType.ignore)
            {
                queries.Add(view.ShowTypeFavorite == View.eShowType.show ? "IsFave = 1" : "IsFave = 0");
            }
            //unwatched
            if (view.ShowTypeWatched != View.eShowType.ignore)
            {
                queries.Add(view.ShowTypeWatched == View.eShowType.show ? "UnwatchedEpisodeCount <=0" : "UnwatchedEpisodeCount > 0");
                
            }
            //recently watched
            if (view.ShowTypeRecentlyWatched != View.eShowType.ignore)
            {
                DateTime date = DateTime.Now.Subtract(new TimeSpan(7, 0, 0, 0));
                queries.Add(view.ShowTypeRecentlyWatched == View.eShowType.show
                                ? "WatchedDate>={"+objs.Count+"}"
                                : "(IFNULL(WatchedDate,'')='' OR WatchedDate<{"+objs.Count+"})");
                objs.Add(date);
               
            }
            //completed
            if (view.ShowTypeCompleted!=View.eShowType.ignore)
            {
                string q = view.ShowTypeCompleted == View.eShowType.show ? ">=" : "<";
                queries.Add(string.Format("AnimeGroupID IN (SELECT AnimeGroupID FROM AnimeSeries INNER JOIN AniDB_Anime ON AnimeSeries.AniDB_ID=AniDB_Anime.AnimeID WHERE AnimeSeries.LatestLocalEpisodeNumber {0} AniDB_Anime.EpisodeCountNormal)",q));
            }
		    //new season
            if (view.ShowTypeNewSeason != View.eShowType.ignore)
            {
                string anidate = Utils.AniDBDate(DateTime.Now.Subtract(new TimeSpan(31, 0, 0, 0)));
                string cmp = "<";
                if (view.ShowTypeNewSeason == View.eShowType.show)
                    cmp = ">=";
                string bquery = "(AnimeGroupID IN (SELECT AnimeGroupID FROM AnimeSeries WHERE AniDB_ID IN (SELECT AnimeID AS AniDB_ID FROM AniDB_Anime WHERE AirDate "+cmp+"{"+objs.Count+"})))";
                queries.Add(bquery);
                objs.Add(anidate);
            }

			//blu ray
			if (view.ShowTypeBluRay != View.eShowType.ignore)
			{
				if (view.ShowTypeBluRay == View.eShowType.show)
					queries.Add("AnimeGroupID IN (SELECT AnimeGroupID FROM AnimeSeries  INNER JOIN AniDB_File ON AnimeSeries.AniDB_ID=AniDB_File.AnimeID  WHERE AniDB_File.File_Source = 'Blu-ray')");
				else
					queries.Add("AnimeGroupID IN (SELECT AnimeGroupID FROM AnimeSeries  INNER JOIN AniDB_File ON AnimeSeries.AniDB_ID=AniDB_File.AnimeID  WHERE AniDB_File.File_Source <> 'Blu-ray')");
			}

			//DVD
			if (view.ShowTypeDVD != View.eShowType.ignore)
			{
				if (view.ShowTypeDVD == View.eShowType.show)
					queries.Add("AnimeGroupID IN (SELECT AnimeGroupID FROM AnimeSeries  INNER JOIN AniDB_File ON AnimeSeries.AniDB_ID=AniDB_File.AnimeID  WHERE AniDB_File.File_Source = 'DVD')");
				else
					queries.Add("AnimeGroupID IN (SELECT AnimeGroupID FROM AnimeSeries  INNER JOIN AniDB_File ON AnimeSeries.AniDB_ID=AniDB_File.AnimeID  WHERE AniDB_File.File_Source <> 'DVD')");
			}

            Dictionary<string,int> gens=new Dictionary<string, int>();


            //AdultContent
		    if (BaseConfig.Settings.HideRestrictedSeries)
            {
                Genre gg = Genre.Find("18 Restricted");
                if ((gg!=null) && (gg.GenreID.HasValue))
                    gens.Add(gg.GenreLarge,-gg.GenreID.Value);
            }

            if ((view.ShowTypeAdultContent != View.eShowType.ignore) && (gens.Count==0))
            {
                Genre gg = Genre.Find("18 Restricted");
                if ((gg!=null) && (gg.GenreID.HasValue))
                {
                    int val=gg.GenreID.Value;
                    if (view.ShowTypeAdultContent != View.eShowType.show)
                        val=-val;
                    gens.Add(gg.GenreLarge, val);
                }
            }
		    string languagesshow = "";
		    string languageshide = "";
            view.GetAudioLanguages(ref languagesshow, ref languageshide);
		    string subsshow = "";
		    string subshide = "";
            view.GetSubtitleLanguages(ref subsshow, ref subshide);
            List<Language> langs=new List<Language>();
		    if ((languagesshow.Length>0) || (languageshide.Length>0) || (subsshow.Length>0) || (subshide.Length>0))
                langs=Language.GetAll();
		    string rq = "AnimeGroupID IN (SELECT AnimeGroupID FROM AnimeSeries WHERE {0})";
            if ((languagesshow.Length>0) || (languageshide.Length>0))
            {               
               queries.Add(string.Format(rq,GetQueryLanguage("Languages",langs, languagesshow, languageshide,ref objs)));
            }
            if ((subsshow.Length>0) || (subshide.Length>0))
            {
               queries.Add(string.Format(rq,GetQueryLanguage("Subtitles",langs, subsshow, subshide,ref objs)));
            }

            // Anime Types
            string AnimeTypesShow = "";
            string AnimeTypesHide = "";
            String AnimeHideTypesOfInt = "";
            String AnimeShowTypesOfInt = "";
            view.GetAnimeTypes(ref AnimeTypesShow, ref AnimeTypesHide);

            if(!String.IsNullOrEmpty(AnimeTypesHide))
            {
                // convert string AnimeTypes to their enum Int value
                String[] AnimeTypesHideStringList;

                AnimeTypesHideStringList = AnimeTypesHide.Split(',');

                List<int> AnimeTypeHideIntList = new List<int>();

                foreach(String AnimeHideType in AnimeTypesHideStringList)
                {
                    String AnimeHideTypeEnum = AnimeHideType.Replace(' ', '_');
                    AniDB_Anime.AnimeTypes AnimeType = (AniDB_Anime.AnimeTypes)Enum.Parse(typeof(AniDB_Anime.AnimeTypes), AnimeHideTypeEnum, true);
                    AnimeTypeHideIntList.Add((int)AnimeType);
                }

                //turn list of int's into a string
                foreach(int AnimeTypeInt in AnimeTypeHideIntList)
                    AnimeHideTypesOfInt += AnimeTypeInt.ToString() + ",";

                // remove extra , at end of string
                if(AnimeHideTypesOfInt.Length > 0)
                    AnimeHideTypesOfInt = AnimeHideTypesOfInt.Remove(AnimeHideTypesOfInt.Length - 1);
            }
            if (!String.IsNullOrEmpty(AnimeTypesShow))
            {
                String[] AnimeTypesShowStringList;

                AnimeTypesShowStringList = AnimeTypesShow.Split(',');

                List<int> AnimeTypeShowIntList = new List<int>();

                foreach (String AnimeShowType in AnimeTypesShowStringList)
                {
                    String AnimeShowTypeEnum = AnimeShowType.Replace(' ', '_');
                    AniDB_Anime.AnimeTypes AnimeType = (AniDB_Anime.AnimeTypes)Enum.Parse(typeof(AniDB_Anime.AnimeTypes), AnimeShowTypeEnum, true);
                    AnimeTypeShowIntList.Add((int)AnimeType);
                }


                //turn list of int's into a string
                foreach (int AnimeTypeInt in AnimeTypeShowIntList)
                    AnimeShowTypesOfInt += AnimeTypeInt.ToString() + ",";

                // remove extra , at end of string
                if (AnimeShowTypesOfInt.Length > 0)
                    AnimeShowTypesOfInt = AnimeShowTypesOfInt.Remove(AnimeShowTypesOfInt.Length - 1);
            }
            
            List<AniDB_Anime.AnimeTypes> AnimeTypes = new List<AniDB_Anime.AnimeTypes>();
            if (AnimeShowTypesOfInt.Length > 0 || AnimeHideTypesOfInt.Length > 0)
            {
                foreach(AniDB_Anime.AnimeTypes AnimeType in Enum.GetValues(typeof(AniDB_Anime.AnimeTypes)))
                {
                    AnimeTypes.Add(AnimeType);
                }
            }
            string qu = "AnimeGroupID IN (SELECT AnimeGroupID FROM AnimeSeries INNER JOIN AniDB_Anime ON AnimeSeries.AniDB_ID=AniDB_Anime.AnimeID WHERE {0})";
            if (AnimeShowTypesOfInt.Length > 0 || AnimeHideTypesOfInt.Length > 0)
            {
                queries.Add(string.Format(qu,GetQueryType(AnimeTypes, AnimeShowTypesOfInt, AnimeHideTypesOfInt, ref objs)));
            }
            
            string yearshow = "";
            string yearhide = "";
            view.GetYears(ref yearshow, ref yearhide);
		    
            //year
            if ((yearhide.Length > 0) || (yearshow.Length > 0))
            {
                string q = "AnimeGroupID IN (SELECT AnimeGroupID FROM AnimeSeries INNER JOIN AniDB_Anime ON AnimeSeries.AniDB_ID=AniDB_Anime.AnimeID WHERE {0})";
                List<string> subq=new List<string>();
                string[] ys = yearshow.Split(',');
                foreach(string y in ys)
                {
                    string yy = y.Trim();
                    if (yy.Length>0)
                    {
                        subq.Add("(BeginYear<={"+objs.Count+"} AND EndYear>={"+objs.Count+"})");
                        objs.Add(int.Parse(y));
                    }
                }
                ys = yearhide.Split(',');
                foreach (string y in ys)
                {
                    string yy = y.Trim();
                    if (yy.Length > 0)
                    {
                        subq.Add("(BeginYear>{" + objs.Count + "} OR EndYear<{" + objs.Count + "})");
                        objs.Add(int.Parse(y));
                    }
                }
                string q2 = "";
                foreach (string s in subq)
                    q2 += s + " AND ";
                if (q2.Length>0)
                {
                    q2 = q2.Substring(0, q2.Length - 5);
                    queries.Add(string.Format(q,q2));
                }
            }
		    //Genres
		    string genshow="";
		    string genhide="";
		    view.GetGenres(ref genshow, ref genhide);
		    string[] genreshow = genshow.Split(',');
            string[] genrehide = genhide.Split(',');
            foreach(string g in genreshow)
            {
                
                 string ge = g.Trim();
                 if (ge.Length > 0)
                 {
                     Genre gg = Genre.Find(ge);
                     if ((gg != null) && (gg.GenreID.HasValue) && (!gens.ContainsKey(gg.GenreLarge)))
                     {
                         gens.Add(gg.GenreLarge, gg.GenreID.Value);
                     }
                 }
            }
            foreach (string g in genrehide)
            {
                string ge = g.Trim();
                if (ge.Length > 0)
                {
                    Genre gg = Genre.Find(ge);
                    if ((gg != null) && (gg.GenreID.HasValue) && (!gens.ContainsKey(gg.GenreLarge)))
                    {
                        gens.Add(gg.GenreLarge, -gg.GenreID.Value);
                    }
                }
            }
            
            
            if (gens.Count>0)
            {
                int hidecount=0;
                int showcount=0;
                foreach (string k in gens.Keys)
                {
                    if (gens[k]<0)
                        hidecount++;
                    else
                        showcount++;
                }
                string qr="";                    
                string in1 = "";
                string out1 = "";
                foreach (string k in gens.Keys)
                {
                    int val = gens[k];
                    if (val>0)
                    {
                        in1 += "{" + objs.Count + "},";
                        objs.Add(val);
                    }

                }
                foreach (string k in gens.Keys)
                {
                    int val = gens[k];
                    if (val < 0)
                    {
                        out1 += "{" + objs.Count + "},";
                        objs.Add(-val);
                    }
                }
                if (in1.Length>0)
                    in1 = in1.Substring(0, in1.Length - 1);
                if (out1.Length>0)
                    out1 = out1.Substring(0, out1.Length - 1);
                if ((showcount > 0) && (hidecount > 0))
                    qr = string.Format("AnimeGroupID IN (SELECT DISTINCT(AnimeGroupID) FROM AnimeSeries WHERE AnimeSeriesID IN (SELECT AnimeSeriesID FROM CrossRef_Genre_AnimeSeries WHERE GenreID IN ({0})) AND AnimeSeriesID NOT IN (SELECT AnimeSeriesID FROM CrossRef_Genre_AnimeSeries WHERE GenreID IN ({1})))",in1,out1);                
                else if (showcount > 0)
                    qr = string.Format("AnimeGroupID IN (SELECT DISTINCT(AnimeGroupID) FROM AnimeSeries WHERE AnimeSeriesID IN (SELECT AnimeSeriesID FROM CrossRef_Genre_AnimeSeries WHERE GenreID IN ({0})))", in1);
                else if (hidecount>0)
                    qr = string.Format("AnimeGroupID NOT IN (SELECT DISTINCT(AnimeGroupID) FROM AnimeSeries WHERE AnimeSeriesID IN (SELECT AnimeSeriesID FROM CrossRef_Genre_AnimeSeries WHERE GenreID IN ({0})))",out1);
                queries.Add(qr);
		    }
		    List<AnimeGroup> grps = new List<AnimeGroup>();
            string rquery = "";
            foreach (string query in queries)
            {
                rquery += query + " AND ";
            }
            if (rquery.Length > 0)
                rquery = rquery.Substring(0, rquery.Length - 5);
			foreach (AnimeGroup grp in AnimeGroup.GetFromQuery<AnimeGroup>(rquery,objs.ToArray()))
			{
				    
				//method:
				// - when show=false, the show isn't show (abort checks for groups)
				// - when show=true the show could be excluded by another option
				//   so keep processing
				
				bool? show = null;

			    //TODO MissingEps in AnimeSeries
				//missing eps

                if (view.ShowTypeMissingEpisodes != View.eShowType.ignore)
                {

                    bool bMissingEps = false;
                    //MainWindow.dbHelper.DeubbgingOn = true;
                    if (grp.HasMissingEpisodes)
                        bMissingEps = true;
                    //MainWindow.dbHelper.DeubbgingOn = false;
                    show = view.ShowMissingEpisodes(bMissingEps);
                    if (show.HasValue)
                    {
                        if (show.Value != true)                       
                            continue;
                    }
                }
			 
				grps.Add(grp);

				
			}

			TimeSpan ts2 = DateTime.Now - start;
			totalTime += ts2.TotalMilliseconds;

			// now sort the groups by name
			List<SortPropOrFieldAndDirection> sortCriteria = new List<SortPropOrFieldAndDirection>();
			View.eSortType sortType = ((view != null) ? view.SortType : View.eSortType.Name);
			switch (sortType)
			{
				case View.eSortType.Name:
					sortCriteria.Add(new SortPropOrFieldAndDirection("SortName", false, SortType.eString));
					break;
				case View.eSortType.AniDBRating:
					sortCriteria.Add(new SortPropOrFieldAndDirection("AniDBRating", true, SortType.eDoubleOrFloat));
					break;
				case View.eSortType.UserRating:
					sortCriteria.Add(new SortPropOrFieldAndDirection("UserRating", true, SortType.eDoubleOrFloat));
					break;
				case View.eSortType.AirDate:
					sortCriteria.Add(new SortPropOrFieldAndDirection("AirDate", true, SortType.eDateTime));
					break;
				case View.eSortType.WatchedDate:
					sortCriteria.Add(new SortPropOrFieldAndDirection("WatchedDate", true, SortType.eDateTime));
					sortCriteria.Add(new SortPropOrFieldAndDirection("SortName", false, SortType.eString));
					break;
				case View.eSortType.AddedDate:
					sortCriteria.Add(new SortPropOrFieldAndDirection("DateTimeUpdated", true, SortType.eDateTime));
					break;
				default:
					sortCriteria.Add(new SortPropOrFieldAndDirection("SortName", false, SortType.eString));
					break;
			}
			grps = Sorting.MultiSort<AnimeGroup>(grps, sortCriteria);

			BaseConfig.MyAnimeLog.Write("Total time for getting groups: {0}-{1}/{2}/{3}/{4}", grps.Count, totalTime, totalTime3, totalTime4, totalTime5);

			return grps;
		}*/
	}
}
