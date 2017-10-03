using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.GUI.Library;
using Shoko.Commons.Extensions;
using Shoko.MyAnime3.DataHelpers;
using Action = MediaPortal.GUI.Library.Action;
using Shoko.MyAnime3.ViewModel;
using Shoko.MyAnime3.ViewModel.Helpers;
using Shoko.MyAnime3.ViewModel.Server;

namespace Shoko.MyAnime3.Windows
{
	public class WideBannerWindow : GUIWindow
	{
		[SkinControl(50)] protected GUIFacadeControl m_Facade = null;

		[SkinControl(11)] protected GUILabelControl labelWideBannerSource = null;
		[SkinControl(14)] protected GUILabelControl labelDisabled = null;
		[SkinControl(15)] protected GUILabelControl labelDefault = null;

		[SkinControl(140)] protected GUIButtonControl btnFanart = null;
		[SkinControl(141)] protected GUIButtonControl btnPosters = null;

		[SkinControl(910)] protected GUIButtonControl btnAnimeInfo = null;
		[SkinControl(911)] protected GUIButtonControl btnAnimeCharacters = null;
		[SkinControl(912)] protected GUIButtonControl btnAnimeRelations = null;
		[SkinControl(913)] protected GUIButtonControl btnAnimeFanart = null;
		[SkinControl(914)] protected GUIButtonControl btnAnimePosters = null;
        //[SkinControlAttribute(915)] protected GUIButtonControl btnAnimeWideBanners = null;


        public enum GuiProperty
        {
            WideBanners_PageTitle,
            WideBanners_Count,
            WideBanners_SelectedSource,
            WideBanners_SelectedBannerIsDefault,
            WideBanners_SelectedBannerIsDisabled,

        }

        public void SetGUIProperty(GuiProperty which, string value) { this.SetGUIProperty(which.ToString(), value); }
        public void ClearGUIProperty(GuiProperty which) { this.ClearGUIProperty(which.ToString()); }



        private int AnimeID = -1;

		private GUIFacadeControl.Layout currentView = GUIFacadeControl.Layout.LargeIcons;

		public static int GetWindowID
		{ get { return Constants.WindowIDs.WIDEBANNERS; } }

		public override int GetID
		{ get { return Constants.WindowIDs.WIDEBANNERS; } }

		public int GetWindowId()
		{ return Constants.WindowIDs.WIDEBANNERS; }

		public override bool Init()
		{
            return this.InitSkin<GuiProperty>("Anime3_WideBanners.xml");
        }

		public void setPageTitle(string Title)
		{
            SetGUIProperty(GuiProperty.WideBanners_PageTitle, Title);
        }

		protected GUIFacadeControl.Layout CurrentView
		{
			get { return currentView; }
			set { currentView = value; }
		}

		protected override void OnPageLoad()
		{
			//MainWindow.setGUIProperty("FanArt.SelectedPreview", "");


			BaseConfig.MyAnimeLog.Write("OnPageLoad seriesid : {0}", MainWindow.GlobalSeriesID.ToString());

			if (m_Facade != null)
			{
				m_Facade.CurrentLayout = GUIFacadeControl.Layout.LargeIcons;
				currentView = GUIFacadeControl.Layout.Filmstrip;
			}

			base.OnPageLoad();

			// update skin controls
			UpdateLayoutButton();
            if (labelWideBannerSource != null) labelWideBannerSource.Label = Translation.Source + ": ";
            if (labelDefault != null) labelDefault.Label = Translation.Default + ": ";
            if (labelDisabled != null) labelDisabled.Label = Translation.Disabled;

            ClearProperties();
			ShowWideBanners();
		}

