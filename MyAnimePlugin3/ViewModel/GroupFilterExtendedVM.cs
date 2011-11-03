using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyAnimePlugin3.ViewModel
{
	public class GroupFilterExtendedVM
	{
		public GroupFilterVM GroupFilter { get; set; }

		public int SeriesCount { get; set; }
		public int GroupCount { get; set; }


		public GroupFilterExtendedVM(JMMServerBinary.Contract_GroupFilterExtended contract)
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
