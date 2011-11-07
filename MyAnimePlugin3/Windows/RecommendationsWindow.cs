using System;
using System.Collections.Generic;

using MediaPortal.GUI.Library;

using BinaryNorthwest;

using System.IO;

using MediaPortal.Dialogs;
using MyAnimePlugin3.Downloads;
using Action = MediaPortal.GUI.Library.Action;
using MyAnimePlugin3.ViewModel;
using System.ComponentModel;

namespace MyAnimePlugin3.Windows
{
	public class RecommendationsWindow : GUIWindow
	{
		private BackgroundWorker getDataWorker = new BackgroundWorker();
		private List<RecommendationVM> colRecs = new List<RecommendationVM>();

		[SkinControlAttribute(50)] protected GUIFacadeControl m_Facade = null;

		[SkinControlAttribute(801)] protected GUIButtonControl btnViewWatch = null;
		[SkinControlAttribute(802)] protected GUIButtonControl btnViewDownload = null;
		[SkinControlAttribute(803)] protected GUIButtonControl btnGetMissingInfo = null;

		[SkinControlAttribute(920)] protected GUIButtonControl btnWindowContinueWatching = null;
		[SkinControlAttribute(921)] protected GUIButtonControl btnWindowUtilities = null;
		[SkinControlAttribute(922)] protected GUIButtonControl btnWindowCalendar = null;
		[SkinControlAttribute(923)] protected GUIButtonControl btnWindowDownloads = null;
		//[SkinControlAttribute(924)] protected GUIButtonControl btnWindowCollectionStats = null;
		//[SkinControlAttribute(925)] protected GUIButtonControl btnWindowRecommendations = null;

		[SkinControlAttribute(1461)] protected GUILabelControl dummyAnyRecords = null;
		[SkinControlAttribute(1462)] protected GUILabelControl dummyModeWatch = null;
		[SkinControlAttribute(1463)] protected GUILabelControl dummyModeDownload = null;

		public RecommendationsWindow()
		{
			// get ID of windowplugin belonging to this setup
			// enter your own unique code
			GetID = Constants.WindowIDs.RECOMMENDATIONS;

			setGUIProperty("Recommendations.Status", "-");

			getDataWorker.DoWork += new DoWorkEventHandler(getDataWorker_DoWork);
			getDataWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(getDataWorker_RunWorkerCompleted);
		}

		void getDataWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			colRecs = e.Result as List<RecommendationVM>;

			if (colRecs == null || colRecs.Count == 0)
			{
				if (dummyAnyRecords != null) dummyAnyRecords.Visible = false;
				setGUIProperty("Recommendations.Status", "No recommendations available");
				return;
			}

			if (dummyAnyRecords != null) dummyAnyRecords.Visible = true;

			foreach (RecommendationVM rec in colRecs)
			{
				GUIListItem item = new GUIListItem(rec.Recommended_DisplayName);
				//AniDB_AnimeVM anime = rec.AnimeSeries.AniDB_Anime;

				item.IconImage = item.IconImageBig = rec.Recommended_PosterPath;
				item.TVTag = rec;
				item.OnItemSelected += new GUIListItem.ItemSelectedHandler(onFacadeItemSelected);
				m_Facade.Add(item);
			}

			if (m_Facade.Count > 0)
			{
				m_Facade.SelectedListItemIndex = 0;

				RecommendationVM rec = m_Facade.SelectedListItem.TVTag as RecommendationVM;
				if (rec != null)
				{
					SetRec(rec);
				}
			}
		}

		void getDataWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			List<RecommendationVM> tempRecs = new List<RecommendationVM>();

			int recType = 1;
			if (dummyModeDownload != null && dummyModeDownload.Visible)
				recType = 2;

			List<JMMServerBinary.Contract_Recommendation> contracts =
					JMMServerVM.Instance.clientBinaryHTTP.GetRecommendations(10, JMMServerVM.Instance.CurrentUser.JMMUserID, recType);

			foreach (JMMServerBinary.Contract_Recommendation contract in contracts)
			{
				RecommendationVM rec = new RecommendationVM();
				rec.Populate(contract);
				tempRecs.Add(rec);
			}

