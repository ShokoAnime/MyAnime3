using System;
using System.Collections.Generic;

using MediaPortal.GUI.Library;

using BinaryNorthwest;

using System.IO;

using MediaPortal.Dialogs;
using MyAnimePlugin3.Downloads;
using Action = MediaPortal.GUI.Library.Action;
using MyAnimePlugin3.ViewModel;

namespace MyAnimePlugin3.Windows
{
	public class CalendarWindow : GUIWindow
	{
		private List<AniDB_AnimeVM> colAnime = new List<AniDB_AnimeVM>();

		[SkinControlAttribute(50)]
		protected GUIFacadeControl m_Facade = null;

		[SkinControlAttribute(920)] protected GUIButtonControl btnWindowContinueWatching = null;
		[SkinControlAttribute(921)] protected GUIButtonControl btnWindowUtilities = null;
		//[SkinControlAttribute(922)] protected GUIButtonControl btnWindowCalendar = null;
		[SkinControlAttribute(923)] protected GUIButtonControl btnWindowDownloads = null;
		//[SkinControlAttribute(924)] protected GUIButtonControl btnWindowCollectionStats = null;

		[SkinControlAttribute(1401)] protected GUILabelControl dummyAnyAnimeForMonth = null;
		[SkinControlAttribute(1402)] protected GUILabelControl dummyAnimeInCollection = null;
		[SkinControlAttribute(1403)] protected GUILabelControl dummyRelatedAnime = null;

		[SkinControlAttribute(81)] protected GUIButtonControl btnCurrentMinusThree = null;
		[SkinControlAttribute(82)] protected GUIButtonControl btnCurrentMinusTwo = null;
		[SkinControlAttribute(83)] protected GUIButtonControl btnCurrentMinusOne = null;

		[SkinControlAttribute(84)] protected GUIButtonControl btnCurrentPlusOne = null;
		[SkinControlAttribute(85)] protected GUIButtonControl btnCurrentPlusTwo = null;
		[SkinControlAttribute(86)] protected GUIButtonControl btnCurrentPlusThree = null;

		DateTime monthNow = DateTime.Now;

		DateTime monthMinusThree = DateTime.Now;
		DateTime monthMinusTwo = DateTime.Now;
		DateTime monthMinusOne = DateTime.Now;

		DateTime monthPlusThree = DateTime.Now;
		DateTime monthPlusTwo = DateTime.Now;
		DateTime monthPlusOne = DateTime.Now;

		public CalendarWindow()
		{
			// get ID of windowplugin belonging to this setup
			// enter your own unique code
			GetID = Constants.WindowIDs.CALENDAR;

			setGUIProperty("Calendar.Status", "-");
		}

		public override int GetID
		{
			get { return Constants.WindowIDs.CALENDAR; }
			set { base.GetID = value; }
		}

		protected override void OnPageLoad()
		{
			base.OnPageLoad();

			BaseConfig.MyAnimeLog.Write("OnPageLoad: {0}/{1}", MainWindow.CurrentCalendarMonth, MainWindow.CurrentCalendarYear);

			LoadData();
			m_Facade.Focus = true;
		}

