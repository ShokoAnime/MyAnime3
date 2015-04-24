using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyAnimePlugin3.ViewModel
{
	public class CrossRef_AniDB_TraktResultVM
	{
		public int AnimeID { get; set; }
		public string TraktID { get; set; }
		public int TraktSeasonNumber { get; set; }
		public int AdminApproved { get; set; }
		public string ShowName { get; set; }

		public CrossRef_AniDB_TraktResultVM()
		{
		}

        public CrossRef_AniDB_TraktResultVM(JMMServerBinary.Contract_Azure_CrossRef_AniDB_Trakt contract)
		{
			this.AnimeID = contract.AnimeID;
			this.TraktID = contract.TraktID;
			this.TraktSeasonNumber = contract.TraktSeasonNumber;
			this.AdminApproved = contract.IsAdminApproved;
			this.ShowName = contract.TraktTitle;
		}

		public override string ToString()
		{
			return string.Format("{0} = {1} Season # {2}", AnimeID, TraktID, TraktSeasonNumber);
		}
	}
}
