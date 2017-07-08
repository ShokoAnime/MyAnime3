using System.IO;
using Shoko.Models.Enums;
using Shoko.Models.Server;
using Shoko.MyAnime3.ImageManagement;
using Shoko.MyAnime3.Windows;

namespace Shoko.MyAnime3.ViewModel.Server
{
    public class VM_TvDB_ImagePoster : TvDB_ImagePoster
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
                    ImageDownloadRequest req = new ImageDownloadRequest(ImageEntityType.TvDB_Cover, this, false);
                    MainWindow.imageHelper.DownloadImage(req);
                    if (File.Exists(FullImagePathPlain)) return FullImagePathPlain;
                }

                return FullImagePathPlain;
            }
        }



    }
}