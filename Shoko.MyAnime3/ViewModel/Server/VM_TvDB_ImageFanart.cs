using System.IO;
using Shoko.Models.Enums;
using Shoko.Models.Server;
using Shoko.MyAnime3.ImageManagement;
using Shoko.MyAnime3.Windows;

namespace Shoko.MyAnime3.ViewModel.Server
{
    public class VM_TvDB_ImageFanart : TvDB_ImageFanart
    {
        public bool IsImageDefault { get; set; } = false;

        public string FullImagePathPlain
        {
            get
            {
                if (string.IsNullOrEmpty(BannerPath)) return string.Empty;
                return Path.Combine(Utils.GetTvDBImagePath(), BannerPath.Replace("/", @"\"));
            }
        }

        public string FullImagePath
        {
            get
            {
                if (string.IsNullOrEmpty(FullImagePathPlain)) return FullImagePathPlain;

                if (!File.Exists(FullImagePathPlain))
                {
                    ImageDownloadRequest req = new ImageDownloadRequest(ImageEntityType.TvDB_FanArt, this, false);
                    MainWindow.imageHelper.DownloadImage(req);
                    if (File.Exists(FullImagePathPlain)) return FullImagePathPlain;
                }

                return FullImagePathPlain;
            }
        }

        public string FullThumbnailPathPlain
        {
            get
            {
                if (string.IsNullOrEmpty(ThumbnailPath)) return string.Empty;
                return Path.Combine(Utils.GetTvDBImagePath(), ThumbnailPath.Replace("/", @"\"));
            }
        }

        public string FullThumbnailPath
        {
            get
            {
                if (string.IsNullOrEmpty(FullThumbnailPathPlain)) return FullThumbnailPathPlain;

                if (!File.Exists(FullThumbnailPathPlain))
                {
                    ImageDownloadRequest req = new ImageDownloadRequest(ImageEntityType.TvDB_FanArt, this, false);
                    MainWindow.imageHelper.DownloadImage(req);
                    if (File.Exists(FullThumbnailPathPlain)) return FullThumbnailPathPlain;
                }

                return FullThumbnailPathPlain;
            }
        }

    }
}