using System;
using System.IO;
using Shoko.Models.Enums;
using Shoko.Models.Server;
using Shoko.MyAnime3.ImageManagement;
using Shoko.MyAnime3.Windows;

namespace Shoko.MyAnime3.ViewModel.Server
{
    public class VM_Trakt_ImagePoster : Trakt_ImagePoster
    {
        public string FullThumbnailPath => FullImagePath;

        public bool IsImageDefault { get; set; } = false;
        public string FullImagePathPlain
        {
            get
            {
                // typical url
                // http://vicmackey.trakt.tv/images/seasons/3228-1.jpg
                // http://vicmackey.trakt.tv/images/posters/1130.jpg

                if (string.IsNullOrEmpty(ImageURL)) return string.Empty;

                int pos = ImageURL.IndexOf(@"images/", StringComparison.Ordinal);
                if (pos <= 0) return string.Empty;

                string relativePath = ImageURL.Substring(pos + 7, ImageURL.Length - pos - 7);
                relativePath = relativePath.Replace("/", @"\");

                return Path.Combine(Utils.GetTraktImagePath(), relativePath);
            }
        }

        public string FullImagePath
        {
            get
            {
                if (string.IsNullOrEmpty(FullImagePathPlain)) return FullImagePathPlain;

                if (!File.Exists(FullImagePathPlain))
                {
                    ImageDownloadRequest req = new ImageDownloadRequest(ImageEntityType.Trakt_Poster, this, false);
                    MainWindow.imageHelper.DownloadImage(req);
                    if (File.Exists(FullImagePathPlain)) return FullImagePathPlain;
                }

                return FullImagePathPlain;
            }
        }


    }
}