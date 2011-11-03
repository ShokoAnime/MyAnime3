using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Data;
using System.Web;
using MyAnimePlugin3.DataHelpers;


namespace MyAnimePlugin3.Downloads
{
	public class UTorrentHelper
	{
		private string address = "";
		private string port = "";
		private string username = "";
		private string password = "";
		private string token = "";

		public bool Initialised { get; set;}

		CookieContainer cookieJar = null;

		//private const string urlTorrentList = "http://{0}:{1}/gui/?token={2}&list=1";
		private const string urlTorrentList = "http://{0}:{1}/gui/?token={2}&list=1";
		private const string urlTorrentFileList = "http://{0}:{1}/gui/?token={2}&action=getfiles&hash={3}";

		private const string urlTorrentTokenPage = "http://{0}:{1}/gui/token.html";
		private const string urlTorrentStart = "http://{0}:{1}/gui/?token={2}&action=start&hash={3}";
		private const string urlTorrentStop = "http://{0}:{1}/gui/?token={2}&action=stop&hash={3}";
		private const string urlTorrentPause = "http://{0}:{1}/gui/?token={2}&action=pause&hash={3}";
		private const string urlTorrentAddURL = "http://{0}:{1}/gui/?token={2}&action=add-url&s={3}";
		private const string urlTorrentRemove = "http://{0}:{1}/gui/?token={2}&action=remove&hash={3}";
		private const string urlTorrentRemoveData = "http://{0}:{1}/gui/?token={2}&action=removedata&hash={3}";
		private const string urlTorrentFilePriority = "http://{0}:{1}/gui/?token={2}&action=setprio&hash={3}&p={4}&f={5}";

		private System.Timers.Timer torrentsTimer = null;

		public delegate void ListRefreshedEventHandler(ListRefreshedEventArgs ev);
		public event ListRefreshedEventHandler ListRefreshedEvent;
		protected void OnListRefreshedEvent(ListRefreshedEventArgs ev)
		{
			if (ListRefreshedEvent != null)
			{
				ListRefreshedEvent(ev);
			}
		}

		public UTorrentHelper()
		{
			Initialised = false;
		}

		public void Init()
		{

			address = BaseConfig.Settings.UTorrentAddress;
			port = BaseConfig.Settings.UTorrentPort;
			username = BaseConfig.Settings.UTorrentUsername;
			password = BaseConfig.Settings.UTorrentPassword;

			PopulateToken();

			// timer for automatic updates
			torrentsTimer = new System.Timers.Timer();
			torrentsTimer.AutoReset = false;
			torrentsTimer.Interval = 10 * 1000; // 10 seconds
			torrentsTimer.Elapsed += new System.Timers.ElapsedEventHandler(torrentsTimer_Elapsed);

			if (ValidCredentials())
			{
				// get the intial list of completed torrents
				List<Torrent> torrents = new List<Torrent>();
				bool success = GetTorrentList(ref torrents);

				if (success)
				{
					foreach (Torrent tor in torrents)
					{
						if (!MainWindow.completedTorrents.Contains(tor.Hash) && tor.Remaining == 0)
						{
							MainWindow.completedTorrents.Add(tor.Hash);
						}
					}
				}

				torrentsTimer.Start();
				Initialised = true;
			}
		}

		void torrentsTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{

			try
			{
				torrentsTimer.Stop();

				List<Torrent> torrents = new List<Torrent>();
				//BaseConfig.MyAnimeLog.Write("Getting torrents list...");

				bool success = GetTorrentList(ref torrents);

				if (success)
				{
					OnListRefreshedEvent(new ListRefreshedEventArgs(torrents));
					torrentsTimer.Interval = 10 * 1000;
				}
				else
					torrentsTimer.Interval = 60 * 1000;

				torrentsTimer.Start();
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write(ex.ToString());
				torrentsTimer.Start();
			}

		}

		private bool ValidCredentials()
		{
			if (address.Trim().Length == 0) return false;
			if (port.Trim().Length == 0) return false;
			if (username.Trim().Length == 0) return false;
			if (password.Trim().Length == 0) return false;

			return true;
		}

