using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BinaryNorthwest;
using MyAnimePlugin3.Events;
using System.ComponentModel;
using System.Threading;
using System.IO;

namespace MyAnimePlugin3.ViewModel
{
	public class JMMServerHelper
	{
		private BackgroundWorker downloadRelatedAnimeWorker = new BackgroundWorker();
		private BackgroundWorker downloadAnimeWorker = new BackgroundWorker();
		private BackgroundWorker downloadCharacterCreatorImagesWorker = new BackgroundWorker();
		private BackgroundWorker downloadCharacterImagesForSeiyuuWorker = new BackgroundWorker();
		private BackgroundWorker downloadRecommendedAnimeWorker = new BackgroundWorker();

		public JMMServerHelper()
		{
			downloadRelatedAnimeWorker.DoWork += new DoWorkEventHandler(downloadRelatedAnimeWorker_DoWork);
			downloadRelatedAnimeWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(downloadRelatedAnimeWorker_RunWorkerCompleted);

			downloadCharacterCreatorImagesWorker.DoWork += new DoWorkEventHandler(downloadCharacterCreatorImagesWorker_DoWork);
			downloadCharacterCreatorImagesWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(downloadCharacterCreatorImagesWorker_RunWorkerCompleted);

			downloadAnimeWorker.DoWork += new DoWorkEventHandler(downloadAnimeWorker_DoWork);
			downloadAnimeWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(downloadAnimeWorker_RunWorkerCompleted);

			downloadRecommendedAnimeWorker.DoWork += new DoWorkEventHandler(downloadRecommendedAnimeWorker_DoWork);
			downloadRecommendedAnimeWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(downloadRecommendedAnimeWorker_RunWorkerCompleted);

			downloadCharacterImagesForSeiyuuWorker.DoWork += new DoWorkEventHandler(downloadCharacterImagesForSeiyuuWorker_DoWork);
		}

		

		

		
		public delegate void GotCharacterCreatorImagesEventHandler(GotCharacterCreatorImagesEventArgs ev);
		public event GotCharacterCreatorImagesEventHandler GotCharacterCreatorImagesEvent;
		protected void OnGotCharacterCreatorImagesEvent(GotCharacterCreatorImagesEventArgs ev)
		{
			if (GotCharacterCreatorImagesEvent != null)
			{
				GotCharacterCreatorImagesEvent(ev);
			}
		}

		public delegate void GotCharacterImagesEventHandler(GotCharacterImagesEventArgs ev);
		public event GotCharacterImagesEventHandler GotCharacterImagesEvent;
		protected void OnGotCharacterImagesEvent(GotCharacterImagesEventArgs ev)
		{
			if (GotCharacterImagesEvent != null)
			{
				GotCharacterImagesEvent(ev);
			}
		}

		public delegate void GotRelatedAnimeEventHandler(GotAnimeForRelatedEventArgs ev);
		public event GotRelatedAnimeEventHandler GotRelatedAnimeEvent;
		protected void OnGotRelatedAnimeEvent(GotAnimeForRelatedEventArgs ev)
		{
			if (GotRelatedAnimeEvent != null)
			{
				GotRelatedAnimeEvent(ev);
			}
		}

		public delegate void GotRecommendedAnimeEventHandler(GotAnimeForRecommendedEventArgs ev);
		public event GotRecommendedAnimeEventHandler GotRecommendedAnimeEvent;
		protected void OnGotRecommendedAnimeEvent(GotAnimeForRecommendedEventArgs ev)
		{
			if (GotRecommendedAnimeEvent != null)
			{
				GotRecommendedAnimeEvent(ev);
			}
		}

		public delegate void GotAnimeEventHandler(GotAnimeEventArgs ev);
		public event GotAnimeEventHandler GotAnimeEvent;
		protected void OnGotAnimeEvent(GotAnimeEventArgs ev)
		{
			if (GotAnimeEvent != null)
			{
				GotAnimeEvent(ev);
			}
		}

		public void DownloadRecommendedAnime()
		{
			if (downloadRecommendedAnimeWorker.IsBusy) return;
			downloadRecommendedAnimeWorker.RunWorkerAsync();
		}

		public void DownloadRelatedAnime(int animeID)
		{
			if (downloadRelatedAnimeWorker.IsBusy) return;
			downloadRelatedAnimeWorker.RunWorkerAsync(animeID);
		}

		public void UpdateAnime(int animeID)
		{
			if (downloadAnimeWorker.IsBusy) return;
			downloadAnimeWorker.RunWorkerAsync(animeID);
		}

