using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Shoko.Models.TvDB;

namespace Shoko.MyAnime3.ViewModel
{
	public class TVDB_Series_Search_Response : TVDB_Series_Search_Response
	{
		public string Id { get; set; }
		public int SeriesID { get; set; }
		public string Overview { get; set; }
		public string SeriesName { get; set; }
		public string Banner { get; set; }
		public string Language { get; set; }

		public TVDB_Series_Search_Response()
		{
		}

		public TVDB_Series_Search_Response(JMMServerBinary.Contract_TVDBSeriesSearchResult contract)
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