		private void ShowWideBanners()
		{
			GUIControl.ClearControl(this.GetID, m_Facade.GetID);

			VM_AnimeSeries_User ser = ShokoServerHelper.GetSeries(MainWindow.GlobalSeriesID);
			if (ser != null)
			{
				if (ser.CrossRefAniDBTvDBV2 != null && ser.CrossRefAniDBTvDBV2.Count > 0) 
					AnimeID = ser.CrossRefAniDBTvDBV2[0].AnimeID;
			}

			if (ser != null)
			{
				List<VM_TvDB_ImageWideBanner> tvDBWideBanners = ser.Anime.AniDB_AnimeCrossRefs.TvDBImageWideBanners?.CastList<VM_TvDB_ImageWideBanner>() ?? new List<VM_TvDB_ImageWideBanner>();

				GUIListItem item = null;
				foreach (VM_TvDB_ImageWideBanner banner in tvDBWideBanners)
				{
					item = new GUIListItem();
					item.IconImage = item.IconImageBig = banner.FullImagePath;
					item.TVTag = banner;
					item.OnItemSelected += new GUIListItem.ItemSelectedHandler(onFacadeItemSelected);
					m_Facade.Add(item);
				}


				if (m_Facade.Count > 0)
				{
					m_Facade.SelectedListItemIndex = 0;
					VM_TvDB_ImageWideBanner selectedBanner = m_Facade.SelectedListItem.TVTag as VM_TvDB_ImageWideBanner;
					if (selectedBanner != null)
					{
						SetWideBannerProperties(selectedBanner);
					}

					GUIControl.FocusControl(GetID, 50);
				}
			}
		}

		protected bool AllowView(GUIFacadeControl.Layout view)
		{
			if (view == GUIFacadeControl.Layout.List)
				return false;

			if (view == GUIFacadeControl.Layout.AlbumView)
				return false;

			if (view == GUIFacadeControl.Layout.Filmstrip)
				return false;

			if (view == GUIFacadeControl.Layout.Playlist)
				return false;

			return true;
		}

        protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
        {
            MainMenu menu = new MainMenu();
            menu.Add(btnPosters, () =>
            {
                GUIWindowManager.CloseCurrentWindow();
                GUIWindowManager.ActivateWindow(Constants.WindowIDs.POSTERS, false);
            });
            menu.Add(btnFanart, () =>
            {
                GUIWindowManager.CloseCurrentWindow();
                GUIWindowManager.ActivateWindow(Constants.WindowIDs.FANART, false);
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

		}

        private void ClearProperties()
        {
            ClearGUIProperty(GuiProperty.WideBanners_Count);
            ClearGUIProperty(GuiProperty.WideBanners_SelectedSource);
            ClearGUIProperty(GuiProperty.WideBanners_SelectedBannerIsDefault);
            ClearGUIProperty(GuiProperty.WideBanners_SelectedBannerIsDisabled);
        }

        // triggered when a selection change was made on the facade
        private void onFacadeItemSelected(GUIListItem item, GUIControl parent)
		{
			// if this is not a message from the facade, exit
			if (parent != m_Facade && parent != m_Facade.ThumbnailLayout)
				return;

			VM_TvDB_ImageWideBanner banner = item.TVTag as VM_TvDB_ImageWideBanner;
			if (banner != null)
			{
				SetWideBannerProperties(banner);
			}

		}

		private void SetWideBannerProperties(VM_TvDB_ImageWideBanner banner)
		{
            string source = Translation.TheTVDB;

            string isDisabled = banner.Enabled == 0 ? Translation.Yes : Translation.No;
            string isDefault = (banner.IsImageDefault) ? Translation.Yes : Translation.No;

            SetGUIProperty(GuiProperty.WideBanners_SelectedSource, source);
            SetGUIProperty(GuiProperty.WideBanners_SelectedBannerIsDefault, isDefault);
            SetGUIProperty(GuiProperty.WideBanners_SelectedBannerIsDisabled, isDisabled);
        }

		protected override void OnShowContextMenu()
		{
			try
			{
                GUIListItem currentitem = m_Facade.SelectedListItem;
                if (currentitem == null || !(currentitem.TVTag is VM_TvDB_ImageWideBanner)) return;
                VM_TvDB_ImageWideBanner selectedBanner = currentitem.TVTag as VM_TvDB_ImageWideBanner;
                bool isDisabled = selectedBanner.Enabled == 0;

                ContextMenu cmenu = new ContextMenu(Translation.WideBanner);
                cmenu.AddAction(isDisabled ? Translation.Enable : Translation.Disable, () =>
                {
                    ShokoServerHelper.EnableDisableWideBanner(isDisabled, selectedBanner, AnimeID);
                    ShowWideBanners();
                });
                if (!isDisabled)
                {
                    cmenu.AddAction(selectedBanner.IsImageDefault ? Translation.RemoveAsDefault : Translation.SetAsDefault, () =>
                    {
                        ShokoServerHelper.SetDefaultWideBanner(!selectedBanner.IsImageDefault, selectedBanner, AnimeID);
                        ShowWideBanners();
                    });
                }
                cmenu.Show();

            }
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write("Exception in Wide Banner Chooser Context Menu: " + ex.Message);
				return;
			}
		}
	}
}
