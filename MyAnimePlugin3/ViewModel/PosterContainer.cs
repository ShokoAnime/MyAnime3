using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyAnimePlugin3.ViewModel
{
	public class PosterContainer
	{
		public ImageEntityType ImageType { get; set; }
		public object PosterObject { get; set; }

		public PosterContainer(ImageEntityType imageType, object poster)
		{
			ImageType = imageType;
			PosterObject = poster;

			switch (ImageType)
			{
				case ImageEntityType.AniDB_Cover:
					AniDB_AnimeVM anime = PosterObject as AniDB_AnimeVM;
					IsImageEnabled = anime.ImageEnabled == 1;
					IsImageDefault = anime.IsImageDefault;
					PosterSource = "AniDB";
					break;

				case ImageEntityType.TvDB_Cover:
					TvDB_ImagePosterVM tvPoster = PosterObject as TvDB_ImagePosterVM;
					IsImageEnabled = tvPoster.Enabled == 1;
					IsImageDefault = tvPoster.IsImageDefault;
					PosterSource = "TvDB";
					break;

				case ImageEntityType.MovieDB_Poster:
					MovieDB_PosterVM moviePoster = PosterObject as MovieDB_PosterVM;
					IsImageEnabled = moviePoster.Enabled == 1;
					IsImageDefault = moviePoster.IsImageDefault;
					PosterSource = "MovieDB";
					break;

				case ImageEntityType.Trakt_Poster:
					Trakt_ImagePosterVM traktPoster = PosterObject as Trakt_ImagePosterVM;
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
						AniDB_AnimeVM anime = PosterObject as AniDB_AnimeVM;
						return anime.PosterPath;

					case ImageEntityType.TvDB_Cover:
						TvDB_ImagePosterVM tvPoster = PosterObject as TvDB_ImagePosterVM;
						return tvPoster.FullImagePath;

					case ImageEntityType.MovieDB_Poster:
						MovieDB_PosterVM moviePoster = PosterObject as MovieDB_PosterVM;
						return moviePoster.FullImagePath;

					case ImageEntityType.Trakt_Poster:
						Trakt_ImagePosterVM traktPoster = PosterObject as Trakt_ImagePosterVM;
						return traktPoster.FullImagePath;
				}

				return "";
			}
		}

		private bool isImageEnabled = false;
		public bool IsImageEnabled
		{
			get { return isImageEnabled; }
			set { isImageEnabled = value;}
		}

		private bool isImageDefault = false;
		public bool IsImageDefault
		{
			get { return isImageDefault; }
			set { isImageDefault = value; }
		}


		private string posterSource = "";
		public string PosterSource
		{
			get { return posterSource; }
			set { posterSource = value; }
		}
	}
}
