using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.GUI.Library;
using MyAnimePlugin3.DataHelpers;
using Action = MediaPortal.GUI.Library.Action;
using MyAnimePlugin3.ViewModel;
using System.IO;
namespace MyAnimePlugin3.Windows
{
	public class PosterWindow : GUIWindow
	{
		[SkinControlAttribute(50)]
		protected GUIFacadeControl m_Facade = null;

		[SkinControlAttribute(2)] protected GUIButtonControl buttonLayouts = null;
		[SkinControlAttribute(11)] protected GUILabelControl labelPosterSource = null;
		[SkinControlAttribute(14)] protected GUILabelControl labelDisabled = null;
		[SkinControlAttribute(15)] protected GUILabelControl labelDefault = null;

		[SkinControlAttribute(140)] protected GUIButtonControl btnFanart = null;
		[SkinControlAttribute(141)] protected GUIButtonControl btnWideBanners = null;
        [SkinControlAttribute(1400)] protected GUILabelControl dummyLarger = null;

		[SkinControlAttribute(910)] protected GUIButtonControl btnAnimeInfo = null;
		[SkinControlAttribute(911)] protected GUIButtonControl btnAnimeCharacters = null;
		[SkinControlAttribute(912)] protected GUIButtonControl btnAnimeRelations = null;
		[SkinControlAttribute(913)] protected GUIButtonControl btnAnimeFanart = null;
		//[SkinControlAttribute(914)] protected GUIButtonControl btnAnimePosters = null;
		[SkinControlAttribute(915)] protected GUIButtonControl btnAnimeWideBanners = null;

		private int AnimeID = -1;
        private bool viewIsLarger = false;

		private GUIFacadeControl.Layout currentView = GUIFacadeControl.Layout.Filmstrip;

		public static int GetWindowID
		{ get { return Constants.WindowIDs.POSTERS; } }

		public override int GetID
		{ get { return Constants.WindowIDs.POSTERS; } }

		public int GetWindowId()
		{ return Constants.WindowIDs.POSTERS; }

		public override bool Init()
		{
			String xmlSkin = GUIGraphicsContext.Skin + @"\Anime3_Posters.xml";
			return Load(xmlSkin);
		}

		public void setPageTitle(string Title)
		{
			MainWindow.setGUIProperty("Posters.PageTitle", Title);
		}


		protected GUIFacadeControl.Layout CurrentView
		{
			get { return currentView; }
			set { currentView = value; }
		}

		protected override void OnPageLoad()
		{
			//MainWindow.setGUIProperty("FanArt.SelectedPreview", "");

            CurrentView = BaseConfig.Settings.LastPosterViewMode;

			BaseConfig.MyAnimeLog.Write("OnPageLoad seriesid : {0}", MainWindow.GlobalSeriesID.ToString());

			if (m_Facade != null)
			{
				m_Facade.CurrentLayout = CurrentView;
			}

			base.OnPageLoad();

			// update skin controls
			UpdateLayoutButton();
			if (labelPosterSource != null) labelPosterSource.Label = "Source:";
			if (labelDefault != null) labelDefault.Label = "Default: ";
			if (labelDisabled != null) labelDisabled.Label = "Disabled";

			ClearProperties();
			ShowPosters();
		}

		private void ShowPosters()
		{
			GUIControl.ClearControl(this.GetID, m_Facade.GetID);
			List<PosterContainer> allPosters = new List<PosterContainer>();

			string displayname = "";

			AnimeSeriesVM ser = JMMServerHelper.GetSeries(MainWindow.GlobalSeriesID);
			if (ser != null)
			{
				displayname = ser.SeriesName;
				if (ser.CrossRef_AniDB_TvDBV2 != null && ser.CrossRef_AniDB_TvDBV2.Count > 0)
					AnimeID = ser.CrossRef_AniDB_TvDBV2[0].AnimeID;
			}

			BaseConfig.MyAnimeLog.Write("ShowPosters for {0} - {1}", displayname, AnimeID);

			foreach (PosterContainer pstr in ser.AniDB_Anime.AniDB_AnimeCrossRefs.AllPosters)
			{
				if (!File.Exists(pstr.FullImagePath)) continue;

				allPosters.Add(pstr);
			}

			GUIListItem item = null;
			foreach (PosterContainer poster in allPosters)
			{
				item = new GUIListItem();
				item.IconImage = item.IconImageBig = poster.FullImagePath;
				item.TVTag = poster;
				item.OnItemSelected += new GUIListItem.ItemSelectedHandler(onFacadeItemSelected);
				m_Facade.Add(item);
			}

			if (m_Facade.Count > 0)
			{
				m_Facade.SelectedListItemIndex = 0;

				// Work around for Filmstrip not allowing to programmatically select item
				if (m_Facade.CurrentLayout == GUIFacadeControl.Layout.Filmstrip)
				{
					
				}

				PosterContainer selectedPoster = m_Facade.SelectedListItem.TVTag as PosterContainer;
				if (selectedPoster != null)
				{
					SetPosterProperties(selectedPoster);
				}

				GUIControl.FocusControl(GetID, 50);
			}
		}

		protected bool AllowView(GUIFacadeControl.Layout view)
		{
			if (view == GUIFacadeControl.Layout.List)
				return false;

			if (view == GUIFacadeControl.Layout.AlbumView)
				return false;

			if (view == GUIFacadeControl.Layout.Playlist)
				return false;

			return true;
		}

