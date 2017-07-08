using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Shoko.Models.Enums;
using Shoko.MyAnime3.ImageManagement;

namespace Shoko.MyAnime3.ViewModel
{
	public class Trakt_Episode
	{
		public int Trakt_EpisodeID { get; set; }
		public int Trakt_ShowID { get; set; }
		public int Season { get; set; }
		public int EpisodeNumber { get; set; }
		public string Title { get; set; }
		public string URL { get; set; }
		public string Overview { get; set; }
		public string EpisodeImage { get; set; }

		public Trakt_Episode(JMMServerBinary.Contract_Trakt_Episode contract)
		{
			this.Trakt_EpisodeID = contract.Trakt_EpisodeID;
			this.Trakt_ShowID = contract.Trakt_ShowID;
			this.Season = contract.Season;
			this.EpisodeNumber = contract.EpisodeNumber;
			this.Title = contract.Title;
			this.URL = contract.URL;
			this.Overview = contract.Overview;
			this.EpisodeImage = contract.EpisodeImage;
		}
	}
}
