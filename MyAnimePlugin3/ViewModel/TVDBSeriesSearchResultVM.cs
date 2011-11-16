using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyAnimePlugin3.ViewModel
{
	public class TVDBSeriesSearchResultVM
	{
		public string Id { get; set; }
		public int SeriesID { get; set; }
		public string Overview { get; set; }
		public string SeriesName { get; set; }
		public string Banner { get; set; }
		public string Language { get; set; }

		public TVDBSeriesSearchResultVM()
		{
		}

		public TVDBSeriesSearchResultVM(JMMServerBinary.Contract_TVDBSeriesSearchResult contract)
		{
			this.Id = contract.Id;
			this.SeriesID = contract.SeriesID;
			this.Overview = contract.Overview;
			this.SeriesName = contract.SeriesName;
			this.Banner = contract.Banner;
			this.Language = contract.Language;
		}

		public override string ToString()
		{
			return string.Format("{0} --- {1} ({2})", SeriesID, SeriesName, Language);
		}
	}
}