        public override void OnAction(MediaPortal.GUI.Library.Action action)
        {
            if (viewIsLarger)
            {
                if (action.m_key.KeyCode == 27)
                {
                    // makes esc exit fullscreen
                    //hide fullscreen
                    dummyLarger.Visible = true;
                    viewIsLarger = false;
                    return;
                }
                if (action.IsUserAction())
                {
                    //this stops the selection wondering around the screen
                    return;
                }
            }

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

		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			if (control == btnWideBanners)
			{
				GUIWindowManager.CloseCurrentWindow();
				GUIWindowManager.ActivateWindow(Constants.WindowIDs.WIDEBANNERS, false);
			}

			if (control == btnFanart)
			{
				GUIWindowManager.CloseCurrentWindow();
				GUIWindowManager.ActivateWindow(Constants.WindowIDs.FANART, false);
			}

			if (control == buttonLayouts)
			{
				switch (CurrentView)
				{
					case GUIFacadeControl.Layout.LargeIcons:
						m_Facade.CurrentLayout = GUIFacadeControl.Layout.Filmstrip;
						CurrentView = GUIFacadeControl.Layout.Filmstrip;
                        BaseConfig.Settings.LastPosterViewMode = GUIFacadeControl.Layout.Filmstrip;
                        BaseConfig.Settings.Save();
						break;

					case GUIFacadeControl.Layout.Filmstrip:
						m_Facade.CurrentLayout = GUIFacadeControl.Layout.LargeIcons;
						CurrentView = GUIFacadeControl.Layout.LargeIcons;
                        BaseConfig.Settings.LastPosterViewMode = GUIFacadeControl.Layout.LargeIcons;
                        BaseConfig.Settings.Save();
						break;
				}

				UpdateLayoutButton();
				GUIControl.FocusControl(GetID, controlId);
			}

			if (MA3WindowManager.HandleWindowChangeButton(control))
				return;
		}

		private void UpdateLayoutButton()
		{
			string strLine = string.Empty;
			GUIFacadeControl.Layout view = CurrentView;
			switch (view)
			{
				case GUIFacadeControl.Layout.List:
					strLine = GUILocalizeStrings.Get(101);
					break;
				case GUIFacadeControl.Layout.SmallIcons:
					strLine = GUILocalizeStrings.Get(100);
					break;
				case GUIFacadeControl.Layout.LargeIcons:
					strLine = GUILocalizeStrings.Get(417);
					break;
				case GUIFacadeControl.Layout.Filmstrip:
					strLine = GUILocalizeStrings.Get(733);
					break;
				case GUIFacadeControl.Layout.Playlist:
					strLine = GUILocalizeStrings.Get(101);
					break;
			}
			if (buttonLayouts != null)
				GUIControl.SetControlLabel(GetID, buttonLayouts.GetID, strLine);
		}

		private void ClearProperties()
		{
			MainWindow.setGUIProperty("Posters.Count", " ");
			MainWindow.setGUIProperty("Posters.SelectedSource", " ");
			MainWindow.setGUIProperty("Posters.SelectedPosterIsDefault", " ");
			MainWindow.setGUIProperty("Posters.SelectedPosterIsDisabled", " ");
		}

		// triggered when a selection change was made on the facade
		private void onFacadeItemSelected(GUIListItem item, GUIControl parent)
		{
			// if this is not a message from the facade, exit
			if (parent != m_Facade && parent != m_Facade.FilmstripLayout &&
				parent != m_Facade.ThumbnailLayout)
				return;

			PosterContainer selectedPoster = item.TVTag as PosterContainer;
			if (selectedPoster != null)
			{
                //set large image
                MainWindow.setGUIProperty("Posters.PosterPath", selectedPoster.FullImagePath);

				SetPosterProperties(selectedPoster);
			}

		}

		private void SetPosterProperties(PosterContainer poster)
		{
			string isDefault = "No";
			if (poster.IsImageDefault) isDefault = "Yes";

			string isDisabled = "No";
			if (!poster.IsImageEnabled) isDisabled = "Yes";


			MainWindow.setGUIProperty("Posters.SelectedSource", poster.PosterSource);
			MainWindow.setGUIProperty("Posters.SelectedPosterIsDefault", isDefault);
			MainWindow.setGUIProperty("Posters.SelectedPosterIsDisabled", isDisabled);
		}

		protected override void OnShowContextMenu()
		{
			try
			{
				GUIListItem currentitem = this.m_Facade.SelectedListItem;
				if (currentitem == null || !(currentitem.TVTag is PosterContainer)) return;
				PosterContainer selectedPoster = currentitem.TVTag as PosterContainer;

				IDialogbox dlg = (IDialogbox)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
				if (dlg == null) return;
				dlg.Reset();
				dlg.SetHeading("Poster");
	

				GUIListItem pItem;


				if (!selectedPoster.IsImageEnabled)
				{
					pItem = new GUIListItem("Enable");
					dlg.Add(pItem);
				}
				else
				{
					pItem = new GUIListItem("Disable");
					dlg.Add(pItem);
				}

				if (selectedPoster.IsImageEnabled)
				{
					if (selectedPoster.IsImageDefault)
					{
						pItem = new GUIListItem("Remove as Default");
						dlg.Add(pItem);
					}
					else
					{
						pItem = new GUIListItem("Set as Default");
						dlg.Add(pItem);
					}
				}

			
				// lets show it
				dlg.DoModal(GUIWindowManager.ActiveWindow);

				if (dlg.SelectedId == 1) // enabled/disable
				{
					bool endis = !selectedPoster.IsImageEnabled;
					JMMServerHelper.EnableDisablePoster(endis, selectedPoster, AnimeID);

					ShowPosters();
					return;
				}

				if (dlg.SelectedId == 2)
				{
					bool isdef = !selectedPoster.IsImageDefault;
					JMMServerHelper.SetDefaultPoster(isdef, selectedPoster, AnimeID);

					ShowPosters();
					return;
				}
				
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write("Exception in Poster Chooser Context Menu: " + ex.Message + ex.StackTrace);
				return;
			}
		}
	}

}
