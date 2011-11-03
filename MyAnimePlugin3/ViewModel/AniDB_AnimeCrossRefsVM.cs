using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyAnimePlugin3.ViewModel
{
	public class AniDB_AnimeCrossRefsVM
	{
		public int AnimeID { get; set; }

		public AniDB_AnimeCrossRefsVM()
		{
		}

		private bool tvDBCrossRefExists = false;
		public bool TvDBCrossRefExists
		{
			get { return tvDBCrossRefExists; }
			set { tvDBCrossRefExists = value; }
		}


		public TvDB_SeriesVM TvDBSeries { get; set; }
		public CrossRef_AniDB_TvDBVM CrossRef_AniDB_TvDB  { get; set; }
		public List<TvDB_EpisodeVM> TvDBEpisodes  { get; set; }
		public List<TvDB_ImageFanartVM> TvDBImageFanarts  { get; set; }
		public List<TvDB_ImagePosterVM> TvDBImagePosters  { get; set; }
		public List<TvDB_ImageWideBannerVM> TvDBImageWideBanners  { get; set; }

		private bool movieDBCrossRefExists = false;
		public bool MovieDBCrossRefExists
		{
			get { return movieDBCrossRefExists; }
			set { movieDBCrossRefExists = value; }
		}

		public MovieDB_MovieVM MovieDB_Movie  { get; set; }
		public CrossRef_AniDB_OtherVM CrossRef_AniDB_MovieDB  { get; set; }
		public List<MovieDB_FanartVM> MovieDBFanarts  { get; set; }
		public List<MovieDB_PosterVM> MovieDBPosters  { get; set; }

		public List<PosterContainer> AllPosters  { get; set; }
		public List<FanartContainer> AllFanarts  { get; set; }

		private bool traktCrossRefExists = false;
		public bool TraktCrossRefExists
		{
			get { return traktCrossRefExists; }
			set { traktCrossRefExists = value; }
		}

		public CrossRef_AniDB_TraktVM CrossRef_AniDB_Trakt  { get; set; }
		public Trakt_ShowVM TraktShow  { get; set; }
		public Trakt_ImageFanartVM TraktImageFanart  { get; set; }
		public Trakt_ImagePosterVM TraktImagePoster  { get; set; }

		public void Populate(JMMServerBinary.Contract_AniDB_AnimeCrossRefs details)
		{
			AnimeID = details.AnimeID;

			AniDB_AnimeVM anime = JMMServerHelper.GetAnime(AnimeID);
			if (anime == null) return;
			
			CrossRef_AniDB_TvDB = null;
			TvDBSeries = null;
			TvDBEpisodes = new List<TvDB_EpisodeVM>();
			TvDBImageFanarts = new List<TvDB_ImageFanartVM>();
			TvDBImagePosters = new List<TvDB_ImagePosterVM>();
			TvDBImageWideBanners = new List<TvDB_ImageWideBannerVM>();

			CrossRef_AniDB_MovieDB = null;
			MovieDB_Movie = null;
			MovieDBPosters = new List<MovieDB_PosterVM>();
			MovieDBFanarts = new List<MovieDB_FanartVM>();

			CrossRef_AniDB_Trakt = null;
			TraktShow = null;
			TraktImageFanart = null;
			TraktImagePoster = null;

			AllPosters = new List<PosterContainer>();
			AllFanarts = new List<FanartContainer>();

			// Trakt
			if (details.CrossRef_AniDB_Trakt != null)
				CrossRef_AniDB_Trakt = new CrossRef_AniDB_TraktVM(details.CrossRef_AniDB_Trakt);

			if (details.TraktShow != null)
				TraktShow = new Trakt_ShowVM(details.TraktShow);

			if (details.TraktImageFanart != null)
			{
				TraktImageFanart = new Trakt_ImageFanartVM(details.TraktImageFanart);

				bool isDefault = false;
				if (anime != null && anime.DefaultFanart != null && anime.DefaultFanart.ImageParentType == (int)ImageEntityType.Trakt_Fanart
					&& anime.DefaultFanart.ImageParentID == TraktImageFanart.Trakt_ImageFanartID)
				{
					isDefault = true;
				}

				TraktImageFanart.IsImageDefault = isDefault;

				AllFanarts.Add(new FanartContainer(ImageEntityType.Trakt_Fanart, TraktImageFanart));
			}

			if (details.TraktImagePoster != null)
			{
				TraktImagePoster = new Trakt_ImagePosterVM(details.TraktImagePoster);

				bool isDefault = false;
				if (anime != null && anime.DefaultPoster != null && anime.DefaultPoster.ImageParentType == (int)ImageEntityType.Trakt_Poster
					&& anime.DefaultPoster.ImageParentID == TraktImagePoster.Trakt_ImagePosterID)
				{
					isDefault = true;
				}

				TraktImagePoster.IsImageDefault = isDefault;

				AllPosters.Add(new PosterContainer(ImageEntityType.Trakt_Poster, TraktImagePoster));
			}

			if (CrossRef_AniDB_Trakt == null || TraktShow == null)
				TraktCrossRefExists = false;
			else
				TraktCrossRefExists = true;

			// TvDB
			if (details.CrossRef_AniDB_TvDB != null)
				CrossRef_AniDB_TvDB = new CrossRef_AniDB_TvDBVM(details.CrossRef_AniDB_TvDB);

			if (details.TvDBSeries != null)
				TvDBSeries = new TvDB_SeriesVM(details.TvDBSeries);

			foreach (JMMServerBinary.Contract_TvDB_Episode contract in details.TvDBEpisodes)
				TvDBEpisodes.Add(new TvDB_EpisodeVM(contract));

			foreach (JMMServerBinary.Contract_TvDB_ImageFanart contract in details.TvDBImageFanarts)
			{
				bool isDefault = false;
				if (anime != null && anime.DefaultFanart != null && anime.DefaultFanart.ImageParentType == (int)ImageEntityType.TvDB_FanArt
					&& anime.DefaultFanart.ImageParentID == contract.TvDB_ImageFanartID)
				{
					isDefault = true;
				}

				TvDB_ImageFanartVM tvFanart = new TvDB_ImageFanartVM(contract);
				tvFanart.IsImageDefault = isDefault;
				TvDBImageFanarts.Add(tvFanart);

				AllFanarts.Add(new FanartContainer(ImageEntityType.TvDB_FanArt, tvFanart));
			}

			foreach (JMMServerBinary.Contract_TvDB_ImagePoster contract in details.TvDBImagePosters)
			{
				bool isDefault = false;
				if (anime != null && anime.DefaultPoster != null && anime.DefaultPoster.ImageParentType == (int)ImageEntityType.TvDB_Cover
					&& anime.DefaultPoster.ImageParentID == contract.TvDB_ImagePosterID)
				{
					isDefault = true;
				}

				TvDB_ImagePosterVM tvPoster = new TvDB_ImagePosterVM(contract);
				tvPoster.IsImageDefault = isDefault;
				TvDBImagePosters.Add(tvPoster);
				AllPosters.Add(new PosterContainer(ImageEntityType.TvDB_Cover, tvPoster));
			}

			foreach (JMMServerBinary.Contract_TvDB_ImageWideBanner contract in details.TvDBImageWideBanners)
			{
				bool isDefault = false;
				if (anime != null && anime.DefaultWideBanner != null && anime.DefaultWideBanner.ImageParentType == (int)ImageEntityType.TvDB_Banner
					&& anime.DefaultWideBanner.ImageParentID == contract.TvDB_ImageWideBannerID)
				{
					isDefault = true;
				}

				TvDB_ImageWideBannerVM tvBanner = new TvDB_ImageWideBannerVM(contract);
				tvBanner.IsImageDefault = isDefault;
				TvDBImageWideBanners.Add(tvBanner);
			}

			if (CrossRef_AniDB_TvDB == null || TvDBSeries == null)
				TvDBCrossRefExists = false;
			else
				TvDBCrossRefExists = true;

			// MovieDB
			if (details.CrossRef_AniDB_MovieDB != null)
				CrossRef_AniDB_MovieDB = new CrossRef_AniDB_OtherVM(details.CrossRef_AniDB_MovieDB);

			if (details.MovieDBMovie != null)
				MovieDB_Movie = new MovieDB_MovieVM(details.MovieDBMovie);

			foreach (JMMServerBinary.Contract_MovieDB_Fanart contract in details.MovieDBFanarts)
			{
				bool isDefault = false;
				if (anime != null && anime.DefaultFanart != null && anime.DefaultFanart.ImageParentType == (int)ImageEntityType.MovieDB_FanArt
					&& anime.DefaultFanart.ImageParentID == contract.MovieDB_FanartID)
				{
					isDefault = true;
				}

				MovieDB_FanartVM movieFanart = new MovieDB_FanartVM(contract);
				movieFanart.IsImageDefault = isDefault;
				MovieDBFanarts.Add(movieFanart);
				AllFanarts.Add(new FanartContainer(ImageEntityType.MovieDB_FanArt, movieFanart));
			}

			foreach (JMMServerBinary.Contract_MovieDB_Poster contract in details.MovieDBPosters)
			{
				bool isDefault = false;
				if (anime != null && anime.DefaultPoster != null && anime.DefaultPoster.ImageParentType == (int)ImageEntityType.MovieDB_Poster
					&& anime.DefaultPoster.ImageParentID == contract.MovieDB_PosterID)
				{
					isDefault = true;
				}

				MovieDB_PosterVM moviePoster = new MovieDB_PosterVM(contract);
				moviePoster.IsImageDefault = isDefault;
				MovieDBPosters.Add(moviePoster);
				AllPosters.Add(new PosterContainer(ImageEntityType.MovieDB_Poster, moviePoster));
			}

			if (CrossRef_AniDB_MovieDB == null || MovieDB_Movie == null)
				MovieDBCrossRefExists = false;
			else
				MovieDBCrossRefExists = true;

		}
	}
}
