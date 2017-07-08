using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Shoko.Models.Client;

namespace Shoko.MyAnime3.ViewModel
{
	public class CL_TraktTVShowResponse : CL_TraktTVShowResponse
	{
		public string title { get; set; }
		public string year { get; set; }
		public string url { get; set; }
		public string first_aired { get; set; }
		public string country { get; set; }
		public string overview { get; set; }
		public string tvdb_id { get; set; }

	    public CL_TraktTVShowResponse()
		{
		}

		public CL_TraktTVShowResponse(JMMServerBinary.Contract_TraktTVShowResponse contract)
		{
			this.title = contract.title;
			this.year = contract.year;
			this.url = contract.url;
			this.first_aired = contract.first_aired;
			this.country = contract.country;
			this.overview = contract.overview;
			this.tvdb_id = contract.tvdb_id;
		}

		public override string ToString()
		{
			return string.Format("{0} ({1}) - {2}", title, year, overview);
		}
	}
}
