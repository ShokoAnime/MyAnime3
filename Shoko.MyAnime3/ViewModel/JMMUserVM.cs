using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Shoko.Models.Server;
using Shoko.MyAnime3.Extensions;

namespace Shoko.MyAnime3.ViewModel
{
	public class JMMUser : JMMUser
	{
		public int JMMUserID { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
		public int IsAdmin { get; set; }
		public int IsAniDBUser { get; set; }
		public int IsTraktUser { get; set; }
		public HashSet<string> HideTags { get; set; }

	    public JMMUser()
		{
		}

		public JMMUser(JMMServerBinary.Contract_JMMUser contract)
		{
			this.JMMUserID = contract.JMMUserID.Value;
			this.Username = contract.Username;
			this.Password = contract.Password;
			this.IsAdmin = contract.IsAdmin;
			this.IsAniDBUser = contract.IsAniDBUser;
			this.IsTraktUser = contract.IsTraktUser;
			this.HideTags = new HashSet<string>(contract.HideCategories,StringComparer.InvariantCultureIgnoreCase);
		}

		public override string ToString()
		{
			return string.Format("{0} - {1} ({2}) - {3}", Username, ModelExtensions.GetIsAdminUser(this), ModelExtensions.GetIsAniDBUserBool(this), HideTags);
		}
	}
}
