using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace MyAnimePlugin3.Downloads
{
	public class TorrentsTokyoToshokan : ITorrentSource
	{
		#region ITorrentSource Members

		public string GetSourceName()
		{
			return "TT";
		}

		public string GetSourceLongName()
		{
			return "Tokyo Toshokan";
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
			return true;
		}


		private List<TorrentLink> ParseSource(string output)
		{
			List<TorrentLink> torLinks = new List<TorrentLink>();

			char q = (char)34;
			string quote = q.ToString();

			string startBlock = "<a rel=" + quote + "nofollow" + quote + " type=" + quote + "application/x-bittorrent" + quote;

			string torStart = "href=" + quote;
			string torEnd = quote;

			string nameStart = ">";
			string nameEnd = "</a>";

			string sizeStart = "Size:";
			string sizeEnd = "|";

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

				string torSize = "";
				int posSizeStart = output.IndexOf(sizeStart, posNameEnd);
				if (posSizeStart > 0)
				{
					int posSizeEnd = output.IndexOf(sizeEnd, posSizeStart + sizeStart.Length + 1);

					torSize = output.Substring(posSizeStart + sizeStart.Length, posSizeEnd - posSizeStart - sizeStart.Length);
				}

				TorrentLink torrentLink = new TorrentLink();
				torrentLink.Source = this.GetSourceName();
				torrentLink.SourceLong = this.GetSourceLongName();
				torrentLink.TorrentDownloadLink = torLink;
				torrentLink.TorrentName = torName;
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
			//string urlBase = "http://www.tokyotosho.info/search.php?terms={0}&type=1";
			string urlBase = "http://www.tokyotosho.info/search.php?terms={0}";
			
			string searchCriteria = "";
			foreach (string parm in searchParms)
			{
				if (searchCriteria.Length > 0) searchCriteria += "+";
				searchCriteria += parm.Trim();
			}

			string url = string.Format(urlBase, searchCriteria);
			string output = Utils.DownloadWebPage(url);

			BaseConfig.MyAnimeLog.Write("GetTorrents Search: " + url);
			//BaseConfig.MyAnimeLog.Write("GetTorrents Results: " + output);

			return ParseSource(output);
		}

		public List<TorrentLink> BrowseTorrents()
		{
			string url = "http://www.tokyotosho.info/?cat=1";
			string output = Utils.DownloadWebPage(url);

			return ParseSource(output);
		}

		#endregion
	}
}
