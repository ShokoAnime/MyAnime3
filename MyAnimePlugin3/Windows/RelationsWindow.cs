using System;
using System.Collections.Generic;
using System.Text;

using MediaPortal.GUI.Library;
using System.IO;
using AniDBHelper;
using MediaPortal.Dialogs;
using MyAnimePlugin3.Downloads;
using Action = MediaPortal.GUI.Library.Action;
using MyAnimePlugin3.ViewModel;

namespace MyAnimePlugin3.Windows
{
	public class RelationsWindow : GUIWindow
	{
		[SkinControlAttribute(50)] protected GUIFacadeControl m_Facade = null;

		[SkinControlAttribute(1301)] protected GUILabelControl dummyRelationsExist = null;

		[SkinControlAttribute(1302)] protected GUILabelControl dummyHasSeries = null;
		[SkinControlAttribute(1303)] protected GUILabelControl dummyHasFiles = null;

		[SkinControlAttribute(2)] protected GUIButtonControl btnGetMissingInfo = null;

		[SkinControlAttribute(910)] protected GUIButtonControl btnAnimeInfo = null;
		[SkinControlAttribute(911)] protected GUIButtonControl btnAnimeCharacters = null;
		//[SkinControlAttribute(912)] protected GUIButtonControl btnAnimeRelations = null;
		[SkinControlAttribute(913)] protected GUIButtonControl btnAnimeFanart = null;
		[SkinControlAttribute(914)] protected GUIButtonControl btnAnimePosters = null;
		[SkinControlAttribute(915)] protected GUIButtonControl btnAnimeWideBanners = null;


		private List<AniDB_Anime_RelationVM> relations = new List<AniDB_Anime_RelationVM>();
		AnimeSeriesVM serMain = null;
		AniDB_AnimeVM mainAnime = null;

		public RelationsWindow()
		{
			// get ID of windowplugin belonging to this setup
			// enter your own unique code
			GetID = Constants.WindowIDs.RELATIONS;

			MainWindow.ServerHelper.GotRelatedAnimeEvent += new JMMServerHelper.GotRelatedAnimeEventHandler(ServerHelper_GotRelatedAnimeEvent);

			setGUIProperty("Related.DownloadStatus", "-");
		}

		void ServerHelper_GotRelatedAnimeEvent(Events.GotAnimeForRelatedEventArgs ev)
		{
			if (GUIWindowManager.ActiveWindow != Constants.WindowIDs.RELATIONS) return;
			setGUIProperty("Related.DownloadStatus", "-");
			int aid = ev.AnimeID;
			LoadData();
			ShowRelations();
		}


		public override int GetID
		{
			get { return Constants.WindowIDs.RELATIONS; }
			set { base.GetID = value; }
		}

		protected override void OnPageLoad()
		{
			base.OnPageLoad();

			if (m_Facade != null)
				m_Facade.CurrentLayout = GUIFacadeControl.Layout.Filmstrip;
			

			BaseConfig.MyAnimeLog.Write("OnPageLoad: {0}", MainWindow.GlobalSeriesID.ToString());

			LoadData();

			ShowMainAnime();
			ShowRelations();

			m_Facade.Focus = true;
		}

		private void LoadData()
		{
			relations.Clear();

			mainAnime = null;
			serMain = null;



			serMain = JMMServerHelper.GetSeries(MainWindow.GlobalSeriesID);
			if (serMain != null)
				mainAnime = serMain.AniDB_Anime;

			if (mainAnime == null)
				return;

			try
			{
				relations = mainAnime.RelatedAnimeLinks;
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write(ex.ToString());
			}

		}

		private void ShowMainAnime()
		{
			setGUIProperty("Related.Main.Title", mainAnime.FormattedTitle);
			setGUIProperty("Related.Main.Year", mainAnime.Year);

		}

		private void ShowRelations()
		{
			GUIControl.ClearControl(this.GetID, m_Facade.GetID);

			if (dummyRelationsExist != null)
			{
				if (relations.Count > 0) dummyRelationsExist.Visible = true;
				else dummyRelationsExist.Visible = false;
			}

			string imagePathMissing = GUIGraphicsContext.Skin + @"\Media\MyAnime3\anime3_question_poster.png";
			string imagePathNoPicture = GUIGraphicsContext.Skin + @"\Media\MyAnime3\anime3_blankchar.png";

			foreach (AniDB_Anime_RelationVM ra in relations)
			{
				string imagePath = "";

				BaseConfig.MyAnimeLog.Write("AnimeID: " + mainAnime.AnimeID.ToString() + ", Related ID: " + ra.AniDB_Anime.AnimeID.ToString());
				BaseConfig.MyAnimeLog.Write("Poster Path: " + ra.AniDB_Anime.DefaultPosterPath);

				AniDB_AnimeVM anime = ra.AniDB_Anime;
				if (anime != null)
				{
					// try and load the series
					AnimeSeriesVM serAnime = ra.AnimeSeries;
					if (serAnime != null)
					{
						string posterName = ImageAllocator.GetSeriesImageAsFileName(serAnime, GUIFacadeControl.Layout.Filmstrip);
						if (File.Exists(posterName)) imagePath = posterName;
					}

					if (imagePath.Length == 0)
					{
						if (anime.DefaultPosterPath.Trim().Length > 0 && File.Exists(anime.DefaultPosterPath))
							imagePath = anime.DefaultPosterPath;
						else
							imagePath = imagePathNoPicture;

					}
				}
				else
					imagePath = imagePathMissing;


				GUIListItem item = new GUIListItem();
				item.IconImage = item.IconImageBig = imagePath;
				item.TVTag = ra;
				item.OnItemSelected += new GUIListItem.ItemSelectedHandler(onFacadeItemSelected);
				m_Facade.Add(item);
			}

			if (m_Facade.Count > 0)
			{
				m_Facade.SelectedListItemIndex = 0;

				AniDB_Anime_RelationVM ra = m_Facade.SelectedListItem.TVTag as AniDB_Anime_RelationVM;
				if (ra != null)
				{
					SetAnime(ra);
				}
			}

			//GUIControl.FocusControl(GetID, 50);
		}



