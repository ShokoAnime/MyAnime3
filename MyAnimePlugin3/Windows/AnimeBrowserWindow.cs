using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.GUI.Library;

using BinaryNorthwest;

namespace MyAnimePlugin3.Windows
{
	//TODO
	public class AnimeBrowserWindow : GUIWindow
	{
		/*
		[SkinControlAttribute(50)]
		protected GUIFacadeControl m_Facade = null;

		public AnimeBrowserWindow()
		{
			// get ID of windowplugin belonging to this setup
			// enter your own unique code
			GetID = Constants.WindowIDs.BROWSER;

			//MainWindow.anidbProcessor.GotAnimeInfoEvent += new AnimePlugin.AniDBLib.GotAnimeInfoEventHandler(anidbProcessor_GotAnimeInfoEvent);
			//MainWindow.imageDownloader.ImageDownloadEvent += new MyAnimePlugin2.DataHelpers.ImageDownloader.ImageDownloadEventHandler(imageDownloader_ImageDownloadEvent);

			//MainWindow.anidbProcessor.AniDBStatusEvent += new AniDBLib.AniDBStatusEventHandler(anidbProcessor_AniDBStatusEvent);

			//setGUIProperty("Browser.Status", "-");
		}

		public override int GetID
		{
			get { return Constants.WindowIDs.BROWSER; }
			set { base.GetID = value; }
		}

		protected override void OnPageLoad()
		{
			base.OnPageLoad();

			LoadData();
		}

		private void LoadData()
		{
			RefreshResults();
		}

		private void RefreshResults()
		{
			List<AniDB_Anime> allAnime = AniDB_Anime.GetAll();
			List<SortPropOrFieldAndDirection> sortCriteria = new List<SortPropOrFieldAndDirection>();
			sortCriteria.Add(new SortPropOrFieldAndDirection("RomajiName", false, SortType.eString));

			allAnime = Sorting.MultiSort<AniDB_Anime>(allAnime, sortCriteria);

			m_Facade.Clear();
			GUIListItem item = null;

			foreach (AniDB_Anime anime in allAnime)
			{
				if (anime.RomajiName.Trim().Length == 0) continue;
				item = new GUIListItem();
				item.Label = anime.RomajiName;
				item.TVTag = anime;
				m_Facade.Add(item);
			}
		}

		public override bool Init()
		{
			return Load(GUIGraphicsContext.Skin + @"\anime3_Browser.xml");
		}
		public static void setGUIProperty(string which, string value)
		{
			MediaPortal.GUI.Library.GUIPropertyManager.SetProperty("#Anime3." + which, value);
		}

		public static void clearGUIProperty(string which)
		{
			setGUIProperty(which, "-"); // String.Empty doesn't work on non-initialized fields, as a result they would display as ugly #TVSeries.bla.bla
		}*/
	}
}
