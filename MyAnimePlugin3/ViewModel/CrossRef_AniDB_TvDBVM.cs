using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyAnimePlugin3.ViewModel
{
	public class CrossRef_AniDB_TvDBVMV2
	{
		public int CrossRef_AniDB_TvDBV2ID { get; set; }
		public int AnimeID { get; set; }
		public int AniDBStartEpisodeType { get; set; }
		public int AniDBStartEpisodeNumber { get; set; }
		public int TvDBID { get; set; }
		public int TvDBSeasonNumber { get; set; }
		public int TvDBStartEpisodeNumber { get; set; }
		public string TvDBTitle { get; set; }
		public int CrossRefSource { get; set; }

		public string AniDBStartEpisodeNumberString
		{
			get
			{
				return string.Format("# {0}", AniDBStartEpisodeNumber);
			}
		}

		public string TvDBSeasonNumberString
		{
			get
			{
				return string.Format("S{0}", TvDBSeasonNumber);
			}
		}

		public string TvDBStartEpisodeNumberString
		{
			get
			{
				return string.Format("EP# {0}", TvDBStartEpisodeNumber);
			}
		}

		public CrossRef_AniDB_TvDBVMV2()
		{
		}

		public CrossRef_AniDB_TvDBVMV2(JMMServerBinary.Contract_CrossRef_AniDB_TvDBV2 contract)
		{
			this.AnimeID = contract.AnimeID;
			this.AniDBStartEpisodeType = contract.AniDBStartEpisodeType;
			this.AniDBStartEpisodeNumber = contract.AniDBStartEpisodeNumber;
			this.TvDBID = contract.TvDBID;
			this.CrossRef_AniDB_TvDBV2ID = contract.CrossRef_AniDB_TvDBV2ID;
			this.TvDBSeasonNumber = contract.TvDBSeasonNumber;
			this.TvDBStartEpisodeNumber = contract.TvDBStartEpisodeNumber;
			this.CrossRefSource = contract.CrossRefSource;
			this.TvDBTitle = contract.TvDBTitle;
		}

		public CrossRef_AniDB_TvDBVMV2(JMMServerBinary.Contract_Azure_CrossRef_AniDB_TvDB contract)
		{
			this.AnimeID = contract.AnimeID;
			this.AniDBStartEpisodeType = contract.AniDBStartEpisodeType;
			this.AniDBStartEpisodeNumber = contract.AniDBStartEpisodeNumber;
			this.TvDBID = contract.TvDBID;
			//this.CrossRef_AniDB_TvDBV2ID = contract.CrossRef_AniDB_TvDBV2ID;
			this.TvDBSeasonNumber = contract.TvDBSeasonNumber;
			this.TvDBStartEpisodeNumber = contract.TvDBStartEpisodeNumber;
			this.CrossRefSource = contract.CrossRefSource;
			this.TvDBTitle = contract.TvDBTitle;
		}

		public override string ToString()
		{
			return string.Format("{0} = {1}   AniDB # {2}:{3}   TvDB {4}:{5}", AnimeID, TvDBID, AniDBStartEpisodeType, AniDBStartEpisodeNumber, TvDBSeasonNumber, TvDBStartEpisodeNumber);
		}
	}
}
