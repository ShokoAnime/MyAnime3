using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace MyAnimePlugin3.Providers.TheMovieDB
{
	public class MovieDBSearchResult
	{
		private int id = 0;
		public int Id
		{
			get { return id; }
			set { id = value; }
		}

		private string title = "";
		public string Title
		{
			get { return title; }
			set { title = value; }
		}

		private string release = "";
		public string Release
		{
			get { return release; }
			set { release = value; }
		}

		public override string ToString()
		{
			return "MovieDBSearchResult: " + id + ": " + title + ": " + release;

		}

		public MovieDBSearchResult()
		{
		}

		public bool Init(XmlNode result)
		{
			if (result["id"] == null) return false;

			if (result["id"] != null) id = int.Parse(result["id"].InnerText);
			if (result["name"] != null) title = result["name"].InnerText;
			if (result["released"] != null) release = result["released"].InnerText;

			return true;
		}
	}
}