		private void LoadData()
		{
			dummyAnyAnimeForMonth.Visible = false;

			monthNow = new DateTime(MainWindow.CurrentCalendarYear, MainWindow.CurrentCalendarMonth, 1);

			monthMinusOne = monthNow.AddMonths(-1);
			monthMinusTwo = monthNow.AddMonths(-2);
			monthMinusThree = monthNow.AddMonths(-3);

			monthPlusOne = monthNow.AddMonths(1);
			monthPlusTwo = monthNow.AddMonths(2);
			monthPlusThree = monthNow.AddMonths(3);

			setGUIProperty("Calendar.CurrentMonth", monthNow.ToString("MMM"));
			setGUIProperty("Calendar.CurrentYear", monthNow.ToString("yyyy"));

			setGUIProperty("Calendar.MinusOneMonth", monthMinusOne.ToString("MMM"));
			setGUIProperty("Calendar.MinusOneYear", monthMinusOne.ToString("yyyy"));

			setGUIProperty("Calendar.MinusTwoMonth", monthMinusTwo.ToString("MMM"));
			setGUIProperty("Calendar.MinusTwoYear", monthMinusTwo.ToString("yyyy"));

			setGUIProperty("Calendar.MinusThreeMonth", monthMinusThree.ToString("MMM"));
			setGUIProperty("Calendar.MinusThreeYear", monthMinusThree.ToString("yyyy"));


			setGUIProperty("Calendar.PlusOneMonth", monthPlusOne.ToString("MMM"));
			setGUIProperty("Calendar.PlusOneYear", monthPlusOne.ToString("yyyy"));

			setGUIProperty("Calendar.PlusTwoMonth", monthPlusTwo.ToString("MMM"));
			setGUIProperty("Calendar.PlusTwoYear", monthPlusTwo.ToString("yyyy"));

			setGUIProperty("Calendar.PlusThreeMonth", monthPlusThree.ToString("MMM"));
			setGUIProperty("Calendar.PlusThreeYear", monthPlusThree.ToString("yyyy"));

			
			// find the anime for this month
			colAnime = JMMServerHelper.GetAnimeForMonthYear(monthNow.Month, monthNow.Year);

			if (colAnime.Count > 0) dummyAnyAnimeForMonth.Visible = true;

			// now sort the groups by air date
			List<SortPropOrFieldAndDirection> sortCriteria = new List<SortPropOrFieldAndDirection>();
			sortCriteria.Add(new SortPropOrFieldAndDirection("AirDate", false, SortType.eDateTime));
			colAnime = Sorting.MultiSort<AniDB_AnimeVM>(colAnime, sortCriteria);

			//BaseConfig.MyAnimeLog.Write(monthNow.ToString("MMM yyyy").ToUpper());
			
			GUIControl.ClearControl(this.GetID, m_Facade.GetID);
			m_Facade.CurrentLayout = GUIFacadeControl.Layout.Filmstrip;

			int selIndex = 0;
			int pos = 0;
			foreach (AniDB_AnimeVM anime in colAnime)
			{
				//BaseConfig.MyAnimeLog.Write(anime.ToString());

				string imagePath = GUIGraphicsContext.Skin + @"\Media\MyAnime3\anime3_question_poster.png";
				if (anime.DefaultPosterPath.Trim().Length > 0)
				{
					if (File.Exists(anime.DefaultPosterPath))
						imagePath = anime.DefaultPosterPath;
				}

				if (anime.AnimeID == MainWindow.GlobalSeriesID)
				{
					selIndex = pos;
				}
				pos++;

				GUIListItem item = new GUIListItem();
				item.IconImage = item.IconImageBig = imagePath;
				item.TVTag = anime;
				item.OnItemSelected += new GUIListItem.ItemSelectedHandler(onFacadeItemSelected);
				m_Facade.Add(item);
			}

			if (m_Facade.Count > 0)
			{
				//BaseConfig.MyAnimeLog.Write("selIndex: {0}", selIndex.ToString());

				int currentIndex = m_Facade.SelectedListItemIndex;
				if (selIndex >= 0 && selIndex < m_Facade.Count && selIndex != currentIndex)
				{
					int increment = (currentIndex < selIndex) ? 1 : -1;
					MediaPortal.GUI.Library.Action.ActionType actionType = (currentIndex < selIndex) ? MediaPortal.GUI.Library.Action.ActionType.ACTION_MOVE_RIGHT : MediaPortal.GUI.Library.Action.ActionType.ACTION_MOVE_LEFT;
					for (int i = currentIndex; i != selIndex; i += increment)
					{
						m_Facade.OnAction(new MediaPortal.GUI.Library.Action(actionType, 0, 0));
					}
				}

				m_Facade.SelectedListItemIndex = selIndex;

				AniDB_AnimeVM anime = m_Facade.SelectedListItem.TVTag as AniDB_AnimeVM;
				if (anime != null)
				{
					SetAnime(anime);
				}
			}

			// set the focus button
			btnCurrentPlusOne.Focus = false;
			switch (MainWindow.CurrentCalendarButton)
			{
				case 1: btnCurrentMinusThree.Focus = true; break;
				case 2: btnCurrentMinusTwo.Focus = true; break;
				case 3: btnCurrentMinusOne.Focus = true; break;
				case 4: btnCurrentPlusOne.Focus = true; break;
				case 5: btnCurrentPlusTwo.Focus = true; break;
				case 6: btnCurrentPlusThree.Focus = true; break;
			}
		}

		private void onFacadeItemSelected(GUIListItem item, GUIControl parent)
		{
            //BaseConfig.MyAnimeLog.Write("Facade Item Selected");
			// if this is not a message from the facade, exit
			if (parent != m_Facade && parent != m_Facade.FilmstripLayout && parent != m_Facade.CoverFlowLayout)
				return;

			AniDB_AnimeVM anime = item.TVTag as AniDB_AnimeVM;
			SetAnime(anime);

		}

		public override bool OnMessage(GUIMessage message)
		{
			//BaseConfig.MyAnimeLog.Write("OnMessage: {0}", message.Message);
			switch (message.Message)
			{
				case GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS_CHANGED:
					{
						//BaseConfig.MyAnimeLog.Write("GUI_MSG_ITEM_FOCUS_CHANGED: {0}", message.SenderControlId.ToString());
						break;
					}

				case GUIMessage.MessageType.GUI_MSG_SETFOCUS:
					{
						//BaseConfig.MyAnimeLog.Write("GUI_MSG_SETFOCUS: {0}", message..ToString());
						break;
					}
			}

			return base.OnMessage(message);
		}

