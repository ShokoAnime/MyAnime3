using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shoko.MyAnime3.ViewModel
{
	public class MovieDB_Movie
	{
		public int MovieDB_MovieID { get; set; }
		public int MovieId { get; set; }
		public string MovieName { get; set; }
		public string OriginalName { get; set; }
		public string Overview { get; set; }

		public string SiteURL
		{
			get { return string.Format(Constants.URLS.MovieDB_Series, MovieId); }
		}

		public MovieDB_Movie(JMMServerBinary.Contract_MovieDB_Movie contract)
		{
			this.MovieDB_MovieID = contract.MovieDB_MovieID;
			this.MovieId = contract.MovieId;
			this.MovieName = contract.MovieName;
			this.OriginalName = contract.OriginalName;
			this.Overview = contract.Overview;
		}
	}
}
