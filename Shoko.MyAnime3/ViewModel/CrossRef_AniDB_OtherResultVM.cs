using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Shoko.Models.Client;

namespace Shoko.MyAnime3.ViewModel
{
	public class CL_CrossRef_AniDB_Other_Response : CL_CrossRef_AniDB_Other_Response
	{
		public int AnimeID { get; set; }
		public string CrossRefID { get; set; }

		public CL_CrossRef_AniDB_Other_Response()
		{
		}

		public CL_CrossRef_AniDB_Other_Response(JMMServerBinary.Contract_CrossRef_AniDB_OtherResult contract)
		{
			this.AnimeID = contract.AnimeID;
			this.CrossRefID = contract.CrossRefID;

		}

		public override string ToString()
		{
			return string.Format("{0} = {1}", AnimeID, CrossRefID);
		}
	}
}
