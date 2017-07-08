using System.Collections.Generic;
using System.Drawing;
using System.IO;
using MediaPortal.GUI.Library;
using Shoko.Models.Client;
using Shoko.Models.Server;
using Shoko.MyAnime3.Events;
using Shoko.MyAnime3.Extensions;
using Shoko.MyAnime3.ViewModel;
using Shoko.MyAnime3.ViewModel.Helpers;
using Shoko.MyAnime3.ViewModel.Server;

namespace Shoko.MyAnime3.Windows
{
    public class CharWindow : GUIWindow
    {
        [SkinControl(50)] protected GUIFacadeControl m_Facade = null;

        [SkinControl(1301)] protected GUILabelControl dummyCharactersExist = null;

        [SkinControl(2)] protected GUIButtonControl btnGetMissingInfo = null;
        [SkinControl(3)] protected GUIButtonControl btnRefreshView = null;

        [SkinControl(910)] protected GUIButtonControl btnAnimeInfo = null;

        //[SkinControlAttribute(911)] protected GUIButtonControl btnAnimeCharacters = null;
        [SkinControl(912)] protected GUIButtonControl btnAnimeRelations = null;

        [SkinControl(913)] protected GUIButtonControl btnAnimeFanart = null;
        [SkinControl(914)] protected GUIButtonControl btnAnimePosters = null;
        [SkinControl(915)] protected GUIButtonControl btnAnimeWideBanners = null;

        [SkinControl(930)] protected GUIButtonControl btnSeiyuu = null;

        [SkinControl(5681)] protected GUILabelControl dummyPosterMainChar = null;
        [SkinControl(5682)] protected GUILabelControl dummyPosterMainActor = null;
        [SkinControl(5684)] protected GUILabelControl dummyPosterSeries = null;

        [SkinControl(5691)] protected GUILabelControl dummyMainCharExists = null;
        [SkinControl(5692)] protected GUILabelControl dummyMainActorExists = null;
        [SkinControl(5694)] protected GUILabelControl dummySeriesExists = null;

        private List<CL_AniDB_Character> charList = new List<CL_AniDB_Character>();
        VM_AniDB_Anime mainAnime;
        VM_AnimeSeries_User serMain;


        public enum GuiProperty
        {
            Character_Status,
            Title,
            Character_Name,
            Character_KanjiName,
            Actor_Name,
            Actor_KanjiName,
            Character_CharacterCount,
            Character_Main_Title,
            Character_CharType,
            Character_Description,
            Character_PosterMainChar,
            Character_PosterSeries,
            Character_PosterMainActor
        }

        public void SetGUIProperty(GuiProperty which, string value)
        {
            this.SetGUIProperty(which.ToString(), value);
        }

        public void ClearGUIProperty(GuiProperty which)
        {
            this.ClearGUIProperty(which.ToString());
        }


        public CharWindow()
        {
            GetID = Constants.WindowIDs.CHARACTERS;

            MainWindow.ServerHelper.GotCharacterCreatorImagesEvent += ServerHelper_GotCharacterCreatorImagesEvent;
        }

        void ServerHelper_GotCharacterCreatorImagesEvent(GotCharacterCreatorImagesEventArgs ev)
        {
            if (GUIWindowManager.ActiveWindow != Constants.WindowIDs.CHARACTERS) return;
            int aid = ev.AnimeID;
            if (mainAnime == null || aid != mainAnime.AnimeID) return;
            ClearGUIProperty(GuiProperty.Character_Status);
            ShowCharacters();
        }

        public override int GetID
        {
            get { return Constants.WindowIDs.CHARACTERS; }
            set { base.GetID = value; }
        }

        protected override void OnPageLoad()
        {
            base.OnPageLoad();
            LoadInfo();

            if (m_Facade != null)
                m_Facade.CurrentLayout = GUIFacadeControl.Layout.Filmstrip;

            ShowCharacters();
            ClearGUIProperty(GuiProperty.Character_Status);
            if (m_Facade != null)
                m_Facade.Focus = true;
        }

        private void LoadInfo()
        {
            if (MainWindow.GlobalSeriesID > 0)
            {
                serMain = ShokoServerHelper.GetSeries(MainWindow.GlobalSeriesID);
                if (serMain != null)
                    mainAnime = serMain.Anime;
            }
        }

