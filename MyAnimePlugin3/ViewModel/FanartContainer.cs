using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyAnimePlugin3.ViewModel
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
					TvDB_ImageFanartVM tvFanart = FanartObject as TvDB_ImageFanartVM;
					IsImageEnabled = tvFanart.Enabled == 1;
					IsImageDefault = tvFanart.IsImageDefault;
					FanartSource = "TvDB";
					break;

				case ImageEntityType.MovieDB_FanArt:
					MovieDB_FanartVM movieFanart = FanartObject as MovieDB_FanartVM;
					IsImageEnabled = movieFanart.Enabled == 1;
					IsImageDefault = movieFanart.IsImageDefault;
					FanartSource = "MovieDB";
					break;

				case ImageEntityType.Trakt_Fanart:
					Trakt_ImageFanartVM traktFanart = FanartObject as Trakt_ImageFanartVM;
					IsImageEnabled = traktFanart.Enabled == 1;
					IsImageDefault = traktFanart.IsImageDefault;
					FanartSource = "Trakt";
					break;
			}


		}

		public string FullImagePath
		{
			get
			{
				switch (ImageType)
				{

					case ImageEntityType.TvDB_FanArt:
						TvDB_ImageFanartVM tvFanart = FanartObject as TvDB_ImageFanartVM;
						return tvFanart.FullImagePath;

					case ImageEntityType.MovieDB_FanArt:
						MovieDB_FanartVM movieFanart = FanartObject as MovieDB_FanartVM;
						return movieFanart.FullImagePath;

					case ImageEntityType.Trakt_Fanart:
						Trakt_ImageFanartVM traktFanart = FanartObject as Trakt_ImageFanartVM;
						return traktFanart.FullImagePath;
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
						TvDB_ImageFanartVM tvFanart = FanartObject as TvDB_ImageFanartVM;
						return tvFanart.FullThumbnailPath;

					case ImageEntityType.MovieDB_FanArt:
						MovieDB_FanartVM movieFanart = FanartObject as MovieDB_FanartVM;
						return movieFanart.FullImagePath;

					case ImageEntityType.Trakt_Fanart:
						Trakt_ImageFanartVM traktFanart = FanartObject as Trakt_ImageFanartVM;
						return traktFanart.FullImagePath;
				}

				return "";
			}
		}

		private bool isImageEnabled = false;
		public bool IsImageEnabled
		{
			get { return isImageEnabled; }
			set { isImageEnabled = value; }
		}

		private bool isImageDefault = false;
		public bool IsImageDefault
		{
			get { return isImageDefault; }
			set { isImageDefault = value; }
		}


		private string fanartSource = "";
		public string FanartSource
		{
			get { return fanartSource; }
			set
			{
				fanartSource = value;
			}
		}
	}
}
