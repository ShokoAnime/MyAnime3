using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyAnimePlugin3.Events
{
	public class GotCharacterImagesEventArgs : EventArgs
	{
		public readonly int AniDB_SeiyuuID = 0;

		public GotCharacterImagesEventArgs(int aniDB_SeiyuuID)
		{
			this.AniDB_SeiyuuID = aniDB_SeiyuuID;
		}
	}
}
