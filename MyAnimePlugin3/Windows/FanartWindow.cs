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
			String xmlSkin = GUIGraphicsContext.Skin + @"\Anime3_FanArt.xml";
			return Load(xmlSkin);
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
					strLine = "CoverFlow";
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
			MainWindow.setGUIProperty("FanArt.Source", " ");
			MainWindow.setGUIProperty("FanArt.SelectedFanartResolution", " ");
			MainWindow.setGUIProperty("FanArt.SelectedFanartIsDefault", " ");
			MainWindow.setGUIProperty("FanArt.SelectedFanartIsDisabled", " ");
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
			MainWindow.setGUIProperty("FanArt.PageTitle", Title);
		}

		protected override void OnShowContextMenu()
		{
			try
			{
				GUIListItem currentitem = this.m_Facade.SelectedListItem;
				if (currentitem == null || !(currentitem.TVTag is FanartContainer)) return;
				FanartContainer selectedFanart = currentitem.TVTag as FanartContainer;

				IDialogbox dlg = (IDialogbox)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
				if (dlg == null) return;
				dlg.Reset();
				dlg.SetHeading("Fanart");


				GUIListItem pItem;


				if (!selectedFanart.IsImageEnabled)
				{
					pItem = new GUIListItem("Enable");
					dlg.Add(pItem);
				}
				else
				{
					pItem = new GUIListItem("Disable");
					dlg.Add(pItem);
				}

				if (selectedFanart.IsImageEnabled)
				{
					if (selectedFanart.IsImageDefault)
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
					bool endis = !selectedFanart.IsImageEnabled;
					JMMServerHelper.EnableDisableFanart(endis, selectedFanart, AnimeID);

					ShowFanart();
					return;
				}

				if (dlg.SelectedId == 2)
				{
					bool isdef = !selectedFanart.IsImageDefault;
					JMMServerHelper.SetDefaultFanart(isdef, selectedFanart, AnimeID);

					ShowFanart();
					return;
				}

			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write("Exception in Fanart Chooser Context Menu: " + ex.Message + ex.StackTrace);
				return;
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
			if (control == btnWideBanners)
			{
				GUIWindowManager.CloseCurrentWindow();
				GUIWindowManager.ActivateWindow(Constants.WindowIDs.WIDEBANNERS, false);
			}

			if (control == btnPosters)
			{
				GUIWindowManager.CloseCurrentWindow();
				GUIWindowManager.ActivateWindow(Constants.WindowIDs.POSTERS, false);
			}

			if (control == buttonLayouts)
			{
				bool shouldContinue = false;
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
			}

			if (MA3WindowManager.HandleWindowChangeButton(control))
				return;

			if (actionType != MediaPortal.GUI.Library.Action.ActionType.ACTION_SELECT_ITEM) return; // some other events raised onClicked too for some reason?
			
		}

        public override void OnAction(MediaPortal.GUI.Library.Action action)
        {
            if (viewIsFullscreen)
            {
                if(action.m_key.KeyCode == 27)
                {
                    // makes esc exit fullscreen
                    //hide fullscreen
                    dummyFullscreen.Visible = true;
                    viewIsFullscreen = false;
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
        

		void setFanartPreviewBackground(FanartContainer fanart)
		{
			MainWindow.setGUIProperty("FanArt.SelectedFanartResolution", " ");
			MainWindow.setGUIProperty("FanArt.SelectedPreview", " ");
			MainWindow.setGUIProperty("FanArt.SelectedFanartIsDisabled", " ");
			MainWindow.setGUIProperty("FanArt.SelectedFanartIsDefault", " ");
			MainWindow.setGUIProperty("FanArt.Source", " ");

			if (fanart == null) return;

			if (fanart.ImageType == ImageEntityType.TvDB_FanArt)
			{
				TvDB_ImageFanartVM fanartTvDB = fanart.FanartObject as TvDB_ImageFanartVM;
				MainWindow.setGUIProperty("FanArt.SelectedFanartResolution", fanartTvDB.BannerType2);
			}

			MainWindow.setGUIProperty("FanArt.SelectedFanartIsDisabled", fanart.IsImageEnabled ? "No" : "Yes");
			MainWindow.setGUIProperty("FanArt.SelectedFanartIsDefault", fanart.IsImageDefault ? "Yes" : "No");
			MainWindow.setGUIProperty("FanArt.Source", fanart.FanartSource);

			string preview = string.Empty;

			if (File.Exists(fanart.FullImagePath))
			{
				// Ensure Fanart on Disk is valid as well
				ImageAllocator.LoadImageFastFromFile(fanart.FullImagePath);

				// Should be safe to assign fullsize fanart if available
				preview = ImageAllocator.GetOtherImage(fanart.FullImagePath, default(System.Drawing.Size), false);
			}
			else
				preview = m_Facade.SelectedListItem.IconImageBig;

			MainWindow.setGUIProperty("FanArt.SelectedPreview", preview);
		}
	}
}
