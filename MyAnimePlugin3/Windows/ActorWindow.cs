using System.Collections.Generic;
using System.Globalization;
using System.IO;
using MediaPortal.GUI.Library;
using MyAnimePlugin3.JMMServerBinary;
using MyAnimePlugin3.ViewModel;
// ReSharper disable InconsistentNaming

namespace MyAnimePlugin3.Windows
{
    public class ActorWindow : GUIWindow
    {
        public enum GuiProperty
        {
            Actors_Status,
            Actors_Actor_Name,
            Actors_Actor_Poster,
            Actors_Character_CharacterCount,
            Actors_Character_Name,
            Actors_Character_KanjiName,
            Actors_Character_Description,
            Actors_Character_CharType,
            Actors_Character_Poster,
            Actors_Series_Title,
            Actors_Series_Poster
        }

        public void SetGUIProperty(GuiProperty which, string value)
        {
            this.SetGUIProperty(which.ToString(), value);
        }

        public void ClearGUIProperty(GuiProperty which)
        {
            this.ClearGUIProperty(which.ToString());
        }


        [SkinControl(50)] protected GUIFacadeControl m_Facade = null;
        [SkinControl(1301)] protected GUILabelControl dummyCharactersExist = null;
        [SkinControl(2)] protected GUIButtonControl btnGetMissingInfo = null;

        private AniDB_SeiyuuVM seiyuu;
        private List<AniDB_CharacterVM> charList = new List<AniDB_CharacterVM>();

        public ActorWindow()
        {
            // get ID of windowplugin belonging to this setup
            // enter your own unique code
            // ReSharper disable once VirtualMemberCallInConstructor
            GetID = Constants.WindowIDs.ACTORS;

            MainWindow.ServerHelper.GotCharacterImagesEvent += ServerHelper_GotCharacterImagesEvent;
        }

        void ServerHelper_GotCharacterImagesEvent(Events.GotCharacterImagesEventArgs ev)
        {
            if (GUIWindowManager.ActiveWindow == Constants.WindowIDs.ACTORS)
            {
                int sid = ev.AniDB_SeiyuuID;
                if (seiyuu != null && sid == seiyuu.AniDB_SeiyuuID)
                    RefreshCharacters();
            }
            ClearGUIProperty(GuiProperty.Actors_Status);
        }

        public override int GetID
        {
            get { return Constants.WindowIDs.ACTORS; }
            set { base.GetID = value; }
        }

        protected override void OnPageLoad()
        {
            base.OnPageLoad();

            if (m_Facade != null)
                m_Facade.CurrentLayout = GUIFacadeControl.Layout.Filmstrip;

            ShowCharacters();
            ClearGUIProperty(GuiProperty.Actors_Status);
            if (m_Facade!=null)
                m_Facade.Focus = true;
        }

        public override bool Init()
        {
            return this.InitSkin<GuiProperty>("Anime3_Actors.xml");
        }

        private void RefreshCharacters()
        {
            charList.Clear();
            List<JMMServerBinary.Contract_AniDB_Character> charContracts =
                new List<Contract_AniDB_Character>(JMMServerVM.Instance.clientBinaryHTTP.GetCharactersForSeiyuu(MainWindow.GlobalSeiyuuID));
            if (charContracts == null)
            {
                ClearGUIProperty(GuiProperty.Actors_Character_CharacterCount);
                return;
            }

            foreach (JMMServerBinary.Contract_AniDB_Character chr in charContracts)
                charList.Add(new AniDB_CharacterVM(chr));
            foreach (AniDB_CharacterVM aniChar in charList)
            {
                if (!string.IsNullOrEmpty(aniChar.PosterPath) && File.Exists(aniChar.PosterPath))
                {
                    string imagePath = aniChar.PosterPath;
                    bool fnd = false;
                    foreach (GUIListItem g in m_Facade.FilmstripLayout.ListItems)
                    {
                        AniDB_CharacterVM ac = g.TVTag as AniDB_CharacterVM;
                        if (ac != null)
                        {
                            if (ac.CharID == aniChar.CharID)
                            {
                                fnd = true;
                                g.IconImage = g.IconImageBig = imagePath;
                                break;
                            }
                        }
                    }
                    if (!fnd)
                    {
                        GUIListItem item = new GUIListItem(string.Empty);
                        item.IconImage = item.IconImageBig = imagePath;
                        item.TVTag = aniChar;
                        item.OnItemSelected += onFacadeItemSelected;
                        m_Facade.Add(item);
                        BaseConfig.MyAnimeLog.Write(aniChar.ToString());
                    }
                }
            }
            if (dummyCharactersExist != null)
                dummyCharactersExist.Visible = charList.Count > 0;
            SetGUIProperty(GuiProperty.Actors_Character_CharacterCount, charList.Count.ToString(Globals.Culture));
        }