		public void DownloadCharacterCreatorImages(AniDB_AnimeVM anime)
		{
			if (downloadCharacterCreatorImagesWorker.IsBusy) return;
			downloadCharacterCreatorImagesWorker.RunWorkerAsync(anime);
		}

		public void DownloadCharacterImagesForSeiyuu(AniDB_SeiyuuVM seiyuu)
		{
			if (downloadCharacterImagesForSeiyuuWorker.IsBusy) return;
			downloadCharacterImagesForSeiyuuWorker.RunWorkerAsync(seiyuu);
		}

		void downloadRecommendedAnimeWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{

		}

		void downloadRecommendedAnimeWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			List<JMMServerBinary.Contract_Recommendation> contracts =
					JMMServerVM.Instance.clientBinaryHTTP.GetRecommendations(20, JMMServerVM.Instance.CurrentUser.JMMUserID, 2); // downloads only


			foreach (JMMServerBinary.Contract_Recommendation contract in contracts)
			{
				RecommendationVM rec = new RecommendationVM();
				rec.Populate(contract);

				if (rec.Recommended_AniDB_Anime == null)
				{
					BaseConfig.MyAnimeLog.Write("Updating data for anime: " + rec.RecommendedAnimeID.ToString());
					JMMServerVM.Instance.clientBinaryHTTP.UpdateAnimeData(rec.RecommendedAnimeID);
				}
			}

			// refresh the data
			List<RecommendationVM> tempRecs = new List<RecommendationVM>();
			contracts = JMMServerVM.Instance.clientBinaryHTTP.GetRecommendations(20, JMMServerVM.Instance.CurrentUser.JMMUserID, 2); // downloads only
			foreach (JMMServerBinary.Contract_Recommendation contract in contracts)
			{
				RecommendationVM rec = new RecommendationVM();
				rec.Populate(contract);
				if (rec.Recommended_AniDB_Anime == null) tempRecs.Add(rec);
			}

			// lets try and download the images
			DateTime start = DateTime.Now;
			bool imagesAvailable = false;
			bool timeOut = false;

			while (!imagesAvailable && !timeOut)
			{
				BaseConfig.MyAnimeLog.Write("Checking for images...");
				bool foundAllImages = true;
				foreach (RecommendationVM rec in tempRecs)
				{
					if (!File.Exists(rec.Recommended_AniDB_Anime.PosterPathNoDefault))
					{
						BaseConfig.MyAnimeLog.Write("Downloading image for : " + rec.Recommended_AniDB_Anime.AnimeID.ToString());
						MainWindow.imageHelper.DownloadAniDBCover(rec.Recommended_AniDB_Anime, false);
						foundAllImages = false;
					}
				}
				TimeSpan ts = DateTime.Now - start;
				if (ts.TotalSeconds > 15) timeOut = true;
				imagesAvailable = foundAllImages;

				Thread.Sleep(2000);
			}


			OnGotRecommendedAnimeEvent(new GotAnimeForRecommendedEventArgs());
		}

