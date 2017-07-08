using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shoko.MyAnime3.ViewModel
{
	public class AniDB_Vote 
	{
		public int EntityID { get; set; }
		public decimal VoteValue { get; set; }
		public int Shoko.Models.Enums.VoteType { get; set; }

		public AniDB_Vote()
		{
		}

		public AniDB_Vote(JMMServerBinary.Contract_AniDBVote contract)
		{
			this.EntityID = contract.EntityID;
			this.VoteValue = contract.VoteValue;
			this.Shoko.Models.Enums.VoteType = contract.Shoko.Models.Enums.VoteType;
		}

		public override string ToString()
		{
			return string.Format("{0} - {1}", EntityID, VoteValue);
		}
	}
}
