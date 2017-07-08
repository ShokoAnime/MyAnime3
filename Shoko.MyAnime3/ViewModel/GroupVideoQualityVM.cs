using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Shoko.Models.Client;

namespace Shoko.MyAnime3.ViewModel
{
	public class CL_GroupVideoQuality : CL_GroupVideoQuality
	{
		public string GroupName { get; set; }
		public string GroupNameShort { get; set; }
		public int Ranking { get; set; }
		public string Resolution { get; set; }
		public string VideoSource { get; set; }
		public int VideoBitDepth { get; set; }
		public int FileCountNormal { get; set; }
		public bool NormalComplete { get; set; }
		public int FileCountSpecials { get; set; }
		public bool SpecialsComplete { get; set; }

		public List<int> NormalEpisodeNumbers { get; set; }
		public string NormalEpisodeNumberSummary { get; set; }

	    public CL_GroupVideoQuality(JMMServerBinary.Contract_GroupVideoQuality contract)
		{
			this.GroupName = contract.GroupName;
			this.GroupNameShort = contract.GroupNameShort;
			this.Ranking = contract.Ranking;
			this.Resolution = contract.Resolution;
			this.VideoSource = contract.VideoSource;
			this.FileCountNormal = contract.FileCountNormal;
			this.FileCountSpecials = contract.FileCountSpecials;
			this.NormalComplete = contract.NormalComplete;
			this.SpecialsComplete = contract.SpecialsComplete;
			this.NormalEpisodeNumbers = new List<int>(contract.NormalEpisodeNumbers);
			this.NormalEpisodeNumberSummary = contract.NormalEpisodeNumberSummary;

			this.VideoBitDepth = contract.VideoBitDepth;
		}

	    public override string ToString()
		{
			return string.Format("{0} - {1}/{2} - {3}/{4} "+ Translation.Files, GroupNameShort, Resolution, VideoSource, FileCountNormal, FileCountSpecials);
		}
	}
}
