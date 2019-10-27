using System;
using System.Collections.Generic;

using MediaPortal.GUI.Library;
using System.ComponentModel;
using Shoko.Commons.Extensions;
using Shoko.Models.Client;
using Shoko.MyAnime3.ViewModel;
using Shoko.MyAnime3.ViewModel.Helpers;
using Shoko.MyAnime3.ViewModel.Server;

namespace Shoko.MyAnime3.Windows
{
	public class RecommendationsWindow : GUIWindow
	{
		private BackgroundWorker getDataWorker = new BackgroundWorker();
		private List<VM_Recommendation> colRecs = new List<VM_Recommendation>();

		[SkinControl(50)] protected GUIFacadeControl m_Facade = null;

		[SkinControl(801)] protected GUIButtonControl btnViewWatch = null;
		[SkinControl(802)] protected GUIButtonControl btnViewDownload = null;
		[SkinControl(803)] protected GUIButtonControl btnGetMissingInfo = null;

		[SkinControl(920)] protected GUIButtonControl btnWindowContinueWatching = null;
		[SkinControl(921)] protected GUIButtonControl btnWindowUtilities = null;
		[SkinControl(922)] protected GUIButtonControl btnWindowCalendar = null;

		//[SkinControlAttribute(924)] protected GUIButtonControl btnWindowCollectionStats = null;
		//[SkinControlAttribute(925)] protected GUIButtonControl btnWindowRecommendations = null;

		[SkinControl(1461)] protected GUILabelControl dummyAnyRecords = null;
		[SkinControl(1462)] protected GUILabelControl dummyModeWatch = null;


        public enum GuiProperty
        {
            Recommendations_Status,
            Recommendations_CurrentView,
            Recommendations_Rec_Title,
            Recommendations_Rec_Description,
            Recommendations_Rec_ApprovalRating,
            Recommendations_Rec_Image,
            Recommendations_Rec_AniDBRating,
            Recommendations_BasedOn_Title,
            Recommendations_BasedOn_VoteValue,
            Recommendations_BasedOn_Image,
            Recommendations_Button_Watch_Texture,
            Recommendations_Button_Download_Texture,
            Fanart_1,
            Fanart_2

        }
        public void SetGUIProperty(GuiProperty which, string value) { this.SetGUIProperty(which.ToString(), value); }
        public void ClearGUIProperty(GuiProperty which) { this.ClearGUIProperty(which.ToString()); }
        public string GetPropertyName(GuiProperty which) { return this.GetPropertyName(which.ToString()); }



        public RecommendationsWindow()
		{
			// get ID of windowplugin belonging to this setup
			// enter your own unique code
			GetID = Constants.WindowIDs.RECOMMENDATIONS;

			getDataWorker.DoWork += new DoWorkEventHandler(getDataWorker_DoWork);
			getDataWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(getDataWorker_RunWorkerCompleted);

			MainWindow.ServerHelper.GotRecommendedAnimeEvent += new ShokoServerHelper.GotRecommendedAnimeEventHandler(ServerHelper_GotRecommendedAnimeEvent);
		}

		void ServerHelper_GotRecommendedAnimeEvent(Events.GotAnimeForRecommendedEventArgs ev)
		{
			if (GUIWindowManager.ActiveWindow != Constants.WindowIDs.RECOMMENDATIONS) return;
			ClearGUIProperty(GuiProperty.Recommendations_Status);
			LoadData();
		}

		void getDataWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			colRecs = e.Result as List<VM_Recommendation>;

			if (colRecs == null || colRecs.Count == 0)
			{
				if (dummyAnyRecords != null) dummyAnyRecords.Visible = false;
			    SetGUIProperty(GuiProperty.Recommendations_Status, Translation.NoRecommendationsAvailable);
                return;
			}

			if (dummyAnyRecords != null) dummyAnyRecords.Visible = true;

			foreach (VM_Recommendation rec in colRecs)
			{
				GUIListItem item = new GUIListItem("");
				//AniDB_AnimeVM anime = rec.AnimeSeries.AniDB_Anime;

				item.IconImage = item.IconImageBig = rec.Recommended_PosterPath;
				item.TVTag = rec;
				item.OnItemSelected += new GUIListItem.ItemSelectedHandler(onFacadeItemSelected);
				m_Facade.Add(item);
			}

			if (m_Facade.Count > 0)
			{
				m_Facade.SelectedListItemIndex = 0;

				VM_Recommendation rec = m_Facade.SelectedListItem.TVTag as VM_Recommendation;
				if (rec != null)
				{
					SetRec(rec);
				}
			}
		}

		void getDataWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			List<VM_Recommendation> tempRecs = new List<VM_Recommendation>();

			int recType = 1;


