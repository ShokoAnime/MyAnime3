using System;
using System.Collections.Generic;
using System.Text;

using MediaPortal.GUI.Library;
using System.IO;
using System.Linq;
using MediaPortal.Dialogs;
using Shoko.Models.Client;
using Action = MediaPortal.GUI.Library.Action;
using Shoko.MyAnime3.ViewModel;
using Shoko.MyAnime3.ViewModel.Helpers;
using Shoko.MyAnime3.ViewModel.Server;

namespace Shoko.MyAnime3.Windows
{
	public class RelationsWindow : GUIWindow
	{
		[SkinControl(50)] protected GUIFacadeControl m_Facade = null;

		[SkinControl(1301)] protected GUILabelControl dummyRelationsExist = null;

		[SkinControl(1302)] protected GUILabelControl dummyHasSeries = null;
		[SkinControl(1303)] protected GUILabelControl dummyHasFiles = null;

		[SkinControl(2)] protected GUIButtonControl btnGetMissingInfo = null;

		[SkinControl(910)] protected GUIButtonControl btnAnimeInfo = null;
		[SkinControl(911)] protected GUIButtonControl btnAnimeCharacters = null;
		//[SkinControlAttribute(912)] protected GUIButtonControl btnAnimeRelations = null;
		[SkinControl(913)] protected GUIButtonControl btnAnimeFanart = null;
		[SkinControl(914)] protected GUIButtonControl btnAnimePosters = null;
		[SkinControl(915)] protected GUIButtonControl btnAnimeWideBanners = null;

	    public enum GuiProperty
	    {
	        Related_DownloadStatus,
	        Related_Main_Title,
	        Related_Main_Year,
            Related_Title,
            Related_Episodes,
	        Related_Year,
	        Related_Description,
	        Related_Genre,
	        Related_GenreShort,
	        Related_Status,
	        Related_Relationship
	    }

	    public void SetGUIProperty(GuiProperty which, string value) { this.SetGUIProperty(which.ToString(), value); }
        public void ClearGUIProperty(GuiProperty which) { this.ClearGUIProperty(which.ToString()); }



        private List<CL_AniDB_Anime_Relation> relations = new List<CL_AniDB_Anime_Relation>();
		VM_AnimeSeries_User serMain = null;
		VM_AniDB_Anime mainAnime = null;

		public RelationsWindow()
		{
			// get ID of windowplugin belonging to this setup
			// enter your own unique code
			GetID = Constants.WindowIDs.RELATIONS;

			MainWindow.ServerHelper.GotRelatedAnimeEvent += new ShokoServerHelper.GotRelatedAnimeEventHandler(ServerHelper_GotRelatedAnimeEvent);
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
                serMain = ShokoServerHelper.GetSeries(MainWindow.GlobalSeriesID);
                if (serMain != null)
                    mainAnime = serMain.Anime;
            }
        }

