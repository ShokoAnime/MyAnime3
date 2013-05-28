using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MediaPortal.GUI.Library;
using System.ComponentModel;
using MyAnimePlugin3.ViewModel;

namespace MyAnimePlugin3.Windows
{
	public class RandomWindow : GUIWindow
	{
		//public RandomSeriesEpisodeLevel RandomLevel { get; set; }
		//public RandomObjectType RandomType { get; set; }
		//private object LevelObject = null;
		private static Random rndm = new Random();

		[SkinControlAttribute(801)] protected GUIButtonControl btnRandom = null;
		[SkinControlAttribute(806)] protected GUIButtonControl btnAddCategory = null;
		[SkinControlAttribute(807)] protected GUIButtonControl btnClearcategories = null;
		[SkinControlAttribute(808)] protected GUIButtonControl btnAllAnycategories = null;
		[SkinControlAttribute(810)] protected GUIButtonControl btnEpisodeList = null;

		[SkinControlAttribute(802)] protected GUICheckButton togWatched = null;
        [SkinControlAttribute(803)] protected GUICheckButton togUnwatched = null;
        [SkinControlAttribute(804)] protected GUICheckButton togPartiallyWatched = null;
        [SkinControlAttribute(805)] protected GUICheckButton togCompleteOnly = null;

        [SkinControlAttribute(821)] protected GUICheckButton togEpisodeWatched = null;
        [SkinControlAttribute(822)] protected GUICheckButton togEpisodeUnwatched = null;
		[SkinControlAttribute(823)] protected GUIButtonControl btnEpisodeAddCategory = null;
		[SkinControlAttribute(824)] protected GUIButtonControl btnEpisodeClearcategories = null;
		[SkinControlAttribute(825)] protected GUIButtonControl btnPlayEpisode = null;
		[SkinControlAttribute(826)] protected GUIButtonControl btnEpisodeAllAnycategories = null;

		[SkinControlAttribute(901)] protected GUIButtonControl btnSwitchSeries = null;
		[SkinControlAttribute(902)] protected GUIButtonControl btnSwitchEpisode = null;

		[SkinControlAttribute(920)] protected GUIButtonControl btnWindowContinueWatching = null;
		[SkinControlAttribute(921)] protected GUIButtonControl btnWindowUtilities = null;
		[SkinControlAttribute(922)] protected GUIButtonControl btnWindowCalendar = null;
		[SkinControlAttribute(923)] protected GUIButtonControl btnWindowDownloads = null;
		[SkinControlAttribute(925)] protected GUIButtonControl btnWindowRecommendations = null;
		[SkinControlAttribute(927)] protected GUIButtonControl btnWindowPlaylists = null;

		[SkinControlAttribute(1551)] protected GUILabelControl dummyRandomSeries = null;
		[SkinControlAttribute(1552)] protected GUILabelControl dummyRandomEpisode = null;
		[SkinControlAttribute(1553)] protected GUILabelControl dummyNoData = null;


		private List<string> AllCategories = new List<string>();

		private BackgroundWorker getDataWorker = new BackgroundWorker();

		public RandomWindow()
		{
			// get ID of windowplugin belonging to this setup
			// enter your own unique code
			GetID = Constants.WindowIDs.RANDOM;

			setGUIProperty("Status", "-");
			setGUIProperty("LevelType", "-");
			setGUIProperty("LevelName", "-");

			getDataWorker.DoWork += new DoWorkEventHandler(getDataWorker_DoWork);
			getDataWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(getDataWorker_RunWorkerCompleted);
		}

