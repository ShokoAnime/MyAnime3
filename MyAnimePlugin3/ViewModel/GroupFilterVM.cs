using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyAnimePlugin3.Windows;

namespace MyAnimePlugin3.ViewModel
{
	public class GroupFilterVM : IComparable<GroupFilterVM>, IVM
	{
		public int? GroupFilterID { get; set; }
		public string GroupFilterName { get; set; }
		public int ApplyToSeries { get; set; }
		public int BaseCondition { get; set; }
		public string SortingCriteria { get; set; }

		public int SeriesCount { get; set; }
		public int GroupCount { get; set; }
        public int FilterCount { get; set; }
        public int FilterType { get; set; }




		public AnimeGroupVM RandomGroup { get; set; }
	    public GroupFilterVM ParentFilter { get; set; } = null;

		public string PredefinedCriteria { get; set; }

		public List<GroupFilterConditionVM> FilterConditions { get; set; }
		public List<GroupFilterSortingCriteria> SortCriteriaList { get; set; }

        public Dictionary<int, HashSet<int>> Groups { get; set; }
        public Dictionary<int, HashSet<int>> Series { get; set; }
        public HashSet<int> Childs { get; set; }



        public Boolean IsApplyToSeries
		{
			get { return ApplyToSeries == 1; }
			set
			{
				ApplyToSeries = value ? 1 : 0;
			}
		}

		public GroupFilterVM()
		{
			GroupFilterID = null;
			GroupCount = 0;
			SeriesCount = 0;
			this.SortCriteriaList = new List<GroupFilterSortingCriteria>();
			this.FilterConditions = new List<GroupFilterConditionVM>();
		}


		public GroupFilterVM(JMMServerBinary.Contract_GroupFilter contract)
		{
			this.SortCriteriaList = new List<GroupFilterSortingCriteria>();
			this.FilterConditions = new List<GroupFilterConditionVM>();

			// read only members
			Populate(contract);
		}

		public void Populate(JMMServerBinary.Contract_GroupFilter contract)
		{
			this.GroupFilterID = contract.GroupFilterID;
			this.GroupFilterName = contract.GroupFilterName;
			this.ApplyToSeries = contract.ApplyToSeries;
			this.BaseCondition = contract.BaseCondition;
			this.PredefinedCriteria = "";
			this.FilterConditions.Clear();
            this.FilterType = contract.FilterType;

            this.Groups = contract.Groups.ToDictionary(a => a.Key, a => new HashSet<int>(a.Value));
            this.Series = contract.Series.ToDictionary(a => a.Key, a => new HashSet<int>(a.Value));
            this.Childs = new HashSet<int>(contract.Childs);
            GroupCount = Groups.ContainsKey(JMMServerVM.Instance.CurrentUser.JMMUserID) ? Groups[JMMServerVM.Instance.CurrentUser.JMMUserID].Count : 0;
            SeriesCount = Series.ContainsKey(JMMServerVM.Instance.CurrentUser.JMMUserID) ? Series[JMMServerVM.Instance.CurrentUser.JMMUserID].Count : 0;
            FilterCount = Childs.Count;


            if (contract.FilterConditions != null)
			{
				foreach (JMMServerBinary.Contract_GroupFilterCondition gfc_con in contract.FilterConditions)
					FilterConditions.Add(new GroupFilterConditionVM(gfc_con));
			}
			//SortCriteriaList = new ObservableCollection<GroupFilterSortingCriteria>();
			SortCriteriaList.Clear();

			string sortCriteriaRaw = contract.SortingCriteria;

			if (!string.IsNullOrEmpty(sortCriteriaRaw))
			{
				string[] scrit = sortCriteriaRaw.Split('|');
				foreach (string sortpair in scrit)
				{
					string[] spair = sortpair.Split(';');
					if (spair.Length != 2) continue;

					int stype = 0;
					int sdir = 0;

					int.TryParse(spair[0], out stype);
					int.TryParse(spair[1], out sdir);

					if (stype > 0 && sdir > 0)
					{
						GroupFilterSortingCriteria gfsc = new GroupFilterSortingCriteria();
						gfsc.GroupFilterID = this.GroupFilterID;
						gfsc.SortType = (GroupFilterSorting)stype;
						gfsc.SortDirection = (GroupFilterSortDirection)sdir;
						SortCriteriaList.Add(gfsc);
					}
				}
			}

			//FilterConditions = new List<GroupFilterConditionVM>(FilterConditions.OrderBy(p => p.ConditionTypeString));
		}



		public int CompareTo(GroupFilterVM obj)
		{
			return GroupFilterName.CompareTo(obj.GroupFilterName);
		}

		public override string ToString()
		{
			return string.Format("{0} - {1}", GroupFilterID, GroupFilterName); ;
		}
	}
}
