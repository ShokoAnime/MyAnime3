using System;
using System.IO;
using Shoko.Commons.Extensions;
using Shoko.Models.Client;
using Shoko.Models.Enums;
using Shoko.Models.Server;
using Shoko.MyAnime3.ImageManagement;
using Shoko.MyAnime3.ViewModel.Server;
using Shoko.MyAnime3.Windows;

namespace Shoko.MyAnime3.Extensions
{
    public static class ModelExtensions
    {
        public static string GetPosterPath(this CL_AniDB_Anime_Relation aniDbAnimeRelationVm)
        {
            if (aniDbAnimeRelationVm.AniDB_Anime != null)
                return ((VM_AniDB_Anime) aniDbAnimeRelationVm.AniDB_Anime).PosterPath;
            return String.Empty;
        }

        public static string GetPosterPathPlain(this CL_AniDB_Character aniDbCharacterVm)
        {
            if (String.IsNullOrEmpty(aniDbCharacterVm.PicName)) return "";

            return Path.Combine(Utils.GetAniDBCharacterImagePath(aniDbCharacterVm.CharID), aniDbCharacterVm.PicName);
        }

        public static string GetPosterPath(this CL_AniDB_Character aniDbCharacterVm)
        {
            if (String.IsNullOrEmpty(aniDbCharacterVm.GetPosterPathPlain())) return aniDbCharacterVm.GetPosterPathPlain();

            if (!File.Exists(aniDbCharacterVm.GetPosterPathPlain()))
            {
                ImageDownloadRequest req = new ImageDownloadRequest(ImageEntityType.AniDB_Character, aniDbCharacterVm, false);
                MainWindow.imageHelper.DownloadImage(req);
                if (File.Exists(aniDbCharacterVm.GetPosterPathPlain())) return aniDbCharacterVm.GetPosterPathPlain();
            }

            return aniDbCharacterVm.GetPosterPathPlain();
        }

        public static string GetPosterPathPlain(this AniDB_Seiyuu aniDbSeiyuuVm)
        {
            if (String.IsNullOrEmpty(aniDbSeiyuuVm.PicName)) return "";

            return Path.Combine(Utils.GetAniDBCreatorImagePath(aniDbSeiyuuVm.SeiyuuID), aniDbSeiyuuVm.PicName);
        }

        public static string GetPosterPath(this AniDB_Seiyuu aniDbSeiyuuVm)
        {
            if (String.IsNullOrEmpty(aniDbSeiyuuVm.GetPosterPathPlain())) return aniDbSeiyuuVm.GetPosterPathPlain();

            if (!File.Exists(aniDbSeiyuuVm.GetPosterPathPlain()))
            {
                ImageDownloadRequest req = new ImageDownloadRequest(ImageEntityType.AniDB_Creator, aniDbSeiyuuVm, false);
                MainWindow.imageHelper.DownloadImage(req);
                if (File.Exists(aniDbSeiyuuVm.GetPosterPathPlain())) return aniDbSeiyuuVm.GetPosterPathPlain();
            }

            return aniDbSeiyuuVm.GetPosterPathPlain();
        }

        public static string GetAniDBStartEpisodeNumberString(this CrossRef_AniDB_TvDBV2 crossRefAniDbTvDbvmv2)
        {
            return String.Format("# {0}", crossRefAniDbTvDbvmv2.AniDBStartEpisodeNumber);
        }

        public static string GetTvDBSeasonNumberString(this CrossRef_AniDB_TvDBV2 crossRefAniDbTvDbvmv2)
        {
            return String.Format("S{0}", crossRefAniDbTvDbvmv2.TvDBSeasonNumber);
        }

        public static string GetTvDBStartEpisodeNumberString(this CrossRef_AniDB_TvDBV2 crossRefAniDbTvDbvmv2)
        {
            return String.Format("EP# {0}", crossRefAniDbTvDbvmv2.TvDBStartEpisodeNumber);
        }

