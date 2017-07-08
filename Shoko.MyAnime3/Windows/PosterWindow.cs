using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.GUI.Library;
using Shoko.MyAnime3.DataHelpers;
using Action = MediaPortal.GUI.Library.Action;
using System.IO;
using Shoko.MyAnime3.ViewModel;
using Shoko.MyAnime3.ViewModel.Helpers;
using Shoko.MyAnime3.ViewModel.Server;

namespace Shoko.MyAnime3.Windows
{
	public class PosterWindow : GUIWindow
	{
		[SkinControl(50)]
		protected GUIFacadeControl m_Facade = null;

		[SkinControl(2)] protected GUIButtonControl buttonLayouts = null;
		[SkinControl(11)] protected GUILabelControl labelPosterSource = null;
		[SkinControl(14)] protected GUILabelControl labelDisabled = null;
		[SkinControl(15)] protected GUILabelControl labelDefault = null;

		[SkinControl(140)] protected GUIButtonControl btnFanart = null;
		[SkinControl(141)] protected GUIButtonControl btnWideBanners = null;
        [SkinControl(1400)] protected GUILabelControl dummyLarger = null;

		[SkinControl(910)] protected GUIButtonControl btnAnimeInfo = null;
		[SkinControl(911)] protected GUIButtonControl btnAnimeCharacters = null;
		[SkinControl(912)] protected GUIButtonControl btnAnimeRelations = null;
		[SkinControl(913)] protected GUIButtonControl btnAnimeFanart = null;
		//[SkinControlAttribute(914)] protected GUIButtonControl btnAnimePosters = null;
		[SkinControl(915)] protected GUIButtonControl btnAnimeWideBanners = null;

        public enum GuiProperty
        {
            Posters_PageTitle,
            Posters_Count,
            Posters_SelectedSource,
            Posters_SelectedPosterIsDefault,
            Posters_SelectedPosterIsDisabled,
            Posters_PosterPath

        }

        public void SetGUIProperty(GuiProperty which, string value) { this.SetGUIProperty(which.ToString(), value); }
        public void ClearGUIProperty(GuiProperty which) { this.ClearGUIProperty(which.ToString()); }


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
            return this.InitSkin<GuiProperty>("Anime3_Posters.xml");
        }

		public void setPageTitle(string Title)  
		{
            SetGUIProperty(GuiProperty.Posters_PageTitle, Title);
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

			VM_AnimeSeries_User ser = ShokoServerHelper.GetSeries(MainWindow.GlobalSeriesID);
			if (ser != null)
			{
				displayname = ser.SeriesName;
				if (ser.CrossRefAniDBTvDBV2 != null && ser.CrossRefAniDBTvDBV2.Count > 0)
					AnimeID = ser.CrossRefAniDBTvDBV2[0].AnimeID;
			}
            else
                return;
            BaseConfig.MyAnimeLog.Write("ShowPosters for {0} - {1}", displayname, AnimeID);

			foreach (PosterContainer pstr in ser.Anime.AniDB_AnimeCrossRefs.AllPosters)
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

        protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
        {
            MainMenu menu = new MainMenu();
            menu.Add(btnWideBanners, () =>
            {
                GUIWindowManager.CloseCurrentWindow();
                GUIWindowManager.ActivateWindow(Constants.WindowIDs.WIDEBANNERS, false);
            });
            menu.Add(btnFanart, () =>
            {
                GUIWindowManager.CloseCurrentWindow();
                GUIWindowManager.ActivateWindow(Constants.WindowIDs.FANART, false);
            });
            menu.Add(buttonLayouts, () =>
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
            });
            menu.Check(control);
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
            ClearGUIProperty(GuiProperty.Posters_Count);
            ClearGUIProperty(GuiProperty.Posters_SelectedSource);
            ClearGUIProperty(GuiProperty.Posters_SelectedPosterIsDefault);
            ClearGUIProperty(GuiProperty.Posters_SelectedPosterIsDisabled);
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
                SetGUIProperty(GuiProperty.Posters_PosterPath, selectedPoster.FullImagePath);

				SetPosterProperties(selectedPoster);
			}
            else
                ClearGUIProperty(GuiProperty.Posters_PosterPath);
        }

		private void SetPosterProperties(PosterContainer poster)
		{

            string isDefault = Translation.No;
            if (poster.IsImageDefault) isDefault = Translation.Yes;
            string isDisabled = Translation.No;
            if (!poster.IsImageEnabled) isDisabled = Translation.Yes;

            SetGUIProperty(GuiProperty.Posters_SelectedSource, poster.PosterSource);
            SetGUIProperty(GuiProperty.Posters_SelectedPosterIsDefault, isDefault);
            SetGUIProperty(GuiProperty.Posters_SelectedPosterIsDisabled, isDisabled);
        }

        protected override void OnShowContextMenu()
        {
            try
            {
                GUIListItem currentitem = m_Facade.SelectedListItem;
                if (currentitem == null || !(currentitem.TVTag is PosterContainer)) return;
                PosterContainer selectedPoster = currentitem.TVTag as PosterContainer;

                ContextMenu cmenu = new ContextMenu(Translation.Poster);
                cmenu.AddAction(selectedPoster.IsImageEnabled ? Translation.Disable : Translation.Enable, () =>
                {
                    bool endis = !selectedPoster.IsImageEnabled;
                    ShokoServerHelper.EnableDisablePoster(endis, selectedPoster, AnimeID);
                    ShowPosters();
                });
                if (selectedPoster.IsImageEnabled)
                {
                    cmenu.AddAction(selectedPoster.IsImageDefault ? Translation.RemoveAsDefault : Translation.SetAsDefault, () =>
                    {
                        bool isdef = !selectedPoster.IsImageDefault;
                        ShokoServerHelper.SetDefaultPoster(isdef, selectedPoster, AnimeID);
                        ShowPosters();
                    });
                }
                cmenu.Show();
            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write("Exception in Poster Chooser Context Menu: " + ex.Message + ex.StackTrace);
            }
        }
    }

}
