using System.Collections.Generic;
using System.Linq;
using Shoko.Models.Client;
using Shoko.Models.Enums;

namespace Shoko.MyAnime3.ViewModel.Server
{
    public class VM_AniDB_AnimeCrossRefs : CL_AniDB_AnimeCrossRefs
    {
        public bool TvDBCrossRefExists => !(CrossRef_AniDB_TvDB == null || CrossRef_AniDB_TvDB.Count == 0 || TvDBSeries == null || TvDBSeries.Count == 0);

        public new List<VM_TvDB_ImageFanart> TvDBImageFanarts => base.TvDBImageFanarts.Cast<VM_TvDB_ImageFanart>().ToList();
        public new List<VM_TvDB_ImagePoster> TvDBImagePosters => base.TvDBImagePosters.Cast<VM_TvDB_ImagePoster>().ToList();
        public new List<VM_TvDB_ImageWideBanner> TvDBImageWideBanners => base.TvDBImageWideBanners.Cast<VM_TvDB_ImageWideBanner>().ToList();


        public bool MovieDBCrossRefExists => !(CrossRef_AniDB_MovieDB == null || MovieDBMovie == null);

        public new List<VM_MovieDB_Fanart> MovieDBFanarts => base.MovieDBFanarts.Cast<VM_MovieDB_Fanart>().ToList();
        public new List<VM_MovieDB_Poster> MovieDBPosters => base.MovieDBPosters.Cast<VM_MovieDB_Poster>().ToList();

        public List<PosterContainer> AllPosters { get; set; }


        public List<FanartContainer> AllFanarts { get; set; }

        public bool TraktCrossRefExists => !(CrossRef_AniDB_Trakt == null || CrossRef_AniDB_Trakt.Count == 0 || TraktShows == null || TraktShows.Count == 0);

        /*
        public new List<VM_Trakt_ImageFanart> TraktImageFanarts => base.TraktImageFanarts.Cast<VM_Trakt_ImageFanart>().ToList();
        public new List<VM_Trakt_ImagePoster> TraktImagePosters => base.TraktImageFanarts.Cast<VM_Trakt_ImagePoster>().ToList();
        */
        public void Fill(VM_AniDB_Anime anime)
        {
            AllFanarts = new List<FanartContainer>();
            AllPosters = new List<PosterContainer>();
            /*
            foreach (VM_Trakt_ImageFanart contract in TraktImageFanarts)
            {
                contract.IsImageDefault = anime?.DefaultImageFanart != null && anime.DefaultImageFanart.ImageParentType == (int) ImageEntityType.Trakt_Fanart && anime.DefaultImageFanart.ImageParentID == contract.Trakt_ImageFanartID;
                AllFanarts.Add(new FanartContainer(ImageEntityType.Trakt_Fanart, contract));
            }
            foreach (VM_Trakt_ImagePoster contract in TraktImagePosters)
            {
                contract.IsImageDefault = anime?.DefaultImagePoster != null && anime.DefaultImagePoster.ImageParentType == (int) ImageEntityType.Trakt_Poster && anime.DefaultImagePoster.ImageParentID == contract.Trakt_ImagePosterID;
                AllPosters.Add(new PosterContainer(ImageEntityType.Trakt_Poster, contract));
            }*/
            foreach (VM_TvDB_ImageFanart contract in TvDBImageFanarts)
            {
                contract.IsImageDefault = anime?.DefaultImageFanart != null && anime.DefaultImageFanart.ImageParentType == (int) ImageEntityType.TvDB_FanArt && anime.DefaultImageFanart.ImageParentID == contract.TvDB_ImageFanartID;
                AllFanarts.Add(new FanartContainer(ImageEntityType.TvDB_FanArt, contract));
            }

            foreach (VM_TvDB_ImagePoster contract in TvDBImagePosters)
            {
                contract.IsImageDefault = anime?.DefaultImagePoster != null && anime.DefaultImagePoster.ImageParentType == (int) ImageEntityType.TvDB_Cover && anime.DefaultImagePoster.ImageParentID == contract.TvDB_ImagePosterID;
                AllPosters.Add(new PosterContainer(ImageEntityType.TvDB_Cover, contract));
            }

            foreach (VM_TvDB_ImageWideBanner contract in TvDBImageWideBanners)
                contract.IsImageDefault = anime?.DefaultImageWideBanner != null && anime.DefaultImageWideBanner.ImageParentType == (int) ImageEntityType.TvDB_Banner && anime.DefaultImageWideBanner.ImageParentID == contract.TvDB_ImageWideBannerID;
            foreach (VM_MovieDB_Fanart contract in MovieDBFanarts)
            {
                contract.IsImageDefault = anime?.DefaultImageFanart != null && anime.DefaultImageFanart.ImageParentType == (int) ImageEntityType.MovieDB_FanArt && anime.DefaultImageFanart.ImageParentID == contract.MovieDB_FanartID;
                AllFanarts.Add(new FanartContainer(ImageEntityType.MovieDB_FanArt, contract));
            }

            foreach (VM_MovieDB_Poster contract in MovieDBPosters)
            {
                contract.IsImageDefault = anime?.DefaultImagePoster != null && anime.DefaultImagePoster.ImageParentType == (int) ImageEntityType.MovieDB_Poster && anime.DefaultImagePoster.ImageParentID == contract.MovieDB_PosterID;
                AllPosters.Add(new PosterContainer(ImageEntityType.MovieDB_Poster, contract));
            }
        }
    }
}