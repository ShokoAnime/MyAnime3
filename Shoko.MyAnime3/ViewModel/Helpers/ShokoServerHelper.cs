using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using Shoko.Commons.Extensions;
using Shoko.Commons.Utils;
using Shoko.Models.Client;
using Shoko.Models.Enums;
using Shoko.Models.Server;
using Shoko.MyAnime3.Events;
using Shoko.MyAnime3.Extensions;
using Shoko.MyAnime3.ViewModel.Server;
using Shoko.MyAnime3.Windows;

namespace Shoko.MyAnime3.ViewModel.Helpers
{
    public class ShokoServerHelper
    {
        private readonly BackgroundWorker downloadRelatedAnimeWorker = new BackgroundWorker();
        private readonly BackgroundWorker downloadAnimeWorker = new BackgroundWorker();
        private readonly BackgroundWorker downloadCharacterCreatorImagesWorker = new BackgroundWorker();
        private readonly BackgroundWorker downloadCharacterImagesForSeiyuuWorker = new BackgroundWorker();
        private readonly BackgroundWorker downloadRecommendedAnimeWorker = new BackgroundWorker();

        public ShokoServerHelper()
        {
            downloadRelatedAnimeWorker.DoWork += downloadRelatedAnimeWorker_DoWork;
            downloadRelatedAnimeWorker.RunWorkerCompleted += downloadRelatedAnimeWorker_RunWorkerCompleted;

            downloadCharacterCreatorImagesWorker.DoWork += downloadCharacterCreatorImagesWorker_DoWork;
            downloadCharacterCreatorImagesWorker.RunWorkerCompleted += downloadCharacterCreatorImagesWorker_RunWorkerCompleted;

            downloadAnimeWorker.DoWork += downloadAnimeWorker_DoWork;
            downloadAnimeWorker.RunWorkerCompleted += downloadAnimeWorker_RunWorkerCompleted;

            downloadRecommendedAnimeWorker.DoWork += downloadRecommendedAnimeWorker_DoWork;
            downloadRecommendedAnimeWorker.RunWorkerCompleted += downloadRecommendedAnimeWorker_RunWorkerCompleted;

            downloadCharacterImagesForSeiyuuWorker.DoWork += downloadCharacterImagesForSeiyuuWorker_DoWork;
        }


        public delegate void GotCharacterCreatorImagesEventHandler(GotCharacterCreatorImagesEventArgs ev);

        public event GotCharacterCreatorImagesEventHandler GotCharacterCreatorImagesEvent;

        protected void OnGotCharacterCreatorImagesEvent(GotCharacterCreatorImagesEventArgs ev)
        {
            if (GotCharacterCreatorImagesEvent != null)
                GotCharacterCreatorImagesEvent(ev);
        }

        public delegate void GotCharacterImagesEventHandler(GotCharacterImagesEventArgs ev);

        public event GotCharacterImagesEventHandler GotCharacterImagesEvent;

        protected void OnGotCharacterImagesEvent(GotCharacterImagesEventArgs ev)
        {
            if (GotCharacterImagesEvent != null)
                GotCharacterImagesEvent(ev);
        }

        public delegate void GotRelatedAnimeEventHandler(GotAnimeForRelatedEventArgs ev);

        public event GotRelatedAnimeEventHandler GotRelatedAnimeEvent;

        protected void OnGotRelatedAnimeEvent(GotAnimeForRelatedEventArgs ev)
        {
            if (GotRelatedAnimeEvent != null)
                GotRelatedAnimeEvent(ev);
        }

        public delegate void GotRecommendedAnimeEventHandler(GotAnimeForRecommendedEventArgs ev);

        public event GotRecommendedAnimeEventHandler GotRecommendedAnimeEvent;

        protected void OnGotRecommendedAnimeEvent(GotAnimeForRecommendedEventArgs ev)
        {
            if (GotRecommendedAnimeEvent != null)
                GotRecommendedAnimeEvent(ev);
        }

