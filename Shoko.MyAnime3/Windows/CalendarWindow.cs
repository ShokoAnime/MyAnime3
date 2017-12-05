using System;
using System.Collections.Generic;
using MediaPortal.GUI.Library;

using System.IO;
using System.Linq;
using Shoko.Models.Client;
using Shoko.MyAnime3.ViewModel;
using Shoko.MyAnime3.ViewModel.Helpers;
using Shoko.MyAnime3.ViewModel.Server;

namespace Shoko.MyAnime3.Windows
{
    public class CalendarWindow : GUIWindow
    {
        private List<VM_AniDB_Anime> colAnime = new List<VM_AniDB_Anime>();

        [SkinControl(50)]
        protected GUIFacadeControl m_Facade = null;

        [SkinControl(920)]
        protected GUIButtonControl btnWindowContinueWatching = null;
        [SkinControl(921)]
        protected GUIButtonControl btnWindowUtilities = null;
        //[SkinControlAttribute(922)] protected GUIButtonControl btnWindowCalendar = null;

        //[SkinControlAttribute(924)] protected GUIButtonControl btnWindowCollectionStats = null;
        [SkinControl(925)]
        protected GUIButtonControl btnWindowRecommendations = null;

        [SkinControl(1401)]
        protected GUILabelControl dummyAnyAnimeForMonth = null;
        [SkinControl(1402)]
        protected GUILabelControl dummyAnimeInCollection = null;
        [SkinControl(1403)]
        protected GUILabelControl dummyRelatedAnime = null;

        [SkinControl(81)]
        protected GUIButtonControl btnCurrentMinusThree = null;
        [SkinControl(82)]
        protected GUIButtonControl btnCurrentMinusTwo = null;
        [SkinControl(83)]
        protected GUIButtonControl btnCurrentMinusOne = null;

        [SkinControl(84)]
        protected GUIButtonControl btnCurrentPlusOne = null;
        [SkinControl(85)]
        protected GUIButtonControl btnCurrentPlusTwo = null;
        [SkinControl(86)]
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
            colAnime = ShokoServerHelper.GetAnimeForMonthYear(monthNow.Month, monthNow.Year);

            dummyAnyAnimeForMonth.Visible = ((colAnime.Count > 0) || (m_Facade.Count > 0));


            // now sort the groups by air date
           
            if (step == -1)
                colAnime = colAnime.OrderByDescending(a => a.AirDate).ToList();
            else
                colAnime = colAnime.OrderBy(a => a.AirDate).ToList();

            //BaseConfig.MyAnimeLog.Write(monthNow.ToString("MMM yyyy").ToUpper());
            if (m_Facade.Count == 0)
            {
                GUIControl.ClearControl(GetID, m_Facade.GetID);
                m_Facade.CurrentLayout = GUIFacadeControl.Layout.Filmstrip;
            }

            VM_AniDB_Anime selanime = null;
            if (m_Facade.Count > 0)
                selanime = (VM_AniDB_Anime)m_Facade.SelectedListItem.TVTag;

            int selIndex = 0;
            foreach (VM_AniDB_Anime anime in colAnime)
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

                VM_AniDB_Anime anime = m_Facade.SelectedListItem.TVTag as VM_AniDB_Anime;
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

            VM_AniDB_Anime anime = item.TVTag as VM_AniDB_Anime;
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

        private void SetAnime(VM_AniDB_Anime anime)
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
                cmenu.AddAction(Translation.BookmarkThisAnime, () =>
                {
                    VM_AniDB_Anime anime3;
                    if ((anime3 = m_Facade.SelectedListItem.TVTag as VM_AniDB_Anime) != null)
                    {
                        VM_BookmarkedAnime bookmark = new VM_BookmarkedAnime();
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
                    VM_AniDB_Anime anime4;
                    if ((anime4 = m_Facade.SelectedListItem.TVTag as VM_AniDB_Anime) != null)
                    {
                        CL_Response<CL_AnimeSeries_User> resp = VM_ShokoServer.Instance.ShokoServices.CreateSeriesFromAnime(
                            anime4.AnimeID, null, VM_ShokoServer.Instance.CurrentUser.JMMUserID,false);
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
