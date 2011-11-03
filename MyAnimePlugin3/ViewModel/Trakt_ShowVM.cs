using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyAnimePlugin3.ViewModel
{
	public class Trakt_ShowVM
	{
		public int Trakt_ShowID { get; set; }
		public string TraktID { get; set; }
		public string Title { get; set; }
		public string Year { get; set; }
		public string URL { get; set; }
		public string Overview { get; set; }
		public int? TvDB_ID { get; set; }
		public List<Trakt_SeasonVM> Seasons { get; set; }

		public string ShowURL
		{
			get { return URL; }
			
		}

		public Trakt_ShowVM(JMMServerBinary.Contract_Trakt_Show contract)
		{
			this.Trakt_ShowID = contract.Trakt_ShowID;
			this.TraktID = contract.TraktID;
			this.Year = contract.Year;
			this.Overview = contract.Overview;
			this.TvDB_ID = contract.TvDB_ID;
			this.URL = contract.URL;
			this.Seasons = new List<Trakt_SeasonVM>();
			foreach (JMMServerBinary.Contract_Trakt_Season season in contract.Seasons)
				Seasons.Add(new Trakt_SeasonVM(season));

			this.Title = contract.Title;
		}
	}
}
