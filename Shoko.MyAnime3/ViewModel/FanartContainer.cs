using Shoko.Models.Enums;
using Shoko.MyAnime3.ViewModel.Server;

namespace Shoko.MyAnime3.ViewModel
{
    public class FanartContainer
    {
        public ImageEntityType ImageType { get; set; }
        public object FanartObject { get; set; }

        public FanartContainer(ImageEntityType imageType, object poster)
        {
            ImageType = imageType;
            FanartObject = poster;

            switch (ImageType)
            {
                case ImageEntityType.TvDB_FanArt:
                    VM_TvDB_ImageFanart tvFanart = (VM_TvDB_ImageFanart) FanartObject;
                    IsImageEnabled = tvFanart.Enabled == 1;
                    IsImageDefault = tvFanart.IsImageDefault;
                    FanartSource = "TvDB";
                    break;

                case ImageEntityType.MovieDB_FanArt:
                    VM_MovieDB_Fanart movieFanart = (VM_MovieDB_Fanart) FanartObject;
                    IsImageEnabled = movieFanart.Enabled == 1;
                    IsImageDefault = movieFanart.IsImageDefault;
                    FanartSource = "MovieDB";
                    break;
                    /*
                case ImageEntityType.Trakt_Fanart:
                    VM_Trakt_ImageFanart traktFanart = (VM_Trakt_ImageFanart) FanartObject;
                    IsImageEnabled = traktFanart.Enabled == 1;
                    IsImageDefault = traktFanart.IsImageDefault;
                    FanartSource = "Trakt";
                    break;*/
            }
        }

        public string FullImagePath
        {
            get
            {
                switch (ImageType)
                {
                    case ImageEntityType.TvDB_FanArt:
                        return ((VM_TvDB_ImageFanart)FanartObject).FullImagePath;

                    case ImageEntityType.MovieDB_FanArt:
                        return ((VM_MovieDB_Fanart)FanartObject).FullImagePath;
                        /*
                    case ImageEntityType.Trakt_Fanart:
                        return ((VM_Trakt_ImageFanart)FanartObject).FullImagePath;*/
                }

                return "";
            }
        }

        public string FullThumbnailPath
        {
            get
            {
                switch (ImageType)
                {
                    case ImageEntityType.TvDB_FanArt:
                        return ((VM_TvDB_ImageFanart)FanartObject).FullThumbnailPath;

                    case ImageEntityType.MovieDB_FanArt:
                        return ((VM_MovieDB_Fanart)FanartObject).FullImagePath;
                        /*
                    case ImageEntityType.Trakt_Fanart:
                        return ((VM_Trakt_ImageFanart)FanartObject).FullImagePath;*/
                }

                return string.Empty;
            }
        }

        public bool IsImageEnabled { get; set; }

        public bool IsImageDefault { get; set; }


        public string FanartSource { get; set; } = string.Empty;
    }
}