		public void SetDetails()
		{

			if (MainWindow.RandomWindow_RandomType == RandomObjectType.Series)
			{
				dummyRandomSeries.Visible = true;
				dummyRandomEpisode.Visible = false;

				if (btnRandom != null) btnRandom.NavigateDown = 802;
				if (btnRandom != null) btnRandom.NavigateUp = 810;
				if (btnRandom != null) btnRandom.NavigateRight = 805;
			}
			else
			{
				dummyRandomSeries.Visible = false;
				dummyRandomEpisode.Visible = true;

				if (btnRandom != null) btnRandom.NavigateDown = 821;
				if (btnRandom != null) btnRandom.NavigateUp = 825;
				if (btnRandom != null) btnRandom.NavigateRight = 823;
			}

			SetDisplayDetails();
		}

		private void SetDisplayDetails()
		{

			string combinedDetails = "";

			setGUIProperty("LevelType", "-");
			setGUIProperty("LevelName", "-");
			setGUIProperty("NumberOfMatches", "-");
			setGUIProperty("CombinedFilterDetails", "-");
			setGUIProperty("Series.CategoryType", "-");
			setGUIProperty("Episode.CategoryType", "-");
			setGUIProperty("Series.Categories", "-");
			setGUIProperty("Episode.Categories", "-");

			if (MainWindow.RandomWindow_RandomLevel == RandomSeriesEpisodeLevel.GroupFilter)
			{
				setGUIProperty("LevelType", "Group Filter");
				GroupFilterVM gf = MainWindow.RandomWindow_LevelObject as GroupFilterVM;
				setGUIProperty("LevelName", gf.GroupFilterName);

				combinedDetails += "Group Filter: " + gf.GroupFilterName;
			}
			if (MainWindow.RandomWindow_RandomLevel == RandomSeriesEpisodeLevel.Group)
			{
				setGUIProperty("LevelType", "Group");
				AnimeGroupVM grp = MainWindow.RandomWindow_LevelObject as AnimeGroupVM;
				setGUIProperty("LevelName", grp.GroupName);

				combinedDetails += "Group: " + grp.GroupName;
			}
			if (MainWindow.RandomWindow_RandomLevel == RandomSeriesEpisodeLevel.Series)
			{
				setGUIProperty("LevelType", "Series");
				AnimeSeriesVM ser = MainWindow.RandomWindow_LevelObject as AnimeSeriesVM;
				setGUIProperty("LevelName", ser.SeriesName);

				combinedDetails += "Series: " + ser.SeriesName;
			}

			if (MainWindow.RandomWindow_RandomType == RandomObjectType.Series)
				combinedDetails += string.Format(" ({0} Matches)", MainWindow.RandomWindow_MatchesFound);

			setGUIProperty("CombinedFilterDetails", combinedDetails);

			setGUIProperty("NumberOfMatches", MainWindow.RandomWindow_MatchesFound.ToString());

			setGUIProperty("Series.Categories", MainWindow.RandomWindow_SeriesCategories);
			setGUIProperty("Episode.Categories", MainWindow.RandomWindow_EpisodeCategories);

			if (MainWindow.RandomWindow_SeriesAllCategories)
				setGUIProperty("Series.CategoryType", "All");
			else
				setGUIProperty("Series.CategoryType", "Any");

			if (MainWindow.RandomWindow_EpisodeAllCategories)
				setGUIProperty("Episode.CategoryType", "All");
			else
				setGUIProperty("Episode.CategoryType", "Any");

		}

		void getDataWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (btnRandom != null) btnRandom.IsFocused = true;
		}

		void getDataWorker_DoWork(object sender, DoWorkEventArgs e)
		{

			if (MainWindow.RandomWindow_RandomType == RandomObjectType.Series) SetRandomSeries();
			if (MainWindow.RandomWindow_RandomType == RandomObjectType.Episode) SetRandomEpisode();
		}

		public override bool Init()
		{
			return Load(GUIGraphicsContext.Skin + @"\Anime3_Random.xml");
		}

		public override int GetID
		{
			get { return Constants.WindowIDs.RANDOM; }
			set { base.GetID = value; }
		}

