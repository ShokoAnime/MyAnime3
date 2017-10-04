using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Shoko.Models.Client;
using Shoko.Models.Enums;
using Shoko.Models.Server;
using Shoko.MyAnime3.DataHelpers;
using Shoko.MyAnime3.Extensions;
using Shoko.MyAnime3.ViewModel;
using Shoko.MyAnime3.ViewModel.Helpers;
using Shoko.MyAnime3.ViewModel.Server;

namespace Shoko.MyAnime3.ImageManagement
{
    public class ImageDownloader
    {

        private readonly BlockingList<ImageDownloadRequest> imagesToDownload = new BlockingList<ImageDownloadRequest>();
        private readonly BackgroundWorker workerImages = new BackgroundWorker();
        private static Dictionary<string,object> locks = new Dictionary<string, object>();
        private static object hashlock = new object();
        
        public int QueueCount => imagesToDownload.Count;

        public ImageDownloader()
        {
            workerImages.WorkerReportsProgress = true;
            workerImages.WorkerSupportsCancellation = true;
            workerImages.DoWork += ProcessImages;
        }

        public delegate void QueueUpdateEventHandler(QueueUpdateEventArgs ev);

        public event QueueUpdateEventHandler QueueUpdateEvent;

        protected void OnQueueUpdateEvent(QueueUpdateEventArgs ev)
        {
            QueueUpdateEvent?.Invoke(ev);
        }

        public delegate void ImageDownloadEventHandler(ImageDownloadEventArgs ev);

        public event ImageDownloadEventHandler ImageDownloadEvent;

        protected void OnImageDownloadEvent(ImageDownloadEventArgs ev)
        {
            ImageDownloadEvent?.Invoke(ev);
        }

        public void Init()
        {
            workerImages.RunWorkerAsync();
        }
        


        public void DownloadAniDBCover(VM_AniDB_Anime anime, bool forceDownload)
        {
            if (string.IsNullOrEmpty(anime.Picname)) return;

            try
            {
                ImageDownloadRequest req = new ImageDownloadRequest(ImageEntityType.AniDB_Cover, anime, forceDownload);

                // check if this file has already been downloaded and exists
                if (!req.ForceDownload)
                {
                    // check to make sure the file actually exists
                    if (!File.Exists(anime.PosterPath))
                    {
                        imagesToDownload.Add(req);
                        OnQueueUpdateEvent(new QueueUpdateEventArgs(QueueCount));
                        return;
                    }

                    // the file exists so don't download it again
                    return;
                }

                imagesToDownload.Add(req);
                OnQueueUpdateEvent(new QueueUpdateEventArgs(QueueCount));
            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write(ex.ToString());
            }
        }