        public delegate void GotAnimeEventHandler(GotAnimeEventArgs ev);

        public event GotAnimeEventHandler GotAnimeEvent;

        protected void OnGotAnimeEvent(GotAnimeEventArgs ev)
        {
            if (GotAnimeEvent != null)
                GotAnimeEvent(ev);
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

        public void DownloadCharacterCreatorImages(VM_AniDB_Anime anime)
        {
            if (downloadCharacterCreatorImagesWorker.IsBusy) return;
            downloadCharacterCreatorImagesWorker.RunWorkerAsync(anime);
        }

        public void DownloadCharacterImagesForSeiyuu(AniDB_Seiyuu seiyuu)
        {
            if (downloadCharacterImagesForSeiyuuWorker.IsBusy) return;
            downloadCharacterImagesForSeiyuuWorker.RunWorkerAsync(seiyuu);
        }

        void downloadRecommendedAnimeWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
        }

        void downloadRecommendedAnimeWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            List<VM_Recommendation> contracts = VM_ShokoServer.Instance.ShokoServices.GetRecommendations(20, VM_ShokoServer.Instance.CurrentUser.JMMUserID, 2).CastList<VM_Recommendation>(); // downloads only


            foreach (VM_Recommendation rec in contracts)
            {
                if (rec.Recommended_AniDB_Anime == null)
                {
                    BaseConfig.MyAnimeLog.Write("Updating data for anime: " + rec.RecommendedAnimeID);
                    VM_ShokoServer.Instance.ShokoServices.UpdateAnimeData(rec.RecommendedAnimeID);
                }
            }

            // refresh the data
            List<VM_Recommendation> tempRecs = new List<VM_Recommendation>();
            contracts = VM_ShokoServer.Instance.ShokoServices.GetRecommendations(20, VM_ShokoServer.Instance.CurrentUser.JMMUserID, 2).CastList<VM_Recommendation>(); // downloads only
            foreach (VM_Recommendation rec in contracts)
            {
                if (rec.Recommended_AniDB_Anime == null)
                    tempRecs.Add(rec);
            }

            // lets try and download the images
            DateTime start = DateTime.Now;
            bool imagesAvailable = false;
            bool timeOut = false;

            while (!imagesAvailable && !timeOut)
            {
                BaseConfig.MyAnimeLog.Write("Checking for images...");
                bool foundAllImages = true;
                foreach (VM_Recommendation rec in tempRecs)
                    if (!File.Exists(rec.Recommended_AniDB_Anime.PosterPathNoDefault))
                    {
                        BaseConfig.MyAnimeLog.Write("Downloading image for : " + rec.Recommended_AniDB_Anime.AnimeID);
                        MainWindow.imageHelper.DownloadAniDBCover(rec.Recommended_AniDB_Anime, false);
                        foundAllImages = false;
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

            BaseConfig.MyAnimeLog.Write("Updating data for anime: " + animeID);
            VM_ShokoServer.Instance.ShokoServices.UpdateAnimeData(animeID);

            OnGotAnimeEvent(new GotAnimeEventArgs(animeID));
        }

        void downloadCharacterCreatorImagesWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
        }

        void downloadCharacterCreatorImagesWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            VM_AniDB_Anime anime = (VM_AniDB_Anime) e.Argument;

            MainWindow.imageHelper.DownloadAniDBCharactersCreatorsSync(anime, false);

            OnGotCharacterCreatorImagesEvent(new GotCharacterCreatorImagesEventArgs(anime.AnimeID));
        }

        void downloadCharacterImagesForSeiyuuWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            AniDB_Seiyuu seiyuu = (AniDB_Seiyuu) e.Argument;

            List<CL_AniDB_Character> charContracts = VM_ShokoServer.Instance.ShokoServices.GetCharactersForSeiyuu(seiyuu.AniDB_SeiyuuID);
            if (charContracts == null) return;
            MainWindow.imageHelper.DownloadAniDBCharactersForSeiyuuSync(charContracts, false);

