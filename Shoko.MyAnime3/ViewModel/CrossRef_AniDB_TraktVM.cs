using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Shoko.Models.Server;

namespace Shoko.MyAnime3.ViewModel
{
	public class CrossRef_AniDB_TraktV2 : CrossRef_AniDB_TraktV2
	{
		public int CrossRef_AniDB_TraktID { get; set; }
		public int AnimeID { get; set; }
		public string TraktID { get; set; }
		public int TraktSeasonNumber { get; set; }
		public int CrossRefSource { get; set; }

		public CrossRef_AniDB_TraktV2()
		{
		}

		public CrossRef_AniDB_TraktV2(JMMServerBinary.Contract_CrossRef_AniDB_TraktV2 contract)
		{
            this.CrossRef_AniDB_TraktID = contract.CrossRef_AniDB_TraktV2ID;
			this.AnimeID = contract.AnimeID;
			this.TraktID = contract.TraktID;
			this.TraktSeasonNumber = contract.TraktSeasonNumber;
			this.CrossRefSource = contract.CrossRefSource;
		}

		public override string ToString()
		{
			return string.Format("{0} = {1} Season # {2}", AnimeID, TraktID, TraktSeasonNumber);
		}
	}
}
