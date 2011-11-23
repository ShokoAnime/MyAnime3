using System;
using System.Collections.Generic;

using MediaPortal.GUI.Library;

using BinaryNorthwest;

using System.IO;

using MediaPortal.Dialogs;
using MyAnimePlugin3.Downloads;
using Action = MediaPortal.GUI.Library.Action;
using MyAnimePlugin3.ViewModel;
using System.ComponentModel;

namespace MyAnimePlugin3.Windows
{
	public class ContinueWatchingWindow : GUIWindow
	{
		private List<AnimeEpisodeVM> colEpisodes = new List<AnimeEpisodeVM>();
		private BackgroundWorker getDataWorker = new BackgroundWorker();

		[SkinControlAttribute(50)] protected GUIFacadeControl m_Facade = null;

		[SkinControlAttribute(801)] protected GUIButtonControl btnRefresh = null;

		//[SkinControlAttribute(920)] protected GUIButtonControl btnWindowContinueWatching = null;
		[SkinControlAttribute(921)] protected GUIButtonControl btnWindowUtilities = null;
		[SkinControlAttribute(922)] protected GUIButtonControl btnWindowCalendar = null;
		[SkinControlAttribute(923)] protected GUIButtonControl btnWindowDownloads = null;
		//[SkinControlAttribute(924)] protected GUIButtonControl btnWindowCollectionStats = null;
		[SkinControlAttribute(925)] protected GUIButtonControl btnWindowRecommendations = null;

		[SkinControlAttribute(1451)] protected GUILabelControl dummyAnyRecords = null;

		public ContinueWatchingWindow()
		{
			// get ID of windowplugin belonging to this setup
			// enter your own unique code
			GetID = Constants.WindowIDs.WATCHING;

			setGUIProperty("Watching.Status", "-");

			getDataWorker.DoWork += new DoWorkEventHandler(getDataWorker_DoWork);
			getDataWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(getDataWorker_RunWorkerCompleted);
		}

		void getDataWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			colEpisodes = e.Result as List<AnimeEpisodeVM>;

			if (colEpisodes == null || colEpisodes.Count == 0)
			{
				if (dummyAnyRecords != null) dummyAnyRecords.Visible = false;
				setGUIProperty("Watching.Status", "No episodes have recently been watched");
				return;
			}

			if (dummyAnyRecords != null) dummyAnyRecords.Visible = true;

			foreach (AnimeEpisodeVM ep in colEpisodes)
			{
				GUIListItem item = new GUIListItem(ep.AnimeSeries.SeriesName);
				AniDB_AnimeVM anime = ep.AnimeSeries.AniDB_Anime;

				string imagePath = GUIGraphicsContext.Skin + @"\Media\MyAnime3\anime3_question_poster.png";
				if (anime.DefaultPosterPath.Trim().Length > 0)
				{
					if (File.Exists(anime.DefaultPosterPath))
						imagePath = anime.DefaultPosterPath;
				}

				item.IconImage = item.IconImageBig = imagePath;
				item.TVTag = ep;
				item.OnItemSelected += new GUIListItem.ItemSelectedHandler(onFacadeItemSelected);
				m_Facade.Add(item);
			}

			if (m_Facade.Count > 0)
			{
				m_Facade.SelectedListItemIndex = 0;

				AnimeEpisodeVM ep = m_Facade.SelectedListItem.TVTag as AnimeEpisodeVM;
				if (ep != null)
				{
					SetEpisode(ep);
				}
			}
		}

		void getDataWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			List<AnimeEpisodeVM> tempEpisodes = new List<AnimeEpisodeVM>();
			List<JMMServerBinary.Contract_AnimeEpisode> epContracts = JMMServerVM.Instance.clientBinaryHTTP.GetEpisodesToWatch_RecentlyWatched(
				15, JMMServerVM.Instance.CurrentUser.JMMUserID);

			foreach (JMMServerBinary.Contract_AnimeEpisode contract in epContracts)
			{
				AnimeEpisodeVM ep = new AnimeEpisodeVM(contract);
				tempEpisodes.Add(ep);
			}

			// just doing this to preload the series and anime data
			foreach (AnimeEpisodeVM ep in colEpisodes)
			{
				AniDB_AnimeVM anime = ep.AnimeSeries.AniDB_Anime;
			}

