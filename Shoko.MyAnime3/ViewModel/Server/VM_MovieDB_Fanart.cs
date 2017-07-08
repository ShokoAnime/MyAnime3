using System.IO;
using Shoko.Models.Enums;
using Shoko.Models.Server;
using Shoko.MyAnime3.ImageManagement;
using Shoko.MyAnime3.Windows;

namespace Shoko.MyAnime3.ViewModel.Server
{
    public class VM_MovieDB_Fanart : MovieDB_Fanart
    {
        public bool IsImageDefault { get; set; } = false;

        public string FullImagePathPlain
        {
            get
            {
                if (string.IsNullOrEmpty(URL)) return string.Empty;
                return Path.Combine(Utils.GetMovieDBImagePath(), URL.Replace("/", @"\").TrimStart('\\'));
            }
        }

        public string FullImagePath
        {
            get
            {
                if (string.IsNullOrEmpty(FullImagePathPlain)) return FullImagePathPlain;

                if (!File.Exists(FullImagePathPlain))
                {
                    ImageDownloadRequest req = new ImageDownloadRequest(ImageEntityType.MovieDB_FanArt, this, false);
                    MainWindow.imageHelper.DownloadImage(req);
                    if (File.Exists(FullImagePathPlain)) return FullImagePathPlain;
                }

                return FullImagePathPlain;
            }
        }


    }
}