using System;
using System.Collections.Generic;
using System.Text;

namespace MyAnimePlugin3.Downloads
{
	public interface ITorrentSource
	{
		string GetSourceName();
		string GetSourceLongName();
		List<TorrentLink> GetTorrents(List<string> searchParms);
		bool SupportsSearching();
		bool SupportsBrowsing();
		bool SupportsCRCMatching();
	}
}