		// triggered when a selection change was made on the facade
		private void onFacadeItemSelected(GUIListItem item, GUIControl parent)
		{
			// if this is not a message from the facade, exit
			//if (parent != m_Facade)
			//	return;

			AniDB_Anime_RelationVM ra = item.TVTag as AniDB_Anime_RelationVM;
			if (ra != null)
			{
				SetAnime(ra);
			}

		}

		private void SetAnime(AniDB_Anime_RelationVM ra)
		{
			AniDB_AnimeVM anime = ra.AniDB_Anime;

			setGUIProperty("Related.Title", ra.DisplayName);
			setGUIProperty("Related.Relationship", ra.RelationType);
			setGUIProperty("Related.Episodes", "-");
			setGUIProperty("Related.Year", "-");
			setGUIProperty("Related.Description", "-");
			setGUIProperty("Related.Genre", "-");
			setGUIProperty("Related.GenreShort", "-");
			setGUIProperty("Related.Status", "-");

			if (dummyHasSeries != null) dummyHasSeries.Visible = false;
			if (dummyHasSeries != null && ra.AnimeSeries != null) dummyHasSeries.Visible = false;

			if (anime != null)
			{
				setGUIProperty("Related.Episodes", anime.EpisodeCountNormal.ToString() + " (" + anime.EpisodeCountSpecial.ToString() + " Specials)");
				setGUIProperty("Related.Year", anime.AirDateAsString);
				setGUIProperty("Related.Description", anime.ParsedDescription);
				setGUIProperty("Related.Genre", anime.CategoriesFormatted);
				setGUIProperty("Related.GenreShort", anime.CategoriesFormattedShort);
			}

			if (ra.AnimeSeries != null)
			{
				if (ra.AnimeSeries.MissingEpisodeCount > 0)
					setGUIProperty("Related.Status", "Collecting");
				else
					setGUIProperty("Related.Status", "All Episodes Available");
			}
			else
				setGUIProperty("Related.Status", "Not In My Collection");
		}


		public override bool Init()
		{
			return Load(GUIGraphicsContext.Skin + @"\Anime3_Relations.xml");
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

					base.OnAction(action);
					break;
				default:
					base.OnAction(action);
					break;
			}
		}

		protected override void OnShowContextMenu()
		{
			try
			{
				GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
				if (dlg == null)
					return;

				dlg.Reset();
				dlg.SetHeading("Relations");
				dlg.Add("Search for Torrents");
				dlg.DoModal(GUIWindowManager.ActiveWindow);

				switch (dlg.SelectedId)
				{
					case 1:
						DownloadHelper.SearchAnime(mainAnime);
						break;
				}
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write("Error in menu: {0}", ex);
			}
		}

		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			//BaseConfig.MyAnimeLog.Write("OnClicked: {0}", controlId.ToString());

			

			if (control == this.m_Facade)
			{
				AniDB_Anime_RelationVM ra = m_Facade.SelectedListItem.TVTag as AniDB_Anime_RelationVM;
				if (ra != null && ra.AnimeSeries != null)
				{
					// show relations for this anime
					MainWindow.GlobalSeriesID = ra.AnimeSeries.AnimeSeriesID.Value;
					GUIWindowManager.CloseCurrentWindow();
					GUIWindowManager.ActivateWindow(Constants.WindowIDs.RELATIONS, false);
				}
				else
				{
					Utils.DialogMsg("Error", "You do not have this series in your collection");
					return;
				}
			}

			if (this.btnGetMissingInfo != null && control == this.btnGetMissingInfo)
			{
				MainWindow.ServerHelper.DownloadRelatedAnime(mainAnime.AnimeID);
				setGUIProperty("Related.DownloadStatus", "Waiting on server...");
				GUIControl.FocusControl(GetID, 50);

				return;
			}

			if (MA3WindowManager.HandleWindowChangeButton(control))
				return;

			base.OnClicked(controlId, control, actionType);
		}
	}
}
