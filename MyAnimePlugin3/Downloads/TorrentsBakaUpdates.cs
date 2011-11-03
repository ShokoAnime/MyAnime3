using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace MyAnimePlugin3.Downloads
{
	public class TorrentsBakaUpdates : ITorrentSource
	{
		#region ITorrentSource Members

		public string GetSourceName()
		{
			return "BU";
		}

		public string GetSourceLongName()
		{
			return "Baka Updates";
		}

		public bool SupportsSearching()
		{
			return true;
		}

		public bool SupportsBrowsing()
		{
			return true;
		}

		public bool SupportsCRCMatching()
		{
			return false;
		}


		private List<TorrentLink> ParseSourceBrowse(string output)
		{
			List<TorrentLink> torLinks = new List<TorrentLink>();

			char q = (char)34;
			string quote = q.ToString();

			string startBlock = "rlsheader";

			string nameStart = "class=" + quote + "thereleasetitle";
			string nameStart2 = ">";
			string nameEnd = "</a>";

			string epnoEnd = "</td>";

			string groupStart = "<a href=" + quote + "/groups/info/name";
			string groupStart2 = ">";
			string groupEnd = "</a>";

			string torStart = "<a class='btlink' href=" + quote;
			string torEnd = quote;

			int pos = output.IndexOf(startBlock, 0);
			pos = output.IndexOf(nameStart, pos + 1);

			while (pos > 0)
			{

				if (pos <= 0) break;

				int posNameStart = output.IndexOf(nameStart, pos);
				posNameStart = output.IndexOf(nameStart2, posNameStart + nameStart.Length + 1);
				int posNameEnd = output.IndexOf(nameEnd, posNameStart + 1);
				string torName = output.Substring(posNameStart + 1, posNameEnd - posNameStart - 1);

				string epno = "";
				int posEpnoEnd = output.IndexOf(epnoEnd, posNameEnd + nameEnd.Length);
				if (posEpnoEnd > 0)
				{
					epno = output.Substring(posNameEnd + nameEnd.Length, posEpnoEnd - posNameEnd - nameEnd.Length);
					epno = epno.Replace("-", "");
					epno = epno.Replace("\t", "");
					epno = epno.Trim();
				}

				int posGroupStart = output.IndexOf(groupStart, posNameEnd);
				posGroupStart = output.IndexOf(groupStart2, posGroupStart + groupStart.Length + 1);
				int posGroupEnd = output.IndexOf(groupEnd, posGroupStart + 1);
				string torGroup = output.Substring(posGroupStart + 1, posGroupEnd - posGroupStart - 1);

				int posTorStart = output.IndexOf(torStart, pos);
				int posTorEnd = output.IndexOf(torEnd, posTorStart + torStart.Length + 1);

				string torLink = output.Substring(posTorStart + torStart.Length, posTorEnd - posTorStart - torStart.Length);
				torLink = HttpUtility.HtmlDecode(torLink);
				torLink = DownloadHelper.FixNyaaTorrentLink(torLink);


				TorrentLink torrentLink = new TorrentLink();
				torrentLink.Source = this.GetSourceName();
				torrentLink.SourceLong = this.GetSourceLongName();
				torrentLink.TorrentDownloadLink = torLink;
				torrentLink.TorrentName = string.Format("{0} - {1} ({2})",  torName, epno, torGroup);
				torrentLink.Size = "UNKNOWN";
				torLinks.Add(torrentLink);

				pos = output.IndexOf(nameStart, pos + 1);

			}

			return torLinks;
		}

		private List<TorrentLink> ParseSourceSearch(string output)
		{
			List<TorrentLink> torLinks = new List<TorrentLink>();

			char q = (char)34;
			string quote = q.ToString();

			string startBlock = "rlsheader";

			string nameStart = "class=" + quote + "thereleasetitle";
			string nameStart2 = ">";
			string nameEnd = "</a>";

			string epnoStart = "<td>";
			string epnoEnd = "</td>";

			string groupStart = "<a href=" + quote + "/groups/info/name";
			string groupStart2 = ">";
			string groupEnd = "</a>";

			string torStart = "<a class='btlink' href=" + quote;
			string torEnd = quote;

			int pos = output.IndexOf(startBlock, 0);
			pos = output.IndexOf(nameStart, pos + 1);

			while (pos > 0)
			{

				if (pos <= 0) break;

				int posNameStart = output.IndexOf(nameStart, pos);
				posNameStart = output.IndexOf(nameStart2, posNameStart + nameStart.Length + 1);
				int posNameEnd = output.IndexOf(nameEnd, posNameStart + 1);
				string torName = output.Substring(posNameStart + 1, posNameEnd - posNameStart - 1);

				string epno = "";

				int posEpnoStart = output.IndexOf(epnoStart, posNameEnd);
				int posEpnoEnd = output.IndexOf(epnoEnd, posEpnoStart + epnoStart.Length + 1);
				if (posEpnoEnd > 0)
				{
					epno = output.Substring(posEpnoStart + epnoStart.Length, posEpnoEnd - posEpnoStart - epnoStart.Length);
					epno = epno.Replace("-", "");
					epno = epno.Replace("\t", "");
					epno = epno.Trim();
				}

				int posGroupStart = output.IndexOf(groupStart, posNameEnd);
				posGroupStart = output.IndexOf(groupStart2, posGroupStart + groupStart.Length + 1);
				int posGroupEnd = output.IndexOf(groupEnd, posGroupStart + 1);
				string torGroup = output.Substring(posGroupStart + 1, posGroupEnd - posGroupStart - 1);

				int posTorStart = output.IndexOf(torStart, pos);
				int posTorEnd = output.IndexOf(torEnd, posTorStart + torStart.Length + 1);

				string torLink = output.Substring(posTorStart + torStart.Length, posTorEnd - posTorStart - torStart.Length);
				torLink = HttpUtility.HtmlDecode(torLink);
				torLink = DownloadHelper.FixNyaaTorrentLink(torLink);


				TorrentLink torrentLink = new TorrentLink();
				torrentLink.Source = this.GetSourceName();
				torrentLink.TorrentDownloadLink = torLink;
				torrentLink.TorrentName = string.Format("{0} - {1} ({2})", torName, epno, torGroup);
				torrentLink.Size = "UNKNOWN";
				torLinks.Add(torrentLink);

				pos = output.IndexOf(nameStart, pos + 1);

			}

			return torLinks;
		}

		public List<TorrentLink> GetTorrents(List<string> searchParms)
		{
			string urlBase = "http://www.baka-updates.com/search/search?searchitem={0}&submit.x=0&submit.y=0&submit=submit&searchradio=releases";

			string searchCriteria = "";
			foreach (string parm in searchParms)
			{
				if (searchCriteria.Length > 0) searchCriteria += "+";
				searchCriteria += parm.Trim();
			}

			string url = string.Format(urlBase, searchCriteria);
			string output = Utils.DownloadWebPage(url);

			return ParseSourceSearch(output);
		}

		public List<TorrentLink> BrowseTorrents()
		{
			string url = "http://www.baka-updates.com/releases";
			string output = Utils.DownloadWebPage(url);

			return ParseSourceBrowse(output);
		}

		#endregion
	}
}
