using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Web;
using System.Collections;

namespace MyAnimePlugin3.Downloads
{
	public class TorrentsBakaBT : ITorrentSource
	{
		#region ITorrentSource Members

		public string GetSourceName()
		{
			return "BakaBT";
		}

		public string GetSourceLongName()
		{
			return "BakaBT";
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


		public string Login(string username, string password)
		{
			if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)) return "";

			try
			{
				CookieContainer container = new CookieContainer();
				string formUrl = "http://bakabt.me/login.php"; // NOTE: This is the URL the form POSTs to, not the URL of the form (you can find this in the "action" attribute of the HTML's form tag
				string formParams = string.Format("username={0}&password={1}", username, password);

				HttpWebRequest req = (HttpWebRequest)WebRequest.Create(formUrl);
				req.ContentType = "application/x-www-form-urlencoded";
				req.Method = "POST";
				//req.AllowAutoRedirect = false;
				req.CookieContainer = container;
				req.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";
				req.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
				req.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
				byte[] bytes = Encoding.ASCII.GetBytes(formParams);
				req.ContentLength = bytes.Length;
				using (Stream os = req.GetRequestStream())
				{
					os.Write(bytes, 0, bytes.Length);
				}


				HttpWebResponse WebResponse = (HttpWebResponse)req.GetResponse();

				Stream responseStream = WebResponse.GetResponseStream();
				String enco = WebResponse.CharacterSet;
				Encoding encoding = null;
				if (!String.IsNullOrEmpty(enco))
					encoding = Encoding.GetEncoding(WebResponse.CharacterSet);
				if (encoding == null)
					encoding = Encoding.Default;
				StreamReader Reader = new StreamReader(responseStream, encoding);

				string output = Reader.ReadToEnd();

				if (container.Count < 3)
					return "";

				//Grab the cookie we just got back for this specifc page
				return container.GetCookieHeader(new Uri("http://www.bakabt.me/index.php"));
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write("Login: " + ex.ToString());
				return "";
			}
		}

