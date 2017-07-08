using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Shoko.Models.Enums;
using Shoko.Models.Server;
using Shoko.MyAnime3.ImageManagement;

namespace Shoko.MyAnime3.ViewModel
{
	public class TvDB_Episode : TvDB_Episode
	{
		public int TvDB_EpisodeID { get; set; }
		public int Id { get; set; }
		public int SeriesID { get; set; }
		public int SeasonID { get; set; }
		public int SeasonNumber { get; set; }
		public int EpisodeNumber { get; set; }
		public string EpisodeName { get; set; }
		public string Overview { get; set; }
		public string Filename { get; set; }
		public int EpImgFlag { get; set; }
		public int? AbsoluteNumber { get; set; }
		public int? AirsAfterSeason { get; set; }
		public int? AirsBeforeEpisode { get; set; }
		public int? AirsBeforeSeason { get; set; }

	    public TvDB_Episode(JMMServerBinary.Contract_TvDB_Episode contract)
		{
			this.TvDB_EpisodeID = contract.TvDB_EpisodeID;
			this.Id = contract.Id;
			this.SeriesID = contract.SeriesID;
			this.SeasonID = contract.SeasonID;
			this.SeasonNumber = contract.SeasonNumber;
			this.EpisodeNumber = contract.EpisodeNumber;
			this.EpisodeName = contract.EpisodeName;
			this.Overview = contract.Overview;
			this.Filename = contract.Filename;
			this.EpImgFlag = contract.EpImgFlag;
			this.AbsoluteNumber = contract.AbsoluteNumber;

			this.AirsAfterSeason = contract.AirsAfterSeason;
			this.AirsBeforeEpisode = contract.AirsBeforeEpisode;
			this.AirsBeforeSeason = contract.AirsBeforeSeason;
		}
	}
}