			e.Result = tempEpisodes;
			
		}

		public override bool Init()
		{
			return Load(GUIGraphicsContext.Skin + @"\Anime3_Watching.xml");
		}

		public override int GetID
		{
			get { return Constants.WindowIDs.WATCHING; }
			set { base.GetID = value; }
		}

		protected override void OnPageLoad()
		{
			base.OnPageLoad();
			m_Facade.CurrentLayout = GUIFacadeControl.Layout.Filmstrip;
			BaseConfig.MyAnimeLog.Write("OnPageLoad: ContinueWatchingWindow");

			LoadData();
			m_Facade.Focus = true;
		}

		public static void setGUIProperty(string which, string value)
		{
			MediaPortal.GUI.Library.GUIPropertyManager.SetProperty("#Anime3." + which, value);
		}

		public static void clearGUIProperty(string which)
		{
			setGUIProperty(which, "-"); // String.Empty doesn't work on non-initialized fields, as a result they would display as ugly #TVSeries.bla.bla
		}

		private void LoadData()
		{
			colEpisodes.Clear();
			GUIControl.ClearControl(this.GetID, m_Facade.GetID);
			setGUIProperty("Watching.Status", "Loading Data...");
			if (dummyAnyRecords != null) dummyAnyRecords.Visible = false;

			getDataWorker.RunWorkerAsync();
		}

		private void onFacadeItemSelected(GUIListItem item, GUIControl parent)
		{
			//BaseConfig.MyAnimeLog.Write("Facade Item Selected");
			// if this is not a message from the facade, exit
			if (parent != m_Facade && parent != m_Facade.FilmstripLayout)
				return;

			AnimeEpisodeVM ep = m_Facade.SelectedListItem.TVTag as AnimeEpisodeVM;
			SetEpisode(ep);

		}

		private void SetEpisode(AnimeEpisodeVM ep)
		{
			if (ep == null) return;

			AniDB_AnimeVM anime = ep.AnimeSeries.AniDB_Anime;

			Dictionary<int, TvDB_EpisodeVM> dictTvDBEpisodes = anime.DictTvDBEpisodes;
			Dictionary<int, int> dictTvDBSeasons = anime.DictTvDBSeasons;
			Dictionary<int, int> dictTvDBSeasonsSpecials = anime.DictTvDBSeasonsSpecials;
			CrossRef_AniDB_TvDBVM tvDBCrossRef = anime.CrossRefTvDB;
			ep.SetTvDBInfo(dictTvDBEpisodes, dictTvDBSeasons, dictTvDBSeasonsSpecials, tvDBCrossRef);


			clearGUIProperty("Watching.Series.Title");
			clearGUIProperty("Watching.Series.Description");
			clearGUIProperty("Watching.Series.LastWatched");
			clearGUIProperty("Watching.Series.EpisodesAvailable");
			clearGUIProperty("Watching.Episode.Title");
			clearGUIProperty("Watching.Episode.AirDate");
			clearGUIProperty("Watching.Episode.RunTime");
			clearGUIProperty("Watching.Episode.FileInfo");
			clearGUIProperty("Watching.Episode.Overview");
			clearGUIProperty("Watching.Episode.Image");
			clearGUIProperty("Watching.Episode.Logos");


			setGUIProperty("Watching.Series.Title", ep.AnimeSeries.SeriesName);
			setGUIProperty("Watching.Series.Description", ep.AnimeSeries.Description);
			setGUIProperty("Watching.Series.LastWatched", ep.AnimeSeries.WatchedDate.HasValue ? ep.AnimeSeries.WatchedDate.Value.ToString("dd MMM yy", Globals.Culture) : "-");
			setGUIProperty("Watching.Series.EpisodesAvailable", ep.AnimeSeries.UnwatchedEpisodeCount.ToString());

			setGUIProperty("Watching.Episode.Title", ep.EpisodeNumberAndNameWithType);
			setGUIProperty("Watching.Episode.AirDate", ep.AirDateAsString);
			setGUIProperty("Watching.Episode.RunTime", Utils.FormatSecondsToDisplayTime(ep.AniDB_LengthSeconds));
			

			if (ep.EpisodeImageLocation.Length > 0)
				setGUIProperty("Watching.Episode.Image", ep.EpisodeImageLocation);

			// Overview
			string overview = ep.EpisodeOverview;
			if (BaseConfig.Settings.HidePlot)
			{ 
				if (ep.EpisodeOverview.Trim().Length > 0 && ep.IsWatched == 0)
					overview = "*** Hidden to prevent spoilers ***";
			}
			setGUIProperty("Watching.Episode.Overview", overview);

			// File Info
			List<VideoDetailedVM>  filesForEpisode = new List<VideoDetailedVM>();
			List<JMMServerBinary.Contract_VideoDetailed> contracts = JMMServerVM.Instance.clientBinaryHTTP.GetFilesForEpisode(ep.AnimeEpisodeID,
				JMMServerVM.Instance.CurrentUser.JMMUserID);

			foreach (JMMServerBinary.Contract_VideoDetailed fi in contracts)
				filesForEpisode.Add(new VideoDetailedVM(fi));

			string finfo = "";
			foreach (VideoDetailedVM vid in filesForEpisode)
				finfo = vid.FileSelectionDisplay;

			if (filesForEpisode.Count > 1)
				finfo = filesForEpisode.Count.ToString() + " Files Available";

			setGUIProperty("Watching.Episode.FileInfo", finfo);

			// Logos
			string logos = Logos.buildLogoImage(ep);

			BaseConfig.MyAnimeLog.Write(logos);
			setGUIProperty("Watching.Episode.Logos", logos);
		}

		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			if (MA3WindowManager.HandleWindowChangeButton(control))
				return;

			if (btnRefresh != null && control == btnRefresh)
			{
				this.btnRefresh.IsFocused = false;
				m_Facade.Focus = true;
				LoadData();
			}

			if (control == this.m_Facade)
			{
				// show the files if we are looking at a torrent
				GUIListItem item = m_Facade.SelectedListItem;
				if (item == null || item.TVTag == null) return;
				if (item.TVTag.GetType() == typeof(AnimeEpisodeVM))
				{
					AnimeEpisodeVM ep = item.TVTag as AnimeEpisodeVM;
					if (ep != null)
					{
						MainWindow.vidHandler.ResumeOrPlay(ep);
					}
				}
			}

			base.OnClicked(controlId, control, actionType);
		}

		protected override void OnShowContextMenu()
		{
			GUIListItem currentitem = this.m_Facade.SelectedListItem;
			if (currentitem == null) return;

			if (currentitem.TVTag.GetType() == typeof(AnimeEpisodeVM))
			{
				AnimeEpisodeVM ep = currentitem.TVTag as AnimeEpisodeVM;
				if (ep != null)
				{
					GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
					if (dlg == null)
						return;

					dlg.Reset();
					dlg.SetHeading(ep.EpisodeNumberAndName);
					dlg.Add("Mark as Watched");
					
					dlg.DoModal(GUIWindowManager.ActiveWindow);

					switch (dlg.SelectedLabel)
					{
						case 0:
							ep.ToggleWatchedStatus(true);
							LoadData();
							break;

					}
				}
			}
		}
	}
}