		private List<TorrentLink> ParseSource(string output)
		{
			List<TorrentLink> torLinks = new List<TorrentLink>();

			char q = (char)34;
			string quote = q.ToString();

			//<td class="name">

			// remove html codes
			string rubbish1 = "<span class=" + quote + "highlight" + quote + ">";
			string rubbish2 = "</span>";



			//string startBlock = "<td class=" + quote + "name" + quote;
			string startBlock = "<td class=" + quote + "category";
			string altBlock = "class=" + quote + "alt_title" + quote;

			string catStart = "title=" + quote;
			string catEnd = quote;

			string linkStart = "href=" + quote;
			string linkEnd = quote;

			string nameStart = "title=" + quote + "Download torrent:";
			string nameStart2 = quote + ">";
			string nameEnd = "</a>";

			string sizeStart = "<td class=" + quote + "size" + quote + ">";
			string sizeEnd = "</td>";

			string seedInit = "<td class=" + quote + "peers" + quote + ">";
			string seedStart = quote + ">";
			string seedEnd = "</a>";

			string leechStart = quote + ">";
			string leechEnd = "</a>";

			int pos = output.IndexOf(startBlock, 0);
			while (pos > 0)
			{

				if (pos <= 0) break;

				int poscatStart = output.IndexOf(catStart, pos + 1);
				int poscatEnd = output.IndexOf(catEnd, poscatStart + catStart.Length + 1);

				string cat = output.Substring(poscatStart + catStart.Length, poscatEnd - poscatStart - catStart.Length);

				int poslinkStart = output.IndexOf(linkStart, poscatEnd + 1);
				int poslinkEnd = output.IndexOf(linkEnd, poslinkStart + linkStart.Length + 1);

				string link = output.Substring(poslinkStart + linkStart.Length, poslinkEnd - poslinkStart - linkStart.Length);

				int posnameStart = output.IndexOf(nameStart, poslinkEnd);
				int posnameStart2 = output.IndexOf(nameStart2, posnameStart + nameStart.Length);
				int posnameEnd = output.IndexOf(nameEnd, posnameStart2 + nameStart2.Length + 1);

				string torName = output.Substring(posnameStart2 + nameStart2.Length, posnameEnd - posnameStart2 - nameStart2.Length);

				torName = torName.Replace(rubbish1, "");
				torName = torName.Replace(rubbish2, "");

				// remove html codes
				torName = HttpUtility.HtmlDecode(torName);

				//Console.WriteLine("{0} - {1}", posNameStart, posNameEnd);

				string torSize = "";
				int posSizeStart = output.IndexOf(sizeStart, posnameEnd);
				int posSizeEnd = 0;
				if (posSizeStart > 0)
				{
					posSizeEnd = output.IndexOf(sizeEnd, posSizeStart + sizeStart.Length + 1);

					torSize = output.Substring(posSizeStart + sizeStart.Length, posSizeEnd - posSizeStart - sizeStart.Length);
				}

				int posSeedInit = output.IndexOf(seedInit, posSizeEnd);

				string torSeed = "";
				int posSeedStart = output.IndexOf(seedStart, posSeedInit + seedInit.Length + 1);
				int posSeedEnd = 0;
				if (posSeedStart > 0)
				{
					posSeedEnd = output.IndexOf(seedEnd, posSeedStart + seedStart.Length + 1);

					torSeed = output.Substring(posSeedStart + seedStart.Length, posSeedEnd - posSeedStart - seedStart.Length);
				}

				string torLeech = "";
				int posLeechStart = output.IndexOf(leechStart, posSeedStart + 3);
				int posLeechEnd = 0;
				if (posLeechStart > 0)
				{
					posLeechEnd = output.IndexOf(leechEnd, posLeechStart + leechStart.Length + 1);

					torLeech = output.Substring(posLeechStart + leechStart.Length, posLeechEnd - posLeechStart - leechStart.Length);
				}

				TorrentLink torrentLink = new TorrentLink();
				torrentLink.Source = this.GetSourceName();
				torrentLink.SourceLong = this.GetSourceLongName();
				torrentLink.TorrentDownloadLink = "";
				torrentLink.TorrentLinkURL = string.Format("http://bakabt.me{0} ", link);
				torrentLink.TorrentName = string.Format("[MAIN] {0} [{1}]", torName.Trim(), cat);
				torrentLink.Size = torSize.Trim();
				torrentLink.Seeders = torSeed.Trim();
				torrentLink.Leechers = torLeech.Trim();
				torLinks.Add(torrentLink);

				// now we have the main link provided by BakaBT
				// BakaBT also provides alternative links, so lets include those as well

				int temppos = output.IndexOf(startBlock, pos + 1);
				int altpos = output.IndexOf(altBlock, pos + 1);

				while (temppos > altpos && altpos > 0)
				{
					string linkStartAlt = "href=" + quote;
					string linkEndAlt = quote;

					string nameStartAlt = quote + ">";
					string nameEndAlt = "</a>";

					string sizeStartAlt = "<td class=" + quote + "size" + quote + ">";
					string sizeEndAlt = "</td>";

					string seedInitAlt = "<td class=" + quote + "peers" + quote + ">";
					string seedStartAlt = quote + ">";
					string seedEndAlt = "</a>";

					string leechStartAlt = quote + ">";
					string leechEndAlt = "</a>";

					int poslinkStartAlt = output.IndexOf(linkStartAlt, altpos + 1);
					int poslinkEndAlt = output.IndexOf(linkEndAlt, poslinkStartAlt + linkStartAlt.Length + 1);

					string linkAlt = output.Substring(poslinkStartAlt + linkStartAlt.Length, poslinkEndAlt - poslinkStartAlt - linkStartAlt.Length);

					int posnameStartAlt = output.IndexOf(nameStartAlt, poslinkEndAlt);
					int posnameEndAlt = output.IndexOf(nameEndAlt, posnameStartAlt + nameStartAlt.Length + 1);

					string torNameAlt = output.Substring(posnameStartAlt + nameStartAlt.Length, posnameEndAlt - posnameStartAlt - nameStartAlt.Length);

					// remove html codes
					torNameAlt = torNameAlt.Replace(rubbish1, "");
					torNameAlt = torNameAlt.Replace(rubbish2, "");

					torNameAlt = HttpUtility.HtmlDecode(torNameAlt);

					string torSizeAlt = "";
					int posSizeStartAlt = output.IndexOf(sizeStartAlt, posnameEndAlt);
					int posSizeEndAlt = 0;
					if (posSizeStartAlt > 0)
					{
						posSizeEndAlt = output.IndexOf(sizeEndAlt, posSizeStartAlt + sizeStartAlt.Length + 1);

						torSizeAlt = output.Substring(posSizeStartAlt + sizeStartAlt.Length, posSizeEndAlt - posSizeStartAlt - sizeStartAlt.Length);
					}

					int posSeedInitAlt = output.IndexOf(seedInitAlt, posSizeEndAlt);

					string torSeedAlt = "";
					int posSeedStartAlt = output.IndexOf(seedStartAlt, posSeedInitAlt + seedInitAlt.Length + 1);
					int posSeedEndAlt = 0;
					if (posSeedStartAlt > 0)
					{
						posSeedEndAlt = output.IndexOf(seedEndAlt, posSeedStartAlt + seedStartAlt.Length + 1);

						torSeedAlt = output.Substring(posSeedStartAlt + seedStartAlt.Length, posSeedEndAlt - posSeedStartAlt - seedStartAlt.Length);
					}

					string torLeechAlt = "";
					int posLeechStartAlt = output.IndexOf(leechStartAlt, posSeedStartAlt + 3);
					int posLeechEndAlt = 0;
					if (posLeechStartAlt > 0)
					{
						posLeechEndAlt = output.IndexOf(leechEndAlt, posLeechStartAlt + leechStartAlt.Length + 1);

						torLeechAlt = output.Substring(posLeechStartAlt + leechStartAlt.Length, posLeechEndAlt - posLeechStartAlt - leechStartAlt.Length);
					}

					TorrentLink torrentLinkAlt = new TorrentLink();
					torrentLinkAlt.Source = this.GetSourceName();
					torrentLinkAlt.SourceLong = this.GetSourceLongName();
					torrentLinkAlt.TorrentDownloadLink = "";
					torrentLinkAlt.TorrentLinkURL = string.Format("http://bakabt.me{0} ", linkAlt);
					torrentLinkAlt.TorrentName = string.Format("[ALT] {0} [{1}]", torNameAlt.Trim(), cat);
					torrentLinkAlt.Size = torSizeAlt.Trim();
					torrentLinkAlt.Seeders = torSeedAlt.Trim();
					torrentLinkAlt.Leechers = torLeechAlt.Trim();
					torLinks.Add(torrentLinkAlt);

					altpos = output.IndexOf(altBlock, posLeechEndAlt + 1);
				}

				pos = output.IndexOf(startBlock, pos + 1);



				//Console.WriteLine("{0} - {1}", torName, torLink);
			}
			//Console.ReadLine();

			return torLinks;
		}