		protected override void OnPageLoad()
		{
			base.OnPageLoad();
			BaseConfig.MyAnimeLog.Write("OnPageLoad: RandomWindow");


			if (togWatched != null) togWatched.Selected = MainWindow.RandomWindow_SeriesWatched;
			if (togUnwatched != null) togUnwatched.Selected = MainWindow.RandomWindow_SeriesUnwatched;
			if (togPartiallyWatched != null) togPartiallyWatched.Selected = MainWindow.RandomWindow_SeriesPartiallyWatched;
			if (togCompleteOnly != null) togCompleteOnly.Selected = MainWindow.RandomWindow_SeriesOnlyComplete;

			if (togEpisodeUnwatched != null) togEpisodeUnwatched.Selected = MainWindow.RandomWindow_EpisodeUnwatched;
			if (togEpisodeWatched != null) togEpisodeWatched.Selected = MainWindow.RandomWindow_EpisodeWatched;

			LoadData();

			//m_Facade.Focus = true;
		}

		public static void setGUIProperty(string which, string value)
		{
			MediaPortal.GUI.Library.GUIPropertyManager.SetProperty("#Anime3.Random." + which, value);
		}

		public static void clearGUIProperty(string which)
		{
			setGUIProperty(which, "-"); // String.Empty doesn't work on non-initialized fields, as a result they would display as ugly #TVSeries.bla.bla
		}

		private void LoadData()
		{
			if (getDataWorker.IsBusy) return;

			setGUIProperty("Status", "Loading Data...");
			dummyNoData.Visible = true;

			if (MainWindow.RandomWindow_RandomLevel == RandomSeriesEpisodeLevel.All)
			{
				MainWindow.RandomWindow_LevelObject = GroupFilterHelper.AllGroupsFilter;
				MainWindow.RandomWindow_RandomLevel = RandomSeriesEpisodeLevel.GroupFilter;
			}

			SetDetails();

			if (MainWindow.RandomWindow_RandomType == RandomObjectType.Series && MainWindow.RandomWindow_CurrentSeries != null)
				DisplaySeries();
			else if (MainWindow.RandomWindow_RandomType == RandomObjectType.Episode && MainWindow.RandomWindow_CurrentEpisode != null)
				DisplayEpisode();
			else
				getDataWorker.RunWorkerAsync();
		}

