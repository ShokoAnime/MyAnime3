using System;
using System.Collections.Generic;

using System.Net;
using System.IO;
using System.Xml;

using System.Xml.Serialization;

using System.Web;

namespace MyAnimePlugin3.DataHelpers
{
	public class XMLService
	{
		//TODO
		/*
		public static void Send_CrossRef_Series_AniDB_TvDB(CrossRef_Series_AniDB_TvDB data)
		{
			AnimePluginSettings settings = new AnimePluginSettings();
			string uri = string.Format("http://{0}/AddCrossRefSeries.aspx", settings.XMLWebCacheIP);
			CrossRefSeries crs = new CrossRefSeries(data);
			string xml = crs.ToXML();

			SendData(uri, xml);
		}

		public static void Delete_CrossRef_Series_AniDB_TvDB(int tvDBID)
		{
			AnimePluginSettings settings = new AnimePluginSettings();
			string uri = string.Format("http://{0}/DeleteCrossRefSeries.aspx?TvDB_ID={1}", settings.XMLWebCacheIP, tvDBID);

			//BaseConfig.MyAnimeLog.Write("Deleting cross ref: " + uri);

			SendData(uri, "");
		}

        public static void Send_CrossRef_Episode_AniDB_TvDB(CrossRef_Episode_AniDB_TvDB data)
        {
            AnimePluginSettings settings = new AnimePluginSettings();
            string uri = string.Format("http://{0}/AddCrossRefEpisode.aspx", settings.XMLWebCacheIP);
            CrossRefEpisode cre = new CrossRefEpisode(data);
            string xml = cre.ToXML();

            SendData(uri, xml);
        }


		public static CrossRef_Series_AniDB_TvDB Get_CrossRefSeries(int aniDBID)
		{
			try
			{
				AnimePluginSettings settings = new AnimePluginSettings();
				string uri = string.Format("http://{0}/GetCrossRefSeries.aspx?id={1}", settings.XMLWebCacheIP, aniDBID);
				string xml = GetData(uri);

				if (xml.Trim().Length == 0) return null;

				CrossRef_Series_AniDB_TvDB crossRef = new CrossRef_Series_AniDB_TvDB();

				XmlDocument docCrossRef = new XmlDocument();
				docCrossRef.LoadXml(xml);

				// populate the fields
				crossRef.AniDB_ID = int.Parse(TryGetProperty(docCrossRef, "CrossRef_Series_AniDB_TvDB", "AniDB_ID"));
				crossRef.TvDB_ID = int.Parse(TryGetProperty(docCrossRef, "CrossRef_Series_AniDB_TvDB", "TvDB_ID"));

				return crossRef;
			}
			catch (Exception ex)
			{
                BaseConfig.MyAnimeLog.Write("Error in XMLService.Get_CrossRefSeries:: {0}", ex);
				return null;
			}
		}


		public static PluginVersion Get_PluginVersion()
		{
			try
			{
				AnimePluginSettings settings = new AnimePluginSettings();
				string uri = string.Format("http://{0}/GetPluginVersion.aspx",
					settings.XMLWebCacheIP);
				string xml = GetData(uri);

				if (xml.Trim().Length == 0) return null;

				PluginVersion pVersion = new PluginVersion();

				XmlDocument docVersion = new XmlDocument();
				docVersion.LoadXml(xml);
				
				// populate the fields
				pVersion.VersionNumber = TryGetProperty(docVersion, "PluginVersion", "VersionNumber");
				pVersion.DownloadLink = TryGetProperty(docVersion, "PluginVersion", "DownloadLink");

				return pVersion;
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write("Error in XMLService.Get_CrossRefSeries:: {0}", ex);
				return null;
			}
		}


		public static List<string> Get_FileCRCsForEpisode(int episodeID)
		{
			try
			{
				List<string> crcList = new List<string>();

				AnimePluginSettings settings = new AnimePluginSettings();
				string uri = string.Format("http://{0}/GetFileCRCsForEpisode.aspx?id={1}", settings.XMLWebCacheIP, episodeID);
				string xml = GetData(uri);

				if (xml.Trim().Length == 0) return crcList;

				XmlDocument docCRC = new XmlDocument();
				docCRC.LoadXml(xml);

				// populate the fields

				string allCRCs = TryGetProperty(docCRC, "CRCCollection", "CRCs");

				if (allCRCs.Trim().Length > 0)
				{
					string[] crcs = allCRCs.Split('|');
					foreach (string s in crcs)
					{
						crcList.Add(s.Trim());
					}
				}

				return crcList;
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write("Error in XMLService.Get_FileCRCsForEpisode: {0}", ex);
				return null;
			}
		}

		private static string TryGetProperty(XmlDocument doc, string keyName, string propertyName)
		{
			try
			{
				string prop = doc[keyName][propertyName].InnerText.Trim();
				return prop;
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write("---------------------------------------------------------------");
                BaseConfig.MyAnimeLog.Write("Error in XMLService.TryGetProperty: {0}-{1}", Utils.GetParentMethodName(), ex.ToString());
				BaseConfig.MyAnimeLog.Write("keyName: {0}, propertyName: {1}", keyName, propertyName);
				BaseConfig.MyAnimeLog.Write("---------------------------------------------------------------");
            }

			return "";
		}

		private static string GetData(string uri)
		{
			AnimePluginSettings settings = new AnimePluginSettings();
			if (!settings.UseWebCache) return "";

			try
			{
				DateTime start = DateTime.Now;

				//BaseConfig.MyAnimeLog.Write("GetData called by: {0}", Utils.GetParentMethodName());
				string xml = Utils.DownloadWebPage(uri);
				BaseConfig.MyAnimeLog.Write("GetData for: {0}", uri.ToString());
				BaseConfig.MyAnimeLog.Write("GetData returned in {0}: {1}", Utils.GetParentMethodName(), xml);
                if (xml.Contains(Constants.WebCacheError)) return "";

				TimeSpan ts = DateTime.Now - start;
				//BaseConfig.MyAnimeLog.Write("Got Community Data in {0} ms: {1} --- {2}", ts.TotalMilliseconds, uri.ToString(), xml);

				return xml;
			}
			catch (WebException webEx)
			{
                BaseConfig.MyAnimeLog.Write("Error(1) in XMLService.GetData: {0}", webEx);
            }
			catch (Exception ex)
			{
                BaseConfig.MyAnimeLog.Write("Error(2) in XMLService.GetData: {0}", ex);
            }

			return "";
		}

		private static void SendData(string uri, string xml)
		{
			AnimePluginSettings settings = new AnimePluginSettings();
			if (!settings.UseWebCache) return;

			if (!Utils.IsRunningFromConfig()) MainWindow.xmlQueue.SendXML(new XMLSendRequest(uri, xml));
		}*/
	}