		public List<TorrentLink> GetTorrents(List<string> searchParms)
		{
			try
			{
				if (string.IsNullOrEmpty(BaseConfig.Settings.BakaBTUsername) || string.IsNullOrEmpty(BaseConfig.Settings.BakaBTPassword))
					return new List<TorrentLink>();

				if (string.IsNullOrEmpty(BaseConfig.Settings.BakaBTCookieHeader))
				{
					string cookie = Login(BaseConfig.Settings.BakaBTUsername, BaseConfig.Settings.BakaBTPassword);
					BaseConfig.Settings.BakaBTCookieHeader = cookie;
				}

				if (string.IsNullOrEmpty(BaseConfig.Settings.BakaBTCookieHeader))
					return new List<TorrentLink>();

				string urlBase = "http://bakabt.me/browse.php?only=0&hentai=1&incomplete=1&lossless=1&hd=1&multiaudio=1&bonus=1&c1=1&c2=1&c5=1&reorder=1&q={0}";

				string searchCriteria = "";
				foreach (string parm in searchParms)
				{
					if (searchCriteria.Length > 0) searchCriteria += "+";
					searchCriteria += parm.Trim();
				}

				string url = string.Format(urlBase, searchCriteria);
				string output = Utils.DownloadWebPage(url, BaseConfig.Settings.BakaBTCookieHeader, true);

				return ParseSource(output);
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write("GetTorrents: " + ex.ToString());
				return new List<TorrentLink>();
			}
		}

