using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyAnimePlugin3.ViewModel
{
	public class GroupFilterConditionVM
	{
		public int? GroupFilterConditionID { get; set; }
		public int? GroupFilterID { get; set; }
		public int ConditionType { get; set; }
		public int ConditionOperator { get; set; }
		public string ConditionParameter { get; set; }

		public GroupFilterConditionType ConditionTypeEnum
		{
			get { return (GroupFilterConditionType)ConditionType; }
		}

		public GroupFilterOperator ConditionOperatorEnum
		{
			get { return (GroupFilterOperator)ConditionOperator; }
		}

		public GroupFilterConditionVM()
		{
			GroupFilterConditionID = null;
			GroupFilterID = null;
			ConditionType = 1;
			ConditionOperator = 1;
			ConditionParameter = "";
		}


		public GroupFilterConditionVM(JMMServerBinary.Contract_GroupFilterCondition contract)
		{
			// read only members
			this.GroupFilterConditionID = contract.GroupFilterConditionID;
			this.GroupFilterID = contract.GroupFilterID;
			this.ConditionOperator = contract.ConditionOperator;
			this.ConditionParameter = contract.ConditionParameter;
			this.ConditionType = contract.ConditionType;
		}

		public JMMServerBinary.Contract_GroupFilterCondition ToContract()
		{
			JMMServerBinary.Contract_GroupFilterCondition contract = new JMMServerBinary.Contract_GroupFilterCondition();
			contract.GroupFilterConditionID = this.GroupFilterConditionID;
			contract.GroupFilterID = this.GroupFilterID;
			contract.ConditionOperator = this.ConditionOperator;
			contract.ConditionParameter = this.ConditionParameter;
			contract.ConditionType = this.ConditionType;

			return contract;
		}
	}
}