		private List<AnimeSeriesVM> GetSeriesForGroupFilter()
		{
			List<AnimeSeriesVM> serList = new List<AnimeSeriesVM>();
			try
			{
				BaseConfig.MyAnimeLog.Write("Getting list of candidate random series");

				if (MainWindow.RandomWindow_RandomLevel != RandomSeriesEpisodeLevel.GroupFilter) return serList;
				GroupFilterVM gf = MainWindow.RandomWindow_LevelObject as GroupFilterVM;
				if (gf == null) return serList;

				BaseConfig.MyAnimeLog.Write("Getting list of candidate random series for: " + gf.GroupFilterName);

				bool allCats = MainWindow.RandomWindow_SeriesAllCategories;
				if (MainWindow.RandomWindow_RandomType == RandomObjectType.Episode)
					allCats = MainWindow.RandomWindow_EpisodeAllCategories;

				bool completeSeries = true;
				bool allWatched = true;
				bool unwatched = true;
				bool partiallyWatched = true;

				if (togWatched != null) allWatched = togWatched.Selected;
				if (togUnwatched != null) unwatched = togUnwatched.Selected;
				if (togPartiallyWatched != null) partiallyWatched = togPartiallyWatched.Selected;
				if (togCompleteOnly != null) completeSeries = togCompleteOnly.Selected;

				List<JMMServerBinary.Contract_AnimeGroup> contracts = JMMServerVM.Instance.clientBinaryHTTP.GetAnimeGroupsForFilter(
					gf.GroupFilterID.Value, JMMServerVM.Instance.CurrentUser.JMMUserID, BaseConfig.Settings.SingleSeriesGroups);

				BaseConfig.MyAnimeLog.Write("Total groups for filter = " + contracts.Count.ToString());

				string selectedCategories = MainWindow.RandomWindow_SeriesCategories;
				if (MainWindow.RandomWindow_RandomType == RandomObjectType.Episode)
					selectedCategories = MainWindow.RandomWindow_EpisodeCategories;

				foreach (JMMServerBinary.Contract_AnimeGroup grpContract in contracts)
				{
					AnimeGroupVM grp = new AnimeGroupVM(grpContract);
					// ignore sub groups
					if (grp.AnimeGroupParentID.HasValue) continue;

					foreach (AnimeSeriesVM ser in grp.AllSeries)
					{
						// categories
						if (!string.IsNullOrEmpty(selectedCategories))
						{
							string filterParm = selectedCategories.Trim();

							string[] cats = filterParm.Split(',');

							bool foundCat = false;
							if (allCats) foundCat = true; // all

							int index = 0;
							foreach (string cat in cats)
							{
								string thiscat = cat.Trim();
								if (thiscat.Trim().Length == 0) continue;
								if (thiscat.Trim() == ",") continue;

								index = ser.CategoriesString.IndexOf(thiscat, 0, StringComparison.InvariantCultureIgnoreCase);

								if (!allCats) // any
								{
									if (index > -1)
									{
										foundCat = true;
										break;
									}
								}
								else //all
								{
									if (index < 0)
									{
										foundCat = false;
										break;
									}
								}
							}
							if (!foundCat) continue;

						}

						if (!ser.IsComplete && completeSeries) continue;

						if (allWatched && ser.AllFilesWatched)
						{
							serList.Add(ser);
							continue;
						}

						if (unwatched && !ser.AnyFilesWatched)
						{
							serList.Add(ser);
							continue;
						}


						if (partiallyWatched && ser.AnyFilesWatched && !ser.AllFilesWatched)
						{
							serList.Add(ser);
							continue;
						}
					}
					
				}

			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write(ex.ToString());
			}

			return serList;
		}

		private List<AnimeSeriesVM> GetSeriesForGroup()
		{
			List<AnimeSeriesVM> serList = new List<AnimeSeriesVM>();
			try
			{
				BaseConfig.MyAnimeLog.Write("Getting list of candidate random series for group");

				if (MainWindow.RandomWindow_RandomLevel != RandomSeriesEpisodeLevel.Group) return serList;
				AnimeGroupVM grp = MainWindow.RandomWindow_LevelObject as AnimeGroupVM;
				if (grp == null) return serList;

				bool allCats = MainWindow.RandomWindow_SeriesAllCategories;
				if (MainWindow.RandomWindow_RandomType == RandomObjectType.Episode)
					allCats = MainWindow.RandomWindow_EpisodeAllCategories;

				BaseConfig.MyAnimeLog.Write("Getting list of candidate random series for: " + grp.GroupName);

				string selectedCategories = MainWindow.RandomWindow_SeriesCategories;
				if (MainWindow.RandomWindow_RandomType == RandomObjectType.Episode)
					selectedCategories = MainWindow.RandomWindow_EpisodeCategories;

				foreach (AnimeSeriesVM ser in grp.AllSeries)
				{
					// categories
					if (!string.IsNullOrEmpty(selectedCategories))
					{
						string filterParm = selectedCategories.Trim();

						string[] cats = filterParm.Split(',');

						bool foundCat = false;
						if (allCats) foundCat = true; // all

						int index = 0;
						foreach (string cat in cats)
						{
							string thiscat = cat.Trim();
							if (thiscat.Trim().Length == 0) continue;
							if (thiscat.Trim() == ",") continue;

							index = ser.CategoriesString.IndexOf(thiscat, 0, StringComparison.InvariantCultureIgnoreCase);

							if (!allCats) // any
							{
								if (index > -1)
								{
									foundCat = true;
									break;
								}
							}
							else //all
							{
								if (index < 0)
								{
									foundCat = false;
									break;
								}
							}
						}
						if (!foundCat) continue;

					}
					serList.Add(ser);
				}

			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write(ex.ToString());
			}

			return serList;
		}

