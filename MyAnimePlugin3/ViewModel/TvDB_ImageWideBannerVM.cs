using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MyAnimePlugin3.ViewModel
{
	public class TvDB_ImageWideBannerVM
	{
		public int TvDB_ImageWideBannerID { get; set; }
		public int Id { get; set; }
		public int SeriesID { get; set; }
		public string BannerPath { get; set; }
		public string BannerType { get; set; }
		public string BannerType2 { get; set; }
		public string Language { get; set; }
		public int Enabled { get; set; }
		public int? SeasonNumber { get; set; }

		public string FullImagePath
		{
			get
			{
				string fname = BannerPath;
				fname = BannerPath.Replace("/", @"\");
				return Path.Combine(Utils.GetTvDBImagePath(), fname);
			}
		}

		private bool isImageDefault = false;
		public bool IsImageDefault
		{
			get { return isImageDefault; }
			set { isImageDefault = value; }
		}

		public TvDB_ImageWideBannerVM(JMMServerBinary.Contract_TvDB_ImageWideBanner contract)
		{
			this.TvDB_ImageWideBannerID = contract.TvDB_ImageWideBannerID;
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
