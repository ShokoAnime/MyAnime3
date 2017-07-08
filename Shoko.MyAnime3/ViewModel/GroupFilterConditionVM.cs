using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Shoko.Models.Server;

namespace Shoko.MyAnime3.ViewModel
{
	public class GroupFilterCondition : GroupFilterCondition
	{
		public int? GroupFilterConditionID { get; set; }
		public int? GroupFilterID { get; set; }
		public int ConditionType { get; set; }
		public int ConditionOperator { get; set; }
		public string ConditionParameter { get; set; }



		public GroupFilterCondition()
		{
			GroupFilterConditionID = null;
			GroupFilterID = null;
			ConditionType = 1;
			ConditionOperator = 1;
			ConditionParameter = "";
		}


		public GroupFilterCondition(JMMServerBinary.Contract_GroupFilterCondition contract)
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
