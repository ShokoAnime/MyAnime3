using System;
using System.Collections.Generic;
using System.Text;

using System.Xml;
using MediaPortal.GUI.Library;

namespace MyAnimePlugin3.Providers.TheTvDB
{
	public class TvDBSeries 
	{
        public string Id { get; set; }

        public string SeriesID { get; set; }

        public string Genre { get; set; }

        public string Network { get; set; }

        public string Overview { get; set; }

        public string Rating { get; set; }

        public string SeriesName { get; set; }

        public string Status { get; set; }

        public string Banner { get; set; }

        public string Fanart { get; set; }

        public string Lastupdated { get; set; }

        public string Poster { get; set; }

        public override string ToString()
        {
            return "TvDB_Series: " + Id + ":" + SeriesID + ":" + SeriesName;
        }
		public TvDBSeries()
		{
            Id = string.Empty;
            SeriesID = string.Empty;
            Genre = string.Empty;
            Network = string.Empty;
            Overview = string.Empty;
            Rating = string.Empty;
            SeriesName = string.Empty;
            Status = string.Empty;
            Banner = string.Empty;
            Fanart = string.Empty;
            Lastupdated = string.Empty;
            Poster = string.Empty;
		}

		public TvDBSeries(XmlDocument doc)
		{
			this.Id = TryGetProperty(doc, "id");
			this.SeriesName = TryGetProperty(doc, "SeriesName");
		}

		protected string TryGetProperty(XmlDocument doc, string propertyName)
		{
			try
			{
				string prop = doc["Data"]["Series"][propertyName].InnerText.Trim();
				return prop;
			}
			catch (Exception ex)
			{
				BaseConfig.MyAnimeLog.Write("Erorr in TryGetProperty: {0}", ex);
			}

			return "";
		}
	}
}