        public void DownloadAniDBCharactersCreatorsSync(VM_AniDB_Anime anime, bool forceDownload)
        {
            try
            {
                foreach (CL_AniDB_Character chr in anime.Characters)
                {
                    if (!string.IsNullOrEmpty(chr.PicName))
                    {
                        ImageDownloadRequest req = new ImageDownloadRequest(ImageEntityType.AniDB_Character, chr, forceDownload);

                        // check if this file has already been downloaded and exists
                        if (!req.ForceDownload)
                        {
                            // check to make sure the file actually exists
                            if (!File.Exists(chr.GetPosterPath()))
                                ProcessImageDownloadRequest(req);
                        }
                        else
                        {
                            ProcessImageDownloadRequest(req);
                        }
                    }

                    if (chr.Seiyuu == null) continue;

                    if (!string.IsNullOrEmpty(chr.Seiyuu.PicName))
                    {
                        ImageDownloadRequest req = new ImageDownloadRequest(ImageEntityType.AniDB_Creator, chr.Seiyuu, forceDownload);

                        // check if this file has already been downloaded and exists
                        if (!req.ForceDownload)
                        {
                            // check to make sure the file actually exists
                            if (!File.Exists(chr.Seiyuu.GetPosterPath()))
                                ProcessImageDownloadRequest(req);
                        }
                        else
                        {
                            ProcessImageDownloadRequest(req);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write(ex.ToString());
            }
        }

        public void DownloadAniDBCharactersForSeiyuuSync(List<CL_AniDB_Character> chars, bool forceDownload)
        {
            try
            {
                foreach (CL_AniDB_Character chr in chars)
                    if (!string.IsNullOrEmpty(chr.PicName))
                    {
                        ImageDownloadRequest req = new ImageDownloadRequest(ImageEntityType.AniDB_Character, chr, forceDownload);

                        // check if this file has already been downloaded and exists
                        if (!req.ForceDownload)
                        {
                            // check to make sure the file actually exists
                            if (!File.Exists(chr.GetPosterPath()))
                                ProcessImageDownloadRequest(req);
                        }
                        else
                        {
                            ProcessImageDownloadRequest(req);
                        }
                    }
            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write(ex.ToString());
            }
        }

        public void DownloadTvDBPoster(VM_TvDB_ImagePoster poster, bool forceDownload)
        {
            if (string.IsNullOrEmpty(poster.BannerPath)) return;

            try
            {
                ImageDownloadRequest req = new ImageDownloadRequest(ImageEntityType.TvDB_Cover, poster, forceDownload);

                // check if this file has already been downloaded and exists
                if (!req.ForceDownload)
                {
                    // check to make sure the file actually exists
                    if (!File.Exists(poster.FullImagePath))
                    {
                        imagesToDownload.Add(req);
                        OnQueueUpdateEvent(new QueueUpdateEventArgs(QueueCount));
                        return;
                    }

                    // the file exists so don't download it again
                    return;
                }

                imagesToDownload.Add(req);
                OnQueueUpdateEvent(new QueueUpdateEventArgs(QueueCount));
            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write(ex.ToString());
            }
        }

        public void DownloadTvDBWideBanner(VM_TvDB_ImageWideBanner wideBanner, bool forceDownload)
        {
            if (string.IsNullOrEmpty(wideBanner.BannerPath)) return;

            try
            {
                ImageDownloadRequest req = new ImageDownloadRequest(ImageEntityType.TvDB_Banner, wideBanner, forceDownload);

                // check if this file has already been downloaded and exists
                if (!req.ForceDownload)
                {
                    // check to make sure the file actually exists
                    if (!File.Exists(wideBanner.FullImagePath))
                    {
                        imagesToDownload.Add(req);
                        OnQueueUpdateEvent(new QueueUpdateEventArgs(QueueCount));
                        return;
                    }

                    // the file exists so don't download it again
                    return;
                }

                imagesToDownload.Add(req);
                OnQueueUpdateEvent(new QueueUpdateEventArgs(QueueCount));
            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write(ex.ToString());
            }
        }

        public void DownloadTvDBEpisode(TvDB_Episode episode, bool forceDownload)
        {
            if (string.IsNullOrEmpty(episode.Filename)) return;

            try
            {
                ImageDownloadRequest req = new ImageDownloadRequest(ImageEntityType.TvDB_Episode, episode, forceDownload);

                // check if this file has already been downloaded and exists
                if (!req.ForceDownload)
                {
                    // check to make sure the file actually exists
                    if (!File.Exists(episode.GetFullImagePath()))
                    {
                        imagesToDownload.Add(req);
                        OnQueueUpdateEvent(new QueueUpdateEventArgs(QueueCount));
                        return;
                    }

                    // the file exists so don't download it again
                    return;
                }

                imagesToDownload.Add(req);
                OnQueueUpdateEvent(new QueueUpdateEventArgs(QueueCount));
            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write(ex.ToString());
            }
        }

        public void DownloadTvDBFanart(VM_TvDB_ImageFanart fanart, bool forceDownload)
        {
            if (string.IsNullOrEmpty(fanart.BannerPath)) return;

            try
            {
                ImageDownloadRequest req = new ImageDownloadRequest(ImageEntityType.TvDB_FanArt, fanart, forceDownload);

                // check if this file has already been downloaded and exists
                if (!req.ForceDownload)
                {
                    // check to make sure the file actually exists
                    if (!File.Exists(fanart.FullImagePath) || !File.Exists(fanart.FullThumbnailPath))
                    {
                        imagesToDownload.Add(req);
                        OnQueueUpdateEvent(new QueueUpdateEventArgs(QueueCount));
                        return;
                    }

                    // the file exists so don't download it again
                    return;
                }

                imagesToDownload.Add(req);
                OnQueueUpdateEvent(new QueueUpdateEventArgs(QueueCount));
            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write(ex.ToString());
            }
        }

        public void DownloadMovieDBPoster(VM_MovieDB_Poster poster, bool forceDownload)
        {
            if (string.IsNullOrEmpty(poster.URL)) return;

            try
            {
                ImageDownloadRequest req = new ImageDownloadRequest(ImageEntityType.MovieDB_Poster, poster, forceDownload);

                // check if this file has already been downloaded and exists
                if (!req.ForceDownload)
                {
                    // check to make sure the file actually exists
                    if (!File.Exists(poster.FullImagePath))
                    {
                        imagesToDownload.Add(req);
                        OnQueueUpdateEvent(new QueueUpdateEventArgs(QueueCount));
                        return;
                    }

                    // the file exists so don't download it again
                    return;
                }

                imagesToDownload.Add(req);
                OnQueueUpdateEvent(new QueueUpdateEventArgs(QueueCount));
            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write(ex.ToString());
            }
        }

        public void DownloadMovieDBFanart(VM_MovieDB_Fanart fanart, bool forceDownload)
        {
            if (string.IsNullOrEmpty(fanart.URL)) return;

            try
            {
                ImageDownloadRequest req = new ImageDownloadRequest(ImageEntityType.MovieDB_FanArt, fanart, forceDownload);

                // check if this file has already been downloaded and exists
                if (!req.ForceDownload)
                {
                    // check to make sure the file actually exists
                    if (!File.Exists(fanart.FullImagePath))
                    {
                        imagesToDownload.Add(req);
                        OnQueueUpdateEvent(new QueueUpdateEventArgs(QueueCount));
                        return;
                    }

                    // the file exists so don't download it again
                    return;
                }

                imagesToDownload.Add(req);
                OnQueueUpdateEvent(new QueueUpdateEventArgs(QueueCount));
            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write(ex.ToString());
            }
        }
        /*
        public void DownloadTraktPoster(VM_Trakt_ImagePoster poster, bool forceDownload)
        {
            if (string.IsNullOrEmpty(poster.ImageURL)) return;

            try
            {
                ImageDownloadRequest req = new ImageDownloadRequest(ImageEntityType.Trakt_Poster, poster, forceDownload);

                // check if this file has already been downloaded and exists
                if (!req.ForceDownload)
                {
                    // check to make sure the file actually exists
                    if (!File.Exists(poster.FullImagePath))
                    {
                        imagesToDownload.Add(req);
                        OnQueueUpdateEvent(new QueueUpdateEventArgs(QueueCount));
                        return;
                    }

                    // the file exists so don't download it again
                    return;
                }

                imagesToDownload.Add(req);
                OnQueueUpdateEvent(new QueueUpdateEventArgs(QueueCount));
            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write(ex.ToString());
            }
        }

        public void DownloadTraktFanart(VM_Trakt_ImageFanart fanart, bool forceDownload)
        {
            if (string.IsNullOrEmpty(fanart.ImageURL)) return;

            try
            {
                ImageDownloadRequest req = new ImageDownloadRequest(ImageEntityType.Trakt_Fanart, fanart, forceDownload);

                // check if this file has already been downloaded and exists
                if (!req.ForceDownload)
                {
                    // check to make sure the file actually exists
                    if (!File.Exists(fanart.FullImagePath))
                    {
                        imagesToDownload.Add(req);
                        OnQueueUpdateEvent(new QueueUpdateEventArgs(QueueCount));
                        return;
                    }

                    // the file exists so don't download it again
                    return;
                }

                imagesToDownload.Add(req);
                OnQueueUpdateEvent(new QueueUpdateEventArgs(QueueCount));
            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write(ex.ToString());
            }
        }

        public void DownloadTraktEpisode(Trakt_Episode episode, bool forceDownload)
        {
            if (string.IsNullOrEmpty(episode.EpisodeImage)) return;

            try
            {
                ImageDownloadRequest req = new ImageDownloadRequest(ImageEntityType.Trakt_Episode, episode, forceDownload);

                // check if this file has already been downloaded and exists
                if (!req.ForceDownload)
                {
                    // check to make sure the file actually exists
                    if (!File.Exists(episode.GetFullImagePath()))
                    {
                        imagesToDownload.Add(req);
                        OnQueueUpdateEvent(new QueueUpdateEventArgs(QueueCount));
                        return;
                    }

                    // the file exists so don't download it again
                    return;
                }

                imagesToDownload.Add(req);
                OnQueueUpdateEvent(new QueueUpdateEventArgs(QueueCount));
            }
            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write(ex.ToString());
            }
        }
        */
        private string GetFileName(ImageDownloadRequest req, bool thumbNailOnly)
        {
            switch (req.ImageType)
            {
                case ImageEntityType.AniDB_Cover:
                    return ((VM_AniDB_Anime)req.ImageData).PosterPathNoDefaultPlain;

                case ImageEntityType.AniDB_Character:
                    return ((CL_AniDB_Character) req.ImageData).GetPosterPathPlain();

                case ImageEntityType.AniDB_Creator:
                    return ((AniDB_Seiyuu) req.ImageData).GetPosterPathPlain();

                case ImageEntityType.TvDB_Cover:
                    return ((VM_TvDB_ImagePoster)req.ImageData).FullImagePathPlain;

                case ImageEntityType.TvDB_Banner:
                    return ((VM_TvDB_ImageWideBanner)req.ImageData).FullImagePathPlain;

                case ImageEntityType.TvDB_Episode:
                    return ((TvDB_Episode)req.ImageData).GetFullImagePathPlain();

                case ImageEntityType.TvDB_FanArt:
                    return thumbNailOnly ? ((VM_TvDB_ImageFanart)req.ImageData).FullThumbnailPathPlain : ((VM_TvDB_ImageFanart)req.ImageData).FullImagePathPlain;

                case ImageEntityType.MovieDB_Poster:
                    return ((VM_MovieDB_Poster)req.ImageData).FullImagePathPlain;

                case ImageEntityType.MovieDB_FanArt:
                    return ((VM_MovieDB_Fanart)req.ImageData).FullImagePathPlain;
                /*
            case ImageEntityType.Trakt_Poster:
                return ((VM_Trakt_ImagePoster)req.ImageData).FullImagePathPlain;

            case ImageEntityType.Trakt_Fanart:
                return ((VM_Trakt_ImageFanart)req.ImageData).FullImagePathPlain;

            case ImageEntityType.Trakt_Episode:
                return ((Trakt_Episode)req.ImageData).GetFullImagePathPlain();
                 */
                default:
                    return string.Empty;
            }
        }

        private int GetEntityID(ImageDownloadRequest req)
        {
            switch (req.ImageType)
            {
                case ImageEntityType.AniDB_Cover:
                    return ((VM_AniDB_Anime)req.ImageData).AnimeID;

                case ImageEntityType.AniDB_Character:
                    return ((CL_AniDB_Character)req.ImageData).AniDB_CharacterID;

                case ImageEntityType.AniDB_Creator:
                    return ((AniDB_Seiyuu)req.ImageData).AniDB_SeiyuuID;

                case ImageEntityType.TvDB_Cover:
                    return ((VM_TvDB_ImagePoster)req.ImageData).TvDB_ImagePosterID;

                case ImageEntityType.TvDB_Banner:
                    return ((VM_TvDB_ImageWideBanner)req.ImageData).TvDB_ImageWideBannerID;

                case ImageEntityType.TvDB_Episode:
                    return ((TvDB_Episode)req.ImageData).TvDB_EpisodeID;

                case ImageEntityType.TvDB_FanArt:
                    return ((VM_TvDB_ImageFanart)req.ImageData).TvDB_ImageFanartID;

                case ImageEntityType.MovieDB_Poster:
                    return ((VM_MovieDB_Poster)req.ImageData).MovieDB_PosterID;

                case ImageEntityType.MovieDB_FanArt:
                    return ((VM_MovieDB_Fanart)req.ImageData).MovieDB_FanartID;
                    /*
                case ImageEntityType.Trakt_Poster:
                    return ((VM_Trakt_ImagePoster)req.ImageData).Trakt_ImagePosterID.ToString();

                case ImageEntityType.Trakt_Fanart:
                    return ((VM_Trakt_ImageFanart)req.ImageData).Trakt_ImageFanartID.ToString();
                    */
                case ImageEntityType.Trakt_Episode:
                    return ((Trakt_Episode)req.ImageData).Trakt_EpisodeID;

                default:
                    return 0;
            }
        }

        public object GetUniqueLock(ImageDownloadRequest req, bool thumb)
        {
            string r=req.ImageType + "_" + GetEntityID(req) + "_" + (thumb ? "1" : "0");
            lock (hashlock)
            {
                if (!locks.ContainsKey(r))
                    locks.Add(r, new object());
                return locks[r];
            }
        }
        private void ProcessImages(object sender, DoWorkEventArgs args)
        {
            foreach (ImageDownloadRequest req in imagesToDownload)
                ProcessImageDownloadRequest(req);
        }

        private void ProcessImageDownloadRequest(ImageDownloadRequest req)
        {
            try
            {
                string fileName = GetFileName(req, false);
                int entityID = GetEntityID(req);
                bool downloadImage;
                bool fileExists = File.Exists(fileName);

                downloadImage = !fileExists || req.ForceDownload;





                if (downloadImage)
                {
 
                    lock (GetUniqueLock(req,false))
                    {
                        string tempName = Path.Combine(Utils.GetImagesTempFolder(), Path.GetFileName(fileName) ?? "");
                        if (File.Exists(tempName)) File.Delete(tempName);
                        OnImageDownloadEvent(new ImageDownloadEventArgs("", req, ImageDownloadEventType.Started));

                        if (LoadAndSaveImage(entityID, (int)req.ImageType, false, tempName))
                        {
                            MoveFile(fileExists, fileName, tempName);
                        }          
                    }

                }

                OnImageDownloadEvent(new ImageDownloadEventArgs("", req, ImageDownloadEventType.Complete));


                // if the file is a tvdb fanart also get the thumbnail
                if (req.ImageType == ImageEntityType.TvDB_FanArt)
                {
                    fileName = GetFileName(req, true);
                    entityID = GetEntityID(req);
                    fileExists = File.Exists(fileName);

                    downloadImage = !fileExists || req.ForceDownload;

                    if (downloadImage)
                    {
                        lock (GetUniqueLock(req, true))
                        {
                            string tempName = Path.Combine(Utils.GetImagesTempFolder(), Path.GetFileName(fileName) ?? "");
                            if (File.Exists(tempName)) File.Delete(tempName);

                            OnImageDownloadEvent(new ImageDownloadEventArgs("", req, ImageDownloadEventType.Started));

                            if (LoadAndSaveImage(entityID, (int)req.ImageType, true, tempName))
                            {
                                MoveFile(fileExists, fileName, tempName);
                            }
                          
                        }
                    }
                }

                imagesToDownload.Remove(req);
                OnQueueUpdateEvent(new QueueUpdateEventArgs(QueueCount));

                //OnGotShowInfoEvent(new GotShowInfoEventArgs(req.animeID));
            }
            catch (Exception ex)
            {
                imagesToDownload.Remove(req);
                OnQueueUpdateEvent(new QueueUpdateEventArgs(QueueCount));
                BaseConfig.MyAnimeLog.Write(ex.ToString());
            }
        }

        bool LoadAndSaveImage(int entityid, int type, bool thumb, string tempname)
        {
            Stream imageArray = null;
            using (MemoryStream ms = new MemoryStream())
            {
                try
                {
                    imageArray = VM_ShokoServer.Instance.ShokoImages.GetImage(entityid, type, thumb);
                    imageArray.CopyTo(ms);
                    try
                    {
                        imageArray.Close();
                    }
                    catch
                    {
                        //ignored 
                    }
                    if (ms.Length > 0)
                    {
                        ms.Position = 0;
                        byte[] image = ms.ToArray();
                        if (image.Length > 8)
                        {
                            if (((image[0] == 0xFF) && (image[1] == 0xD8)) || (image[0] == 0x89 && image[1] == 0x50 && image[2] == 0x4e && image[3] == 0x47) || (image[0] == 0x47 && image[1] == 0x49 && image[2] == 0x46))
                            {
                                Stream fw = null;
                                try
                                {
                                    fw = File.OpenWrite(tempname);
                                    fw.Write(image, 0, image.Length);
                                    fw.Close();
                                }
                                catch (Exception)
                                {
                                    fw?.Dispose();
                                    return false;
                                }
                                return true;

                            }
                        }
                    }
                    return false;
                }
                catch
                {
                    return false;
                }
            }
        }

        void MoveFile(bool fileExists, string fileName, string tempName)
        {
            // move the file to it's final location
            if (fileExists) File.Delete(fileName);
            string fullPath = Path.GetDirectoryName(fileName);
                if (fullPath != null)
            {
                if (!Directory.Exists(fullPath))
                    Directory.CreateDirectory(fullPath);
            }

            // move the file to it's final location
            File.Move(tempName, fileName);
        }

        public void DownloadImage(ImageDownloadRequest req)
        {
            try
            {

                    string fileName = GetFileName(req, false);
                    int entityID = GetEntityID(req);
                    bool fileExists = File.Exists(fileName);

                    bool downloadImage = !fileExists || req.ForceDownload;

                    if (downloadImage)
                    {
                        lock (GetUniqueLock(req, false))
                        {
                            string tempName = Path.Combine(Utils.GetImagesTempFolder(), Path.GetFileName(fileName) ?? "");
                            if (File.Exists(tempName)) File.Delete(tempName);


                            OnImageDownloadEvent(new ImageDownloadEventArgs("", req, ImageDownloadEventType.Started));

                            if (LoadAndSaveImage(entityID, (int) req.ImageType, false, tempName))
                            {
                                MoveFile(fileExists,fileName,tempName);
                            }
                        }
                    }


                    // if the file is a tvdb fanart also get the thumbnail
                    if (req.ImageType == ImageEntityType.TvDB_FanArt)
                    {
                        fileName = GetFileName(req, true);
                        entityID = GetEntityID(req);
                        fileExists = File.Exists(fileName);

                        downloadImage = !fileExists || req.ForceDownload;

                        if (downloadImage)
                        {
                            lock (GetUniqueLock(req, true))
                            {
                                string tempName = Path.Combine(Utils.GetImagesTempFolder(), Path.GetFileName(fileName) ?? "");
                                if (File.Exists(tempName)) File.Delete(tempName);

                                OnImageDownloadEvent(new ImageDownloadEventArgs("", req, ImageDownloadEventType.Started));

                                if (LoadAndSaveImage(entityID, (int)req.ImageType, true, tempName))
                                {
                                    MoveFile(fileExists, fileName, tempName);
                                }                        
                            }
                        }
                    }
                }

            catch (Exception ex)
            {
                BaseConfig.MyAnimeLog.Write(ex.ToString());
            }
        }
    }
}