        private void ShowCharacters()
        {
            GUIControl.ClearControl(GetID, m_Facade.GetID);

            BaseConfig.MyAnimeLog.Write("ActorWindow.GlobalSeiyuuID = {0}",
                MainWindow.GlobalSeiyuuID.ToString(CultureInfo.InvariantCulture));

            charList.Clear();
            seiyuu = null;

            JMMServerBinary.Contract_AniDB_Seiyuu contract =
                JMMServerVM.Instance.clientBinaryHTTP.GetAniDBSeiyuu(MainWindow.GlobalSeiyuuID);
            if (contract == null)
            {
                ClearGUIProperty(GuiProperty.Actors_Actor_Name);
                ClearGUIProperty(GuiProperty.Actors_Actor_Poster);
                ClearGUIProperty(GuiProperty.Actors_Character_CharacterCount);
                return;
            }

            seiyuu = new AniDB_SeiyuuVM(contract);


            SetGUIProperty(GuiProperty.Actors_Actor_Name, seiyuu.SeiyuuName);

            string imagePath = GUIGraphicsContext.Skin + @"\Media\MyAnime3\anime3_blankchar.png";
            if (File.Exists(seiyuu.PosterPath))
                imagePath = seiyuu.PosterPath;

            SetGUIProperty(GuiProperty.Actors_Actor_Poster, imagePath);

            List<JMMServerBinary.Contract_AniDB_Character> charContracts =
                new List<Contract_AniDB_Character>(JMMServerVM.Instance.clientBinaryHTTP.GetCharactersForSeiyuu(MainWindow.GlobalSeiyuuID));
            if (charContracts == null)
            {
                ClearGUIProperty(GuiProperty.Actors_Character_CharacterCount);
                return;
            }

            foreach (JMMServerBinary.Contract_AniDB_Character chr in charContracts)
                charList.Add(new AniDB_CharacterVM(chr));

            bool missingImages = false;
            string imagePathNoPicture = GUIGraphicsContext.Skin + @"\Media\MyAnime3\anime3_blankchar.png";
            foreach (AniDB_CharacterVM aniChar in charList)
            {
                imagePath = imagePathNoPicture;
                if (!string.IsNullOrEmpty(aniChar.PosterPath) && File.Exists(aniChar.PosterPath))
                    imagePath = aniChar.PosterPath;
                else
                    missingImages = true;

                GUIListItem item = new GUIListItem(string.Empty);
                item.IconImage = item.IconImageBig = imagePath;
                item.TVTag = aniChar;
                item.OnItemSelected += onFacadeItemSelected;
                m_Facade.Add(item);

                BaseConfig.MyAnimeLog.Write(aniChar.ToString());
            }


            if (dummyCharactersExist != null)
                dummyCharactersExist.Visible = charList.Count > 0;

            SetGUIProperty(GuiProperty.Actors_Character_CharacterCount, charList.Count.ToString(Globals.Culture));


            if (m_Facade.Count > 0)
            {
                m_Facade.SelectedListItemIndex = 0;

                AniDB_CharacterVM aniChar = m_Facade.SelectedListItem.TVTag as AniDB_CharacterVM;
                if (aniChar != null)
                {
                    SetCharacterProperties(aniChar);
                }
            }

            if (missingImages)
                GetMissingInfo();
        }


        private void onFacadeItemSelected(GUIListItem item, GUIControl parent)
        {
            // if this is not a message from the facade, exit
            if (parent != m_Facade)
                return;

            if (item == null || item.TVTag == null || !(item.TVTag is AniDB_CharacterVM))
                return;

            SetCharacterProperties((AniDB_CharacterVM)item.TVTag);
        }

        public void GetMissingInfo()
        {
            MainWindow.ServerHelper.DownloadCharacterImagesForSeiyuu(seiyuu);
            SetGUIProperty(GuiProperty.Actors_Status, Translation.RefreshingView + "...");
            m_Facade.Focus = true;
        }

        public override bool OnMessage(GUIMessage message)
        {
            switch (message.Message)
            {
                case GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS_CHANGED:
                {
                    int iControl = message.SenderControlId;

                    if (iControl ==  m_Facade.GetID)
                    {
                        GUIListItem item = m_Facade.SelectedListItem;

                        if (item == null || item.TVTag == null || !(item.TVTag is AniDB_CharacterVM))
                            return true;

                        SetCharacterProperties((AniDB_CharacterVM)item.TVTag);
                    }
                }

                    return true;

                default:
                    return base.OnMessage(message);
            }
        }

        private void SetCharacterProperties(AniDB_CharacterVM aniChar)
        {
            SetGUIProperty(GuiProperty.Actors_Character_Name, aniChar.CharName);
            SetGUIProperty(GuiProperty.Actors_Character_KanjiName, aniChar.CharKanjiName);
            SetGUIProperty(GuiProperty.Actors_Character_Description, aniChar.CharDescription);
            SetGUIProperty(GuiProperty.Actors_Character_CharType, aniChar.CharType);

            string imagePath = GUIGraphicsContext.Skin + @"\Media\MyAnime3\anime3_blankchar.png";
            if (File.Exists(aniChar.PosterPath))
                imagePath = aniChar.PosterPath;

            SetGUIProperty(GuiProperty.Actors_Character_Poster, imagePath);

            if (aniChar.Anime != null)
            {
                SetGUIProperty(GuiProperty.Actors_Series_Title, aniChar.Anime.FormattedTitle);
                SetGUIProperty(GuiProperty.Actors_Series_Poster,
                    ImageAllocator.GetAnimeImageAsFileName(aniChar.Anime, GUIFacadeControl.Layout.List));
            }
            else
            {
                ClearGUIProperty(GuiProperty.Actors_Series_Title);
                ClearGUIProperty(GuiProperty.Actors_Series_Poster);
            }
        }


        protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
        {
            MainMenu menu = new MainMenu();
            menu.Add(btnGetMissingInfo, GetMissingInfo);
            if (menu.Check(control))
                return;
            base.OnClicked(controlId, control, actionType);
        }
    }
}