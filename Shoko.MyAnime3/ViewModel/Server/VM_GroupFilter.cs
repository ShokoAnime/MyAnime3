using System;
using Shoko.Models.Client;
using Shoko.MyAnime3.ViewModel.Helpers;

namespace Shoko.MyAnime3.ViewModel.Server
{
	public class VM_GroupFilter : CL_GroupFilter, IComparable<VM_GroupFilter>, IVM
	{

	    public int SeriesCount => Series.Count;
	    public int GroupCount => Groups.Count;
	    public int FilterCount => Childs.Count;

		public VM_AnimeGroup_User RandomGroup { get; set; }
	    public VM_GroupFilter ParentFilter { get; set; } = null;

		public string PredefinedCriteria { get; set; }

	  


		public int CompareTo(VM_GroupFilter obj)
		{
			return String.Compare(GroupFilterName, obj.GroupFilterName, StringComparison.Ordinal);
		}

		public override string ToString()
		{
			return $"{GroupFilterID} - {GroupFilterName}"; 
		}
	}
}
