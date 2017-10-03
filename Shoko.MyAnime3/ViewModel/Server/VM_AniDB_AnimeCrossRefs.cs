using System.Collections.Generic;
using System.Linq;
using Shoko.Commons.Extensions;
using Shoko.Models.Client;
using Shoko.Models.Enums;

namespace Shoko.MyAnime3.ViewModel.Server
{
    public class VM_AniDB_AnimeCrossRefs : CL_AniDB_AnimeCrossRefs
    {
        public bool TvDBCrossRefExists => !(CrossRef_AniDB_TvDB == null || CrossRef_AniDB_TvDB.Count == 0 || TvDBSeries == null || TvDBSeries.Count == 0);

    

        public bool MovieDBCrossRefExists => !(CrossRef_AniDB_MovieDB == null || MovieDBMovie == null);

      
        public List<PosterContainer> AllPosters { get; set; }


        public List<FanartContainer> AllFanarts { get; set; }

        public bool TraktCrossRefExists => !(CrossRef_AniDB_Trakt == null || CrossRef_AniDB_Trakt.Count == 0 || TraktShows == null || TraktShows.Count == 0);


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
            if (TvDBImageFanarts != null)
            {
                foreach (VM_TvDB_ImageFanart contract in TvDBImageFanarts.CastList<VM_TvDB_ImageFanart>())
                {
                    contract.IsImageDefault = anime?.DefaultImageFanart != null && anime.DefaultImageFanart.ImageParentType == (int) ImageEntityType.TvDB_FanArt && anime.DefaultImageFanart.ImageParentID == contract.TvDB_ImageFanartID;
                    AllFanarts.Add(new FanartContainer(ImageEntityType.TvDB_FanArt, contract));
                }
            }
            if (TvDBImagePosters != null)
            {
                foreach (VM_TvDB_ImagePoster contract in TvDBImagePosters.CastList<VM_TvDB_ImagePoster>())
                {
                    contract.IsImageDefault = anime?.DefaultImagePoster != null && anime.DefaultImagePoster.ImageParentType == (int) ImageEntityType.TvDB_Cover && anime.DefaultImagePoster.ImageParentID == contract.TvDB_ImagePosterID;
                    AllPosters.Add(new PosterContainer(ImageEntityType.TvDB_Cover, contract));
                }
            }
            if (TvDBImageWideBanners != null)
            {
                foreach (VM_TvDB_ImageWideBanner contract in TvDBImageWideBanners.CastList<VM_TvDB_ImageWideBanner>())
                    contract.IsImageDefault = anime?.DefaultImageWideBanner != null && anime.DefaultImageWideBanner.ImageParentType == (int) ImageEntityType.TvDB_Banner && anime.DefaultImageWideBanner.ImageParentID == contract.TvDB_ImageWideBannerID;
            }
            if (MovieDBFanarts != null)
            {
                foreach (VM_MovieDB_Fanart contract in MovieDBFanarts.CastList<VM_MovieDB_Fanart>())
                {
                    contract.IsImageDefault = anime?.DefaultImageFanart != null && anime.DefaultImageFanart.ImageParentType == (int) ImageEntityType.MovieDB_FanArt && anime.DefaultImageFanart.ImageParentID == contract.MovieDB_FanartID;
                    AllFanarts.Add(new FanartContainer(ImageEntityType.MovieDB_FanArt, contract));
                }
            }
            if (MovieDBPosters != null)
            {
                foreach (VM_MovieDB_Poster contract in MovieDBPosters.CastList<VM_MovieDB_Poster>())
                {
                    contract.IsImageDefault = anime?.DefaultImagePoster != null && anime.DefaultImagePoster.ImageParentType == (int) ImageEntityType.MovieDB_Poster && anime.DefaultImagePoster.ImageParentID == contract.MovieDB_PosterID;
                    AllPosters.Add(new PosterContainer(ImageEntityType.MovieDB_Poster, contract));
                }
            }
            BaseConfig.MyAnimeLog.Write("Fill: Posters: " + AllPosters.Count + " Fanarts: " + AllFanarts.Count);

        }
    }
}