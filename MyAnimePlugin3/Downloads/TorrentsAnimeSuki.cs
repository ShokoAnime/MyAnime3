using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace MyAnimePlugin3.Downloads
{
	public class TorrentsAnimeSuki : ITorrentSource
	{
		public string GetSourceName()
		{
			return "Suki";
		}

		public string GetSourceLongName()
		{
			return "Animesuki";
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


		private List<TorrentLink> ParseSource(string output)
		{
			List<TorrentLink> torLinks = new List<TorrentLink>();

			char q = (char)34;
			string quote = q.ToString();

			string startBlock = "<td class=" + quote + "bb_m";

			string torStart = "<a target=" + quote + "_top" + quote + " href=" + quote;
			string torEnd = quote;

			string nameStart = "rel=" + quote + "nofollow" + quote + ">";
			string nameEnd = "</a>";

			string groupStart = "<td nowrap=" + quote + "nowrap" + quote + ">";
			string groupEnd = "</a>";

			string sizeStart = "<td nowrap=" + quote + "nowrap" + quote + ">";
			string sizeEnd = "</td>";

			int pos = output.IndexOf(startBlock, 0);
			while (pos > 0)
			{

				if (pos <= 0) break;

				int posTorStart = output.IndexOf(torStart, pos + 1);
				int posTorEnd = output.IndexOf(torEnd, posTorStart + torStart.Length + 1);

				//Console.WriteLine("{0} - {1}", posTorStart, posTorEnd);

				string torLink = output.Substring(posTorStart + torStart.Length, posTorEnd - posTorStart - torStart.Length);
				torLink = DownloadHelper.FixNyaaTorrentLink(torLink);

				// remove html codes
				//torLink = torLink.Replace("amp;", "");
				torLink = HttpUtility.HtmlDecode(torLink);

				//BaseConfig.MyAnimeLog.Write("HttpUtility.HtmlDecode(torLink): {0}", HttpUtility.HtmlDecode(torLink));
				//BaseConfig.MyAnimeLog.Write("HttpUtility.UrlDecode(torLink): {0}", HttpUtility.UrlDecode(torLink));
				//BaseConfig.MyAnimeLog.Write("HttpUtility.UrlEncode(torLink): {0}", HttpUtility.UrlEncode(torLink));

				int posNameStart = output.IndexOf(nameStart, posTorEnd);
				int posNameEnd = output.IndexOf(nameEnd, posNameStart + nameStart.Length + 1);

				//Console.WriteLine("{0} - {1}", posNameStart, posNameEnd);

				string torName = output.Substring(posNameStart + nameStart.Length, posNameEnd - posNameStart - nameStart.Length);
				torName = torName.Replace("<b>", "");
				torName = torName.Replace("</b>", "");

				int posGroupStart = output.IndexOf(groupStart, posNameEnd);
				posGroupStart = output.IndexOf(">", posGroupStart + groupStart.Length + 1);
				int posGroupEnd = output.IndexOf(groupEnd, posGroupStart + 1);
				string torGroup = output.Substring(posGroupStart + 1, posGroupEnd - posGroupStart - 1);

				string torSize = "";
				int posSizeStart = output.IndexOf(sizeStart, posGroupEnd);

				if (posSizeStart > 0)
				{
					int posSizeEnd = output.IndexOf(sizeEnd, posSizeStart + sizeStart.Length + 1);

					torSize = output.Substring(posSizeStart + sizeStart.Length, posSizeEnd - posSizeStart - sizeStart.Length);
					torSize = torSize.Replace("&nbsp;", "");
				}

				TorrentLink torrentLink = new TorrentLink();
				torrentLink.Source = this.GetSourceName();
				torrentLink.SourceLong = this.GetSourceLongName();
				torrentLink.TorrentDownloadLink = torLink;
				torrentLink.TorrentName = torName + " (" + torGroup + ")";
				torrentLink.Size = torSize.Trim();
				torLinks.Add(torrentLink);

				pos = output.IndexOf(startBlock, pos + 1);

				//Console.WriteLine("{0} - {1}", torName, torLink);
			}
			//Console.ReadLine();

			return torLinks;
		}

		public List<TorrentLink> GetTorrents(List<string> searchParms)
		{
			string urlBase = "http://www.animesuki.com/search.php?torrent=yes&query={0}";

			bool containsEpNo = false;
			string searchCriteria = "";
			foreach (string parm in searchParms)
			{
				if (searchCriteria.Length > 0) searchCriteria += "+";
				
				int epno = 0;
				int.TryParse(parm.Trim(), out epno);
				if (epno > 0)
				{
					containsEpNo = true;
					searchCriteria += epno.ToString(); // we are removing zero padding here as well
				}
				else
					searchCriteria += parm.Trim();
			}

			if (containsEpNo) searchCriteria += "&episode=yes";

			string url = string.Format(urlBase, searchCriteria);
			string output = Utils.DownloadWebPage(url);

			return ParseSource(output);
		}

		public List<TorrentLink> BrowseTorrents()
		{
			string url = "http://www.animesuki.com/";
			string output = Utils.DownloadWebPage(url);

			return ParseSource(output);
		}
	}
}
