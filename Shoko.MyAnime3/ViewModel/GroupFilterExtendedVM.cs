using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Shoko.Models.Client;

namespace Shoko.MyAnime3.ViewModel
{
	public class CL_GroupFilterExtended : CL_GroupFilterExtended
	{
		public GroupFilterVM GroupFilter { get; set; }

		public int SeriesCount { get; set; }
		public int GroupCount { get; set; }


		public CL_GroupFilterExtended(JMMServerBinary.Contract_GroupFilterExtended contract)
		{
			this.GroupFilter = new GroupFilterVM(contract.GroupFilter);

			SeriesCount = contract.SeriesCount;
			GroupCount = contract.GroupCount;
		}

		public override string ToString()
		{
			return string.Format("Group Filter: {0} - Groups: {1}, Series: {2}", GroupFilter.GroupFilterName, GroupCount, SeriesCount);
		}
	}
}