		private void SetAnime(AniDB_AnimeVM anime)
		{
			if (dummyAnimeInCollection != null) dummyAnimeInCollection.Visible = false;
			if (dummyRelatedAnime != null) dummyRelatedAnime.Visible = false;

			if (anime == null) return;

			setGUIProperty("Calendar.Title", anime.MainTitle);
			setGUIProperty("Calendar.Description", anime.ParsedDescription);
			setGUIProperty("Calendar.AirDate", anime.AirDateAsString);
			setGUIProperty("Calendar.Genre", anime.CategoriesFormatted);
			setGUIProperty("Calendar.GenreShort", anime.CategoriesFormattedShort);
			setGUIProperty("Calendar.Restricted", "-");
		}

		public override bool Init()
		{
			return Load(GUIGraphicsContext.Skin + @"\Anime3_Calendar.xml");
		}

		public static void setGUIProperty(string which, string value)
		{
			MediaPortal.GUI.Library.GUIPropertyManager.SetProperty("#Anime3." + which, value);
		}

		public static void clearGUIProperty(string which)
		{
			setGUIProperty(which, "-"); // String.Empty doesn't work on non-initialized fields, as a result they would display as ugly #TVSeries.bla.bla
		}

		protected override void OnShowContextMenu()
		{
			try
			{
				GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
				if (dlg == null)
					return;

				dlg.Reset();
				dlg.SetHeading("Calendar");
				dlg.Add("Search for Torrents");
				dlg.DoModal(GUIWindowManager.ActiveWindow);

				switch (dlg.SelectedId)
				{
					case 1:

						AniDB_AnimeVM anime2 = null;
						if ((anime2 = this.m_Facade.SelectedListItem.TVTag as AniDB_AnimeVM) != null)
						{
							DownloadHelper.SearchAnime(anime2);
						}

						break;
				}
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write("Error in menu: {0}", ex);
			}
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
			if (MA3WindowManager.HandleWindowChangeButton(control))
				return;

			if (control == this.btnCurrentMinusOne)
			{
				MainWindow.CurrentCalendarMonth = monthMinusOne.Month;
				MainWindow.CurrentCalendarYear = monthMinusOne.Year;
				MainWindow.CurrentCalendarButton = 3;
				GUIWindowManager.CloseCurrentWindow();
				GUIWindowManager.ActivateWindow(Constants.WindowIDs.CALENDAR, false);

				return;
			}

			if (control == this.btnCurrentMinusTwo)
			{
				MainWindow.CurrentCalendarMonth = monthMinusTwo.Month;
				MainWindow.CurrentCalendarYear = monthMinusTwo.Year;
				MainWindow.CurrentCalendarButton = 2;
				GUIWindowManager.CloseCurrentWindow();
				GUIWindowManager.ActivateWindow(Constants.WindowIDs.CALENDAR, false);

				return;
			}

			if (control == this.btnCurrentMinusThree)
			{
				MainWindow.CurrentCalendarMonth = monthMinusThree.Month;
				MainWindow.CurrentCalendarYear = monthMinusThree.Year;
				MainWindow.CurrentCalendarButton = 1;
				GUIWindowManager.CloseCurrentWindow();
				GUIWindowManager.ActivateWindow(Constants.WindowIDs.CALENDAR, false);

				return;
			}

			if (control == this.btnCurrentPlusOne)
			{
				MainWindow.CurrentCalendarMonth = monthPlusOne.Month;
				MainWindow.CurrentCalendarYear = monthPlusOne.Year;
				MainWindow.CurrentCalendarButton = 4;
				GUIWindowManager.CloseCurrentWindow();
				GUIWindowManager.ActivateWindow(Constants.WindowIDs.CALENDAR, false);

				return;
			}

			if (control == this.btnCurrentPlusTwo)
			{
				MainWindow.CurrentCalendarMonth = monthPlusTwo.Month;
				MainWindow.CurrentCalendarYear = monthPlusTwo.Year;
				MainWindow.CurrentCalendarButton = 5;
				GUIWindowManager.CloseCurrentWindow();
				GUIWindowManager.ActivateWindow(Constants.WindowIDs.CALENDAR, false);

				return;
			}

			if (control == this.btnCurrentPlusThree)
			{
				MainWindow.CurrentCalendarMonth = monthPlusThree.Month;
				MainWindow.CurrentCalendarYear = monthPlusThree.Year;
				MainWindow.CurrentCalendarButton = 6;
				GUIWindowManager.CloseCurrentWindow();
				GUIWindowManager.ActivateWindow(Constants.WindowIDs.CALENDAR, false);

				return;
			}

			base.OnClicked(controlId, control, actionType);
		}
	}
}