		private void SetRandomSeries()
		{
			try
			{
				List<AnimeSeriesVM> serList = new List<AnimeSeriesVM>();
				if (MainWindow.RandomWindow_RandomLevel == RandomSeriesEpisodeLevel.GroupFilter)
					serList = GetSeriesForGroupFilter();

				if (MainWindow.RandomWindow_RandomLevel == RandomSeriesEpisodeLevel.Group)
					serList = GetSeriesForGroup();

				if (MainWindow.RandomWindow_RandomLevel == RandomSeriesEpisodeLevel.Series)
				{
					AnimeSeriesVM ser = MainWindow.RandomWindow_LevelObject as AnimeSeriesVM;
					if (ser != null) serList.Add(ser);
				}

				BaseConfig.MyAnimeLog.Write("Found " + serList.Count.ToString() + " series");

				if (serList != null && serList.Count > 0)
				{
					MainWindow.RandomWindow_MatchesFound = serList.Count;

					AnimeSeriesVM ser = serList[rndm.Next(0, serList.Count)];
					MainWindow.RandomWindow_CurrentSeries = ser;
					DisplaySeries();
				}
				else
					DisplaySeries();
				

				SetDisplayDetails();
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write(ex.ToString());
			}
		}

		private void SetRandomEpisode()
		{
			try
			{
				List<AnimeSeriesVM> serList = new List<AnimeSeriesVM>();
				if (MainWindow.RandomWindow_RandomLevel == RandomSeriesEpisodeLevel.GroupFilter)
					serList = GetSeriesForGroupFilter();

				if (MainWindow.RandomWindow_RandomLevel == RandomSeriesEpisodeLevel.Group)
					serList = GetSeriesForGroup();

				if (MainWindow.RandomWindow_RandomLevel == RandomSeriesEpisodeLevel.Series)
				{
					AnimeSeriesVM ser = MainWindow.RandomWindow_LevelObject as AnimeSeriesVM;
					if (ser != null) serList.Add(ser);
				}

				BaseConfig.MyAnimeLog.Write("Found " + serList.Count.ToString() + " series for episodes");

				bool watched = true;
				bool unwatched = true;

				if (togEpisodeWatched != null) watched = togEpisodeWatched.Selected;
				if (togEpisodeUnwatched != null) unwatched = togEpisodeUnwatched.Selected;

				MainWindow.RandomWindow_CurrentEpisode = null;

				if (serList != null && serList.Count > 0)
				{
					Dictionary<int, AnimeSeriesVM> dictSeries = new Dictionary<int, AnimeSeriesVM>();
					foreach (AnimeSeriesVM ser in serList)
						dictSeries[ser.AnimeSeriesID.Value] = ser;
					
					bool needEpisode = true;
					while (needEpisode)
					{
						if (dictSeries.Values.Count == 0) 
							break;

						List<AnimeSeriesVM> tempSerList = new List<AnimeSeriesVM>(dictSeries.Values);
						MainWindow.RandomWindow_CurrentSeries = tempSerList[rndm.Next(0, tempSerList.Count)];

						// get all the episodes
						List<AnimeEpisodeVM> epList = new List<AnimeEpisodeVM>();
						foreach (AnimeEpisodeVM ep in MainWindow.RandomWindow_CurrentSeries.AllEpisodes)
						{
							bool useEp = false;
							if (watched && ep.Watched)
								useEp = true;

							if (unwatched && !ep.Watched)
								useEp = true;

							if (ep.LocalFileCount == 0)
								useEp = false;

							if (ep.EpisodeTypeEnum != enEpisodeType.Episode && ep.EpisodeTypeEnum != enEpisodeType.Special)
								useEp = false;

							if (useEp) epList.Add(ep);
						}

						if (epList.Count > 0)
						{
							MainWindow.RandomWindow_CurrentEpisode = epList[rndm.Next(0, epList.Count)];
							needEpisode = false;
						}
						else
							dictSeries.Remove(MainWindow.RandomWindow_CurrentSeries.AnimeSeriesID.Value);
					}
				}

				DisplayEpisode();

				SetDisplayDetails();
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write(ex.ToString());
			}
		}

