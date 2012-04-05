using System;
using System.Collections.Generic;
using MediaPortal.GUI.Library;
using System.IO;
using MediaPortal.Dialogs;
using Action = MediaPortal.GUI.Library.Action;
using MyAnimePlugin3.ViewModel;

namespace MyAnimePlugin3.Windows
{
	public class ActorWindow : GUIWindow
	{
		[SkinControlAttribute(50)] protected GUIFacadeControl m_Facade = null;

		[SkinControlAttribute(1301)]
		protected GUILabelControl dummyCharactersExist = null;

		[SkinControlAttribute(2)]
		protected GUIButtonControl btnGetMissingInfo = null;

		private AniDB_SeiyuuVM seiyuu = null;
		private List<AniDB_CharacterVM> charList = new List<AniDB_CharacterVM>();

		public ActorWindow()
        {
            // get ID of windowplugin belonging to this setup
            // enter your own unique code
			GetID = Constants.WindowIDs.ACTORS;

			MainWindow.ServerHelper.GotCharacterImagesEvent += new JMMServerHelper.GotCharacterImagesEventHandler(ServerHelper_GotCharacterImagesEvent);
        }

		void ServerHelper_GotCharacterImagesEvent(Events.GotCharacterImagesEventArgs ev)
		{
			if (GUIWindowManager.ActiveWindow != Constants.WindowIDs.ACTORS) return;
			int sid = ev.AniDB_SeiyuuID;
			if (seiyuu == null || sid != seiyuu.AniDB_SeiyuuID) return;
			setGUIProperty("Status", "-");
			ShowCharacters();
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

			setGUIProperty("Status", "-");
			
			m_Facade.Focus = true;
		}

		public override bool Init()
		{
			return Load(GUIGraphicsContext.Skin + @"\Anime3_Actors.xml");
		}

		public static void setGUIProperty(string which, string value)
		{
			MediaPortal.GUI.Library.GUIPropertyManager.SetProperty("#Anime3.Actors." + which, value);
		}

		public static void clearGUIProperty(string which)
		{
			setGUIProperty(which, "-"); // String.Empty doesn't work on non-initialized fields, as a result they would display as ugly #TVSeries.bla.bla
		}

		private void ShowCharacters()
		{
			GUIControl.ClearControl(this.GetID, m_Facade.GetID);

			//if (dummyMainCharExists != null) dummyMainCharExists.Visible = false;
			//if (dummyMainActorExists != null) dummyMainActorExists.Visible = false;
			//if (dummySeriesExists != null) dummySeriesExists.Visible = false;

			clearGUIProperty("Actor.Name");
			clearGUIProperty("Actor.Poster");

			BaseConfig.MyAnimeLog.Write("ActorWindow.GlobalSeiyuuID = {0}", MainWindow.GlobalSeiyuuID.ToString());

			charList.Clear();
			seiyuu = null;

			JMMServerBinary.Contract_AniDB_Seiyuu contract = JMMServerVM.Instance.clientBinaryHTTP.GetAniDBSeiyuu(MainWindow.GlobalSeiyuuID);
			if (contract == null) return;

			seiyuu = new AniDB_SeiyuuVM(contract);


			setGUIProperty("Actor.Name", seiyuu.SeiyuuName);

			string imagePath = GUIGraphicsContext.Skin + @"\Media\MyAnime3\anime3_blankchar.png";
			if (File.Exists(seiyuu.PosterPath))
				imagePath = seiyuu.PosterPath;

			setGUIProperty("Actor.Poster", imagePath);

			List<JMMServerBinary.Contract_AniDB_Character> charContracts = JMMServerVM.Instance.clientBinaryHTTP.GetCharactersForSeiyuu(MainWindow.GlobalSeiyuuID);
			if (charContracts == null) return;

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

				GUIListItem item = new GUIListItem("");
				item.IconImage = item.IconImageBig = imagePath;
				item.TVTag = aniChar;
				item.OnItemSelected += new GUIListItem.ItemSelectedHandler(onFacadeItemSelected);
				m_Facade.Add(item);

				BaseConfig.MyAnimeLog.Write(aniChar.ToString());
			}

			
			if (dummyCharactersExist != null)
			{
				if (charList.Count > 0) dummyCharactersExist.Visible = true;
				else dummyCharactersExist.Visible = false;
			}
			setGUIProperty("Character.CharacterCount", charList.Count.ToString());


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
				MainWindow.ServerHelper.DownloadCharacterImagesForSeiyuu(seiyuu);
		}

		private void onFacadeItemSelected(GUIListItem item, GUIControl parent)
		{
			// if this is not a message from the facade, exit
			if (parent != m_Facade)
				return;

			if (item == null || item.TVTag == null || !(item.TVTag is AniDB_CharacterVM))
				return;

			AniDB_CharacterVM aniChar = item.TVTag as AniDB_CharacterVM;
			if (aniChar == null) return;

			SetCharacterProperties(aniChar);
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

		private void SetCharacterProperties(AniDB_CharacterVM aniChar)
		{

			clearGUIProperty("Character.Name");
			clearGUIProperty("Character.KanjiName");
			clearGUIProperty("Character.Description");
			clearGUIProperty("Character.CharType");
			clearGUIProperty("Character.Poster");
			clearGUIProperty("Series.Poster");
			clearGUIProperty("Series.Title");

			setGUIProperty("Character.Name", aniChar.CharName);
			setGUIProperty("Character.KanjiName", aniChar.CharKanjiName);
			setGUIProperty("Character.Description", aniChar.CharDescription);
			setGUIProperty("Character.CharType", aniChar.CharType);

			string imagePath = GUIGraphicsContext.Skin + @"\Media\MyAnime3\anime3_blankchar.png";
			if (File.Exists(aniChar.PosterPath))
				imagePath = aniChar.PosterPath;

			setGUIProperty("Character.Poster", imagePath);

			if (aniChar.Anime != null)
			{
				setGUIProperty("Series.Title", aniChar.Anime.FormattedTitle);
				setGUIProperty("Series.Poster", ImageAllocator.GetAnimeImageAsFileName(aniChar.Anime, GUIFacadeControl.Layout.List));
			}

		}

		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			if (this.btnGetMissingInfo != null && control == this.btnGetMissingInfo)
			{
				MainWindow.ServerHelper.DownloadCharacterImagesForSeiyuu(seiyuu);
				setGUIProperty("Status", "Refreshing view...");
				this.btnGetMissingInfo.IsFocused = false;
				GUIControl.FocusControl(GetID, 50);

				return;
			}

			if (MA3WindowManager.HandleWindowChangeButton(control))
				return;

			base.OnClicked(controlId, control, actionType);
		}
	}
}
