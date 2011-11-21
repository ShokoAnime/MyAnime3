using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyAnimePlugin3.ConfigFiles
{
	public class ServerProfile
	{
		public string ProfileName { get; set; }
		public string ServerAddress { get; set; }
		public string ServerPort { get; set; }
		public string LastJMMUserID { get; set; }

		public ServerProfile()
		{
		}

		public ServerProfile(string settingString)
		{
			string[] values = settingString.Split(';');
			ProfileName = values[0].Trim();
			ServerAddress = values[1].Trim();
			ServerPort = values[2].Trim();
			LastJMMUserID = values[3].Trim();
		}

	}
}
