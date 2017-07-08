using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Shoko.Models.Client;

namespace Shoko.MyAnime3.ViewModel
{
	public class CL_CrossRef_AniDB_MAL_Response : CL_CrossRef_AniDB_MAL_Response
	{
		public int AnimeID { get; set; }
		public int MALID { get; set; }
		public int CrossRefSource { get; set; }
		public string MALTitle { get; set; }
		public int StartEpisodeType { get; set; }
		public int StartEpisodeNumber { get; set; }



		public CL_CrossRef_AniDB_MAL_Response()
		{
		}

		public CL_CrossRef_AniDB_MAL_Response(JMMServerBinary.Contract_CrossRef_AniDB_MALResult contract)
		{
			this.AnimeID = contract.AnimeID;
			this.MALID = contract.MALID;
			this.CrossRefSource = contract.CrossRefSource;
			this.MALTitle = contract.MALTitle;
			this.StartEpisodeType = contract.StartEpisodeType;
			this.StartEpisodeNumber = contract.StartEpisodeNumber;

		}

		public override string ToString()
		{
			return string.Format("{0} --- {1}", MALID, MALTitle);
		}
	}
}