		void downloadAnimeWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			
		}

		void downloadAnimeWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			int animeID = int.Parse(e.Argument.ToString());

			BaseConfig.MyAnimeLog.Write("Updating data for anime: " + animeID.ToString());
			JMMServerVM.Instance.clientBinaryHTTP.UpdateAnimeData(animeID);

			OnGotAnimeEvent(new GotAnimeEventArgs(animeID));
		}

		void downloadCharacterCreatorImagesWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{

		}

		void downloadCharacterCreatorImagesWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			AniDB_AnimeVM anime = e.Argument as AniDB_AnimeVM;

			MainWindow.imageHelper.DownloadAniDBCharactersCreatorsSync(anime, false);

			OnGotCharacterCreatorImagesEvent(new GotCharacterCreatorImagesEventArgs(anime.AnimeID));
		}

		void downloadCharacterImagesForSeiyuuWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			AniDB_SeiyuuVM seiyuu = e.Argument as AniDB_SeiyuuVM;

			List<JMMServerBinary.Contract_AniDB_Character> charContracts = JMMServerVM.Instance.clientBinaryHTTP.GetCharactersForSeiyuu(seiyuu.AniDB_SeiyuuID);
			if (charContracts == null) return;

			List<AniDB_CharacterVM> charList = new List<AniDB_CharacterVM>();
			foreach (JMMServerBinary.Contract_AniDB_Character chr in charContracts)
				charList.Add(new AniDB_CharacterVM(chr));

			MainWindow.imageHelper.DownloadAniDBCharactersForSeiyuuSync(charList, false);

			OnGotCharacterImagesEvent(new GotCharacterImagesEventArgs(seiyuu.AniDB_SeiyuuID));
		}

		void downloadRelatedAnimeWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			
		}

		void downloadRelatedAnimeWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			int animeID = int.Parse(e.Argument.ToString());

			List<AniDB_Anime_RelationVM> links = GetRelatedAnime(animeID);
			foreach (AniDB_Anime_RelationVM link in links)
			{
				if (!link.AnimeInfoExists)
				{
					BaseConfig.MyAnimeLog.Write("Updating data for anime: " + link.RelatedAnimeID.ToString());
					JMMServerVM.Instance.clientBinaryHTTP.UpdateAnimeData(link.RelatedAnimeID);
				}
			}
			
			// lets try and download the images
			DateTime start = DateTime.Now;
			bool imagesAvailable = false;
			bool timeOut = false;

			while (!imagesAvailable && !timeOut)
			{
				BaseConfig.MyAnimeLog.Write("Checking for images...");
				links = GetRelatedAnime(animeID);
				bool foundAllImages = true;
				foreach (AniDB_Anime_RelationVM link in links)
				{
					if (link.AnimeInfoExists)
					{
						if (!File.Exists(link.AniDB_Anime.PosterPathNoDefault))
						{
							BaseConfig.MyAnimeLog.Write("Downloading image for : " + link.AniDB_Anime.AnimeID.ToString());
							MainWindow.imageHelper.DownloadAniDBCover(link.AniDB_Anime, false);
							foundAllImages = false;
						}
					}
				}
				TimeSpan ts = DateTime.Now - start;
				if (ts.TotalSeconds > 15) timeOut = true;
				imagesAvailable = foundAllImages;

				Thread.Sleep(2000);
			}


			OnGotRelatedAnimeEvent(new GotAnimeForRelatedEventArgs(animeID));
		}

		public static AniDB_AnimeVM GetAnime(int animeID)
		{
			JMMServerBinary.Contract_AniDBAnime contractAnime = JMMServerVM.Instance.clientBinaryHTTP.GetAnime(animeID);
			if (contractAnime == null) return null;

			return new AniDB_AnimeVM(contractAnime);
		}

		public static byte[] GetImage(string entityID, ImageEntityType imageType, bool thumbNailOnly)
		{
			try
			{
				byte[] imageArray = JMMServerVM.Instance.imageClient.GetImage(entityID, (int)imageType, thumbNailOnly);
				return imageArray;
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write(ex.ToString());
				return null;
			}
		}

		public static GroupFilterExtendedVM GetGroupFilterExtended(int groupFilterID)
		{
			if (JMMServerVM.Instance.CurrentUser == null) return null;

			JMMServerBinary.Contract_GroupFilterExtended contract = JMMServerVM.Instance.clientBinaryHTTP.GetGroupFilterExtended(groupFilterID, JMMServerVM.Instance.CurrentUser.JMMUserID);
			if (contract == null) return null;

			return new GroupFilterExtendedVM(contract);
		}

		public static List<GroupFilterVM> GetAllGroupFilters()
		{
			List<GroupFilterVM> gfs = new List<GroupFilterVM>();
			try
			{
				/*List<JMMServerBinary.Contract_GroupFilterExtended> gf_cons = JMMServerVM.Instance.clientBinaryHTTP.GetAllGroupFiltersExtended(1);
				foreach (JMMServerBinary.Contract_GroupFilterExtended gf_con in gf_cons)
				{
					GroupFilterVM gf = new GroupFilterVM();
					gf.Populate(gf_con);
					gfs.Add(gf);
				}*/

				List<JMMServerBinary.Contract_GroupFilter> gf_cons = JMMServerVM.Instance.clientBinaryHTTP.GetAllGroupFilters();
				foreach (JMMServerBinary.Contract_GroupFilter gf_con in gf_cons)
				{
					GroupFilterVM gf = new GroupFilterVM(gf_con);
					gfs.Add(gf);
				}

			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write(ex.ToString());
			}

			gfs.Sort();
			return gfs;
		}

		public static List<AnimeGroupVM> GetAnimeGroupsForFilter(GroupFilterVM groupFilter)
		{
			DateTime start = DateTime.Now;

			List<AnimeGroupVM> allGroups = new List<AnimeGroupVM>();

			if (JMMServerVM.Instance.CurrentUser == null) return allGroups;

			try
			{
				List<JMMServerBinary.Contract_AnimeGroup> rawGrps = JMMServerVM.Instance.clientBinaryHTTP.GetAnimeGroupsForFilter(groupFilter.GroupFilterID.Value,
					JMMServerVM.Instance.CurrentUser.JMMUserID, BaseConfig.Settings.SingleSeriesGroups);
				if (rawGrps == null) return allGroups;

				foreach (JMMServerBinary.Contract_AnimeGroup contract in rawGrps)
					allGroups.Add(new AnimeGroupVM(contract));

				// apply sorting
				List<SortPropOrFieldAndDirection> sortCriteria = GroupFilterHelper.GetSortDescriptions(groupFilter);
				if (sortCriteria.Count == 0)
				{
					// default sort by name
					SortPropOrFieldAndDirection sortProp = GroupFilterHelper.GetSortDescription(GroupFilterSorting.SortName, GroupFilterSortDirection.Asc);
					sortCriteria.Add(sortProp);
				}

				allGroups = Sorting.MultiSort<AnimeGroupVM>(allGroups, sortCriteria);

				foreach (SortPropOrFieldAndDirection scrit in sortCriteria)
				{
					BaseConfig.MyAnimeLog.Write(string.Format("Sorting: {0} / {1} / {2}", scrit.sPropertyOrFieldName, scrit.fSortDescending, scrit.sortType));
				}

				TimeSpan ts = DateTime.Now - start;
				string msg = string.Format("JMMServerHelper: Got groups for filter: {0} - {1} in {2} ms", groupFilter.GroupFilterName, allGroups.Count, ts.TotalMilliseconds);
				BaseConfig.MyAnimeLog.Write(msg);
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write(ex.ToString());
			}
			return allGroups;
		}

		public static List<AnimeGroupVM> GetSubGroupsForGroup(AnimeGroupVM grp)
		{
			DateTime start = DateTime.Now;

			List<AnimeGroupVM> allGroups = new List<AnimeGroupVM>();

			try
			{
				List<JMMServerBinary.Contract_AnimeGroup> rawGroups = JMMServerVM.Instance.clientBinaryHTTP.GetSubGroupsForGroup(grp.AnimeGroupID,
					JMMServerVM.Instance.CurrentUser.JMMUserID);
				if (rawGroups == null) return allGroups;

				foreach (JMMServerBinary.Contract_AnimeGroup contract in rawGroups)
					allGroups.Add(new AnimeGroupVM(contract));

				TimeSpan ts = DateTime.Now - start;
				string msg = string.Format("Got sub groups for group: {0} - {1} in {2} ms", grp.GroupName, allGroups.Count, ts.TotalMilliseconds);
				BaseConfig.MyAnimeLog.Write(msg);
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write(ex.ToString());
			}
			return allGroups;
		}

		public static List<AnimeSeriesVM> GetAnimeSeriesForGroup(AnimeGroupVM grp)
		{
			DateTime start = DateTime.Now;

			List<AnimeSeriesVM> allSeries = new List<AnimeSeriesVM>();

			try
			{
				List<JMMServerBinary.Contract_AnimeSeries> rawSeries = JMMServerVM.Instance.clientBinaryHTTP.GetSeriesForGroup(grp.AnimeGroupID,
					JMMServerVM.Instance.CurrentUser.JMMUserID);
				if (rawSeries == null) return allSeries;

				foreach (JMMServerBinary.Contract_AnimeSeries contract in rawSeries)
					allSeries.Add(new AnimeSeriesVM(contract));

				TimeSpan ts = DateTime.Now - start;
				string msg = string.Format("Got series for group: {0} - {1} in {2} ms", grp.GroupName, allSeries.Count, ts.TotalMilliseconds);
				BaseConfig.MyAnimeLog.Write(msg);
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write(ex.ToString());
			}
			return allSeries;
		}

		public static List<AnimeSeriesVM> GetAnimeSeriesForGroupRecursive(AnimeGroupVM grp)
		{
			DateTime start = DateTime.Now;

			List<AnimeSeriesVM> allSeries = new List<AnimeSeriesVM>();

			try
			{
				List<JMMServerBinary.Contract_AnimeSeries> rawSeries = JMMServerVM.Instance.clientBinaryHTTP.GetSeriesForGroupRecursive(grp.AnimeGroupID,
					JMMServerVM.Instance.CurrentUser.JMMUserID);
				if (rawSeries == null) return allSeries;

				foreach (JMMServerBinary.Contract_AnimeSeries contract in rawSeries)
					allSeries.Add(new AnimeSeriesVM(contract));

				TimeSpan ts = DateTime.Now - start;
				//string msg = string.Format("Got series for group Recursive: {0} - {1} in {2} ms", grp.GroupName, allSeries.Count, ts.TotalMilliseconds);
				//BaseConfig.MyAnimeLog.Write(msg);
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write(ex.ToString());
			}
			return allSeries;
		}

		public static AnimeGroupVM GetGroup(int animeGroupID)
		{
			try
			{
				JMMServerBinary.Contract_AnimeGroup rawGroup = JMMServerVM.Instance.clientBinaryHTTP.GetGroup(animeGroupID, JMMServerVM.Instance.CurrentUser.JMMUserID);
				if (rawGroup == null) return null;

				return new AnimeGroupVM(rawGroup);
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write(ex.ToString());
			}
			return null;
		}

		public static AnimeSeriesVM GetSeries(int animeSeriesID)
		{
			try
			{
				JMMServerBinary.Contract_AnimeSeries rawSeries = JMMServerVM.Instance.clientBinaryHTTP.GetSeries(animeSeriesID, JMMServerVM.Instance.CurrentUser.JMMUserID);
				if (rawSeries == null) return null;

				return new AnimeSeriesVM(rawSeries);
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write(ex.ToString());
			}
			return null;
		}

		public static AnimeSeriesVM GetSeriesForAnime(int animeID)
		{
			try
			{
				JMMServerBinary.Contract_AnimeSeries rawSeries = JMMServerVM.Instance.clientBinaryHTTP.GetSeriesForAnime(animeID, JMMServerVM.Instance.CurrentUser.JMMUserID);
				if (rawSeries == null) return null;

				return new AnimeSeriesVM(rawSeries);
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write(ex.ToString());
			}
			return null;
		}

		public static List<JMMUserVM> GetAllUsers()
		{
			List<JMMUserVM> allusers = new List<JMMUserVM>();
			try
			{
				List<JMMServerBinary.Contract_JMMUser> users = JMMServerVM.Instance.clientBinaryHTTP.GetAllUsers();
				foreach (JMMServerBinary.Contract_JMMUser user in users)
					allusers.Add(new JMMUserVM(user));

				return allusers;
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write(ex.ToString());
			}
			return allusers;

			
		}

		public static List<AnimeEpisodeVM> GetEpisodesForSeries(int animeSeriesID)
		{
			List<AnimeEpisodeVM> allEps = new List<AnimeEpisodeVM>();
			try
			{
				List<JMMServerBinary.Contract_AnimeEpisode> eps = JMMServerVM.Instance.clientBinaryHTTP.GetEpisodesForSeries(animeSeriesID,
					JMMServerVM.Instance.CurrentUser.JMMUserID);
				foreach (JMMServerBinary.Contract_AnimeEpisode ep in eps)
					allEps.Add(new AnimeEpisodeVM(ep));

				return allEps;
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write(ex.ToString());
			}
			return allEps;
		}

		public static List<VideoLocalVM> GetUnlinkedVideos()
		{
			List<VideoLocalVM> unlinkedVideos = new List<VideoLocalVM>();

			try
			{
				List<JMMServerBinary.Contract_VideoLocal> vids = JMMServerVM.Instance.clientBinaryHTTP.GetUnrecognisedFiles(JMMServerVM.Instance.CurrentUser.JMMUserID);

				foreach (JMMServerBinary.Contract_VideoLocal vid in vids)
				{
					unlinkedVideos.Add(new VideoLocalVM(vid));
				}
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write(ex.ToString());
			}
			return unlinkedVideos;
		}

		public static bool LinkedFileToEpisode(int videoLocalID, int animeEpisodeID)
		{

			try
			{
				string result = JMMServerVM.Instance.clientBinaryHTTP.AssociateSingleFile(videoLocalID, animeEpisodeID);
				if (string.IsNullOrEmpty(result))
					return true;
				else
				{
					BaseConfig.MyAnimeLog.Write("Error in LinkedFileToEpisode: " + result);
					return false;
				}
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write(ex.ToString());
			}
			return false;
		}

		public static void SetWatchedStatusOnSeries(bool watchedStatus, int maxEpisodeNumber, int animeSeriesID)
		{
			AnimeSeriesVM series = GetSeries(animeSeriesID);
			if (series == null) return;

			foreach (AnimeEpisodeTypeVM epType in series.EpisodeTypes)
			{
				SetWatchedStatusOnSeries(watchedStatus, maxEpisodeNumber, animeSeriesID, epType.EpisodeType);
			}
		}

		public static void SetWatchedStatusOnSeries(bool watchedStatus, int maxEpisodeNumber, int animeSeriesID, enEpisodeType episodeType)
		{
			try
			{
				JMMServerVM.Instance.clientBinaryHTTP.SetWatchedStatusOnSeries(animeSeriesID, watchedStatus, maxEpisodeNumber,
					(int)episodeType, JMMServerVM.Instance.CurrentUser.JMMUserID);
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write("SetWatchedStatusOnSeries: " + ex.Message);
			}
		}

		public static void DeleteGroup(AnimeGroupVM grp)
		{
		}

		public static void SetDefaultPoster(bool isDefault, PosterContainer poster, int animeID)
		{
			if (!JMMServerVM.Instance.ServerOnline) return;

			try
			{

				switch (poster.ImageType)
				{
					case ImageEntityType.TvDB_Cover:
						TvDB_ImagePosterVM tvPoster = poster.PosterObject as TvDB_ImagePosterVM;
						JMMServerVM.Instance.clientBinaryHTTP.SetDefaultImage(isDefault, animeID,
							tvPoster.TvDB_ImagePosterID, (int)ImageEntityType.TvDB_Cover, (int)ImageSizeType.Poster);
						tvPoster.IsImageDefault = isDefault;
						break;

					case ImageEntityType.Trakt_Poster:
						Trakt_ImagePosterVM traktPoster = poster.PosterObject as Trakt_ImagePosterVM;
						JMMServerVM.Instance.clientBinaryHTTP.SetDefaultImage(isDefault, animeID,
							traktPoster.Trakt_ImagePosterID, (int)ImageEntityType.Trakt_Poster, (int)ImageSizeType.Poster);
						traktPoster.IsImageDefault = isDefault;
						break;

					case ImageEntityType.AniDB_Cover:
						AniDB_AnimeVM anime = poster.PosterObject as AniDB_AnimeVM;
						JMMServerVM.Instance.clientBinaryHTTP.SetDefaultImage(isDefault, animeID,
							anime.AnimeID, (int)ImageEntityType.AniDB_Cover, (int)ImageSizeType.Poster);
						anime.IsImageDefault = isDefault;
						break;

					case ImageEntityType.MovieDB_Poster:
						MovieDB_PosterVM moviePoster = poster.PosterObject as MovieDB_PosterVM;
						JMMServerVM.Instance.clientBinaryHTTP.SetDefaultImage(isDefault, animeID,
							moviePoster.MovieDB_PosterID, (int)ImageEntityType.MovieDB_Poster, (int)ImageSizeType.Poster);
						moviePoster.IsImageDefault = isDefault;
						break;
				}
				poster.IsImageDefault = isDefault;

			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write("SetDefaultPoster: " + ex.Message);
			}


		}

		public static void EnableDisablePoster(bool enabled, PosterContainer poster, int animeID)
		{
			try
			{
				if (!enabled && poster.IsImageDefault)
					SetDefaultPoster(false, poster, animeID);

				switch (poster.ImageType)
				{
					case ImageEntityType.TvDB_Cover:
						TvDB_ImagePosterVM tvPoster = poster.PosterObject as TvDB_ImagePosterVM;
						JMMServerVM.Instance.clientBinaryHTTP.EnableDisableImage(enabled, tvPoster.TvDB_ImagePosterID, (int)ImageEntityType.TvDB_Cover);
						tvPoster.Enabled = enabled ? 1 : 0;
						break;

					case ImageEntityType.Trakt_Poster:
						Trakt_ImagePosterVM traktPoster = poster.PosterObject as Trakt_ImagePosterVM;
						JMMServerVM.Instance.clientBinaryHTTP.EnableDisableImage(enabled, traktPoster.Trakt_ImagePosterID, (int)ImageEntityType.Trakt_Poster);
						traktPoster.Enabled = enabled ? 1 : 0;
						break;

					case ImageEntityType.AniDB_Cover:
						AniDB_AnimeVM anime = poster.PosterObject as AniDB_AnimeVM;
						JMMServerVM.Instance.clientBinaryHTTP.EnableDisableImage(enabled, anime.AnimeID, (int)ImageEntityType.AniDB_Cover);
						anime.ImageEnabled = enabled ? 1 : 0;
						break;

					case ImageEntityType.MovieDB_Poster:
						MovieDB_PosterVM moviePoster = poster.PosterObject as MovieDB_PosterVM;
						JMMServerVM.Instance.clientBinaryHTTP.EnableDisableImage(enabled, moviePoster.MovieDB_PosterID, (int)ImageEntityType.MovieDB_Poster);
						moviePoster.Enabled = enabled ? 1 : 0;
						break;
				}
				poster.IsImageEnabled = enabled;
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write("SetDefaultPoster: " + ex.Message);
			}
				
		}

		public static void SetDefaultWideBanner(bool isDefault, TvDB_ImageWideBannerVM banner, int animeID)
		{
			if (!JMMServerVM.Instance.ServerOnline) return;

			try
			{

				JMMServerVM.Instance.clientBinaryHTTP.SetDefaultImage(isDefault, animeID,
								banner.TvDB_ImageWideBannerID, (int)ImageEntityType.TvDB_Banner, (int)ImageSizeType.WideBanner);
				banner.IsImageDefault = isDefault;

			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write("SetDefaultPoster: " + ex.Message);
			}
		}

		public static void EnableDisableWideBanner(bool enabled, TvDB_ImageWideBannerVM banner, int animeID)
		{
			try
			{
				if (!enabled && banner.IsImageDefault)
					SetDefaultWideBanner(false, banner, animeID);

				JMMServerVM.Instance.clientBinaryHTTP.EnableDisableImage(enabled, banner.TvDB_ImageWideBannerID, (int)ImageEntityType.TvDB_Banner);
				banner.Enabled = enabled ? 1 : 0;
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write("SetDefaultPoster: " + ex.Message);
			}

		}

		public static void SetDefaultFanart(bool isDefault, FanartContainer fanart, int animeID)
		{
			if (!JMMServerVM.Instance.ServerOnline) return;

			try
			{

				switch (fanart.ImageType)
				{
					case ImageEntityType.TvDB_FanArt:
						TvDB_ImageFanartVM tvFanart = fanart.FanartObject as TvDB_ImageFanartVM;
						JMMServerVM.Instance.clientBinaryHTTP.SetDefaultImage(isDefault, animeID,
							tvFanart.TvDB_ImageFanartID, (int)ImageEntityType.TvDB_FanArt, (int)ImageSizeType.Fanart);
						tvFanart.IsImageDefault = isDefault;
						break;

					case ImageEntityType.Trakt_Fanart:
						Trakt_ImageFanartVM traktFanart = fanart.FanartObject as Trakt_ImageFanartVM;
						JMMServerVM.Instance.clientBinaryHTTP.SetDefaultImage(isDefault, animeID,
							traktFanart.Trakt_ImageFanartID, (int)ImageEntityType.Trakt_Fanart, (int)ImageSizeType.Fanart);
						traktFanart.IsImageDefault = isDefault;
						break;

					case ImageEntityType.MovieDB_FanArt:
						MovieDB_FanartVM movieFanart = fanart.FanartObject as MovieDB_FanartVM;
						JMMServerVM.Instance.clientBinaryHTTP.SetDefaultImage(isDefault, animeID,
							movieFanart.MovieDB_FanartID, (int)ImageEntityType.MovieDB_FanArt, (int)ImageSizeType.Fanart);
						movieFanart.IsImageDefault = isDefault;
						break;
				}
				fanart.IsImageDefault = isDefault;

			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write("SetDefaultPoster: " + ex.Message);
			}
		}

		public static void EnableDisableFanart(bool enabled, FanartContainer fanart, int animeID)
		{
			try
			{
				if (!enabled && fanart.IsImageDefault)
					SetDefaultFanart(false, fanart, animeID);

				switch (fanart.ImageType)
				{
					case ImageEntityType.TvDB_FanArt:
						TvDB_ImageFanartVM tvFanart = fanart.FanartObject as TvDB_ImageFanartVM;
						JMMServerVM.Instance.clientBinaryHTTP.EnableDisableImage(enabled, tvFanart.TvDB_ImageFanartID, (int)ImageEntityType.TvDB_FanArt);
						tvFanart.Enabled = enabled ? 1 : 0;
						break;

					case ImageEntityType.Trakt_Fanart:
						Trakt_ImageFanartVM traktFanart = fanart.FanartObject as Trakt_ImageFanartVM;
						JMMServerVM.Instance.clientBinaryHTTP.EnableDisableImage(enabled, traktFanart.Trakt_ImageFanartID, (int)ImageEntityType.Trakt_Fanart);
						traktFanart.Enabled = enabled ? 1 : 0;
						break;

					case ImageEntityType.MovieDB_FanArt:
						MovieDB_FanartVM movieFanart = fanart.FanartObject as MovieDB_FanartVM;
						JMMServerVM.Instance.clientBinaryHTTP.EnableDisableImage(enabled, movieFanart.MovieDB_FanartID, (int)ImageEntityType.MovieDB_FanArt);
						movieFanart.Enabled = enabled ? 1 : 0;
						break;
				}
				fanart.IsImageEnabled = enabled;
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write("SetDefaultPoster: " + ex.Message);
			}

		}

		public static List<AniDBReleaseGroupVM> GetReleaseGroupsForAnime(int animeID)
		{
			List<AniDBReleaseGroupVM> grps = new List<AniDBReleaseGroupVM>();

			try
			{

				List<JMMServerBinary.Contract_AniDBReleaseGroup> grpsRaw = JMMServerVM.Instance.clientBinaryHTTP.GetReleaseGroupsForAnime(animeID);

				foreach (JMMServerBinary.Contract_AniDBReleaseGroup grp in grpsRaw)
				{
					grps.Add(new AniDBReleaseGroupVM(grp));
				}
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write(ex.ToString());
			}

			return grps;
		}

		public static List<AniDB_AnimeVM> GetAnimeForMonthYear(int month, int year)
		{
			List<AniDB_AnimeVM> allAnime = new List<AniDB_AnimeVM>();

			try
			{
				List<JMMServerBinary.Contract_AniDBAnime> rawAnime = JMMServerVM.Instance.clientBinaryHTTP.GetAnimeForMonth(JMMServerVM.Instance.CurrentUser.JMMUserID,
					month, year);
				if (rawAnime == null) return allAnime;

				foreach (JMMServerBinary.Contract_AniDBAnime contract in rawAnime)
					allAnime.Add(new AniDB_AnimeVM(contract));
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write(ex.ToString());
			}
			return allAnime;
		}

		public static List<AniDB_Anime_RelationVM> GetRelatedAnime(int animeID)
		{
			List<AniDB_Anime_RelationVM> allRelations = new List<AniDB_Anime_RelationVM>();
			try
			{
				List<JMMServerBinary.Contract_AniDB_Anime_Relation> links = JMMServerVM.Instance.clientBinaryHTTP.GetRelatedAnimeLinks(animeID,
					JMMServerVM.Instance.CurrentUser.JMMUserID);

				List<AniDB_Anime_RelationVM> tempList = new List<AniDB_Anime_RelationVM>();
				foreach (JMMServerBinary.Contract_AniDB_Anime_Relation link in links)
				{
					AniDB_Anime_RelationVM rel = new AniDB_Anime_RelationVM();
					rel.Populate(link);
					allRelations.Add(rel);
				}

				List<SortPropOrFieldAndDirection> sortCriteria = new List<SortPropOrFieldAndDirection>();
				sortCriteria.Add(new SortPropOrFieldAndDirection("SortPriority", false, SortType.eInteger));
				allRelations = Sorting.MultiSort<AniDB_Anime_RelationVM>(allRelations, sortCriteria);

				return allRelations;
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write(ex.ToString());
			}
			return allRelations;
		}

		public static List<AniDB_CharacterVM> GetCharactersForAnime(int animeID)
		{
			List<AniDB_CharacterVM> allCharacters = new List<AniDB_CharacterVM>();
			try
			{
				List<JMMServerBinary.Contract_AniDB_Character> chars = JMMServerVM.Instance.clientBinaryHTTP.GetCharactersForAnime(animeID);

				// first add all the main characters
				foreach (JMMServerBinary.Contract_AniDB_Character chr in chars)
				{
					if (chr.CharType.Equals(Constants.CharacterType.MAIN, StringComparison.InvariantCultureIgnoreCase))
						allCharacters.Add(new AniDB_CharacterVM(chr));
				}

				// now add all the character types
				foreach (JMMServerBinary.Contract_AniDB_Character chr in chars)
				{
					if (!chr.CharType.Equals(Constants.CharacterType.MAIN, StringComparison.InvariantCultureIgnoreCase))
						allCharacters.Add(new AniDB_CharacterVM(chr));
				}

				return allCharacters;
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write(ex.ToString());
			}
			return allCharacters;
		}
	}
}
