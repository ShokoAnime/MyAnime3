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

        public enum GuiProperty
        {
            Related_DownloadStatus,
            Related_Main_Title,
            Related_Main_Year,
            Related_Episodes,
            Related_Year,
            Related_Description,
            Related_Genre,
            Related_GenreShort,
            Related_Status

        }
        public void SetGUIProperty(GuiProperty which, string value) { this.SetGUIProperty(which.ToString(), value); }
        public void ClearGUIProperty(GuiProperty which) { this.ClearGUIProperty(which.ToString()); }



        private List<AniDB_Anime_RelationVM> relations = new List<AniDB_Anime_RelationVM>();
		AnimeSeriesVM serMain = null;
		AniDB_AnimeVM mainAnime = null;

		public RelationsWindow()
		{
			// get ID of windowplugin belonging to this setup
			// enter your own unique code
			GetID = Constants.WindowIDs.RELATIONS;

			MainWindow.ServerHelper.GotRelatedAnimeEvent += new JMMServerHelper.GotRelatedAnimeEventHandler(ServerHelper_GotRelatedAnimeEvent);
		}

		void ServerHelper_GotRelatedAnimeEvent(Events.GotAnimeForRelatedEventArgs ev)
		{
			if (GUIWindowManager.ActiveWindow != Constants.WindowIDs.RELATIONS) return;
            ClearGUIProperty(GuiProperty.Related_DownloadStatus);
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

            LoadInfo();
            LoadData();

			ShowMainAnime();
			ShowRelations();

            if (m_Facade != null)
                m_Facade.Focus = true;
		}

        private void LoadInfo()
        {
            if (MainWindow.GlobalSeriesID > 0)
            {
                serMain = JMMServerHelper.GetSeries(MainWindow.GlobalSeriesID);
                if (serMain != null)
                    mainAnime = serMain.AniDB_Anime;
            }
        }

        private void LoadData()
		{
			relations.Clear();

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
            SetGUIProperty(GuiProperty.Related_Main_Title, mainAnime.FormattedTitle);
            SetGUIProperty(GuiProperty.Related_Main_Year, mainAnime.Year);

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
            ClearGUIProperty(GuiProperty.Related_Status);

			if (dummyHasSeries != null) dummyHasSeries.Visible = false;
			if (dummyHasSeries != null && ra.AnimeSeries != null) dummyHasSeries.Visible = false;
            if (anime != null)
            {
                SetGUIProperty(GuiProperty.Related_Episodes, anime.EpisodeCountNormal.ToString(Globals.Culture) + " (" + anime.EpisodeCountSpecial.ToString(Globals.Culture) + " " + Translation.Specials + ")");
                SetGUIProperty(GuiProperty.Related_Year, anime.AirDateAsString);
                SetGUIProperty(GuiProperty.Related_Description, anime.ParsedDescription);
                SetGUIProperty(GuiProperty.Related_Genre, anime.TagsFormatted);
                SetGUIProperty(GuiProperty.Related_GenreShort, anime.TagsFormattedShort);
            }
            else
            {
                ClearGUIProperty(GuiProperty.Related_Episodes);
                ClearGUIProperty(GuiProperty.Related_Year);
                ClearGUIProperty(GuiProperty.Related_Description);
                ClearGUIProperty(GuiProperty.Related_Genre);
                ClearGUIProperty(GuiProperty.Related_GenreShort);
            }

            SetGUIProperty(GuiProperty.Related_Status, ra.AnimeSeries != null ? (ra.AnimeSeries.MissingEpisodeCount > 0 ? Translation.Collecting : Translation.AllEpisodesAvailable) : Translation.NotInMyCollection);

		}


		public override bool Init()
		{
            return this.InitSkin<GuiProperty>("Anime3_Relations.xml");

		}



		protected override void OnShowContextMenu()
		{
			try
			{
                ContextMenu cmenu = new ContextMenu(Translation.Relations);
                cmenu.AddAction(Translation.SearchForTorrents, () =>
                {
                    DownloadHelper.SearchAnime(mainAnime);
                });
                cmenu.Show();
            }
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write("Error in menu: {0}", ex);
			}
		}

		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
            //BaseConfig.MyAnimeLog.Write("OnClicked: {0}", controlId.ToString());

            MainMenu menu = new MainMenu();
            menu.Add(btnGetMissingInfo, () =>
            {
                SetGUIProperty(GuiProperty.Related_DownloadStatus, Translation.WaitingOnServer + "...");
                m_Facade.Focus = true;
                MainWindow.ServerHelper.DownloadRelatedAnime(mainAnime.AnimeID);
            });
            if (menu.Check(control))
                return;

            if (control == this.m_Facade)
			{
				AniDB_Anime_RelationVM ra = m_Facade.SelectedListItem.TVTag as AniDB_Anime_RelationVM;
				if (ra != null && ra.AnimeSeries != null && ra.AnimeSeries.AnimeSeriesID.HasValue)
				{
					// show relations for this anime
					MainWindow.GlobalSeriesID = ra.AnimeSeries.AnimeSeriesID.Value;
					GUIWindowManager.CloseCurrentWindow();
					GUIWindowManager.ActivateWindow(Constants.WindowIDs.RELATIONS, false);
				}
				else
				{
                    Utils.DialogMsg(Translation.Error, Translation.YouDontHaveTheSeries);
                    return;
				}
			}


			base.OnClicked(controlId, control, actionType);
		}
	}
}