		    e.Result = VM_ShokoServer.Instance.ShokoServices.GetRecommendations(20, VM_ShokoServer.Instance.CurrentUser.JMMUserID, recType).CastList<VM_Recommendation>();

		}

		public override bool Init()
		{
            bool res = this.InitSkin<GuiProperty>("Anime3_Recommendations.xml");
            SetGUIProperty(GuiProperty.Fanart_1, "hover_My_Anime3.jpg");
            SetGUIProperty(GuiProperty.Fanart_2, "hover_My_Anime3.jpg");
            return res;
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


		    SetGUIProperty(GuiProperty.Recommendations_CurrentView, Translation.Watch);
            
			LoadData();
			m_Facade.Focus = true;
		}


		private void LoadData()
		{
			colRecs.Clear();
			GUIControl.ClearControl(this.GetID, m_Facade.GetID);
			SetGUIProperty(GuiProperty.Recommendations_Status, Translation.LoadingData+"...");
			if (dummyAnyRecords != null) dummyAnyRecords.Visible = false;

			getDataWorker.RunWorkerAsync();
		}

		private void onFacadeItemSelected(GUIListItem item, GUIControl parent)
		{
			//BaseConfig.MyAnimeLog.Write("Facade Item Selected");
			// if this is not a message from the facade, exit
			if (parent != m_Facade && parent != m_Facade.FilmstripLayout)
				return;

			VM_Recommendation rec = m_Facade.SelectedListItem.TVTag as VM_Recommendation;
			SetRec(rec);

		}

		protected override void OnShowContextMenu()
		{
			GUIListItem currentitem = this.m_Facade.SelectedListItem;
			if (currentitem == null) return;

			if (currentitem.TVTag.GetType() == typeof(VM_Recommendation))
			{
				VM_Recommendation rec = currentitem.TVTag as VM_Recommendation;
				if (rec != null)
				{
                    ContextMenu cmenu = new ContextMenu(rec.Recommended_DisplayName);
                    cmenu.AddAction(Translation.DontShowThisAnime, () =>
                    {
                        int recType = 1;


                        VM_ShokoServer.Instance.ShokoServices.IgnoreAnime(rec.RecommendedAnimeID, recType, VM_ShokoServer.Instance.CurrentUser.JMMUserID);

                        LoadData();
                    });
                    cmenu.AddAction(Translation.BookmarkThisAnime, () =>
                    {
                        VM_BookmarkedAnime bookmark = new VM_BookmarkedAnime();
                        bookmark.AnimeID = rec.RecommendedAnimeID;
                        bookmark.Downloading = 0;
                        bookmark.Notes = "";
                        bookmark.Priority = 1;
                        if (bookmark.Save())
                            Utils.DialogMsg(Translation.Sucess, Translation.BookmarkCreated);
                    });
                    cmenu.AddAction(Translation.CreateSeriesForAnime, () =>
                    {
                        CL_Response<CL_AnimeSeries_User> resp = VM_ShokoServer.Instance.ShokoServices.CreateSeriesFromAnime(
                                rec.RecommendedAnimeID, null, VM_ShokoServer.Instance.CurrentUser.JMMUserID,false);
                        if (string.IsNullOrEmpty(resp.ErrorMessage))
                            Utils.DialogMsg(Translation.Sucess, Translation.SeriesCreated);
                        else
                            Utils.DialogMsg(Translation.Error, resp.ErrorMessage);
                    });
                    cmenu.Show();
				}
			}
		}

		private void SetRec(VM_Recommendation rec)
		{
			if (rec == null) return;
            SetGUIProperty(GuiProperty.Recommendations_Rec_Title, rec.Recommended_DisplayName);
            SetGUIProperty(GuiProperty.Recommendations_Rec_Description, rec.Recommended_Description);
            SetGUIProperty(GuiProperty.Recommendations_Rec_ApprovalRating, rec.Recommended_ApprovalRating);
            SetGUIProperty(GuiProperty.Recommendations_Rec_Image, rec.Recommended_PosterPath);

			try
			{
                if (rec.Recommended_AniDB_Anime != null)
                    SetGUIProperty(GuiProperty.Recommendations_Rec_AniDBRating, rec.Recommended_AniDB_Anime.AniDBRatingFormatted);
                else
                    ClearGUIProperty(GuiProperty.Recommendations_Rec_AniDBRating);
            }
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write(ex.ToString());
			}

            SetGUIProperty(GuiProperty.Recommendations_BasedOn_Title, rec.BasedOn_DisplayName);
            SetGUIProperty(GuiProperty.Recommendations_BasedOn_VoteValue, rec.BasedOn_VoteValueFormatted);
            SetGUIProperty(GuiProperty.Recommendations_BasedOn_Image, rec.BasedOn_PosterPath);

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
				
			    SetGUIProperty(GuiProperty.Recommendations_CurrentView, Translation.Watch);
                
				LoadData();
			}

			if (btnViewDownload != null && control == btnViewDownload)
			{
				this.btnViewDownload.IsFocused = false;
				m_Facade.Focus = true;

				if (dummyModeWatch != null) dummyModeWatch.Visible = false;
				
                SetGUIProperty(GuiProperty.Recommendations_CurrentView, Translation.Download);

                LoadData();
			}

			if (this.btnGetMissingInfo != null && control == this.btnGetMissingInfo)
			{
				MainWindow.ServerHelper.DownloadRecommendedAnime();
				SetGUIProperty(GuiProperty.Recommendations_Status, Translation.WaitingOnServer+"...");
				GUIControl.FocusControl(GetID, 50);

				return;
			}

			if (control == this.m_Facade)
			{
				// show the files if we are looking at a torrent
				GUIListItem item = m_Facade.SelectedListItem;
				if (item == null || item.TVTag == null) return;
				if (item.TVTag.GetType() == typeof(VM_Recommendation))
				{
					VM_Recommendation rec = item.TVTag as VM_Recommendation;
					if (rec != null)
					{
						if (dummyModeWatch != null && dummyModeWatch.Visible)
						{
						    VM_AnimeEpisode_User aniEp = (VM_AnimeEpisode_User)VM_ShokoServer.Instance.ShokoServices.GetNextUnwatchedEpisode(rec.Recommended_AnimeSeries.AnimeSeriesID,
							VM_ShokoServer.Instance.CurrentUser.JMMUserID);
							if (aniEp != null)
							{
								MainWindow.vidHandler.ResumeOrPlay(aniEp);
							}
							else
							{
                                Utils.DialogMsg(Translation.Error, Translation.CouldNotFindFirstEpisode);
                            }
						}

					}
					
				}
			}

			base.OnClicked(controlId, control, actionType);
		}
	}
}
