using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MediaPortal.GUI.Library;

namespace MyAnimePlugin3
{
	public class MA3WindowManager
	{
		public const int AnimeInfo = 910;
		public const int AnimeCharacters = 911;
		public const int AnimeRelations = 912;
		public const int AnimeFanart = 913;
		public const int AnimePosters = 914;
		public const int AnimeWideBanners = 915;
		public const int AnimeSimilar = 916;

		public const int ContinueWatching = 920;
		public const int Utilities = 921;
		public const int Calendar = 922;
		public const int Downloads = 923;
		public const int CollectionStats = 924;
		public const int Recommendations = 925;

		public static bool HandleWindowChangeButton(GUIControl control)
		{
			if (control == null) return false;

			int controlID = control.GetID;

			switch (controlID)
			{
				case AnimeInfo:
					GUIWindowManager.CloseCurrentWindow();
					GUIWindowManager.ActivateWindow(Constants.WindowIDs.ANIMEINFO, false);
					return true;

				case AnimeCharacters:
					GUIWindowManager.CloseCurrentWindow();
					GUIWindowManager.ActivateWindow(Constants.WindowIDs.CHARACTERS, false);
					return true;

				case AnimeRelations:
					GUIWindowManager.CloseCurrentWindow();
					GUIWindowManager.ActivateWindow(Constants.WindowIDs.RELATIONS, false);
					return true;

				case AnimeFanart:
					GUIWindowManager.CloseCurrentWindow();
					GUIWindowManager.ActivateWindow(Constants.WindowIDs.FANART, false);
					return true;

				case AnimePosters:
					GUIWindowManager.CloseCurrentWindow();
					GUIWindowManager.ActivateWindow(Constants.WindowIDs.POSTERS, false);
					return true;

				case AnimeWideBanners:
					GUIWindowManager.CloseCurrentWindow();
					GUIWindowManager.ActivateWindow(Constants.WindowIDs.WIDEBANNERS, false);
					return true;

				case AnimeSimilar:
					GUIWindowManager.CloseCurrentWindow();
					GUIWindowManager.ActivateWindow(Constants.WindowIDs.SIMILAR, false);
					return true;

				case ContinueWatching:
					GUIWindowManager.CloseCurrentWindow();
					GUIWindowManager.ActivateWindow(Constants.WindowIDs.WATCHING, false);
					return true;

				case Utilities:
					GUIWindowManager.CloseCurrentWindow();
					GUIWindowManager.ActivateWindow(Constants.WindowIDs.ADMIN, false);
					return true;

				case Calendar:
					GUIWindowManager.CloseCurrentWindow();
					GUIWindowManager.ActivateWindow(Constants.WindowIDs.CALENDAR, false);
					return true;

				case Downloads:
					GUIWindowManager.CloseCurrentWindow();
					GUIWindowManager.ActivateWindow(Constants.WindowIDs.DOWNLOADS, false);
					return true;

				case CollectionStats:
					GUIWindowManager.CloseCurrentWindow();
					GUIWindowManager.ActivateWindow(Constants.WindowIDs.COLLECTION, false);
					return true;

				case Recommendations:
					GUIWindowManager.CloseCurrentWindow();
					GUIWindowManager.ActivateWindow(Constants.WindowIDs.RECOMMENDATIONS, false);
					return true;
			}

			return false;
		}
	}
}
