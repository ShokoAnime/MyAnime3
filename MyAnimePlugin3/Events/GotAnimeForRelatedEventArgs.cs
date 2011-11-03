using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyAnimePlugin3.Events
{
	/// <summary>
	/// This event occurs when all the related anime for an anime have beedn download on the server
	/// </summary>
	public class GotAnimeForRelatedEventArgs : EventArgs
	{
		public readonly int AnimeID = 0;

		public GotAnimeForRelatedEventArgs(int animeID)
		{
			this.AnimeID = animeID;
		}
	}
}
