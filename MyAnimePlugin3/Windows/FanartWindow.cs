using System;
using System.Collections.Generic;

using MediaPortal.GUI.Library;
using System.ComponentModel;

using MyAnimePlugin3.DataHelpers;
using Action = MediaPortal.GUI.Library.Action;
using MyAnimePlugin3.ViewModel;
using System.IO;

namespace MyAnimePlugin3.Windows
{
	public class FanartWindow : GUIWindow
	{
		[SkinControlAttribute(50)] protected GUIFacadeControl m_Facade = null;

		[SkinControlAttribute(2)] protected GUIButtonControl buttonLayouts = null;
		[SkinControlAttribute(11)] protected GUILabelControl labelResolution = null;
		[SkinControlAttribute(14)] protected GUILabelControl labelDisabled = null;
		[SkinControlAttribute(15)] protected GUILabelControl labelDefault = null;

		[SkinControlAttribute(140)] protected GUIButtonControl btnPosters = null;
		[SkinControlAttribute(141)] protected GUIButtonControl btnWideBanners = null;
        [SkinControlAttribute(1400)] protected GUILabelControl dummyFullscreen = null;

		[SkinControlAttribute(910)] protected GUIButtonControl btnAnimeInfo = null;
		[SkinControlAttribute(911)] protected GUIButtonControl btnAnimeCharacters = null;
		[SkinControlAttribute(912)] protected GUIButtonControl btnAnimeRelations = null;
		//[SkinControlAttribute(913)] protected GUIButtonControl btnAnimeFanart = null;
		[SkinControlAttribute(914)] protected GUIButtonControl btnAnimePosters = null;
		[SkinControlAttribute(915)] protected GUIButtonControl btnAnimeWideBanners = null;

        public enum GuiProperty
        {
            FanArt_PageTitle,
            FanArt_Source,
            FanArt_SelectedFanartResolution,
            FanArt_SelectedFanartIsDefault,
            FanArt_SelectedFanartIsDisabled,
            FanArt_SelectedPreview
        }

        public void SetGUIProperty(GuiProperty which, string value) { this.SetGUIProperty(which.ToString(), value); }
        public void ClearGUIProperty(GuiProperty which) { this.ClearGUIProperty(which.ToString()); }



        enum menuAction
		{
			use,
			download,
			delete,
			optionRandom,
			disable,
			enable,
			filters,
			reload,
            fullscreen
		}

		enum menuFilterAction
		{
			all,
			hd,
			fullhd
		}

		const int windowID = 6103;

		private GUIFacadeControl.Layout currentView = GUIFacadeControl.Layout.SmallIcons;
        private bool viewIsFullscreen = false;

		public static int GetWindowID
		{ get { return windowID; } }

		public override int GetID
		{ get { return windowID; } }

		public int GetWindowId()
		{ return windowID; }

		public override bool Init()
		{
            return this.InitSkin<GuiProperty>("Anime3_FanArt.xml");
		}

		private int AnimeID = -1;


		protected GUIFacadeControl.Layout CurrentView
		{
			get { return currentView; }
			set { currentView = value; }
		}
		

		protected override void OnPageLoad()
		{
            viewIsFullscreen = false;
			//MainWindow.setGUIProperty("FanArt.SelectedPreview", "");


            CurrentView = BaseConfig.Settings.LastFanartViewMode;

			BaseConfig.MyAnimeLog.Write("OnPageLoad:FanartWindow seriesid : {0} -  CurrentView: {1}", MainWindow.GlobalSeriesID.ToString(), CurrentView);


			if (m_Facade != null)
			{
				m_Facade.CurrentLayout = CurrentView;
			}

			base.OnPageLoad();

			// update skin controls
			UpdateLayoutButton();

			ClearProperties();


			BaseConfig.MyAnimeLog.Write("Fanart Chooser Window initializing");

			ShowFanart();
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
				case GUIFacadeControl.Layout.CoverFlow:
                    strLine = GUILocalizeStrings.Get(791);
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
            ClearGUIProperty(GuiProperty.FanArt_Source);
            ClearGUIProperty(GuiProperty.FanArt_SelectedFanartResolution);
            ClearGUIProperty(GuiProperty.FanArt_SelectedFanartIsDefault);
            ClearGUIProperty(GuiProperty.FanArt_SelectedFanartIsDisabled);
        }

