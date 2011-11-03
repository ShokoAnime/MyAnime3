using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyAnimePlugin3.ViewModel
{
	public class TvDB_SeriesVM
	{
		public int TvDB_SeriesID { get; set; }
		public int SeriesID { get; set; }
		public string SeriesName { get; set; }
		public string Overview { get; set; }
		public string Status { get; set; }
		public string Banner { get; set; }
		public string Fanart { get; set; }
		public string Lastupdated { get; set; }
		public string Poster { get; set; }

		public string SeriesURL
		{
			get { return string.Format(Constants.URLS.TvDB_Series, SeriesID); }
			
		}

		public TvDB_SeriesVM(JMMServerBinary.Contract_TvDB_Series contract)
		{
			this.TvDB_SeriesID = contract.TvDB_SeriesID;
			this.SeriesID = contract.SeriesID;
			this.Overview = contract.Overview;
			this.SeriesName = contract.SeriesName;
			this.Status = contract.Status;
			this.Banner = contract.Banner;
			this.Fanart = contract.Fanart;
			this.Lastupdated = contract.Lastupdated;
			this.Poster = contract.Poster;
		}
	}
}