        private void ShowCharacters()
        {
            GUIControl.ClearControl(GetID, m_Facade.GetID);

            if (dummyMainCharExists != null) dummyMainCharExists.Visible = false;
            if (dummyMainActorExists != null) dummyMainActorExists.Visible = false;
            if (dummySeriesExists != null) dummySeriesExists.Visible = false;

            BaseConfig.MyAnimeLog.Write("CharWindow.GlobalSeriesID = {0}", MainWindow.GlobalSeriesID.ToString());

            charList.Clear();

            if (serMain?.Anime == null)
            {
                ClearGUIProperty(GuiProperty.Title);
                ClearGUIProperty(GuiProperty.Character_Name);
                ClearGUIProperty(GuiProperty.Character_KanjiName);
                ClearGUIProperty(GuiProperty.Actor_Name);
                ClearGUIProperty(GuiProperty.Actor_KanjiName);
                ClearGUIProperty(GuiProperty.Character_CharacterCount);
                return;
            }
            serMain = ShokoServerHelper.GetSeries(MainWindow.GlobalSeriesID);
            if (serMain != null)
                mainAnime = serMain.Anime;
            else
                return;

            if (mainAnime == null)
                return;
            SetGUIProperty(GuiProperty.Character_Main_Title, mainAnime.FormattedTitle);

            charList = mainAnime.Characters;
            if (dummyCharactersExist != null)
                dummyCharactersExist.Visible = charList.Count > 0;
            SetGUIProperty(GuiProperty.Character_CharacterCount, charList.Count.ToString(Globals.Culture));
            SetGUIProperty(GuiProperty.Title, serMain.SeriesName);

            string imagePathNoPicture = GUIGraphicsContext.Skin + @"\Media\MyAnime3\anime3_blankchar.png";

            foreach (CL_AniDB_Character aniChar in charList)
            {
                string imagePath = imagePathNoPicture;
                if (!string.IsNullOrEmpty(aniChar.GetPosterPath()) && File.Exists(aniChar.GetPosterPath()))
                    imagePath = aniChar.GetPosterPath();

                GUIListItem item = new GUIListItem("");
                item.IconImage = item.IconImageBig = imagePath;
                item.TVTag = aniChar;
                item.OnItemSelected += onFacadeItemSelected;
                m_Facade.Add(item);
            }

            if (m_Facade.Count > 0)
            {
                m_Facade.SelectedListItemIndex = 0;

                CL_AniDB_Character aniChar = m_Facade.SelectedListItem.TVTag as CL_AniDB_Character;
                if (aniChar != null)
                    SetCharacterProperties(aniChar);
            }
        }

        private void SetCharacterProperties(CL_AniDB_Character aniChar)
        {
            if (dummyMainCharExists != null) dummyMainCharExists.Visible = true;
            if (dummyMainActorExists != null) dummyMainActorExists.Visible = false;
            if (dummySeriesExists != null) dummySeriesExists.Visible = false;

            ClearGUIProperty(GuiProperty.Character_PosterSeries);
            SetGUIProperty(GuiProperty.Character_Name, aniChar.CharName);
            SetGUIProperty(GuiProperty.Character_KanjiName, aniChar.CharKanjiName);
            SetGUIProperty(GuiProperty.Character_Description, aniChar.CharDescription);
            SetGUIProperty(GuiProperty.Character_CharType, aniChar.CharType);

            string imagePath = GUIGraphicsContext.Skin + @"\Media\MyAnime3\anime3_blankchar.png";
            if (File.Exists(aniChar.GetPosterPath()))
                imagePath = aniChar.GetPosterPath();

            try
            {
                Image theImage = Image.FromFile(imagePath);
                float width = theImage.PhysicalDimension.Width;
                float height = theImage.PhysicalDimension.Height;

                if (dummyPosterMainChar != null) dummyPosterMainChar.Visible = height > width;
            }
            catch
            {
            }

            SetGUIProperty(GuiProperty.Character_PosterMainChar, imagePath);

            SetActorProperties(aniChar);
        }