		private void ShowFanart()
		{
			GUIControl.ClearControl(this.GetID, m_Facade.GetID);
			List<FanartContainer> allFanart = new List<FanartContainer>();

			AnimeSeriesVM ser = JMMServerHelper.GetSeries(MainWindow.GlobalSeriesID);
			if (ser != null)
			{
				if (ser.CrossRef_AniDB_TvDBV2 != null && ser.CrossRef_AniDB_TvDBV2.Count > 0)
					AnimeID = ser.CrossRef_AniDB_TvDBV2[0].AnimeID;
			}

			if (ser == null) return;

			BaseConfig.MyAnimeLog.Write("ShowFanart for {0}", AnimeID);
			GUIListItem item = null;
			foreach (FanartContainer fanart in ser.AniDB_Anime.AniDB_AnimeCrossRefs.AllFanarts)
			{
				if (!File.Exists(fanart.FullImagePath)) continue;

				item = new GUIListItem();
				item.IconImage = item.IconImageBig = fanart.FullThumbnailPath;
				item.TVTag = fanart;
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

				FanartContainer selectedFanart = m_Facade.SelectedListItem.TVTag as FanartContainer;
				if (selectedFanart != null)
				{
					setFanartPreviewBackground(selectedFanart);
				}

				GUIControl.FocusControl(GetID, 50);
			}
		}

		public void setPageTitle(string Title)
		{
            SetGUIProperty(GuiProperty.FanArt_PageTitle, Title);
        }

		protected override void OnShowContextMenu()
		{
			try
			{

                GUIListItem currentitem = m_Facade.SelectedListItem;
                if (currentitem == null || !(currentitem.TVTag is FanartContainer)) return;
                FanartContainer selectedFanart = currentitem.TVTag as FanartContainer;


                ContextMenu cmenu = new ContextMenu(Translation.Fanart);
                cmenu.AddAction(selectedFanart.IsImageEnabled ? Translation.Disable : Translation.Enable, () =>
                {
                    bool endis = !selectedFanart.IsImageEnabled;
                    JMMServerHelper.EnableDisableFanart(endis, selectedFanart, AnimeID);
                    ShowFanart();

                });
                if (selectedFanart.IsImageEnabled)
                {
                    cmenu.AddAction(selectedFanart.IsImageDefault ? Translation.RemoveAsDefault : Translation.SetAsDefault, () =>
                    {
                        bool isdef = !selectedFanart.IsImageDefault;
                        JMMServerHelper.SetDefaultFanart(isdef, selectedFanart, AnimeID);
                        ShowFanart();
                    });
                }
                cmenu.Show();
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write("Exception in Fanart Chooser Context Menu: " + ex.Message + ex.StackTrace);
			}
		}

		