        private void LoadData()
		{
			relations.Clear();

			if (serMain != null)
				mainAnime = serMain.Anime;

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

            BaseConfig.MyAnimeLog.Write("CharWindow.GlobalSeriesID = {0}", MainWindow.GlobalSeriesID.ToString());
            BaseConfig.MyAnimeLog.Write("CharWindow.Relations count = " + relations.Count);

            if (relations?.Count > 0)
		    {
		        foreach (CL_AniDB_Anime_Relation ra in relations)
		        {
		            string imagePath = "";

		            if (ra.AniDB_Anime != null && ra.AnimeSeries != null)
		            {
                        BaseConfig.MyAnimeLog.Write("AnimeID: " + MainWindow.GlobalSeriesID + ", Related ID: " +
                            ra.AniDB_Anime.AnimeID.ToString());
                        BaseConfig.MyAnimeLog.Write("Poster Path: " + ((VM_AniDB_Anime)ra.AniDB_Anime).DefaultPosterPath);

                        VM_AniDB_Anime anime = (VM_AniDB_Anime)ra.AniDB_Anime;
		                // try and load the series
		                VM_AnimeSeries_User serAnime = (VM_AnimeSeries_User)ra.AnimeSeries;

		                if (serAnime != null)
		                {
		                    string posterName = ImageAllocator.GetSeriesImageAsFileName(serAnime,
		                        GUIFacadeControl.Layout.Filmstrip);
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

                    // If it has no title skip it as it's probably an invalid item
		            if (!string.IsNullOrEmpty(ra.AniDB_Anime?.MainTitle))
		            {
		                GUIListItem item = new GUIListItem();
		                item.IconImage = item.IconImageBig = imagePath;
		                item.TVTag = ra;
		                item.OnItemSelected += new GUIListItem.ItemSelectedHandler(onFacadeItemSelected);
		                m_Facade.Add(item);
		            }
		        }
		    }

		    if (m_Facade.Count > 0)
			{
				m_Facade.SelectedListItemIndex = 0;

			    CL_AniDB_Anime_Relation ra = m_Facade.SelectedListItem?.TVTag as CL_AniDB_Anime_Relation;
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

		    if (item.TVTag != null)
		    {
		        CL_AniDB_Anime_Relation ra = item.TVTag as CL_AniDB_Anime_Relation;
		        if (ra != null)
		        {
		            SetAnime(ra);
		        }
		    }
		}

		private void SetAnime(CL_AniDB_Anime_Relation ra)
		{
            VM_AniDB_Anime anime = (VM_AniDB_Anime)ra.AniDB_Anime;
            ClearGUIProperty(GuiProperty.Related_Status);

			if (dummyHasSeries != null) dummyHasSeries.Visible = false;
			if (dummyHasSeries != null && ra.AnimeSeries != null) dummyHasSeries.Visible = false;
            if (anime != null)
            {
                SetGUIProperty(GuiProperty.Related_Title, anime.MainTitle);
                SetGUIProperty(GuiProperty.Related_Episodes, anime.EpisodeCountNormal.ToString(Globals.Culture) + " (" + anime.EpisodeCountSpecial.ToString(Globals.Culture) + " " + Translation.Specials + ")");
                SetGUIProperty(GuiProperty.Related_Year, anime.AirDateAsString);
                SetGUIProperty(GuiProperty.Related_Description, anime.ParsedDescription);
                SetGUIProperty(GuiProperty.Related_Genre, anime.TagsFormatted);
                SetGUIProperty(GuiProperty.Related_GenreShort, anime.TagsFormattedShort);

                if (string.IsNullOrEmpty(ra.RelationType))
                    SetGUIProperty(GuiProperty.Related_Relationship, string.Empty);
                else
                    SetGUIProperty(GuiProperty.Related_Relationship, $"({ra.RelationType})");
            }
            else
            {
                ClearGUIProperty(GuiProperty.Related_Title);
                ClearGUIProperty(GuiProperty.Related_Episodes);
                ClearGUIProperty(GuiProperty.Related_Year);
                ClearGUIProperty(GuiProperty.Related_Description);
                ClearGUIProperty(GuiProperty.Related_Genre);
                ClearGUIProperty(GuiProperty.Related_GenreShort);
                ClearGUIProperty(GuiProperty.Related_Relationship);
            }

            SetGUIProperty(GuiProperty.Related_Status, ra.AnimeSeries != null ? (ra.AnimeSeries.MissingEpisodeCount > 0 ? Translation.Collecting : Translation.AllEpisodesAvailable) : Translation.NotInMyCollection);

		}


		public override bool Init()
		{
            return this.InitSkin<GuiProperty>("Anime3_Relations.xml");

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
				CL_AniDB_Anime_Relation ra = m_Facade.SelectedListItem.TVTag as CL_AniDB_Anime_Relation;
				if (ra != null && ra.AnimeSeries != null && ra.AnimeSeries.AnimeSeriesID!=0)
				{
					// show relations for this anime
					MainWindow.GlobalSeriesID = ra.AnimeSeries.AnimeSeriesID;
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
