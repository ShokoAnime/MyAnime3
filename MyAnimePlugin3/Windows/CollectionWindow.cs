using System;
using System.Collections.Generic;
using System.ComponentModel;

using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;

using MyAnimePlugin3.Downloads;

namespace MyAnimePlugin3.Windows
{
	public class CollectionWindow : GUIWindow
	{
		//TODO
		/*
		[SkinControlAttribute(901)]        protected GUIButtonControl btnNavLeft = null;
		[SkinControlAttribute(902)]        protected GUIButtonControl btnNavRight = null;

		[SkinControlAttribute(801)]        protected GUIButtonControl btnInfoPage = null;
		[SkinControlAttribute(802)]        protected GUIButtonControl btnAnimeTypePage = null;
		[SkinControlAttribute(803)]        protected GUIButtonControl btnMissingPage = null;

		[SkinControlAttribute(1500)]        protected GUILabelControl dummyPageInfo = null;
		[SkinControlAttribute(1501)]        protected GUILabelControl dummyPageAnimeType = null;
		[SkinControlAttribute(1503)]        protected GUILabelControl dummyPageMissing = null;
		[SkinControlAttribute(1504)]        protected GUILabelControl dummyCreatingList = null;

		[SkinControlAttribute(810)]        protected GUIListControl lstMissing = null;
		[SkinControlAttribute(811)]        protected GUIButtonControl btnEpisodes = null;
		[SkinControlAttribute(812)]        protected GUIButtonControl btnRelations = null;

		struct CollectionInfo
		{
			public string TotalFileSize;
			public int Episodes;
			public int Files;
			public int AnimeWatched;
			public int PartiallyWatched;
			public int EpisodesWatched;
			public decimal PercentageEpisodesWatched;
			public string TotalRuntime;
			public string TotalWatchTime;
			public int TotalTimesWatched;
			public int TotalTimesPlayed;
			public decimal PercentageAniDB;
			public decimal PercentageAniDBWatched;
			public int RatingsGiven;
			public int Genres;
			public int AwardsCount;
			public int FanSubGroups;
			public int RRated;
			public int Characters;
			public int VoiceActers;
			public int AnimeMissingEpisodes;
			public int MissingEpisodes;
			public int MissingRelations;
			public List<AnimeTypeStats> AnimeTypeList;
			public List<RelationsItem> MissingRelationsList;
			public List<ItemDetails> MissingEpisodesList;
		}


		
		MissingTypes MissingType = MissingTypes.Episodes;
		bool MissingPageOpen = false;
		int ListSelectedItem;
		static public BackgroundWorker GetDataWorker;
		CollectionInfo GlobalCI;
		bool IsCalculated = false;
		
		enum MissingTypes
		{
			Episodes = 1,
			Relations = 2
		}

		struct RelationsItem
		{
			public AnimeSeries AnimeWithRelation;
			public AniDB_Anime Relation;
			public int ID;
		}

		struct ItemDetails
		{
			public AnimeEpisode Episode;
			public int ID;
		}

		public CollectionWindow()
		{
			GetID = Constants.WindowIDs.COLLECTION;

			MainWindow.anidbProcessor.GotAnimeInfoEvent += new AnimePlugin.AniDBLib.GotAnimeInfoEventHandler(anidbProcessor_GotAnimeInfoEvent);

			MainWindow.anidbProcessor.AniDBStatusEvent += new AniDBLib.AniDBStatusEventHandler(anidbProcessor_AniDBStatusEvent);

			// This background worker is started by the wain window when it is shown. This means the data is gatherd when someone enters the plugin, not before, but also means they dont have to wait on the collection window ether.
			BaseConfig.MyAnimeLog.Write("COLLECTION WINDOW: setting up background worker");
			GetDataWorker = new BackgroundWorker();
			GetDataWorker.DoWork += new DoWorkEventHandler(GetDataWorker_DoWork);
			GetDataWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(GetDataWorker_RunWorkerCompleted);

			
		}

		void GetDataWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			GlobalCI = (CollectionInfo)e.Result;
			IsCalculated = true;

			ShowCollectionInfo();
		}

		void ShowCollectionInfo()
		{
			if (GUIWindowManager.ActiveWindow == Constants.WindowIDs.COLLECTION)
			{
				MainPage();
				StatsPage();
			}
		}

		void GetDataWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			CollectionInfo ci = FormatTextAnime();

			e.Result = ci;
		}

		public override int GetID
		{
			get { return Constants.WindowIDs.COLLECTION; }
			set { base.GetID = value; }
		}

		void anidbProcessor_GotAnimeInfoEvent(GotAnimeInfoEventArgs ev)
		{
			if (GUIWindowManager.ActiveWindow != Constants.WindowIDs.COLLECTION) return;

			int aid = ev.animeID;

			BaseConfig.MyAnimeLog.Write("anidbProcessor_GotAnimeInfoEvent: {0}", aid.ToString());

			// check if we are waiting on this anime
			//bool isRelated = false;
			foreach (RelationsItem relation in GlobalCI.MissingRelationsList)
			{
				if (MissingType != MissingTypes.Relations) return;
				if (relation.Relation.AnimeID == aid)
				{
					ListSelectedItem = lstMissing.SelectedListItemIndex;
					MissingPage(MissingTypes.Relations);
					lstMissing.SelectedListItemIndex = ListSelectedItem;
				}
			}
		}

		void anidbProcessor_AniDBStatusEvent(AniDBStatusEventArgs ev)
		{
			if (GUIWindowManager.ActiveWindow != Constants.WindowIDs.COLLECTION) return;

			try
			{
				string cmdDesc = "";
				switch (ev.evType)
				{
					case enHelperActivityType.GettingAnimeInfo:
						cmdDesc = "Getting anime info: " + ev.Status; break;
					case enHelperActivityType.GettingAnimeHTTP:
						cmdDesc = "Getting http anime info: " + ev.Status; break;
					case enHelperActivityType.LoggingIn:
						cmdDesc = "Logging in..."; break;
					case enHelperActivityType.LoggingOut:
						cmdDesc = "Logging out..."; break;
					default:
						cmdDesc = ""; break;
				}

				setGUIProperty("Collection.Status", cmdDesc);
			}
			catch { }
		}

		protected override void OnPageLoad()
		{
			base.OnPageLoad();

			BaseConfig.MyAnimeLog.Write("Collection Window loaded");
			MainWindow.NotificationsManager.WindowChange(true);

			HideControls();
			Setup();
			LoadFanart();

			InitGUIProperties();

			if (MissingPageOpen == true)
			{
				HideControls();
				dummyPageMissing.Visible = true;

				BaseConfig.MyAnimeLog.Write("Selected item index is: " + ListSelectedItem.ToString());

				GUIWindow window = GUIWindowManager.GetWindow(GUIWindowManager.ActiveWindow);
				GUIControl control = window.GetControl(window.GetFocusControlId());
				control.Focus = false;
				lstMissing.Focus = true;

				if (MissingType == MissingTypes.Relations)
				{
					MissingPage(MissingTypes.Relations);
					lstMissing.SelectedListItemIndex = ListSelectedItem;
				}
				else
				{
					MissingPage(MissingTypes.Episodes);
					lstMissing.SelectedListItemIndex = ListSelectedItem;
				}
			}
			else
			{
				//MissingPage(MissingTypes.Episodes);
				dummyPageInfo.Visible = true;
			}

			MissingPageOpen = false;
			setGUIProperty("Collection.Status", "-");


			
			if (!IsCalculated)
				GetDataWorker.RunWorkerAsync();
			else
				ShowCollectionInfo();
		}

		public override bool OnMessage(GUIMessage message)
		{
			return base.OnMessage(message);
		}

		protected override void OnShowContextMenu()
		{
			GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg == null)
				return;

			dlg.Reset();
			dlg.SetHeading("Collection window actions");

			dlg.Add("ReCalculate");

			dlg.DoModal(GUIWindowManager.ActiveWindow);

			int selectedLabel = 0;
			selectedLabel = dlg.SelectedLabel;

			switch (selectedLabel)
			{
				case 0:
					InitGUIProperties();
					GetDataWorker.RunWorkerAsync();
					break;

				default:
					return;
			}
		}


		protected override void OnPageDestroy(int new_windowId)
		{

			MainWindow.NotificationsManager.WindowChange(false);

			base.OnPageDestroy(new_windowId);
		}

		public override bool Init()
		{
			BaseConfig.MyAnimeLog.Write("Init Collection window");
			return Load(GUIGraphicsContext.Skin + @"\Anime3_Collection.xml");
		}

		public static void setGUIProperty(string which, string value)
		{
			MediaPortal.GUI.Library.GUIPropertyManager.SetProperty("#Anime3." + which, value);
		}

		public static void clearGUIProperty(string which)
		{
			setGUIProperty(which, "-"); // String.Empty doesn't work on non-initialized fields, as a result they would display as ugly #TVSeries.bla.bla
		}

		public override void OnAction(MediaPortal.GUI.Library.Action action)
		{
			switch (action.wID)
			{
				case MediaPortal.GUI.Library.Action.ActionType.ACTION_MOVE_DOWN:
				case MediaPortal.GUI.Library.Action.ActionType.ACTION_MOVE_LEFT:
				case MediaPortal.GUI.Library.Action.ActionType.ACTION_MOVE_RIGHT:
				case MediaPortal.GUI.Library.Action.ActionType.ACTION_MOVE_UP:
					// Reset timer on Notification window.
					MainWindow.NotificationsManager.KeyPress();

					base.OnAction(action);
					break;
				default:
					base.OnAction(action);
					break;
			}
		}

		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			if (control == this.btnNavLeft)
			{
				GUIWindowManager.CloseCurrentWindow();
				GUIWindowManager.ActivateWindow(Constants.WindowIDs.DOWNLOADS, false);

				return;
			}

			if (control == this.btnNavRight)
			{
				GUIWindowManager.CloseCurrentWindow();
				GUIWindowManager.ActivateWindow(Constants.WindowIDs.CALENDAR, false);

				return;
			}

			if (control == this.btnInfoPage)
			{
				HideControls();
				dummyPageInfo.Visible = true;
			}

			if (control == this.btnAnimeTypePage)
			{
				HideControls();
				dummyPageAnimeType.Visible = true;
			}

			if (control == this.btnMissingPage)
			{
				HideControls();
				dummyPageMissing.Visible = true;
			}

			if (control == this.btnRelations)
			{
				MissingPage(MissingTypes.Relations); 
			}

			if (control == this.btnEpisodes)
			{
				MissingPage(MissingTypes.Episodes);
			}

			if (control == this.lstMissing)
			{
				if (MissingType == MissingTypes.Episodes)
					DownloadMissing();
				else
				{
					MissingPageOpen = true;
					AniDB_Anime anime = new AniDB_Anime();
					GUIListItem item = this.lstMissing.SelectedListItem;
					RelationsItem RI = GlobalCI.MissingRelationsList[item.ItemId];
					ShowDetails(RI.Relation);
				}
			}

			MainWindow.NotificationsManager.ButtonPress(control);

			base.OnClicked(controlId, control, actionType);
		}

		private void ShowDetails(AniDB_Anime anime)
		{ 
			MainWindow.ImagesAniDBID = anime.AnimeID;
			GUIWindowManager.ActivateWindow(Constants.WindowIDs.ANIMEINFO, false);
		}

		private void LoadFanart()
		{
			clearGUIProperty("Collection.Fanart");
			Fanart fanart = null;
			string title;
			if (MainWindow.ImagesListlevel == Listlevel.Group)
			{
				// get the group
				AnimeGroup grp = new AnimeGroup();
				if (grp.Load(MainWindow.ImagesParentID))
					fanart = new Fanart(grp);
				title = grp.GroupName;
			}
			else
			{
				AnimeSeries ser = new AnimeSeries();
				if (ser.Load(MainWindow.ImagesParentID))
					fanart = new Fanart(ser);
				title = ser.SeriesName;
			}

			if (fanart == null) return;

			if (fanart.FileName.Length > 0)
			{
				setGUIProperty("Collection.Fanart", fanart.FileName);
			}
		}

		private void HideControls()
		{
			dummyPageInfo.Visible = false;
			dummyPageAnimeType.Visible = false;
			dummyPageMissing.Visible = false;
		}

		private void Setup()
		{
			
			
			//AnimeTypeList = new List<AnimeTypeStats>();
		}

#region SkinProperties

        private void InitGUIProperties()
		{
			setGUIProperty("Collection.Missing.Type", "Episodes");

			setGUIProperty("Collection.Stats.TVSeries.Total", "Calculating..");
			setGUIProperty("Collection.Stats.TVSeries.InCollection", "Calculating..");
			setGUIProperty("Collection.Stats.TVSeries.Watched", "Calculating..");
			setGUIProperty("Collection.Stats.TVSeries.Watching", "Calculating..");
			setGUIProperty("Collection.Stats.TVSeries.Size", "Calculating..");
			setGUIProperty("Collection.Stats.TVSeries.Runtime", "Calculating..");

			setGUIProperty("Collection.Stats.Movies.Total", "Calculating..");
			setGUIProperty("Collection.Stats.Movies.InCollection", "Calculating..");
			setGUIProperty("Collection.Stats.Movies.Watched", "Calculating..");
			setGUIProperty("Collection.Stats.Movies.Watching", "Calculating..");
			setGUIProperty("Collection.Stats.Movies.Size", "Calculating..");
			setGUIProperty("Collection.Stats.Movies.Runtime", "Calculating..");

			setGUIProperty("Collection.Stats.OVA.Total", "Calculating..");
			setGUIProperty("Collection.Stats.OVA.InCollection", "Calculating..");
			setGUIProperty("Collection.Stats.OVA.Watched", "Calculating..");
			setGUIProperty("Collection.Stats.OVA.Watching", "Calculating..");
			setGUIProperty("Collection.Stats.OVA.Size", "Calculating..");
			setGUIProperty("Collection.Stats.OVA.Runtime", "Calculating..");

			setGUIProperty("Collection.Stats.Other.Total", "Calculating..");
			setGUIProperty("Collection.Stats.Other.InCollection", "Calculating..");
			setGUIProperty("Collection.Stats.Other.Watched", "Calculating..");
			setGUIProperty("Collection.Stats.Other.Watching", "Calculating..");
			setGUIProperty("Collection.Stats.Other.Size", "Calculating..");
			setGUIProperty("Collection.Stats.Other.Runtime", "Calculating..");

			setGUIProperty("Collection.Stats.Web.Total", "Calculating..");
			setGUIProperty("Collection.Stats.Web.InCollection", "Calculating..");
			setGUIProperty("Collection.Stats.Web.Watched", "Calculating..");
			setGUIProperty("Collection.Stats.Web.Watching", "Calculating..");
			setGUIProperty("Collection.Stats.Web.Size", "Calculating..");
			setGUIProperty("Collection.Stats.Web.Runtime", "Calculating..");

			setGUIProperty("Collection.Stats.TVSpecial.Total", "Calculating..");
			setGUIProperty("Collection.Stats.TVSpecial.InCollection", "Calculating..");
			setGUIProperty("Collection.Stats.TVSpecial.Watched", "Calculating..");
			setGUIProperty("Collection.Stats.TVSpecial.Watching", "Calculating..");
			setGUIProperty("Collection.Stats.TVSpecial.Size", "Calculating..");
			setGUIProperty("Collection.Stats.TVSpecial.Runtime", "Calculating..");

			setGUIProperty("Collection.Main.AnimeCount", "Calculating..");
			setGUIProperty("Collection.Main.AnimeGroupCount", "Calculating..");
			setGUIProperty("Collection.Main.FansubGroupCount", "Calculating..");
			setGUIProperty("Collection.Main.EpisodesCount", "Calculating..");
			setGUIProperty("Collection.Main.TotalFiles", "Calculating..");
			setGUIProperty("Collection.Main.TotalSizeFiles", "Calculating..");
			setGUIProperty("Collection.Main.AnimeWatched", "Calculating..");
			setGUIProperty("Collection.Main.AnimePartiallyWatched", "Calculating..");
			setGUIProperty("Collection.Main.AnimeEpisodesWatched", "Calculating..");
			setGUIProperty("Collection.Main.PercentageEpisodesWatched", "Calculating..");
			setGUIProperty("Collection.Main.TotalRuntime", "Calculating..");
			setGUIProperty("Collection.Main.TotalWatchTime", "Calculating..");
			setGUIProperty("Collection.Main.TotalTimesWatched", "Calculating..");
			setGUIProperty("Collection.Main.TotalTimesPlayed", "Calculating..");
			setGUIProperty("Collection.Main.PercentageAniDB", "Calculating..");
			setGUIProperty("Collection.Main.PercentageWatchedAniDB", "Calculating..");
			setGUIProperty("Collection.Main.RatingsGiven", "Calculating..");
			setGUIProperty("Collection.Main.VoiceActers", "Calculating..");
			setGUIProperty("Collection.Main.Genre", "Calculating..");
			setGUIProperty("Collection.Main.AnimeWithAwards", "Calculating..");
			setGUIProperty("Collection.Main.RRatedAnime", "Calculating..");
			setGUIProperty("Collection.Main.Characters", "Calculating..");
			setGUIProperty("Collection.Main.AnimeMissingEpisodes", "Calculating..");
			setGUIProperty("Collection.Main.MissingEpisodes", "Calculating..");
			setGUIProperty("Collection.Main.MissingRelations", "Calculating..");

			setGUIProperty("Collection.Stats.TVSeries.Total", "UnKnown");
			setGUIProperty("Collection.Stats.Movies.Total", "UnKnown");
			setGUIProperty("Collection.Stats.OVA.Total", "UnKnown");
			setGUIProperty("Collection.Stats.Other.Total", "UnKnown");
			setGUIProperty("Collection.Stats.Web.Total", "UnKnown");
			setGUIProperty("Collection.Stats.TVSpecial.Total", "UnKnown");
		}

		private void StatsPage()
		{


			AnimeTypeStats TVSeries = GlobalCI.AnimeTypeList.Find(delegate(AnimeTypeStats tps) { return tps.Type == AniDB_Anime.AnimeTypes.TV_Series; });
			AnimeTypeStats Movies = GlobalCI.AnimeTypeList.Find(delegate(AnimeTypeStats tps) { return tps.Type == AniDB_Anime.AnimeTypes.Movie; });
			AnimeTypeStats OVA = GlobalCI.AnimeTypeList.Find(delegate(AnimeTypeStats tps) { return tps.Type == AniDB_Anime.AnimeTypes.OVA; });
			AnimeTypeStats Other = GlobalCI.AnimeTypeList.Find(delegate(AnimeTypeStats tps) { return tps.Type == AniDB_Anime.AnimeTypes.Other; });
			AnimeTypeStats Web = GlobalCI.AnimeTypeList.Find(delegate(AnimeTypeStats tps) { return tps.Type == AniDB_Anime.AnimeTypes.Web; });
			AnimeTypeStats Special = GlobalCI.AnimeTypeList.Find(delegate(AnimeTypeStats tps) { return tps.Type == AniDB_Anime.AnimeTypes.TV_Special; });
			//AnimeTypeStats MusicVideo = AnimeTypeList.Find(delegate(AnimeTypeStats tps) { return tps.Type == AniDB_Anime.AnimeTypes.MusicVideo; });
			//AnimeTypeStats Unknown = AnimeTypeList.Find(delegate(AnimeTypeStats tps) { return tps.Type == AniDB_Anime.AnimeTypes.Unknown; });

			
			setGUIProperty("Collection.Stats.TVSeries.InCollection", TVSeries.InCollection.ToString());
			setGUIProperty("Collection.Stats.TVSeries.Watched", TVSeries.Watched.ToString());
			setGUIProperty("Collection.Stats.TVSeries.Watching", TVSeries.Watching.ToString());
			setGUIProperty("Collection.Stats.TVSeries.Size", BytesToMegaBytesString(TVSeries.Size));
			setGUIProperty("Collection.Stats.TVSeries.Runtime", CreateFormatedTimeString(TVSeries.Runtime, 1, true));


			setGUIProperty("Collection.Stats.Movies.InCollection", Movies.InCollection.ToString());
			setGUIProperty("Collection.Stats.Movies.Watched", Movies.Watched.ToString());
			setGUIProperty("Collection.Stats.Movies.Watching", Movies.Watching.ToString());
			setGUIProperty("Collection.Stats.Movies.Size", BytesToMegaBytesString(Movies.Size));
			setGUIProperty("Collection.Stats.Movies.Runtime", CreateFormatedTimeString(Movies.Runtime, 1, true));


			setGUIProperty("Collection.Stats.OVA.InCollection", OVA.InCollection.ToString());
			setGUIProperty("Collection.Stats.OVA.Watched", OVA.Watched.ToString());
			setGUIProperty("Collection.Stats.OVA.Watching", OVA.Watching.ToString());
			setGUIProperty("Collection.Stats.OVA.Size", BytesToMegaBytesString(OVA.Size));
			setGUIProperty("Collection.Stats.OVA.Runtime", CreateFormatedTimeString(OVA.Runtime, 1, true));


			setGUIProperty("Collection.Stats.Other.InCollection", Other.InCollection.ToString());
			setGUIProperty("Collection.Stats.Other.Watched", Other.Watched.ToString());
			setGUIProperty("Collection.Stats.Other.Watching", Other.Watching.ToString());
			setGUIProperty("Collection.Stats.Other.Size", BytesToMegaBytesString(Other.Size));
			setGUIProperty("Collection.Stats.Other.Runtime", CreateFormatedTimeString(Other.Runtime, 1, true));


			setGUIProperty("Collection.Stats.Web.InCollection", Web.InCollection.ToString());
			setGUIProperty("Collection.Stats.Web.Watched", Web.Watched.ToString());
			setGUIProperty("Collection.Stats.Web.Watching", Web.Watching.ToString());
			setGUIProperty("Collection.Stats.Web.Size", BytesToMegaBytesString(Web.Size));
			setGUIProperty("Collection.Stats.Web.Runtime", CreateFormatedTimeString(Web.Runtime, 1, true));


			setGUIProperty("Collection.Stats.TVSpecial.InCollection", Special.InCollection.ToString());
			setGUIProperty("Collection.Stats.TVSpecial.Watched", Special.Watched.ToString());
			setGUIProperty("Collection.Stats.TVSpecial.Watching", Special.Watching.ToString());
			setGUIProperty("Collection.Stats.TVSpecial.Size", BytesToMegaBytesString(Special.Size));
			setGUIProperty("Collection.Stats.TVSpecial.Runtime", CreateFormatedTimeString(Special.Runtime, 1, true));
		}

		private void MainPage()
		{


			setGUIProperty("Collection.Main.AnimeCount", AnimeSeries.GetAll().Count.ToString());
			setGUIProperty("Collection.Main.AnimeGroupCount", FormatTextAnimeGroupCount());
			setGUIProperty("Collection.Main.FansubGroupCount", GlobalCI.FanSubGroups.ToString() + " Known: " + AniDB_GroupStatus.GetAll().Count.ToString());
			setGUIProperty("Collection.Main.EpisodesCount", GlobalCI.Episodes.ToString());
			setGUIProperty("Collection.Main.TotalFiles", GlobalCI.Files.ToString());
			setGUIProperty("Collection.Main.TotalSizeFiles", GlobalCI.TotalFileSize);
			setGUIProperty("Collection.Main.AnimeWatched", GlobalCI.AnimeWatched.ToString());
			setGUIProperty("Collection.Main.AnimePartiallyWatched", GlobalCI.PartiallyWatched.ToString());
			setGUIProperty("Collection.Main.AnimeEpisodesWatched", GlobalCI.EpisodesWatched.ToString());
			setGUIProperty("Collection.Main.PercentageEpisodesWatched", GlobalCI.PercentageEpisodesWatched.ToString() + "%");
			setGUIProperty("Collection.Main.TotalRuntime", GlobalCI.TotalRuntime);
			setGUIProperty("Collection.Main.TotalWatchTime", GlobalCI.TotalWatchTime);
			setGUIProperty("Collection.Main.TotalTimesWatched", GlobalCI.TotalTimesWatched.ToString());
			setGUIProperty("Collection.Main.TotalTimesPlayed", GlobalCI.TotalTimesPlayed.ToString());
			setGUIProperty("Collection.Main.PercentageAniDB", GlobalCI.PercentageAniDB.ToString() + "%");
			setGUIProperty("Collection.Main.PercentageWatchedAniDB", GlobalCI.PercentageAniDBWatched.ToString() + "%");
			setGUIProperty("Collection.Main.RatingsGiven", GlobalCI.RatingsGiven.ToString());
			setGUIProperty("Collection.Main.VoiceActers", GlobalCI.VoiceActers.ToString());
			setGUIProperty("Collection.Main.Genre", GlobalCI.Genres.ToString());
			setGUIProperty("Collection.Main.AnimeWithAwards", GlobalCI.AwardsCount.ToString());
			setGUIProperty("Collection.Main.RRatedAnime", GlobalCI.RRated.ToString());
			setGUIProperty("Collection.Main.Characters", GlobalCI.Characters.ToString());
			setGUIProperty("Collection.Main.AnimeMissingEpisodes", GlobalCI.AnimeMissingEpisodes.ToString());
			setGUIProperty("Collection.Main.MissingEpisodes", GlobalCI.MissingEpisodes.ToString());
			setGUIProperty("Collection.Main.MissingRelations", GlobalCI.MissingRelations.ToString());
		}

#endregion

        private void MissingPage(MissingTypes Type)
		{
			if (Type == MissingTypes.Episodes)
			{
				MissingType = MissingTypes.Episodes;

				dummyCreatingList.Visible = true;

				BaseConfig.MyAnimeLog.Write("COLLECTION: creating missing episode list");

				GUIWindow window = GUIWindowManager.GetWindow(GUIWindowManager.ActiveWindow);
                window.Render(0);

				lstMissing.Clear();

				foreach (ItemDetails Details in GlobalCI.MissingEpisodesList)
				{
					GUIListItem item = new GUIListItem();
					item.Label = Details.Episode.DisplayName;
					item.Label2 = Details.Episode.Series.FormattedName;
					item.ItemId = Details.ID;

					lstMissing.Add(item);

				}

				dummyCreatingList.Visible = false;
				BaseConfig.MyAnimeLog.Write("COLLECTION: finished creating missing episode list");
			}
			else
			{
				MissingType = MissingTypes.Relations;
				lstMissing.Clear();

				foreach (RelationsItem Details in GlobalCI.MissingRelationsList)
				{
					GUIListItem item = new GUIListItem();
					if (!String.IsNullOrEmpty(Details.Relation.EnglishName))
						item.Label = Details.Relation.EnglishName;
					else if (!String.IsNullOrEmpty(Details.Relation.RomajiName))
						item.Label = Details.Relation.RomajiName;
					else
					{
						MainWindow.anidbProcessor.UpdateAnimeInfoHTTP(Details.Relation.AnimeID, false, false);
					}
					item.Label2 = Details.AnimeWithRelation.FormattedName;
					item.ItemId = Details.ID;

					lstMissing.Add(item);

				}
			}
			
			String strType;

			if(MissingType == MissingTypes.Episodes)
				strType = "Episodes";
			else
				strType = "Relations";

			setGUIProperty("Collection.Missing.Type", strType);
		}

		enum AnimeTypes
		{
			TVSeries = 1,
			OVA = 2,
			Movie = 3,
			Special = 4,
			Unknown = 5,
			Other = 6,
			Web = 7,
			MusicVideo = 8
		}

		struct AnimeTypeStats
		{
			public AniDB_Anime.AnimeTypes Type;
			public int Total;
			public int InCollection;
			public int Watched;
			public int Watching;
			public long Size;
			public long Runtime;
		}

		CollectionInfo FormatTextAnime()
		{
			CollectionInfo ci = new CollectionInfo();

			//Setup collectionInfo object
			ci.AnimeTypeList = new List<AnimeTypeStats>();
			ci.MissingRelationsList = new List<RelationsItem>();
			ci.MissingEpisodesList = new List<ItemDetails>();
			ci.Episodes = 0;
			ci.Files = 0;
			ci.AnimeWatched = 0;
			ci.PartiallyWatched = 0;
			ci.EpisodesWatched = 0;
			ci.TotalTimesWatched = 0;
			ci.TotalTimesPlayed = 0;
			ci.RatingsGiven = 0;
			ci.Genres = 0;
			ci.AwardsCount = 0;
			ci.FanSubGroups = 0;
			ci.RRated = 0;
			ci.Characters = 0;
			ci.VoiceActers = 0;
			ci.AnimeMissingEpisodes = 0;
			ci.MissingEpisodes = 0;
			ci.MissingRelations = 0;
            ci.PercentageAniDB = 0;
            ci.PercentageAniDBWatched = 0;

			long iTimeSpent = 0;
			long iTotalRuntime = 0;
			long iTotalFileSize = 0;
			List<String> Genre = new List<string>();
			List<AniDB_Character> CharacterList = new List<AniDB_Character>();
			List<AniDB_GroupStatus> FansubGroupsList = new List<AniDB_GroupStatus>();

			foreach (AniDB_Anime.AnimeTypes AnimeType in Enum.GetValues(typeof(AniDB_Anime.AnimeTypes)))
			{
				
				AnimeTypeStats ATS = new AnimeTypeStats();
				ATS.Type = AnimeType;
				ATS.InCollection = 0;
				ATS.Runtime = 0;
				ATS.Size = 0;
				ATS.Total = 0;
				ATS.Watched = 0;
				ATS.Watching = 0;

				ci.AnimeTypeList.Add(ATS);
			}

			foreach (AnimeSeries anime in AnimeSeries.GetAll())
			{
				if (anime.AniDB_ID == null)
					continue;

				//AnimeTypes Type = AnimeTypes.Unknown;

				AniDB_Anime AniDBAnime = new AniDB_Anime();
				AniDBAnime.Load(anime.AniDB_ID.Value);
				
				int iWatchedEpisodes = 0, iUnwatchedEpisodes = 0;
				string fcount = "";
				anime.GetWatchedUnwatchedCount(ref iUnwatchedEpisodes, ref iWatchedEpisodes, ref fcount);

				foreach (AniDB_GroupStatus group in AniDB_GroupStatus.GetFromQuery<AniDB_GroupStatus>("AnimeID={0} ORDER BY GroupName", anime.AniDB_ID.Value))
				{
					if (group.FileCountLocal != 0)
					{
						if (!FansubGroupsList.Contains(group))
							FansubGroupsList.Add(group);
					}
				}                

				AnimeTypeStats Stats = ci.AnimeTypeList.Find(delegate(AnimeTypeStats tps) { return tps.Type == (AniDB_Anime.AnimeTypes)AniDBAnime.AnimeType; });

				Stats.InCollection++;

				foreach(AnimeEpisode Episode in anime.GetMissingEpisodes())
				{
					//BaseConfig.MyAnimeLog.Write("Episode name: " + Episode.DisplayName);
					ItemDetails Item = new ItemDetails();
				
					Item.Episode = Episode;
					Item.ID = ci.MissingEpisodesList.Count + 1;

					ci.MissingEpisodesList.Add(Item);
				}


				if (iUnwatchedEpisodes == 0)
				{
					ci.AnimeWatched++;
					Stats.Watched++;
				}

				if (iUnwatchedEpisodes > 0 && iWatchedEpisodes > 0)
				{
					ci.PartiallyWatched++;
					Stats.Watching++;
				}

				ci.EpisodesWatched += iWatchedEpisodes;                    

				iTimeSpent += anime.TimeSpentWatching;

				if (anime.UserRating != 0)
					ci.RatingsGiven++;

				if (!string.IsNullOrEmpty(AniDBAnime.AwardList))
					ci.AwardsCount++;

				foreach(AniDB_Character Character in AniDBAnime.Characters)
				{
					if (!CharacterList.Contains(Character))
						CharacterList.Add(Character);

					foreach (AniDB_Creator acter in Character.Creators)
					{
						ci.VoiceActers++;
					}
				}


				ci.TotalTimesWatched += anime.WatchedCount;

				
				if (AniDBAnime.Restricted == 1)
					ci.RRated++;

				if (AniDBAnime.RelatedAnime.Count > 0)
				{
					foreach (AniDB_RelatedAnime relAnime in AniDBAnime.RelatedAnime)
					{
						AniDB_Anime relatedAniDBAnime = new AniDB_Anime();
						relatedAniDBAnime.Load(relAnime.AnimeRelID);

						AnimeSeries relAnimeSeries = new AnimeSeries();
						relAnimeSeries.LoadUsingAniDBSeriesID(relAnime.AnimeRelID);

						if (!relAnimeSeries.LoadUsingAniDBSeriesID(relAnime.AnimeRelID))
						{
							ci.MissingRelations++;

							RelationsItem item = new RelationsItem();
							item.Relation = relatedAniDBAnime;
							item.AnimeWithRelation = anime;
							item.ID = ci.MissingRelationsList.Count; // the count is 1 based while the ID is 0 based, this means i dont have to + 1 to this item's ID when adding a new item to the list.

							ci.MissingRelationsList.Add(item);
						}
					}
				}

				foreach (Genre gen in anime.Genres)
				{
					string strGenreNew;
					strGenreNew = gen.GenreLarge.Trim();

					if (!Genre.Contains(strGenreNew))
						Genre.Add(strGenreNew);
				}


				AnimeEpisodeList AnimeEpisodes = anime.Episodes;
				foreach (AnimeEpisode episode in AnimeEpisodes)
				{
					ci.Episodes++;
					//bool bFoundFile = false;
					ci.TotalTimesPlayed += episode.PlayedCount;

					AniDB_Episode anidbEp = episode.AniDB_Episode;
					iTotalRuntime += anidbEp.LengthSeconds;
					Stats.Runtime += anidbEp.LengthSeconds;

					foreach (FileLocal file in episode.FileLocals)
					{
						ci.Files++;
						
						iTotalFileSize += file.FileSize;
						Stats.Size += file.FileSize;
					}
				}

				if (anime.HasMissingEpisodes)
					ci.AnimeMissingEpisodes++;

				ci.MissingEpisodes += anime.GetMissingEpisodesCount();

				int index = ci.AnimeTypeList.FindIndex(delegate(AnimeTypeStats tps) { return tps.Type == (AniDB_Anime.AnimeTypes)AniDBAnime.AnimeType; });
				ci.AnimeTypeList[index] = Stats;

			}

			if (ci.EpisodesWatched == 0 || ci.Episodes == 0)
				ci.PercentageEpisodesWatched = 0;
			else
				ci.PercentageEpisodesWatched = Decimal.Round((Decimal.Multiply(Decimal.Divide(ci.EpisodesWatched, ci.Episodes), 100)),2);

			ci.Genres += Genre.Count;

			// turn into MegaBytes
			ci.TotalFileSize = BytesToMegaBytesString(iTotalFileSize);

			// Create Time Spent Watching string
			if (iTimeSpent != 0)
				ci.TotalWatchTime = CreateFormatedTimeString(iTimeSpent, 1, false);
			else
				ci.TotalWatchTime = "You have not watched any anime yet. Go Watch some!";

			// Create Total runtime string
			if (iTotalRuntime != 0)
				ci.TotalRuntime = CreateFormatedTimeString(iTotalRuntime, 1, false);
			else
				ci.TotalRuntime = "Runtime is 0. Go get some anime!";

			// Get Character count
			ci.Characters = CharacterList.Count;

			// Fansub groups with anime in collection count
			ci.FanSubGroups = FansubGroupsList.Count;

            // AniDB related percentages
            int TotalAniDBAnime = AniDB_Anime.GetAll().Count;
            ci.PercentageAniDB = Decimal.Round(Decimal.Multiply(Decimal.Divide(AnimeSeries.GetAll().Count, TotalAniDBAnime), 100), 2);
            ci.PercentageAniDBWatched = Decimal.Round(Decimal.Multiply(Decimal.Divide(ci.AnimeWatched, TotalAniDBAnime), 100), 2);



			return ci;
		}

		string BytesToMegaBytesString(long Bytes)
		{
			long iTotalFileSize = (Bytes / 1000) / 1000;

			string TotalFileSize = iTotalFileSize.ToString();

			if (TotalFileSize.Length > 3)
				TotalFileSize = TotalFileSize.Insert((TotalFileSize.Length - 3), ",");

			return TotalFileSize += " MB's";
		}

		string CreateFormatedTimeString(long iRuntime, int baseFactor, bool Short)
		{
			long Months, Days, Hours, Minutes, Seconds = 0;

			Months = (long)Math.Floor((decimal)(iRuntime / (2592000 * baseFactor)));
			iRuntime = iRuntime - ((2592000 * baseFactor) * Months);

			Days = (long)Math.Floor((decimal)(iRuntime / (86400 * baseFactor)));
			iRuntime = iRuntime - ((86400 * baseFactor) * Days);

			Hours = (long)Math.Floor((decimal)(iRuntime / (3600 * baseFactor)));
			iRuntime = iRuntime - ((3600 * baseFactor) * Hours);

			Minutes = (long)Math.Floor((decimal)(iRuntime / (60 * baseFactor)));
			iRuntime = iRuntime - ((60 * baseFactor) * Minutes);

			Seconds = (int)Math.Floor((double)(iRuntime / baseFactor));

			// create string
			String TotalTime = "";

			if (Months != 0)
			{
				TotalTime += Months.ToString();

				if (Short)
					TotalTime += "M ";
				else
					TotalTime += " Months ";
			}

			if (Days != 0)
			{
				TotalTime += Days.ToString();

				if (Short)
					TotalTime += "D ";
				else
					TotalTime += " Days ";
			}

			if (Hours != 0)
			{
				TotalTime += Hours.ToString();

				if (Short)
					TotalTime += "H ";
				else
					TotalTime += " Hours ";
			}

			if (Minutes != 0)
			{
				TotalTime += Minutes.ToString();

				if (Short)
					TotalTime += "M ";
				else
					TotalTime += " Minutes ";
			}

			if (Seconds != 0)
			{
				TotalTime += Seconds.ToString();

				if (Short)
					TotalTime += "S ";
				else
					TotalTime += " Seconds ";
			}

			return TotalTime;
		}
		

		string FormatTextAnimeGroupCount()
		{
			return AnimeGroup.GetAll().Count.ToString();
		}

		void DownloadMissing()
		{
			MissingPageOpen = true;
			ListSelectedItem = lstMissing.SelectedListItemIndex;

			GUIListItem item = lstMissing.SelectedListItem;

			if (MissingType == MissingTypes.Episodes)
			{
				foreach (ItemDetails Details in GlobalCI.MissingEpisodesList)
				{
					if (Details.ID == item.ItemId)
					{
						DownloadHelper.SearchEpisode(Details.Episode);
					}
				}
			}
			else
			{
				foreach (RelationsItem Relation in GlobalCI.MissingRelationsList)
				{
					if ( Relation.ID == item.ItemId )
					{
						DownloadHelper.SearchAnime(Relation.Relation);
					}
				}
			}
		}*/

	}
}