            OnGotCharacterImagesEvent(new GotCharacterImagesEventArgs(seiyuu.AniDB_SeiyuuID));
        }

        void downloadRelatedAnimeWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
        }

        void downloadRelatedAnimeWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            int animeID = int.Parse(e.Argument.ToString());

            List<CL_AniDB_Anime_Relation> links = GetRelatedAnime(animeID);
            foreach (CL_AniDB_Anime_Relation link in links)
                if (!link.AnimeInfoExists())
                {
                    BaseConfig.MyAnimeLog.Write("Updating data for anime: " + link.RelatedAnimeID);
                    VM_ShokoServer.Instance.ShokoServices.UpdateAnimeData(link.RelatedAnimeID);
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
                foreach (CL_AniDB_Anime_Relation link in links)
                    if (link.AnimeInfoExists())
                        if (!File.Exists(((VM_AniDB_Anime)link.AniDB_Anime).PosterPathNoDefault))
                        {
                            BaseConfig.MyAnimeLog.Write("Downloading image for : " + link.AniDB_Anime.AnimeID);
                            MainWindow.imageHelper.DownloadAniDBCover((VM_AniDB_Anime)link.AniDB_Anime, false);
                            foundAllImages = false;
                        }
                TimeSpan ts = DateTime.Now - start;
                if (ts.TotalSeconds > 15) timeOut = true;
                imagesAvailable = foundAllImages;

                Thread.Sleep(2000);
            }


            OnGotRelatedAnimeEvent(new GotAnimeForRelatedEventArgs(animeID));
        }

        public static VM_AniDB_Anime GetAnime(int animeID)
        {
            return (VM_AniDB_Anime)VM_ShokoServer.Instance.ShokoServices.GetAnime(animeID);

        }

        /*
		public static CL_GroupFilterExtended GetGroupFilterExtended(int groupFilterID)
		{
			if (JMMServerVM.Instance.CurrentUser == null) return null;

			JMMServerBinary.Contract_GroupFilterExtended contract = JMMServerVM.Instance.clientBinaryHTTP.GetGroupFilterExtended(groupFilterID, JMMServerVM.Instance.CurrentUser.JMMUserID);
			if (contract == null) return null;

			return new CL_GroupFilterExtended(contract);
		}
        */

        public static List<VM_GroupFilter> GetTopLevelGroupFilters()
        {
            return GetChildGroupFilters(null);
        }

        public static List<VM_GroupFilter> GetChildGroupFilters(VM_GroupFilter grpf)
        {
            List<VM_GroupFilter> gfs = new List<VM_GroupFilter>();
            try
            {
                int grid = 0;
                if (grpf != null)
                    grid = grpf.GroupFilterID;


                List<VM_GroupFilter> gf_cons = VM_ShokoServer.Instance.ShokoServices.GetGroupFilters(grid).CastList<VM_GroupFilter>();


                foreach (VM_GroupFilter gf_con in gf_cons.Where(a => a.Groups.ContainsKey(VM_ShokoServer.Instance.CurrentUser.JMMUserID) && a.Groups[VM_ShokoServer.Instance.CurrentUser.JMMUserID].Count() > 0
                                                                                           || (a.FilterType & (int) GroupFilterType.Directory) == (int) GroupFilterType.Directory).OrderBy(a => a.GroupFilterName))
                {
                    gf_con.ParentFilter = grpf;
                    gfs.Add(gf_con);
                }
            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write(ex.ToString());
            }

            gfs.Sort();
            return gfs;
        }

        public static List<VM_AnimeGroup_User> GetAnimeGroupsForFilter(VM_GroupFilter groupFilter)
        {
            try
            {
                List<VM_AnimeGroup_User> rawGrps = VM_ShokoServer.Instance.ShokoServices.GetAnimeGroupsForFilter(groupFilter.GroupFilterID,
                            VM_ShokoServer.Instance.CurrentUser.JMMUserID, false).CastList<VM_AnimeGroup_User>() ?? new List<VM_AnimeGroup_User>(); 
                return groupFilter.SortGroups(rawGrps.AsQueryable()).ToList();
            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write(ex.ToString());
            }
            return new List<VM_AnimeGroup_User>(); 
        }

        public static List<VM_AnimeGroup_User> GetSubGroupsForGroup(VM_AnimeGroup_User grp)
        {
            try
            {
                return VM_ShokoServer.Instance.ShokoServices.GetSubGroupsForGroup(grp.AnimeGroupID,
                        VM_ShokoServer.Instance.CurrentUser.JMMUserID).CastList<VM_AnimeGroup_User>() ?? new List<VM_AnimeGroup_User>();

            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write(ex.ToString());
            }
            return new List<VM_AnimeGroup_User>();
        }

        public static List<VM_AnimeSeries_User> GetAnimeSeriesForGroup(VM_AnimeGroup_User grp)
        {
            try
            {
                return VM_ShokoServer.Instance.ShokoServices.GetSeriesForGroup(grp.AnimeGroupID,
                        VM_ShokoServer.Instance.CurrentUser.JMMUserID).CastList<VM_AnimeSeries_User>() ?? new List<VM_AnimeSeries_User>();
            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write(ex.ToString());
            }
            return new List<VM_AnimeSeries_User>();
        }

        public static List<VM_AnimeSeries_User> GetAnimeSeriesForGroupRecursive(VM_AnimeGroup_User grp)
        {
            try
            {
                return VM_ShokoServer.Instance.ShokoServices.GetSeriesForGroupRecursive(grp.AnimeGroupID,
                           VM_ShokoServer.Instance.CurrentUser.JMMUserID).CastList<VM_AnimeSeries_User>() ?? new List<VM_AnimeSeries_User>();
            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write(ex.ToString());
            }
            return new List<VM_AnimeSeries_User>();
        }

        public static VM_AnimeGroup_User GetGroup(int animeGroupID)
        {
            try
            {
                return (VM_AnimeGroup_User)VM_ShokoServer.Instance.ShokoServices.GetGroup(animeGroupID, VM_ShokoServer.Instance.CurrentUser.JMMUserID);
            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write(ex.ToString());
            }
            return null;
        }

        public static VM_AnimeSeries_User GetSeries(int animeSeriesID)
        {
            try
            {
                return (VM_AnimeSeries_User)VM_ShokoServer.Instance.ShokoServices.GetSeries(animeSeriesID, VM_ShokoServer.Instance.CurrentUser.JMMUserID);
            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write(ex.ToString());
            }
            return null;
        }

        public static VM_AnimeSeries_User GetSeriesForAnime(int animeID)
        {
            try
            {
                return (VM_AnimeSeries_User)VM_ShokoServer.Instance.ShokoServices.GetSeriesForAnime(animeID, VM_ShokoServer.Instance.CurrentUser.JMMUserID);
            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write(ex.ToString());
            }
            return null;
        }

        public static List<JMMUser> GetAllUsers()
        {
            List<JMMUser> allusers = new List<JMMUser>();
            try
            {
                return VM_ShokoServer.Instance.ShokoServices.GetAllUsers();
            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write(ex.ToString());
            }
            return allusers;
        }

        public static List<VM_AnimeEpisode_User> GetEpisodesForSeries(int animeSeriesID)
        {
            try
            {
                return VM_ShokoServer.Instance.ShokoServices.GetEpisodesForSeries(animeSeriesID,
                        VM_ShokoServer.Instance.CurrentUser.JMMUserID).CastList<VM_AnimeEpisode_User>() ?? new List<VM_AnimeEpisode_User>();
            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write(ex.ToString());
            }
            return new List<VM_AnimeEpisode_User>();
        }

        public static List<VM_VideoLocal> GetUnlinkedVideos()
        {
            try
            {
                return VM_ShokoServer.Instance.ShokoServices.GetUnrecognisedFiles(VM_ShokoServer.Instance.CurrentUser.JMMUserID).CastList<VM_VideoLocal>() ?? new List<VM_VideoLocal>();
            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write(ex.ToString());
            }
            return new List<VM_VideoLocal>();
        }

        public static bool LinkedFileToEpisode(int videoLocalID, int animeEpisodeID)
        {
            try
            {
                string result = VM_ShokoServer.Instance.ShokoServices.AssociateSingleFile(videoLocalID, animeEpisodeID);
                if (string.IsNullOrEmpty(result))
                {
                    return true;
                }
                BaseConfig.MyAnimeLog.Write("Error in LinkedFileToEpisode: " + result);
                return false;
            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write(ex.ToString());
            }
            return false;
        }

        public static void SetWatchedStatusOnSeries(bool watchedStatus, int maxEpisodeNumber, int animeSeriesID)
        {
            VM_AnimeSeries_User series = GetSeries(animeSeriesID);
            if (series == null) return;

            foreach (VM_AnimeEpisodeType epType in series.EpisodeTypes)
                SetWatchedStatusOnSeries(watchedStatus, maxEpisodeNumber, animeSeriesID, epType.EpisodeType);
        }

        public static void SetWatchedStatusOnSeries(bool watchedStatus, int maxEpisodeNumber, int animeSeriesID, EpisodeType episodeType)
        {
            try
            {
                VM_ShokoServer.Instance.ShokoServices.SetWatchedStatusOnSeries(animeSeriesID, watchedStatus, maxEpisodeNumber,
                    (int) episodeType, VM_ShokoServer.Instance.CurrentUser.JMMUserID);
            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write("SetWatchedStatusOnSeries: " + ex.Message);
            }
        }

        public static void DeleteGroup(VM_AnimeGroup_User grp)
        {
        }

        public static void SetDefaultPoster(bool isDefault, PosterContainer poster, int animeID)
        {
            if (!VM_ShokoServer.Instance.ServerOnline) return;

            try
            {
                switch (poster.ImageType)
                {
                    case ImageEntityType.TvDB_Cover:
                        VM_TvDB_ImagePoster tvPoster = (VM_TvDB_ImagePoster) poster.PosterObject;
                        VM_ShokoServer.Instance.ShokoServices.SetDefaultImage(isDefault, animeID,
                            tvPoster.TvDB_ImagePosterID, (int) ImageEntityType.TvDB_Cover, (int) ImageSizeType.Poster);
                        tvPoster.IsImageDefault = isDefault;
                        break;
                    /*
                           case ImageEntityType.Trakt_Poster:

                               VM_Trakt_ImagePoster traktPoster = (VM_Trakt_ImagePoster) poster.PosterObject;
                               VM_ShokoServer.Instance.ShokoServices.SetDefaultImage(isDefault, animeID,
                                   traktPoster.Trakt_ImagePosterID, (int) ImageEntityType.Trakt_Poster, (int) ImageSizeType.Poster);
                               traktPoster.IsImageDefault = isDefault;
                    break;
                    */
                    case ImageEntityType.AniDB_Cover:
                        VM_AniDB_Anime anime = (VM_AniDB_Anime) poster.PosterObject;
                        VM_ShokoServer.Instance.ShokoServices.SetDefaultImage(isDefault, animeID,
                            anime.AnimeID, (int) ImageEntityType.AniDB_Cover, (int) ImageSizeType.Poster);
                        anime.IsImageDefault = isDefault;
                        break;

                    case ImageEntityType.MovieDB_Poster:
                        VM_MovieDB_Poster moviePoster = (VM_MovieDB_Poster) poster.PosterObject;
                        VM_ShokoServer.Instance.ShokoServices.SetDefaultImage(isDefault, animeID,
                            moviePoster.MovieDB_PosterID, (int) ImageEntityType.MovieDB_Poster, (int) ImageSizeType.Poster);
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
                        VM_TvDB_ImagePoster tvPoster = (VM_TvDB_ImagePoster) poster.PosterObject;
                        VM_ShokoServer.Instance.ShokoServices.EnableDisableImage(enabled, tvPoster.TvDB_ImagePosterID, (int) ImageEntityType.TvDB_Cover);
                        tvPoster.Enabled = enabled ? 1 : 0;
                        break;
                    /*
                    case ImageEntityType.Trakt_Poster:

                        VM_Trakt_ImagePoster traktPoster = (VM_Trakt_ImagePoster) poster.PosterObject;
                        VM_ShokoServer.Instance.ShokoServices.EnableDisableImage(enabled, traktPoster.Trakt_ImagePosterID, (int) ImageEntityType.Trakt_Poster);
                        traktPoster.Enabled = enabled ? 1 : 0;
                    break;
                    */
                    case ImageEntityType.AniDB_Cover:
                        VM_AniDB_Anime anime = (VM_AniDB_Anime) poster.PosterObject;
                        VM_ShokoServer.Instance.ShokoServices.EnableDisableImage(enabled, anime.AnimeID, (int) ImageEntityType.AniDB_Cover);
                        anime.ImageEnabled = enabled ? 1 : 0;
                        break;

                    case ImageEntityType.MovieDB_Poster:
                        VM_MovieDB_Poster moviePoster = (VM_MovieDB_Poster) poster.PosterObject;
                        VM_ShokoServer.Instance.ShokoServices.EnableDisableImage(enabled, moviePoster.MovieDB_PosterID, (int) ImageEntityType.MovieDB_Poster);
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

        public static void SetDefaultWideBanner(bool isDefault, VM_TvDB_ImageWideBanner banner, int animeID)
        {
            if (!VM_ShokoServer.Instance.ServerOnline) return;

            try
            {
                VM_ShokoServer.Instance.ShokoServices.SetDefaultImage(isDefault, animeID,
                    banner.TvDB_ImageWideBannerID, (int) ImageEntityType.TvDB_Banner, (int) ImageSizeType.WideBanner);
                banner.IsImageDefault = isDefault;
            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write("SetDefaultPoster: " + ex.Message);
            }
        }

        public static void EnableDisableWideBanner(bool enabled, VM_TvDB_ImageWideBanner banner, int animeID)
        {
            try
            {
                if (!enabled && banner.IsImageDefault)
                    SetDefaultWideBanner(false, banner, animeID);

                VM_ShokoServer.Instance.ShokoServices.EnableDisableImage(enabled, banner.TvDB_ImageWideBannerID, (int) ImageEntityType.TvDB_Banner);
                banner.Enabled = enabled ? 1 : 0;
            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write("SetDefaultPoster: " + ex.Message);
            }
        }

        public static void SetDefaultFanart(bool isDefault, FanartContainer fanart, int animeID)
        {
            if (!VM_ShokoServer.Instance.ServerOnline) return;

            try
            {
                switch (fanart.ImageType)
                {
                    case ImageEntityType.TvDB_FanArt:
                        VM_TvDB_ImageFanart tvFanart = (VM_TvDB_ImageFanart) fanart.FanartObject;
                        VM_ShokoServer.Instance.ShokoServices.SetDefaultImage(isDefault, animeID,
                            tvFanart.TvDB_ImageFanartID, (int) ImageEntityType.TvDB_FanArt, (int) ImageSizeType.Fanart);
                        tvFanart.IsImageDefault = isDefault;
                        break;
                        /*
                    case ImageEntityType.Trakt_Fanart:
                        VM_Trakt_ImageFanart traktFanart = (VM_Trakt_ImageFanart) fanart.FanartObject;
                        VM_ShokoServer.Instance.ShokoServices.SetDefaultImage(isDefault, animeID,
                            traktFanart.Trakt_ImageFanartID, (int) ImageEntityType.Trakt_Fanart, (int) ImageSizeType.Fanart);
                        traktFanart.IsImageDefault = isDefault;
                        break;
                        */
                    case ImageEntityType.MovieDB_FanArt:
                        VM_MovieDB_Fanart movieFanart = (VM_MovieDB_Fanart) fanart.FanartObject;
                        VM_ShokoServer.Instance.ShokoServices.SetDefaultImage(isDefault, animeID,
                            movieFanart.MovieDB_FanartID, (int) ImageEntityType.MovieDB_FanArt, (int) ImageSizeType.Fanart);
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
                        VM_TvDB_ImageFanart tvFanart = (VM_TvDB_ImageFanart) fanart.FanartObject;
                        VM_ShokoServer.Instance.ShokoServices.EnableDisableImage(enabled, tvFanart.TvDB_ImageFanartID, (int) ImageEntityType.TvDB_FanArt);
                        tvFanart.Enabled = enabled ? 1 : 0;
                        break;
                        /*
                    case ImageEntityType.Trakt_Fanart:
                        VM_Trakt_ImageFanart traktFanart = (VM_Trakt_ImageFanart) fanart.FanartObject;
                        VM_ShokoServer.Instance.ShokoServices.EnableDisableImage(enabled, traktFanart.Trakt_ImageFanartID, (int) ImageEntityType.Trakt_Fanart);
                        traktFanart.Enabled = enabled ? 1 : 0;
                        break;
                        */
                    case ImageEntityType.MovieDB_FanArt:
                        VM_MovieDB_Fanart movieFanart = (VM_MovieDB_Fanart) fanart.FanartObject;
                        VM_ShokoServer.Instance.ShokoServices.EnableDisableImage(enabled, movieFanart.MovieDB_FanartID, (int) ImageEntityType.MovieDB_FanArt);
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

        public static List<CL_AniDB_GroupStatus> GetReleaseGroupsForAnime(int animeID)
        {
            try
            {
                return VM_ShokoServer.Instance.ShokoServices.GetReleaseGroupsForAnime(animeID) ?? new List<CL_AniDB_GroupStatus>();
            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write(ex.ToString());
            }

            return new List<CL_AniDB_GroupStatus>();
        }

        public static List<VM_AniDB_Anime> GetAnimeForMonthYear(int month, int year)
        {
            try
            {
                return VM_ShokoServer.Instance.ShokoServices.GetAnimeForMonth(VM_ShokoServer.Instance.CurrentUser.JMMUserID,month, year).CastList<VM_AniDB_Anime>() ?? new List<VM_AniDB_Anime>();
            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write(ex.ToString());
            }
            return new List<VM_AniDB_Anime>();
        }

        public static List<CL_AniDB_Anime_Relation> GetRelatedAnime(int animeID)
        {
            try
            {
                return VM_ShokoServer.Instance.ShokoServices.GetRelatedAnimeLinks(animeID, VM_ShokoServer.Instance.CurrentUser.JMMUserID).OrderBy(a=>a.GetSortPriority()).ToList();
            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write(ex.ToString());
            }
            return new List<CL_AniDB_Anime_Relation>();
        }

        public static List<CL_AniDB_Character> GetCharactersForAnime(int animeID)
        {
            List<CL_AniDB_Character> allCharacters = new List<CL_AniDB_Character>();
            try
            {
                List<CL_AniDB_Character> chars =VM_ShokoServer.Instance.ShokoServices.GetCharactersForAnime(animeID);

                // first add all the main characters
                foreach (CL_AniDB_Character chr in chars)
                    if (chr.CharType.Equals(Constants.CharacterType.MAIN, StringComparison.InvariantCultureIgnoreCase))
                        allCharacters.Add(chr);

                // now add all the character types
                foreach (CL_AniDB_Character chr in chars)
                    if (!chr.CharType.Equals(Constants.CharacterType.MAIN, StringComparison.InvariantCultureIgnoreCase))
                        allCharacters.Add(chr);

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