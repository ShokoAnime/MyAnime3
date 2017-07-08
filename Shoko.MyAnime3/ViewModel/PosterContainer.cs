using Shoko.Models.Enums;
using Shoko.MyAnime3.ViewModel.Server;

namespace Shoko.MyAnime3.ViewModel
{
    public class PosterContainer
    {

        public bool IsImageEnabled { get; set; }
        public bool IsImageDefault { get; set; }
        public string PosterSource { get; set; } = string.Empty;
        public ImageEntityType ImageType { get; set; }
        public object PosterObject { get; set; }

        public PosterContainer(ImageEntityType imageType, object poster)
        {
            ImageType = imageType;
            PosterObject = poster;

            switch (ImageType)
            {
                case ImageEntityType.AniDB_Cover:
                    VM_AniDB_Anime anime = (VM_AniDB_Anime)PosterObject;
                    IsImageEnabled = anime.ImageEnabled == 1;
                    IsImageDefault = anime.IsImageDefault;
                    PosterSource = "AniDB";
                    break;

                case ImageEntityType.TvDB_Cover:
                    VM_TvDB_ImagePoster tvPoster = (VM_TvDB_ImagePoster)PosterObject;
                    IsImageEnabled = tvPoster.Enabled == 1;
                    IsImageDefault = tvPoster.IsImageDefault;
                    PosterSource = "TvDB";
                    break;

                case ImageEntityType.MovieDB_Poster:
                    VM_MovieDB_Poster moviePoster = (VM_MovieDB_Poster)PosterObject;
                    IsImageEnabled = moviePoster.Enabled == 1;
                    IsImageDefault = moviePoster.IsImageDefault;
                    PosterSource = "MovieDB";
                    break;

                case ImageEntityType.Trakt_Poster:
                    VM_Trakt_ImagePoster traktPoster = (VM_Trakt_ImagePoster)PosterObject;
                    IsImageEnabled = traktPoster.Enabled == 1;
                    IsImageDefault = traktPoster.IsImageDefault;
                    PosterSource = "Trakt";
                    break;
            }
        }

        public string FullImagePath
        {
            get
            {
                switch (ImageType)
                {
                    case ImageEntityType.AniDB_Cover:
                        return ((VM_AniDB_Anime)PosterObject).PosterPath;

                    case ImageEntityType.TvDB_Cover:
                        return ((VM_TvDB_ImagePoster)PosterObject).FullImagePath;

                    case ImageEntityType.MovieDB_Poster:
                        return ((VM_MovieDB_Poster)PosterObject).FullImagePath;

                    case ImageEntityType.Trakt_Poster:
                        return ((VM_Trakt_ImagePoster)PosterObject).FullImagePath;
                }

                return string.Empty;
            }
        }

    }
}