			e.Result = tempRecs;

		}

		public override bool Init()
		{
			return Load(GUIGraphicsContext.Skin + @"\Anime3_Recommendations.xml");
		}

		public override int GetID
		{
			get { return Constants.WindowIDs.RECOMMENDATIONS; }
			set { base.GetID = value; }
		}

		protected override void OnPageLoad()
		{
			base.OnPageLoad();
			m_Facade.CurrentLayout = GUIFacadeControl.Layout.Filmstrip;
			BaseConfig.MyAnimeLog.Write("OnPageLoad: RecommendationsWindow");

			if (dummyModeWatch != null) dummyModeWatch.Visible = true;
			if (dummyModeDownload != null) dummyModeDownload.Visible = false;

			setGUIProperty("Recommendations.CurrentView", "Watch");

			LoadData();
			m_Facade.Focus = true;
		}

		public static void setGUIProperty(string which, string value)
		{
			MediaPortal.GUI.Library.GUIPropertyManager.SetProperty("#Anime3." + which, value);
		}

		public static void clearGUIProperty(string which)
		{
			setGUIProperty(which, "-"); // String.Empty doesn't work on non-initialized fields, as a result they would display as ugly #TVSeries.bla.bla
		}

		private void LoadData()
		{
			colRecs.Clear();
			GUIControl.ClearControl(this.GetID, m_Facade.GetID);
			setGUIProperty("Recommendations.Status", "Loading Data...");
			if (dummyAnyRecords != null) dummyAnyRecords.Visible = false;

			getDataWorker.RunWorkerAsync();
		}

		private void onFacadeItemSelected(GUIListItem item, GUIControl parent)
		{
			//BaseConfig.MyAnimeLog.Write("Facade Item Selected");
			// if this is not a message from the facade, exit
			if (parent != m_Facade && parent != m_Facade.FilmstripLayout)
				return;

			RecommendationVM rec = m_Facade.SelectedListItem.TVTag as RecommendationVM;
			SetRec(rec);

		}

		private void SetRec(RecommendationVM rec)
		{
			if (rec == null) return;

			clearGUIProperty("Recommendations.Rec.Title");
			clearGUIProperty("Recommendations.Rec.Description");
			clearGUIProperty("Recommendations.Rec.ApprovalRating");
			
			clearGUIProperty("Recommendations.BasedOn.Title");
			clearGUIProperty("Recommendations.BasedOn.VoteValue");


			setGUIProperty("Recommendations.Rec.Title", rec.Recommended_DisplayName);
			setGUIProperty("Recommendations.Rec.Description", rec.Recommended_Description);
			setGUIProperty("Recommendations.Rec.ApprovalRating", rec.Recommended_ApprovalRating);

			setGUIProperty("Recommendations.BasedOn.Title", rec.BasedOn_DisplayName);
			setGUIProperty("Recommendations.BasedOn.VoteValue", rec.BasedOn_VoteValueFormatted);
			setGUIProperty("Recommendations.BasedOn.Image", rec.BasedOn_PosterPath);
		}

		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			if (MA3WindowManager.HandleWindowChangeButton(control))
				return;

			if (btnViewWatch != null && control == btnViewWatch)
			{
				this.btnViewWatch.IsFocused = false;
				m_Facade.Focus = true;

				if (dummyModeWatch != null) dummyModeWatch.Visible = true;
				if (dummyModeDownload != null) dummyModeDownload.Visible = false;

				setGUIProperty("Recommendations.CurrentView", "Watch");

				LoadData();
			}

			if (btnViewDownload != null && control == btnViewDownload)
			{
				this.btnViewDownload.IsFocused = false;
				m_Facade.Focus = true;

				if (dummyModeWatch != null) dummyModeWatch.Visible = false;
				if (dummyModeDownload != null) dummyModeDownload.Visible = true;

				setGUIProperty("Recommendations.CurrentView", "Download");

				LoadData();
			}

			if (control == this.m_Facade)
			{
				// show the files if we are looking at a torrent
				GUIListItem item = m_Facade.SelectedListItem;
				if (item == null || item.TVTag == null) return;
				if (item.TVTag.GetType() == typeof(RecommendationVM))
				{
					RecommendationVM rec = item.TVTag as RecommendationVM;
					if (rec != null)
					{
						if (dummyModeWatch != null && dummyModeWatch.Visible)
						{
							JMMServerBinary.Contract_AnimeEpisode ep = JMMServerVM.Instance.clientBinaryHTTP.GetNextUnwatchedEpisode(rec.Recommended_AnimeSeries.AnimeSeriesID.Value,
							JMMServerVM.Instance.CurrentUser.JMMUserID);
							if (ep != null)
							{
								AnimeEpisodeVM aniEp = new AnimeEpisodeVM(ep);
								MainWindow.vidHandler.ResumeOrPlay(aniEp);
							}
							else
							{
								Utils.DialogMsg("Error", "Could not find the first episode");
							}
						}
					}
					
				}
			}

			base.OnClicked(controlId, control, actionType);
		}
	}
}
