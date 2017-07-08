using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Shoko.Models.Server;

namespace Shoko.MyAnime3.ViewModel
{
	public class CrossRef_AniDB_MAL : CrossRef_AniDB_MAL
	{
		public int CrossRef_AniDB_MALID { get; set; }
		public int AnimeID { get; set; }
		public int MALID { get; set; }
		public string MALTitle { get; set; }
		public int CrossRefSource { get; set; }
		public int StartEpisodeType { get; set; }
		public int StartEpisodeNumber { get; set; }

		public CrossRef_AniDB_MAL()
		{
		}

		public CrossRef_AniDB_MAL(JMMServerBinary.Contract_CrossRef_AniDB_MAL contract)
		{
			this.CrossRef_AniDB_MALID = contract.CrossRef_AniDB_MALID;
			this.AnimeID = contract.AnimeID;
			this.MALID = contract.MALID;
			this.MALTitle = contract.MALTitle;
			this.CrossRefSource = contract.CrossRefSource;
			this.StartEpisodeType = contract.StartEpisodeType;
			this.StartEpisodeNumber = contract.StartEpisodeNumber;
		}

		public override string ToString()
		{
			return string.Format("{0} = {1} - {2}", AnimeID, MALID, MALTitle);
		}
	}
}
