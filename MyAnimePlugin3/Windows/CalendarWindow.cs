using System;
using System.Collections.Generic;
using MediaPortal.GUI.Library;
using BinaryNorthwest;
using System.IO;
using MyAnimePlugin3.Downloads;
using MyAnimePlugin3.ViewModel;

namespace MyAnimePlugin3.Windows
{
    public class CalendarWindow : GUIWindow
    {
        private List<AniDB_AnimeVM> colAnime = new List<AniDB_AnimeVM>();

        [SkinControlAttribute(50)]
        protected GUIFacadeControl m_Facade = null;

        [SkinControlAttribute(920)]
        protected GUIButtonControl btnWindowContinueWatching = null;
        [SkinControlAttribute(921)]
        protected GUIButtonControl btnWindowUtilities = null;
        //[SkinControlAttribute(922)] protected GUIButtonControl btnWindowCalendar = null;
        [SkinControlAttribute(923)]
        protected GUIButtonControl btnWindowDownloads = null;
        //[SkinControlAttribute(924)] protected GUIButtonControl btnWindowCollectionStats = null;
        [SkinControlAttribute(925)]
        protected GUIButtonControl btnWindowRecommendations = null;

        [SkinControlAttribute(1401)]
        protected GUILabelControl dummyAnyAnimeForMonth = null;
        [SkinControlAttribute(1402)]
        protected GUILabelControl dummyAnimeInCollection = null;
        [SkinControlAttribute(1403)]
        protected GUILabelControl dummyRelatedAnime = null;

        [SkinControlAttribute(81)]
        protected GUIButtonControl btnCurrentMinusThree = null;
        [SkinControlAttribute(82)]
        protected GUIButtonControl btnCurrentMinusTwo = null;
        [SkinControlAttribute(83)]
        protected GUIButtonControl btnCurrentMinusOne = null;

        [SkinControlAttribute(84)]
        protected GUIButtonControl btnCurrentPlusOne = null;
        [SkinControlAttribute(85)]
        protected GUIButtonControl btnCurrentPlusTwo = null;
        [SkinControlAttribute(86)]
        protected GUIButtonControl btnCurrentPlusThree = null;

        public enum GuiProperty
        {
            Calendar_Status,
            Calendar_Title,
            Calendar_Description,
            Calendar_AirDate,
            Calendar_Genre,
            Calendar_GenreShort,
            Calendar_Restricted,
            Calendar_CurrentMonth,
            Calendar_CurrentYear,
            Calendar_MinusOneMonth,
            Calendar_MinusOneYear,
            Calendar_MinusTwoMonth,
            Calendar_MinusTwoYear,
            Calendar_MinusThreeMonth,
            Calendar_MinusThreeYear,
            Calendar_PlusOneMonth,
            Calendar_PlusOneYear,
            Calendar_PlusTwoMonth,
            Calendar_PlusTwoYear,
            Calendar_PlusThreeMonth,
            Calendar_PlusThreeYear
        }


        public void SetGUIProperty(GuiProperty which, string value) { this.SetGUIProperty(which.ToString(), value); }
        public void ClearGUIProperty(GuiProperty which) { this.ClearGUIProperty(which.ToString()); }

        public CalendarWindow()
        {
            // get ID of windowplugin belonging to this setup
            // enter your own unique code
            // ReSharper disable DoNotCallOverridableMethodsInConstructor
            GetID = Constants.WindowIDs.CALENDAR;
            // ReSharper restore DoNotCallOverridableMethodsInConstructor
            ClearGUIProperty(GuiProperty.Calendar_Status);
        }

        public override int GetID
        {
            get { return Constants.WindowIDs.CALENDAR; }
            set { base.GetID = value; }
        }

        protected override void OnPageLoad()
        {
            base.OnPageLoad();
            _usedMonths = new List<DateTime>();
            _usedMonths.Add(new DateTime(MainWindow.CurrentCalendarYear, MainWindow.CurrentCalendarMonth, 1));
            BaseConfig.MyAnimeLog.Write("OnPageLoad: {0}/{1}", MainWindow.CurrentCalendarMonth, MainWindow.CurrentCalendarYear);
            LoadData(0, new DateTime(MainWindow.CurrentCalendarYear, MainWindow.CurrentCalendarMonth, 1));

            // Load 6 future calendar months
            int monthsToLoad = 1;
            while (monthsToLoad <= 6)
            {
                LoadData(monthsToLoad, new DateTime(MainWindow.CurrentCalendarYear, MainWindow.CurrentCalendarMonth + monthsToLoad, 1));
                monthsToLoad++;
            }

            m_Facade.Focus = true;
        }