		private void PopulateToken()
		{
			cookieJar = new CookieContainer();
			token = "";

			if (!ValidCredentials())
			{
				BaseConfig.MyAnimeLog.Write("Credentials are not valid for uTorrent");
				return;
			}

			string url = "";
			try
			{
				
				url = string.Format(urlTorrentTokenPage, address, port);
				BaseConfig.MyAnimeLog.Write("token url: {0}", url);
				HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(url);
				webReq.Timeout = 10000; // 10 seconds
				webReq.Credentials = new NetworkCredential(username, password);
				webReq.CookieContainer = cookieJar;

				HttpWebResponse WebResponse = (HttpWebResponse)webReq.GetResponse();
				

				Stream responseStream = WebResponse.GetResponseStream();
				StreamReader Reader = new StreamReader(responseStream, Encoding.UTF8);

				string output = Reader.ReadToEnd();
				BaseConfig.MyAnimeLog.Write("token reponse: {0}", output);

				WebResponse.Close();
				responseStream.Close();

				// parse and get the token
				// <html><div id='token' style='display:none;'>u3iiuDG4dwYDMzurIFif7FS-ldLPcvHk6QlB4y8LSKK5mX9GSPUZ_PpxD0s=</div></html>

				char q = (char)34;
				string quote = q.ToString();

				string torStart = "display:none;'>";
				string torEnd = "</div>";

				int posTorStart = output.IndexOf(torStart, 0);
				if (posTorStart <= 0) return;

				int posTorEnd = output.IndexOf(torEnd, posTorStart + torStart.Length + 1);

				token = output.Substring(posTorStart + torStart.Length, posTorEnd - posTorStart - torStart.Length);
				//BaseConfig.MyAnimeLog.Write("token: {0}", token);
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write("Error in StartTorrent: {0} - {1}", url, ex.ToString());
				return;
			}
		}

		public void RemoveTorrent(string hash)
		{
			if (!ValidCredentials())
			{
				BaseConfig.MyAnimeLog.Write("Credentials are not valid for uTorrent");
				return;
			}

			try
			{
				string url = string.Format(urlTorrentRemove, address, port, token, hash);
				string output = GetWebResponse(url);

				return;
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write("Error in StartTorrent: {0}", ex.ToString());
				return;
			}
		}

		public void RemoveTorrentAndData(string hash)
		{
			if (!ValidCredentials())
			{
				BaseConfig.MyAnimeLog.Write("Credentials are not valid for uTorrent");
				return;
			}

			try
			{
				string url = string.Format(urlTorrentRemoveData, address, port, token, hash);
				string output = GetWebResponse(url);

				return;
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write("Error in StartTorrent: {0}", ex.ToString());
				return;
			}
		}

		public void AddTorrentFromURL(string downloadURL)
		{
			if (!ValidCredentials())
			{
				BaseConfig.MyAnimeLog.Write("Credentials are not valid for uTorrent");
				return;
			}

			try
			{
				string encodedURL = HttpUtility.UrlEncode(downloadURL);
				string url = string.Format(urlTorrentAddURL, address, port, token, encodedURL);

				BaseConfig.MyAnimeLog.Write("Downloading: {0}", encodedURL);

				string output = GetWebResponse(url);

				return;
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write("Error in AddTorrentFromURL: {0}", ex.ToString());
				return;
			}
		}

		public void StopTorrent(string hash)
		{
			if (!ValidCredentials())
			{
				BaseConfig.MyAnimeLog.Write("Credentials are not valid for uTorrent");
				return;
			}

			try
			{
				string url = string.Format(urlTorrentStop, address, port, token, hash);
				string output = GetWebResponse(url);

				return;
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write("Error in StartTorrent: {0}", ex.ToString());
				return;
			}
		}

		public void StartTorrent(string hash)
		{
			if (!ValidCredentials())
			{
				BaseConfig.MyAnimeLog.Write("Credentials are not valid for uTorrent");
				return;
			}

			try
			{
				string url = string.Format(urlTorrentStart, address, port, token, hash);
				string output = GetWebResponse(url);

				return;
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write("Error in StartTorrent: {0}", ex.ToString());
				return;
			}
		}

		public void PauseTorrent(string hash)
		{
			if (!ValidCredentials())
			{
				BaseConfig.MyAnimeLog.Write("Credentials are not valid for uTorrent");
				return;
			}

			try
			{
				string url = string.Format(urlTorrentPause, address, port, token, hash);
				string output = GetWebResponse(url);

				return;
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write("Error in StartTorrent: {0}", ex.ToString());
				return;
			}
		}

		private string GetWebResponse(string url)
		{
			HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(url);
			webReq.Timeout = 15000; // 15 seconds
			webReq.Credentials = new NetworkCredential(username, password);
			webReq.CookieContainer = cookieJar;

			bool tryAgain = false;
			HttpWebResponse webResponse = null;
			try
			{
				webResponse = (HttpWebResponse)webReq.GetResponse();
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write("UTorrent:: GetWebResponse: {0}", ex.Message);
				if (ex.ToString().Contains("(400) Bad Request"))
				{
					BaseConfig.MyAnimeLog.Write("UTorrent:: GetWebResponse 400 bad request, will try again...");
					tryAgain = true;
				}
			}

			if (tryAgain)
			{
				PopulateToken();

				// fin the token in the url and replace it with the new one
				//http://{0}:{1}/gui/?token={2}&list=1
				int iStart = url.IndexOf(@"?token=", 0);
				int iFinish = url.IndexOf(@"&", 0);

				string prefix = url.Substring(0, iStart);
				string tokenStr = @"?token=" + token;
				string suffix = url.Substring(iFinish, url.Length - iFinish);

				BaseConfig.MyAnimeLog.Write("prefix: {0} --- tokenStr: {1} --- suffix: {2}", prefix, tokenStr, suffix);

				url = prefix + tokenStr + suffix;


				webReq = (HttpWebRequest)WebRequest.Create(url);
				webReq.Timeout = 15000; // 15 seconds
				webReq.Credentials = new NetworkCredential(username, password);
				webReq.CookieContainer = cookieJar;
				webResponse = (HttpWebResponse)webReq.GetResponse();
			}

			if (webResponse == null) return "";

			Stream responseStream = webResponse.GetResponseStream();
			StreamReader Reader = new StreamReader(responseStream, Encoding.UTF8);

			string output = Reader.ReadToEnd();

			webResponse.Close();
			responseStream.Close();

			return output;
		}

