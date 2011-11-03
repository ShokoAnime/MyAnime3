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

        [SkinControlAttribute(5681)] protected GUILabelControl dummyPosterMainChar = null;
        [SkinControlAttribute(5682)] protected GUILabelControl dummyPosterMainActor = null;
        [SkinControlAttribute(5684)] protected GUILabelControl dummyPosterSeries = null;

        [SkinControlAttribute(5691)] protected GUILabelControl dummyMainCharExists = null;
        [SkinControlAttribute(5692)] protected GUILabelControl dummyMainActorExists = null;
        [SkinControlAttribute(5694)] protected GUILabelControl dummySeriesExists = null;

		private List<AniDB_CharacterVM> charList = new List<AniDB_CharacterVM>();
        AniDB_AnimeVM mainAnime = null;
		AnimeSeriesVM serMain = null;

        public CharWindow()
        {
            // get ID of windowplugin belonging to this setup
            // enter your own unique code
            GetID = Constants.WindowIDs.CHARACTERS;

			MainWindow.ServerHelper.GotCharacterCreatorImagesEvent += new JMMServerHelper.GotCharacterCreatorImagesEventHandler(ServerHelper_GotCharacterCreatorImagesEvent);
        }

		void ServerHelper_GotCharacterCreatorImagesEvent(Events.GotCharacterCreatorImagesEventArgs ev)
		{
			if (GUIWindowManager.ActiveWindow != Constants.WindowIDs.CHARACTERS) return;
			int aid = ev.AnimeID;
			if (mainAnime == null || aid != mainAnime.AnimeID) return;
			setGUIProperty("Character.Status", "-");
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

            setGUIProperty("Character.Status", "-");

			m_Facade.Focus = true;
        }

        private void ShowCharacters()
        {
            GUIControl.ClearControl(this.GetID, m_Facade.GetID);

            if (dummyMainCharExists != null) dummyMainCharExists.Visible = false;
            if (dummyMainActorExists != null) dummyMainActorExists.Visible = false;
            if (dummySeriesExists != null) dummySeriesExists.Visible = false;

            clearGUIProperty("Title");
            clearGUIProperty("Character.Name");
            clearGUIProperty("Character.KanjiName");
            clearGUIProperty("Actor.Name");
            clearGUIProperty("Actor.KanjiName");
            clearGUIProperty("Character.CharacterCount");

			BaseConfig.MyAnimeLog.Write("CharWindow.GlobalSeriesID = {0}", MainWindow.GlobalSeriesID.ToString());

			charList.Clear();

			mainAnime = null;
			serMain = null;

			serMain = JMMServerHelper.GetSeries(MainWindow.GlobalSeriesID);
			if (serMain != null)
				mainAnime = serMain.AniDB_Anime;
			else
				return;

			if (mainAnime == null)
				return;

			setGUIProperty("Character.Main.Title", mainAnime.FormattedTitle);

            charList = mainAnime.Characters;
			if (dummyCharactersExist != null)
			{
				if (charList.Count > 0) dummyCharactersExist.Visible = true;
				else dummyCharactersExist.Visible = false;
			}
            setGUIProperty("Character.CharacterCount", charList.Count.ToString());

			setGUIProperty("Title", serMain.SeriesName);

			string imagePathNoPicture = GUIGraphicsContext.Skin + @"\Media\MyAnime3\anime3_blankchar.png";

			foreach (AniDB_CharacterVM aniChar in charList)
            {
				string imagePath = imagePathNoPicture;
				if (!string.IsNullOrEmpty(aniChar.PosterPath) && File.Exists(aniChar.PosterPath))
					imagePath = aniChar.PosterPath;

				GUIListItem item = new GUIListItem(aniChar.CharName);
				item.IconImage = item.IconImageBig = imagePath;
				item.TVTag = aniChar;
				item.OnItemSelected += new GUIListItem.ItemSelectedHandler(onFacadeItemSelected);
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

            clearGUIProperty("Character.Name");
            clearGUIProperty("Character.KanjiName");
			clearGUIProperty("Character.Description");
			clearGUIProperty("Character.CharType");
            clearGUIProperty("Character.PosterMainChar");
            clearGUIProperty("Character.PosterSeries");

            setGUIProperty("Character.Name", aniChar.CharName);
            setGUIProperty("Character.KanjiName", aniChar.CharKanjiName);
			setGUIProperty("Character.Description", aniChar.CharDescription);
			setGUIProperty("Character.CharType", aniChar.CharType);

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

            setGUIProperty("Character.PosterMainChar", imagePath);

            SetActorProperties(aniChar);
        }

        private void SetActorProperties(AniDB_CharacterVM aniChar)
        {
            if (dummyMainActorExists != null) dummyMainActorExists.Visible = false;
            if (dummySeriesExists != null) dummySeriesExists.Visible = false;

            clearGUIProperty("Actor.Name");
            clearGUIProperty("Actor.KanjiName");
            clearGUIProperty("Character.PosterMainActor");
            clearGUIProperty("Character.PosterSeries");

            clearGUIProperty("Character.SeriesName");
            clearGUIProperty("Character.SeriesCharacter");
            clearGUIProperty("Character.SeriesKanjiCharacter");

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
                catch { }

                setGUIProperty("Character.PosterMainActor", imagePath);
            }
            else
            {
                if (dummyMainActorExists != null) dummyMainActorExists.Visible = true;

                setGUIProperty("Actor.Name", actor.SeiyuuName);
				if (File.Exists(actor.PosterPath))
					imagePath = actor.PosterPath;

                try
                {
                    System.Drawing.Image theImage = System.Drawing.Image.FromFile(imagePath);
                    float width = theImage.PhysicalDimension.Width;
                    float height = theImage.PhysicalDimension.Height;

                    if (dummyPosterMainActor != null) dummyPosterMainActor.Visible = height > width;
                }
                catch { }

                setGUIProperty("Character.PosterMainActor", imagePath);
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
			if (this.btnGetMissingInfo != null && control == this.btnGetMissingInfo)
			{
				JMMServerVM.Instance.clientBinaryHTTP.UpdateAnimeData(mainAnime.AnimeID);
				setGUIProperty("Character.Status", "Request sent to server, please refresh view...");

				this.btnGetMissingInfo.IsFocused = false;
				GUIControl.FocusControl(GetID, 50);

				return;
			}

			if (this.btnRefreshView != null && control == this.btnRefreshView)
			{
				MainWindow.ServerHelper.DownloadCharacterCreatorImages(mainAnime);
				setGUIProperty("Character.Status", "Refreshing view...");
				this.btnRefreshView.IsFocused = false;
				GUIControl.FocusControl(GetID, 50);

				return;
			}

			if (MA3WindowManager.HandleWindowChangeButton(control))
				return;

            base.OnClicked(controlId, control, actionType);
        }

        protected override void OnShowContextMenu()
        {
            try
            {
               /* GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
                if (dlg == null)
                    return;

                dlg.Reset();
                dlg.SetHeading(mainAnime.RomajiName);
                dlg.Add("Download Character Data From AniDB");
                dlg.DoModal(GUIWindowManager.ActiveWindow);

                switch (dlg.SelectedId)
                {
                    case 1:

                        MainWindow.anidbProcessor.UpdateAnimeInfo(mainAnime.AnimeID, true, false);

                        break;
                }*/
            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write("Error in menu: {0}", ex);
            }
        }

        public override bool OnMessage(GUIMessage message)
        {
            switch (message.Message)
            {
                case GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS_CHANGED:
                    {
                        int iControl = message.SenderControlId;

                        if (iControl == (int)m_Facade.GetID)
                        {
                            GUIListItem item = m_Facade.SelectedListItem;

                            if (item == null || item.TVTag == null || !(item.TVTag is AniDB_CharacterVM))
                                return true;

							AniDB_CharacterVM aniChar = item.TVTag as AniDB_CharacterVM;
                            if (aniChar == null) return true;

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
            if (aniChar == null) return;

            SetCharacterProperties(aniChar);
        }

        public override bool Init()
        {
            return Load(GUIGraphicsContext.Skin + @"\Anime3_Char.xml");
        }
        public static void setGUIProperty(string which, string value)
        {
            MediaPortal.GUI.Library.GUIPropertyManager.SetProperty("#Anime3." + which, value);
        }

        public static void clearGUIProperty(string which)
        {
            setGUIProperty(which, "-"); // String.Empty doesn't work on non-initialized fields, as a result they would display as ugly #TVSeries.bla.bla
        }

    }
}
