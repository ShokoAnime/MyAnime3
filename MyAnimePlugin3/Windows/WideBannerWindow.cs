using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.GUI.Library;
using MyAnimePlugin3.DataHelpers;
using Action = MediaPortal.GUI.Library.Action;
using MyAnimePlugin3.ViewModel;
namespace MyAnimePlugin3.Windows
{
	public class WideBannerWindow : GUIWindow
	{
		[SkinControlAttribute(50)] protected GUIFacadeControl m_Facade = null;

		[SkinControlAttribute(11)] protected GUILabelControl labelWideBannerSource = null;
		[SkinControlAttribute(14)] protected GUILabelControl labelDisabled = null;
		[SkinControlAttribute(15)] protected GUILabelControl labelDefault = null;

		[SkinControlAttribute(140)] protected GUIButtonControl btnFanart = null;
		[SkinControlAttribute(141)] protected GUIButtonControl btnPosters = null;

		[SkinControlAttribute(910)] protected GUIButtonControl btnAnimeInfo = null;
		[SkinControlAttribute(911)] protected GUIButtonControl btnAnimeCharacters = null;
		[SkinControlAttribute(912)] protected GUIButtonControl btnAnimeRelations = null;
		[SkinControlAttribute(913)] protected GUIButtonControl btnAnimeFanart = null;
		[SkinControlAttribute(914)] protected GUIButtonControl btnAnimePosters = null;
		//[SkinControlAttribute(915)] protected GUIButtonControl btnAnimeWideBanners = null;

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
			String xmlSkin = GUIGraphicsContext.Skin + @"\Anime3_WideBanners.xml";
			return Load(xmlSkin);
		}

		public void setPageTitle(string Title)
		{
			MainWindow.setGUIProperty("WideBanners.PageTitle", Title);
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
			if (labelWideBannerSource != null) labelWideBannerSource.Label = "Source:";
			if (labelDefault != null) labelDefault.Label = "Default: ";
			if (labelDisabled != null) labelDisabled.Label = "Disabled";

			ClearProperties();
			ShowWideBanners();
		}

		private void ShowWideBanners()
		{
			GUIControl.ClearControl(this.GetID, m_Facade.GetID);

			AnimeSeriesVM ser = JMMServerHelper.GetSeries(MainWindow.GlobalSeriesID);
			if (ser != null)
			{
				if (ser.CrossRef_AniDB_TvDB != null) 
					AnimeID = ser.CrossRef_AniDB_TvDB.AnimeID;
			}

			if (ser != null)
			{
				List<TvDB_ImageWideBannerVM> tvDBWideBanners = ser.AniDB_Anime.AniDB_AnimeCrossRefs.TvDBImageWideBanners;

				GUIListItem item = null;
				foreach (TvDB_ImageWideBannerVM banner in tvDBWideBanners)
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
					TvDB_ImageWideBannerVM selectedBanner = m_Facade.SelectedListItem.TVTag as TvDB_ImageWideBannerVM;
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

		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{

			if (control == btnPosters)
			{
				GUIWindowManager.CloseCurrentWindow();
				GUIWindowManager.ActivateWindow(Constants.WindowIDs.POSTERS, false);
			}

			if (control == btnFanart)
			{
				GUIWindowManager.CloseCurrentWindow();
				GUIWindowManager.ActivateWindow(Constants.WindowIDs.FANART, false);
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

		}

		private void ClearProperties()
		{
			MainWindow.setGUIProperty("WideBanners.Count", " ");
			MainWindow.setGUIProperty("WideBanners.SelectedSource", " ");
			MainWindow.setGUIProperty("WideBanners.SelectedBannerIsDefault", " ");
			MainWindow.setGUIProperty("WideBanners.SelectedBannerIsDisabled", " ");
		}

		// triggered when a selection change was made on the facade
		private void onFacadeItemSelected(GUIListItem item, GUIControl parent)
		{
			// if this is not a message from the facade, exit
			if (parent != m_Facade && parent != m_Facade.ThumbnailLayout)
				return;

			TvDB_ImageWideBannerVM banner = item.TVTag as TvDB_ImageWideBannerVM;
			if (banner != null)
			{
				SetWideBannerProperties(banner);
			}

		}

		private void SetWideBannerProperties(TvDB_ImageWideBannerVM banner)
		{
			string source = "The TV DB";

			string isDisabled = "No";
			isDisabled = banner.Enabled == 0 ? "Yes" : "No";
						
			string isDefault = "No";
			if (banner.IsImageDefault) isDefault = "Yes";
		
			MainWindow.setGUIProperty("WideBanners.SelectedSource", source);
			MainWindow.setGUIProperty("WideBanners.SelectedBannerIsDefault", isDefault);
			MainWindow.setGUIProperty("WideBanners.SelectedBannerIsDisabled", isDisabled);
		}

		protected override void OnShowContextMenu()
		{
			try
			{
				GUIListItem currentitem = this.m_Facade.SelectedListItem;
				if (currentitem == null || !(currentitem.TVTag is TvDB_ImageWideBannerVM)) return;
				TvDB_ImageWideBannerVM selectedBanner = currentitem.TVTag as TvDB_ImageWideBannerVM;

				IDialogbox dlg = (IDialogbox)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
				if (dlg == null) return;
				dlg.Reset();
				dlg.SetHeading("Wide Banner");

				bool isDisabled = false;
				bool isDefault = false;

				isDisabled = selectedBanner.Enabled == 0 ? true : false;


				GUIListItem pItem;

				if (isDisabled)
				{
					pItem = new GUIListItem("Enable"); dlg.Add(pItem);
				}
				else
				{
					pItem = new GUIListItem("Disable"); dlg.Add(pItem);
				}

				if (!isDisabled)
				{
					if (!selectedBanner.IsImageDefault)
					{
						pItem = new GUIListItem("Set as Default"); dlg.Add(pItem);
					}
					if (selectedBanner.IsImageDefault)
					{
						pItem = new GUIListItem("Remove as Default"); dlg.Add(pItem);
					}
				}


				// lets show it
				dlg.DoModal(GUIWindowManager.ActiveWindow);

				if (dlg.SelectedId == 1) // enabled/disable
				{
					bool endis = isDisabled;
					JMMServerHelper.EnableDisableWideBanner(endis, selectedBanner, AnimeID);

					ShowWideBanners();
					return;
				}

				if (dlg.SelectedId == 2)
				{
					bool isdef = !selectedBanner.IsImageDefault;
					JMMServerHelper.SetDefaultWideBanner(isdef, selectedBanner, AnimeID);

					ShowWideBanners();
					return;
				}

			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write("Exception in Wide Banner Chooser Context Menu: " + ex.Message);
				return;
			}
		}
	}
}