        private void SetActorProperties(CL_AniDB_Character aniChar)
        {
            if (dummyMainActorExists != null) dummyMainActorExists.Visible = false;
            if (dummySeriesExists != null) dummySeriesExists.Visible = false;

            string imagePath = GUIGraphicsContext.Skin + @"\Media\MyAnime3\anime3_blankchar.png";

            // get the actor from the character
            AniDB_Seiyuu actor = aniChar.Seiyuu;

            if (actor == null)
            {
                try
                {
                    Image theImage = Image.FromFile(imagePath);
                    float width = theImage.PhysicalDimension.Width;
                    float height = theImage.PhysicalDimension.Height;

                    if (dummyPosterMainActor != null) dummyPosterMainActor.Visible = height > width;
                }
                // ReSharper disable EmptyGeneralCatchClause
                catch
                {
                }
                // ReSharper restore EmptyGeneralCatchClause

                SetGUIProperty(GuiProperty.Character_PosterMainActor, imagePath);
                ClearGUIProperty(GuiProperty.Actor_Name);
            }
            else
            {
                MainWindow.GlobalSeiyuuID = actor.AniDB_SeiyuuID;

                if (dummyMainActorExists != null) dummyMainActorExists.Visible = true;

                SetGUIProperty(GuiProperty.Actor_Name, actor.SeiyuuName);
                if (File.Exists(actor.GetPosterPath()))
                    imagePath = actor.GetPosterPath();

                try
                {
                    Image theImage = Image.FromFile(imagePath);
                    float width = theImage.PhysicalDimension.Width;
                    float height = theImage.PhysicalDimension.Height;

                    if (dummyPosterMainActor != null) dummyPosterMainActor.Visible = height > width;
                }
                // ReSharper disable EmptyGeneralCatchClause
                catch
                {
                }
                // ReSharper restore EmptyGeneralCatchClause

                SetGUIProperty(GuiProperty.Character_PosterMainActor, imagePath);
            }
        }


        protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
        {
            MainMenu menu = new MainMenu();
            menu.Add(btnGetMissingInfo, () =>
            {
                VM_ShokoServer.Instance.ShokoServices.UpdateAnimeData(mainAnime.AnimeID);
                SetGUIProperty(GuiProperty.Character_Status, Translation.RequestSendToServerPleaseRefresh + "...");
                m_Facade.Focus = true;
            });
            menu.Add(btnRefreshView, () =>
            {
                VM_ShokoServer.Instance.ShokoServices.UpdateAnimeData(mainAnime.AnimeID);
                SetGUIProperty(GuiProperty.Character_Status, Translation.RequestSendToServerPleaseRefresh + "...");
                m_Facade.Focus = true;
            });
            menu.Add(btnSeiyuu, () => GUIWindowManager.ActivateWindow(Constants.WindowIDs.ACTORS, false));
            if (menu.Check(control))
                return;

            try
            {
                if (actionType == Action.ActionType.ACTION_SELECT_ITEM)
                    if (control == m_Facade)
                    {
                        GUIListItem item = m_Facade.SelectedListItem;

                        if (item == null || item.TVTag == null || !(item.TVTag is CL_AniDB_Character))
                            return;

                        CL_AniDB_Character aniChar = item.TVTag as CL_AniDB_Character;
                        AniDB_Seiyuu actor = aniChar.Seiyuu;
                        MainWindow.GlobalSeiyuuID = actor.AniDB_SeiyuuID;
                        GUIWindowManager.ActivateWindow(Constants.WindowIDs.ACTORS, false);
                    }
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch
            {
            }
            // ReSharper restore EmptyGeneralCatchClause


            base.OnClicked(controlId, control, actionType);
        }


        public override bool OnMessage(GUIMessage message)
        {
            switch (message.Message)
            {
                case GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS_CHANGED:
                {
                    int iControl = message.SenderControlId;

                    if (iControl == m_Facade.GetID)
                    {
                        GUIListItem item = m_Facade.SelectedListItem;

                        if (item == null || item.TVTag == null || !(item.TVTag is CL_AniDB_Character))
                            return true;

                        CL_AniDB_Character aniChar = item.TVTag as CL_AniDB_Character;
                        SetCharacterProperties(aniChar);
                    }
                }

                    return true;

                default:
                    return base.OnMessage(message);
            }
        }

        private void onFacadeItemSelected(GUIListItem item, GUIControl parent)
        {
            // if this is not a message from the facade, exit
            if (parent != m_Facade && parent != m_Facade.FilmstripLayout && parent != m_Facade.CoverFlowLayout)
                return;

            if (item == null || item.TVTag == null || !(item.TVTag is CL_AniDB_Character))
                return;

            CL_AniDB_Character aniChar = item.TVTag as CL_AniDB_Character;

            SetCharacterProperties(aniChar);
        }

        public override bool Init()
        {
            return this.InitSkin<GuiProperty>("Anime3_Char.xml");
        }
    }
}