using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Shoko.Models.Server;

namespace Shoko.MyAnime3.ViewModel
{
	public class AniDB_ReleaseGroup : AniDB_ReleaseGroup
	{
		public int GroupID { get; set; }
		public int Rating { get; set; }
		public int Votes { get; set; }
		public int AnimeCount { get; set; }
		public int FileCount { get; set; }
		public string GroupName { get; set; }
		public string GroupNameShort { get; set; }
		public string IRCChannel { get; set; }
		public string IRCServer { get; set; }
		public string URL { get; set; }
		public string Picname { get; set; }

		public AniDB_ReleaseGroup()
		{

		}

		public void Populate(JMMServerBinary.Contract_ReleaseGroup contract)
		{
			this.GroupID = contract.GroupID;
			this.Rating = contract.Rating;
			this.Votes = contract.Votes;
			this.AnimeCount = contract.AnimeCount;
			this.FileCount = contract.FileCount;
			this.GroupName = contract.GroupName;
			this.GroupNameShort = contract.GroupNameShort;
			this.IRCChannel = contract.IRCChannel;
			this.IRCServer = contract.IRCServer;
			this.URL = contract.URL;
			this.Picname = contract.Picname;
		}

		public AniDB_ReleaseGroup(JMMServerBinary.Contract_ReleaseGroup contract)
		{
			Populate(contract);

		}
	}
}
