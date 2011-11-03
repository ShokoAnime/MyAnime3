using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MyAnimePlugin3.ViewModel
{
	public class TvDB_ImageFanartVM
	{
		public int TvDB_ImageFanartID { get; set; }
		public int Id { get; set; }
		public int SeriesID { get; set; }
		public string BannerPath { get; set; }
		public string BannerType { get; set; }
		public string BannerType2 { get; set; }
		public string Colors { get; set; }
		public string Language { get; set; }
		public string ThumbnailPath { get; set; }
		public string VignettePath { get; set; }
		public int Enabled { get; set; }
		public int Chosen { get; set; }

		public string FullImagePath
		{
			get
			{
				string fname = BannerPath;
				fname = BannerPath.Replace("/", @"\");
				return Path.Combine(Utils.GetTvDBImagePath(), fname);
			}
		}

		public string FullThumbnailPath
		{
			get
			{
				string fname = ThumbnailPath;
				fname = ThumbnailPath.Replace("/", @"\");
				return Path.Combine(Utils.GetTvDBImagePath(), fname);
			}
		}

		private bool isImageDefault = false;
		public bool IsImageDefault
		{
			get { return isImageDefault; }
			set { isImageDefault = value; }
		}


		public TvDB_ImageFanartVM(JMMServerBinary.Contract_TvDB_ImageFanart contract)
		{
			this.TvDB_ImageFanartID = contract.TvDB_ImageFanartID;
			this.Id = contract.Id;
			this.SeriesID = contract.SeriesID;
			this.BannerPath = contract.BannerPath;
			this.BannerType = contract.BannerType;
			this.BannerType2 = contract.BannerType2;
			this.Colors = contract.Colors;
			this.Language = contract.Language;
			this.ThumbnailPath = contract.ThumbnailPath;
			this.VignettePath = contract.VignettePath;
			this.Enabled = contract.Enabled;
			this.Chosen = contract.Chosen;
		}
	}
}