        private List<DateTime> _usedMonths = new List<DateTime>();

        private void LoadData(int step, DateTime monthNow, bool special = false)
        {

            BaseConfig.MyAnimeLog.Write("LoadData Step: " + step + " Month: " + monthNow);

            // find the anime for this month
            colAnime = JMMServerHelper.GetAnimeForMonthYear(monthNow.Month, monthNow.Year);

            dummyAnyAnimeForMonth.Visible = ((colAnime.Count > 0) || (m_Facade.Count > 0));


            // now sort the groups by air date
            List<SortPropOrFieldAndDirection> sortCriteria = new List<SortPropOrFieldAndDirection>();
            sortCriteria.Add(new SortPropOrFieldAndDirection("AirDate", step == -1, SortType.eDateTime));
            colAnime = Sorting.MultiSort(colAnime, sortCriteria);

            //BaseConfig.MyAnimeLog.Write(monthNow.ToString("MMM yyyy").ToUpper());
            if (m_Facade.Count == 0)
            {
                GUIControl.ClearControl(GetID, m_Facade.GetID);
                m_Facade.CurrentLayout = GUIFacadeControl.Layout.Filmstrip;
            }

            AniDB_AnimeVM selanime = null;
            if (m_Facade.Count > 0)
                selanime = (AniDB_AnimeVM)m_Facade.SelectedListItem.TVTag;

            int selIndex = 0;
            foreach (AniDB_AnimeVM anime in colAnime)
            {
                //BaseConfig.MyAnimeLog.Write(anime.ToString());

                string imagePath = GUIGraphicsContext.Skin + @"\Media\MyAnime3\anime3_question_poster.png";
                if (anime.DefaultPosterPath.Trim().Length > 0)
                {
                    if (File.Exists(anime.DefaultPosterPath))
                        imagePath = anime.DefaultPosterPath;
                }

                if ((anime.AnimeID == MainWindow.GlobalSeriesID) && (m_Facade.Count == 0))
                {
                    selanime = anime;

                }


                GUIListItem item = new GUIListItem();
                item.IconImage = item.IconImageBig = imagePath;
                item.TVTag = anime;
                item.OnItemSelected += onFacadeItemSelected;
                if (step == -1)
                    m_Facade.Insert(0, item);
                else
                    m_Facade.Add(item);
            }

            if ((m_Facade.Count > 0) && (selanime != null))
            {
                //BaseConfig.MyAnimeLog.Write("selIndex: {0}", selIndex.ToString());

                for (int x = 0; x < m_Facade.Count; x++)
                {
                    if ((m_Facade.FilmstripLayout.ListItems[x].TVTag) == selanime)
                    {
                        selIndex = x;
                        break;
                    }
                }
                if (special) //hack
                {
                    m_Facade.OnAction(new MediaPortal.GUI.Library.Action(MediaPortal.GUI.Library.Action.ActionType.ACTION_MOVE_LEFT, 0, 0));
                    m_Facade.OnAction(new MediaPortal.GUI.Library.Action(MediaPortal.GUI.Library.Action.ActionType.ACTION_MOVE_RIGHT, 0, 0));
                }

                m_Facade.SelectedListItemIndex = selIndex;

                AniDB_AnimeVM anime = m_Facade.SelectedListItem.TVTag as AniDB_AnimeVM;
                if (anime != null)
                {
                    SetAnime(anime);
                }
            }
            if (step == 0)
            {
                LoadData(-1, monthNow.AddMonths(-1), true);
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
            if (m_Facade.SelectedListItemIndex == 0)
            {
                DateTime kmonth = new DateTime(MainWindow.CurrentCalendarYear, MainWindow.CurrentCalendarMonth, 1);
                kmonth = kmonth.AddMonths(-1);
                if (!_usedMonths.Contains(kmonth))
                {
                    _usedMonths.Add(kmonth);
                    LoadData(-1, kmonth);
                    m_Facade.FilmstripLayout.Scrollbar.UpdateLayout();
                }
            }
            else if (m_Facade.SelectedListItemIndex == m_Facade.Count - 1)
            {
                DateTime kmonth = new DateTime(MainWindow.CurrentCalendarYear, MainWindow.CurrentCalendarMonth, 1);
                kmonth = kmonth.AddMonths(1);
                if (!_usedMonths.Contains(kmonth))
                {
                    _usedMonths.Add(kmonth);
                    LoadData(1, kmonth);
                    m_Facade.FilmstripLayout.Scrollbar.UpdateLayout();
                }
            }


        }

        private void SetAnime(AniDB_AnimeVM anime)
        {
            if (dummyAnimeInCollection != null) dummyAnimeInCollection.Visible = false;
            if (dummyRelatedAnime != null) dummyRelatedAnime.Visible = false;

            if (anime == null) return;

            SetGUIProperty(GuiProperty.Calendar_Title, anime.FormattedTitle);
            SetGUIProperty(GuiProperty.Calendar_Description, anime.ParsedDescription);
            SetGUIProperty(GuiProperty.Calendar_AirDate, anime.AirDateAsString);
            SetGUIProperty(GuiProperty.Calendar_Genre, anime.TagsFormatted);
            SetGUIProperty(GuiProperty.Calendar_GenreShort, anime.TagsFormattedShort);
            ClearGUIProperty(GuiProperty.Calendar_Restricted);
            if (anime.AirDate.HasValue)
            {
                MainWindow.CurrentCalendarYear = anime.AirDate.Value.Year;
                MainWindow.CurrentCalendarMonth = anime.AirDate.Value.Month;
                SetGUIProperty(GuiProperty.Calendar_CurrentMonth, anime.AirDate.Value.ToString("MMM", Globals.Culture));
                SetGUIProperty(GuiProperty.Calendar_CurrentYear, anime.AirDate.Value.ToString("yyyy", Globals.Culture));
            }
            else
            {
                ClearGUIProperty(GuiProperty.Calendar_CurrentMonth);
                ClearGUIProperty(GuiProperty.Calendar_CurrentYear);
            }
        }

        public override bool Init()
        {
            return this.InitSkin<GuiProperty>("Anime3_Calendar.xml");
        }

        protected override void OnShowContextMenu()
        {
            try
            {
                ContextMenu cmenu = new ContextMenu(Translation.Calendar);
                cmenu.AddAction(Translation.SearchForTorrents, () =>
                {
                    AniDB_AnimeVM anime2;
                    if ((anime2 = m_Facade.SelectedListItem.TVTag as AniDB_AnimeVM) != null)
                        DownloadHelper.SearchAnime(anime2);
                });
                cmenu.AddAction(Translation.BookmarkThisAnime, () =>
                {
                    AniDB_AnimeVM anime3;
                    if ((anime3 = m_Facade.SelectedListItem.TVTag as AniDB_AnimeVM) != null)
                    {
                        BookmarkedAnimeVM bookmark = new BookmarkedAnimeVM();
                        bookmark.AnimeID = anime3.AnimeID;
                        bookmark.Downloading = 0;
                        bookmark.Notes = string.Empty;
                        bookmark.Priority = 1;
                        if (bookmark.Save())
                        {
                            Utils.DialogMsg(Translation.Sucess, Translation.BookmarkCreated);
                        }
                    }
                });
                cmenu.AddAction(Translation.CreateSeriesForAnime, () =>
                {
                    AniDB_AnimeVM anime4;
                    if ((anime4 = m_Facade.SelectedListItem.TVTag as AniDB_AnimeVM) != null)
                    {
                        JMMServerBinary.Contract_AnimeSeries_SaveResponse resp = JMMServerVM.Instance.clientBinaryHTTP.CreateSeriesFromAnime(
                            anime4.AnimeID, null, JMMServerVM.Instance.CurrentUser.JMMUserID);
                        if (string.IsNullOrEmpty(resp.ErrorMessage))
                            Utils.DialogMsg(Translation.Sucess, Translation.SeriesCreated);
                        else
                            Utils.DialogMsg(Translation.Error, resp.ErrorMessage);
                    }
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
            if (MA3WindowManager.HandleWindowChangeButton(control))
                return;
            base.OnClicked(controlId, control, actionType);
        }
    }
}