		// triggered when a selection change was made on the facade
		private void onFacadeItemSelected(GUIListItem item, GUIControl parent)
		{
			

			// if this is not a message from the facade, exit
			if (parent != m_Facade && parent != m_Facade.FilmstripLayout && parent != m_Facade.CoverFlowLayout &&
				parent != m_Facade.ThumbnailLayout && parent != m_Facade.ListLayout)
				return;

			setFanartPreviewBackground(item.TVTag as FanartContainer);

		}

		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
            MainMenu menu = new MainMenu();
            menu.Add(btnWideBanners, () =>
            {
                GUIWindowManager.CloseCurrentWindow();
                GUIWindowManager.ActivateWindow(Constants.WindowIDs.WIDEBANNERS, false);
            });
            menu.Add(btnPosters, () =>
            {
                GUIWindowManager.CloseCurrentWindow();
                GUIWindowManager.ActivateWindow(Constants.WindowIDs.POSTERS, false);
            });
            menu.Add(buttonLayouts, () =>
            {
                bool shouldContinue;
                do
                {
                    shouldContinue = false;
                    switch (CurrentView)
                    {
                        case GUIFacadeControl.Layout.List:
                            CurrentView = GUIFacadeControl.Layout.Playlist;
                            if (!AllowView(CurrentView) || m_Facade.PlayListLayout == null)
                            {
                                shouldContinue = true;
                            }
                            else
                            {
                                m_Facade.CurrentLayout = GUIFacadeControl.Layout.Playlist;
                                BaseConfig.Settings.LastFanartViewMode = GUIFacadeControl.Layout.Playlist;
                                BaseConfig.Settings.Save();
                            }
                            break;

                        case GUIFacadeControl.Layout.Playlist:
                            CurrentView = GUIFacadeControl.Layout.SmallIcons;
                            if (!AllowView(CurrentView) || m_Facade.ThumbnailLayout == null)
                            {
                                shouldContinue = true;
                            }
                            else
                            {
                                m_Facade.CurrentLayout = GUIFacadeControl.Layout.SmallIcons;
                                BaseConfig.Settings.LastFanartViewMode = GUIFacadeControl.Layout.SmallIcons;
                                BaseConfig.Settings.Save();
                            }
                            break;

                        case GUIFacadeControl.Layout.SmallIcons:
                            CurrentView = GUIFacadeControl.Layout.LargeIcons;
                            if (!AllowView(CurrentView) || m_Facade.ThumbnailLayout == null)
                            {
                                shouldContinue = true;
                            }
                            else
                            {
                                m_Facade.CurrentLayout = GUIFacadeControl.Layout.LargeIcons;
                                BaseConfig.Settings.LastFanartViewMode = GUIFacadeControl.Layout.LargeIcons;
                                BaseConfig.Settings.Save();
                            }
                            break;

                        case GUIFacadeControl.Layout.LargeIcons:
                            CurrentView = GUIFacadeControl.Layout.Filmstrip;
                            if (!AllowView(CurrentView) || m_Facade.FilmstripLayout == null)
                            {
                                shouldContinue = true;
                            }
                            else
                            {
                                m_Facade.CurrentLayout = GUIFacadeControl.Layout.Filmstrip;
                                BaseConfig.Settings.LastFanartViewMode = GUIFacadeControl.Layout.Filmstrip;
                                BaseConfig.Settings.Save();
                            }
                            break;

                        case GUIFacadeControl.Layout.Filmstrip:
                            CurrentView = GUIFacadeControl.Layout.List;
                            if (!AllowView(CurrentView) || m_Facade.ListLayout == null)
                            {
                                shouldContinue = true;
                            }
                            else
                            {
                                m_Facade.CurrentLayout = GUIFacadeControl.Layout.List;
                                BaseConfig.Settings.LastFanartViewMode = GUIFacadeControl.Layout.List;
                                BaseConfig.Settings.Save();
                            }
                            break;
                    }
                } while (shouldContinue);
                UpdateLayoutButton();
                GUIControl.FocusControl(GetID, controlId);
            });
            if (menu.Check(control))
                return;
            base.OnClicked(controlId, control, actionType);
        }



        void setFanartPreviewBackground(FanartContainer fanart)
        {
            if (fanart == null)
            {
                ClearGUIProperty(GuiProperty.FanArt_SelectedFanartResolution);
                ClearGUIProperty(GuiProperty.FanArt_SelectedPreview);
                ClearGUIProperty(GuiProperty.FanArt_SelectedFanartIsDisabled);
                ClearGUIProperty(GuiProperty.FanArt_SelectedFanartIsDefault);
                ClearGUIProperty(GuiProperty.FanArt_Source);
                return;
            }

            if (fanart.ImageType == ImageEntityType.TvDB_FanArt)
            {
                TvDB_ImageFanartVM fanartTvDb = fanart.FanartObject as TvDB_ImageFanartVM;
                if (fanartTvDb != null)
                    SetGUIProperty(GuiProperty.FanArt_SelectedFanartResolution, fanartTvDb.BannerType2);
                else
                    ClearGUIProperty(GuiProperty.FanArt_SelectedFanartResolution);
            }
            else
                ClearGUIProperty(GuiProperty.FanArt_SelectedFanartResolution);

            SetGUIProperty(GuiProperty.FanArt_SelectedFanartIsDisabled, fanart.IsImageEnabled ? Translation.No : Translation.Yes);
            SetGUIProperty(GuiProperty.FanArt_SelectedFanartIsDefault, fanart.IsImageDefault ? Translation.Yes : Translation.No);
            SetGUIProperty(GuiProperty.FanArt_Source, fanart.FanartSource);

            string preview;

            if (File.Exists(fanart.FullImagePath))
            {
                // Ensure Fanart on Disk is valid as well
                ImageAllocator.LoadImageFastFromFile(fanart.FullImagePath);
                // Should be safe to assign fullsize fanart if available
                preview = ImageAllocator.GetOtherImage(fanart.FullImagePath, default(System.Drawing.Size), false);
            }
            else
                preview = m_Facade.SelectedListItem.IconImageBig;

            SetGUIProperty(GuiProperty.FanArt_SelectedPreview, preview);
        }
    }
}