	[Serializable]
	[XmlRoot("CrossRef_Series_AniDB_TvDB")]
	public class CrossRefSeries : XMLBase
	{
		protected int aniDB_ID;
		public int AniDB_ID
		{
			get { return aniDB_ID; }
			set { aniDB_ID = value; }
		}

		protected int tvDB_ID;
		public int TvDB_ID
		{
			get { return tvDB_ID; }
			set { tvDB_ID = value; }
		}

		protected string username = "";
		public string Username
		{
			get { return username; }
			set { username = value; }
		}

		// default constructor
		public CrossRefSeries()
		{
		}

		//TODO
		/*
		// default constructor
		public CrossRefSeries(CrossRef_Series_AniDB_TvDB data)
		{
			AnimePluginSettings settings = new AnimePluginSettings();
			this.AniDB_ID = data.AniDB_ID;
			this.TvDB_ID = data.TvDB_ID;
			this.Username = settings.Username;
		}*/
	}

	[Serializable]
	[XmlRoot("PluginVersion")]
	public class PluginVersion : XMLBase
	{
		protected string versionNumber;
		public string VersionNumber
		{
			get { return versionNumber; }
			set { versionNumber = value; }
		}

		protected string downloadLink;
		public string DownloadLink
		{
			get { return downloadLink; }
			set { downloadLink = value; }
		}



		// default constructor
		public PluginVersion()
		{
		}
	}
}
