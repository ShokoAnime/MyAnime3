using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyAnimePlugin3.ViewModel
{
	public class JMMUserVM
	{
		public int JMMUserID { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
		public int IsAdmin { get; set; }
		public int HideHentai { get; set; }
		public string HideCategories { get; set; }

		public bool IsAdminUser
		{
			get { return IsAdmin == 1; }
		}

		public JMMUserVM()
		{
		}

		public JMMUserVM(JMMServerBinary.Contract_JMMUser contract)
		{
			this.JMMUserID = contract.JMMUserID.Value;
			this.Username = contract.Username;
			this.Password = contract.Password;
			this.IsAdmin = contract.IsAdmin;
			this.HideHentai = 0;
			this.HideCategories = contract.HideCategories;
		}

		public override string ToString()
		{
			return string.Format("{0} - {1} ({2}) - {3}", Username, IsAdmin, HideHentai, HideCategories);
		}
	}
}