		private void DisplayEpisode()
		{
			clearGUIProperty("Series.Title");
			clearGUIProperty("Series.Description");
			clearGUIProperty("Series.LastWatched");
			clearGUIProperty("Series.EpisodesWatched");
			clearGUIProperty("Series.EpisodesUnwatched");
			clearGUIProperty("Series.Poster");

			clearGUIProperty("NumberOfMatches");

			clearGUIProperty("Episode.Title");
			clearGUIProperty("Episode.AirDate");
			clearGUIProperty("Episode.RunTime");
			clearGUIProperty("Episode.FileInfo");
			clearGUIProperty("Episode.Overview");
			clearGUIProperty("Episode.Image");
			clearGUIProperty("Episode.Logos");

			if (MainWindow.RandomWindow_CurrentEpisode == null)
			{
				setGUIProperty("Status", "No Matches Found");
				MainWindow.RandomWindow_MatchesFound = 0;
				MainWindow.RandomWindow_CurrentSeries = null;
				MainWindow.RandomWindow_CurrentEpisode = null;
			}
			else
			{
				setGUIProperty("Series.Title", MainWindow.RandomWindow_CurrentSeries.SeriesName);
				setGUIProperty("Series.Description", MainWindow.RandomWindow_CurrentSeries.Description);
				setGUIProperty("Series.LastWatched", MainWindow.RandomWindow_CurrentSeries.WatchedDate.HasValue ? MainWindow.RandomWindow_CurrentSeries.WatchedDate.Value.ToString("dd MMM yy", Globals.Culture) : "-");
				setGUIProperty("Series.EpisodesWatched", MainWindow.RandomWindow_CurrentSeries.WatchedEpisodeCount.ToString());
				setGUIProperty("Series.EpisodesUnwatched", MainWindow.RandomWindow_CurrentSeries.UnwatchedEpisodeCount.ToString());
				setGUIProperty("Series.Poster", ImageAllocator.GetSeriesImageAsFileName(MainWindow.RandomWindow_CurrentSeries, GUIFacadeControl.Layout.List));

				setGUIProperty("Episode.Title", MainWindow.RandomWindow_CurrentEpisode.EpisodeNumberAndNameWithType);
				setGUIProperty("Episode.AirDate", MainWindow.RandomWindow_CurrentEpisode.AirDateAsString);
				setGUIProperty("Episode.RunTime", Utils.FormatSecondsToDisplayTime(MainWindow.RandomWindow_CurrentEpisode.AniDB_LengthSeconds));


				if (MainWindow.RandomWindow_CurrentEpisode.EpisodeImageLocation.Length > 0)
					setGUIProperty("Episode.Image", MainWindow.RandomWindow_CurrentEpisode.EpisodeImageLocation);

				// Overview
				string overview = MainWindow.RandomWindow_CurrentEpisode.EpisodeOverview;
				if (BaseConfig.Settings.HidePlot)
				{
					if (MainWindow.RandomWindow_CurrentEpisode.EpisodeOverview.Trim().Length > 0 && MainWindow.RandomWindow_CurrentEpisode.IsWatched == 0)
						overview = "*** Hidden to prevent spoilers ***";
				}
				setGUIProperty("Episode.Overview", overview);

				// File Info
				List<VideoDetailedVM> filesForEpisode = new List<VideoDetailedVM>();
				List<JMMServerBinary.Contract_VideoDetailed> contracts = JMMServerVM.Instance.clientBinaryHTTP.GetFilesForEpisode(MainWindow.RandomWindow_CurrentEpisode.AnimeEpisodeID,
					JMMServerVM.Instance.CurrentUser.JMMUserID);

				foreach (JMMServerBinary.Contract_VideoDetailed fi in contracts)
					filesForEpisode.Add(new VideoDetailedVM(fi));

				string finfo = "";
				foreach (VideoDetailedVM vid in filesForEpisode)
					finfo = vid.FileSelectionDisplay;

				if (filesForEpisode.Count > 1)
					finfo = filesForEpisode.Count.ToString() + " Files Available";

				setGUIProperty("Episode.FileInfo", finfo);

				// Logos
				string logos = Logos.buildLogoImage(MainWindow.RandomWindow_CurrentEpisode);

				BaseConfig.MyAnimeLog.Write(logos);
				setGUIProperty("Episode.Logos", logos);

				dummyNoData.Visible = false;
			}
		}

