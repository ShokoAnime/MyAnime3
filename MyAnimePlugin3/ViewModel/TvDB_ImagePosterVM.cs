using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using MyAnimePlugin3.ImageManagement;

namespace MyAnimePlugin3.ViewModel
{
	public class TvDB_ImagePosterVM
	{
		public int TvDB_ImagePosterID { get; set; }
		public int Id { get; set; }
		public int SeriesID { get; set; }
		public string BannerPath { get; set; }
		public string BannerType { get; set; }
		public string BannerType2 { get; set; }
		public string Language { get; set; }
		public int Enabled { get; set; }
		public int? SeasonNumber { get; set; }

		public string FullImagePathPlain
		{
			get
			{
				if (string.IsNullOrEmpty(BannerPath)) return "";

				string fname = BannerPath;
				fname = BannerPath.Replace("/", @"\");
				return Path.Combine(Utils.GetTvDBImagePath(), fname);
			}
		}

		public string FullImagePath
		{
			get
			{
				if (string.IsNullOrEmpty(FullImagePathPlain)) return FullImagePathPlain;

				if (!File.Exists(FullImagePathPlain))
				{
					ImageDownloadRequest req = new ImageDownloadRequest(ImageEntityType.TvDB_Cover, this, false);
					MainWindow.imageHelper.DownloadImage(req);
					if (File.Exists(FullImagePathPlain)) return FullImagePathPlain;
				}

				return FullImagePathPlain;
			}
		}

		private bool isImageDefault = false;
		public bool IsImageDefault
		{
			get { return isImageDefault; }
			set { isImageDefault = value; }
		}

		public TvDB_ImagePosterVM(JMMServerBinary.Contract_TvDB_ImagePoster contract)
		{
			this.TvDB_ImagePosterID = contract.TvDB_ImagePosterID;
			this.Id = contract.Id;
			this.SeriesID = contract.SeriesID;
			this.BannerPath = contract.BannerPath;
			this.BannerType = contract.BannerType;
			this.BannerType2 = contract.BannerType2;
			this.Language = contract.Language;
			this.Enabled = contract.Enabled;
			this.SeasonNumber = contract.SeasonNumber;
		}
	}
}
