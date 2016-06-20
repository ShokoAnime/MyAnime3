using System;
using System.Collections.Generic;

using MediaPortal.GUI.Library;

using System.IO;


using MediaPortal.Dialogs;
using Action = MediaPortal.GUI.Library.Action;
using MyAnimePlugin3.ViewModel;

namespace MyAnimePlugin3.Windows
{
    public class CharWindow : GUIWindow
    {
		[SkinControlAttribute(50)] protected GUIFacadeControl m_Facade = null;

		[SkinControlAttribute(1301)] protected GUILabelControl dummyCharactersExist = null;

		[SkinControlAttribute(2)]
		protected GUIButtonControl btnGetMissingInfo = null;
		[SkinControlAttribute(3)]
		protected GUIButtonControl btnRefreshView = null;

		[SkinControlAttribute(910)] protected GUIButtonControl btnAnimeInfo = null;
		//[SkinControlAttribute(911)] protected GUIButtonControl btnAnimeCharacters = null;
		[SkinControlAttribute(912)] protected GUIButtonControl btnAnimeRelations = null;
		[SkinControlAttribute(913)] protected GUIButtonControl btnAnimeFanart = null;
		[SkinControlAttribute(914)] protected GUIButtonControl btnAnimePosters = null;
		[SkinControlAttribute(915)] protected GUIButtonControl btnAnimeWideBanners = null;

		[SkinControlAttribute(930)] protected GUIButtonControl btnSeiyuu = null;

        [SkinControlAttribute(5681)] protected GUILabelControl dummyPosterMainChar = null;
        [SkinControlAttribute(5682)] protected GUILabelControl dummyPosterMainActor = null;
        [SkinControlAttribute(5684)] protected GUILabelControl dummyPosterSeries = null;

        [SkinControlAttribute(5691)] protected GUILabelControl dummyMainCharExists = null;
        [SkinControlAttribute(5692)] protected GUILabelControl dummyMainActorExists = null;
        [SkinControlAttribute(5694)] protected GUILabelControl dummySeriesExists = null;

		private List<AniDB_CharacterVM> charList = new List<AniDB_CharacterVM>();
        AniDB_AnimeVM mainAnime = null;
		AnimeSeriesVM serMain = null;


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
        public void SetGUIProperty(GuiProperty which, string value) { this.SetGUIProperty(which.ToString(), value); }
        public void ClearGUIProperty(GuiProperty which) { this.ClearGUIProperty(which.ToString()); }


        public CharWindow()
        {
            // get ID of windowplugin belonging to this setup
            // enter your own unique code
            GetID = Constants.WindowIDs.CHARACTERS;

            MainWindow.ServerHelper.GotCharacterCreatorImagesEvent += ServerHelper_GotCharacterCreatorImagesEvent;
        }

        void ServerHelper_GotCharacterCreatorImagesEvent(Events.GotCharacterCreatorImagesEventArgs ev)
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

			if (m_Facade != null)
				m_Facade.CurrentLayout = GUIFacadeControl.Layout.Filmstrip;

            ShowCharacters();
            ClearGUIProperty(GuiProperty.Character_Status);
            if (m_Facade != null)
    			m_Facade.Focus = true;
        }

        private void ShowCharacters()
        {
            GUIControl.ClearControl(this.GetID, m_Facade.GetID);

            if (dummyMainCharExists != null) dummyMainCharExists.Visible = false;
            if (dummyMainActorExists != null) dummyMainActorExists.Visible = false;
            if (dummySeriesExists != null) dummySeriesExists.Visible = false;

			BaseConfig.MyAnimeLog.Write("CharWindow.GlobalSeriesID = {0}", MainWindow.GlobalSeriesID.ToString());

			charList.Clear();

			mainAnime = null;
			serMain = null;
            if ((serMain == null) || (serMain.AniDB_Anime == null))
            {
                ClearGUIProperty(GuiProperty.Title);
                ClearGUIProperty(GuiProperty.Character_Name);
                ClearGUIProperty(GuiProperty.Character_KanjiName);
                ClearGUIProperty(GuiProperty.Actor_Name);
                ClearGUIProperty(GuiProperty.Actor_KanjiName);
                ClearGUIProperty(GuiProperty.Character_CharacterCount);
                return;
            }
            serMain = JMMServerHelper.GetSeries(MainWindow.GlobalSeriesID);
			if (serMain != null)
				mainAnime = serMain.AniDB_Anime;
			else
				return;

			if (mainAnime == null)
				return;
            SetGUIProperty(GuiProperty.Character_Main_Title, mainAnime.FormattedTitle);

            charList = mainAnime.Characters;
			if (dummyCharactersExist != null)
			{
                dummyCharactersExist.Visible = charList.Count > 0;
            }
            SetGUIProperty(GuiProperty.Character_CharacterCount, charList.Count.ToString(Globals.Culture));
            SetGUIProperty(GuiProperty.Title, serMain.SeriesName);

            string imagePathNoPicture = GUIGraphicsContext.Skin + @"\Media\MyAnime3\anime3_blankchar.png";

			foreach (AniDB_CharacterVM aniChar in charList)
            {
				string imagePath = imagePathNoPicture;
				if (!string.IsNullOrEmpty(aniChar.PosterPath) && File.Exists(aniChar.PosterPath))
					imagePath = aniChar.PosterPath;

				GUIListItem item = new GUIListItem("");
				item.IconImage = item.IconImageBig = imagePath;
				item.TVTag = aniChar;
				item.OnItemSelected += onFacadeItemSelected;
				m_Facade.Add(item);
            }

            if (m_Facade.Count > 0)
            {
                m_Facade.SelectedListItemIndex = 0;

				AniDB_CharacterVM aniChar = m_Facade.SelectedListItem.TVTag as AniDB_CharacterVM;
                if (aniChar != null)
                {
                    SetCharacterProperties(aniChar);
                }
            }
        }