		private void DisplaySeries()
		{
			clearGUIProperty("Series.Title");
			clearGUIProperty("Series.Description");
			clearGUIProperty("Series.LastWatched");
			clearGUIProperty("Series.EpisodesWatched");
			clearGUIProperty("Series.EpisodesUnwatched");
			clearGUIProperty("Series.Poster");

			clearGUIProperty("NumberOfMatches");

			if (MainWindow.RandomWindow_CurrentSeries == null)
			{
				setGUIProperty("Status", "No Matches Found");
				MainWindow.RandomWindow_MatchesFound = 0;
				MainWindow.RandomWindow_CurrentSeries = null;
			}
			else
			{
				setGUIProperty("Series.Title", MainWindow.RandomWindow_CurrentSeries.SeriesName);
				setGUIProperty("Series.Description", MainWindow.RandomWindow_CurrentSeries.Description);
				setGUIProperty("Series.LastWatched", MainWindow.RandomWindow_CurrentSeries.WatchedDate.HasValue ? MainWindow.RandomWindow_CurrentSeries.WatchedDate.Value.ToString("dd MMM yy", Globals.Culture) : "-");
				setGUIProperty("Series.EpisodesWatched", MainWindow.RandomWindow_CurrentSeries.WatchedEpisodeCount.ToString());
				setGUIProperty("Series.EpisodesUnwatched", MainWindow.RandomWindow_CurrentSeries.UnwatchedEpisodeCount.ToString());
				setGUIProperty("Series.Poster", ImageAllocator.GetSeriesImageAsFileName(MainWindow.RandomWindow_CurrentSeries, GUIFacadeControl.Layout.List));

				dummyNoData.Visible = false;
			}
		}

		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			if (MA3WindowManager.HandleWindowChangeButton(control))
				return;

			if (btnAddCategory != null && control == btnAddCategory)
			{
				string cat = Utils.PromptSelectCategory("");
				if (!string.IsNullOrEmpty(cat))
				{
					if (!string.IsNullOrEmpty(MainWindow.RandomWindow_SeriesCategories))
						MainWindow.RandomWindow_SeriesCategories += ", ";

					MainWindow.RandomWindow_SeriesCategories += cat;

					SetDisplayDetails();
				}
			}

			if (btnEpisodeAddCategory != null && control == btnEpisodeAddCategory)
			{
				string cat = Utils.PromptSelectCategory("");
				if (!string.IsNullOrEmpty(cat))
				{
					if (!string.IsNullOrEmpty(MainWindow.RandomWindow_EpisodeCategories))
						MainWindow.RandomWindow_EpisodeCategories += ", ";

					MainWindow.RandomWindow_EpisodeCategories += cat;

					SetDisplayDetails();
				}
			}

			if (btnClearcategories != null && control == btnClearcategories)
			{
				MainWindow.RandomWindow_SeriesCategories = "";
				SetDisplayDetails();
			}

			if (btnEpisodeClearcategories != null && control == btnEpisodeClearcategories)
			{
				MainWindow.RandomWindow_EpisodeCategories = "";
				SetDisplayDetails();
			}

