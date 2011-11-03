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
	public class RelationsWindow_Old : GUIWindow
	{
		[SkinControlAttribute(50)] protected GUIFacadeControl m_Facade = null;

		[SkinControlAttribute(1333)] protected GUILabelControl dummyMainHasFiles = null;

		[SkinControlAttribute(1301)] protected GUILabelControl dummyPrequelExists = null;
		[SkinControlAttribute(1302)] protected GUILabelControl dummyPrequelHasInfo = null;
		[SkinControlAttribute(1303)] protected GUILabelControl dummyPrequelHasFiles = null;

		[SkinControlAttribute(1311)] protected GUILabelControl dummySequelExists = null;
		[SkinControlAttribute(1312)] protected GUILabelControl dummySequelHasInfo = null;
		[SkinControlAttribute(1313)] protected GUILabelControl dummySequelHasFiles = null;

		[SkinControlAttribute(1321)] protected GUILabelControl dummyOtherExists = null;
		[SkinControlAttribute(1322)] protected GUILabelControl dummyOtherHasInfo = null;
		[SkinControlAttribute(1323)] protected GUILabelControl dummyOtherHasFiles = null;

		[SkinControlAttribute(71)] protected GUIImage imgMainShow = null;
		[SkinControlAttribute(72)] protected GUIImage imgSequel = null;
		[SkinControlAttribute(73)] protected GUIImage imgPrequel = null;

		[SkinControlAttribute(81)] protected GUIButtonControl btnMain = null;
		[SkinControlAttribute(82)] protected GUIButtonControl btnSequel = null;
		[SkinControlAttribute(83)] protected GUIButtonControl btnPrequel = null;

		[SkinControlAttribute(101)] protected GUILabelControl lblSequelTitle = null;
		[SkinControlAttribute(102)] protected GUILabelControl lblSequelYear = null;

		[SkinControlAttribute(201)] protected GUILabelControl lblPrequelTitle = null;
		[SkinControlAttribute(202)] protected GUILabelControl lblPrequelYear = null;

		[SkinControlAttribute(901)] protected GUIButtonControl btnNavLeft = null;
		[SkinControlAttribute(902)] protected GUIButtonControl btnNavRight = null;

		private List<AniDB_Anime_RelationVM> relations = new List<AniDB_Anime_RelationVM>();
		AnimeSeriesVM serMain = null;
		AniDB_AnimeVM mainAnime = null;

		AniDB_AnimeVM sequelAnime = null;
		AnimeSeriesVM serSequel = null;

		AniDB_AnimeVM prequelAnime = null;
		AnimeSeriesVM serPrequel = null;

		public RelationsWindow_Old()
		{
			// get ID of windowplugin belonging to this setup
			// enter your own unique code
			GetID = Constants.WindowIDs.RELATIONS_OLD;

			setGUIProperty("Related.Status", "-");
		}


		public override int GetID
		{
			get { return Constants.WindowIDs.RELATIONS_OLD; }
			set { base.GetID = value; }
		}

		protected override void OnPageLoad()
		{
			base.OnPageLoad();

			BaseConfig.MyAnimeLog.Write("OnPageLoad: {0}", MainWindow.GlobalSeriesID.ToString());

			LoadData();

			ShowMainAnime();
			ShowSequel();
			ShowPrequel();
			ShowOtherRelations();

			//LoadFanart();
		}

		private void LoadData()
		{
			relations.Clear();

			mainAnime = null;
			serMain = null;

			sequelAnime = null;
			serSequel = null;

			prequelAnime = null;
			serPrequel = null;

			dummyMainHasFiles.Visible = false;

			serMain = JMMServerHelper.GetSeries(MainWindow.GlobalSeriesID);
			if (serMain != null)
				mainAnime = serMain.AniDB_Anime;

			if (mainAnime == null)
				return;

			if (serMain != null) dummyMainHasFiles.Visible = true;

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
			setGUIProperty("Related.Main.Episodes", mainAnime.EpisodeCountNormal.ToString() + " (" + mainAnime.EpisodeCountSpecial.ToString() + " Specials)");

			string imagePath = GUIGraphicsContext.Skin + @"\Media\MyAnime3\anime3_blankchar.png";
			if (serMain != null)
			{
				string posterName = ImageAllocator.GetSeriesImageAsFileName(serMain, GUIFacadeControl.Layout.Filmstrip);
				if (File.Exists(posterName))
					imagePath = posterName;
				imgMainShow.FileName = imagePath;
			}
		}

		private void ShowSequel()
		{
			AniDB_Anime_RelationVM sequel = null;
			foreach (AniDB_Anime_RelationVM ra in relations)
			{
				if (ra.IsSequel) sequel = ra;
			}

			dummySequelExists.Visible = false;
			dummySequelHasInfo.Visible = false;
			dummySequelHasFiles.Visible = false;

			if (sequel == null) // no sequel found
			{
				setGUIProperty("Related.Sequel.Title", "No Sequel");
				setGUIProperty("Related.Sequel.Year", ".");
				setGUIProperty("Related.Sequel.Episodes", ".");
			}
			else
			{
				dummySequelExists.Visible = true;

				setGUIProperty("Related.Sequel.Title", sequel.AnimeID.ToString());

				// look for the data locally
			    sequelAnime = sequel.AniDB_Anime;
				if (sequelAnime!=null)
				{
                    setGUIProperty("Related.Sequel.Title", sequelAnime.MainTitle);
					dummySequelHasInfo.Visible = true;

					// try and load the series
					serSequel = sequel.AnimeSeries;
					if (serSequel != null)
					{
						dummySequelHasFiles.Visible = true; // user has this series
						setGUIProperty("Related.Sequel.Title", serSequel.SeriesName);
					}
					else
						setGUIProperty("Related.Sequel.Title", sequelAnime.MainTitle);

					setGUIProperty("Related.Sequel.Year", sequelAnime.Year);
					setGUIProperty("Related.Sequel.Episodes", sequelAnime.EpisodeCountNormal.ToString() + " (" + sequelAnime.EpisodeCountSpecial.ToString() + " Specials)");

					if (serSequel != null)
					{
						string imagePath = GUIGraphicsContext.Skin + @"\Media\MyAnime3\anime3_blankchar.png";
						string posterName = ImageAllocator.GetSeriesImageAsFileName(serSequel, GUIFacadeControl.Layout.Filmstrip);
						if (File.Exists(posterName))
							imagePath = posterName;
						imgSequel.FileName = imagePath;
						
					}
					else
					{
						if (sequelAnime.DefaultPosterPath.Trim().Length > 0 && File.Exists(sequelAnime.DefaultPosterPath))
						{
							imgSequel.FileName = sequelAnime.DefaultPosterPath;
						}
					}
				}
			}
		}

		private void ShowPrequel()
		{
			AniDB_Anime_RelationVM prequel = null;
			foreach (AniDB_Anime_RelationVM ra in relations)
			{
				if (ra.IsPrequel) prequel = ra;
			}

			dummyPrequelExists.Visible = false;
			dummyPrequelHasInfo.Visible = false;
			dummyPrequelHasFiles.Visible = false;

			if (prequel == null) // no sequel found
			{
				setGUIProperty("Related.Prequel.Title", "No Prequel");
				setGUIProperty("Related.Prequel.Year", ".");
				setGUIProperty("Related.Prequel.Episodes", ".");
			}
			else
			{

				dummyPrequelExists.Visible = true;

				
				// look for the data locally
				prequelAnime = prequel.AniDB_Anime;
				if (prequelAnime!=null)
				{
                    setGUIProperty("Related.Prequel.Title", prequelAnime.MainTitle);

					dummyPrequelHasInfo.Visible = true;

					// try and load the series
					serPrequel = prequel.AnimeSeries;
					if (serPrequel != null)
					{
						dummyPrequelHasFiles.Visible = true; // user has this series
						setGUIProperty("Related.Prequel.Title", serPrequel.SeriesName);
					}
					else
						setGUIProperty("Related.Prequel.Title", prequelAnime.MainTitle);

					setGUIProperty("Related.Prequel.Year", prequelAnime.Year);
					setGUIProperty("Related.Prequel.Episodes", prequelAnime.EpisodeCountNormal.ToString() + " (" + prequelAnime.EpisodeCountSpecial.ToString() + " Specials)");

					if (serPrequel != null)
					{
						string imagePath = GUIGraphicsContext.Skin + @"\Media\MyAnime3\anime3_blankchar.png";
						string posterName = ImageAllocator.GetSeriesImageAsFileName(serPrequel, GUIFacadeControl.Layout.Filmstrip);
						if (File.Exists(posterName))
							imagePath = posterName;
						imgPrequel.FileName = imagePath;

					}
					else
					{
						if (prequelAnime.DefaultPosterPath.Trim().Length > 0 && File.Exists(prequelAnime.DefaultPosterPath))
						{
							imgPrequel.FileName = prequelAnime.DefaultPosterPath;
						}
					}
				}
			}
		}

		private void ShowOtherRelations()
		{
			GUIControl.ClearControl(this.GetID, m_Facade.GetID);

			dummyOtherExists.Visible = false;
			m_Facade.CurrentLayout = GUIFacadeControl.Layout.Filmstrip;

			string imagePathMissing = GUIGraphicsContext.Skin + @"\Media\MyAnime3\anime3_question_poster.png";
			string imagePathNoPicture = GUIGraphicsContext.Skin + @"\Media\MyAnime3\anime3_blankchar.png";

			foreach (AniDB_Anime_RelationVM ra in relations)
			{
				BaseConfig.MyAnimeLog.Write("ShowOtherRelations: {0}", ra);

				if (!ra.IsPrequel && !ra.IsSequel) // not the sequel or prequel
				{
					dummyOtherExists.Visible = true;
					string imagePath = "";

				    AniDB_AnimeVM anime = ra.AniDB_Anime;
					if (anime!=null)
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
			}

			if (m_Facade.Count > 0)
			{
				m_Facade.SelectedListItemIndex = 0;

				AniDB_Anime_RelationVM ra = m_Facade.SelectedListItem.TVTag as AniDB_Anime_RelationVM;
				if (ra != null)
				{
					SetOtherAnime(ra);
				}

				//GUIControl.FocusControl(GetID, 50);
			}
		}



		// triggered when a selection change was made on the facade
		private void onFacadeItemSelected(GUIListItem item, GUIControl parent)
		{
			// if this is not a message from the facade, exit
			if (parent != m_Facade && parent != m_Facade.FilmstripLayout)
				return;

			AniDB_Anime_RelationVM ra = item.TVTag as AniDB_Anime_RelationVM;
			if (ra != null)
			{
				SetOtherAnime(ra);
			}

		}

		private void SetOtherAnime(AniDB_Anime_RelationVM ra)
        {
		    AniDB_AnimeVM anime = ra.AniDB_Anime;
            dummyOtherHasFiles.Visible = false;
            dummyOtherHasInfo.Visible = false;

			setGUIProperty("Related.Other.Title", ra.DisplayName);
			setGUIProperty("Related.Other.Relationship", ra.RelationType);
			setGUIProperty("Related.Other.Episodes", "-");
			setGUIProperty("Related.Other.Year", "-");
            	
			if (anime!=null)
            {
    			dummyOtherHasInfo.Visible = true;

				setGUIProperty("Related.Other.Episodes", anime.EpisodeCountNormal.ToString() + " (" + anime.EpisodeCountSpecial.ToString() + " Specials)");
				setGUIProperty("Related.Other.Year", anime.Year);

				// try and load the series
				AnimeSeriesVM serAnime = ra.AnimeSeries;
				if (serAnime!=null)
				{
				    dummyOtherHasFiles.Visible = true;
                    setGUIProperty("Related.Other.Title", serAnime.SeriesName);
				}
			}
		}


		public override bool Init()
		{
			return Load(GUIGraphicsContext.Skin + @"\Anime3_Relations_old.xml");
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
				dlg.Add("Go To Episode List");
				dlg.Add("Search for Torrents");
				dlg.DoModal(GUIWindowManager.ActiveWindow);

				//TODO
				/*
				switch (dlg.SelectedId)
				{
					case 1:
						Utils.ShowEpisodesForAnime(mainAnime);
						break;

					case 2:
						DownloadHelper.SearchAnime(mainAnime);
						break;
				}*/
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write("Error in menu: {0}", ex);
			}
		}

		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			//BaseConfig.MyAnimeLog.Write("OnClicked: {0}", controlId.ToString());

			//TODO
			/*
			if (control == this.btnMain)
			{
				foreach (AniDB_RelatedAnime rel in relations)
				{
                    AniDB_Anime anime = rel.RelatedAnime;
                    if (anime == null)
                    {
                        MainWindow.anidbProcessor.UpdateAnimeInfoHTTP(rel.AnimeRelID, true, false);
						MainWindow.anidbProcessor.UpdateAnimeInfo(rel.AnimeRelID, true, false);
					}
                    else
                    {
                        // this means it is not a full record - downloaded from titles
                        if (anime.AnimeType==-1)
                        {
							MainWindow.anidbProcessor.UpdateAnimeInfoHTTP(rel.AnimeRelID, true, false);
							MainWindow.anidbProcessor.UpdateAnimeInfo(rel.AnimeRelID, true, false);
                        }
                    }
				}

				return;
			}*/

			if (control == this.btnSequel)
			{
				if (serSequel != null)
				{
					MainWindow.GlobalSeriesID = serSequel.AnimeSeriesID.Value;
					GUIWindowManager.CloseCurrentWindow();
					GUIWindowManager.ActivateWindow(Constants.WindowIDs.RELATIONS, false);
				}

				return;
			}

			if (control == this.btnPrequel)
			{
				if (serPrequel != null)
				{
					MainWindow.GlobalSeriesID = serPrequel.AnimeSeriesID.Value;
					GUIWindowManager.CloseCurrentWindow();
					GUIWindowManager.ActivateWindow(Constants.WindowIDs.RELATIONS, false);
				}

				return;
			}

			if (control == this.btnNavLeft)
			{
				GUIWindowManager.CloseCurrentWindow();
				GUIWindowManager.ActivateWindow(Constants.WindowIDs.ANIMEINFO, false);

				return;
			}

			if (control == this.btnNavRight)
			{
				GUIWindowManager.CloseCurrentWindow();
				GUIWindowManager.ActivateWindow(Constants.WindowIDs.CHARACTERS, false);

				return;
			}

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
			}

			base.OnClicked(controlId, control, actionType);
		}
	}

}