		public bool GetTorrentList(ref List<Torrent> torrents)
		{
			torrents = new List<Torrent>();

			if (!ValidCredentials())
			{
				BaseConfig.MyAnimeLog.Write("Credentials are not valid for uTorrent");
				return false;
			}

			string url = "";
			try
			{
				//http://[IP]:[PORT]/gui/?list=1
				url = string.Format(urlTorrentList, address, port, token);
				string output = GetWebResponse(url);
				if (output.Length == 0) return false;


				//BaseConfig.MyAnimeLog.Write("Torrent List JSON: {0}", output);
				TorrentList torList = JSONHelper.Deserialize<TorrentList>(output);

				foreach (object[] obj in torList.torrents)
				{
					Torrent tor = new Torrent(obj);
					torrents.Add(tor);
				}

				return true;
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write("Error in GetTorrentList: {0} - {1}", url, ex.ToString());
				return false;
			}
		}

		public bool GetFileList(string hash, ref List<TorrentFile> torFiles)
		{
			torFiles = new List<TorrentFile>();

			if (!ValidCredentials())
			{
				BaseConfig.MyAnimeLog.Write("Credentials are not valid for uTorrent");
				return false;
			}

			try
			{
				string url = string.Format(urlTorrentFileList, address, port, token, hash);
				string output = GetWebResponse(url);
				if (output.Length == 0) return false;

				TorrentFileList fileList = JSONHelper.Deserialize<TorrentFileList>(output);

				// find the 2nd instance of the "["
				int pos = output.IndexOf("[", 0);
				if (pos > 0) pos = output.IndexOf("[", pos + 1);
				if (pos > 0)
				{
					string output2 = output;
					output2 = output2.Substring(pos, output2.Length - pos);
					//BaseConfig.MyAnimeLog.Write("output2: {0}", output2);
					output2 = output2.Replace("[", "");
					output2 = output2.Replace("]", "");
					output2 = output2.Replace("{", "");
					output2 = output2.Replace("}", "");
					output2 = output2.Replace("\r", "");
					output2 = output2.Replace("\n", "");
					//BaseConfig.MyAnimeLog.Write("output2: {0}", output2);

					DataTable dtDetails = CsvParser.Parse(output2);
					// there will be 4 columns per row

					//BaseConfig.MyAnimeLog.Write("dtDetails.Columns.Count: {0}", dtDetails.Columns.Count.ToString());
					//BaseConfig.MyAnimeLog.Write("dtDetails.Rows.Count: {0}", dtDetails.Rows.Count.ToString());

					int i = 0;
					while (i < dtDetails.Columns.Count)
					{
						TorrentFile tf = new TorrentFile();
						tf.FileName = dtDetails.Rows[0][i].ToString();
						tf.FileSize = long.Parse(dtDetails.Rows[0][i + 1].ToString());
						tf.Downloaded = long.Parse(dtDetails.Rows[0][i + 2].ToString());
						tf.Priority = long.Parse(dtDetails.Rows[0][i + 3].ToString());

						torFiles.Add(tf);

						i += 4;
					}
				}

				return true;
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write("Error in GetTorrentList: {0}", ex.ToString());
				return false;
			}
		}

		public void FileSetPriority(string hash, int idx, TorrentFilePriority priority)
		{
			if (!ValidCredentials())
			{
				BaseConfig.MyAnimeLog.Write("Credentials are not valid for uTorrent");
				return;
			}

			try
			{
				string url = string.Format(urlTorrentFilePriority, address, port, token, hash, (int)priority, idx);
				string output = GetWebResponse(url);

				return;
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write("Error in StartTorrent: {0}", ex.ToString());
				return;
			}
		}
	}

	public class ListRefreshedEventArgs : EventArgs
	{
		public readonly List<Torrent> Torrents = new List<Torrent>();

		public ListRefreshedEventArgs(List<Torrent> tors)
		{
			this.Torrents = tors;
		}
	}

	public enum TorrentFilePriority
	{
		DontDownload = 0,
		Low = 1,
		Medium = 2,
		High = 3
	}

	public enum TorrentOriginator
	{
		Manual = 0,
		Series = 1,
		Episode = 2
	}

	public enum TorrentDownloadStatus
	{
		Ongoing = 0,
		Complete = 1
	}
}