        public static string GetFullImagePathPlain(this TvDB_Episode tvDbEpisodeVm)
        {
            if (String.IsNullOrEmpty(tvDbEpisodeVm.Filename)) return String.Empty;
            return Path.Combine(Utils.GetTvDBImagePath(), tvDbEpisodeVm.Filename.Replace("/", @"\"));
        }

        public static string GetFullImagePath(this TvDB_Episode tvDbEpisodeVm)
        {
            if (String.IsNullOrEmpty(tvDbEpisodeVm.GetFullImagePathPlain())) return tvDbEpisodeVm.GetFullImagePathPlain();

            if (!File.Exists(tvDbEpisodeVm.GetFullImagePathPlain()))
            {
                ImageDownloadRequest req = new ImageDownloadRequest(ImageEntityType.TvDB_Episode, tvDbEpisodeVm, false);
                MainWindow.imageHelper.DownloadImage(req);
                if (File.Exists(tvDbEpisodeVm.GetFullImagePathPlain())) return tvDbEpisodeVm.GetFullImagePathPlain();
            }

            return tvDbEpisodeVm.GetFullImagePathPlain();
        }

        public static string GetOnlineImagePath(this TvDB_Episode tvDbEpisodeVm)
        {
            if (String.IsNullOrEmpty(tvDbEpisodeVm.Filename)) return "";
            return string.Format(Constants.URLS.TvDB_Images, tvDbEpisodeVm.Filename);
        }

        public static string GetTraktID(this CL_TraktTVShowResponse CL_TraktTVShowResponse)
        {
            if (String.IsNullOrEmpty(CL_TraktTVShowResponse.url)) return String.Empty;

            int pos = CL_TraktTVShowResponse.url.LastIndexOf("/", StringComparison.Ordinal);
            if (pos < 0) return String.Empty;

            string id = CL_TraktTVShowResponse.url.Substring(pos + 1, CL_TraktTVShowResponse.url.Length - pos - 1);
            return id;
        }
        /*
        public static string GetFullImagePathPlain(this Trakt_Episode traktEpisodeVm)
        {
            // typical EpisodeImage url
            // http://vicmackey.trakt.tv/images/episodes/3228-1-1.jpg

            // get the TraktID from the URL
            // http://trakt.tv/show/11eyes/season/1/episode/1 (11 eyes)

            if (String.IsNullOrEmpty(traktEpisodeVm.EpisodeImage)) return "";
            if (String.IsNullOrEmpty(traktEpisodeVm.URL)) return "";

            // on Trakt, if the episode doesn't have a proper screenshot, they will return the
            // fanart instead, we will ignore this
            int pos = traktEpisodeVm.EpisodeImage.IndexOf(@"episodes/", StringComparison.Ordinal);
            if (pos <= 0) return "";

            int posID = traktEpisodeVm.URL.IndexOf(@"show/", StringComparison.Ordinal);
            if (posID <= 0) return "";

            int posIDNext = traktEpisodeVm.URL.IndexOf(@"/", posID + 6, StringComparison.Ordinal);
            if (posIDNext <= 0) return "";

            string traktID = traktEpisodeVm.URL.Substring(posID + 5, posIDNext - posID - 5);
            traktID = traktID.Replace("/", @"\");

            string imageName = traktEpisodeVm.EpisodeImage.Substring(pos + 9, traktEpisodeVm.EpisodeImage.Length - pos - 9);
            imageName = imageName.Replace("/", @"\");

            string relativePath = Path.Combine("episodes", traktID);
            relativePath = Path.Combine(relativePath, imageName);

            return Path.Combine(Utils.GetTraktImagePath(), relativePath);
        }

        public static string GetFullImagePath(this Trakt_Episode traktEpisodeVm)
        {
            if (String.IsNullOrEmpty(traktEpisodeVm.GetFullImagePathPlain())) return traktEpisodeVm.GetFullImagePathPlain();

            if (!File.Exists(traktEpisodeVm.GetFullImagePathPlain()))
            {
                ImageDownloadRequest req = new ImageDownloadRequest(ImageEntityType.Trakt_Episode, traktEpisodeVm, false);
                MainWindow.imageHelper.DownloadImage(req);
                if (File.Exists(traktEpisodeVm.GetFullImagePathPlain())) return traktEpisodeVm.GetFullImagePathPlain();
            }

            return traktEpisodeVm.GetFullImagePathPlain();
        }

        public static string GetOnlineImagePath(this Trakt_Episode traktEpisodeVm)
        {
            if (String.IsNullOrEmpty(traktEpisodeVm.EpisodeImage)) return String.Empty;
            return traktEpisodeVm.EpisodeImage;
        }
        */

        public static string GetPrettyDescription(this CL_GroupVideoQuality CL_GroupVideoQuality)
        {
            return String.Format("{0} - {1}/{2} - {3}/{4} " + Translation.Files, CL_GroupVideoQuality.GroupNameShort, CL_GroupVideoQuality.Resolution, CL_GroupVideoQuality.VideoSource, CL_GroupVideoQuality.FileCountNormal, CL_GroupVideoQuality.FileCountSpecials);
        }


        /*
        public static string GetLocalFileSystemFullPath(this CL_VideoLocal_Place videoLocalPlaceVm)
        {
            if (videoLocalPlaceVm.ImportFolder.CloudID.HasValue)
                return String.Empty;

            if (BaseConfig.Settings.ImportFolderMappings.ContainsKey(videoLocalPlaceVm.ImportFolderID))
            {
                try
                {
                    string path = Path.Combine(BaseConfig.Settings.ImportFolderMappings[videoLocalPlaceVm.ImportFolderID], videoLocalPlaceVm.FilePath);
                    if (File.Exists(path))
                        return path;
                }
                catch (Exception)
                {
                    //ignored
                }
            }
            try
            {
                if (File.Exists(Path.Combine(videoLocalPlaceVm.ImportFolder.ImportFolderLocation, videoLocalPlaceVm.FilePath)))
                    return Path.Combine(videoLocalPlaceVm.ImportFolder.ImportFolderLocation, videoLocalPlaceVm.FilePath);
            }
            catch (Exception)
            {
                //ignored
            }
            return String.Empty;
        }

        public static string GetFullPath(this CL_VideoLocal_Place videoLocalPlaceVm) => Path.Combine(videoLocalPlaceVm.ImportFolder.ImportFolderLocation, videoLocalPlaceVm.FilePath);
        public static string GetFileName(this CL_VideoLocal_Place videoLocalPlaceVm) => Path.GetFileName(videoLocalPlaceVm.FilePath);
        public static string GetFileDirectory(this CL_VideoLocal_Place videoLocalPlaceVm) => Path.GetDirectoryName(videoLocalPlaceVm.GetFullPath());

    */

        public static string GetDescription(this ImportFolder ImportFolder)
        {
            string desc = ImportFolder.ImportFolderLocation;
            if (ImportFolder.IsFolderDropSource())
                desc += " (Drop Source)";

            if (ImportFolder.IsFolderDropDestination())
                desc += " (Drop Destination)";
            if (ImportFolder.CloudID.HasValue)
                desc += " *** CLOUD FOLDER ***";
            else if (string.IsNullOrEmpty(ImportFolder.GetLocalFileSystemFullPath()))
                desc += " *** LOCAL PATH INVALID ***";

            return desc;
        }
    }
}
