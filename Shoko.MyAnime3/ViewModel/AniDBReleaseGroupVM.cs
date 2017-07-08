﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shoko.MyAnime3.ViewModel
{
	public class AniDBReleaseGroupVM
	{
		public int GroupID { get; set; }
		public string GroupName { get; set; }
		public bool UserCollecting { get; set; }
		public int FileCount { get; set; }
		public string EpisodeRange { get; set; }

		public AniDBReleaseGroupVM()
		{
		}

		public AniDBReleaseGroupVM(JMMServerBinary.Contract_AniDBReleaseGroup contract)
		{
			this.GroupID = contract.GroupID;
			this.GroupName = contract.GroupName;
			this.UserCollecting = contract.UserCollecting;
			this.FileCount = contract.FileCount;
			this.EpisodeRange = contract.EpisodeRange;
		}

		public override string ToString()
		{
			return string.Format("{0} - {1} (Collecting: {2}) (Files: {3})", GroupID, GroupName, UserCollecting, FileCount);
		}
	}
}
