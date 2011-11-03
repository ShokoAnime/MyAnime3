using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.GUI.Library;
using BinaryNorthwest;

namespace MyAnimePlugin3.Windows
{
	public class AnimeNewsWindow : GUIWindow
	{
		//TODO
		/*
		[SkinControlAttribute(50)]
		protected GUIFacadeControl m_Facade = null;

		[SkinControlAttribute(901)]protected GUIButtonControl btnNavLeft = null;
		[SkinControlAttribute(902)]protected GUIButtonControl btnNavRight = null;

		[SkinControlAttribute(811)]
		protected GUIButtonControl btnPreviousReview = null;

		[SkinControlAttribute(812)]
		protected GUIButtonControl btnNextReview = null;

		private AniDB_Anime mainAnime = null;
		private List<AniDB_Review> reviews = new List<AniDB_Review>();
		private int reviewIdx = 0;

		public AnimeNewsWindow()
		{
			// get ID of windowplugin belonging to this setup
			// enter your own unique code
			GetID = Constants.WindowIDs.NEWS;

			MainWindow.anidbProcessor.GotAnimeInfoEvent += new AnimePlugin.AniDBLib.GotAnimeInfoEventHandler(anidbProcessor_GotAnimeInfoEvent);
			MainWindow.imageDownloader.ImageDownloadEvent += new MyAnimePlugin3.DataHelpers.ImageDownloaderOld.ImageDownloadEventHandler(imageDownloader_ImageDownloadEvent);
			MainWindow.anidbProcessor.GotReviewEvent += new AnimePlugin.AniDBLib.GotReviewEventHandler(anidbProcessor_GotReviewEvent);
			MainWindow.anidbProcessor.AniDBStatusEvent += new AniDBLib.AniDBStatusEventHandler(anidbProcessor_AniDBStatusEvent);

			setGUIProperty("News.Status", "-");
		}

		void anidbProcessor_GotReviewEvent(AnimePlugin.GotReviewEventArgs ev)
		{
			if (GUIWindowManager.ActiveWindow != Constants.WindowIDs.NEWS) return;

			int reviewID = ev.reviewID;
		    if (mainAnime == null)
                return;
		    bool found = false;
            foreach(CrossRef_Series_Review rev in mainAnime.ReviewList)
            {
                if (rev.ReviewID==reviewID)
                {
                    found = true;
                    break;
                }
            }
            if (!found) return;
    		LoadReviews(true);
			if (reviews.Count == 1) ShowReview(0);
		}

		void anidbProcessor_AniDBStatusEvent(AniDBStatusEventArgs ev)
		{
			if (GUIWindowManager.ActiveWindow != Constants.WindowIDs.NEWS) return;

			try
			{
				string cmdDesc = "";
				switch (ev.evType)
				{
					case enHelperActivityType.GettingAnimeInfo:
						cmdDesc = "Getting anime info: " + ev.Status; break;
					case enHelperActivityType.GettingAnimeHTTP:
						cmdDesc = "Getting http anime info: " + ev.Status; break;
					case enHelperActivityType.GettingReview:
						cmdDesc = "Getting review: " + ev.Status; break;
					case enHelperActivityType.LoggingIn:
						cmdDesc = "Logging in..."; break;
					case enHelperActivityType.LoggingOut:
						cmdDesc = "Logging out..."; break;
					default:
						cmdDesc = ""; break;
				}

				setGUIProperty("News.Status", cmdDesc);
			}
			catch { }
		}

		void imageDownloader_ImageDownloadEvent(DataHelpers.ImageDownloadEventArgsOld ev)
		{
			if (GUIWindowManager.ActiveWindow != Constants.WindowIDs.NEWS) return;

            if (ev.Req.ImageType == DataHelpers.AnimeImageType.AniDB_Cover)
			{
				int aid = -1;
				if (int.TryParse(ev.Req.Identifier, out aid))
				{
				}
			}

		}

		void anidbProcessor_GotAnimeInfoEvent(GotAnimeInfoEventArgs ev)
		{
			if (GUIWindowManager.ActiveWindow != Constants.WindowIDs.NEWS) return;

			int aid = ev.animeID;

			if (mainAnime != null && aid == mainAnime.AnimeID)
			{
				mainAnime.Load(aid);

				foreach (CrossRef_Series_Review rev in mainAnime.ReviewList)
					MainWindow.anidbProcessor.UpdateReview(rev.ReviewID, false);
			}
		}

		public override int GetID
		{
			get { return Constants.WindowIDs.NEWS; }
			set { base.GetID = value; }
		}

		protected override void OnPageLoad()
		{
			base.OnPageLoad();

			reviews = new List<AniDB_Review>();
			reviewIdx = 0;

			setGUIProperty("News.ReviewText", "-");
			setGUIProperty("News.Author", "-");
			setGUIProperty("News.RatingAverage", "-");
			setGUIProperty("News.RatingAnimation", "-");
			setGUIProperty("News.RatingSound", "-");
			setGUIProperty("News.RatingStory", "-");
			setGUIProperty("News.RatingCharacter", "-");
			setGUIProperty("News.RatingValue", "-");
			setGUIProperty("News.RatingEnjoyment", "-");

			LoadData();
			LoadFanart();
		}

		private void LoadData()
		{
			clearGUIProperty("News.ReviewText");
			setGUIProperty("News.CurrentReview", "0");
			setGUIProperty("News.ReviewCount", "0");

			mainAnime = new AniDB_Anime();
			if (!mainAnime.Load(MainWindow.ImagesAniDBID))
			{
				mainAnime = null;
				return;
			}

			// check if the existing record has any review id's
			if (mainAnime.ReviewList.Count == 0)
			{
				MainWindow.anidbProcessor.UpdateAnimeInfo(mainAnime.AnimeID, true, false);
			}
			else
			{
				LoadReviews(false);
				if (reviews.Count > 0)
				{
					ShowReview(0);
				}
			}
		}

		private void LoadReviews(bool dbOnly)
		{
			reviews.Clear();
			foreach (CrossRef_Series_Review re in mainAnime.ReviewList)
			{
				AniDB_Review rev = new AniDB_Review();
				if (rev.Load(re.ReviewID))
					reviews.Add(rev);
				else
				{
                    if (!dbOnly) MainWindow.anidbProcessor.UpdateReview(re.ReviewID, false);
				}
			}
			setGUIProperty("News.ReviewCount", reviews.Count.ToString());

			reviewIdx = 0;
		}

		private void LoadFanart()
		{
			clearGUIProperty("News.Fanart");
			Fanart fanart = null;
			string title;
			if (MainWindow.ImagesListlevel == Listlevel.Group)
			{
				// get the group
				AnimeGroup grp = new AnimeGroup();
				if (grp.Load(MainWindow.ImagesParentID))
					fanart = new Fanart(grp);
				title = grp.GroupName;
			}
			else
			{
				AnimeSeries ser = new AnimeSeries();
				if (ser.Load(MainWindow.ImagesParentID))
					fanart = new Fanart(ser);
				title = ser.SeriesName;
			}

			if (fanart == null) return;

			if (fanart.FileName.Length > 0)
			{
				setGUIProperty("News.Fanart", fanart.FileName);
			}
		}

		private void ShowReview(int idx)
		{
			reviewIdx = idx;
			AniDB_Review rev = reviews[idx];
			setGUIProperty("News.CurrentReview", (reviewIdx + 1).ToString());

			int totalRating = rev.RatingAnimation + rev.RatingSound + rev.RatingStory +
				rev.RatingCharacter + rev.RatingValue + rev.RatingEnjoyment;

			double dblRating = (double)totalRating / (double)6;

		    setGUIProperty("News.ReviewText", rev.ParsedReviewText);
            setGUIProperty("News.Author", rev.AuthorID.ToString());
			setGUIProperty("News.RatingAverage", Utils.FormatAniDBRating(dblRating));
			setGUIProperty("News.RatingAnimation", Utils.FormatAniDBRating(rev.RatingAnimation));
			setGUIProperty("News.RatingSound", Utils.FormatAniDBRating(rev.RatingSound));
			setGUIProperty("News.RatingStory", Utils.FormatAniDBRating(rev.RatingStory));
			setGUIProperty("News.RatingCharacter", Utils.FormatAniDBRating(rev.RatingCharacter));
			setGUIProperty("News.RatingValue", Utils.FormatAniDBRating(rev.RatingValue));
			setGUIProperty("News.RatingEnjoyment", Utils.FormatAniDBRating(rev.RatingEnjoyment));
		}

		public override bool Init()
		{
			return Load(GUIGraphicsContext.Skin + @"\anime3_News.xml");
		}
		public static void setGUIProperty(string which, string value)
		{
			MediaPortal.GUI.Library.GUIPropertyManager.SetProperty("#Anime3." + which, value);
		}

		public static void clearGUIProperty(string which)
		{
			setGUIProperty(which, "-"); // String.Empty doesn't work on non-initialized fields, as a result they would display as ugly #TVSeries.bla.bla
		}

		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			if (control == this.btnNavLeft)
			{
				GUIWindowManager.CloseCurrentWindow();
				GUIWindowManager.ActivateWindow(Constants.WindowIDs.RELATIONS, false);

				return;
			}

			if (control == this.btnNavRight)
			{
				GUIWindowManager.CloseCurrentWindow();
				GUIWindowManager.ActivateWindow(Constants.WindowIDs.CHARACTERS, false);

				return;
			}

			if (control == this.btnNextReview)
			{
				if (reviews.Count == 0) return;
				if (reviewIdx + 1 == reviews.Count)
					reviewIdx = 0;
				else
					reviewIdx++;

				ShowReview(reviewIdx);

				return;
			}

			if (control == this.btnPreviousReview)
			{
				if (reviews.Count == 0) return;
				if (reviewIdx == 0)
					reviewIdx = reviews.Count - 1;
				else
					reviewIdx--;

				ShowReview(reviewIdx);

				return;
			}

			MainWindow.NotificationsManager.ButtonPress(control);

			base.OnClicked(controlId, control, actionType);
		}*/

	}
}
