using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Shoko.Models.Server;

namespace Shoko.MyAnime3.ViewModel
{
	public class CrossRef_AniDB_Other : CrossRef_AniDB_Other
	{
		public int CrossRef_AniDB_OtherID { get; set; }
		public int AnimeID { get; set; }
		public string CrossRefID { get; set; }
		public int CrossRefSource { get; set; }
		public int Shoko.Models.Enums.CrossRefType { get; set; }

		public CrossRef_AniDB_Other()
		{
		}

		public CrossRef_AniDB_Other(JMMServerBinary.Contract_CrossRef_AniDB_Other contract)
		{
			this.CrossRef_AniDB_OtherID = contract.CrossRef_AniDB_OtherID;
			this.AnimeID = contract.AnimeID;
			this.CrossRefID = contract.CrossRefID;
			this.CrossRefSource = contract.CrossRefSource;
			this.Shoko.Models.Enums.CrossRefType = contract.Shoko.Models.Enums.CrossRefType;
		}

		public override string ToString()
		{
			return string.Format("{0} = {1} Type {2}", AnimeID, CrossRefID, Shoko.Models.Enums.CrossRefType);
		}
	}
}