		private void SetCharacterProperties(AniDB_CharacterVM aniChar)
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
			if (File.Exists(aniChar.PosterPath))
                imagePath = aniChar.PosterPath;

            try
            {
                System.Drawing.Image theImage = System.Drawing.Image.FromFile(imagePath);
                float width = theImage.PhysicalDimension.Width;
                float height = theImage.PhysicalDimension.Height;

                if (dummyPosterMainChar != null) dummyPosterMainChar.Visible = height > width;
            }
            catch { }

            SetGUIProperty(GuiProperty.Character_PosterMainChar, imagePath);

            SetActorProperties(aniChar);
        }

        private void SetActorProperties(AniDB_CharacterVM aniChar)
        {
            if (dummyMainActorExists != null) dummyMainActorExists.Visible = false;
            if (dummySeriesExists != null) dummySeriesExists.Visible = false;

            string imagePath = GUIGraphicsContext.Skin + @"\Media\MyAnime3\anime3_blankchar.png";

            // get the actor from the character
            AniDB_SeiyuuVM actor = aniChar.Creator;

            if (actor == null)
            {
                try
                {
                    System.Drawing.Image theImage = System.Drawing.Image.FromFile(imagePath);
                    float width = theImage.PhysicalDimension.Width;
                    float height = theImage.PhysicalDimension.Height;

                    if (dummyPosterMainActor != null) dummyPosterMainActor.Visible = height > width;
                }
                // ReSharper disable EmptyGeneralCatchClause
                catch { }
                // ReSharper restore EmptyGeneralCatchClause

                SetGUIProperty(GuiProperty.Character_PosterMainActor, imagePath);
                ClearGUIProperty(GuiProperty.Actor_Name);
            }
            else
            {
                MainWindow.GlobalSeiyuuID = actor.AniDB_SeiyuuID;

                if (dummyMainActorExists != null) dummyMainActorExists.Visible = true;

                SetGUIProperty(GuiProperty.Actor_Name, actor.SeiyuuName);
                if (File.Exists(actor.PosterPath))
                    imagePath = actor.PosterPath;

                try
                {
                    System.Drawing.Image theImage = System.Drawing.Image.FromFile(imagePath);
                    float width = theImage.PhysicalDimension.Width;
                    float height = theImage.PhysicalDimension.Height;

                    if (dummyPosterMainActor != null) dummyPosterMainActor.Visible = height > width;
                }
                // ReSharper disable EmptyGeneralCatchClause
                catch { }
                // ReSharper restore EmptyGeneralCatchClause

                SetGUIProperty(GuiProperty.Character_PosterMainActor, imagePath);
            }
        }


        protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
        {
            MainMenu menu = new MainMenu();
            menu.Add(btnGetMissingInfo, () =>
            {
                JMMServerVM.Instance.clientBinaryHTTP.UpdateAnimeData(mainAnime.AnimeID);
                SetGUIProperty(GuiProperty.Character_Status, Translation.RequestSendToServerPleaseRefresh + "...");
                m_Facade.Focus = true;
            });
            menu.Add(btnRefreshView, () =>
            {
                JMMServerVM.Instance.clientBinaryHTTP.UpdateAnimeData(mainAnime.AnimeID);
                SetGUIProperty(GuiProperty.Character_Status, Translation.RequestSendToServerPleaseRefresh + "...");
                m_Facade.Focus = true;
            });
            menu.Add(btnSeiyuu, () => GUIWindowManager.ActivateWindow(Constants.WindowIDs.ACTORS, false));
            if (menu.Check(control))
                return;

            try
            {
                if (actionType == Action.ActionType.ACTION_SELECT_ITEM)
                {
                    if (control == m_Facade)
                    {
                        GUIListItem item = m_Facade.SelectedListItem;

                        if (item == null || item.TVTag == null || !(item.TVTag is AniDB_CharacterVM))
                            return;

                        AniDB_CharacterVM aniChar = item.TVTag as AniDB_CharacterVM;
                        AniDB_SeiyuuVM actor = aniChar.Creator;
                        MainWindow.GlobalSeiyuuID = actor.AniDB_SeiyuuID;
                        GUIWindowManager.ActivateWindow(Constants.WindowIDs.ACTORS, false);
                    }
                }
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch { }
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

                            if (item == null || item.TVTag == null || !(item.TVTag is AniDB_CharacterVM))
                                return true;

                            AniDB_CharacterVM aniChar = item.TVTag as AniDB_CharacterVM;
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

            if (item == null || item.TVTag == null || !(item.TVTag is AniDB_CharacterVM))
                return;

            AniDB_CharacterVM aniChar = item.TVTag as AniDB_CharacterVM;

            SetCharacterProperties(aniChar);
        }

        public override bool Init()
        {
            return this.InitSkin<GuiProperty>("Anime3_Char.xml");
        }

    }
}
