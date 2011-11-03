using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyAnimePlugin3.Events
{
	public class ServerStatusEventArgs : EventArgs
	{
		public bool IsBanned { get; set; }
		public string BanReason  { get; set; }
		public int HasherQueueCount { get; set; }
		public string HasherQueueState { get; set; }
		public int GeneralQueueCount { get; set; }
		public string GeneralQueueState { get; set; }
		public bool HasherQueueRunning { get; set; }
		public bool GeneralQueueRunning { get; set; }

		public ServerStatusEventArgs()
		{
			
		}
	}
}
