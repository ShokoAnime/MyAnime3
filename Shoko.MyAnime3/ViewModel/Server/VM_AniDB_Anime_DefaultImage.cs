using System.IO;
using Shoko.Models.Client;
using Shoko.Models.Enums;
using Shoko.MyAnime3.ViewModel.Helpers;

namespace Shoko.MyAnime3.ViewModel.Server
{
    public class VM_AniDB_Anime_DefaultImage : CL_AniDB_Anime_DefaultImage
    {
        public new VM_TvDB_ImageFanart TVFanart
        {
            get => (VM_TvDB_ImageFanart) base.TVFanart;
            set => base.TVFanart = value;
        }

        public new VM_TvDB_ImagePoster TVPoster
        {
            get => (VM_TvDB_ImagePoster) base.TVPoster;
            set => base.TVPoster = value;
        }

        public new VM_TvDB_ImageWideBanner TVWideBanner
        {
            get => (VM_TvDB_ImageWideBanner) base.TVWideBanner;
            set => base.TVWideBanner = value;
        }

        public new VM_Trakt_ImagePoster TraktPoster
        {
            get => (VM_Trakt_ImagePoster) base.TraktPoster;
            set => base.TraktPoster = value;
        }

        public new VM_Trakt_ImageFanart TraktFanart
        {
            get => (VM_Trakt_ImageFanart) base.TraktFanart;
            set => base.TraktFanart = value;
        }

        public new VM_MovieDB_Poster MoviePoster
        {
            get => (VM_MovieDB_Poster) base.MoviePoster;
            set => base.MoviePoster = value;
        }

        public new VM_MovieDB_Fanart MovieFanart
        {
            get => (VM_MovieDB_Fanart) base.MovieFanart;
            set => base.MovieFanart = value;
        }


        public string FullImagePath
        {
            get
            {
                ImageEntityType itype = (ImageEntityType) ImageParentType;
                string fileName = string.Empty;

                switch (itype)
                {
                    case ImageEntityType.AniDB_Cover:

                        VM_AniDB_Anime anime = ShokoServerHelper.GetAnime(AnimeID);
                        if (anime != null)
                            fileName = anime.PosterPath;
                        break;

                    case ImageEntityType.TvDB_Cover:
                        fileName = TVPoster.FullImagePath;
                        break;

                    case ImageEntityType.MovieDB_Poster:
                        fileName = MoviePoster.FullImagePath;
                        break;

                    case ImageEntityType.MovieDB_FanArt:
                        fileName = MovieFanart.FullImagePath;
                        break;

                    case ImageEntityType.TvDB_FanArt:
                        fileName = TVFanart.FullImagePath;
                        break;

                    case ImageEntityType.TvDB_Banner:
                        fileName = TVWideBanner.FullImagePath;
                        break;

                    case ImageEntityType.Trakt_Poster:
                        fileName = TraktPoster.FullImagePath;
                        break;

                    case ImageEntityType.Trakt_Fanart:
                        fileName = TraktFanart.FullImagePath;
                        break;
                }

                if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
                    return fileName;
                return string.Empty;
            }
        }

        public string FullThumbnailPath
        {
            get
            {
                ImageEntityType itype = (ImageEntityType) ImageParentType;
                string fileName = string.Empty;

                switch (itype)
                {
                    case ImageEntityType.AniDB_Cover:
                        VM_AniDB_Anime anime = ShokoServerHelper.GetAnime(AnimeID);
                        if (anime != null)
                            fileName = anime.PosterPath;
                        break;

                    case ImageEntityType.TvDB_Cover:
                        fileName = TVPoster.FullImagePath;
                        break;

                    case ImageEntityType.MovieDB_Poster:
                        fileName = MoviePoster.FullImagePath;
                        break;

                    case ImageEntityType.MovieDB_FanArt:
                        fileName = MovieFanart.FullImagePath;
                        break;

                    case ImageEntityType.TvDB_FanArt:
                        fileName = TVFanart.FullThumbnailPath;
                        break;

                    case ImageEntityType.TvDB_Banner:
                        fileName = TVWideBanner.FullImagePath;
                        break;

                    case ImageEntityType.Trakt_Poster:
                        fileName = TraktPoster.FullImagePath;
                        break;

                    case ImageEntityType.Trakt_Fanart:
                        fileName = TraktFanart.FullImagePath;
                        break;
                }

                if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
                    return fileName;
                return string.Empty;
            }
        }

        public override string ToString()
        {
            return $"{AnimeID} - {ImageParentID}";
        }
    }
}