		public List<TorrentLink> BrowseTorrents()
		{
			try
			{
				if (string.IsNullOrEmpty(BaseConfig.Settings.BakaBTUsername) || string.IsNullOrEmpty(BaseConfig.Settings.BakaBTPassword))
					return new List<TorrentLink>();

				if (string.IsNullOrEmpty(BaseConfig.Settings.BakaBTCookieHeader))
				{
					string cookie = Login(BaseConfig.Settings.BakaBTUsername, BaseConfig.Settings.BakaBTPassword);
					BaseConfig.Settings.BakaBTCookieHeader = cookie;
				}

				if (string.IsNullOrEmpty(BaseConfig.Settings.BakaBTCookieHeader))
					return new List<TorrentLink>();

				string url = "http://bakabt.me/browse.php?only=0&hentai=1&incomplete=1&lossless=1&hd=1&multiaudio=1&bonus=1&c1=1&c2=1&c5=1&reorder=1&q=";
				string output = Utils.DownloadWebPage(url, BaseConfig.Settings.BakaBTCookieHeader, true);

				return ParseSource(output);
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write("BrowseTorrents: " + ex.ToString());
				return new List<TorrentLink>();
			}
		}

		public string GetTorrentLinkFromTorrentPage(string pageSource)
		{
			string startBlock = "<div class=\"download_link";

			string linkStart = "href=\"";
			string linkEnd = "\"";

			int pos = pageSource.IndexOf(startBlock, 0);

			if (pos <= 0) return null;

			int poslinkStart = pageSource.IndexOf(linkStart, pos + 1);
			int poslinkEnd = pageSource.IndexOf(linkEnd, poslinkStart + linkStart.Length + 1);

			string link = pageSource.Substring(poslinkStart + linkStart.Length, poslinkEnd - poslinkStart - linkStart.Length);

			return link;
		}

		public string PopulateTorrentLink(string torrentLinkURL)
		{
			try
			{
				if (string.IsNullOrEmpty(BaseConfig.Settings.BakaBTUsername) || string.IsNullOrEmpty(BaseConfig.Settings.BakaBTPassword))
					return "";

				if (string.IsNullOrEmpty(BaseConfig.Settings.BakaBTCookieHeader))
				{
					string cookie = Login(BaseConfig.Settings.BakaBTUsername, BaseConfig.Settings.BakaBTPassword);
					BaseConfig.Settings.BakaBTCookieHeader = cookie;
				}

				if (string.IsNullOrEmpty(BaseConfig.Settings.BakaBTCookieHeader))
					return "";

				string url = torrentLinkURL;
				string output = Utils.DownloadWebPage(url, BaseConfig.Settings.BakaBTCookieHeader, true);

				string torDownloadLink = GetTorrentLinkFromTorrentPage(output);
				return string.Format("http://bakabt.me{0}", torDownloadLink);
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write("PopulateTorrentLink: " + ex.ToString());
				return "";
			}
		}

		private List<Cookie> GetAllCookies(CookieContainer cc)
		{
			List<Cookie> lstCookies = new List<Cookie>();

			Hashtable table = (Hashtable)cc.GetType().InvokeMember("m_domainTable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.Instance, null, cc, new object[] { });

			foreach (var pathList in table.Values)
			{
				SortedList lstCookieCol = (SortedList)pathList.GetType().InvokeMember("m_list", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.Instance, null, pathList, new object[] { });
				foreach (CookieCollection colCookies in lstCookieCol.Values)
					foreach (Cookie c in colCookies) lstCookies.Add(c);
			}

			return lstCookies;
		}

		private string ShowAllCookies(CookieContainer cc)
		{
			StringBuilder sb = new StringBuilder();
			List<Cookie> lstCookies = GetAllCookies(cc);
			sb.AppendLine("=========================================================== ");
			sb.AppendLine(lstCookies.Count + " cookies found.");
			sb.AppendLine("=========================================================== ");
			int cpt = 1;
			foreach (Cookie c in lstCookies)
				sb.AppendLine("#" + cpt++ + "> Name: " + c.Name + "\tValue: " + c.Value + "\tDomain: " + c.Domain + "\tPath: " + c.Path + "\tExp: " + c.Expires.ToString());

			return sb.ToString();
		}

		#endregion
	}
}