			if (btnAllAnycategories != null && control == btnAllAnycategories)
			{
				MainWindow.RandomWindow_SeriesAllCategories = !MainWindow.RandomWindow_SeriesAllCategories;
				SetDisplayDetails();
			}

			if (btnEpisodeAllAnycategories != null && control == btnEpisodeAllAnycategories)
			{
				MainWindow.RandomWindow_EpisodeAllCategories = !MainWindow.RandomWindow_EpisodeAllCategories;
				SetDisplayDetails();
			}

			if (btnRandom != null && control == btnRandom)
			{
				if (togWatched != null) MainWindow.RandomWindow_SeriesWatched = togWatched.Selected;
				if (togUnwatched != null) MainWindow.RandomWindow_SeriesUnwatched = togUnwatched.Selected;
				if (togPartiallyWatched != null) MainWindow.RandomWindow_SeriesPartiallyWatched = togPartiallyWatched.Selected;
				if (togCompleteOnly != null) MainWindow.RandomWindow_SeriesOnlyComplete = togCompleteOnly.Selected;

				if (togEpisodeUnwatched != null) MainWindow.RandomWindow_EpisodeUnwatched = togEpisodeUnwatched.Selected;
				if (togEpisodeWatched != null) MainWindow.RandomWindow_EpisodeWatched = togEpisodeWatched.Selected;

				MainWindow.RandomWindow_CurrentEpisode = null;
				MainWindow.RandomWindow_CurrentSeries = null;
				this.btnRandom.IsFocused = true;
				LoadData();
			}

			if (btnSwitchSeries != null && control == btnSwitchSeries)
			{
				this.btnSwitchSeries.IsFocused = false;
				this.btnRandom.IsFocused = true;
				MainWindow.RandomWindow_RandomType = RandomObjectType.Series;
				LoadData();
			}

			if (btnSwitchEpisode != null && control == btnSwitchEpisode)
			{
				this.btnSwitchEpisode.IsFocused = false;
				this.btnRandom.IsFocused = true;
				MainWindow.RandomWindow_RandomType = RandomObjectType.Episode;
				LoadData();
			}

			if (btnPlayEpisode != null && control == btnPlayEpisode)
			{
				if (MainWindow.RandomWindow_CurrentEpisode == null) return;
				MainWindow.vidHandler.ResumeOrPlay(MainWindow.RandomWindow_CurrentEpisode);
			}


			if (btnEpisodeList != null && control == btnEpisodeList)
			{
				if (MainWindow.RandomWindow_CurrentSeries == null) return;
				MainWindow.curGroupFilter = GroupFilterHelper.AllGroupsFilter;

				// find the group for this series
				AnimeGroupVM grp = JMMServerHelper.GetGroup(MainWindow.RandomWindow_CurrentSeries.AnimeGroupID);
				if (grp == null)
				{
					BaseConfig.MyAnimeLog.Write("Group not found");
					return;
				}
				MainWindow.curAnimeGroup = grp;
				MainWindow.curAnimeGroupViewed = grp;
				MainWindow.curAnimeSeries = MainWindow.RandomWindow_CurrentSeries;

				bool foundEpType = false;
				foreach (AnimeEpisodeTypeVM anEpType in MainWindow.RandomWindow_CurrentSeries.EpisodeTypesToDisplay)
				{
					if (anEpType.EpisodeType == enEpisodeType.Episode)
					{
						MainWindow.curAnimeEpisodeType = anEpType;
						foundEpType = true;
						break;
					}
				}

				if (!foundEpType) return;


				MainWindow.listLevel = Listlevel.Episode;

				GUIWindowManager.CloseCurrentWindow();
				GUIWindowManager.ActivateWindow(Constants.WindowIDs.MAIN, false);
				return;
			}

			base.OnClicked(controlId, control, actionType);
		}
	}
}
